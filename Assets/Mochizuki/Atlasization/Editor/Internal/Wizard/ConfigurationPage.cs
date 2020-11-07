/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using Mochizuki.Atlasization.Internal.Attributes;
using Mochizuki.Atlasization.Internal.Enum;
using Mochizuki.Atlasization.Internal.Models;
using Mochizuki.Atlasization.Internal.Utilities;

using UnityEditor;

using UnityEngine;

namespace Mochizuki.Atlasization.Internal.Wizard
{
    internal class ConfigurationPage : ScriptableObject, IWizardPage
    {
        private const string DirectoryGuid = "74d2990457cbdd24eb5bb687e871daed";

        [DirectoryField]
        [SerializeField]
        private DefaultAsset _destination;

        private bool _isSplitByMaterialKey;

        private string _name;

        private AtlasSize _size;

        public WizardPage PageId => WizardPage.Configuration;

        public void OnInitialize()
        {
            if (_destination == null)
                _destination = AssetDatabase.LoadAssetAtPath<DefaultAsset>(AssetDatabase.GUIDToAssetPath(DirectoryGuid));
            _isSplitByMaterialKey = false;
            _name = null;
            _size = AtlasSize.Four;
        }

        public void OnAwake(AtlasConfiguration configuration) { }

        public bool OnGui(AtlasConfiguration configuration)
        {
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField(@"
アトラス化されたテクスチャおよび Prefab の出力設定を行います。
アトラス化作業によって変更された Prefab は同名で設定したディレクトリに出力されます。
".Trim());
            }

            EditorGUILayout.LabelField("グローバル設定");
            using (new EditorGUI.IndentLevelScope())
            {
                CustomField.PropertyField(this, nameof(_destination), "出力先ディレクトリ");
                _name = EditorGUILayout.TextField("出力ファイル名", _name ?? configuration.GameObject.name);
            }

            EditorGUILayout.LabelField("メッシュ設定");
            using (new EditorGUI.IndentLevelScope())
                _isSplitByMaterialKey = EditorGUILayout.Toggle("Material Key を使用する", _isSplitByMaterialKey);

            EditorGUILayout.LabelField("テクスチャ設定");
            using (new EditorGUI.IndentLevelScope())
                _size = CustomField.EnumField("出力サイズ", _size);

            return _destination != null && !string.IsNullOrWhiteSpace(_name);
        }

        public void OnFinalize(AtlasConfiguration configuration)
        {
            configuration.AtlasTextureSize = _size;
            configuration.BaseName = _name;
            configuration.DestinationPath = AssetDatabase.GetAssetPath(_destination);
            configuration.IsUseMaterialKey = _isSplitByMaterialKey;
        }

        public void OnDiscard() { }
    }
}