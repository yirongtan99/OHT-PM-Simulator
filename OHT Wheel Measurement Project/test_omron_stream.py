import socket

SENSOR_IP = '192.168.250.1' 
SENSOR_PORT = 64000

def test_sensor_stream():
    print(f"Connecting to {SENSOR_IP}:{SENSOR_PORT}...")
    try:
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as sock:
            sock.settimeout(2.0)
            sock.connect((SENSOR_IP, SENSOR_PORT))
            print("Connected! Fetching 5 readings...")
            
            for i in range(5):
                cmd = b"MS,01,0\r\n"
                sock.sendall(cmd)
                resp = sock.recv(1024)
                print(f"Reading {i+1}: {resp}")
                
    except Exception as e:
        print(f"Error: {e}")

if __name__ == "__main__":
    test_sensor_stream()
