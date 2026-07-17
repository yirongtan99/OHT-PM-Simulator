import socket
import time
import csv
import threading
from datetime import datetime
from collections import deque
from flask import Flask, render_template, jsonify

app = Flask(__name__)
app.config['TEMPLATES_AUTO_RELOAD'] = True

# Prevent caching of index.html by the browser
@app.after_request
def add_header(response):
    response.headers['Cache-Control'] = 'no-cache, no-store, must-revalidate'
    response.headers['Pragma'] = 'no-cache'
    response.headers['Expires'] = '0'
    return response

# --- Configuration ---
SENSOR_IP = '192.168.250.1' 
SENSOR_PORT = 64000
CSV_FILENAME = 'OHT_Wheel_Predictive_Maintenance_Log.csv'

WHEEL_BASE_DIAMETER_MM = 125.0
WHEEL_DETECT_THRESHOLD_MM = 40.0  # Detect wheel when distance is < 40mm

# --- Global State for Dashboard ---
current_distance = 0.0
# Keep the last 100 distance readings for the live scrolling chart
distance_history = deque([0.0]*100, maxlen=100)
# Auto-incrementing OHT counter for testing
oht_counter = 1

import random
def get_oht_number():
    return f"OHT-{random.randint(1, 10):03d}"

def send_omron_command(sock):
    command_str = "MS,01,0\r\n"
    sock.sendall(command_str.encode('ascii'))
    response = sock.recv(1024).decode('ascii').strip()
    
    parts = response.split(',')
    if len(parts) >= 3 and parts[0] == 'MS':
        hex_value_str = parts[2].strip()
        
        try:
            int_val = int(hex_value_str, 16)
        except ValueError:
            return 999.0
            
        if 0x7FFF0000 <= int_val <= 0x7FFFFFFF:
            return 999.0 # Out of range
            
        if int_val >= 0x80000000:
            int_val -= 0x100000000
            
        mm_value = int_val / 100000.0 
        return mm_value
    return 999.0

def sensor_polling_thread():
    global current_distance, distance_history
    print(f"Connecting to Omron ZP-EIP at {SENSOR_IP}:{SENSOR_PORT}...")
    
    # Initialize the CSV file with headers if it doesn't exist
    try:
        with open(CSV_FILENAME, mode='x', newline='') as file:
            writer = csv.writer(file)
            writer.writerow(['Timestamp', 'OHT_Number', 'FL_mm', 'FR_mm', 'BL_mm', 'BR_mm'])
    except FileExistsError:
        pass
    
    while True:
        try:
            with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as sock:
                sock.settimeout(5.0)
                sock.connect((SENSOR_IP, SENSOR_PORT))
                print("Connected! Live tracking running...")
                
                wheel_in_view = False
                max_dist = -999.0
                
                while True:
                    dist = send_omron_command(sock)
                    current_distance = dist
                    distance_history.append(dist)
                    
                    if -40.0 < current_distance < 40.0:
                        if not wheel_in_view:
                            wheel_in_view = True
                            max_dist = current_distance
                        if current_distance > max_dist:
                            max_dist = current_distance
                            
                    elif wheel_in_view and (current_distance <= -40.0 or current_distance >= 40.0 or current_distance == 999.0):
                        oht_no = get_oht_number()
                        timestamp = datetime.now().strftime('%Y-%m-%d %H:%M:%S')
                        
                        # Sensor zeroed at 125mm wheel top (0.0).
                        # Farther objects (floor, worn wheels) output negative numbers.
                        # max_dist is the highest point (closest to sensor).
                        wheel_diameter = WHEEL_BASE_DIAMETER_MM + max_dist
                        
                        # Prototype simulation: Use the single tested wheel to generate data for all 4 wheels
                        # Adding minor random jitter (+/- 0.02mm) to make the visualization realistic for testing
                        fl = round(wheel_diameter, 3)
                        fr = round(wheel_diameter + random.uniform(-0.015, 0.015), 3)
                        bl = round(wheel_diameter + random.uniform(-0.020, 0.020), 3)
                        br = round(wheel_diameter + random.uniform(-0.025, 0.025), 3)
                        
                        with open(CSV_FILENAME, mode='a', newline='') as file:
                            writer = csv.writer(file)
                            writer.writerow([timestamp, oht_no, fl, fr, bl, br])
                        
                        wheel_in_view = False
                    
                    time.sleep(0.01)
                    
        except Exception as e:
            print(f"Sensor loop exception: {e}. Reconnecting in 2 seconds...")
            time.sleep(2)

@app.route('/')
def index():
    return render_template('index.html')

@app.route('/api/live')
def api_live():
    recent = list(distance_history)
    return jsonify({
        'left': {
            'current_distance': current_distance,
            'history': recent
        },
        'right': {
            'current_distance': current_distance, # mocked for prototype
            'history': recent # mocked for prototype
        }
    })

@app.route('/api/history')
def api_history():
    history_data = []
    try:
        with open(CSV_FILENAME, mode='r') as file:
            reader = csv.DictReader(file)
            for row in reader:
                # We need timestamp, OHT_Number, Max_Wear_Diameter_mm
                history_data.append(row)
    except FileNotFoundError:
        pass
    # Return the last 1000 events to allow trend plotting
    return jsonify(history_data[-1000:])

if __name__ == '__main__':
    # Start the sensor background thread
    t = threading.Thread(target=sensor_polling_thread, daemon=True)
    t.start()
    
    # Run the Flask web server
    app.run(host='0.0.0.0', port=5000, debug=False)
