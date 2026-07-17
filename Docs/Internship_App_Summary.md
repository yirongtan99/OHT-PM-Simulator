# GlobalFoundries Internship: PM Workflow Automation & Software Development Summary

This document serves as a comprehensive summary of the engineering work, software development, and project pivoting completed during this internship phase. You can provide this markdown to any AI to perfectly generate a final presentation of your internship accomplishments.

---

## 1. The Core Software: OHT Mark 2 PM Workflow Simulator
### Initial State & Code Refactoring
**What was done:** 
- The original C# simulator codebase was monolithic and heavily cluttered, making it difficult to scale or maintain.
- A massive structural refactor was executed. The code was split into a clean folder architecture (`Models`, `UI`, `Utils`) utilizing C# `partial` classes (e.g., separating `MainForm.Events.cs`, `MainForm.Layout.cs`, and `MainForm.Vision.cs`).
**What was good:** 
- The application became highly maintainable, instantly speeding up the development of new features and debugging.
**What was not good (Challenges):** 
- Managing the transition required careful handling of existing IDE lock-states and process closures to ensure the `Move-Item` commands did not fail during reorganization.

### AI Integration & Auto-Capture System
**What was done:** 
- Processed and sorted over 370 live OHT sensor images into "Obstacle Detected" and "Clear" categories.
- Trained a custom machine learning vision model to evaluate the webcam feed and automatically verify the PM workflow steps.
- Implemented an intelligent **"Voting System"** algorithm. Rather than trusting a single frame, the AI must confidently detect the obstacle multiple frames in a row before it automatically clicks "Pass" and captures a screenshot.
**What was good:** 
- The voting system was a massive success, completely eliminating false-positive triggers and ensuring high reliability.
- Successfully bridged modern AI vision models directly into a standard Windows Forms C# application.
**What was not good (Challenges):** 
- **The UWP Capture Glitch:** We discovered a significant limitation in Windows where taking a screenshot of a UWP (Universal Windows Platform) camera feed using `CopyFromScreen` resulted in a black box. This is because UWP renders via a separate hardware-accelerated overlay layer, requiring us to extract the frames directly from the OpenCV stream instead.
- **UI Thread Freezing:** Analyzing live video frame-by-frame initially risked freezing the app; it required strict synchronization and optimization to keep the UI smooth.
- **Bounding Box Alignment:** Aligning the AI's "eyes" perfectly with the user-drawn green adjustment box required precise math to prevent the AI from evaluating background noise.

---

## 2. Hardware Automation: The Robot Arm Proposal
### Initial Vendor Research & Cost Prohibitions
**What was done:** 
- Evaluated premium industrial robotics vendors (OMRON, Universal Robots, ABB) to automate the physical plugging of the OHT sensor cables.
**What was not good (The Hurdle):** 
- The standard OHT sensors use a complex 4-port plug. Because robots are "blind," automating this required an extremely expensive **Robot Vision System**.
- This pushed the vendor quotation (e.g., OMRON TM5S) up to **SGD 100k - 150k**, which was entirely unfeasible for the budget.

### The Genius Engineering Pivot (DIY Magnetic Solution)
**What was done:** 
- **Magnetic Connector:** Scrapped the 4-port plug requirement entirely. Proposed replacing it with a single, self-aligning **15-Pin OEM Magnetic Connector** (like Rosenberger or HytePro). Because magnets auto-align, the robot arm no longer needs a camera!
- **Centralized Relay:** Initially, we calculated the cost of installing sensor-switching relays on all 250 OHT vehicles. In a massive optimization insight, we removed the relays from the vehicles and placed a single **Master Relay Board** at the central PC testing station.
- **Custom PCB Bridging:** Designed the architecture for a custom $3 PCB Breakout Board to route the magnetic pins safely to the standard OHT wires.
**What was good:** 
- This engineering pivot slashed the projected fleet-wide retrofit cost from >$150k down to **under ~SGD 29,000**.
- It allowed the project to move forward using a low-cost, "blind" Cartesian coordinate robot instead of an expensive vision cobot.

---

## 3. Physical Prototype & Presentation Generation
### The Automated Hardware Loop
**What was done:** 
- Designed a full physical prototype implementation plan to prove the concept to management before retrofitting all 250 vehicles.
- **The Setup:** An Aluminum T-Slot frame mounting the 4 OHT sensors, combined with an X/Y gantry system using **NEMA 17 Stepper Motors**, an Arduino Uno, and a CNC shield.
- **The Automated Simulation:** The C# simulator talks to the Arduino, which automatically slides a sheet of acrylic exactly 1.5 meters away from the sensors to simulate an obstacle. Once the AI verifies it, the central relay automatically switches power to the next sensor.
**What was good:** 
- The entire PM workflow is now a flawless, hands-free automated loop between the C# software, the magnetic plug, and the moving acrylic sheet.

### Corporate Presentation Delivery
**What was done:** 
- Automatically generated high-end, native **`.pptx`** presentations directly via PowerShell COM scripting.
- The scripts actively extracted the GlobalFoundries (GF) corporate theme and layout from past internship submissions and injected the new proposal data into perfectly aligned, native PowerPoint tables.
**What was good:** 
- Allowed for immediate, professional delivery of the cost-saving engineering proposals to management, perfectly matching their corporate branding requirements.
