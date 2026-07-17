using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using CvSize = OpenCvSharp.Size;

namespace OHTPmSimulatorV5;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        try
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
        catch (Exception ex)
        {
            string errorPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "StartupError.txt");
            File.WriteAllText(errorPath, ex.ToString());

            MessageBox.Show(
                "The application failed to start.\n\nError saved to:\n" + errorPath,
                "Startup Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }
}

public sealed class MainForm : Form
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
        MinimumSize = new System.Drawing.Size(1360, 820);
        BackColor = Color.FromArgb(11, 15, 26);
        Font = new Font("Segoe UI", 9.5f);
        DoubleBuffered = true;

        BuildSteps();
        BuildUi();
        BindGrid();
        RefreshWindowList();
        ResetUi(clearLog: true);
        StartSimulationTimer();
        FormClosing += MainForm_FormClosing;
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        StopCamera();
        DisposePicture(waveformBox);
        DisposePicture(visionBox);
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

    private void BuildUi()
    {
        var header = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.FromArgb(20, 27, 45) };
        Controls.Add(header);
        
        var headerAccent = new Panel { Dock = DockStyle.Bottom, Height = 2, BackColor = Color.FromArgb(9, 132, 227) };
        header.Controls.Add(headerAccent);

        header.Controls.Add(new Label
        {
            Text = "OHT Mark 2 PM Workflow Simulator",
            ForeColor = Color.FromArgb(248, 250, 252),
            Font = new Font("Segoe UI Semibold", 18f, FontStyle.Bold),
            Bounds = new Rectangle(22, 12, 1150, 36)
        });
        header.Controls.Add(new Label
        {
            Text = "Preventive Maintenance Workflow Checklist & Digital Evidence Capture System",
            ForeColor = Color.FromArgb(148, 163, 184),
            Font = new Font("Segoe UI", 9.5f),
            Bounds = new Rectangle(24, 48, 1280, 24)
        });

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.FromArgb(11, 15, 26)
        };
        root.ColumnStyles.Clear();
        root.RowStyles.Clear();
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 360));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        Controls.Add(root);

        BuildLeftPane(root);
        BuildRightPane(root);
    }

    private void BuildLeftPane(TableLayoutPanel root)
    {
        var left = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Margin = Padding.Empty,
            BackColor = Color.FromArgb(11, 15, 26)
        };
        left.ColumnStyles.Clear();
        left.RowStyles.Clear();
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 210));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 292));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 126));
        left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.Controls.Add(left, 0, 0);

        var info = Card("Run Information");
        lblRunFolder = InfoRow(info, "Run Folder:", 16);
        lblStep = InfoRow(info, "Current Step:", 50);
        lblSensor = InfoRow(info, "Sensor:", 84);
        lblCondition = InfoRow(info, "Condition:", 118);
        lblStatus = InfoRow(info, "Status:", 152);
        left.Controls.Add(info, 0, 0);

        var guidance = Card("Technician Guidance");
        lblGuidance = new Label
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(14, 42, 14, 12),
            ForeColor = Color.FromArgb(226, 232, 240),
            Font = new Font("Segoe UI", 9.5f),
            Text = "Press Start Run to begin."
        };
        guidance.Controls.Add(lblGuidance);
        left.Controls.Add(guidance, 0, 1);

        var controls = Card("Operator Controls");
        var buttonGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12, 40, 12, 10),
            ColumnCount = 2,
            RowCount = 5,
            BackColor = Color.Transparent
        };
        buttonGrid.ColumnStyles.Clear();
        buttonGrid.RowStyles.Clear();
        buttonGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        buttonGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        for (int i = 0; i < 5; i++) buttonGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 20));

        btnStart = ActionButton("Start Run", Color.FromArgb(9, 132, 227), Color.White, BtnStart_Click);
        btnOk = ActionButton("OK / Next", Color.FromArgb(0, 184, 148), Color.White, BtnOk_Click);
        btnFail = ActionButton("Adjustment Needed", Color.FromArgb(255, 118, 117), Color.White, BtnFail_Click);
        btnAdjusted = ActionButton("Adjustment Done", Color.FromArgb(162, 155, 254), Color.White, BtnAdjusted_Click);
        btnCaptureApp = ActionButton("Capture App", Color.FromArgb(45, 52, 54), Color.White, BtnCaptureApp_Click);
        btnCaptureScreen = ActionButton("Capture Screen", Color.FromArgb(45, 52, 54), Color.White, BtnCaptureScreen_Click);
        btnOpenFolder = ActionButton("Open Folder", Color.FromArgb(45, 52, 54), Color.White, BtnOpenFolder_Click);
        btnSetPath = ActionButton("Set Save Path", Color.FromArgb(45, 52, 54), Color.White, BtnSetPath_Click);
        btnReset = ActionButton("Reset", Color.FromArgb(45, 52, 54), Color.White, BtnReset_Click);
        btnComplete = ActionButton("Complete Run", Color.FromArgb(224, 86, 153), Color.White, BtnComplete_Click);

        buttonGrid.Controls.Add(btnStart, 0, 0); buttonGrid.Controls.Add(btnOk, 1, 0);
        buttonGrid.Controls.Add(btnFail, 0, 1); buttonGrid.Controls.Add(btnAdjusted, 1, 1);
        buttonGrid.Controls.Add(btnCaptureApp, 0, 2); buttonGrid.Controls.Add(btnCaptureScreen, 1, 2);
        buttonGrid.Controls.Add(btnOpenFolder, 0, 3); buttonGrid.Controls.Add(btnSetPath, 1, 3);
        buttonGrid.Controls.Add(btnReset, 0, 4); buttonGrid.Controls.Add(btnComplete, 1, 4);
        controls.Controls.Add(buttonGrid);
        left.Controls.Add(controls, 0, 2);

        var notes = Card("Step Notes");
        noteBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(14, 42, 14, 12),
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            PlaceholderText = "Optional note for the current step or adjustment...",
            BackColor = Color.FromArgb(11, 15, 26),
            ForeColor = Color.FromArgb(248, 250, 252),
            BorderStyle = BorderStyle.FixedSingle
        };
        notes.Controls.Add(noteBox);
        left.Controls.Add(notes, 0, 3);

        var pathCard = Card("Save Root Folder");
        lblSaveRoot = new Label
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(14, 42, 14, 14),
            AutoEllipsis = true,
            ForeColor = Color.FromArgb(148, 163, 184),
            Text = rootFolder
        };
        pathCard.Controls.Add(lblSaveRoot);
        left.Controls.Add(pathCard, 0, 4);
    }

    private void BuildRightPane(TableLayoutPanel root)
    {
        var right = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Margin = new Padding(12, 0, 0, 0),
            BackColor = Color.FromArgb(11, 15, 26)
        };
        right.RowStyles.Clear();
        right.ColumnStyles.Clear();
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 0));
        right.RowStyles.Add(new RowStyle(SizeType.Percent, 68));
        right.RowStyles.Add(new RowStyle(SizeType.Percent, 32));

        root.Controls.Add(right, 1, 0);

        var top = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty, BackColor = Color.FromArgb(11, 15, 26) };
        top.ColumnStyles.Clear();
        top.RowStyles.Clear();
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56));
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44));
        right.Controls.Add(top, 0, 1);

        var wfCard = Card("Sensor Waveform Reading");

        Panel topCtrlPanel = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Color.Transparent };
        wfCard.Controls.Add(topCtrlPanel);
        topCtrlPanel.BringToFront();

        lblReading = new Label
        {
            Bounds = new Rectangle(16, 4, 400, 22),
            Text = "Reading source: placeholder / no live app selected",
            ForeColor = Color.FromArgb(226, 232, 240)
        };
        topCtrlPanel.Controls.Add(lblReading);

        cmbWindow = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Bounds = new Rectangle(16, 30, 280, 28),
            BackColor = Color.FromArgb(11, 15, 26),
            ForeColor = Color.FromArgb(248, 250, 252),
            FlatStyle = FlatStyle.Flat
        };
        topCtrlPanel.Controls.Add(cmbWindow);

        btnRefreshWindows = TopBarButton("Refresh App List", Color.FromArgb(45, 52, 54), Color.White, BtnRefreshWindows_Click);
        btnRefreshWindows.SetBounds(306, 30, 120, 28);
        topCtrlPanel.Controls.Add(btnRefreshWindows);

        btnLaunchHokuyo = TopBarButton("Launch Hokuyo", Color.FromArgb(9, 132, 227), Color.White, BtnLaunchHokuyo_Click);
        btnLaunchHokuyo.SetBounds(436, 30, 120, 28);
        topCtrlPanel.Controls.Add(btnLaunchHokuyo);
        
        chkAutoCapture = new CheckBox { Text = "Auto Capture PM", ForeColor = Color.White, Bounds = new Rectangle(566, 32, 150, 24) };
        topCtrlPanel.Controls.Add(chkAutoCapture);
        
        chkShowCoords = new CheckBox { Text = "Show Coords", ForeColor = Color.FromArgb(0, 184, 148), Checked = true, Bounds = new Rectangle(720, 32, 120, 24) };
        topCtrlPanel.Controls.Add(chkShowCoords);

        Panel graphContainer = new Panel 
        { 
            Dock = DockStyle.Fill,
            Padding = new Padding(14),
            BackColor = Color.FromArgb(20, 27, 45) 
        };
        wfCard.Controls.Add(graphContainer);
        graphContainer.BringToFront();

        tbYMin = new TrackBar { Orientation = Orientation.Vertical, Dock = DockStyle.Left, Width = 30, Minimum = 0, Maximum = 1000, Value = 100, TickStyle = TickStyle.None };
        tbYMax = new TrackBar { Orientation = Orientation.Vertical, Dock = DockStyle.Right, Width = 30, Minimum = 0, Maximum = 1000, Value = 500, TickStyle = TickStyle.None };
        tbXMin = new TrackBar { Orientation = Orientation.Horizontal, Dock = DockStyle.Top, Height = 30, Minimum = 0, Maximum = 1000, Value = 100, TickStyle = TickStyle.None };
        tbXMax = new TrackBar { Orientation = Orientation.Horizontal, Dock = DockStyle.Bottom, Height = 30, Minimum = 0, Maximum = 1000, Value = 800, TickStyle = TickStyle.None };

        waveformBox = new PictureBox
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(11, 15, 26),
            SizeMode = PictureBoxSizeMode.Zoom,
            BorderStyle = BorderStyle.None
        };
        
        graphContainer.Controls.Add(waveformBox);
        graphContainer.Controls.Add(tbXMin);
        graphContainer.Controls.Add(tbXMax);
        graphContainer.Controls.Add(tbYMin);
        graphContainer.Controls.Add(tbYMax);

        top.Controls.Add(wfCard, 0, 0);

        var visCard = Card("Robot Vision / PC Camera Feed");

        lblVision = new Label
        {
            Bounds = new Rectangle(16, 40, 360, 22),
            Text = "Vision status: camera not started",
            ForeColor = Color.FromArgb(226, 232, 240)
        };
        visCard.Controls.Add(lblVision);

        btnToggleCamera = TopBarButton("Start PC Camera", Color.FromArgb(9, 132, 227), Color.White, BtnToggleCamera_Click);
        btnToggleCamera.SetBounds(16, 66, 150, 30);
        visCard.Controls.Add(btnToggleCamera);

        var camHint = new Label
        {
            Text = "Click Start PC Camera to display live webcam feed.",
            Bounds = new Rectangle(180, 71, 420, 22),
            ForeColor = Color.FromArgb(148, 163, 184)
        };
        visCard.Controls.Add(camHint);

        visionBox = new PictureBox
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(14, 112, 14, 14),
            BackColor = Color.FromArgb(11, 15, 26),
            SizeMode = PictureBoxSizeMode.Zoom,
            BorderStyle = BorderStyle.None
        };
        visCard.Controls.Add(visionBox);

        top.Controls.Add(visCard, 1, 0);

        var bottom = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = new Padding(0, 12, 0, 0), BackColor = Color.FromArgb(11, 15, 26) };
        bottom.ColumnStyles.Clear();
        bottom.RowStyles.Clear();
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56));
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44));
        right.Controls.Add(bottom, 0, 2);

        var statusCard = Card("Status Table");
        progressPanel = new Panel { Bounds = new Rectangle(16, 42, 650, 10), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, BackColor = Color.FromArgb(11, 15, 26) };
        progressPanel.Paint += ProgressPanel_Paint;
        statusCard.Controls.Add(progressPanel);
        grid = new DataGridView
        {
            Bounds = new Rectangle(16, 66, 650, 255),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            RowHeadersVisible = false,
            AutoGenerateColumns = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = Color.FromArgb(20, 27, 45),
            GridColor = Color.FromArgb(35, 47, 79),
            BorderStyle = BorderStyle.None,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None
        };
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(11, 15, 26);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(148, 163, 184);
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold);
        grid.DefaultCellStyle.BackColor = Color.FromArgb(20, 27, 45);
        grid.DefaultCellStyle.ForeColor = Color.FromArgb(226, 232, 240);
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(9, 132, 227);
        grid.DefaultCellStyle.SelectionForeColor = Color.White;
        grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.2f);
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "#", DataPropertyName = nameof(PmStep.Index), FillWeight = 8 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Step", DataPropertyName = nameof(PmStep.StepName), FillWeight = 25 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Sensor", DataPropertyName = nameof(PmStep.Sensor), FillWeight = 16 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Condition", DataPropertyName = nameof(PmStep.Condition), FillWeight = 28 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", DataPropertyName = nameof(PmStep.DisplayStatus), FillWeight = 18 });
        statusCard.Controls.Add(grid);
        bottom.Controls.Add(statusCard, 0, 0);

        var logCard = Card("Run Log");
        logBox = new RichTextBox { Dock = DockStyle.Fill, Margin = new Padding(14, 40, 14, 14), ReadOnly = true, WordWrap = false, BackColor = Color.FromArgb(11, 15, 26), ForeColor = Color.FromArgb(226, 232, 240), Font = new Font("Consolas", 9.5f), BorderStyle = BorderStyle.FixedSingle };
        logCard.Controls.Add(logBox);
        bottom.Controls.Add(logCard, 1, 0);
    }

    private Panel Card(string title)
    {
        var card = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(20, 27, 45), Margin = Padding.Empty };
        card.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 3, BackColor = Color.FromArgb(9, 132, 227) });
        card.Controls.Add(new Label
        {
            Text = title,
            Dock = DockStyle.Top,
            Height = 38,
            Padding = new Padding(12, 10, 0, 0),
            Font = new Font("Segoe UI Semibold", 11.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(248, 250, 252)
        });
        return card;
    }

    private Label InfoRow(Control parent, string caption, int y)
    {
        parent.Controls.Add(new Label
        {
            Text = caption,
            Bounds = new Rectangle(14, y + 20, 130, 24),
            Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(148, 163, 184)
        });
        var value = new Label
        {
            Text = "-",
            Bounds = new Rectangle(150, y + 20, 190, 24),
            AutoEllipsis = true,
            Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold),
            ForeColor = Color.FromArgb(248, 250, 252)
        };
        parent.Controls.Add(value);
        return value;
    }

    private Button ActionButton(string text, Color back, Color fore, EventHandler click)
    {
        var button = new Button
        {
            Text = text,
            Dock = DockStyle.Fill,
            Margin = new Padding(6),
            BackColor = back,
            ForeColor = fore,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = GetHoverColor(back);
        button.FlatAppearance.MouseDownBackColor = GetPressColor(back);
        button.Click += click;
        return button;
    }

    private Button TopBarButton(string text, Color back, Color fore, EventHandler click)
    {
        var button = new Button
        {
            Text = text,
            BackColor = back,
            ForeColor = fore,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI Semibold", 9.2f, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Margin = Padding.Empty,
            Dock = DockStyle.None
        };

        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = GetHoverColor(back);
        button.FlatAppearance.MouseDownBackColor = GetPressColor(back);
        button.Click += click;
        return button;
    }

    private static Color GetHoverColor(Color baseColor)
    {
        if (baseColor == Color.Transparent) return Color.FromArgb(30, 255, 255, 255);
        int r = Math.Min(255, baseColor.R + (255 - baseColor.R) / 5);
        int g = Math.Min(255, baseColor.G + (255 - baseColor.G) / 5);
        int b = Math.Min(255, baseColor.B + (255 - baseColor.B) / 5);
        return Color.FromArgb(baseColor.A, r, g, b);
    }

    private static Color GetPressColor(Color baseColor)
    {
        if (baseColor == Color.Transparent) return Color.FromArgb(50, 255, 255, 255);
        int r = (int)(baseColor.R * 0.8);
        int g = (int)(baseColor.G * 0.8);
        int b = (int)(baseColor.B * 0.8);
        return Color.FromArgb(baseColor.A, r, g, b);
    }

    private void ProgressPanel_Paint(object? sender, PaintEventArgs e)
    {
        if (progressPanel.Width <= 0) return;
        float pct = 0f;
        if (steps.Count > 0)
        {
            if (currentIndex >= steps.Count || (currentIndex == -1 && steps.All(s => s.Status == StepStatus.Passed)))
            {
                pct = 1f;
            }
            else if (currentIndex >= 0)
            {
                pct = (float)currentIndex / steps.Count;
            }
        }
        int fillWidth = (int)(progressPanel.Width * pct);
        if (fillWidth > 0)
        {
            using var brush = new SolidBrush(Color.FromArgb(0, 184, 148));
            e.Graphics.FillRectangle(brush, 0, 0, fillWidth, progressPanel.Height);
        }
    }

    private void BindGrid()
    {
        grid.DataSource = null;
        grid.DataSource = steps;
    }

    private void StartSimulationTimer()
    {
        for (int i = 0; i < 180; i++) waveform.Enqueue(0.5f);
        uiTimer.Interval = 160;
        uiTimer.Tick += (_, _) =>
        {
            UpdateWaveformImage();
            UpdateVisionImage();
        };
        uiTimer.Start();
    }

    private void UpdateWaveformImage()
    {
        WindowItem? selected = cmbWindow.SelectedItem as WindowItem;
        if (selected is not null && selected.Handle != IntPtr.Zero)
        {
            Bitmap? captured = TryCaptureWindow(selected.Handle);
            if (captured is not null)
            {
                lblWaveSource.Text = "Live source: " + selected.Title;
                
                // --- OpenCV Overlay Logic ---
                using var src = BitmapConverter.ToMat(captured);
                // Dynamically update max values so the sliders perfectly map to the image size
                tbYMin.Maximum = src.Height > 0 ? src.Height : 1000;
                tbYMax.Maximum = src.Height > 0 ? src.Height : 1000;
                tbXMin.Maximum = src.Width > 0 ? src.Width : 1000;
                tbXMax.Maximum = src.Width > 0 ? src.Width : 1000;

                int yMin = tbYMin.Value;
                int yMax = tbYMax.Value;
                int xMin = tbXMin.Value;
                int xMax = tbXMax.Value;
                
                // Clamp coordinates to image bounds
                yMin = Math.Max(0, Math.Min(yMin, src.Height - 1));
                yMax = Math.Max(0, Math.Min(yMax, src.Height - 1));
                xMin = Math.Max(0, Math.Min(xMin, src.Width - 1));
                xMax = Math.Max(0, Math.Min(xMax, src.Width - 1));
                
                if (xMin < xMax && yMin < yMax) {
                    // Draw the green bounding box
                    Cv2.Rectangle(src, new OpenCvSharp.Point(xMin, yMin), new OpenCvSharp.Point(xMax, yMax), new Scalar(0, 255, 0), 2);
                    // Draw a thick red line at the top (Y-Min) to represent the strict 2m cutoff limit
                    Cv2.Line(src, new OpenCvSharp.Point(xMin, yMin), new OpenCvSharp.Point(xMax, yMin), new Scalar(0, 0, 255), 3);
                    
                    // Draw coordinates directly onto the image so they are always visible
                    if (chkShowCoords.Checked) {
                        Cv2.PutText(src, $"Top(2m): {yMin}", new OpenCvSharp.Point(xMin + 5, Math.Max(20, yMin - 10)), HersheyFonts.HersheySimplex, 0.6, new Scalar(0, 0, 255), 2);
                        Cv2.PutText(src, $"Bot: {yMax}", new OpenCvSharp.Point(xMin + 5, Math.Min(src.Height - 5, yMax + 20)), HersheyFonts.HersheySimplex, 0.6, new Scalar(0, 255, 0), 2);
                        Cv2.PutText(src, $"Left: {xMin}", new OpenCvSharp.Point(Math.Max(5, xMin - 85), (yMin + yMax) / 2), HersheyFonts.HersheySimplex, 0.6, new Scalar(0, 255, 0), 2);
                        Cv2.PutText(src, $"Right: {xMax}", new OpenCvSharp.Point(Math.Min(src.Width - 90, xMax + 10), (yMin + yMax) / 2), HersheyFonts.HersheySimplex, 0.6, new Scalar(0, 255, 0), 2);
                    }
                }
                
                Bitmap processedBmp = BitmapConverter.ToBitmap(src);
                SetPicture(waveformBox, processedBmp);
                
                return;
            }
        }

        latestReading = 0.52f + 0.22f * (float)Math.Sin(Environment.TickCount / 170.0) + (float)(rng.NextDouble() * 0.08 - 0.04);
        if (waitingAdjustment) latestReading = 0.25f + (float)rng.NextDouble() * 0.12f;
        latestReading = Math.Max(0.05f, Math.Min(0.95f, latestReading));
        if (waveform.Count > 220) waveform.Dequeue();
        waveform.Enqueue(latestReading);
        lblReading.Text = $"Reading: {latestReading:0.000}   Target range: 0.35 - 0.85";
        lblWaveSource.Text = "No live app selected. Placeholder waveform active.";
        SetPicture(waveformBox, DrawWaveformBitmap(Math.Max(320, waveformBox.Width), Math.Max(220, waveformBox.Height)));
    }

    private void UpdateVisionImage()
    {
        if (webcamEnabled && webcam is not null && webcam.IsOpened())
        {
            using var frame = new Mat();
            if (webcam.Read(frame) && !frame.Empty())
            {
                lblVision.Text = "Vision status: live PC camera";
                Bitmap live = BitmapConverter.ToBitmap(frame);
                SetPicture(visionBox, live);
                return;
            }
        }

        visionTargetX = Math.Max(40, Math.Min(visionTargetX + rng.Next(-8, 9), Math.Max(80, visionBox.Width - 130)));
        visionTargetY = Math.Max(40, Math.Min(visionTargetY + rng.Next(-7, 8), Math.Max(80, visionBox.Height - 110)));
        visionX += Math.Sign(visionTargetX - visionX) * Math.Min(8, Math.Abs(visionTargetX - visionX));
        visionY += Math.Sign(visionTargetY - visionY) * Math.Min(7, Math.Abs(visionTargetY - visionY));
        lblVision.Text = waitingAdjustment ? "Vision status: placeholder alignment warning" : "Vision status: placeholder target tracking";
        SetPicture(visionBox, DrawVisionBitmap(Math.Max(280, visionBox.Width), Math.Max(220, visionBox.Height)));
    }

    private Bitmap DrawWaveformBitmap(int width, int height)
    {
        var bmp = new Bitmap(width, height);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.FromArgb(20, 27, 45));

        Rectangle plot = new Rectangle(22, 24, Math.Max(80, width - 44), Math.Max(80, height - 48));
        using Pen gridPen = new(Color.FromArgb(35, 47, 79));
        using Pen axisPen = new(Color.FromArgb(51, 65, 85));
        using Pen wavePen = new(Color.FromArgb(9, 132, 227), 2.6f);
        using Brush fillBrush = new SolidBrush(Color.FromArgb(15, 9, 132, 227));
        using Brush textBrush = new SolidBrush(Color.FromArgb(148, 163, 184));
        using Font small = new("Segoe UI", 8.5f);

        g.FillRectangle(new SolidBrush(Color.FromArgb(11, 15, 26)), plot);

        for (int i = 0; i <= 4; i++) g.DrawLine(gridPen, plot.Left, plot.Top + i * plot.Height / 4, plot.Right, plot.Top + i * plot.Height / 4);
        for (int i = 0; i <= 6; i++) g.DrawLine(gridPen, plot.Left + i * plot.Width / 6, plot.Top, plot.Left + i * plot.Width / 6, plot.Bottom);
        g.DrawRectangle(axisPen, plot);
        g.DrawString("Simulated Hokuyo capture / serial parsing", small, textBrush, plot.Left, 4);

        PointF[] pts = waveform.Select((v, i) => new PointF(plot.Left + i * plot.Width / (float)Math.Max(1, waveform.Count - 1), plot.Bottom - v * plot.Height)).ToArray();
        if (pts.Length > 1)
        {
            using GraphicsPath area = new();
            area.AddPolygon(pts.Concat(new[] { new PointF(plot.Right, plot.Bottom), new PointF(plot.Left, plot.Bottom) }).ToArray());
            g.FillPath(fillBrush, area);
            g.DrawLines(wavePen, pts);
        }

        using Brush statusBrush = new SolidBrush(waitingAdjustment ? Color.FromArgb(255, 118, 117) : Color.FromArgb(0, 184, 148));
        g.DrawString(waitingAdjustment ? "OUT OF RANGE / ADJUSTMENT" : "NOMINAL SIMULATION", new Font("Segoe UI Semibold", 9f), statusBrush, plot.Left + 8, plot.Bottom - 26);
        return bmp;
    }

    private Bitmap DrawVisionBitmap(int width, int height)
    {
        var bmp = new Bitmap(width, height);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.FromArgb(20, 27, 45));

        Rectangle view = new Rectangle(16, 18, Math.Max(100, width - 32), Math.Max(100, height - 42));
        using Pen gridPen = new(Color.FromArgb(35, 47, 79));
        using Pen borderPen = new(Color.FromArgb(51, 65, 85));
        using Brush textBrush = new SolidBrush(Color.FromArgb(148, 163, 184));
        using Font small = new("Segoe UI", 8.5f);

        g.FillRectangle(new SolidBrush(Color.FromArgb(11, 15, 26)), view);

        g.DrawRectangle(borderPen, view);
        for (int i = 1; i < 4; i++) g.DrawLine(gridPen, view.Left, view.Top + i * view.Height / 4, view.Right, view.Top + i * view.Height / 4);
        for (int i = 1; i < 4; i++) g.DrawLine(gridPen, view.Left + i * view.Width / 4, view.Top, view.Left + i * view.Width / 4, view.Bottom);
        int cx = view.Left + view.Width / 2, cy = view.Top + view.Height / 2;
        using Pen crossPen = new(Color.FromArgb(9, 132, 227), 1.5f);
        g.DrawLine(crossPen, cx - 18, cy, cx + 18, cy);
        g.DrawLine(crossPen, cx, cy - 18, cx, cy + 18);
        Rectangle box = new Rectangle(visionX, visionY, 90, 70);
        using Pen boxPen = new(waitingAdjustment ? Color.FromArgb(255, 118, 117) : Color.FromArgb(0, 184, 148), 2f);
        g.DrawRectangle(boxPen, box);
        g.DrawString(waitingAdjustment ? "Alignment warning" : "Target tracked", small, textBrush, box.Left, Math.Max(0, box.Top - 18));
        g.DrawString("Live camera feed simulation", small, textBrush, view.Left, view.Bottom + 6);
        return bmp;
    }

    private Bitmap? TryCaptureWindow(IntPtr hWnd)
    {
        if (hWnd == IntPtr.Zero || IsIconic(hWnd)) return null;
        if (!GetWindowRect(hWnd, out RECT rect)) return null;

        int width = rect.Right - rect.Left;
        int height = rect.Bottom - rect.Top;

        if (width <= 10 || height <= 10) return null;

        var bmp = new Bitmap(width, height);

        try
        {
            using Graphics g = Graphics.FromImage(bmp);
            IntPtr hdc = g.GetHdc();

            try
            {
                // 0x00000002 = PW_RENDERFULLCONTENT
                bool success = PrintWindow(hWnd, hdc, 0x00000002);

                if (!success)
                {
                    g.ReleaseHdc(hdc);
                    bmp.Dispose();
                    return null;
                }
            }
            finally
            {
                try { g.ReleaseHdc(hdc); } catch { }
            }

            return bmp;
        }
        catch
        {
            bmp.Dispose();
            return null;
        }
    }


    private void BtnRefreshWindows_Click(object? sender, EventArgs e)
    {
        RefreshWindowList();
        Log("Refreshed visible window list for waveform source selection.");
    }

    private void BtnLaunchHokuyo_Click(object? sender, EventArgs e)
    {
        string appDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PBSv200 Application");
        string exePath = Path.Combine(appDir, "pbscfg.exe");
        
        if (!File.Exists(exePath))
        {
            appDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "PBSv200 Application");
            exePath = Path.Combine(appDir, "pbscfg.exe");
        }
        
        if (!File.Exists(exePath))
        {
            appDir = Path.Combine(Directory.GetCurrentDirectory(), "PBSv200 Application");
            exePath = Path.Combine(appDir, "pbscfg.exe");
        }

        if (!File.Exists(exePath))
        {
            MessageBox.Show("Could not find pbscfg.exe.\nExpected path:\n" + exePath, "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                WorkingDirectory = appDir,
                UseShellExecute = true
            };
            Process.Start(startInfo);
            Log("Launched Hokuyo PBS Application.");
            
            System.Windows.Forms.Timer autoSelectTimer = new() { Interval = 1500 };
            autoSelectTimer.Tick += (s, ev) =>
            {
                autoSelectTimer.Stop();
                autoSelectTimer.Dispose();
                
                RefreshWindowList();
                
                foreach (var item in cmbWindow.Items)
                {
                    if (item is WindowItem win && (win.Title.Contains("PBS", StringComparison.OrdinalIgnoreCase) || win.Title.Contains("pbscfg", StringComparison.OrdinalIgnoreCase)))
                    {
                        cmbWindow.SelectedItem = win;
                        Log("Automatically selected Hokuyo app window: " + win.Title);
                        break;
                    }
                }
            };
            autoSelectTimer.Start();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Failed to launch Hokuyo App:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RefreshWindowList()
    {
        captureWindows.Clear();
        EnumWindows((hWnd, lParam) =>
        {
            if (!IsWindowVisible(hWnd) || hWnd == Handle) return true;
            int len = GetWindowTextLength(hWnd);
            if (len <= 0) return true;
            var sb = new StringBuilder(len + 1);
            GetWindowText(hWnd, sb, sb.Capacity);
            string title = sb.ToString().Trim();

            if (string.IsNullOrWhiteSpace(title)) return true;

            // Hide system/background windows that are not useful for your sensor display
            if (title.Contains("Microsoft Text Input Application", StringComparison.OrdinalIgnoreCase)) return true;
            if (title.Contains("Program Manager", StringComparison.OrdinalIgnoreCase)) return true;
            if (title.Contains("Default IME", StringComparison.OrdinalIgnoreCase)) return true;
            if (title.Contains("OHT Mark 2 PM Workflow Simulator", StringComparison.OrdinalIgnoreCase)) return true;

            captureWindows.Add(new WindowItem(hWnd, title));
            return true;
        }, IntPtr.Zero);

        captureWindows.Sort((a, b) => string.Compare(a.Title, b.Title, StringComparison.OrdinalIgnoreCase));
        cmbWindow.BeginUpdate();
        cmbWindow.Items.Clear();
        cmbWindow.Items.Add(new WindowItem(IntPtr.Zero, "(No live window / use placeholder waveform)"));
        foreach (var item in captureWindows) cmbWindow.Items.Add(item);
        cmbWindow.SelectedIndex = 0;
        cmbWindow.EndUpdate();
    }

    private void BtnToggleCamera_Click(object? sender, EventArgs e)
    {
        if (webcamEnabled)
        {
            StopCamera();
            Log("PC camera stopped. Vision panel reverted to placeholder mode.");
            return;
        }

        try
        {
           webcam = null;

            for (int cameraIndex = 0; cameraIndex <= 4; cameraIndex++)
            {
                var testCam = new VideoCapture(cameraIndex);

                if (testCam.IsOpened())
                {
                    webcam = testCam;
                    Log("PC camera opened at index: " + cameraIndex);
                    break;
                }

                testCam.Dispose();
            }

            if (webcam is null || !webcam.IsOpened())
            {
                MessageBox.Show("Unable to open PC camera. Placeholder vision mode will remain active.");
                return;
            }
            webcam.Set(VideoCaptureProperties.FrameWidth, 1280);
            webcam.Set(VideoCaptureProperties.FrameHeight, 720);
            webcamEnabled = true;
            btnToggleCamera.Text = "Stop PC Camera";
            btnToggleCamera.BackColor = Color.FromArgb(255, 118, 117);
            btnToggleCamera.ForeColor = Color.White;
            Log("PC camera started successfully.");
        }
        catch (Exception ex)
        {
            StopCamera();
            MessageBox.Show("Unable to start PC camera.\n\n" + ex.Message);
        }
    }

    private void StopCamera()
    {
        webcamEnabled = false;
        if (webcam is not null)
        {
            if (webcam.IsOpened()) webcam.Release();
            webcam.Dispose();
            webcam = null;
        }
        btnToggleCamera.Text = "Start PC Camera";
        btnToggleCamera.BackColor = Color.FromArgb(9, 132, 227);
        btnToggleCamera.ForeColor = Color.White;
    }

    private void BtnStart_Click(object? sender, EventArgs e)
    {
        ResetUi(clearLog: true);
        runFolder = CreateRunFolder();
        currentIndex = 0;
        lblRunFolder.Text = runFolder;
        Log("New PM run folder created: " + runFolder);
        Log("PM run started.");
        LoadCurrentStep();
        SetButtons(false, true, true, false, true, true, true, true, true, true);
    }

    private void BtnOk_Click(object? sender, EventArgs e)
    {
        if (currentIndex < 0 || currentIndex >= steps.Count) return;
        if (waitingAdjustment)
        {
            MessageBox.Show("Complete adjustment first, then press Adjustment Done.");
            return;
        }

        PmStep step = steps[currentIndex];
        step.Status = StepStatus.Passed;
        step.Note = noteBox.Text.Trim();
        string file = SaveAppScreenshot(step, "PASS");
        Log("PASS: " + step.Sensor + " / " + step.Condition);
        Log("Evidence saved: " + Path.GetFileName(file));
        currentIndex++;
        noteBox.Clear();
        grid.Refresh();
        progressPanel.Invalidate();
        if (currentIndex >= steps.Count) CompleteRun(); else LoadCurrentStep();
    }

    private void BtnFail_Click(object? sender, EventArgs e)
    {
        if (currentIndex < 0 || currentIndex >= steps.Count) return;
        waitingAdjustment = true;
        steps[currentIndex].Status = StepStatus.AdjustmentRequired;
        steps[currentIndex].Note = noteBox.Text.Trim();
        lblStatus.Text = "Adjustment Required";
        lblStatus.ForeColor = Color.FromArgb(255, 118, 117);
        lblGuidance.Text = "Perform adjustment, add note if needed, then press Adjustment Done.";
        Log("Adjustment required: " + steps[currentIndex].Sensor + " / " + steps[currentIndex].Condition);
        grid.Refresh();
        SetButtons(false, false, false, true, true, true, true, true, true, true);
    }

    private void BtnAdjusted_Click(object? sender, EventArgs e)
    {
        if (currentIndex < 0 || currentIndex >= steps.Count) return;
        waitingAdjustment = false;
        retestCount++;
        steps[currentIndex].Status = StepStatus.RetestPending;
        steps[currentIndex].Note = noteBox.Text.Trim();
        lblStatus.Text = "Retest Pending";
        lblStatus.ForeColor = Color.FromArgb(9, 132, 227);
        lblGuidance.Text = "Adjustment completed. Verify updated condition, then press OK / Next.";
        Log("Retest pending after adjustment. Retest count: " + retestCount);
        grid.Refresh();
        SetButtons(false, true, true, false, true, true, true, true, true, true);
    }

    private void BtnCaptureApp_Click(object? sender, EventArgs e)
    {
        if (!EnsureRunFolder()) return;
        string file = SaveAppScreenshot(CurrentStepOrPlaceholder(), "MANUAL_APP_CAPTURE");
        Log("App screenshot saved: " + Path.GetFileName(file));
        OpenPath(file);
    }

    private void BtnCaptureScreen_Click(object? sender, EventArgs e)
    {
        if (!EnsureRunFolder()) return;
        string file = SaveFullScreenScreenshot(CurrentStepOrPlaceholder(), "MANUAL_SCREEN_CAPTURE");
        Log("Full screen screenshot saved: " + Path.GetFileName(file));
        OpenPath(file);
    }

    private void BtnOpenFolder_Click(object? sender, EventArgs e) => OpenRunFolder();

    private void BtnSetPath_Click(object? sender, EventArgs e)
    {
        using FolderBrowserDialog dialog = new() { Description = "Select PM run root folder", SelectedPath = rootFolder };
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            rootFolder = dialog.SelectedPath;
            lblSaveRoot.Text = rootFolder;
            Log("Save root folder changed to: " + rootFolder);
        }
    }

    private void BtnReset_Click(object? sender, EventArgs e)
    {
        if (MessageBox.Show("Reset simulator? Saved files remain.", "Reset", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            ResetUi(clearLog: true);
    }

    private void BtnComplete_Click(object? sender, EventArgs e)
    {
        if (!EnsureRunFolder()) return;
        CompleteRun();
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
            Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
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

    // Win32 window enumeration / capture
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    
    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}

public sealed class WindowItem
{
    public IntPtr Handle { get; }
    public string Title { get; }

    public WindowItem(IntPtr handle, string title)
    {
        Handle = handle;
        Title = title;
    }

    public override string ToString() => Title;
}

public sealed class PmStep : INotifyPropertyChanged
{
    private StepStatus status = StepStatus.Pending;
    public int Index { get; set; }
    public string StepName { get; set; }
    public string Sensor { get; set; }
    public string Condition { get; set; }
    public string Guidance { get; set; }
    public string Note { get; set; } = "";

    public StepStatus Status
    {
        get => status;
        set
        {
            status = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayStatus)));
        }
    }

    public string DisplayStatus => Status switch
    {
        StepStatus.Pending => "Pending",
        StepStatus.InProgress => "In Progress",
        StepStatus.Passed => "Passed",
        StepStatus.AdjustmentRequired => "Adjustment Required",
        StepStatus.RetestPending => "Retest Pending",
        _ => "Pending"
    };

    public PmStep(string stepName, string sensor, string condition, string guidance)
    {
        StepName = stepName;
        Sensor = sensor;
        Condition = condition;
        Guidance = guidance;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}

public enum StepStatus
{
    Pending,
    InProgress,
    Passed,
    AdjustmentRequired,
    RetestPending
}
