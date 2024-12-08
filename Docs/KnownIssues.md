# Known Issues

## Character Controller
- Character Controller movement doesn't work as intended
  - **Description**:The character movement system needs refinement for better responsiveness and control

## IK System
- Head IK following camera rotation is not working correctly
  - **Description**: Head IK target doesn't properly follow camera rotation. This might be caused by test rig setup or IK solver limitations
  - **Note**: Issue might be rig-specific, needs further testing with different character rigs

## General Limitations
- IK solvers might behave unexpectedly with complex character rigs
- No support for blending between different IK states