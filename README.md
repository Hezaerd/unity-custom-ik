# Unity IK System

A modular Inverse Kinematics (IK) system for Unity, featuring multiple IK solvers and an auto-rigging system.

## Features

- Multiple IK Solvers:
  - FABRIK (Forward And Backward Reaching Inverse Kinematics)
  - CCD (Cyclic Coordinate Descent)
- Auto-rigging system for quick IK setup
- Prototype of a Third-person character controller with IK integration
- Runtime IK target manipulation
- Ground adaptation for foot placement
- ~Head tracking system
- ~Modular and extensible architecture

## Getting Started

### Prerequisites

- Unity 2022.3.34f1 or higher

### Installation

1. Clone this repository or download the latest release
2. Import the package into your Unity project

### Quick Setup

1. Add your character model to the scene
2. Add the `ThirdPersonCamera` component to the camera
3. Add the `IKCharacterController` component to your character
4. Use the `AutoRig` component to automatically setup IK chains:
   - Select your character
   - Add the `AutoRig` component
   - Choose your preferred IK solver type
   - Click "Setup IK"


### Known Issues

The complete list of known issues can be found [here](Docs/KnownIssues.md).
