/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Mochizuki.Atlasization.Internal.Enum;
using Mochizuki.Atlasization.Internal.Models;
using Mochizuki.Atlasization.Internal.Wizard;

using UnityEditor;

using UnityEngine;

using Debug = UnityEngine.Debug;

namespace Mochizuki.Atlasization
{
    public class AtlasEditor : EditorWindow
    {
        private const string Namespace = "Mochizuki";
        private const string Product = "Atlasization";
        private const string Version = "0.1.0";

        private AtlasConfiguration _configuration;
        private WizardPage _current;
        private Vector2 _scroll = Vector2.zero;

        private List<IWizardPage> _wizardPages;

        [MenuItem("Mochizuki/Atlasization/Documents")]
        public static void ShowDocument()
        {
            Process.Start("https://docs.mochizuki.moe/Unity/Atlasization/");
        }

        [MenuItem("Mochizuki/Atlasization/Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<AtlasEditor>();
            window.titleContent = new GUIContent("Atlasization Editor");

            window.Show();
        }

        private void OnEnable()
        {
            if (_configuration == null)
                _configuration = new AtlasConfiguration();
            if (_wizardPages == null)
                _wizardPages = new List<IWizardPage>();

            _wizardPages.Clear();
            _wizardPages.Add(new StartPage());
            _wizardPages.Add(new InitialPage());
            _wizardPages.Add(new MeshMappingPage());
            _wizardPages.Add(new TextureMappingPage());
            _wizardPages.Add(new ShaderSettings());
            _wizardPages.Add(new ConfigurationPage());
            _wizardPages.Add(new ConfirmationPage());

            _wizardPages.ForEach(w => w.OnInitialize());
        }

        private void OnGUI()
        {
            EditorStyles.label.wordWrap = true;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"{Product} by {Namespace} - {Version}");
            EditorGUILayout.Space();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            var lastId = Enum.GetNames(typeof(WizardPage)).Length - 1;
            EditorGUILayout.LabelField($"ステップ {(int) _current + 1} / {lastId + 2}");

            var page = _wizardPages.First(w => w.PageId == _current);

            try
            {
                var isValidationSuccess = page.OnGui(_configuration);
                var previous = _current;

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (_current != 0 && GUILayout.Button("前へ"))
                    {
                        _current--;
                        page.OnDiscard();
                    }

                    using (new EditorGUI.DisabledGroupScope(!isValidationSuccess))
                    {
                        if (_current != (WizardPage) lastId && GUILayout.Button("次へ"))
                        {
                            _current++;
                            page.OnFinalize(_configuration);
                        }

                        if (_current == (WizardPage) lastId && GUILayout.Button("生成"))
                        {
                            page.OnFinalize(_configuration);
                            _current = 0;
                            _wizardPages.ForEach(w => w.OnInitialize());
                        }
                    }
                }

                if (_current != previous)
                {
                    if (_current < previous)
                        page.OnDiscard();
                    _wizardPages.First(w => w.PageId == _current).OnAwake(_configuration);
                    _scroll = Vector2.zero;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);

                _wizardPages.ForEach(w => w.OnDiscard());
                _wizardPages.ForEach(w => w.OnInitialize());
            }

            EditorGUILayout.EndScrollView();
        }
    }
}