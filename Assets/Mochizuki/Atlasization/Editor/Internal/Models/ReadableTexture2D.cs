/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using System.Linq;

using UnityEngine;

namespace Mochizuki.Atlasization.Internal.Models
{
    internal class ReadableTexture2D
    {
        public Texture2D Original { get; }

        public Texture2D Texture { get; }

        public ReadableTexture2D(Texture2D readable, Texture2D original)
        {
            Texture = readable;
            Original = original;
        }

        private Color[] GetPixels()
        {
            return Texture.GetPixels();
        }

        public bool IsEquals(ReadableTexture2D other)
        {
            var c1 = Texture.GetPixels();
            var c2 = other.GetPixels();

            if (c1.Length != c2.Length)
                return false;

            return !c1.Where((t, i) => t != c2[i]).Any();
        }
    }
}