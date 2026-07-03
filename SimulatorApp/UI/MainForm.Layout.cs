using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace OHTPmSimulatorV5;

public partial class MainForm
{
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
            Dock = DockStyle.None,
            Anchor = AnchorStyles.Top | AnchorStyles.Left,
            Padding = new Padding(12),
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.FromArgb(11, 15, 26),
            Size = new System.Drawing.Size(Math.Max(1300, ClientSize.Width), Math.Max(820, ClientSize.Height))
        };
        
        this.Resize += (s, e) => {
            if (root != null) {
                root.Width = Math.Max(1300, ClientSize.Width);
                root.Height = Math.Max(820, ClientSize.Height);
            }
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
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 0));
        right.Controls.Add(top, 0, 1);

        var wfCard = Card("Sensor Waveform Reading");

        Panel topCtrlPanel = new Panel { Dock = DockStyle.Top, Height = 96, BackColor = Color.Transparent };
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
        
        chkAutoCapture = new CheckBox { Text = "Auto Capture PM", ForeColor = Color.White, Bounds = new Rectangle(16, 64, 140, 24) };
        topCtrlPanel.Controls.Add(chkAutoCapture);
        
        chkShowCoords = new CheckBox { Text = "Show Coords", ForeColor = Color.FromArgb(0, 184, 148), Checked = true, Bounds = new Rectangle(166, 64, 110, 24) };
        topCtrlPanel.Controls.Add(chkShowCoords);
        
        Button btnToggleVision = TopBarButton("Toggle Vision Pane", Color.FromArgb(45, 52, 54), Color.White, (s, e) => {
            if (top.ColumnStyles[1].Width > 0) {
                top.ColumnStyles[1].SizeType = SizeType.Absolute;
                top.ColumnStyles[1].Width = 0;
                top.ColumnStyles[0].SizeType = SizeType.Percent;
                top.ColumnStyles[0].Width = 100;
            } else {
                top.ColumnStyles[1].SizeType = SizeType.Percent;
                top.ColumnStyles[1].Width = 44;
                top.ColumnStyles[0].SizeType = SizeType.Percent;
                top.ColumnStyles[0].Width = 56;
            }
        });
        btnToggleVision.SetBounds(286, 62, 140, 28);
        topCtrlPanel.Controls.Add(btnToggleVision);

        Panel graphContainer = new Panel 
        { 
            Dock = DockStyle.Fill,
            Padding = new Padding(14),
            BackColor = Color.FromArgb(20, 27, 45) 
        };
        wfCard.Controls.Add(graphContainer);
        graphContainer.BringToFront();

        tbYMin = new TrackBar { Orientation = Orientation.Vertical, Dock = DockStyle.Left, Width = 30, Minimum = 0, Maximum = 2000, Value = 482, TickStyle = TickStyle.None };
        tbYMax = new TrackBar { Orientation = Orientation.Vertical, Dock = DockStyle.Right, Width = 30, Minimum = 0, Maximum = 2000, Value = 685, TickStyle = TickStyle.None };
        tbXMin = new TrackBar { Orientation = Orientation.Horizontal, Dock = DockStyle.Top, Height = 30, Minimum = 0, Maximum = 2000, Value = 922, TickStyle = TickStyle.None };
        tbXMax = new TrackBar { Orientation = Orientation.Horizontal, Dock = DockStyle.Bottom, Height = 30, Minimum = 0, Maximum = 2000, Value = 1004, TickStyle = TickStyle.None };

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
        
        Panel topStatusPanel = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.Transparent };
        statusCard.Controls.Add(topStatusPanel);
        topStatusPanel.BringToFront();

        progressPanel = new Panel { Bounds = new Rectangle(16, 42, 650, 10), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, BackColor = Color.FromArgb(11, 15, 26) };
        progressPanel.Paint += ProgressPanel_Paint;
        topStatusPanel.Controls.Add(progressPanel);

        Panel gridContainer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 6, 16, 16), BackColor = Color.Transparent };
        statusCard.Controls.Add(gridContainer);
        gridContainer.BringToFront();

        grid = new DataGridView
        {
            Dock = DockStyle.Fill,
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
        
        gridContainer.Controls.Add(grid);
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
}
