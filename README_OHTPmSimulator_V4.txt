# OHT Mark 2 PM Workflow Simulator V4

## Major upgrades
- Modernised responsive layout using nested TableLayoutPanels and PictureBoxes.
- Better spacing, cleaner button alignment, and more consistent card styling.
- Adapts better to large monitors and window resizing.
- Waveform panel can now optionally preview a selected already-open desktop app window.
- Vision panel can now optionally use a live PC/webcam feed through OpenCvSharp.
- Fallback behaviour remains available:
  - no app selected -> placeholder waveform
  - no camera running -> placeholder robot vision
- Screenshots saved with timestamps under:
  RunFolder\Screenshots\

## Packages used
- OpenCvSharp4.Windows
- OpenCvSharp4.runtime.win

## Recommended VS Code extensions
- C# Dev Kit
- C# extension

Microsoft notes that C# Dev Kit builds on the C# extension and that .NET development in VS Code requires the .NET SDK installed locally. citeturn84search1turn84search2

## Project file format
The project continues to use SDK-style WinForms configuration with a Windows target framework and UseWindowsForms=true, matching Microsoft desktop SDK guidance. citeturn84search9

## Files
Use these names in your folder:
- OHTPmSimulator_V4.csproj -> OHTPmSimulator_V4.csproj
- Program_V4.cs -> Program.cs

## Run commands (PowerShell)
cd "C:\Users\yiron\Desktop\FYProject"
dotnet clean
(dotnet restore is optional if already restored)
dotnet build .\OHTPmSimulator_V4.csproj
dotnet run --project .\OHTPmSimulator_V4.csproj

## First tests
1. Start Run
2. Capture App
3. Open Folder
4. Refresh window list and choose a visible app window for waveform preview
5. Start Camera
6. Test Adjustment Needed -> Adjustment Done -> OK/Next
