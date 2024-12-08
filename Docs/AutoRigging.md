# Auto-Rigging System

The Auto-Rigging system provides tools to automatically setup IK chains on humanoid or custom rigs.

## Components

### AutoRig
Main component for automatic IK setup.

#### Important
- This component should be placed on the root of the character
- The algorithm is not perfect and might require manual adjustments !!!

#### Settings
- `maxChainLength`: Maximum bones in an IK chain
- `createTargets`: Create IK target objects
- `targetSuffix`: Suffix for target object names
- `ikType`: Type of IK solver to use

#### Usage
1. Add `AutoRig` component to the root of your character
2. Configure settings in the inspector
3. Click "Setup IK" button

### Target Management
- Targets are created in a separate "IK_Targets" container
- Each IK chain gets its own target

## Best Practices
1. Clean hierarchy before setup
2. Use consistent naming conventions
3. Keep chain lengths reasonable (2-4 bones)
4. Use appropriate solver for each chain type 