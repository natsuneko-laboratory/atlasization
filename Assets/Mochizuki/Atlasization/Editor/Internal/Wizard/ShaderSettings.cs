/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using System.Linq;

using Mochizuki.Atlasization.Internal.Enum;
using Mochizuki.Atlasization.Internal.Models;

using UnityEditor;

using UnityEngine;

namespace Mochizuki.Atlasization.Internal.Wizard
{
    internal class ShaderSettings : IWizardPage
    {
        private int _index;
        private List<AtlasMeshLayout> _layouts;
        private List<Material> _materials;
        private List<string> _shaders;

        public WizardPage PageId => WizardPage.ShaderSettings;

        public void OnInitialize() { }

        public void OnAwake(AtlasConfiguration configuration)
        {
            _index = 0;
            _layouts = configuration.MeshLayouts;
            _materials = new List<Material>();
            _shaders = ShaderUtil.GetAllShaderInfo().Where(w => w.supported).Select(w => w.name).ToList();

            foreach (var group in _layouts.GroupBy(w => w.Channel))
            {
                var value = group.First().Material.shader.name;
                var isAllMaterialEquals = group.All(w => w.Material.shader.name == value);

                var material = !isAllMaterialEquals ? new Material(Shader.Find("Standard")) : Object.Instantiate(group.First().Material);
                material.name = $"Material (Layer {group.Key})";
                _materials.Add(material);
            }
        }

        public bool OnGui(AtlasConfiguration configuration)
        {
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField(@"
シェーダーに関する設定を行います。
なお、シェーダーは SubMesh 毎に全て同じものが設定され、前の設定を引き継ぎたい場合は、全てのシェーダーを揃えておく必要があります。
また、引き継がれる設定は、 Atlasization が一番最初に検出した Material の設定になります。
".Trim());
            }

            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();

            var materials = _materials.Select(w => w.name).ToArray();
            _index = EditorGUILayout.Popup("Material for Shader", _index, materials);

            if (EditorGUI.EndChangeCheck())
            {
                var material = _materials[_index];
                material.GetTexturePropertyNames();

                // TODO: Generate NormalMaps, Masks and other sub-textures as atlas
            }

            var shader = _shaders[EditorGUILayout.Popup("Shader", _shaders.FindIndex(w => w == _materials[_index].shader.name), _shaders.ToArray())];
            _materials[_index].shader = Shader.Find(shader);

            return true;
        }

        public void OnFinalize(AtlasConfiguration configuration)
        {
            configuration.NewMaterials.Clear();
            configuration.NewMaterials.AddRange(_materials);
        }

        public void OnDiscard() { }
    }
}