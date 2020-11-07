/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using UnityEditor;

using UnityEngine;

namespace Mochizuki.Atlasization.Internal.Models
{
    internal class AtlasMaterial
    {
        private readonly Material _material;

        public Material Actual { get; private set; }

        public AtlasMaterial(string name, Texture texture)
        {
            _material = new Material(Shader.Find(name)) { mainTexture = texture };
        }

        public Material SaveAsAsset(string path)
        {
            AssetDatabase.CreateAsset(_material, path);
            AssetDatabase.Refresh();

            Actual = AssetDatabase.LoadAssetAtPath<Material>(path);
            return Actual;
        }
    }
}