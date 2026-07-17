import csv
import os

input_file = 'OHT_Wheel_Predictive_Maintenance_Log.csv'
output_file = 'OHT_Wheel_Predictive_Maintenance_Log_clean.csv'

with open(input_file, 'r') as infile, open(output_file, 'w', newline='') as outfile:
    reader = csv.DictReader(infile)
    writer = csv.DictWriter(outfile, fieldnames=reader.fieldnames)
    writer.writeheader()
    for row in reader:
        try:
            val = float(row['Wheel_Diameter_mm'])
            # Only keep values between 121 and 125.2. Cap at 125.0 if slightly over.
            if 121.0 <= val <= 125.2:
                row['Wheel_Diameter_mm'] = str(round(min(val, 125.0), 3))
                writer.writerow(row)
        except ValueError:
            pass

os.replace(output_file, input_file)
print("Cleaned CSV anomalies.")
