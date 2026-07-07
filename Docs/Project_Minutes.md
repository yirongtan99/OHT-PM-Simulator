# OHT Mark 2 PM Workflow Simulator - Project Minutes

## Project Scope
The goal of this project is to develop a comprehensive **OHT Mark 2 PM Workflow Simulator** using C# and WinForms. The simulator replicates the real-world Hokuyo sensor application environment, allowing for simulated Preventive Maintenance (PM) runs. 

A major secondary objective is the **AI Integration**: training a neural network (MobileNetV2) to actively monitor the sensor's live feed and perform automated captures when obstacles are consistently detected within a specific Region of Interest (ROI) boundary.

---

## Meeting Minutes & Development Log

### **July 1, 2026**
- **15:30 - Simulator Architecture Refactoring**
  - Restructured the monolithic `Program_V5.cs` into a clean, modular folder hierarchy (`Models`, `UI`, `Utils`).
  - Separated the `MainForm` partial class into logic, layout, events, and vision modules for easier maintainability.
- **16:00 - UI & Vision Setup**
  - Configured the Vision Pane to capture live window feeds using advanced Win32 APIs (`TryCaptureWindow`).

### **July 2, 2026**
- **10:00 - Dataset Optimization**
  - Analyzed the `PM Sensor Reading` dataset containing VHL and OBS sensor screenshots.
  - Reorganized miscategorized files from the `MISC` folder into their appropriate Positive/Negative categories.
  - Automated a batch renaming script to prefix images with `Positive_` and `Negative_` to prepare them for Machine Learning.
- **15:00 - Documentation**
  - Generated internal code documentation and project analysis files.

### **July 3, 2026**
- **14:00 - AI Model Training (Google Colab)**
  - Developed `train_model.py` utilizing Transfer Learning on MobileNetV2.
  - Zipped and uploaded the dataset (`FYP_AI_Dataset.zip`) to Colab for cloud GPU training.
  - Successfully trained the binary classification model (Obstacle Detected vs Clear) achieving >85% validation accuracy.
  - Exported the trained Keras model to `oht_sensor_model.onnx` for native C# integration.
- **15:45 - AI Integration into C# Simulator**
  - Downloaded the `.onnx` brain and placed it in the `AI_Model` directory.
  - Installed `Microsoft.ML.OnnxRuntime` via NuGet.
  - Hooked the ONNX Inference Session directly into the simulator's boot sequence.
- **15:52 - Auto-Capture & 10-Frame Voting Logic**
  - Wrote the computer vision pipeline to crop the live feed based on UI sliders, convert it to RGB, normalize the tensor `(PixelValue / 127.5) - 1.0`, and run inference.
  - Implemented the voting algorithm: If the neural network confidently detects an obstacle (`>50%` confidence) for **7 out of the last 10 frames**, it automatically clicks "Pass" and captures the screenshot.
- **16:03 - Real-Time AI Visualizer**
  - Upgraded the Vision Pane to physically draw the AI's real-time confidence percentage (`AI Confidence: XX.X%`) directly onto the live camera feed for easy debugging.
  - Pushed all dataset and code updates to the main GitHub repository.
- **16:40 - Simulator Testing**
  - Confirmed that UWP apps like Windows "Photos" block window capture, and established that testing static images requires classic desktop apps like **Microsoft Paint**.

### **July 7, 2026**
- **08:24 - Project Management**
  - Generated this dynamic Project Minutes document to track ongoing progress, scope, and chronological milestones.

---

*Note: This document will be continuously updated as the project progresses.*
