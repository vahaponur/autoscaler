# Auto Scaler for Unity

A Unity tool that automatically scales GameObjects to fit a target size. Particularly useful for normalizing imported 3D models to consistent sizes.

## Features

- **Automatic Scaling**: Automatically scales GameObjects based on their renderer bounds
- **Editor Integration**: Custom inspector with real-time size display and manual fit button
- **Auto-Attach**: Optionally auto-attaches to GameObjects with renderers when they're added to the scene
- **Undo Support**: Full Unity undo/redo support for all operations
- **Gizmo Visualization**: Visual bounds display in Scene view when selected
- **Single Instance**: Prevents multiple AutoScaler components on the same GameObject
- **Child Control**: Option to include or exclude child renderers in scaling calculations

## Installation

### Option 1: Unity Package Manager (Git URL)
1. Open the Package Manager window (Window > Package Manager)
2. Click the '+' button and select "Add package from git URL"
3. Enter: `https://github.com/vahaponur/autoscaler.git`

### Option 2: Local Installation
1. Download this package
2. Extract to `YourProject/Packages/com.vahaponur.autoscaler`

### Option 3: Manual Installation
1. Copy the contents to your Assets folder

## Usage

### Basic Usage
1. Add the `AutoScaler` component to any GameObject with a Renderer
2. Set the `Target Size` field to your desired size (in meters)
3. The object will automatically scale to fit

### Manual Scaling
- Click the "Fit Now" button in the inspector to manually trigger scaling
- Use the context menu (right-click on component header) and select "Fit To Target"

### Auto-Attach Feature
The package includes an auto-attach feature that automatically adds the AutoScaler component to GameObjects with renderers when they're added to the scene. This is enabled by default.

#### Toggling Auto-Attach:
- Go to **Tools > AutoScaler > Auto-Attach Enabled** in the Unity menu
- Click to toggle on/off (checkmark indicates enabled)
- Settings are saved per-project in `ProjectSettings/AutoScalerSettings.json`

#### Git Integration:
If you want project members to have their own auto-attach preferences:
- Go to **Tools > AutoScaler > Add Settings to .gitignore**
- This will add the settings file to your .gitignore
- Each team member can then have their own preference

**Note**: If no .gitignore file exists, you'll see a warning. The tool won't create a .gitignore file automatically.

## Component Properties

- **Target Size**: The desired size of the largest dimension (width, height, or depth) in meters
- **Include Children**: Whether to include child renderers in the bounds calculation (default: true)

## Inspector Features

- **Real-time Size Display**: Shows the actual world-space dimensions of the object
- **Fit Now Button**: Manually trigger the scaling operation
- **Visual Gizmos**: Yellow wireframe box showing the object's bounds in Scene view

## Requirements

- Unity 2021.3 or higher
- Works with any GameObject that has a Renderer component

## License

This package is released under the MIT License. See LICENSE.md for details.

## Support

For issues, questions, or contributions, please visit:
https://github.com/vahaponur/autoscaler