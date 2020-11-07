/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;

using Mochizuki.Atlasization.Internal.Enum;

using UnityEngine;

namespace Mochizuki.Atlasization.Internal.Models
{
    [Serializable]
    internal class AtlasConfiguration
    {
        public AtlasSize AtlasTextureSize { get; set; }

        public string BaseName { get; set; }

        public List<Color> Colors { get; }

        public string DestPath => Path.Combine(DestinationPath, BaseName);

        public string DestinationPath { get; set; }

        public GameObject GameObject { get; set; }

        public bool IsUseMaterialKey { get; set; }

        public List<Material> Materials { get; }

        public Mode Mode { get; set; }

        public List<Renderer> Renderers { get; }

        public List<ReadableTexture2D> Textures { get; }

        public int TextureDivision => (int) Math.Ceiling(Math.Sqrt(Textures.Count));

        public GameObject Workspace { get; set; }

        public AtlasConfiguration()
        {
            Colors = new List<Color>();
            Materials = new List<Material>();
            Renderers = new List<Renderer>();
            Textures = new List<ReadableTexture2D>();
        }
    }
}