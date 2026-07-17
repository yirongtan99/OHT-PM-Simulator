$msoTrue = -1
$msoFalse = 0

$ppt = New-Object -ComObject PowerPoint.Application

$pres = $ppt.Presentations.Add($msoTrue)
$pres.ApplyTheme("C:\Users\yiron\Desktop\FYProject\Context of GF Internship\Submissions\YiRong_U2320967K_PA_Slides.pptx")

$slide1 = $pres.Slides.Add(1, 1)
$slide1.Shapes.Title.TextFrame.TextRange.Text = "Robot Arm Project Proposal"
$slide1.Shapes.Item(2).TextFrame.TextRange.Text = "OHT Mark 2 PM Workflow Simulator`nPresenter: Tan Yi Rong`nDate: July 2026"

$slide2 = $pres.Slides.Add(2, 2)
$slide2.Shapes.Title.TextFrame.TextRange.Text = "Project Objective"
$slide2.Shapes.Item(2).TextFrame.TextRange.Text = "Current State: The PM sensor connection is manual, labour-intensive, and prone to human error.`n`nProposed Solution: Integrate a Collaborative Robot (Cobot) arm to automatically handle the sensor plug connection to the OHT.`n`nGoal: Reduce operational downtime, eliminate manual alignment errors, and enable a fully automated, vision-assisted PM workflow."

$slide3 = $pres.Slides.Add(3, 2)
$slide3.Shapes.Title.TextFrame.TextRange.Text = "Vendor Comparison"
$slide3.Shapes.Item(2).TextFrame.TextRange.Text = "OMRON (TM5S Series): Built-in vision, 900mm reach, SGD 100k - 150k.`nUniversal Robots (UR7e): No camera, ~730mm reach, ~SGD 29k - 36k.`nABB (GoFa): Advanced torque sensors, up to 950mm reach, SGD 40k - 70k.`n`nTakeaway: The extreme budget constraints require us to explore internal engineering alternatives instead of procuring premium hardware."

$slide4 = $pres.Slides.Add(4, 2)
$slide4.Shapes.Title.TextFrame.TextRange.Text = "The Core Challenge"
$slide4.Shapes.Item(2).TextFrame.TextRange.Text = "The Problem: The primary driver of the high vendor quotations is the requirement for a highly precise Robot Vision System.`nWhy Vision is Needed: The current OHT sensor uses a complex 4-port plug. A robot arm without a camera cannot accurately align and insert a 4-pin connector just by 'feel'.`nThe Crossroads: Do we heavily increase the project budget for vision-assisted robots, or do we redesign the hardware to eliminate the need for vision entirely?"

$slide5 = $pres.Slides.Add(5, 2)
$slide5.Shapes.Title.TextFrame.TextRange.Text = "Hardware Redesign: Magnetic Connector"
$slide5.Shapes.Item(2).TextFrame.TextRange.Text = "The Concept: Replace the complex 4-port plug with a centralized single magnetic connector.`nEliminating Vision: Magnetic connectors auto-align themselves via magnetic pull. A low-cost, DIY robot arm only needs to bring the plug close to the port, and magnets will snap it into perfect alignment automatically.`nCost Efficiency: This drastically drops the robotics budget, allowing us to build an in-house DIY arm."

$slide6 = $pres.Slides.Add(6, 2)
$slide6.Shapes.Title.TextFrame.TextRange.Text = "Fleet Retrofit Cost Analysis"
$slide6.Shapes.Item(2).TextFrame.TextRange.Text = "Implementation Considerations for 250 OHT Vehicles:`n`nMagnetic 15-Pin Plug (Premium Rosenberger): SGD 17,500`n(Alternative) Cheaper OEM Plug: ~SGD 3,750`nCustom PCB Breakout & Relays: ~SGD 2,000`nMounting Brackets & Labor: ~SGD 23,750`n`nGRAND TOTAL (Premium Plug): ~SGD 42,550`nGRAND TOTAL (Cheaper OEM Plug): ~SGD 28,800"

$slide7 = $pres.Slides.Add(7, 2)
$slide7.Shapes.Title.TextFrame.TextRange.Text = "Sourcing & PCB Implementation"
$slide7.Shapes.Item(2).TextFrame.TextRange.Text = "Vendor Pricing Comparison:`nRosenberger (Germany): Premium / Aerospace Grade (~SGD 70.00)`nC.C.P. Contact Probes (Taiwan): High Industrial Grade (~SGD 25.00)`nHytePro / Top-Link (China): Standard OEM Pogo-Pin (~SGD 15.00)`n`nInstallation Mechanics (PCB Bridging):`nThe Bridge Board: We will design a tiny, custom PCB (SGD 3 each) to seamlessly map the magnetic pins to our standard wires."

$slide8 = $pres.Slides.Add(8, 2)
$slide8.Shapes.Title.TextFrame.TextRange.Text = "Next Steps & Conclusion"
$slide8.Shapes.Item(2).TextFrame.TextRange.Text = "Finalize the budget review and formally reject the current OMRON / UR / ABB vendor quotations.`nDraft the electrical schematic for the proposed central testing station relay circuit.`nProcure sample magnetic connectors from OEM manufacturers to test alignment strength.`n`nConclusion: By intelligently redesigning the physical connector and centralizing the relay logic, we bypass expensive robot vision requirements and achieve massive cost savings for the automation project."

$pres.SaveAs("C:\Users\yiron\Desktop\FYProject\FYP\Project Proposal\Robot_Arm_Proposal_Slides.pptx")
$pres.Close()

$pres2 = $ppt.Presentations.Add($msoTrue)
$pres2.ApplyTheme("C:\Users\yiron\Desktop\FYProject\Context of GF Internship\Submissions\YiRong_U2320967K_PA_Slides.pptx")

$s1 = $pres2.Slides.Add(1, 1)
$s1.Shapes.Title.TextFrame.TextRange.Text = "Automated Sensor Testing Prototype"
$s1.Shapes.Item(2).TextFrame.TextRange.Text = "Implementation Plan & Hardware Setup`nPresenter: Tan Yi Rong"

$s2 = $pres2.Slides.Add(2, 2)
$s2.Shapes.Title.TextFrame.TextRange.Text = "Prototype Objective"
$s2.Shapes.Item(2).TextFrame.TextRange.Text = "The Goal: Build a small-scale, fully functional physical prototype that perfectly mimics the Overhead Hoist Transport (OHT) sensor environment.`nWhy We Need It: Before retrofitting 250 vehicles, we must validate the new magnetic connector logic and prove our centralized testing station concept works flawlessly.`nThe Scope: Automate the physical 'Detect / No-Detect' environment to eliminate the need for an engineer to manually stand in front of the sensors."

$s3 = $pres2.Slides.Add(3, 2)
$s3.Shapes.Title.TextFrame.TextRange.Text = "Structural Architecture"
$s3.Shapes.Item(2).TextFrame.TextRange.Text = "The Base Structure: A rigid structure built using standard T-Slot Extruded Aluminum (e.g., 2020 or 3030 series) for maximum modularity and ease of adjustment.`nSensor Mounting: All 4 OHT sensors will be securely mounted to the top beam of the aluminum frame, spaced out to perfectly replicate their actual positions on a real Mark 2 OHT chassis.`nCalibration: The frame will be designed to allow slight Z-axis and X-axis adjustments to fine-tune the sensor angles before locking them down."

$s4 = $pres2.Slides.Add(4, 2)
$s4.Shapes.Title.TextFrame.TextRange.Text = "Automated Obstacle Simulation"
$s4.Shapes.Item(2).TextFrame.TextRange.Text = "The Physical Concept: Instead of an engineer manually blocking the sensors, a large sheet of acrylic will serve as the artificial 'obstacle.'`nDistance Accuracy: The rig will be calibrated to mimic an obstacle exactly 1.5 meters away from the mounted sensors.`nAutomated Movement: The acrylic sheet will be programmed to automatically slide Left/Right and Up/Down, triggering the different sensors sequentially to generate live data for the Simulator."

$s5 = $pres2.Slides.Add(5, 2)
$s5.Shapes.Title.TextFrame.TextRange.Text = "Required Hardware & Motors"
$s5.Shapes.Item(2).TextFrame.TextRange.Text = "Linear Motion: 2x Linear Guide Rails (with carriage blocks) to ensure the acrylic sheet slides smoothly without vibration.`nActuation (Motors): 2x NEMA 17 Stepper Motors (one for the X-axis, one for the Y-axis) to provide precise, programmable control over the acrylic sheet's position.`nDrive System: GT2 Timing Belts and Pulleys connected to the stepper motors.`nThe Target: 1x Matte or Tinted Acrylic Sheet (sized appropriately to block the sensors without reflecting stray light)."

$s6 = $pres2.Slides.Add(6, 2)
$s6.Shapes.Title.TextFrame.TextRange.Text = "Electrical & Wiring Architecture"
$s6.Shapes.Item(2).TextFrame.TextRange.Text = "Motor Control: An Arduino Uno paired with a CNC Shield and A4988 Stepper Drivers to execute the programmed movement of the acrylic sheet.`nSensor Wiring: The 4 sensors on the aluminum frame will be wired down into the proposed Centralized Relay Station.`nMagnetic Plug Test: The relay station will route through our prototype PCB Bridge Board, connecting to a sample OEM Magnetic 15-Pin Plug to prove signal integrity."

$s7 = $pres2.Slides.Add(7, 2)
$s7.Shapes.Title.TextFrame.TextRange.Text = "Budget Justification"
$s7.Shapes.Item(2).TextFrame.TextRange.Text = "Items to Purchase: T-Slot Aluminum Extrusions, 2x NEMA 17 Motors + Rails, Arduino Kit, 4-Channel Relay Module, OEM Magnetic Plug Samples, Acrylic Sheet.`nPurchase Justification: Procuring these low-cost Maker components (estimated under SGD 300 total) allows us to physically validate the centralized relay logic and the magnetic plug alignment.`nROI: This minimal upfront prototyping cost acts as the final proof-of-concept required to unlock our SGD ~42k fleet retrofit strategy, definitively proving to management that a >SGD 100k cobot is entirely unnecessary."

$s8 = $pres2.Slides.Add(8, 2)
$s8.Shapes.Title.TextFrame.TextRange.Text = "Full System Integration (The Loop)"
$s8.Shapes.Item(2).TextFrame.TextRange.Text = "The DIY Coordinate Robot: I will construct a low-cost Cartesian robot arm. Because the magnetic plug is self-aligning, this DIY arm only needs 'blind' coordinate movement to plug in and out - no camera vision is required!`nThe Seamless Loop:`n  1. The DIY arm snaps the magnetic plug into the testing port.`n  2. The C# PM Simulator commands the acrylic sheet to move, simulating an obstacle.`n  3. As the Simulator successfully detects the obstacle and the 'Next Step' button is pressed, the Central Relay instantly switches power to the next sensor.`n  4. The acrylic sheet automatically shifts to the new sensor, entirely automating the once-manual PM testing process!"

$pres2.SaveAs("C:\Users\yiron\Desktop\FYProject\FYP\Project Proposal\Prototype_Implementation_Slides.pptx")
$pres2.Close()
$ppt.Quit()
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($ppt) | Out-Null
