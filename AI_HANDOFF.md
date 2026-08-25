# FYP - AI Handoff & Project State
*Last Updated: August 25, 2026*

## 🏢 Project Background
- **Project:** Automating Preventive Maintenance (PM) for OHT Obstacle Detection Sensors
- **Company:** GlobalFoundries Singapore (F7 AMHS)
- **Problem:** The current PM process for OHT Mark 2 sensors is manual, highly variable, and takes ~1 hour per OHT.
- **Core Architecture Pivot:** The project shifted away from expensive vision-guided robotics (e.g., OMRON TM5S) in favor of a cost-effective mechanical redesign using **self-aligning 15-pin magnetic connectors** and a "blind" cobot (ABB GoFa/PoWa). 
- **Signal Switching:** A custom PCB relay board using an Analog Multiplexer (CD4052) routes the data between the VHL, OBS Left, OBS Right, and OBS Center sensors seamlessly without contact bounce.

## 🗺️ The 3-Phase Roadmap
1. **Phase 1 (Current Focus):** Automate the Cable Plug-In/Out using the cobot and magnetic connectors.
2. **Phase 2:** Automate the Panel Shifting (motorised mechanism to simulate obstacles).
3. **Phase 3:** Integrated Stationary Workstation (unifying the cobot, panel shifter, and C# Simulator App into a one-button process).

## 🚫 Critical Rules (from AGENTS.md)
1. **AI Vision Model is OFF LIMITS:** Do NOT add anything about the AI vision model (ONNX, MobileNetV2, computer vision, voting system, obstacle detection AI) into the FYP report or any report drafts. This phase has been deprioritised and pushed back. Only include it when the user explicitly says so.
2. **Project Minutes:** Whenever significant progress or structural changes occur, update `Docs/Project_Minutes.md` with a timestamp and summary.

## 🎯 Current Situation
- **Hardware Status:** Physical parts (magnetic connectors, cobots, PCBs) for Phase 1 are being procured and will arrive in the coming weeks. We are in a holding pattern for hardware testing.

## 🧹 Repository Maintenance Performed (Aug 25)
- The main `FYProject` repository was cleaned up for seamless syncing between Work and Home.
- Added `.gitignore` to prevent tracking of C# build artifacts (`bin/`, `obj/`) and crash logs.
- Organized documents into `Docs/Project_Management/`.
- All changes committed and pushed to `origin/main`.

## 🚀 Next Steps / Focus Areas
Since hardware is pending, the immediate focus is strictly on Software and Documentation:
1. **Simulator App (C#):** 
   - Review recent uncommitted changes to `MainForm.cs` and `MainForm.Vision.cs`.
   - Ensure the UI logic is robust for when the hardware arrives.
2. **Final Report:**
   - Continue fleshing out `Report_Builder/drafts/Final_Report_Draft.md` according to the 3-phase roadmap and hardware pivot.

## 🤖 Instructions for AI
If you are reading this file on a new PC:
1. Acknowledge this state and the strict rules (especially regarding the AI Vision Model).
2. Ask the user whether they want to resume work on the **Simulator App** or the **Final Report**.
