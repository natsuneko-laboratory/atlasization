/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using System.Linq;

using Mochizuki.Atlasization.Internal.Enum;
using Mochizuki.Atlasization.Internal.Models;
using Mochizuki.Atlasization.Internal.Utilities;

using UnityEditor;

using UnityEngine;

namespace Mochizuki.Atlasization.Internal.Wizard
{
    internal class InitialPage : IWizardPage
    {
        private GameObject _gameObject;
        private GameObject _workspace;

        public WizardPage PageId => WizardPage.Initialize;

        public void OnInitialize()
        {
            _gameObject = null;
        }

        public bool OnGui(AtlasConfiguration configuration)
        {
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField(@"
アトラス化処理を行いたい Prefab もしくは GameObject を設定します。
設定した Prefab もしくは GameObject は、 Mochizuki.Workspace GameObject の子としてシーンに追加されます。
シーンに追加された GameObject は絶対にいじらないでください (処理終了後、自動削除されます)。
".Trim());
            }

            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();

            _gameObject = CustomField.ObjectPicker("Prefab / GameObject", _gameObject);

            if (EditorGUI.EndChangeCheck())
            {
                if (_workspace != null)
                    Object.DestroyImmediate(_workspace);

                if (_gameObject == null)
                    return false;
            }

            var error = ValidateGameObject();
            if (!string.IsNullOrWhiteSpace(error))
            {
                CustomField.ErrorField(error);
                return false;
            }

            return _gameObject != null;
        }

        public void OnFinalize(AtlasConfiguration configuration)
        {
            _workspace = new GameObject("Mochizuki.Workspace (DO NOT EDIT THIS)");
            configuration.GameObject = Object.Instantiate(_gameObject, _workspace.transform);
            configuration.Workspace = _workspace;

            configuration.Renderers.Clear();
            configuration.Renderers.AddRange(configuration.GameObject.GetComponentsInChildren<Renderer>().Where(w => w is SkinnedMeshRenderer || w is MeshRenderer));

            configuration.Materials.Clear();
            configuration.Materials.AddRange(configuration.Renderers.SelectMany(w => w.sharedMaterials).Distinct());

            configuration.Textures.Clear();
            configuration.Textures.AddRange(CollectTexturesFromMaterial(configuration.Materials));
            configuration.Textures.AddRange(CollectColorsFromMaterial(configuration.Materials));
        }

        public void OnDiscard()
        {
            _gameObject = null;
            if (_workspace != null)
            {
                Object.DestroyImmediate(_workspace);
                _workspace = null;
            }
        }

        private string ValidateGameObject()
        {
            if (_gameObject == null)
                return null;

            var renderers = _gameObject.GetComponentsInChildren<Renderer>(true).Where(w => w is SkinnedMeshRenderer || w is MeshRenderer).ToList();
            if (renderers.Count == 0)
                return "Prefab もしくは GameObject に Renderer Component が含まれていません。";

            var materials = renderers.SelectMany(w => w.sharedMaterials).ToList();
            if (materials.Count(w => w != null) != materials.Count)
                return "無効な Material が含まれています。";

            return null;
        }

        private static List<ReadableTexture2D> CollectTexturesFromMaterial(List<Material> materials)
        {
            return materials.Where(w => w.HasProperty("_MainTex") && w.mainTexture != null)
                            .Select(w => w.mainTexture)
                            .OfType<Texture2D>()
                            .Distinct()
                            .Select(TextureUtils.CreateReadableTexture2D)
                            .ToList();
        }

        private static List<ReadableTexture2D> CollectColorsFromMaterial(List<Material> materials)
        {
            return materials.Where(w => w.HasProperty("_Color") && w.mainTexture == null)
                            .Select(w => w.color)
                            .Distinct()
                            .Select(TextureUtils.CreateTextureFromColor)
                            .ToList();
        }
    }
}