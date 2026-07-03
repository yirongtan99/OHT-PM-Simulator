using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using OpenCvSharp;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace OHTPmSimulatorV5;

public partial class MainForm : Form
{
    private readonly BindingList<PmStep> steps = new();
    private readonly Queue<float> waveform = new();
    private readonly Random rng = new();
    private readonly System.Windows.Forms.Timer uiTimer = new();

    private int currentIndex = -1;
    private int retestCount = 0;
    private bool waitingAdjustment = false;
    private string rootFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "OHT_PM_Runs");
    private string? runFolder;
    private float latestReading = 0.5f;
    private int visionX = 120, visionY = 90, visionTargetX = 140, visionTargetY = 110;

    private VideoCapture? webcam;
    private bool webcamEnabled = false;
    private readonly List<WindowItem> captureWindows = new();
    
    // AI Variables
    private InferenceSession? onnxSession;
    private readonly Queue<bool> aiVotingQueue = new();

    // UI
    private Label lblRunFolder = new();
    private Label lblStep = new();
    private Label lblSensor = new();
    private Label lblCondition = new();
    private Label lblStatus = new();
    private Label lblGuidance = new();
    private Label lblSaveRoot = new();
    private Label lblReading = new();
    private Label lblVision = new();
    private Label lblWaveSource = new();

    private DataGridView grid = new();
    private RichTextBox logBox = new();
    private TextBox noteBox = new();
    private Panel progressPanel = new();
    private PictureBox waveformBox = new();
    private PictureBox visionBox = new();
    private ComboBox cmbWindow = new();
    
    // Auto Capture UI
    private CheckBox chkAutoCapture = new();
    private CheckBox chkShowCoords = new();
    private TrackBar tbYMin = new();
    private TrackBar tbYMax = new();
    private TrackBar tbXMin = new();
    private TrackBar tbXMax = new();
    private Label lblCoordinates = new();

    private Button btnStart = new();
    private Button btnOk = new();
    private Button btnFail = new();
    private Button btnAdjusted = new();
    private Button btnCaptureApp = new();
    private Button btnCaptureScreen = new();
    private Button btnOpenFolder = new();
    private Button btnSetPath = new();
    private Button btnRefreshWindows = new();
    private Button btnToggleCamera = new();
    private Button btnReset = new();
    private Button btnComplete = new();
    private Button btnLaunchHokuyo = new();

    public MainForm()
    {
        Text = "OHT Mark 2 PM Workflow Simulator V5";
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;
        AutoScroll = true;
        BackColor = Color.FromArgb(11, 15, 26);
        Font = new Font("Segoe UI", 9.5f);
        DoubleBuffered = true;

        BuildSteps();
        BuildUi();
        BindGrid();
        RefreshWindowList();
        LoadAiModel();
        ResetUi(clearLog: true);
        StartSimulationTimer();
        FormClosing += MainForm_FormClosing;
    }

    private void BuildSteps()
    {
        steps.Clear();
        steps.Add(new PmStep("Docking", "Connector Interface", "Docked", "Confirm docking interface is seated correctly, then press OK."));
        steps.Add(new PmStep("VHS Check", "VHS", "Clear Condition", "Ensure reflective plate is away from the VHS detection path."));
        steps.Add(new PmStep("VHS Check", "VHS", "Reflective Plate Present", "Move reflective plate into VHS detection path."));
        steps.Add(new PmStep("OBS Left Check", "OBS Left", "Clear Condition", "Ensure no obstacle in front of OBS Left."));
        steps.Add(new PmStep("OBS Left Check", "OBS Left", "Obstacle Present", "Move test panel into OBS Left detection path."));
        steps.Add(new PmStep("OBS Center Check", "OBS Center", "Clear Condition", "Ensure no obstacle in front of OBS Center."));
        steps.Add(new PmStep("OBS Center Check", "OBS Center", "Obstacle Present", "Move test panel into OBS Center detection path."));
        steps.Add(new PmStep("OBS Right Check", "OBS Right", "Clear Condition", "Ensure no obstacle in front of OBS Right."));
        steps.Add(new PmStep("OBS Right Check", "OBS Right", "Obstacle Present", "Move test panel into OBS Right detection path."));
        for (int i = 0; i < steps.Count; i++) steps[i].Index = i + 1;
    }

    private void LoadCurrentStep()
    {
        if (currentIndex < 0 || currentIndex >= steps.Count) { CompleteRun(); return; }
        PmStep step = steps[currentIndex];
        step.Status = waitingAdjustment ? StepStatus.AdjustmentRequired : StepStatus.InProgress;
        lblStep.Text = step.StepName;
        lblSensor.Text = step.Sensor;
        lblCondition.Text = step.Condition;
        lblStatus.Text = waitingAdjustment ? "Adjustment Required" : "Waiting for Confirmation";
        lblStatus.ForeColor = waitingAdjustment ? Color.FromArgb(255, 118, 117) : Color.FromArgb(9, 132, 227);
        lblGuidance.Text = step.Guidance;
        progressPanel.Invalidate();
        grid.Refresh();
        grid.ClearSelection();
        if (currentIndex < grid.Rows.Count) grid.Rows[currentIndex].Selected = true;
        Log("Loaded step " + (currentIndex + 1) + "/" + steps.Count + ": " + step.StepName + " | " + step.Sensor + " | " + step.Condition);
    }

    private void CompleteRun()
    {
        if (runFolder is null) return;
        lblStep.Text = "-";
        lblSensor.Text = "-";
        lblCondition.Text = "-";
        lblStatus.Text = "Completed";
        lblStatus.ForeColor = Color.FromArgb(0, 184, 148);
        lblGuidance.Text = "PM check completed. Review saved files and summary.";
        progressPanel.Invalidate();
        WriteSummary();
        Log("PM RUN COMPLETED.");
        SetButtons(true, false, false, false, false, false, true, true, true, false);
        OpenRunFolder();
    }

    private string CreateRunFolder()
    {
        string folder = Path.Combine(rootFolder, DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HHmmss") + "_PM_Run");
        Directory.CreateDirectory(Path.Combine(folder, "Screenshots"));
        return folder;
    }

    private bool EnsureRunFolder()
    {
        if (runFolder is not null) return true;
        MessageBox.Show("Please press Start Run first so a run folder can be created.");
        return false;
    }

    private PmStep CurrentStepOrPlaceholder()
    {
        if (currentIndex >= 0 && currentIndex < steps.Count) return steps[currentIndex];
        return new PmStep("Manual Capture", "N/A", "N/A", "Manual capture") { Index = 0 };
    }

    private string ScreenshotPath(PmStep step, string result)
    {
        if (runFolder is null) throw new InvalidOperationException("No run folder.");
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
        string name = $"{step.Index:D2}_{Safe(step.Sensor)}_{Safe(step.Condition)}_{result}_R{retestCount}_{timestamp}.png";
        return Path.Combine(runFolder, "Screenshots", name);
    }

    private string SaveAppScreenshot(PmStep step, string result)
    {
        string file = ScreenshotPath(step, result);
        Rectangle rect = Bounds;
        using Bitmap bmp = new(rect.Width, rect.Height);
        using Graphics g = Graphics.FromImage(bmp);
        g.CopyFromScreen(rect.Location, System.Drawing.Point.Empty, rect.Size);
        bmp.Save(file, ImageFormat.Png);
        return file;
    }

    private string SaveFullScreenScreenshot(PmStep step, string result)
    {
        string file = ScreenshotPath(step, result);
        Rectangle rect = Screen.PrimaryScreen?.Bounds ?? Bounds;
        using Bitmap bmp = new(rect.Width, rect.Height);
        using Graphics g = Graphics.FromImage(bmp);
        g.CopyFromScreen(rect.Location, System.Drawing.Point.Empty, rect.Size);
        bmp.Save(file, ImageFormat.Png);
        return file;
    }

    private static string Safe(string text)
    {
        foreach (char c in Path.GetInvalidFileNameChars()) text = text.Replace(c, '_');
        return text.Replace(" ", "_");
    }

    private void WriteSummary()
    {
        if (runFolder is null) return;
        using StreamWriter sw = new(Path.Combine(runFolder, "Run_Summary.txt"), false);
        sw.WriteLine("OHT Mark 2 PM Simulator V4 - Run Summary");
        sw.WriteLine("Generated: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        sw.WriteLine("Run Folder: " + runFolder);
        sw.WriteLine(new string('-', 80));
        foreach (PmStep step in steps)
            sw.WriteLine($"{step.Index:D2}. {step.StepName} | {step.Sensor} | {step.Condition} | {step.DisplayStatus} | Note: {step.Note}");
    }

    private void OpenRunFolder()
    {
        if (runFolder is null || !Directory.Exists(runFolder)) { MessageBox.Show("No run folder available."); return; }
        OpenPath(runFolder);
    }

    private static void OpenPath(string path)
    {
        if (File.Exists(path) || Directory.Exists(path))
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = path, UseShellExecute = true });
    }

    private void ResetUi(bool clearLog)
    {
        currentIndex = -1;
        waitingAdjustment = false;
        retestCount = 0;
        runFolder = null;
        foreach (PmStep step in steps)
        {
            step.Status = StepStatus.Pending;
            step.Note = "";
        }
        lblRunFolder.Text = "-";
        lblStep.Text = "-";
        lblSensor.Text = "-";
        lblCondition.Text = "-";
        lblStatus.Text = "Idle";
        lblStatus.ForeColor = Color.FromArgb(148, 163, 184);
        lblGuidance.Text = "Press Start Run to begin.";
        lblSaveRoot.Text = rootFolder;
        noteBox.Clear();
        progressPanel.Invalidate();
        grid.Refresh();
        if (clearLog) logBox.Clear();
        Log("System ready. Waiting to start a new PM run.");
        SetButtons(true, false, false, false, false, false, false, true, true, false);
    }

    private void SetButtons(bool start, bool ok, bool fail, bool adjusted, bool captureApp, bool captureScreen, bool openFolder, bool setPath, bool reset, bool complete)
    {
        btnStart.Enabled = start;
        btnOk.Enabled = ok;
        btnFail.Enabled = fail;
        btnAdjusted.Enabled = adjusted;
        btnCaptureApp.Enabled = captureApp;
        btnCaptureScreen.Enabled = captureScreen;
        btnOpenFolder.Enabled = openFolder;
        btnSetPath.Enabled = setPath;
        btnReset.Enabled = reset;
        btnComplete.Enabled = complete;
    }

    private void SetPicture(PictureBox box, Bitmap image)
    {
        var old = box.Image;
        box.Image = image;
        old?.Dispose();
    }

    private static void DisposePicture(PictureBox box)
    {
        var img = box.Image;
        box.Image = null;
        img?.Dispose();
    }

    private void Log(string message)
    {
        logBox.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + message + Environment.NewLine);
        logBox.ScrollToCaret();
    }

    private void LoadAiModel()
    {
        string modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "AI_Model", "oht_sensor_model.onnx");
        if (File.Exists(modelPath))
        {
            try
            {
                onnxSession = new InferenceSession(modelPath);
                Log("AI Brain natively loaded! ONNX Inference Session Active.");
            }
            catch (Exception ex)
            {
                Log($"Failed to load AI model: {ex.Message}");
            }
        }
        else
        {
            Log($"Warning: AI model not found at {modelPath}. Auto-Capture will not work.");
        }
    }
}
