/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;

using Mochizuki.Atlasization.Internal.Utilities;

using UnityEditor;

using UnityEngine;

using Object = UnityEngine.Object;

namespace Mochizuki.Atlasization.Internal.Models
{
    internal class AtlasMesh
    {
        private readonly List<int> _isAlreadyProcessed;
        private readonly bool _isUseMaterialKey;
        private readonly Mesh _mesh;
        private readonly Renderer _renderer;
        private readonly Mesh _sharedMesh;

        private Mesh _actualMesh;

        public int Channels => _mesh.subMeshCount;

        public AtlasMesh(Renderer renderer, bool isUseMaterialKey)
        {
            _isAlreadyProcessed = new List<int>();
            _renderer = renderer;
            _isUseMaterialKey = isUseMaterialKey;

            if (renderer is SkinnedMeshRenderer smr)
                _sharedMesh = smr.sharedMesh;
            if (renderer is MeshRenderer mr)
                _sharedMesh = mr.gameObject.GetComponent<MeshFilter>().sharedMesh;
            _mesh = Object.Instantiate(_sharedMesh);
        }

        public string CalcMaterialKey()
        {
            if (_isUseMaterialKey)
                return $"Mesh: {_sharedMesh.GetInstanceID()}, Materials: {string.Join(", ", _renderer.sharedMaterials.Select(w => w.GetInstanceID()))}";
            return $"Mesh: {_sharedMesh.GetInstanceID()}, Materials: null";
        }

        public List<int> GetChannels(List<AtlasMeshLayout> layouts)
        {
            var slots = new List<int>();

            foreach (var (material, idx) in _renderer.sharedMaterials.Select((w, i) => (w, i)))
            {
                if (layouts.FirstOrDefault(w => w.Material == material) == null)
                    continue;

                slots.Add(idx);
            }

            return slots;
        }

        public void Apply()
        {
            ApplyTo(_renderer);
        }

        public void ApplyTo(Renderer renderer)
        {
            if (_actualMesh == null)
                throw new InvalidOperationException($"You must call {nameof(SaveAsAsset)} before this.");
            if (renderer is SkinnedMeshRenderer smr)
                smr.sharedMesh = _actualMesh;
            if (renderer is MeshRenderer mr)
                mr.gameObject.GetComponent<MeshFilter>().sharedMesh = _actualMesh;
        }

        public Mesh SaveAsAsset(string path)
        {
            AssetDatabase.CreateAsset(_mesh, path);
            AssetDatabase.Refresh();

            _actualMesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);

            return _actualMesh;
        }

        public void WriteNewUVs(int writeChannel, AtlasTexture texture)
        {
            if (_isAlreadyProcessed.Contains(writeChannel))
                return;
            _isAlreadyProcessed.Add(writeChannel);

            var materials = _renderer.sharedMaterials;
            var calculatedTriangle = new List<int>();
            var uvs = _mesh.uv;

            foreach (var channel in Enumerable.Range(0, _mesh.subMeshCount))
            {
                var associatedTexture = GetTexture(materials[channel]);
                var texelSize = texture.GetTextureSize(associatedTexture);
                var (offsetX, offsetY) = texture.GetTexturePath(associatedTexture);

                foreach (var triangle in _mesh.GetTriangles(channel))
                {
                    if (calculatedTriangle.Contains(triangle))
                        continue;

                    var uv = uvs[triangle];

                    uv.x = ClampUV(uv.x) * texelSize + offsetX;
                    uv.y = ClampUV(uv.y) * texelSize + offsetY;

                    uv.x = ClampUV(uv.x, offsetX, offsetX + texelSize);
                    uv.y = ClampUV(uv.y, offsetY, offsetY + texelSize);

                    calculatedTriangle.Add(triangle);

                    uvs[triangle] = uv;
                }
            }

            _mesh.uv = uvs;
        }

        public void CombineChannels(int writeTo, List<int> channels)
        {
            var triangles = new List<int>();
            foreach (var channel in channels)
                triangles.AddRange(_sharedMesh.GetTriangles(channel));

            _mesh.SetTriangles(triangles, writeTo);
        }

        public void ReduceSubMeshTo(int count)
        {
            _mesh.subMeshCount = count;
        }

        private static ReadableTexture2D GetTexture(Material material)
        {
            if (material.HasProperty("_MainTex") && material.mainTexture != null)
                return TextureUtils.CreateReadableTexture2D(material.mainTexture as Texture2D);
            return TextureUtils.CreateTextureFromColor(material.color);
        }

        private static float ClampUV(float f, float min = 0, float max = 1)
        {
            if (min <= f && f <= max)
                return f;
            if (f < min)
                return max - -1 * (f % max);
            if (f > max)
                return f % max;

            throw new InvalidOperationException();
        }
    }
}