namespace NatSuite.Recorders
{

    using AOT;
    using System;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using Internal;
    using System.Threading;
    using UnityEngine;

    /// <summary>
    /// WEBM video recorder.
    /// </summary>
    public class WEBRecorder
    {

        private readonly IntPtr recorder;

        #region --Client API--
        /// <summary>
        /// Video size.
        /// </summary>
        public (int width, int height) frameSize
        {
            get
            {
                recorder.FrameSize(out var width, out var height);
                return (width, height);
            }
        }

        /// <summary>
        /// Create a WEBM recorder.
        /// </summary>
        /// <param name="width">Video width.</param>
        /// <param name="height">Video height.</param>
        /// <param name="frameRate">Video frame rate.</param>
        /// <param name="sampleRate">Audio sample rate. Pass 0 for no audio.</param>
        /// <param name="channelCount">Audio channel count. Pass 0 for no audio.</param>
        /// <param name="bitrate">Video bitrate in bits per second.</param>
        public WEBRecorder(int width, int height, float frameRate, string audioPath, float recordLength, int bitrate = (int)(960 * 540 * 11.4))
        {
            recorder = WebBridge.CreateRecorder(width, height, frameRate, bitrate, audioPath, recordLength);
        }

        /// <summary>
        /// Commit a video pixel buffer for encoding.
        /// The pixel buffer MUST have an RGBA8888 pixel layout.
        /// </summary>
        /// <param name="pixelBuffer">Pixel buffer containing video frame to commit.</param>
        public async void CommitFrame<T>(T[] pixelBuffer) where T : struct
        {
            var handle = GCHandle.Alloc(pixelBuffer, GCHandleType.Pinned);
            await CommitFrame(handle.AddrOfPinnedObject());
            handle.Free();
        }

        /// <summary>
        /// Commit a video pixel buffer for encoding.
        /// The pixel buffer MUST have an RGBA8888 pixel layout.
        /// </summary>
        /// <param name="nativeBuffer">Pixel buffer in native memory to commit.</param>
        public async Task CommitFrame(IntPtr nativeBuffer)
        {
            recorder.CommitFrame(nativeBuffer);
        }

        /// <summary>
        /// Finish writing and return the path to the recorded media file.
        /// </summary>
        public void FinishWriting()
        {
            recorder.FinishWriting();
        }

        public void SaveVideo()
        {
            WebBridge.SaveVideo();
        }

        public void PlayVideo()
        {
            WebBridge.PlayVideo();
        }
        #endregion
    }
}