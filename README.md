# AutoScaler for Unity

A Unity tool that automatically scales GameObjects to real-world sizes.

## Installation

### Unity Package Manager
```
https://github.com/vahaponur/autoscaler.git
```

## Usage

### Basic Usage
1. Add `AutoScaler` component to any GameObject
2. Set target size in meters
3. Object scales automatically

### Using Presets
1. Select a preset from dropdown (Small, Medium, Large, or custom presets)
2. Object instantly scales to preset size

### Batch Processing
- **Tools > AutoScaler > Batch Process Selected**
- Scale multiple objects at once
- Create and manage custom presets

### Auto Attach (Optional)
- **Tools > AutoScaler > Toggle Auto-Attach**
- When enabled, automatically adds AutoScaler component to new GameObjects with renderers
- Useful for rapid prototyping and importing 3D models
- Settings are saved per project

## Features

- ✅ Real-time automatic scaling
- ✅ Rotation-independent scaling (works correctly with rotated objects)
- ✅ Preset system for quick sizing
- ✅ Batch processing for multiple objects
- ✅ Scale anchors (keep position while scaling)
- ✅ Include/exclude child objects
- ✅ Visual warnings for scale mismatches
- ✅ Auto-attach to new objects (optional)
- ✅ Undo/Redo support

## Requirements

Unity 2021.3 or higher

## License

MIT