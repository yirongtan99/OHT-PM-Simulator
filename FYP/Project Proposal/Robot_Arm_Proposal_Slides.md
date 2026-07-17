# OHT Mark 2 PM Workflow Simulator - Robot Arm Project Proposal

---

## Slide 1: Title Slide
**Title:** Automated PM Workflow: Robot Arm Integration Proposal
**Subtitle:** Feasibility, Vendor Comparison, and Engineering Alternatives
**Presenter:** Tan Yi Rong
**Date:** July 2026

---

## Slide 2: Project Objective
**Header:** Automating the PM Process
**Bullet Points:**
- **Current State:** The Preventive Maintenance (PM) sensor connection is manual, labour-intensive, and prone to human error.
- **Proposed Solution:** Integrate a Collaborative Robot (Cobot) arm to automatically handle the sensor plug connection to the Overhead Hoist Transport (OHT).
- **Goal:** Reduce operational downtime, eliminate manual alignment errors, and enable a fully automated, vision-assisted PM workflow.

---

## Slide 3: Vendor Comparison (OMRON vs. Universal Robots vs. ABB)
**Header:** Market Feasibility & Hardware Evaluation
**Content:** *During our evaluation phases, we assessed three top-tier collaborative robots for this specific application.*

| Feature / Criteria | OMRON (TM5S Series) | Universal Robots (UR7e / UR12e) | ABB (GoFa / POWA Series) |
| :--- | :--- | :--- | :--- |
| **Price Quotation** | **SGD 100k – 150k** | **~SGD 29k** (UR7e) – **~SGD 36k** (UR12e) | **SGD 40k – 70k** (GoFa) / **<SGD 30k** (POWA) |
| **Maximum Reach (Range)** | 900 mm | ~730mm - 1300mm | Up to 950mm (GoFa) |
| **Operating Speed** | 1.4 m/s (Average Speed) | Up to 1.0 m/s (in collab mode) | Up to 2.2 m/s (Fastest) |
| **Safety Factor** | Advanced collision detection (PL d) | 17 configurable safety functions | Advanced torque sensors (GoFa) |
| **Vision Integration** | **Built-in / Integrated Camera** | Without camera (adds heavy cost) | Requires specific vision add-ons |

**Takeaway:** While OMRON offers a superior all-in-one vision solution, and UR/ABB offer great modularity, the extreme budget constraints across all three vendors require us to explore internal engineering alternatives.

---

## Slide 4: The Core Challenge
**Header:** The Budgetary & Vision Hurdle
**Bullet Points:**
- **The Problem:** The primary driver of the high vendor quotations across OMRON, UR, and ABB is the requirement for a highly precise **Robot Vision System**.
- **Why Vision is Needed:** The current OHT sensor uses a complex **4-port plug**. A robot arm without a camera cannot accurately align and insert a 4-pin connector just by "feel". 
- **The Crossroads:** Do we heavily increase the project budget for vision-assisted robots, or do we redesign the hardware to eliminate the need for vision entirely?

---

## Slide 5: Alternative Engineering Proposal (DIY Route)
**Header:** Hardware Redesign: The Magnetic Connector Solution
**Bullet Points:**
- **The Concept:** Replace the complex 4-port plug with a **centralized single magnetic connector**.
- **Eliminating Vision:** Magnetic connectors auto-align themselves via magnetic pull. A low-cost, DIY robot arm (without an expensive vision system) only needs to bring the plug *close* to the port, and the magnets will snap it into perfect alignment automatically.
- **Cost Efficiency:** This drastically drops the robotics budget, allowing us to build an in-house DIY arm instead of relying on premium OMRON/UR/ABB hardware.

---

## Slide 6: Magnetic Alternative - Fleet Retrofit Cost Analysis
**Header:** Implementation Considerations for 250 OHT Vehicles
**Content:** *Adopting the DIY magnetic solution bypasses heavy robot vision costs but introduces a fleet-wide retrofit requirement. We must modify all ~250 Mark 2 OHTs in the fab.*

| Component / Requirement | Estimated Unit Cost | Quantity | Estimated Total Cost |
| :--- | :--- | :--- | :--- |
| **Magnetic 15-Pin Plug** *(Premium Rosenberger)* | SGD 70 | 250 | **SGD 17,500** |
| *(Alternative)* **Magnetic Plug** *(Cheaper OEM)* | ~SGD 15 | 250 | *(~SGD 3,750)* |
| **Custom PCB / Breakout Board** *(To route 4-port slots)* | ~SGD 3 | 250 | **~SGD 750** |
| **Internal Wiring & Ribbon Cables** | ~SGD 2 | 250 sets | **~SGD 500** |
| **Custom Mounting Brackets** *(Machined / 3D Print)* | ~SGD 15 | 250 | **~SGD 3,750** |
| **Technician Retrofit Labor** *(~2 hours @ SGD 40/hr)* | ~SGD 80 | 250 OHTs | **~SGD 20,000** |
| **Testing Station Relay Board** *(Centralized at PC)* | ~SGD 50 | **1 Station** | **~SGD 50** |
| **GRAND TOTAL (Using Premium Rosenberger)** | | | **~SGD 42,550** |
| **GRAND TOTAL (Using Cheaper OEM Plug)** | | | **~SGD 28,800** |

**Takeaway:** By centralizing the sensor switching relays and opting for a high-quality but cheaper OEM magnetic plug, the entire fleet-wide retrofit cost plummets to under SGD 29k. This makes the DIY magnetic solution financially undeniably superior to a >SGD 100k vision cobot.

---

## Slide 7: Magnetic Connector Sourcing & PCB Implementation
**Header:** Hardware Sourcing & Installation
**Content:** *The premium Rosenberger MultiMag 15 is excellent, but OEM alternatives can drastically cut the $17,500 connector budget.*

### Vendor Pricing Comparison
| Manufacturer / Brand | Connector Quality | Estimated Unit Price (Bulk 250) |
| :--- | :--- | :--- |
| **Rosenberger (Germany)** | Premium / Aerospace Grade | ~SGD 70.00 |
| **C.C.P. Contact Probes (Taiwan)** | High Industrial Grade | ~SGD 25.00 |
| **HytePro / Top-Link (China)** | Standard OEM Pogo-Pin | ~SGD 15.00 |

### Installation Mechanics (PCB Bridging)
- **Not Plug-and-Play:** Magnetic plugs with "Straight Panel PCB" legs cannot connect directly to standard wires.
- **The Bridge Board:** We will design a tiny, custom PCB (SGD 3 each). The magnetic connector is permanently soldered into one side of this board.
- **The Connection:** Standard wire terminals (e.g., JST sockets) are soldered to the other side of the PCB, allowing the existing OHT wires to plug in flawlessly.

---

## Slide 8: Next Steps & Conclusion
**Header:** Moving Forward
**Bullet Points:**
- Finalize the budget review and formally reject the current OMRON / UR / ABB vendor quotations.
- Draft the electrical schematic for the proposed central testing station relay circuit.
- Procure sample magnetic connectors from OEM manufacturers to test alignment strength.
- **Conclusion:** By intelligently redesigning the physical connector and centralizing the relay logic, we bypass expensive robot vision requirements and achieve massive cost savings for the automation project.
