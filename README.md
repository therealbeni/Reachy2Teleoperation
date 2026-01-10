# Dance Reachy! 🕺🤖

**Dance Reachy!** is a mixed reality teleoperation and interaction system for the **Reachy 2 humanoid robot**, developed as a semester project.  
The project extends the official Reachy 2 teleoperation stack with expressive, game-inspired interaction modes designed for public demonstrations and exploratory human–robot interaction.

Using a VR/MR headset and Unity, users can:
- Teleoperate Reachy expressively through full-body movement
- Imitate prerecorded robot dance routines with real-time scoring
- Perform goal-oriented tabletop manipulation tasks

The system focuses on **engagement, expressiveness, and usability**, rather than industrial precision or autonomous control.

---

## Project Overview

This project builds on the official Reachy 2 teleoperation pipeline and introduces:

- A **Unity-based mixed reality frontend** running on VR/MR headsets
- **Three interaction modes**:
  1. **Dance as Reachy** – expressive real-time teleoperation  
  2. **Dance after Reachy** – dance imitation with scoring and feedback  
  3. **Tabletop Game** – goal-oriented teleoperation task
- A **motion playback and scoring pipeline** for prerecorded robot motions
- Significant **refactoring and stabilization** of the original teleoperation backend to support multiple modes safely and predictably

The system was evaluated during a **live public demonstration** and through post-demo user questionnaires.

---

## Hardware and Software Requirements

### Hardware
- Reachy 2 humanoid robot
- VR/MR headset compatible with Unity XR  
  *(Tested with Meta Quest 2 and Meta Quest 3)*
- Windows PC capable of running Unity and VR streaming

### Software
- **Unity LTS 2022.3** (recommended)
- Oculus Link (for Meta Quest headsets)
- GStreamer (required for robot communication)

---

## Installation

### 1. Clone the Repository

```bash
git clone --recurse-submodules https://github.com/YOUR_ORG_OR_USERNAME/DanceReachy.git
```
Note: Git LFS must be enabled, as the project contains large binary assets.

## 2. Install GStreamer (Windows)

If you are not using a bundled installer, install:

- GStreamer Windows Runtime (x86_64)
- GStreamer Development Files (x86_64)

After installation, ensure that the following path is present in your system PATH variable:

C:\gstreamer\1.0\msvc_x86_64\bin

Reboot the system after installation.

## 3. Open the Project in Unity

- Open **Unity Hub**
- Select **Unity LTS 2022.3**
- Open the cloned project directory

In the Unity Editor:

- Go to **Edit → Project Settings → XR Plugin Management**
- Enable the plugin corresponding to your VR/MR headset

---

## Running the System

### 1. Connect to the Robot

- Ensure the Reachy 2 robot is powered on and connected to the same network
- Obtain the robot’s IP address
- Enter the IP address in the application to establish a connection

A virtual robot is also available for local testing without hardware.

### 2. Select an Interaction Mode

From the main menu, choose one of the following modes:

- **Dance as Reachy** – live expressive teleoperation
- **Dance after Reachy** – observe and imitate a prerecorded dance routine
- **Tabletop Game** – recreate a target block configuration using teleoperation

Each mode reuses the same underlying teleoperation pipeline with mode-specific logic.

### 3. Safety Notes

- The system applies basic safety constraints such as joint limits and unreachable pose rejection
- Collision detection and avoidance are not implemented
- Teleoperation modes should only be used in


