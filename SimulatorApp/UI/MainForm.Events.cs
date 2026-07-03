using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using OpenCvSharp;

namespace OHTPmSimulatorV5;

public partial class MainForm
{
    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        StopCamera();
        DisposePicture(waveformBox);
        DisposePicture(visionBox);
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
            appDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "PBSv200 Application");
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
}
