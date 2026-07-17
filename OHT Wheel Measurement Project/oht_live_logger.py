import socket
import time
import csv
from datetime import datetime

# ==============================================================================
# OHT Wheel Measurement - Live TCP Data Logger
# 
# This script communicates directly with the OMRON ZP-EIP unit using No-Protocol
# TCP. It continuously polls the sensor for live distance measurements and 
# automatically saves "wheel pass" events to a CSV file for predictive maintenance.
# ==============================================================================

# --- Configuration ---
SENSOR_IP = '192.168.250.1' 
SENSOR_PORT = 64000 # No-Protocol TCP Port found via scanner

CSV_FILENAME = 'OHT_Wheel_Predictive_Maintenance_Log.csv'

# Thresholds to detect a wheel
# We trigger the "event" when distance reading falls below -10.0mm
WHEEL_DETECT_THRESHOLD_MM = -10.0  

def get_oht_number():
    """
    TODO: Insert your logic here to determine which OHT is passing by.
    """
    return "OHT-123" # Placeholder

def send_omron_command(sock):
    """
    Sends the 'MS' command to the ZP-EIP to request the current measurement.
    Decodes the hexadecimal ASCII response into a float (millimeters).
    """
    # 'MS' command: Get latest measured value.
    # Format: MS,Channel,TimeInfo (01 = CH1, 0 = Time Stamp)
    command_str = "MS,01,0\r\n"
    sock.sendall(command_str.encode('ascii'))
    
    # Receive response from Omron unit
    response = sock.recv(1024).decode('ascii').strip()
    
    # Example response: MS,0000012A,FFFF9BCD,00
    # Split the response by comma
    parts = response.split(',')
    
    if len(parts) >= 3 and parts[0] == 'MS':
        hex_value_str = parts[2].strip()
        
        # Convert to integer
        try:
            int_val = int(hex_value_str, 16)
        except ValueError:
            return 999.99
            
        # Check for sensor error/out of range (typically 0x7FFF0000 or 0x7FFFFFFE)
        if 0x7FFF0000 <= int_val <= 0x7FFFFFFF:
            return 999.99 # Out of range (no wheel)
            
        # Handle 32-bit two's complement for negative numbers
        if int_val >= 0x80000000:
            int_val -= 0x100000000
            
        # Omron outputs in 0.01 micrometer steps for this model. Divide by 100,000 to get mm.
        mm_value = int_val / 100000.0 
        return mm_value
        
    return 999.99 # Fallback if parsing fails

def main():
    print(f"Connecting to Omron ZP-EIP at {SENSOR_IP}:{SENSOR_PORT}...")
    
    # Initialize the CSV file with headers if it doesn't exist
    with open(CSV_FILENAME, mode='a', newline='') as file:
        writer = csv.writer(file)
        writer.writerow(['Timestamp', 'OHT_Number', 'Max_Wear_Diameter_mm'])
    
    while True:
        try:
            # Create a TCP socket
            with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as sock:
                sock.settimeout(5.0)
                sock.connect((SENSOR_IP, SENSOR_PORT))
                print("Connected successfully! Starting live tracking...")
                
                wheel_in_view = False
                max_reading = 0.0
                
                while True:
                    # 1. Ask the sensor for the current measurement
                    current_distance = send_omron_command(sock)
                    
                    # 2. Check if a wheel has entered the laser beam
                    if current_distance < WHEEL_DETECT_THRESHOLD_MM:
                        if not wheel_in_view:
                            print("Wheel detected! Recording profile...")
                            wheel_in_view = True
                            max_reading = current_distance # Reset max reading for this pass
                        
                        # Track the lowest/highest point (the peak of the curve)
                        if current_distance < max_reading:
                            max_reading = current_distance
                            
                    # 3. Check if the wheel just left the laser beam
                    elif wheel_in_view and current_distance > WHEEL_DETECT_THRESHOLD_MM:
                        print(f"Wheel passed. Peak measurement: {max_reading:.3f} mm")
                        oht_no = get_oht_number()
                        timestamp = datetime.now().strftime('%Y-%m-%d %H:%M:%S')
                        
                        # Automatically save to the CSV file!
                        with open(CSV_FILENAME, mode='a', newline='') as file:
                            writer = csv.writer(file)
                            writer.writerow([timestamp, oht_no, round(max_reading, 3)])
                        
                        print(f"Logged to CSV: [{timestamp}] {oht_no} | {max_reading:.3f} mm\n")
                        wheel_in_view = False # Reset for the next wheel
                    
                    # Poll every 10 milliseconds (100 times a second)
                    time.sleep(0.01)
                    
        except ConnectionRefusedError:
            print(f"Failed to connect to {SENSOR_IP}:{SENSOR_PORT}. Retrying in 2 seconds...")
            time.sleep(2)
        except Exception as e:
            print(f"Connection dropped: {e}. Reconnecting in 2 seconds...")
            time.sleep(2)

if __name__ == "__main__":
    main()
