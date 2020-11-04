/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using System.Linq;
using System.Text.RegularExpressions;

using UnityEditor;

using UnityEngine;

namespace Mochizuki.Atlasization.Internal.Utilities
{
    internal static class CustomField
    {
        public static T EnumField<T>(string label, T value) where T : System.Enum
        {
            var items = System.Enum.GetNames(typeof(T))
                              .Select(w => EnumUtils.GetEnumMemberValue<T>(w) ?? Regex.Replace(w, "(\\B[A-Z])", " $1"))
                              .Select(w => new GUIContent(w))
                              .ToArray();

            return (T) (object) EditorGUILayout.Popup(new GUIContent(label), (int) (object) value, items);
        }

        public static void ErrorField(string error)
        {
            EditorGUILayout.HelpBox(error, MessageType.Error);
        }

        public static T ObjectPicker<T>(string label, T obj) where T : Object
        {
            return EditorGUILayout.ObjectField(new GUIContent(label), obj, typeof(T), true) as T;
        }

        public static void PropertyField(EditorWindow editor, string property, string label = null)
        {
            var so = new SerializedObject(editor);
            so.Update();

            EditorGUILayout.PropertyField(so.FindProperty(property), string.IsNullOrWhiteSpace(label) ? null : new GUIContent(label), true);

            so.ApplyModifiedProperties();
        }
    }
}