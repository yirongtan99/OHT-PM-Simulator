using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace OHTPmSimulatorV5;

public partial class MainForm
{
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
                    // Evaluate AI before modifying the image!
                    EvaluateAutoCapture(src, xMin, yMin, xMax, yMax);

                    // Draw the green bounding box
                    Cv2.Rectangle(src, new OpenCvSharp.Point(xMin, yMin), new OpenCvSharp.Point(xMax, yMax), new Scalar(0, 255, 0), 2);
                    // Draw a thick red line at the top (Y-Min) to represent the strict 2m cutoff limit
                    Cv2.Line(src, new OpenCvSharp.Point(xMin, yMin), new OpenCvSharp.Point(xMax, yMin), new Scalar(0, 0, 255), 3);
                    
                    // Draw coordinates directly onto the image so they are always visible
                    if (chkShowCoords.Checked) {
                        Cv2.PutText(src, $"Top(2m): {yMin}", new OpenCvSharp.Point(xMin + 5, Math.Max(20, yMin - 10)), HersheyFonts.HersheySimplex, 0.6, new Scalar(0, 0, 255), 2);
                        Cv2.PutText(src, $"Bot: {yMax}", new OpenCvSharp.Point(xMin + 5, Math.Min(src.Height - 5, yMax + 20)), HersheyFonts.HersheySimplex, 0.6, new Scalar(0, 255, 0), 2);
                        Cv2.PutText(src, $"Left: {xMin}", new OpenCvSharp.Point(Math.Max(5, xMin - 130), (yMin + yMax) / 2), HersheyFonts.HersheySimplex, 0.6, new Scalar(0, 255, 0), 2);
                        Cv2.PutText(src, $"Right: {xMax}", new OpenCvSharp.Point(Math.Min(src.Width - 110, xMax + 10), (yMin + yMax) / 2), HersheyFonts.HersheySimplex, 0.6, new Scalar(0, 255, 0), 2);
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

    private void EvaluateAutoCapture(Mat originalFrame, int xMin, int yMin, int xMax, int yMax)
    {
        if (!chkAutoCapture.Checked || onnxSession is null)
        {
            aiVotingQueue.Clear();
            return;
        }

        try
        {
            int width = xMax - xMin;
            int height = yMax - yMin;
            if (width <= 0 || height <= 0) return;
            
            using Mat crop = new Mat(originalFrame, new Rect(xMin, yMin, width, height));
            using Mat resized = new Mat();
            Cv2.Resize(crop, resized, new OpenCvSharp.Size(224, 224));
            
            using Mat rgb = new Mat();
            Cv2.CvtColor(resized, rgb, ColorConversionCodes.BGR2RGB);
            
            float[] tensorData = new float[224 * 224 * 3];
            int index = 0;
            
            rgb.GetArray<Vec3b>(out Vec3b[] pixels);
            for (int i = 0; i < pixels.Length; i++)
            {
                tensorData[index++] = (pixels[i].Item0 / 127.5f) - 1.0f; // R
                tensorData[index++] = (pixels[i].Item1 / 127.5f) - 1.0f; // G
                tensorData[index++] = (pixels[i].Item2 / 127.5f) - 1.0f; // B
            }
            
            var tensor = new DenseTensor<float>(tensorData, new[] { 1, 224, 224, 3 });
            var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("input", tensor) };
            
            using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = onnxSession.Run(inputs);
            float confidence = results.First().AsEnumerable<float>().First();
            
            bool isPositive = confidence > 0.5f;
            
            // Draw the AI Confidence directly onto the live feed so the user can see it!
            Scalar aiColor = isPositive ? new Scalar(0, 255, 0) : new Scalar(0, 0, 255);
            Cv2.PutText(originalFrame, $"AI Confidence: {confidence:P1}", new OpenCvSharp.Point(10, 30), HersheyFonts.HersheySimplex, 0.8, aiColor, 2);
            
            if (aiVotingQueue.Count >= 10) aiVotingQueue.Dequeue();
            aiVotingQueue.Enqueue(isPositive);
            
            if (aiVotingQueue.Count == 10)
            {
                int positiveCount = aiVotingQueue.Count(v => v);
                if (positiveCount >= 7)
                {
                    Log($"AI Auto-Capture Triggered! Confidence: {confidence:P1} (7/10 frames matched)");
                    chkAutoCapture.Checked = false;
                    aiVotingQueue.Clear();
                    
                    if (this.IsHandleCreated)
                    {
                        this.BeginInvoke(new Action(() => BtnOk_Click(this, EventArgs.Empty)));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log($"AI Evaluation Error: {ex.Message}");
            chkAutoCapture.Checked = false;
        }
    }

    private Bitmap? TryCaptureWindow(IntPtr hWnd)
    {
        if (hWnd == IntPtr.Zero || NativeMethods.IsIconic(hWnd)) return null;
        if (!NativeMethods.GetWindowRect(hWnd, out NativeMethods.RECT rect)) return null;

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
                bool success = NativeMethods.PrintWindow(hWnd, hdc, 0x00000002);

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

    private void RefreshWindowList()
    {
        captureWindows.Clear();
        NativeMethods.EnumWindows((hWnd, lParam) =>
        {
            if (!NativeMethods.IsWindowVisible(hWnd) || hWnd == Handle) return true;
            int len = NativeMethods.GetWindowTextLength(hWnd);
            if (len <= 0) return true;
            var sb = new StringBuilder(len + 1);
            NativeMethods.GetWindowText(hWnd, sb, sb.Capacity);
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
}
