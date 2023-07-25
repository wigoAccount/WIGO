# NatRender
NatRender is a lightweight graphics utility library for Unity Engine. Features include:
- High performance pixel buffer readbacks from the GPU on iOS and Android (including OpenGL ES3).
- Running delegates on Unity's render thread.

## Setup Instructions
NatShare should be installed using the Unity Package Manager. In your `manifest.json` file, add the following dependency:
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
    "api.natsuite.natrender": "1.0.1"
  }
}
```

## Pixel Buffer Readbacks
NatRender provides lightweight primitives for performing pixel buffer readbacks from textures on the GPU. This is exposed through implementations the `IReadback` interface. Currently, NatRender provides the following implementations:
- `GLESReadback` for Android when rendering with OpenGL ES3.

Once you create a readback, you can request the pixel data by calling the `Readback` method:
```csharp
// Starting with some texture you want to readback
var texture = ...;
// Create readback
var readback = new GLESReadback(...);
// Issue request
readback.Request(texture, (NativeArray<byte> pixelBuffer) => {
    // Do stuff with pixel buffer

    // And when you're done remember to dispose
    readback.Dispose();
});
```

## Running on the Render Thread
NatRender also provides a way to run an `Action` on Unity's render thread. This is useful for performing native rendering or doing anything that requires being on the primary render thread:
```csharp
RenderThread.Run(() => {
    // We're on Unity's render thread
    Debug.Log("Hello from the render thread");
});
```

___

## Requirements
- Unity 2019.2+
- Android API level 24+
- iOS 13+

## Resources
- [NatRender Documentation](https://docs.natsuite.io/natrender)
- [NatSuite Framework on GitHub](https://github.com/natsuite)
- [Email Support](mailto:yusuf@natsuite.io)

Thank you very much!