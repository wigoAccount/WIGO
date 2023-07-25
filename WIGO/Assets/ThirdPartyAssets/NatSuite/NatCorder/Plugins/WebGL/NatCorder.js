const NatCorder = {
    $sharedInstance: [],

    NCCreateRecorder: function (width, height, frameRate, bitrate, audioPathPtr, recordLength) {
        window["frameBuffer"] = GLctx.createFramebuffer();
        window["gPixels"] = new Uint8Array(4 * width * height);
        const audioPath = Pointer_stringify(audioPathPtr);
        this.width = width;
        this.height = height;
        initRecordVideo(width, height, frameRate, bitrate);

        const recorderInfo = {
            audioPath: audioPath,
            recordLength: recordLength,
        };
        return sharedInstance.push(recorderInfo) - 1;
    },

    NCSaveVideo: function () {
        saveVideo();
    },

    NCPlayVideo: function () {
        playVideo();
    },

    NCFrameSize: function (recorderPtr, outWidth, outHeight) {
        new Int32Array(HEAPU8.buffer, outWidth, 1)[0] = width;
        new Int32Array(HEAPU8.buffer, outHeight, 1)[0] = height;
    },

    NCCommitFrame: function (recorderPtr, nativePtr) {
        const texture = GL.textures[nativePtr];
        GLctx.bindTexture(GLctx.TEXTURE_2D, texture);
        GLctx.bindFramebuffer(GLctx.FRAMEBUFFER, window["frameBuffer"]);
        GLctx.framebufferTexture2D(GLctx.FRAMEBUFFER, GLctx.COLOR_ATTACHMENT0, GLctx.TEXTURE_2D, texture, 0);
        GLctx.readPixels(0, 0, width, height, GLctx.RGBA, GLctx.UNSIGNED_BYTE, window["gPixels"]);
        const pixels = window["gPixels"];
        recordFrame(pixels);
    },

    NCFinishWriting: function (recorderPtr) {
        const recorderInfo = sharedInstance[recorderPtr];
        finishRecord(recorderInfo.recordLength * 1000, recorderInfo.audioPath);
    }
};

autoAddDeps(NatCorder, "$sharedInstance");
mergeInto(LibraryManager.library, NatCorder);