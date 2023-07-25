/* 
*   NatRender
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.Rendering {

    using System;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Scripting;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;

    /// <summary>
    /// Readback provider for OpenGL ES.
    /// This provider is only supported on Android when running with OpenGL ES3.
    /// </summary>
    public sealed class GLESReadback : AndroidJavaProxy, IReadback {

        #region --Client API--
        private Material material;

        /// <summary>
        /// Create a readback provider for OpenGL ES.
        /// </summary>
        /// <param name="width">Output pixel buffer width.</param>
        /// <param name="height">Output pixel buffer height.</param>
        /// <param name="multithreading">Use multithreading. Enabling this will typically increase performance.</param>
        public GLESReadback (int width, int height, Material material, bool multithreading = false) : base(@"api.natsuite.natrender.Readback$Callback") {
            // Get EGL context
            var egl = new AndroidJavaClass(@"android.opengl.EGL14");
            var contextTask = new TaskCompletionSource<AndroidJavaObject>();
            RenderThread.Run(() => {
                AndroidJNI.AttachCurrentThread();
                var context = egl.CallStatic<AndroidJavaObject>(@"eglGetCurrentContext");
                contextTask.SetResult(context);
            });
            this.material = material;
            var eglContext = contextTask.Task.Result;
            // Create readback
            const string className = @"api.natsuite.natrender.GLReadback";
            this.readback = new AndroidJavaObject(className, width, height, eglContext, this, multithreading);
            this.clazz = new AndroidJavaClass(className);
            this.frameBuffer = new RenderTexture(width, height, 0, RenderTextureFormat.Default);
            this.frameBuffer.Create();
            this.readbackTexture = frameBuffer.GetNativeTexturePtr();
            this.bufferSize = width * height * 4;
            // JNI
            this.classPtr = readback.GetRawClass();
            this.readBackMethodID = AndroidJNIHelper.GetMethodID(classPtr, @"readback", @"(IJ)V", false);
            this.baseAddressMethodID = AndroidJNIHelper.GetMethodID(classPtr, @"baseAddress", @"(Ljava/lang/Object;)J", true);
        }

        /// <summary>
        /// Request a readback.
        /// </summary>
        /// <param name="texture">Input texture.</param>
        /// <param name="handler">Readback handler.</param>
        public unsafe void Request (Texture texture, Action<NativeArray<byte>> handler) {
            // Blit
            Graphics.Blit(texture, frameBuffer, material);
            // Readback
            RenderThread.Run(() => {
                readBackParameters[0].i = readbackTexture.ToInt32();
                readBackParameters[1].j = (long)(IntPtr)GCHandle.Alloc(handler, GCHandleType.Normal);
                AndroidJNI.CallVoidMethod(readback.GetRawObject(), readBackMethodID, readBackParameters);
            });
        }

        /// <summary>
        /// Dispose the readback provider.
        /// </summary>
        public async void Dispose () {
            readback.Call(@"release");
            await Task.Yield();
            frameBuffer.Release();
        }
        #endregion


        #region --Operations--
        private readonly AndroidJavaObject readback;
        private readonly AndroidJavaClass clazz;
        private readonly RenderTexture frameBuffer;
        private readonly IntPtr readbackTexture;
        private readonly int bufferSize;

        // JNI params to avoid GC alloc, thanks @nolkeg :D
        private readonly IntPtr classPtr;
        private readonly IntPtr readBackMethodID;
        private readonly IntPtr baseAddressMethodID;
        private readonly jvalue[] readBackParameters = new jvalue[2];
        private readonly jvalue[] baseAddressParameter = new jvalue[1];
        
        [Preserve]
        private unsafe void onReadback (long context, AndroidJavaObject pixelBuffer) {
            // Get handlerr
            var handle = (GCHandle)(IntPtr)context;
            var handler = handle.Target as Action<NativeArray<byte>>;
            handle.Free();
            // Get base address
            baseAddressParameter[0].l = pixelBuffer.GetRawObject();
            var baseAddress = (void*)(IntPtr)AndroidJNI.CallStaticLongMethod(classPtr, baseAddressMethodID, baseAddressParameter);
            // Invoke handler
            var nativeArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(baseAddress, bufferSize, Allocator.None);
            handler?.Invoke(nativeArray);
        }
        #endregion
    }
}