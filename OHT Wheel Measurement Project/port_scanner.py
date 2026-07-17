import socket

def scan_ports(ip, ports):
    print(f"Scanning IP: {ip}")
    open_ports = []
    
    for port in ports:
        sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        sock.settimeout(0.5)  # Quick timeout
        result = sock.connect_ex((ip, port))
        
        if result == 0:
            print(f"[+] Port {port} is OPEN!")
            open_ports.append(port)
        else:
            pass # Port closed or filtered
            
        sock.close()
        
    print("\nScan complete.")
    if open_ports:
        print(f"Open ports found: {open_ports}")
    else:
        print("No open ports found in the specified list.")

if __name__ == "__main__":
    target_ip = "192.168.250.1"
    # Common industrial and Omron ports
    common_ports = [
        80, 443, 502, # Web & Modbus
        8500, 8501, 8502, # Omron No-Protocol / ASCII
        9600, 9601, # Omron FINS
        44818, 2222, # EtherNet/IP
        2883, 2884, # Other Omron sensors
        9876, 64000
    ]
    scan_ports(target_ip, common_ports)
