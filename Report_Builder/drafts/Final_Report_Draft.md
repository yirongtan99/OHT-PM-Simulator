# Final Internship Report

**Author:** Tan Yi Rong
**Matriculation Number:** U2320967K
**Company:** GlobalFoundries Singapore
**Department:** F7 Automated Material Handling System (AMHS)
**Role:** Equipment Engineer Intern
**Attachment Period:** May 25, 2026 – Aug 7, 2026 (11 Weeks)
**Date:** [Insert Submission Date]

---

## Abstract

This report documents the work done during an 11-week Professional Attachment at GlobalFoundries Singapore, within the F7 Automated Material Handling System (AMHS) department. Two workstreams ran concurrently throughout the attachment. The first and primary focus was Project Odin — a sensor-based initiative designed to replace the manual, vernier caliper-based wheel measurement process currently performed by line support during OHT Preventive Maintenance (PM), with a consistent and automated displacement sensor system. The second was the early-stage feasibility study for a Final Year Project (FYP), exploring a phased approach to automating the manual PM workflow for OHT Mark 2 obstacle detection sensors. This report covers the problem being solved, the sensing approach, physical setup, calibration, testing, and future implementation plan for Project Odin, followed by a summary of the vendor research, cost analysis, and the three-phase automation roadmap for the FYP.

---

## 1. Introduction & Background

GlobalFoundries operates an extensive Automated Material Handling System (AMHS) in its F7 fabrication facility. The Overhead Hoist Transport (OHT) fleet moves wafers across the cleanroom automatically, running continuously throughout production. Keeping these vehicles in good working order requires regular Preventive Maintenance carried out by the line support team — the technicians responsible for day-to-day maintenance and servicing of the OHT fleet.

Two related problems were identified going into this attachment. The first was with how OHT wheel measurements were being captured during PM. The second was with the manual, judgment-dependent process used to verify the OHT obstacle detection sensors. Both share a common root cause — over-reliance on individual technician execution, which introduces variability and inconsistency into what should be a standardised process.

Project Odin addressed the first problem. The FYP feasibility study focused on the second, laying out a phased automation roadmap to reduce manual steps in the sensor PM workflow.

---

## 2. Project Odin — OHT Wheel Wear Monitoring

### 2.1 The Problem with the Current Method

As part of the existing PM process, line support is required to measure the diameter of each OHT wheel using a vernier caliper. While this is a standard measurement tool, applying it consistently in a busy maintenance environment is harder than it sounds.

The measurement outcome varies depending on how the caliper is positioned, how much pressure is applied, and how experienced the person doing it is. Different line support staff measure slightly differently, and this results in data that is inconsistent between technicians. A more telling sign of the problem is that the recorded data sometimes shows wheel diameters increasing over time — which physically does not make sense, since a wheel can only wear down, not grow. This pattern in the data is a clear indicator that the measurements are being affected by human error rather than reflecting the actual condition of the wheels.

The consequence of this is that the data collected through the current method cannot be relied upon for any meaningful trend analysis. There is no clean baseline to compare against, and decisions about wheel replacement are still largely based on schedule or visual inspection rather than actual measured wear.

### 2.2 Objective

Project Odin was brought in to solve this directly. The goal is to replace the vernier caliper measurement step with a sensor-based system that captures wheel diameter automatically and consistently every time an OHT passes through the measurement station — removing the human variable from the equation entirely.

With a reliable, repeatable dataset in place, the department will eventually be able to move from scheduled wheel replacement to a predictive maintenance model — where replacement decisions are driven by actual measured wear data rather than fixed time intervals.

### 2.3 How the Sensing Works

The system uses a displacement sensor mounted in a fixed position above the OHT track, facing downward. As an OHT rolls beneath it, the sensor continuously scans the surface below, capturing distance readings in real time as the wheel passes through its field of view.

What the sensor captures across each pass is not just a single reading — it is a profile of the wheel's curvature as it moves underneath. The reading starts shallow at the leading edge of the wheel, climbs as the highest point of the wheel comes into range, then drops back down as the wheel exits. This produces a curve of distance measurements for each pass.

From this curve, the system selects the highest valued data point — the peak of the curvature, which corresponds to the closest the wheel surface gets to the sensor. This is the most representative value of the wheel's current diameter.

The final calculation is:

> **Wheel Diameter = 125mm − Peak Reading**

The sensor is calibrated against a reference wheel with a known diameter of 125mm. At this reference, the peak reading is 0mm. As a wheel wears and its diameter shrinks, it sits slightly lower on the track, causing the peak reading to increase — and the calculated diameter to drop below 125mm. Wheels measuring at or below 123mm are flagged for replacement, representing a 2mm reduction from the healthy baseline.

### 2.4 Physical Setup

The installation on the track consists of the following components:

- **Displacement Sensor (LiDAR)** — Mounted above the track, facing downward. Scans continuously as each OHT rolls beneath it.
- **Master Amplifier Controller** — Processes the primary sensor signal.
- **Slave Amplifier Controller** — Mirrors the data for the secondary channel, allowing future dual-sensor coverage.
- **Ethernet Connectivity** — Links the controllers to the data collection system.
- **Power Supply Unit** — Provides stable power to the sensor and controllers.
- **Circuit Breaker** — For safe isolation of the setup.

The layout is compact and sits unobtrusively on the track without disrupting normal OHT operations.

### 2.5 Calibration & Testing

Before data collection could start, the sensor was calibrated against a reference wheel with a confirmed diameter of 125mm. This set the zero point for the system, making all subsequent readings relative to that known baseline.

Testing was done by physically rolling a wheel beneath the sensor to simulate an OHT pass. The main things being verified were that the system could correctly detect the start and end of a wheel pass, capture the full profile curvature across the pass, and reliably select the peak value.

Several rounds of threshold adjustment were needed before the detection was consistently stable. The challenge was making sure the sensor only triggered on actual wheels and not on background objects or noise at track level. Once the thresholds were set correctly, the system produced reliable results across repeated test runs, and the calculated diameter values matched manual reference measurements taken alongside.

### 2.6 Data Collected

Each time an OHT passes the sensor station, the system logs one record containing:

- Timestamp of the wheel pass
- OHT vehicle number
- Measured diameter for each of the four wheel positions — Front Left (FL), Front Right (FR), Back Left (BL), and Back Right (BR)

In the current prototype stage, only one physical sensor is deployed. The four-wheel data is generated using the single sensor's reading as the primary value, with minor simulated variation added for the other three positions. This is a temporary measure to allow the data structure and logging framework to be fully validated before full deployment.

### 2.7 Future Implementation

The current setup is a validated prototype. Deploying it properly for fleet-wide use involves a few more steps.

The immediate next step is to move the sensor system into the maintenance room inside the fab — the controlled space where OHTs are brought in for scheduled PM. This is the most practical deployment location because every OHT that comes in for PM will naturally pass through the measurement station, ensuring regular readings for every vehicle without any additional process burden on the line support team.

From there, the focus shifts to building up a larger dataset over time. A single measurement every five to six months per vehicle is not enough to draw reliable wear trend conclusions. The dataset needs to grow across multiple PM cycles for a significant portion of the fleet before patterns become clear — such as which vehicles wear faster, which wheel positions degrade first, or whether specific operating routes or track sections accelerate wear.

The key challenge in doing this accurately at scale is vehicle identification. Currently, tagging each measurement to the correct OHT number still requires a manual input step. To make this seamless and error-free, the planned next development for Project Odin is to integrate an **RFID tagging component** into the system. Each OHT already has a unique identifier, and a reader positioned alongside the sensor would automatically pick up the vehicle's tag as it passes — ensuring every measurement is correctly attributed without any manual input.

Once RFID tagging is in place and the dataset is large enough, the system will be positioned to support genuine predictive maintenance, where specific vehicles can be flagged for wheel replacement based on their actual measured wear trajectory rather than a fixed service schedule.

---

## 3. FYP Research & Feasibility — Automating the OHT Sensor PM

### 3.1 Background & Problem Statement

The PM process for the OHT Mark 2 obstacle detection sensors is one of the more involved maintenance tasks that the line support team handles. This PM is critical from a safety standpoint — the sensors being tested are responsible for detecting obstacles in the OHT's path and triggering the vehicle to stop before any collision occurs. Getting the PM right matters.

The sensors covered in each PM cycle are the Vehicle Detection Sensor (VHL) and three obstacle sensors — OBS Left, OBS Right, and OBS Center.

The current process works as follows: line support connects a data cable to the first sensor port and opens the Hokuyo application on a laptop to view the sensor's live waveform reading. Based on what they see on screen, they assess whether the sensor is within the acceptable operating range. If it is within spec, they take a screenshot of the waveform and document it. If it is not, they manually adjust the physical position of the sensor and recheck until the reading is within range. If the sensor cannot be brought into spec even after adjustment, it is swapped out for a replacement unit and the verification is repeated from the start.

This is done for each of the four sensors in turn, which means the data cable has to be physically unplugged from one port and plugged into the next, repeatedly across the session. On top of this, line support also has to manually shift a panel or reflective plate in front of each sensor to simulate an obstacle being present — then check that the sensor correctly registers the detection — before shifting the panel away again to confirm the sensor clears. This is done to verify both the detection and non-detection states for each sensor.

Each full PM cycle takes around one hour per OHT, and the team handles about four OHTs per day. The process is effective, but the reliance on individual technician judgment — both in reading the waveform and in shifting the panel — introduces variability. Whether a waveform is "within spec" depends on the person looking at it, and the consistency of panel positioning from one technician to the next is not guaranteed. These are the gaps the FYP aims to close.

### 3.2 Phased Automation Roadmap

Rather than trying to automate everything at once, the FYP adopts a phased approach that addresses the most impactful manual steps first and builds towards a fully integrated solution over time.

**Phase 1 — Automate the Cable Plug-In/Out**

The first phase focuses on using a Collaborative Robot (cobot) arm to handle the physical plugging and unplugging of the data cable between sensor ports. This is the most repetitive mechanical action in the PM workflow and is a good candidate for automation because it does not require complex judgment — the cobot just needs to locate the correct port, connect the cable, hold it in place while the reading is captured, then disconnect and move to the next port.

Automating this specific step is estimated to remove roughly 10 minutes from each PM cycle. At four OHTs per day and 208 working days per year, that adds up to approximately 416 technician hours saved annually.

**Phase 2 — Automate the Panel Shifting**

The second phase tackles the manual panel shifting that line support currently does to simulate obstacle detection. This involves moving a reflective plate or panel in and out of the sensor's field of view to verify that each sensor correctly registers the presence and absence of an obstacle. Automating this step would require a mechanism — likely motorised or actuator-driven — that can reliably position the panel at the correct location and angle for each sensor test, then retract it again.

This phase has more mechanical complexity than Phase 1, which is why it comes second. Getting the cable connection automated and validated first provides a stable foundation before adding the panel actuation on top.

**Phase 3 — Integrated Stationary Workstation**

Phase 3 is the end state — bringing together the cobot arm, the panel shifting mechanism, and the waveform verification into a single integrated workstation. The idea is that line support would bring an OHT to the station, connect it up, and then step through the PM sequence by pressing a button at each stage. The system handles the physical actions, and the technician's role shifts from doing the manual work to supervising and confirming each step.

This removes the judgment dependency from the process and creates a consistent, repeatable PM workflow that produces standardised documentation automatically at the end of each session.

### 3.3 Vendor Research & Cost Findings

Four cobot options were evaluated for Phase 1: OMRON TM5S, Universal Robots UR7e, ABB GoFa, and ABB PoWa.

| Vendor | Est. Price | Reach | Vision |
|---|---|---|---|
| OMRON TM5S | SGD 100k – 150k | 900mm | Built-in |
| UR7e | ~SGD 29k – 36k | 850mm | 3rd Party |
| ABB GoFa | ~SGD 20k – 30k | Up to 1300mm | SICK (~SGD 5k) |
| ABB PoWa | ~SGD 15k – 20k | Up to 1300mm | 3rd Party |

The OMRON was ruled out based on cost — at over SGD 100k, the price is too high relative to what Phase 1 alone delivers. The ABB series came out as the most viable option.

One practical challenge identified was that the existing 4-port plug arrangement on the OHT is not straightforward for a basic vision system to align to reliably. As a more cost-effective alternative, a proposal was put forward to retrofit all 304 OHTs with a single centralised magnetic connector. Because a magnetic plug snaps into place by physical feel, it removes the need for precise vision-guided alignment. At the workstation side, a custom PCB relay board routes the signal to each sensor in sequence, reproducing the same test workflow from one fixed connection point.

The estimated retrofit cost for the full fleet of 304 vehicles:

| Component | Unit Cost | Total (304 OHTs) |
|---|---|---|
| Rosenberger 15-pin Magnetic Plug (Premium) | ~SGD 70 | ~SGD 21,280 |
| Standard Pogo-Pin Plug (Budget) | ~SGD 4 | ~SGD 1,216 |
| Custom PCB Bridge Board | ~SGD 3 | ~SGD 912 |
| **Grand Total — Premium Route** | | **~SGD 22,192** |
| **Grand Total — Budget Route** | | **~SGD 2,128** |

One-time retrofit labour at ~SGD 24.34/hr (AE rate based on USD 45k/yr, 4-day 12-hour swing shift): **~SGD 7,344** for 304 OHTs at 1 hour each. Estimated rollout: 2.5 to 4 months at 4–6 OHTs per day.

### 3.4 Recommended Path & ROI

The recommended approach is an ABB cobot (~SGD 20k) combined with the budget magnetic plug retrofit (~SGD 2,128) and a custom PCB relay board — bringing the total estimated investment to around SGD 30k. This is roughly one-fifth the cost of the OMRON option for the same functional outcome in Phase 1.

From an ROI standpoint, the system is projected to recover approximately SGD 10,100 in direct manpower costs per year (416 hrs × SGD 24.34/hr), giving a payback period of around three years. Beyond the direct savings, the automation removes the risk of port damage from repeated manual connections, eliminates waveform reading inconsistency between operators, and lays the groundwork for Phases 2 and 3 of the full PM automation roadmap.

---

## 4. Reflections

Working across both projects during this attachment gave a grounded view of what engineering problem-solving actually looks like when you are working within real operational constraints.

For Project Odin, the most interesting part was understanding why the existing measurement method was failing — not because the vernier caliper is a bad tool, but because the conditions it was being used in made consistent results nearly impossible. Once that was clear, the case for an automated sensor-based approach became straightforward. The practical work of setting up, calibrating, and testing the system was also a good exercise in patience — getting the detection thresholds right took more iterations than expected, and there were moments where what looked like a working result turned out to have edge cases that needed fixing.

For the FYP, going through the phased planning process was the most useful part. It forced a more realistic look at what could actually be done within budget and within a reasonable timeframe, rather than trying to design the ideal solution from scratch. The magnetic plug idea was a direct result of that — finding a way to make the hardware problem simpler instead of throwing more expensive vision hardware at it.

---

## 5. Conclusion & Next Steps

Project Odin now has a validated prototype in place and is actively collecting wheel measurement data. The next steps are to deploy the system in the maintenance room for proper fleet-wide coverage, and to integrate RFID tagging so that vehicle identification is fully automatic. Building up the dataset over time remains the priority before any predictive maintenance decisions can be made with confidence.

For the FYP, the feasibility study confirms that automating the sensor PM process is viable at a justifiable cost. Phase 1 — the cobot arm for cable plug-in/out — is the immediate focus, with the magnetic plug retrofit as the enabling hardware change. From there, Phase 2 (panel shifting automation) and Phase 3 (integrated workstation) provide a clear path towards a fully streamlined, technician-supervised PM workflow that is consistent, documented, and scalable across the fleet.
