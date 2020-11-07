/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using System;

using Mochizuki.Atlasization.Internal.Enum;

namespace Mochizuki.Atlasization.Internal.Extensions
{
    internal static class AtlasSizeExtensions
    {
        public static int ToTextureSize(this AtlasSize obj)
        {
            switch (obj)
            {
                case AtlasSize.One:
                    return 1024;

                case AtlasSize.Two:
                    return 2048;

                case AtlasSize.Four:
                    return 4096;

                case AtlasSize.Eight:
                    return 8192;

                default:
                    throw new ArgumentOutOfRangeException(nameof(obj), obj, null);
            }
        }
    }
}