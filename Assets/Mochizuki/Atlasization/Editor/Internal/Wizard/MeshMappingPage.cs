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
    internal class MeshMappingPage : IWizardPage
    {
        private List<AtlasMeshLayout> _layouts;

        public WizardPage PageId => WizardPage.MeshMapping;

        public void OnInitialize() { }

        public void OnAwake(AtlasConfiguration configuration)
        {
            _layouts = configuration.MeshLayouts.ToList();
        }

        public bool OnGui(AtlasConfiguration configuration)
        {
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField(@"
SubMesh の設定を行います。
SubMesh を適切に設定することで、例えば表情に関する部分だけは他の Material (Transparent など) を適用する、といったこと等が可能になります。
※現時点では設定されてる Material ベースで SubMesh の分割を決定しています。そのため、場合によっては意図しない動作になることがあります。
　その場合、それぞれを別の Material (Ctrl+D でコピーしたものなど) に設定することによって修正できます。
".Trim());
            }

            foreach (var (layout, i) in _layouts.Select((w, i) => (w, i)))
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    using (new EditorGUI.DisabledGroupScope(true))
                        CustomField.ObjectPicker("Material", layout.Material);
                    _layouts[i].Channel = EditorGUILayout.IntField("LayerID", layout.Channel);
                }

                EditorGUILayout.Space();
            }

            EditorGUILayout.Space();

            return true;
        }

        public void OnFinalize(AtlasConfiguration configuration)
        {
            configuration.MeshLayouts.Clear();
            configuration.MeshLayouts.AddRange(_layouts);
        }

        public void OnDiscard() { }
    }
}