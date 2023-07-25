/* 
*   NatCorder
*   Copyright (c) 2020 Yusuf Olokoba.
*/

namespace NatSuite.Recorders.Internal
{
    using System;
    using System.Runtime.InteropServices;

    public static class WebBridge
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        private const string Assembly = @"__Internal";
        
        [DllImport(Assembly, EntryPoint = @"NCCreateRecorder")]
        public static extern IntPtr CreateRecorder (int width, int height, float frameRate, int bitrate, string audioPath, float recordLength);
        [DllImport(Assembly, EntryPoint = @"NCFrameSize")]
        public static extern void FrameSize (this IntPtr recorder, out int width, out int height);
        [DllImport(Assembly, EntryPoint = @"NCCommitFrame")]
        public static extern void CommitFrame (this IntPtr recorder, IntPtr pixelBuffer);
        [DllImport(Assembly, EntryPoint = @"NCFinishWriting")]
        public static extern void FinishWriting (this IntPtr recorder);
        [DllImport(Assembly, EntryPoint = @"NCSaveVideo")]
        public static extern void SaveVideo();
        [DllImport(Assembly, EntryPoint = @"NCPlayVideo")]
        public static extern void PlayVideo();
#else
        public static IntPtr CreateRecorder(int width, int height, float frameRate, int bitrate, string audioPath, float recordLength) => IntPtr.Zero;
        //public static void FrameSize (this IntPtr recorder, out int width, out int height) { width = height = 0; }
        public static void CommitFrame(this IntPtr recorder, IntPtr pixelBuffer) { }
        public static void FinishWriting(this IntPtr recorder) { }
        public static void SaveVideo() { }
        public static void PlayVideo() { }
#endif
    }
}