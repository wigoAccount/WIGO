# NatCorder Performance Extensions
Performance extensions for video recording in Unity Engine.

## Setup Instructions
To install the library, add the following to your project's `manifest.json` file in the `Packages` folder:
```json
{
  "scopedRegistries": [
    {
      "name": "NatSuite Framework",
      "url": "https://registry.npmjs.com",
      "scopes": ["api.natsuite"]
    }
  ],
  "dependencies": {
    "api.natsuite.ncpx": "0.0.2"
  }
}
```

## High Performance Recording
NCPX provides highly optimized recorder inputs that offer inexpensive [pixel buffer readbacks](https://docs.natsuite.io/natcorder/workflows/recording-rendertextures) from `Texture` objects, and simultaneously offer [multithreaded recording](https://docs.natsuite.io/natcorder/workflows/performance-considerations#multithreaded-recording).

They can be used independently, or attached to a `CameraInput` when recording game cameras:
```csharp
// Create recorder and recording clock
var recorder = ...;
var clock = ...;
// Create a camera input
var cameraInput = new CameraInput(recorder, clock, cameras);
// Attach optimized texture input from NCPX to the camera input
cameraInput.textureInput = new GLESTextureInput(recorder, multithreading: true);
```

NCPX provides `GLESTextureInput` for OpenGL ES3 on Android.

## Watermark Recording
*INCOMPLETE*

## Cropped Recording
*INCOMPLETE*

## Recording to a Specific File
*INCOMPLETE*

___

## Requirements
- Unity 2019.2+
- NatCorder 1.8.0+

## Resources
- [NatCorder Documentation](https://docs.natsuite.io/natcorder)
- [NatSuite Framework](https://github.com/natsuite)
- [Email Support](mailto:hi@natsuite.io)

Thank you very much!