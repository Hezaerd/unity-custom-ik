# IK Solvers

## Available Solvers

### FABRIK Solver
The FABRIK (Forward And Backward Reaching Inverse Kinematics) solver provides a fast and reliable solution for IK chains.

#### Properties
- `chainLength`: Length of the IK chain
- `Target`: Target transform for the end effector
- `Pole`: Optional pole target for knee/elbow control
- `maxIterations`: Maximum solver iterations
- `delta`: Acceptable error threshold
- `snapBackWeight`: Weight of the original pose

#### Usage 