import csv
with open('OHT_Wheel_Predictive_Maintenance_Log.csv', 'r') as file:
    reader = csv.DictReader(file)
    for row in reader:
        print(repr(list(row.keys())))
        break
