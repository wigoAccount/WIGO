/* 
*   NatCorder
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.Recorders.Inputs {

    using UnityEngine;
    using UnityEngine.Rendering;
    using Unity.Collections.LowLevel.Unsafe;

    /// <summary>
    /// Recorder input for recording video frames from textures.
    /// Textures will be recorded by performing an asynchronous pixel buffer readback.
    /// This texture input typically has better performance, but is not supported on all platforms.
    /// Check for support using `SystemInfo.supportsAsyncGPURReadback`.
    /// </summary>
    public sealed class AsyncTextureInput : ITextureInput {

        #region --Client API--
        /// <summary>
        /// Create a texture input which performs asynchronous readbacks.
        /// </summary>
        /// <param name="recorder">Media recorder to receive video frames.</param>
        public AsyncTextureInput (IMediaRecorder recorder) => this.recorder = recorder;

        /// <summary>
        /// Commit a video frame from a texture.
        /// </summary>
        /// <param name="texture">Source texture.</param>
        /// <param name="timestamp">Frame timestamp in nanoseconds.</param>
        public unsafe void CommitFrame (Texture texture, long timestamp) {
            // Blit
            var (width, height) = recorder.frameSize;
            var renderTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
            Graphics.Blit(texture, renderTexture);
            // Readback
            AsyncGPUReadback.Request(renderTexture, 0, request => recorder?.CommitFrame(
                request.GetData<byte>().GetUnsafeReadOnlyPtr(),
                timestamp
            ));
            RenderTexture.ReleaseTemporary(renderTexture);
        }

        public unsafe void CommitFrame(RenderTexture texture, long timestamp)
        {
            // Readback
            AsyncGPUReadback.Request(texture, 0, request => recorder?.CommitFrame(
                request.GetData<byte>().GetUnsafeReadOnlyPtr(),
                timestamp
            ));
        }

        /// <summary>
        /// Stop recorder input and release resources.
        /// </summary>
        public void Dispose () => recorder = null;
        #endregion


        #region --Operations--
        private IMediaRecorder recorder;
        (int, int) ITextureInput.frameSize => recorder.frameSize;
        #endregion
    }
}