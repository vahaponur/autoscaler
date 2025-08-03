# Changelog

## [1.4.2] - 2025-08-03

### Fixed
- Translated all Turkish text to English for global usage
- All tooltips now in English
- All code comments now in English

## [1.4.1] - 2025-08-03

### Fixed
- Rotation-independent size calculation - objects now scale correctly regardless of rotation
- Size mismatch warnings no longer appear when objects are rotated
- Scale validation now uses local bounds instead of world bounds

### Added
- GetLocalBounds() method for accurate size measurement independent of object rotation

### Improved
- Scale calculations now work correctly for rotated objects
- More accurate size reporting in editor and gizmos

## [1.4.0] - 2025-07-29

### Added
- Preset system integration in AutoScaler component
- Quick preset buttons (Small, Medium, Large)
- Transform mismatch warning system
- Smart scale validation
- Preset management in batch processor

### Changed
- Default scale anchor is now Center
- Improved batch processor UI with tabs
- Better visual feedback for scale mismatches
- Cleaner component inspector

### Removed
- Category system (replaced by presets)
- Script field from inspector

### Fixed
- Collection modification error in batch processor
- Button styling in batch processor
- Scale validation logic

## [1.3.1] - 2025-07-29

### Changed
- Renamed "Pivot" to "Scale Anchor" for clarity
- Removed redundant "Fit Now" button (auto-scales on value change)
- Improved inspector UI with W/H/D labels for dimensions
- Scale anchor offset auto-updates when preset changes
- Preset auto-detects from manual offset values

### Fixed
- Removed duplicate Scale Anchor header in inspector
- Fixed namespace conflicts with global:: prefix

## [1.3.0] - 2025-07-29

### Added
- Batch processing with custom presets
- Smart pivot adjustment with position preservation
- Visual reference objects
- Asset categories system
- Category management window
- Quick category assignment in inspector
- Height display in inspector

## [1.2.0] - 2025-07-29

### Added
- Auto-attach toggle in Unity menu
- Project-based settings system
- Option to add settings file to .gitignore

### Changed
- Auto-attach is now optional but enabled by default
- Settings are stored per-project

## [1.1.0] - 2025-07-29

### Added
- `includeChildren` property to control child renderer inclusion
- `[DisallowMultipleComponent]` attribute

### Fixed
- Prevented multiple AutoScaler components on same GameObject

## [1.0.0] - 2025-07-29

### Added
- Initial release
- Core AutoScaler component
- Custom editor with real-time size display
- Auto-attach feature
- Scene view gizmos
- Full undo/redo support