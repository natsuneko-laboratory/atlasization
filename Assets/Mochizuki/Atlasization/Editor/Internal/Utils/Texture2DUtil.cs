﻿/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using System.Linq;

using UnityEngine;

namespace Mochizuki.Atlasization.Internal.Utils
{
    internal static class Texture2DUtil
    {
        public static Texture2D ResizeTexture(Texture2D texture, int size)
        {
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

            return b;
        }

        public static Texture2D CreateTexture2DFromColor(Color color)
        {
            var texture = new Texture2D(128, 128);
            for (var i = 0; i < texture.height; i++)
            for (var j = 0; j < texture.width; j++)
                texture.SetPixel(i, j, color);

            texture.Apply();

            return texture;
        }

        public static bool CompareTexture(Texture2D a, Texture2D b)
        {
            var c1 = a.GetPixels();
            var c2 = b.GetPixels();

            if (c1.Length != c2.Length)
                return false;

            return !c1.Where((t, i) => t != c2[i]).Any();
        }
    }
}