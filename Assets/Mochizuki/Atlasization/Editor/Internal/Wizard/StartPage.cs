/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using Mochizuki.Atlasization.Internal.Enum;
using Mochizuki.Atlasization.Internal.Models;
using Mochizuki.Atlasization.Internal.Utilities;

using UnityEditor;

using UnityEngine;

namespace Mochizuki.Atlasization.Internal.Wizard
{
    internal class StartPage : IWizardPage
    {
        private Mode _mode;

        public WizardPage PageId => WizardPage.Start;

        public void OnInitialize()
        {
            _mode = Mode.Standard;
        }

        public bool OnGui(AtlasConfiguration configuration)
        {
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField(@"
Unity 上でアトラス化作業を行えるエディター拡張です。
ウィザードに従って操作することで、 Prefab などから簡単にアトラス化された Texture / Material を作成できます。
".Trim());
            }

            _mode = CustomField.EnumField("起動モード", _mode);

            return true;
        }

        public void OnFinalize(AtlasConfiguration configuration)
        {
            configuration.Mode = _mode;
        }

        public void OnDiscard()
        {
            // NOTHING TO DO
        }
    }
}