# AI Handoff & Project State
*Last Updated: August 25, 2026*

## 🎯 Current Situation
- **Hardware Status:** Physical parts for the FYP are currently being procured and are expected to arrive in the coming weeks.
- **Project ODIN (OHT Wheel Measurement):** Temporarily paused. 
  - **Status:** The software was verified, cleaned, and a `requirements.txt` was added. 
  - **Handover:** The OHT codebase was zipped and sent to a colleague, and also pushed to a dedicated, clean GitHub repository (`yirongtan99/Project-Odin`).

## 🧹 Repository Maintenance Performed
- The main `FYProject` repository was heavily cleaned up to allow seamless syncing between the Work PC and Home PC.
- Added a `.gitignore` to prevent tracking of C# build artifacts (`bin/`, `obj/`) and crash logs.
- Organized loose documents into `Docs/Project_Management/`.
- Moved side projects (`pbandai_bot.py`) into `Misc_SideProjects/`.
- All changes have been successfully committed and pushed to `origin/main`.

## 🚀 Next Steps / Focus Areas
Since hardware is pending, the immediate focus is on Software and Documentation:
1. **Simulator App (C#):** 
   - Review recent uncommitted changes to `MainForm.cs` and `MainForm.Vision.cs`.
   - Ensure the UI and simulated vision systems are robust and bug-free in preparation for the real hardware.
2. **Final Report:**
   - Flesh out the architecture and software design sections in `Report_Builder/drafts/Final_Report_Draft.md`.

## 🤖 Instructions for AI
If you are reading this file, you have likely been booted up on a different PC. 
1. Acknowledge this state.
2. Ask the user whether they want to resume work on the **Simulator App** or the **Final Report**.
