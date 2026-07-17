---
marp: true
theme: default
paginate: true
size: 16:9
---

# Final Year Project & Professional Attachment
**Author:** Tan Yi Rong  
**Company:** GlobalFoundries  
**Department:** Automated Material Handling System (AMHS)  
**Projects:** Project Odin & Semi-Automated PM System (FYP)

---

## Introduction & Context
- **Department:** Automated Material Handling System (AMHS)
- **Equipment:** Overhead Hoist Transport (OHT) System
- **Core Objectives:**
  1. Continue **Project Odin** (Wheel wear & tear measurement)
  2. Develop a **Semi-Automated Preventive Maintenance (PM) System** for OHT Mark 2 Obstacle Sensors (FYP)

---

## Project ODIN Recap
<!-- [INSERT OLD SLIDES OF HOW PROJECT ODIN WILL WORK HERE] -->
- Brief recap of the mechanism for capturing predictive maintenance data.
- Overview of the system architecture previously developed.

---

## Project ODIN CAD Design
<!-- [INSERT CAD DRAWING HERE] -->
- Visualization of the Project Odin deployment.
- Exact placement and mounting layout on the OHT track.

---

## Project ODIN Layout
<!-- [INSERT ACTUAL IMAGE OF SETUP HERE] -->
**Physical Setup Components:**
- Master & Slave Controllers
- Ethernet Connectivity
- Power Supply Unit
- Circuit Breaker

---

## Testing Phase
<!-- [INSERT PICTURES OF MANUFACTURED PART AND SENSOR TESTING] -->
- Real manufactured part of the frame in action.
- Live sensor reading feedback on the controller during wheel testing.

---

## Part 1: Project ODIN Objectives
**Objective:** Support Predictive Maintenance (PdM) through wheel condition monitoring.
- Identify parameters for accurate PdM data collection.
- Execute continuous sensor testing to collect wheel measurement data.
- Validate sensor outputs to ensure they accurately reflect physical wear over time.

---

## UI Interface for Live Monitoring
- **Real-Time Data:** Dashboard displaying live sensor readings as OHT vehicles pass the test stations.
- **Immediate Alerts:** Visual indicators highlighting any readings that breach the safety thresholds for wheel wear.

---

## UI Interface for Fleet Overview
- **System-Wide Tracking:** A high-level overview of the entire OHT fleet's status.
- **Maintenance Queues:** Easily identify which vehicles are overdue for inspection or have logged irregular wheel measurements.

---

## UI interface for OHT Analysis
- **Trend Analysis:** Historical graphing of wheel degradation over weeks and months.
- **Predictive Insights:** Data-driven predictions on when specific wheels will fail, allowing for pre-emptive parts replacement instead of reactive maintenance.

---

## Part 2: FYP Research & Feasibility
**Objective:** Automate the repetitive and manual OHT Mark 2 Sensor PM process.
- Reduce operational downtime.
- Eliminate manual alignment errors.
- Enable a fully automated, vision-assisted (or mechanically guided) PM workflow.

---

## Manual PM Process
<!-- [INSERT FLOWCHART HERE] -->
**Current State Challenges:**
- The current total PM takes ~1 hour per OHT, executed on 4 OHTs per day.
- Each OHT undergoes PM every 5-6 months depending on its condition.
- The sensor connection phase is highly manual, forcing technicians to align 4-port plugs without vision assistance.
- Prone to human error, physical strain, and connection damage.

---

## FYP Project Possibility
**Brainstorming the Automation Concept:**
- **The Vision:** Integrate a Collaborative Robot (Cobot) arm to handle the specific plug-in/plug-out process.
- **The Execution:** Use a technician-in-the-loop software workflow to guide the robot, run tests, and log results.
- **The Goal:** Automating this exact step saves up to 10 minutes of the 1-hour total PM time per vehicle, while completely eliminating manual docking errors.

---

## Vendor Search
**Cobot Cost & Specification Findings:**
- **OMRON (TM5S):** SGD 100k - 150k | Integrated Vision | 900mm reach
- **Universal Robots (UR7e):** ~SGD 29k - 36k | 3rd Party Vision | ~730-1300mm reach
- **ABB (GoFa 5/13):** ~SGD 20k - 30k | SICK Smart Vision (~5k) | Up to 1300mm reach
- **ABB (PoWa 7/13):** ~SGD 15k - 20k | 3rd Party Vision | Up to 1300mm reach

---

## Alternative Method (Cost Reduction)
**Retrofit 304 Vehicles with Centralized Magnetic Plugs:**
- **Hardware Route:**
  - Premium: Magnetic 15-Pin Plug (Rosenberger) @ SGD 70/unit $\rightarrow$ ~SGD 23,712
  - Budget: Magnetic 12-Pin Plug (OEM) @ SGD 15/unit $\rightarrow$ ~SGD 6,992
  - Custom PCB Bridge: ~SGD 3/unit
- **Manpower Cost for Retrofit (304 OHTs):**
  - *Metrics:* AE Pay: $45k USD/yr. Shift: 4 days/wk @ 12 hrs/day (~$18.03 USD/hr).
  - *Labor Time:* 30 mins per OHT (152 Total Hours). 
  - *Est. Labor Cost:* ~$2,740 USD (~SGD 3,672)

---

## Justification (Pros & Cons)
- **OMRON (TM5S):** Excellent out-of-the-box integration, but severely breaks the budget (>100k) for a process that only saves 10 mins per PM.
- **ABB / UR Series:** Highly cost-effective (15k - 36k). Require 3rd-party vision plugins (like SICK).
- **The Verdict:** The current 4-port plug is too difficult for basic vision systems. The most viable path is purchasing a cost-effective ABB/UR cobot and retrofitting the fleet with **Magnetic Plugs** (alignment by "feel"), accounting for the one-time ~SGD 3,672 manpower retrofit cost.

---

## ROI of Each Outcome / Purchase
- **Hybrid System Total Cost:** ~SGD 24k to 29k (ABB PoWa + OEM Magnetic Retrofit + Labor). 
- **Time Savings:** 
  - Automating the connection saves **10 minutes per OHT**.
  - At 4 OHTs/day, this saves **40 minutes daily** of strict technician labor.
- **True ROI Value:** Beyond the ~100 hours of direct labor saved annually (across 2 PM cycles per vehicle/yr), the system prevents costly port damage from forced manual connections and eliminates human verification errors, justifying the ~25k investment without needing the 150k OMRON.

---

# END
*Thank you for your time. Any Questions?*
