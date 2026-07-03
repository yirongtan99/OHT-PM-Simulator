# OHT PM Simulator - Codebase Documentation

This document breaks down the code in your `SimulatorApp` folder. It is designed to help you study the logic and explain it during your presentation. Rather than explaining literally every single bracket, this guide breaks the code down **block-by-block** into logical chunks, explaining what each section is responsible for.

---

## 1. Project Structure Overview
When you open the `SimulatorApp` folder, the codebase is split into these core files:
- **`Program.cs`**: The main entry point. This is where the application boots up.
- **`UI/MainForm.cs`**: Contains the core variables and application state.
- **`UI/MainForm.Layout.cs`**: Contains the code that mathematically draws and positions the UI (buttons, grids, panels).
- **`UI/MainForm.Events.cs`**: Contains the logic for what happens when you click buttons (e.g., Launch Hokuyo, Start Camera).
- **`UI/MainForm.Vision.cs`**: Contains the hardcore AI/Computer Vision logic (tracking the bounding box, taking screenshots, parsing the camera feed).
- **`Models/PmStep.cs` & `Models/WindowItem.cs`**: Simple data structures that hold information about the checklist steps and the dropdown window list.
- **`Utils/NativeMethods.cs`**: Contains advanced Windows API tools used to take screenshots of background applications.

---

## 2. Core Execution (Program.cs & MainForm.cs)

### `Program.cs`
This is the very first piece of code that runs when you double-click the app.
```csharp
[STAThread]
private static void Main()
{
    // Tells Windows to prepare the application for rendering
    ApplicationConfiguration.Initialize();
    
    // Boots up your MainForm UI window and keeps the app running
    Application.Run(new MainForm());
}
```

### `MainForm.cs`
This file contains the "Constructor" (the setup method) and all the core variables.
```csharp
public partial class MainForm : Form
{
    // A list that automatically updates the UI table when steps change
    private readonly BindingList<PmStep> steps = new();
    
    // A queue that holds the simulated waveform data for the graph
    private readonly Queue<float> waveform = new();

    public MainForm()
    {
        // Sets the title and default maximized state of the window
        Text = "OHT Mark 2 PM Workflow Simulator V5";
        WindowState = FormWindowState.Maximized;
        AutoScroll = true;

        // The following methods initialize the app:
        BuildSteps();           // Loads the 9 PM Steps into memory
        BuildUi();              // Draws the buttons, cards, and panels
        BindGrid();             // Connects the steps to the Data Table
        RefreshWindowList();    // Scans the PC for the Hokuyo app
        StartSimulationTimer(); // Starts the live waveform animation loop
    }
}
```

---

## 3. The Vision System (MainForm.Vision.cs)

This is the most complex and important part of your project. This file handles drawing the bounding box and simulating the sensor feed.

### The Sensor Waveform Loop
```csharp
private void UpdateWaveformImage()
{
    // Checks if the user selected the Hokuyo app from the dropdown
    WindowItem? selected = cmbWindow.SelectedItem as WindowItem;
    
    // If the Hokuyo app is selected, try to take a screenshot of it
    if (selected is not null && selected.Handle != IntPtr.Zero)
    {
        Bitmap? captured = TryCaptureWindow(selected.Handle);
        
        if (captured is not null)
        {
            // Convert the screenshot into a Computer Vision matrix (Mat)
            using var src = BitmapConverter.ToMat(captured);

            // Fetch the calibration values from the UI sliders
            int yMin = tbYMin.Value; // Top 2m line
            int yMax = tbYMax.Value; // Bottom box limit
            int xMin = tbXMin.Value; // Left box limit
            int xMax = tbXMax.Value; // Right box limit
            
            // Draw a green rectangle onto the image using the slider limits
            Cv2.Rectangle(src, new Point(xMin, yMin), new Point(xMax, yMax), new Scalar(0, 255, 0), 2);
            
            // Draw a strict red line at the top to represent the 2m cutoff limit
            Cv2.Line(src, new Point(xMin, yMin), new Point(xMax, yMin), new Scalar(0, 0, 255), 3);
            
            // Convert the heavily edited image back to a Bitmap and show it on the UI
            SetPicture(waveformBox, BitmapConverter.ToBitmap(src));
            return;
        }
    }
    
    // (If no app is selected, it falls back to drawing a fake placeholder wave)
}
```

### The Screenshot Engine
```csharp
private string SaveAppScreenshot(PmStep step, string conditionSuffix)
{
    // Ask the NativeMethods file to secretly capture the Hokuyo app
    WindowItem? selected = cmbWindow.SelectedItem as WindowItem;
    Bitmap? bmp = TryCaptureWindow(selected.Handle);

    // Format a clean filename: OHT-PM_Step-1_OBS-Center_PASS_10-30-55.png
    string time = DateTime.Now.ToString("HH-mm-ss");
    string filename = $"OHT-PM_Step-{step.Index}_{step.Sensor}_{conditionSuffix}_{time}.png";
    string path = Path.Combine(runFolder, filename);
    
    // Save the image directly to the hard drive
    bmp.Save(path, ImageFormat.Png);
    return path;
}
```

---

## 4. User Interaction (MainForm.Events.cs)

This file is all about what happens when the technician clicks a button.

### Launching the Hokuyo App
```csharp
private void BtnLaunchHokuyo_Click(object? sender, EventArgs e)
{
    // Search 4 folders up from the running app to find the PBSv200 Application folder
    string appDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "PBSv200 Application");
    string exePath = Path.Combine(appDir, "pbscfg.exe");
    
    // Tell Windows to execute the pbscfg.exe file
    var startInfo = new ProcessStartInfo { FileName = exePath, WorkingDirectory = appDir };
    Process.Start(startInfo);
    
    // Wait 1.5 seconds for the app to boot, then automatically select it in the dropdown!
    System.Windows.Forms.Timer autoSelectTimer = new() { Interval = 1500 };
    autoSelectTimer.Tick += (s, ev) => {
        RefreshWindowList();
        // ... selects the app automatically
    };
}
```

### Passing a PM Step
```csharp
private void BtnOk_Click(object? sender, EventArgs e)
{
    // Mark the current step in the table as Passed
    PmStep step = steps[currentIndex];
    step.Status = StepStatus.Passed;
    
    // Take a screenshot of the sensor app to serve as digital evidence
    SaveAppScreenshot(step, "PASS");
    
    // Move to the next step
    currentIndex++;
    
    // Refresh the UI to show the new table
    grid.Refresh();
}
```

---

## 5. UI Layout Engine (MainForm.Layout.cs)

This file purely exists to make the app look pretty. It handles all the complex mathematical resizing so the app looks great on any monitor.

```csharp
private void BuildRightPane(TableLayoutPanel root)
{
    // Creates the top bar containing the waveform and the camera feed
    var top = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
    
    // The waveform takes up 100% of the space, the camera takes up 0% (hidden by default)
    top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
    top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 0));
    
    // Creates the Toggle Vision pane button
    Button btnToggleVision = TopBarButton("Toggle Vision Pane", ... , (s, e) => {
        // When clicked, it changes the 0% width to 44%, forcing the camera pane to slide open!
        if (top.ColumnStyles[1].Width == 0) {
            top.ColumnStyles[1].Width = 44;
            top.ColumnStyles[0].Width = 56;
        }
    });
}
```
