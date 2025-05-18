# CubemapMaker for Resonite

A Unity tool for exporting Reflection Probes to Resonite-compatible cubemaps.

## Features

- Export Unity Reflection Probes to cubemap images
- Support for PNG, JPG, and WebP formats
- Automatic folder organization
- Preview capture results
- Advanced capture settings
- Debug logging options

## Requirements

- Unity 2020.3 or newer
- Resonite (for using the exported cubemaps)

## Installation

1. Clone this repository into your Unity project's Assets folder
2. The tool will be available in the Unity Editor

## Usage

1. Add the `CubemapSnapshot` component to any GameObject with a Reflection Probe
2. Configure the capture settings in the inspector
3. Click "Capture Cubemap" to generate the cubemap
4. Find the exported files in the `CubemapOutput` folder

## Settings

### Format Settings
- **Image Format**: Choose between PNG, JPG, or WebP
- **Quality**: Adjust compression quality for JPG and WebP formats

### Capture Settings
- **Resolution**: Set the cubemap resolution (recommended: 2048)
- **Include Skybox**: Toggle skybox inclusion in the capture
- **Culling Mask**: Configure which layers to include in the capture

### Advanced Settings
- **Show Debug Logs**: Enable detailed logging
- **Show Capture Preview**: Preview the capture results

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.