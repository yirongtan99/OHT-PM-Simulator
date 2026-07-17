import urllib.request
import json

try:
    with urllib.request.urlopen('http://localhost:5000/api/history') as response:
        data = json.loads(response.read().decode())
        print("API HISTORY LENGTH:", len(data))
        if len(data) > 0:
            print("LAST RECORD:", json.dumps(data[-1], indent=2))
            print("FIRST RECORD:", json.dumps(data[0], indent=2))
except Exception as e:
    print(f"Error: {e}")
