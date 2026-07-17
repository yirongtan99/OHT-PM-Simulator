import socket
import time

SENSOR_IP = '192.168.250.1' 
SENSOR_PORT = 64000

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
        return int_val / 100000.0 
    return 999.0

with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as sock:
    sock.settimeout(5.0)
    sock.connect((SENSOR_IP, SENSOR_PORT))
    print("Reading raw sensor distance every 0.5s (Ctrl+C to stop):")
    while True:
        dist = send_omron_command(sock)
        print(f"RAW SENSOR OUTPUT: {dist} mm")
        time.sleep(0.5)
