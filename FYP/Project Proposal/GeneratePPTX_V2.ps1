$msoTrue = -1
$msoFalse = 0

$ppt = New-Object -ComObject PowerPoint.Application
$pres = $ppt.Presentations.Add($msoTrue)
$pres.ApplyTheme("C:\Users\yiron\Desktop\FYProject\Context of GF Internship\Submissions\YiRong_U2320967K_PA_Slides.pptx")

function Format-Text($shape, $size=24) {
    if ($shape.HasTextFrame) {
        $shape.TextFrame.TextRange.Font.Size = $size
        $shape.TextFrame.TextRange.ParagraphFormat.SpaceWithin = 1.2
    }
}

# Slide 1 (Title)
$slide1 = $pres.Slides.Add(1, 1)
$slide1.Shapes.Title.TextFrame.TextRange.Text = "Robot Arm Project Proposal"
$slide1.Shapes.Item(2).TextFrame.TextRange.Text = "OHT Mark 2 PM Workflow Simulator`nPresenter: Tan Yi Rong`nDate: July 2026"

# Slide 2 (Text)
$slide2 = $pres.Slides.Add(2, 2)
$slide2.Shapes.Title.TextFrame.TextRange.Text = "Project Objective"
$slide2.Shapes.Item(2).TextFrame.TextRange.Text = "• Current State: The PM sensor connection is manual, labour-intensive, and prone to human error.`n`n• Proposed Solution: Integrate a Collaborative Robot (Cobot) arm to automatically handle the sensor plug connection to the OHT.`n`n• Goal: Reduce operational downtime, eliminate manual alignment errors, and enable a fully automated, vision-assisted PM workflow."
Format-Text $slide2.Shapes.Item(2) 24

# Slide 3 (Table - Vendor Comparison)
$slide3 = $pres.Slides.Add(3, 2)
$slide3.Shapes.Title.TextFrame.TextRange.Text = "Vendor Comparison"
$slide3.Shapes.Item(2).Delete() # Delete the default text box to make room for table

$tableShape3 = $slide3.Shapes.AddTable(6, 4, 50, 120, 850, 300)
$tbl3 = $tableShape3.Table
$data3 = @(
    @("Criteria", "OMRON (TM5S)", "Universal Robots (UR7e)", "ABB (GoFa / POWA)"),
    @("Price", "SGD 100k - 150k", "~SGD 29k - 36k", "SGD 40k - 70k / <30k"),
    @("Reach", "900 mm", "~730mm - 1300mm", "Up to 950mm"),
    @("Speed", "1.4 m/s", "Up to 1.0 m/s", "Up to 2.2 m/s"),
    @("Safety", "Advanced collision", "17 configurable functions", "Advanced torque sensors"),
    @("Vision", "Built-in / Integrated", "Without camera", "Requires specific add-ons")
)
for ($r = 1; $r -le 6; $r++) {
    for ($c = 1; $c -le 4; $c++) {
        $tbl3.Cell($r, $c).Shape.TextFrame.TextRange.Text = $data3[$r-1][$c-1]
        $tbl3.Cell($r, $c).Shape.TextFrame.TextRange.Font.Size = 18
        if ($r -eq 1) { $tbl3.Cell($r, $c).Shape.TextFrame.TextRange.Font.Bold = $msoTrue }
    }
}

# Slide 4
$slide4 = $pres.Slides.Add(4, 2)
$slide4.Shapes.Title.TextFrame.TextRange.Text = "The Core Challenge"
$slide4.Shapes.Item(2).TextFrame.TextRange.Text = "• The Problem: The primary driver of the high vendor quotations is the requirement for a highly precise Robot Vision System.`n`n• Why Vision is Needed: The current OHT sensor uses a complex 4-port plug. A robot arm without a camera cannot accurately align and insert a 4-pin connector just by 'feel'.`n`n• The Crossroads: Do we heavily increase the project budget for vision-assisted robots, or do we redesign the hardware to eliminate the need for vision entirely?"
Format-Text $slide4.Shapes.Item(2) 24

# Slide 5
$slide5 = $pres.Slides.Add(5, 2)
$slide5.Shapes.Title.TextFrame.TextRange.Text = "Hardware Redesign: Magnetic Connector"
$slide5.Shapes.Item(2).TextFrame.TextRange.Text = "• The Concept: Replace the complex 4-port plug with a centralized single magnetic connector.`n`n• Eliminating Vision: Magnetic connectors auto-align themselves via magnetic pull. A low-cost, DIY robot arm only needs to bring the plug close to the port, and magnets will snap it into perfect alignment automatically.`n`n• Cost Efficiency: This drastically drops the robotics budget, allowing us to build an in-house DIY arm."
Format-Text $slide5.Shapes.Item(2) 24

# Slide 6 (Table - Cost Analysis)
$slide6 = $pres.Slides.Add(6, 2)
$slide6.Shapes.Title.TextFrame.TextRange.Text = "Fleet Retrofit Cost Analysis (250 Vehicles)"
$slide6.Shapes.Item(2).Delete()

$tableShape6 = $slide6.Shapes.AddTable(7, 3, 50, 120, 850, 350)
$tbl6 = $tableShape6.Table
$data6 = @(
    @("Component / Requirement", "Est. Unit Cost", "Total Cost"),
    @("Magnetic 15-Pin Plug (Premium Rosenberger)", "SGD 70", "SGD 17,500"),
    @("Magnetic 15-Pin Plug (Cheaper OEM Alternative)", "~SGD 15", "~SGD 3,750"),
    @("Custom PCB Breakout & Relays", "~SGD 8", "~SGD 2,000"),
    @("Mounting Brackets & Labor", "~SGD 95", "~SGD 23,750"),
    @("GRAND TOTAL (Premium Plug)", "-", "~SGD 42,550"),
    @("GRAND TOTAL (Cheaper OEM Plug)", "-", "~SGD 28,800")
)
for ($r = 1; $r -le 7; $r++) {
    for ($c = 1; $c -le 3; $c++) {
        $tbl6.Cell($r, $c).Shape.TextFrame.TextRange.Text = $data6[$r-1][$c-1]
        $tbl6.Cell($r, $c).Shape.TextFrame.TextRange.Font.Size = 18
        if ($r -eq 1 -or $r -ge 6) { $tbl6.Cell($r, $c).Shape.TextFrame.TextRange.Font.Bold = $msoTrue }
    }
}

# Slide 7 (Table - Sourcing)
$slide7 = $pres.Slides.Add(7, 2)
$slide7.Shapes.Title.TextFrame.TextRange.Text = "Magnetic Sourcing & PCB Implementation"
$slide7.Shapes.Item(2).Delete()

$tableShape7 = $slide7.Shapes.AddTable(4, 3, 50, 120, 850, 200)
$tbl7 = $tableShape7.Table
$data7 = @(
    @("Manufacturer / Brand", "Connector Quality", "Est. Unit Price"),
    @("Rosenberger (Germany)", "Premium / Aerospace Grade", "~SGD 70.00"),
    @("C.C.P. Contact Probes (Taiwan)", "High Industrial Grade", "~SGD 25.00"),
    @("HytePro / Top-Link (China)", "Standard OEM Pogo-Pin", "~SGD 15.00")
)
for ($r = 1; $r -le 4; $r++) {
    for ($c = 1; $c -le 3; $c++) {
        $tbl7.Cell($r, $c).Shape.TextFrame.TextRange.Text = $data7[$r-1][$c-1]
        $tbl7.Cell($r, $c).Shape.TextFrame.TextRange.Font.Size = 18
        if ($r -eq 1) { $tbl7.Cell($r, $c).Shape.TextFrame.TextRange.Font.Bold = $msoTrue }
    }
}

# Add text box under table for Slide 7
$tb7 = $slide7.Shapes.AddTextbox(1, 50, 350, 850, 100) # msoTextOrientationHorizontal = 1
$tb7.TextFrame.TextRange.Text = "• The Bridge Board: We will design a tiny, custom PCB (SGD 3 each) to seamlessly map the magnetic pins to our standard wires."
$tb7.TextFrame.TextRange.Font.Size = 22

# Slide 8
$slide8 = $pres.Slides.Add(8, 2)
$slide8.Shapes.Title.TextFrame.TextRange.Text = "Next Steps & Conclusion"
$slide8.Shapes.Item(2).TextFrame.TextRange.Text = "• Finalize the budget review and formally reject the current OMRON / UR / ABB vendor quotations.`n`n• Draft the electrical schematic for the proposed central testing station relay circuit.`n`n• Procure sample magnetic connectors from OEM manufacturers to test alignment strength.`n`nConclusion: By intelligently redesigning the physical connector and centralizing the relay logic, we bypass expensive robot vision requirements and achieve massive cost savings for the automation project."
Format-Text $slide8.Shapes.Item(2) 22

$pres.SaveAs("C:\Users\yiron\Desktop\FYProject\FYP\Project Proposal\Robot_Arm_Proposal_Slides_V2.pptx")
$pres.Close()

# --- PROTOTYPE PRESENTATION ---
$pres2 = $ppt.Presentations.Add($msoTrue)
$pres2.ApplyTheme("C:\Users\yiron\Desktop\FYProject\Context of GF Internship\Submissions\YiRong_U2320967K_PA_Slides.pptx")

$s1 = $pres2.Slides.Add(1, 1)
$s1.Shapes.Title.TextFrame.TextRange.Text = "Automated Sensor Testing Prototype"
$s1.Shapes.Item(2).TextFrame.TextRange.Text = "Implementation Plan & Hardware Setup`nPresenter: Tan Yi Rong"

$s2 = $pres2.Slides.Add(2, 2)
$s2.Shapes.Title.TextFrame.TextRange.Text = "Prototype Objective"
$s2.Shapes.Item(2).TextFrame.TextRange.Text = "• The Goal: Build a small-scale, fully functional physical prototype that perfectly mimics the Overhead Hoist Transport (OHT) sensor environment.`n`n• Why We Need It: Before retrofitting 250 vehicles, we must validate the new magnetic connector logic and prove our centralized testing station concept works flawlessly.`n`n• The Scope: Automate the physical 'Detect / No-Detect' environment to eliminate the need for an engineer to manually stand in front of the sensors."
Format-Text $s2.Shapes.Item(2) 22

$s3 = $pres2.Slides.Add(3, 2)
$s3.Shapes.Title.TextFrame.TextRange.Text = "Structural Architecture"
$s3.Shapes.Item(2).TextFrame.TextRange.Text = "• The Base Structure: A rigid structure built using standard T-Slot Extruded Aluminum (e.g., 2020 or 3030 series) for maximum modularity and ease of adjustment.`n`n• Sensor Mounting: All 4 OHT sensors will securely mount to the top beam of the aluminum frame, replicating their exact positions on a Mark 2 OHT chassis.`n`n• Calibration: The frame allows slight Z-axis and X-axis adjustments to fine-tune the sensor angles before locking them down."
Format-Text $s3.Shapes.Item(2) 22

$s4 = $pres2.Slides.Add(4, 2)
$s4.Shapes.Title.TextFrame.TextRange.Text = "Automated Obstacle Simulation"
$s4.Shapes.Item(2).TextFrame.TextRange.Text = "• The Physical Concept: Instead of an engineer manually blocking the sensors, a large sheet of acrylic serves as the artificial 'obstacle.'`n`n• Distance Accuracy: The rig is calibrated to mimic an obstacle exactly 1.5 meters away from the mounted sensors.`n`n• Automated Movement: The acrylic sheet is programmed to slide Left/Right and Up/Down, triggering the sensors sequentially to generate live data."
Format-Text $s4.Shapes.Item(2) 22

$s5 = $pres2.Slides.Add(5, 2)
$s5.Shapes.Title.TextFrame.TextRange.Text = "Required Hardware & Motors"
$s5.Shapes.Item(2).TextFrame.TextRange.Text = "• Linear Motion: 2x Linear Guide Rails (with carriage blocks) ensure the acrylic sheet slides smoothly without vibration.`n`n• Actuation (Motors): 2x NEMA 17 Stepper Motors (X-axis, Y-axis) provide precise, programmable control over the acrylic sheet.`n`n• Drive System: GT2 Timing Belts and Pulleys connect to the stepper motors.`n`n• The Target: 1x Matte or Tinted Acrylic Sheet (blocks sensors without reflecting stray light)."
Format-Text $s5.Shapes.Item(2) 22

$s6 = $pres2.Slides.Add(6, 2)
$s6.Shapes.Title.TextFrame.TextRange.Text = "Electrical & Wiring Architecture"
$s6.Shapes.Item(2).TextFrame.TextRange.Text = "• Motor Control: An Arduino Uno paired with a CNC Shield and A4988 Stepper Drivers executes the programmed movement.`n`n• Sensor Wiring: The 4 sensors on the frame wire down into the proposed Centralized Relay Station.`n`n• Magnetic Plug Test: The relay station routes through our prototype PCB Bridge Board, connecting to a sample OEM Magnetic 15-Pin Plug."
Format-Text $s6.Shapes.Item(2) 22

$s7 = $pres2.Slides.Add(7, 2)
$s7.Shapes.Title.TextFrame.TextRange.Text = "Budget Justification"
$s7.Shapes.Item(2).TextFrame.TextRange.Text = "• Items to Purchase: T-Slot Aluminum Extrusions, 2x NEMA 17 Motors + Rails, Arduino Kit, 4-Channel Relay Module, OEM Plug Samples, Acrylic Sheet.`n`n• Justification: Procuring these low-cost Maker components (est. <SGD 300) validates the centralized relay logic and magnetic plug alignment.`n`n• ROI: This minimal upfront cost unlocks our SGD ~42k fleet retrofit strategy, definitively proving a >SGD 100k cobot is entirely unnecessary."
Format-Text $s7.Shapes.Item(2) 22

$s8 = $pres2.Slides.Add(8, 2)
$s8.Shapes.Title.TextFrame.TextRange.Text = "Full System Integration (The Loop)"
$s8.Shapes.Item(2).TextFrame.TextRange.Text = "• The DIY Coordinate Robot: I will construct a low-cost Cartesian robot arm. Because the magnetic plug is self-aligning, this DIY arm only needs 'blind' coordinate movement to plug in and out - no camera vision required!`n`n• The Seamless Loop:`n  1. The DIY arm snaps the magnetic plug into the testing port.`n  2. C# PM Simulator commands the acrylic sheet to simulate an obstacle.`n  3. Simulator successfully detects the obstacle; Central Relay instantly switches power to the next sensor.`n  4. Acrylic sheet shifts to the new sensor, entirely automating the manual PM test!"
Format-Text $s8.Shapes.Item(2) 20

$pres2.SaveAs("C:\Users\yiron\Desktop\FYProject\FYP\Project Proposal\Prototype_Implementation_Slides_V2.pptx")
$pres2.Close()
$ppt.Quit()
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($ppt) | Out-Null
