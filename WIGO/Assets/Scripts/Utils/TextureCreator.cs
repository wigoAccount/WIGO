using UnityEngine;
using UnityEngine.Experimental.Rendering;

public static class TextureCreator
{
    public static Texture2D GetCompressedTexture(int width = 4, int height = 4)
    {
        return new Texture2D(width, height, GetTextureFormat(), false);
    }

    public static Texture2D GetRGBwithoutAlphaTexture(int width = 2, int height = 2)
    {
        return new Texture2D(width, height, TextureFormat.RGB24, false);
    }

    public static Texture2D GetARGB32Texture(int width = 2, int height = 2)
    {
        return new Texture2D(width, height, TextureFormat.ARGB32, false);
    }

    public static Texture2D GetTextureWithPostProcessSupport(int width = 2, int height = 2)
    {
        return new Texture2D(width, height, GraphicsFormat.R8G8B8A8_SRGB, TextureCreationFlags.None);
    }

    public static Texture2D GetRoundedTexture(int h, int w, float r, float cx, float cy, Texture2D sourceTex)
    {
        Color[] c = sourceTex.GetPixels(0, 0, sourceTex.width, sourceTex.height);
        Texture2D b = new Texture2D(h, w);
        for (int i = (int)(cx - r); i < cx + r; i++)
        {
            for (int j = (int)(cy - r); j < cy + r; j++)
            {
                float dx = i - cx;
                float dy = j - cy;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                if (d <= r)
                    b.SetPixel(i - (int)(cx - r), j - (int)(cy - r), sourceTex.GetPixel(i, j));
                else
                    b.SetPixel(i - (int)(cx - r), j - (int)(cy - r), Color.clear);
            }
        }
        b.Apply();
        return b;
    }

    public static TextureFormat GetTextureFormat()
    {
//#if UNITY_EDITOR
//        return TextureFormat.DXT1;
#if UNITY_ANDROID
        return TextureFormat.ETC2_RGB;
#elif UNITY_IOS
        return TextureFormat.ASTC_4x4;
#else
        return TextureFormat.RGB24;
#endif
    }
}
