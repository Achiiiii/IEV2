# RedPill VideoPose Plugin

## Build Sample Scene
1. Create a new Unity project
2. Import RedPill VideoPose package in Package Manager using "Add package from disk"
    - choose package.json inside com.redpill.videopose folder
3. Import Samples/Demo
    - from Sample tab in Package Manager
4. Configure Player Settings
    - Specify "Default Orientation"
        * "Auto Rotation" is not supported
    - Change "Company Name" and "Product Name"
    - Fill "Signing Team ID"
        * Or, after build, configure inside xcode
    - Check "Allow 'unsafe' code"
    - Fill "Camera Usage Description"
5. Configure Build Settings
    - Switch to iOS platform
6. Build and Run
    - Choose a scene you want to run

## Sample Scenes Content
- VRM
    * simple demo of motion capture
    * toggle "Upper Body Mode" from editor to switch between full body and upper body mode
    * toggle "Detect A Pose" to enable/disable A-pose detection in full body mode
        - disabling it may cause imprecise global position
    * full body mode
        - Do an A-pose to start if "Detect A Pose" is set
        - make sure you are fully in the camera view
    * upper body mode
        - no calibration required 
        - at least 1 meter away from camera
    * flip
        - X=-1 flips image horizontally