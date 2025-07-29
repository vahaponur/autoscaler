# AutoScaler Basic Usage Sample

This sample demonstrates the basic usage of the AutoScaler component.

## Contents

- **AutoScalerExample.unity**: A sample scene with various GameObjects demonstrating the AutoScaler functionality
- **ExamplePrefabs/**: Folder containing prefabs with pre-configured AutoScaler components

## How to Use

1. Import this sample through the Package Manager
2. Open the `AutoScalerExample.unity` scene
3. Select any GameObject with the AutoScaler component to see it in action
4. Try changing the `Target Size` value to see objects automatically rescale
5. Click "Fit Now" in the inspector to manually trigger scaling

## Examples Included

1. **Basic Cube**: Simple primitive with AutoScaler set to 1 meter
2. **Complex Model**: Imported 3D model normalized to 2 meters
3. **Nested Objects**: Demonstration of AutoScaler working with child objects

## Tips

- The AutoScaler calculates bounds based on all child renderers
- Yellow gizmos in Scene view show the calculated bounds
- Use the auto-attach feature to automatically add AutoScaler to new objects