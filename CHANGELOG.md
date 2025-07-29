# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.2.1] - 2025-07-29

### Removed
- Removed samples folder to keep package minimal

## [1.2.0] - 2025-07-29

### Added
- Auto-attach toggle in Unity menu (Tools > AutoScaler > Auto-Attach Enabled)
- Project-based settings system for auto-attach preference
- Option to add settings file to .gitignore for team workflows
- Menu item to manage .gitignore integration

### Changed
- Auto-attach is now optional but enabled by default
- Settings are stored per-project in ProjectSettings folder

## [1.1.0] - 2025-07-29

### Added
- `includeChildren` property to control whether child renderers are included in bounds calculation
- `[DisallowMultipleComponent]` attribute to prevent multiple AutoScaler components on the same GameObject

### Changed
- Default behavior now includes child renderers (backward compatible)
- Improved documentation with new features

### Fixed
- Prevented multiple AutoScaler components on the same GameObject

## [1.0.0] - 2025-07-29

### Added
- Initial release of Auto Scaler package
- Core AutoScaler component for automatic GameObject scaling
- Custom editor with "Fit Now" button and real-time size display
- Auto-attach feature for GameObjects with Renderer components
- Scene view gizmos for visualizing object bounds
- Full undo/redo support
- Assembly definitions for proper package structure
- Sample scene demonstrating basic usage