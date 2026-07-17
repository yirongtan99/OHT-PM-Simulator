# OHT Wheel Measurement Project: Equipment Overview and Testing Guide

This document provides an overview of the procured hardware and a step-by-step guide to begin testing the automated OHT wheel measurement system.

## 1. Equipment Overview

Here is a breakdown of what each piece of equipment does in your setup:

### OMRON S8VK-C24024 (Power Supply)
*   **What it is:** A 24V DC industrial power supply.
*   **Its Role:** Industrial sensors and controllers don't plug straight into a wall outlet. This unit takes your standard AC wall power and converts it into a stable 24V DC power to safely run your Omron sensors, controllers, and communication unit.

### OMRON ZP-EIP (EtherNet/IP Communication Unit)
*   **What it is:** A network communication module.
*   **Its Role:** This is the bridge between your sensors and your computer (or a PLC). It takes the raw distance data from the sensor controllers and sends it over an Ethernet cable so you can view, record, and analyze the measurements in real-time on your PC using Omron's software.

### OMRON ZP-LS100S (Laser Displacement Sensors & Controllers)
*   **What it is:** High-precision diffuse-reflective laser (LiDAR) sensors. You have two of these, along with a master and slave amplifier/controller.
*   **Their Role:** These are the "eyes" of the project. They shoot a laser beam at the OHT wheel and measure the distance based on how the light reflects back. 
*   **Master/Slave Setup:** Because you have two sensors, they are connected to a master controller and a slave controller. This allows the two sensors to talk to each other, synchronize their measurements (e.g., measuring both sides of an OHT at the exact same millisecond), and send all their data through a single connection to the ZP-EIP unit.

---

## 2. How the System Connects (The Architecture)

1.  **Power:** The `S8VK-C24024` power supply provides 24V DC power to the Master Controller, the Slave Controller, and the `ZP-EIP` communication unit.
2.  **Sensing:** The two `ZP-LS100S` laser sensor heads plug directly into their respective Master and Slave controllers.
3.  **Synchronization:** The Slave controller snaps into or wires directly to the Master controller so they act as one unified system.
4.  **Data Output:** The Master controller connects to the `ZP-EIP` communication unit.
5.  **PC Connection:** The `ZP-EIP` connects via a standard Ethernet cable to your laptop or PC for data logging.

---

## 3. How to Begin Testing

Now that you have the equipment, here is a step-by-step approach to safely start testing:

### Step 1: Wiring and Initial Power-Up
*   **Warning:** Ensure the power supply is *unplugged* from the wall before wiring the 24V DC lines to your controllers and ZP-EIP.
*   Wire the 24V output from the `S8VK-C24024` to the power terminals on your Master/Slave controllers and the `ZP-EIP`.
*   Plug the `ZP-LS100S` sensor heads into their controllers.
*   Power on the `S8VK-C24024` and verify that the LEDs on the controllers and sensors light up.

### Step 2: Software Setup
*   Download and install **Omron Wave Inspire ZP** (Omron's free setup and monitoring software) on your PC.
*   Connect an Ethernet cable from the `ZP-EIP` to your PC.
*   Configure your PC's IP address to be on the same subnet as the `ZP-EIP` (refer to the ZP-EIP manual for its default IP).
*   Open Wave Inspire ZP and establish a connection to the sensors.

### Step 3: Bench Testing & Alignment
*   Before mounting this to the track, test it on your desk. Place an object (or a spare OHT wheel if you have one) in front of the sensors.
*   Look at the Wave Inspire ZP software to see if the distance measurements are updating in real-time.
*   **Crucial Reminder from your PI:** Remember the "Critical Discovery" from your report! The sensors *must* be placed **parallel** to the OHT wheel, not perpendicular, to get accurate and consistent height measurements. Keep this in mind when positioning the sensors in your newly manufactured 4-part modular aluminum mount.

### Step 4: Live Data Collection
*   Once desk testing is successful, assemble the sensors into your 4-part modular mount.
*   Perform a dry run on the OHT track.
*   Use the software to log the data as an OHT passes by. Check for data consistency and ensure the readings don't wildly fluctuate (which would indicate a reflection or alignment issue).
