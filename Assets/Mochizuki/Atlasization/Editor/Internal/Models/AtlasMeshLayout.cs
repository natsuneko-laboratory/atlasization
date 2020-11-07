/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using UnityEngine;

namespace Mochizuki.Atlasization.Internal.Models
{
    internal class AtlasMeshLayout
    {
        public int Channel { get; set; }

        public Material Material { get; }

        public AtlasMeshLayout(Material material)
        {
            Material = material;
        }
    }
}