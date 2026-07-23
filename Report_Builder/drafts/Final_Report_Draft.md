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

This report documents the work done during an 11-week Professional Attachment at GlobalFoundries Singapore, within the F7 Automated Material Handling System (AMHS) department. The attachment covered two workstreams running concurrently. The first and main focus was the continuation of Project Odin — a sensor-based initiative to measure OHT wheel wear and tear by scanning the wheel profile as vehicles roll beneath a fixed displacement sensor. The second was the early-stage feasibility study for a Final Year Project (FYP), looking into whether the manual Preventive Maintenance (PM) workflow for OHT Mark 2 obstacle detection sensors could be automated. This report covers the sensing approach, physical setup, calibration process, testing, and future implementation plan for Project Odin, followed by a summary of the vendor research, cost analysis, and proposed engineering direction for the FYP.

---

## 1. Introduction & Background

GlobalFoundries operates an extensive Automated Material Handling System (AMHS) in its F7 fabrication facility. The Overhead Hoist Transport (OHT) fleet is central to this — these vehicles move wafers across the cleanroom automatically, running continuously throughout production. Keeping them in good working condition requires regular Preventive Maintenance, both for the mechanical parts like wheels, and for the sensors that detect obstacles during operation.

Two problems were identified going into this attachment. The first was that there was no structured, data-driven way to track OHT wheel wear over time. Wheel condition was typically assessed through visual checks or fixed schedule intervals, which left room for inconsistency. The second problem was that the manual PM workflow for the OHT obstacle detection sensors was repetitive and heavily dependent on individual technician execution — making it both time-consuming and difficult to standardise.

Project Odin was brought in to tackle the first problem. The FYP research ran alongside it to explore what automating the second problem would actually take and cost.

---

## 2. Project Odin — OHT Wheel Wear Monitoring

### 2.1 Objective

The goal of Project Odin is to build an automated system that can continuously monitor OHT wheel wear condition without requiring manual inspection. The underlying idea is that if you can measure the profile of each wheel every time an OHT passes a fixed sensor point, you can track how that profile changes over time — and from that, infer how much the wheel has worn down.

Rather than waiting for a wheel to visibly degrade or cause a fault, the system lets the data do the work. Any downward trend in the wheel diameter across repeated measurements is an early signal that the wheel is approaching the point where it should be replaced. This shifts maintenance from being reactive to being predictive.

### 2.2 How the Sensing Works

The sensor used in this project is a displacement sensor mounted in a fixed position above the OHT track, facing downward. As an OHT rolls beneath it, the sensor continuously scans the surface below, capturing distance readings in real time as the wheel passes through its field of view.

Because the sensor is scanning a moving wheel from above, what it captures is not just a single reading — it is a profile of the wheel's curvature as it passes underneath. The reading starts shallow at the edge of the wheel, rises as the centre of the wheel moves into range, then drops back down as the wheel exits. This produces a curve of distance measurements for each pass.

From this curve, the system selects the highest valued data point — which corresponds to the peak of the wheel, or the closest point the wheel surface gets to the sensor. This is the most representative measurement of the wheel's current diameter at its widest point.

The final calculation is:

> **Wheel Diameter = 125mm − Peak Reading**

The sensor is calibrated against a reference wheel with a known diameter of 125mm. At this reference point, the sensor reads 0mm. As a wheel wears down and its diameter shrinks, it sits slightly lower, and the peak reading increases — meaning the calculated diameter drops below 125mm. A wheel measuring at or below 123mm is flagged for replacement, representing a 2mm reduction from the healthy baseline.

### 2.3 Physical Setup

The physical installation on the track consists of the following components:

- **Displacement Sensor (LiDAR)** — Mounted above the track, facing downward. Scans continuously as each OHT rolls beneath it.
- **Master Amplifier Controller** — Processes the primary sensor signal on the right channel.
- **Slave Amplifier Controller** — Mirrors the data on the left channel for dual-sensor coverage.
- **Ethernet Connectivity** — Links the controllers together and to the data collection system.
- **Power Supply Unit** — Provides stable power to the sensor and controllers.
- **Circuit Breaker** — For safe isolation and protection of the overall setup.

The layout was designed to be compact and sit unobtrusively on the track, so it does not affect normal OHT operations. Both the master and slave controller configuration allow for future expansion to a dual-sensor setup covering both sides of the vehicle.

### 2.4 Calibration & Testing

Before data collection could begin, the sensor had to be calibrated to a known reference. A wheel with a confirmed diameter of 125mm was positioned beneath the sensor and used to zero the system. From that point on, all readings are relative to this baseline — any positive deviation in the peak reading directly translates to wear on the wheel.

Testing was done by physically rolling a wheel beneath the sensor to simulate an OHT pass. The goal was to confirm that the detection logic was working correctly — that the system could consistently identify the start and end of a wheel pass, capture the full profile curvature, select the peak correctly, and compute the right diameter.

Several rounds of threshold adjustment were needed before the detection was stable enough. The main challenge was making sure the system only triggered on actual wheels and not on other objects or background noise at track level. Once the thresholds were set correctly, the system reliably produced consistent results across repeated test runs.

The testing phase also helped validate that the peak selection logic was working as intended. Because the sensor captures a continuous stream of readings as the wheel passes, it is important that the system correctly identifies the single highest value from within each pass, rather than picking up a reading from the edge of the wheel or from outside the wheel entirely.

### 2.5 Data Collected

Each time an OHT passes the sensor station, the system logs one record containing:

- Timestamp of the wheel pass
- OHT vehicle number
- Measured diameter for each of the four wheels — Front Left (FL), Front Right (FR), Back Left (BL), and Back Right (BR)

In the current prototype stage, only one physical sensor is deployed, so the four-wheel measurement is generated by using the single sensor's reading as the primary data point, with minor simulated variation added for the other three positions. This is a temporary workaround to allow the data structure and logging framework to be validated before full deployment.

Data collection has been running since the system was set up, and the records are being stored in a CSV log file that can be reviewed at any time. The existing data shows that the majority of vehicles tested are within the healthy range above 123mm.

### 2.6 Future Implementation

The current setup is a working prototype, but deploying it for actual fleet-wide use will require a few more steps.

The immediate next step is to deploy the sensor system in the maintenance room inside the fab — the controlled environment where OHTs are brought in for scheduled PM. This is a more practical deployment location than the open track because every OHT that goes through PM will naturally pass the measurement station, making it easy to ensure regular readings for the entire fleet without requiring a separate process.

Once the system is in the maintenance room, the focus will shift to building up a larger dataset over time. A single measurement per PM cycle (every 5–6 months) is not enough to identify wear trends with confidence. The dataset needs to grow across multiple PM cycles for multiple vehicles before meaningful patterns can be drawn — such as which OHTs wear faster, which wheel positions degrade first, or whether there are specific track sections that accelerate wear.

The key challenge in doing this reliably is OHT identification. Right now, tagging each measurement to the correct OHT number requires a manual input step. To make this scalable and accurate, the planned next step for Project Odin is to integrate an **RFID tagging component** into the system. Each OHT would carry an RFID tag, and a reader installed alongside the sensor would automatically identify the vehicle as it passes — eliminating the manual tagging step entirely and ensuring every measurement is correctly attributed to the right vehicle from the moment it is logged.

Once RFID tagging is in place and a sufficiently large dataset has been collected, the system will be in a position to support true predictive maintenance — flagging specific vehicles for wheel replacement based on their actual measured wear trend, rather than a fixed schedule.

---

## 3. FYP Research & Feasibility — Automating the OHT Sensor PM

### 3.1 Background & Problem Statement

The current PM process for the OHT Mark 2 obstacle detection sensors requires a technician to work through a set sequence of manual steps for each vehicle. The sensors involved are the Vehicle Detection Sensor (VHL) and three obstacle sensors — OBS Left, OBS Right, and OBS Center.

The full workflow goes roughly as follows:

1. Check the VHL sensor using a reflective plate, then remove the plate.
2. Shift the panel to the left and right positions to test each OBS sensor in turn.
3. Physically plug and unplug a 10-pin data cable between each sensor port in sequence.
4. Verify that each sensor's waveform and reading are within spec.
5. Manually adjust sensor position if any reading is out of spec.
6. Screenshot the passing result and upload it before the OHT is returned to operation.

Each full cycle takes roughly one hour per OHT. The team handles around four OHTs per day, and each vehicle is scheduled for PM every five to six months depending on its condition.

The core challenge with this workflow is not that it is ineffective — it works. The issue is that it is entirely manual and heavily dependent on the individual technician doing it. Repeated plugging and unplugging between ports, manual panel shifting, and manual documentation all add up. The consistency of the result depends on how experienced and careful the person doing the PM is, which makes it difficult to fully standardise.

### 3.2 Proposed Automation Approach

The proposed solution was to bring in a Collaborative Robot (cobot) arm to handle the physical plug-in and plug-out of the sensor data cable — the most repetitive part of the workflow. The idea is that with the right gripper and vision guidance, the cobot can locate the correct port on the OHT, connect the cable, wait for the sensor reading to be captured, then disconnect and move to the next port automatically.

Automating this specific step is expected to save around 10 minutes per PM cycle per OHT. That adds up to roughly 416 technician hours saved per year across the fleet (30 min × 4 OHTs/day × 208 working days).

### 3.3 Vendor Research & Cost Findings

Four cobot options were shortlisted and evaluated: OMRON TM5S, Universal Robots UR7e, ABB GoFa, and ABB PoWa.

| Vendor | Est. Price | Reach | Vision |
|---|---|---|---|
| OMRON TM5S | SGD 100k – 150k | 900mm | Built-in |
| UR7e | ~SGD 29k – 36k | 850mm | 3rd Party |
| ABB GoFa | ~SGD 20k – 30k | Up to 1300mm | SICK (~SGD 5k) |
| ABB PoWa | ~SGD 15k – 20k | Up to 1300mm | 3rd Party |

The OMRON was ruled out quickly. While it has integrated vision, the cost is far too high relative to the time savings on offer — spending over SGD 100k to save 10 minutes per PM is difficult to justify on a business case. The ABB series came out as the most viable option based on the cost-to-capability ratio.

### 3.4 The Magnetic Plug Alternative

One practical challenge that came up during the research was that the existing 4-port plug arrangement on the OHT is not easy for a basic vision system to align to precisely. Getting consistent, reliable alignment would likely require a high-end vision system, which adds cost.

A more practical alternative was proposed: retrofit all 304 OHTs with a single centralised magnetic connector that replaces the four separate 10-pin ports. Because a magnetic plug snaps into place physically, it removes the need for precise vision-guided alignment — the cobot just needs to get close enough and the connector does the rest.

At the workstation side, a custom PCB relay board routes the signal to each of the four sensors in sequence, reproducing the same test sequence as the manual process but from a single fixed connection point.

The estimated cost to retrofit the full fleet of 304 vehicles is as follows:

| Component | Unit Cost | Total (304 OHTs) |
|---|---|---|
| Rosenberger 15-pin Magnetic Plug (Premium) | ~SGD 70 | ~SGD 21,280 |
| Standard Pogo-Pin Plug (Budget) | ~SGD 4 | ~SGD 1,216 |
| Custom PCB Bridge Board | ~SGD 3 | ~SGD 912 |
| **Grand Total — Premium Route** | | **~SGD 22,192** |
| **Grand Total — Budget Route** | | **~SGD 2,128** |

One-time retrofit labour for 304 OHTs at 1 hour each, at an AE rate of ~SGD 24.34/hr (based on USD 45k/yr salary, 4-day 12-hour swing shift): **~SGD 7,344**. Rollout is estimated to take around 2.5 to 4 months at a pace of 4–6 OHTs per day.

### 3.5 Recommended Path & ROI

The recommended approach is to pair a cost-effective ABB cobot (~SGD 20k) with the budget magnetic plug retrofit (~SGD 2,128) and a custom PCB relay board, bringing the total estimated investment to around SGD 30k. This comes to approximately one-fifth of the cost of the OMRON option, for the same functional outcome.

On the ROI side, the system is expected to save around SGD 10,100 in direct manpower costs per year (416 hrs × SGD 24.34/hr), giving a payback period of roughly three years. Beyond the direct savings, the automation also removes the risk of port damage from repeated manual connections, and eliminates operator-to-operator variation in the PM results — both of which have value that is harder to quantify but real.

---

## 4. Reflections

Working across both projects during this attachment gave a well-rounded view of what engineering work actually looks like in a semiconductor fab environment.

For Project Odin, the most useful part was going through the full process of setting up a measurement system from scratch — from physical installation and calibration to validating the data and making sure the detection logic was reliable. The threshold tuning took more iterations than expected, which was a good reminder that getting a sensor-based system to behave consistently in a real environment takes patience and careful testing. The work also surfaced what the next steps need to be — the RFID tagging piece in particular became clearer once it was obvious how important accurate vehicle identification is going to be for building a trustworthy dataset.

For the FYP, the vendor research process was the most valuable exercise. Going through the costing properly made it clear that the budget constraint was actually a design constraint — and working within it led to a more interesting solution than the straightforward "buy the most capable robot" answer. The magnetic plug idea came directly out of trying to find a way to make vision guidance unnecessary, which turned out to reduce both cost and complexity at the same time.

---

## 5. Conclusion & Next Steps

Project Odin has a working prototype in place and is actively collecting wheel measurement data. The sensing approach and calibration are validated, and the logging framework is ready for ongoing use. The immediate next step is to move the system into the maintenance room for proper fleet-wide deployment, and then to integrate RFID tagging so that vehicle identification is automatic and reliable. Building up the dataset over time is the priority before any predictive maintenance decisions can be made with confidence.

For the FYP, the feasibility study confirms that automating the sensor PM workflow is achievable at a cost that makes sense. The recommended path is the ABB cobot paired with the magnetic plug retrofit and relay PCB board. Next steps include finalising the PCB design, running a pilot test on a small group of OHTs with the magnetic connectors, and then scoping the cobot programming work needed to handle the physical connection reliably.
