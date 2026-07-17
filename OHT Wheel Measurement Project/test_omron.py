import socket
import time

SENSOR_IP = '192.168.250.1'
SENSOR_PORT = 64000

print(f"Connecting to {SENSOR_IP}:{SENSOR_PORT}...")
try:
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as sock:
        sock.settimeout(2.0)
        sock.connect((SENSOR_IP, SENSOR_PORT))
        print("Connected! Reading 20 samples...")
        
        for _ in range(20):
            command_str = "MS,01,0\r\n"
            sock.sendall(command_str.encode('ascii'))
            response = sock.recv(1024).decode('ascii').strip()
            print(f"Raw response: {response}")
            time.sleep(0.1)
except Exception as e:
    print(f"Error: {e}")
