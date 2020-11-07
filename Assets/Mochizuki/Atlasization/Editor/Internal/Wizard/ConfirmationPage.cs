/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using System.Linq;

using Mochizuki.Atlasization.Internal.Enum;
using Mochizuki.Atlasization.Internal.Extensions;
using Mochizuki.Atlasization.Internal.Models;
using Mochizuki.Atlasization.Internal.Utilities;

using UnityEditor;

using UnityEngine;

namespace Mochizuki.Atlasization.Internal.Wizard
{
    internal class ConfirmationPage : IWizardPage
    {
        public WizardPage PageId => WizardPage.Finalize;

        public void OnInitialize() { }

        public void OnAwake(AtlasConfiguration configuration) { }

        public bool OnGui(AtlasConfiguration configuration)
        {
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField(@"
設定内容を確認し、問題なければ「生成」をクリックしてください。
なお、元の Prefab / Material / Texture が変更されることはありません。
".Trim());
            }

            using (new EditorGUI.DisabledGroupScope(true))
            {
                CustomField.ObjectPicker("Prefab / GameObject", configuration.GameObject);

                EditorGUILayout.LabelField("Stats:");
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.LabelField($"Materials : {configuration.Materials.Count}");
                    EditorGUILayout.LabelField($"Textures  : {configuration.Textures.Count}");
                }

                EditorGUILayout.LabelField("SubMesh Mapping");
                using (new EditorGUI.IndentLevelScope())
                {
                    foreach (var layout in configuration.MeshLayouts)
                    {
                        CustomField.ObjectPicker("Material Key", layout.Material);
                        EditorGUILayout.IntField("Layer ID", layout.Channel);
                    }
                }

                EditorGUILayout.LabelField("Texture Mapping");
                CustomField.PreviewReadableTexture2Ds(configuration.Textures, configuration.TextureDivision);

                EditorGUILayout.LabelField("グローバル設定");
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.TextField("出力先ディレクトリ", configuration.DestinationPath);
                    EditorGUILayout.TextField("出力ファイル名", configuration.BaseName);
                }

                EditorGUILayout.LabelField("メッシュ設定");
                using (new EditorGUI.IndentLevelScope())
                    EditorGUILayout.Toggle("Material Key を使用する", configuration.IsUseMaterialKey);

                EditorGUILayout.LabelField("テクスチャ設定");
                using (new EditorGUI.IndentLevelScope())
                    CustomField.EnumField("出力サイズ", configuration.AtlasTextureSize);
            }

            return true;
        }

        public void OnFinalize(AtlasConfiguration configuration)
        {
            var texture = new AtlasTexture(configuration.AtlasTextureSize.ToTextureSize(), configuration.AtlasTextureSize.ToTextureSize());
            texture.Create(configuration.Textures, configuration.TextureDivision);
            texture.SaveAsAsset($"{configuration.DestPath}.png");

            var material = texture.CreateMaterial("Standard");
            material.SaveAsAsset($"{configuration.DestPath}.mat");

            var meshes = new Dictionary<string, AtlasMesh>();
            var counter = 0;
            foreach (var renderer in configuration.Renderers)
            {
                var mesh = new AtlasMesh(renderer, configuration.IsUseMaterialKey);
                if (meshes.ContainsKey(mesh.CalcMaterialKey()))
                {
                    meshes[mesh.CalcMaterialKey()].ApplyTo(renderer);
                    renderer.sharedMaterials = Enumerable.Range(0, renderer.sharedMaterials.Length).Select(w => material.Actual).ToArray();
                    continue;
                }

                mesh.WriteNewUVs(0, texture);

                var reduceTo = 0;
                foreach (var group in configuration.MeshLayouts.GroupBy(w => w.Channel))
                {
                    var materialSlots = mesh.GetChannels(group.ToList());
                    if (group.Key >= mesh.Channels)
                        continue;

                    mesh.CombineChannels(group.Key, materialSlots);
                    reduceTo++;
                }

                mesh.ReduceSubMeshTo(reduceTo);
                mesh.SaveAsAsset($"{configuration.DestPath}_{counter++}.asset");
                mesh.Apply();

                meshes.Add(mesh.CalcMaterialKey(), mesh);
                renderer.sharedMaterials = Enumerable.Range(0, renderer.sharedMaterials.Length).Select(_ => material.Actual).ToArray();
            }

            PrefabUtility.SaveAsPrefabAsset(configuration.GameObject, $"{configuration.DestPath}.prefab");
            Object.DestroyImmediate(configuration.Workspace);
        }

        public void OnDiscard() { }
    }
}