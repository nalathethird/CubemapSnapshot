# Cubemap Snapshot for Resonite

A Unity tool for exporting Reflection Probes or creating new ones, to Resonite-compatible cubemaps.
This tool makes it easy to convert Unity's Reflection Probes into the format needed for Resonite's cubemap system.

## Features

- Export Reflection Probes to cubemap images
- Support for PNG (recommended), JPG, and experimental WebP formats
- Automatic folder management per already Baked Reflection Probe
- Preview of captured faces
- User-friendly Unity Editor interface
- Automatic scene pausing during capture
- Cubemap Image Resolution Slider - 256 to 8192
  - **Note**: Resolutions above 4K (4096) require significant VRAM and are not recommended for most use cases
  - High resolutions may cause Unity to become unresponsive during capture

## Requirements

- Unity 2022.3 or newer
- (Optional) A Reflection Probe/s in your scene (baked or realtime)
- Sufficient VRAM for your chosen resolution

## Installation

1. Download the latest release from the [Releases](https://github.com/tgrafk12/CubemapSnapshot/releases) page
2. Import the package into your Unity project
3. Add the CubemapSnapshot component to any GameObject in your scene
4. Enter Play-mode to activate the script and be able to capture! 

## Usage

1. Add the CubemapSnapshot component to a GameObject
2. Configure your desired settings:
   - Resolution: Choose based on your needs (256-8192)
   - Format: PNG (recommended), JPG, or WebP (experimental)
   - Output folder will be automatically managed
3. Enter Play Mode
4. Use the Export button to save your Reflection Probe's cubemap
5. The cubemap will be saved in the specified output folder, organized by probe name

### Format Recommendations

- **PNG**: Recommended format. Provides the best quality with lossless compression
- **JPG**: Good alternative when file size is a concern
- **WebP**: Currently in experimental/beta status. May have quality issues in some cases

## Credits

- Created by [tgrafk12](https://github.com/tgrafk12)
- WebP support powered by [libwebp](https://developers.google.com/speed/webp/docs/api) by Google
- Special thanks to the Resonite community for testing and feedback

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Reporting Issues

If you encounter any issues, please:
1. Check the [Issues](https://github.com/tgrafk12/CubemapSnapshot/issues) page to see if it's already reported
2. If not, create a new issue with:
   - Unity version
   - Steps to reproduce
   - Expected vs actual behavior
   - Screenshots if applicable
   - Export settings used (resolution, format)
   - Steps taken to Import Images to Resonite

## Known Issues

- WebP format is currently in experimental/beta status and may have quality issues
- Very high resolutions (>4K) may cause temporary Unity unresponsiveness during capture
- Scene must be in Play Mode to capture cubemaps