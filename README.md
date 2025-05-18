# CubemapSnapshot for Resonite

A Unity tool for exporting Reflection Probes to Resonite-compatible cubemaps. This tool makes it easy to convert Unity's Reflection Probes into the format needed for Resonite's cubemap system.
OR
If you just want a Pretty Photo of your Unity Scene!
This is made for Unity Editor: Intended for VRChat Maps you own, or created yourself! It brings over important reflection data for other games, or, just to see the map for yourself!

## Features

- Export Reflection Probes to cubemap images - for Resonite's Cubemap Creator
- Support for PNG (recommended), JPG, and WebP formats
- Automatic folder management for captured Cubemap Photos
- Preview of captured faces
- User-friendly Unity Editor interface
- Cubemap Image Resolution Slider - 256 to 8192
  - **Note**: Resolutions above 4K (4096) require significant VRAM and are not recommended for most use cases
  - High resolutions may cause Unity to become unresponsive during capture - its normal!

## Requirements

- Unity 2022.3 or newer
- (Optional) A Reflection Probe/s in your scene (baked or realtime)
- Sufficient RAM for your chosen resolution

## Installation

1. Download the latest release from the [Releases](https://github.com/nalathethird/CubemapSnapshot/releases/latest) page
2. Import the package into your Unity project

## How To Use

1. Add the CubemapSnapshot component to any GameObject in your scene - with or without a Reflection Probe on the GameObject
2. Configure your desired settings:
   - Resolution: Choose based on your needs (256-8192)
   - Format: PNG (recommended), JPG, or WebP (experimental)
   - Output folder will be automatically managed
3. Enter Play-mode to activate the script
4. Press "Capture Cubemap" to run the script!

### Format Recommendations

- **PNG**: Recommended format. Provides the best quality with lossless compression
- **JPG**: Good alternative when file size is a concern
- **WebP**: Currently in experimental/beta status. May have quality issues in some cases -  feel free to make a pull request if you can fix this!

## Credits

- Created by [nalathethird](https://github.com/nalathethird)
- WebP support powered by [libwebp](https://developers.google.com/speed/webp/docs/api) by Google
- Special thanks to the Resonite for the PressKit Assets for the UI images, and their community for testing and feedback

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Reporting Issues

If you encounter any issues, please:
1. Check the [Issues](https://github.com/nalathethird/CubemapSnapshot/issues) page to see if it's already reported
2. If not, create a new issue with:
   - Unity version
   - Steps to reproduce
   - Expected vs actual behavior
   - Screenshots if applicable
   - Export settings used (resolution, format)
   - Errors in Unity Console
   - (Optional) Steps taken to Import Images to Resonite

## Known Issues

- WebP format is currently in experimental/beta status and may have quality issues - Again, make a pull request if you can fix this issue!