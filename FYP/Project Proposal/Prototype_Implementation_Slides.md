# Automated Sensor Testing Prototype - Implementation Plan

---

## Slide 1: Title Slide
**Title:** Automated PM Workflow: Prototype Implementation Plan
**Subtitle:** Full-Scale Physical Simulation & Hardware Setup
**Presenter:** Tan Yi Rong
**Date:** July 2026

---

## Slide 2: Prototype Objective
**Header:** Moving from Concept to Physical Testing
**Bullet Points:**
- **The Goal:** Build a small-scale, fully functional physical prototype that perfectly mimics the Overhead Hoist Transport (OHT) sensor environment.
- **Why We Need It:** Before retrofitting 250 vehicles, we must validate the new magnetic connector logic and prove our centralized testing station concept works flawlessly.
- **The Scope:** Automate the physical "Detect / No-Detect" environment to eliminate the need for an engineer to manually stand in front of the sensors.

---

## Slide 3: Structural Architecture
**Header:** The Aluminum Frame & Sensor Mounts
**Bullet Points:**
- **The Base Structure:** A rigid structure built using standard T-Slot Extruded Aluminum (e.g., 2020 or 3030 series) for maximum modularity and ease of adjustment.
- **Sensor Mounting:** All 4 OHT sensors will be securely mounted to the top beam of the aluminum frame, spaced out to perfectly replicate their actual positions on a real Mark 2 OHT chassis.
- **Calibration:** The frame will be designed to allow slight Z-axis and X-axis adjustments to fine-tune the sensor angles before locking them down.

---

## Slide 4: Automated Obstacle Simulation
**Header:** Motorized "Detect / No-Detect" Acrylic Rig
**Bullet Points:**
- **The Physical Concept:** Instead of an engineer manually blocking the sensors, a large sheet of acrylic will serve as the artificial "obstacle."
- **Distance Accuracy:** The rig will be calibrated to mimic an obstacle exactly **1.5 meters** away from the mounted sensors.
- **Automated Movement:** The acrylic sheet will be programmed to automatically slide Left/Right and Up/Down, triggering the different sensors sequentially to generate live "Detect" and "Clear" data for the PM Workflow Simulator.

---

## Slide 5: Required Hardware & Motors
**Header:** Mechanical Prototyping Bill of Materials
**Bullet Points:**
- **Linear Motion:** 2x Linear Guide Rails (with carriage blocks) to ensure the acrylic sheet slides smoothly without vibration.
- **Actuation (Motors):** 2x NEMA 17 Stepper Motors (one for the X-axis, one for the Y-axis) to provide precise, programmable control over the acrylic sheet's position.
- **Drive System:** GT2 Timing Belts and Pulleys connected to the stepper motors to translate rotational movement into linear sliding.
- **The Target:** 1x Matte or Tinted Acrylic Sheet (sized appropriately to block the sensors without reflecting stray light).

---

## Slide 6: Electrical & Wiring Architecture
**Header:** Wiring the Centralized Testing Station
**Bullet Points:**
- **Motor Control:** An Arduino Uno (or similar microcontroller) paired with a CNC Shield and A4988 Stepper Drivers to execute the programmed movement of the acrylic sheet.
- **Sensor Wiring:** The 4 sensors on the aluminum frame will be wired down into the proposed **Centralized Relay Station** we discussed previously.
- **Magnetic Plug Test:** The relay station will route through our prototype PCB Bridge Board, connecting to a sample OEM Magnetic 15-Pin Plug to prove signal integrity.
- **Power Supply:** A dedicated 12V / 24V industrial power supply to drive both the OHT sensors and the stepper motors simultaneously.

---

## Slide 7: Budget Justification & Procurement
**Header:** Prototype Items List & Supervisor Justification
**Bullet Points:**
- **Items to Purchase:** T-Slot Aluminum Extrusions, 2x NEMA 17 Motors + Rails, Arduino Kit, 4-Channel Relay Module, OEM Magnetic Plug Samples, Acrylic Sheet. 
- **Purchase Justification:** Procuring these low-cost Maker/DIY components (estimated under SGD 300 total) allows us to physically validate the centralized relay logic and the magnetic plug alignment. 
- **ROI (Return on Investment):** This minimal upfront prototyping cost acts as the final proof-of-concept required to unlock our SGD ~42k fleet retrofit strategy, definitively proving to management that a >SGD 100k vision-assisted cobot is entirely unnecessary.

---

## Slide 8: Full System Integration (The Grand Vision)
**Header:** The Complete Automated Workflow
**Bullet Points:**
- **The DIY Coordinate Robot:** Alongside the simulation frame, I will construct a simple, low-cost Cartesian (coordinate-based) robot arm. Because the magnetic plug is self-aligning, this DIY arm only needs "blind" coordinate movement to plug in and out—no expensive camera vision is required!
- **The Seamless Loop:** 
  1. The DIY arm automatically snaps the magnetic plug into the testing port.
  2. The C# PM Simulator commands the acrylic sheet to move, simulating an obstacle.
  3. As the Simulator successfully detects the obstacle and the "Next Step" button is pressed, the Central Relay instantly switches power to the *next* sensor.
  4. The acrylic sheet automatically shifts to the new sensor, entirely automating the once-manual PM testing process!
