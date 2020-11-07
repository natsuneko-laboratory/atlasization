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
    internal class TextureMappingPage : IWizardPage
    {
        private List<AtlasTextureLayout> _textureLayouts;

        public WizardPage PageId => WizardPage.TextureMapping;

        public void OnInitialize() { }

        public void OnAwake(AtlasConfiguration configuration)
        {
        }

        public bool OnGui(AtlasConfiguration configuration)
        {
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField(@"
Material から検出されたテクスチャーの配置を確認します。
メインテクスチャー (_MainTex / Texture2D) のみが対象、またテクスチャーが設定できない (色指定のみなど) 場合はメインカラー (_Color) が代わりに設定されます。
".Trim());
            }

            EditorGUILayout.Space();

            CustomField.PreviewReadableTexture2Ds(configuration.Textures, configuration.TextureDivision);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField($"Texture2D References ({configuration.Textures.Count} Textures, {configuration.Materials.Count} Materials) : ");

            foreach (var (texture, i) in configuration.Textures.Select((w, i) => (w, i)))
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    if (string.IsNullOrWhiteSpace(texture.Original?.name))
                        CustomField.ObjectPicker("Auto Generated", texture.Texture);
                    else
                        CustomField.ObjectPicker(texture.Original.name, texture.Original);
                }

            return true;
        }

        public void OnFinalize(AtlasConfiguration configuration)
        {
            configuration.TextureLayouts.Clear();
            configuration.TextureLayouts.AddRange(_textureLayouts);
        }

        public void OnDiscard() { }
    }
}