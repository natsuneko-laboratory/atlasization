/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;

using Mochizuki.Atlasization.Internal.Models;

using UnityEngine;

namespace Mochizuki.Atlasization.Internal.Utilities
{
    internal static class TextureUtils
    {
        private static readonly Dictionary<TextureFormat, RenderTextureFormat> FormatAliasMap = new Dictionary<TextureFormat, RenderTextureFormat>
        {
            { TextureFormat.Alpha8, RenderTextureFormat.ARGB32 },
            { TextureFormat.ARGB4444, RenderTextureFormat.ARGB4444 },
            { TextureFormat.RGB24, RenderTextureFormat.ARGB32 },
            { TextureFormat.RGBA32, RenderTextureFormat.ARGB32 },
            { TextureFormat.ARGB32, RenderTextureFormat.ARGB32 },
            { TextureFormat.RGB565, RenderTextureFormat.RGB565 },
            { TextureFormat.R16, RenderTextureFormat.R16 },
            { TextureFormat.DXT1, RenderTextureFormat.ARGB32 },
            { TextureFormat.DXT5, RenderTextureFormat.ARGB32 },
            { TextureFormat.RGBA4444, RenderTextureFormat.ARGB4444 },
            { TextureFormat.BGRA32, RenderTextureFormat.ARGB32 },
            { TextureFormat.RHalf, RenderTextureFormat.RHalf },
            { TextureFormat.RGHalf, RenderTextureFormat.RGHalf },
            { TextureFormat.RGBAHalf, RenderTextureFormat.ARGBHalf },
            { TextureFormat.RFloat, RenderTextureFormat.RFloat },
            { TextureFormat.RGFloat, RenderTextureFormat.RGFloat },
            { TextureFormat.RGBAFloat, RenderTextureFormat.ARGBFloat },
            { TextureFormat.RGB9e5Float, RenderTextureFormat.ARGBHalf },
            { TextureFormat.BC6H, RenderTextureFormat.ARGBHalf },
            { TextureFormat.BC7, RenderTextureFormat.ARGB32 },
            { TextureFormat.BC4, RenderTextureFormat.R8 },
            { TextureFormat.BC5, RenderTextureFormat.RGHalf },
            { TextureFormat.DXT1Crunched, RenderTextureFormat.ARGB32 },
            { TextureFormat.DXT5Crunched, RenderTextureFormat.ARGB32 },
            { TextureFormat.PVRTC_RGB2, RenderTextureFormat.ARGB32 },
            { TextureFormat.PVRTC_RGBA2, RenderTextureFormat.ARGB32 },
            { TextureFormat.PVRTC_RGB4, RenderTextureFormat.ARGB32 },
            { TextureFormat.PVRTC_RGBA4, RenderTextureFormat.ARGB32 },
            { TextureFormat.ETC_RGB4, RenderTextureFormat.ARGB32 }
        };

        public static RenderTextureFormat ConvertToRenderTextureFormat(TextureFormat format)
        {
            if (FormatAliasMap.ContainsKey(format))
                return FormatAliasMap[format];
            throw new NotSupportedException();
        }

        public static Texture2D ResizeTexture(Texture2D texture, int size)
        {
            var c = RenderTexture.active;

            // Graphics.ConvertTexture write pixels to GPU
            var a = new Texture2D(size, size);
            Graphics.ConvertTexture(texture, a);

            // CPU (EncodeToPNG, GetPixel and others) could not read pixels directly from GPU
            var r = RenderTexture.GetTemporary(size, size, 0);
            RenderTexture.active = r;
            Graphics.Blit(a, r);

            var b = new Texture2D(size, size);
            b.ReadPixels(new Rect(0, 0, size, size), 0, 0, false);

            RenderTexture.ReleaseTemporary(r);
            RenderTexture.active = c;

            return b;
        }

        public static ReadableTexture2D CreateTextureFromColor(Color color)
        {
            var texture = new Texture2D(128, 128);
            for (var i = 0; i < texture.height; i++)
            for (var j = 0; j < texture.width; j++)
                texture.SetPixel(i, j, color);

            texture.Apply();

            return new ReadableTexture2D(texture, null);
        }

        public static ReadableTexture2D CreateReadableTexture2D(Texture2D texture)
        {
            var r = RenderTexture.GetTemporary(texture.width, texture.height, 0, ConvertToRenderTextureFormat(texture.format));
            Graphics.Blit(texture, r);

            var c = RenderTexture.active;
            RenderTexture.active = r;

            var readableTexture = new Texture2D(texture.width, texture.height);
            readableTexture.ReadPixels(new Rect(0, 0, r.width, r.height), 0, 0);
            readableTexture.Apply();

            RenderTexture.ReleaseTemporary(r);
            RenderTexture.active = c;

            return new ReadableTexture2D(readableTexture, texture);
        }
    }
}