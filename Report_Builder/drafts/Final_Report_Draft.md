# Final Internship & Final Year Project Report

**Author:** Tan Yi Rong  
**Matriculation Number:** U2320967K  
**Company:** GlobalFoundries  
**Department:** Automated Material Handling System (AMHS)  
**Project Titles:** 
1. Project Odin: OHT Wheel Measurement and Predictive Maintenance
2. WSDeg FYP: Feasibility Study and Prototype Development of a Semi-Automated Preventive Maintenance System for OHT Mark 2 Obstacle Detection Sensors  
**Date:** [Insert Submission Date]  

---

## Abstract
This report details the work conducted during an 11-week Professional Attachment at GlobalFoundries within the Automated Material Handling System (AMHS) department. The attachment was bifurcated into two primary initiatives. The first half focused on **Project Odin**, an ongoing effort to measure Overhead Hoist Transport (OHT) wheel wear and tear for Predictive Maintenance (PdM). The second half transitioned into an industry-sponsored Final Year Project (FYP), which aimed to develop a semi-automated preventive maintenance (PM) prototype for OHT Mark 2 obstacle detection sensors. The report outlines the data collection methodologies, vendor engagements, feasibility studies, software development, and AI integration processes undertaken to address the operational challenges in the fab.

---

## 1. Introduction & Background
GlobalFoundries utilizes an extensive Automated Material Handling System (AMHS) to transport wafers across the fabrication plant. A critical component of this system is the Overhead Hoist Transport (OHT) network. Maintaining the reliability of the OHT vehicles requires stringent Preventive Maintenance (PM). Currently, the total PM takes roughly 1 hour per OHT, executed on 4 vehicles per day, with each vehicle undergoing PM every 5-6 months depending on its condition. The sensor connection phase is highly manual, time-consuming, and prone to human inconsistency. The overarching objective of this internship was to analyze these bottlenecks and engineer data-driven and automated solutions to improve operational efficiency.

---

## 2. Part 1: Project Odin (Wheel Wear & Tear)
### 2.1 Objective
Project Odin is a localized initiative aimed at tracking the physical degradation of OHT wheels. Without accurate condition monitoring, maintenance activities remain reactive rather than predictive, leading to unexpected faults and downtime on the production line.

### 2.2 Methodology & Execution
During the first six weeks, primary efforts were dedicated to understudying the AMHS operations and executing Project Odin.
- **Data Collection:** Conducted continuous sensor testing on OHT vehicles to capture accurate physical measurements of the wheels.
- **Validation:** Ensured that the sensor parameters accurately reflected the physical wear over time.
- **Data Processing:** Processed and sorted the raw sensor data, establishing a reliable dataset that could be used by the department to predict future wheel failures and schedule maintenance preemptively.

---

## 3. Part 2: FYP Feasibility Study & Vendor Sourcing
### 3.1 Objective
While executing Project Odin, concurrent research was conducted for the Final Year Project (FYP). The FYP aimed to improve the PM workflow for the OHT Mark 2 obstacle detection sensors by transitioning from a fully manual connector-handling process to a semi-automated one. Automating this exact step would save up to 10 minutes of the 1-hour total PM time per vehicle, while completely eliminating manual docking errors.

### 3.2 Vendor Engagement & Site Visits
To evaluate the feasibility of utilizing collaborative robots (cobots) for sensor connector handling, engagements were made with OMRON and Universal Robots (UR). A slide deck detailing the manual PM pain points was presented to both vendors. Furthermore, a site visit to the OMRON facility provided hands-on evaluation of industrial robotics and vision-assisted alignment technologies.

### 3.3 Challenges, Budgets & Engineering Pivots
A major project management challenge arose when the vendor quotations for high-end robotic arms (e.g., OMRON at >SGD 100k) significantly exceeded the allocated project budget. Given that the automation only saves 10 minutes per vehicle, a >100k investment was difficult to justify.

Consequently, the project scope pivoted towards more cost-effective DIY hardware alternatives paired with budget-friendly cobots (e.g., ABB/UR at SGD 15k-30k). Research was conducted into retrofitting all 304 vehicles, replacing the current 4-port plug with a centralized single magnetic plug. This would allow for proper alignment by physical feel rather than requiring an expensive camera vision system. Factoring in the Associate Engineer (AE) manpower cost for retrofitting 304 vehicles (~SGD 3,672), the total hybrid system cost comes to an estimated ~SGD 25k to 30k. This provides a rapid return on investment (ROI) by saving roughly 100 hours of direct labor annually and eliminating costly port damage from forced connections.

---

## 4. Part 3: FYP Prototype Development
### 4.1 Software-Assisted PM Workflow
Due to the hardware budget constraints, development efforts in the final weeks shifted heavily towards a software-assisted prototype. An offline, Windows-based simulator was developed using C# (WinForms).
- **Core Features:** The software manages PM test sequencing, provides operator guidance, captures screenshots, and logs run results locally within a restricted enterprise IT environment.
- **Optimization:** The simulator codebase was refactored into distinct modules, improving the user interface to accommodate waveform displays and future sensor feedback.

### 4.2 Artificial Intelligence (AI) Integration
To further assist technicians, an AI model was trained to automatically verify sensor functionality.
- **Dataset Preparation:** Over 400 sensor reading images were manually reviewed, sorted, and categorized into "Obstacle Detected" and "Clear" datasets.
- **Training & Testing:** The AI model was trained using this clean dataset. The model is currently undergoing offline testing to fine-tune the sensor's boundary conditions, as live testing on active OHT vehicles remains a hardware constraint.

---

## 5. Discussion & Reflections
This attachment provided profound exposure to industrial realities. 
- **Project Management:** Engaging with external vendors illuminated the gap between theoretical engineering concepts and commercial implementation, particularly regarding budget constraints and ROI justifications.
- **Technical Growth:** Extensive knowledge was gained in C# software architecture, data logging, and practical machine learning implementation (AI image training).
- **Problem Solving:** Navigating hardware limitations by shifting focus to a robust software and AI solution demonstrated the necessity of flexibility in engineering design.

---

## 6. Conclusion & Future Work
The 11-week attachment successfully advanced the predictive maintenance goals of Project Odin and established a strong foundation for the FYP. The C# Workflow Simulator and the AI image verification model serve as a functional proof-of-concept for semi-automated PM operations. 

**Future Work includes:**
1. Finalizing the AI model boundary conditions and fully integrating it into the C# simulator.
2. Procuring the cost-effective ABB/UR cobot to test the software alongside live OHT sensors.
3. Prototyping the DIY magnetic plug system and its associated electrical relay circuits for potential fab-wide deployment.
