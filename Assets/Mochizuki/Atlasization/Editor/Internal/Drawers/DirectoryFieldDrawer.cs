/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using Mochizuki.Atlasization.Internal.Attributes;

using UnityEditor;

using UnityEngine;

namespace Mochizuki.Atlasization.Internal.Drawers
{
    [CustomPropertyDrawer(typeof(DirectoryFieldAttribute))]
    internal class DirectoryFieldDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.objectReferenceValue != null)
            {
                var path = AssetDatabase.GetAssetPath(property.objectReferenceValue);
                if (!AssetDatabase.IsValidFolder(path))
                    property.objectReferenceValue = null;
            }

            EditorGUI.PropertyField(position, property, label);
        }
    }
}