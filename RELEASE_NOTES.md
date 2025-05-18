# Release Notes - CubemapSnapshot v1.0.0

## Overview
CubemapSnapshot is a Unity tool that enables easy export of Reflection Probes to Resonite-compatible cubemaps. This release focuses on providing a stable, user-friendly experience with support for multiple image formats and automatic scene management.

## Major Features

### Core Functionality
- Export Unity Reflection Probes to Resonite-compatible cubemaps
- Support for multiple image formats:
  - PNG (recommended, lossless)
  - JPG (lossy compression)
  - WebP (experimental/beta)
- Resolution options from 256 to 8192 pixels
- Automatic folder management for organized exports
- Preview of captured cubemap faces
- Automatic scene pausing during capture

### User Interface
- Clean, professional Unity Editor interface
- Informative popups for resolution and format choices
- Warning dialogs for high-resolution captures
- Resolution guide with recommended settings
- Advanced settings panel for debugging and preview options
- Export settings confirmation dialog

### Technical Improvements
- Automatic scene pausing using EditorApplication.update
- Proper texture format handling for all supported formats
- Camera capture timing optimizations
- Progress feedback during capture
- Fixed WebP image orientation issues
- Proper pause/unpause functionality

## Known Limitations
- WebP format is currently in experimental/beta status
- High resolutions (>4K) may cause temporary Unity unresponsiveness
- Scene must be in Play Mode to capture cubemaps
- WebP quality may vary depending on content

## System Requirements
- Unity 2022.3 or newer

## Installation
1. Download the latest release from the Releases page
2. Import the package into your Unity project
3. Add the CubemapSnapshot component to any GameObject
4. Enter Play Mode to activate the script

## Usage Notes
- PNG format is recommended for best quality
- 2K (2048) resolution is recommended for most use cases
- Higher resolutions should be used with caution
- WebP format should be used for testing only
- Automatic scene pausing ensures consistent captures

## Bug Fixes
- Fixed texture format mismatches
- Resolved camera capture timing issues
- Corrected WebP image orientation
- Fixed pause/unpause functionality
- Addressed high-resolution capture stability

## Future Improvements
- Enhanced WebP support and quality
- Additional format support
- Performance optimizations for high resolutions
- More advanced preview options
- Batch processing capabilities

## Credits
- Created by tgrafk12
- WebP support powered by libwebp by Google
- Special thanks to the Resonite community for testing and feedback

## Support
For issues, feature requests, or questions:
1. Check the Issues page for existing reports
2. Create a new issue with detailed information
3. Include Unity version, steps to reproduce, and export settings used 