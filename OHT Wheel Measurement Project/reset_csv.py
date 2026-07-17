import csv

CSV_FILENAME = 'OHT_Wheel_Predictive_Maintenance_Log.csv'

with open(CSV_FILENAME, mode='w', newline='') as file:
    writer = csv.writer(file)
    writer.writerow(['Timestamp', 'OHT_Number', 'FL_mm', 'FR_mm', 'BL_mm', 'BR_mm'])

print("CSV reset with 4-wheel columns.")
