/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Mochizuki.Atlasization.Internal.Utilities;

using UnityEditor;

using UnityEngine;

using Object = UnityEngine.Object;

namespace Mochizuki.Atlasization.Internal.Models
{
    internal class AtlasTexture
    {
        private readonly Dictionary<ReadableTexture2D, (float, float)> _coordMappings;
        private readonly Dictionary<ReadableTexture2D, float> _sizeMappings;
        private readonly Texture2D _texture;
        private Texture2D _actualTexture;

        public AtlasTexture(int width, int height)
        {
            _texture = new Texture2D(width, height);
            _coordMappings = new Dictionary<ReadableTexture2D, (float, float)>();
            _sizeMappings = new Dictionary<ReadableTexture2D, float>();
        }

        public void Create(List<ReadableTexture2D> textures, int division)
        {
            var square = _texture.width / division;

            var k = 0;
            for (var i = division; i > 0; i--)
            {
                var y = i * square - square;

                for (var j = 0; j < division; j++)
                {
                    if (k + 1 > textures.Count)
                        break;

                    var x = j * square;
                    var texture = TextureUtils.ResizeTexture(textures[k].Texture, square);

                    for (var m = 0; m < square; m++)
                    for (var n = 0; n < square; n++)
                        _texture.SetPixel(x + n, y + m, texture.GetPixel(n, m));

                    _coordMappings.Add(textures[k], (x / (float) _texture.width, y / (float) _texture.height));
                    _sizeMappings.Add(textures[k++], square / (float) _texture.width);
                }
            }

            _texture.Apply(true, false);
        }

        public AtlasMaterial CreateMaterial(string name)
        {
            if (_actualTexture == null)
                throw new InvalidOperationException($"You must call {nameof(SaveAsAsset)} before this.");

            return new AtlasMaterial(name, _actualTexture);
        }

        public AtlasMaterial CreateMaterial(Material material)
        {
            if (_actualTexture == null)
                throw new InvalidOperationException($"You must call {nameof(SaveAsAsset)} before this.");

            return new AtlasMaterial(material, _actualTexture);
        }

        public Texture2D SaveAsAsset(string path)
        {
            File.WriteAllBytes(path, _texture.EncodeToPNG());

            AssetDatabase.Refresh();

            var importer = (TextureImporter) AssetImporter.GetAtPath(path);
            importer.maxTextureSize = _texture.width;
            importer.SaveAndReimport();

            Object.DestroyImmediate(_texture);

            _actualTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            return _actualTexture;
        }

        public (float, float) GetTexturePath(ReadableTexture2D texture)
        {
            if (_coordMappings.ContainsKey(texture))
                return _coordMappings[texture];
            return _coordMappings.FirstOrDefault(w => texture.IsEquals(w.Key)).Value;
        }

        public float GetTextureSize(ReadableTexture2D texture)
        {
            if (_sizeMappings.ContainsKey(texture))
                return _sizeMappings[texture];
            return _sizeMappings.FirstOrDefault(w => texture.IsEquals(w.Key)).Value;
        }
    }
}