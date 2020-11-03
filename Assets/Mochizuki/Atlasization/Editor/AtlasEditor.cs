/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using UnityEditor;

using UnityEngine;

using Object = UnityEngine.Object;

namespace Mochizuki.Atlasization
{
    public class AtlasEditor : EditorWindow
    {
        private const string DirectoryGuid = "74d2990457cbdd24eb5bb687e871daed";
        private const string Namespace = "Mochizuki";
        private const string Product = "Atlasization";
        private const string Version = "0.1.0";

        [SerializeField]
        private List<Color> _colors;

        [SerializeField]
        private WizardPages _current;

        [DirectoryField]
        [SerializeField]
        private DefaultAsset _dest;

        [SerializeField]
        private List<Renderer> _enabledRenderers;

        [SerializeField]
        private List<Material> _materials;

        [SerializeField]
        private string _name;

        [SerializeField]
        private GameObject _obj;

        [SerializeField]
        private GameObject _original;

        [SerializeField]
        private List<Renderer> _renderers;

        private Vector2 _scroll = Vector2.zero;

        private bool _shouldGenerateColorTextures;

        [SerializeField]
        private AtlasSize _size = AtlasSize.Four;

        [SerializeField]
        private List<Texture2D> _textures;

        private GameObject _workspace;

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

        private void Awake()
        {
            if (_dest == null)
                _dest = AssetDatabase.LoadAssetAtPath<DefaultAsset>(AssetDatabase.GUIDToAssetPath(DirectoryGuid));
        }

        private void OnGUI()
        {
            EditorStyles.label.wordWrap = true;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"{Product} by {Namespace} - {Version}");
            EditorGUILayout.Space();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            EditorGUILayout.LabelField($"ステップ {(int) _current + 1} / {(int) WizardPages.Finalize + 1}");

            var previous = _current;
            bool isValidationSuccess;

            switch (_current)
            {
                case WizardPages.Start:
                    isValidationSuccess = OnShowStartPage();
                    break;

                case WizardPages.Initialize:
                    isValidationSuccess = OnShowInitializePage();
                    break;

                case WizardPages.SelectRenderers:
                    isValidationSuccess = OnShowSelectRenderersPage();
                    break;

                case WizardPages.TextureMapping:
                    isValidationSuccess = OnShowTextureMappingPage();
                    break;

                case WizardPages.Configuration:
                    isValidationSuccess = OnShowConfigurationPage();
                    break;

                case WizardPages.Finalize:
                    isValidationSuccess = OnShowFinalizePage();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (_current != WizardPages.Start && GUILayout.Button("前へ"))
                    _current--;

                using (new EditorGUI.DisabledGroupScope(!isValidationSuccess))
                {
                    if (_current != WizardPages.Finalize && GUILayout.Button("次へ"))
                        _current++;

                    if (_current == WizardPages.Finalize && GUILayout.Button("生成"))
                    {
                        OnFinalize();
                        Cleanup();
                        _current = WizardPages.Start;
                    }
                }
            }

            if (_current != previous)
                _scroll = Vector2.zero;

            EditorGUILayout.EndScrollView();
        }

        private bool OnShowStartPage()
        {
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField(@"
Unity 上でアトラス化作業を行えるエディター拡張です。
ウィザードに従って操作することで、 Prefab などから簡単にアトラス化された Texture / Material を作成できます。
".Trim());
            }

            return true;
        }

        private bool OnShowInitializePage()
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

            _original = ObjectPicker("アトラス化を行う Prefab", _original);

            if (EditorGUI.EndChangeCheck())
            {
                if (_workspace != null)
                    DestroyImmediate(_workspace);
                if (_original == null)
                    return false;

                _workspace = new GameObject { name = "Mochizuki.Workspace (DO NOT EDIT THIS)" };
                _obj = Instantiate(_original, _workspace.transform);

                _renderers = new List<Renderer>();
                _renderers.AddRange(_obj.GetComponentsInChildren<Renderer>(true).Where(w => w is SkinnedMeshRenderer || w is MeshRenderer));

                _enabledRenderers = new List<Renderer>(_renderers);
            }

            if (_renderers?.Count != 0)
                return _original != null;

            ErrorField("Prefab もしくは GameObject に何らかの Renderer コンポーネントが含まれていません");
            return false;
        }

        private bool OnShowSelectRenderersPage()
        {
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField(@"
操作対象とする Renderer を削減したい場合は、必要なものを選択します。
そうでない場合 (削減する必要が無い場合) は、設定を変更する必要はありません。
".Trim());
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("スキャン済み Renderer Components : ");
            EditorGUI.indentLevel++;

            var enabled = new HashSet<Renderer>();

            foreach (var renderer in _renderers)
            {
                var rect = EditorGUILayout.GetControlRect();
                var height = EditorGUIUtility.singleLineHeight;

                var isEnabled = EditorGUI.Toggle(new Rect(rect.x, rect.y, height * 2, height), _enabledRenderers.Contains(renderer));
                if (isEnabled)
                    enabled.Add(renderer);

                using (new EditorGUI.DisabledGroupScope(true))
                    EditorGUI.ObjectField(new Rect(rect.x + height, rect.y, rect.width - height, height), renderer, renderer.GetType(), true);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("全て選択解除"))
                    enabled.Clear();
                if (GUILayout.Button("全て選択"))
                    enabled.UnionWith(_renderers);
            }

            _enabledRenderers = enabled.ToList();

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            _materials = _enabledRenderers.SelectMany(w => w.sharedMaterials).Distinct().ToList();
            _textures = _materials.Where(w => w.HasProperty("_MainTex")).Select(w => w.mainTexture).Where(w => w is Texture2D).Cast<Texture2D>().Distinct().ToList();
            _colors = _materials.Where(w => w.HasProperty("_Color") && w.mainTexture == null).Select(w => w.color).Distinct().ToList();
            _shouldGenerateColorTextures = true;

            return _materials.Count > 0;
        }

        private bool OnShowTextureMappingPage()
        {
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField(@"
Material から検出されたテクスチャーの配置を確認します。
メインテクスチャー (_MainTex / Texture2D) のみが対象、またテクスチャーが設定できない (色指定のみなど) 場合はメインカラー (_Color) が代わりに設定されます。
".Trim());
            }

            EditorGUILayout.Space();

            if (_shouldGenerateColorTextures)
            {
                _textures.AddRange(_colors.Select(CreateTexture2DFromColor));
                _shouldGenerateColorTextures = false;
            }

            var division = Math.Ceiling(Math.Sqrt(_textures.Count));
            var square = (float) ((EditorGUIUtility.currentViewWidth - 10) / division);

            var k = 0;
            for (var i = 0; i < division; i++)
            {
                var rect = EditorGUILayout.GetControlRect(GUILayout.Height(square));
                for (var j = 0; j < division; j++)
                {
                    if (k + 1 > _textures.Count)
                        break;
                    EditorGUI.DrawPreviewTexture(new Rect(rect.x + j * square, rect.y, square, square), _textures[k++]);
                }
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField($"Texture2D References ({_textures.Count} Textures, {_materials.Count} Materials) : ");
            using (new EditorGUI.DisabledGroupScope(true))
            {
                foreach (var texture in _textures)
                    ObjectPicker(string.IsNullOrWhiteSpace(texture.name) ? "Auto Generated" : texture.name, texture);
            }

            return true;
        }

        private bool OnShowConfigurationPage()
        {
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField(@"
アトラス化されたテクスチャおよび Prefab の出力設定を行います。
アトラス化作業によって変更された Prefab は同名で設定したディレクトリに出力されます。
".Trim());
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("グローバル設定");
            EditorGUI.indentLevel++;

            PropertyField(this, nameof(_dest), "出力先ディレクトリ");

            EditorGUI.indentLevel--;

            EditorGUILayout.LabelField("テクスチャー設定");
            EditorGUI.indentLevel++;

            _size = EnumField("出力サイズ (K)", _size);
            _name = EditorGUILayout.TextField("出力ファイル名", _name ?? _original.name);

            EditorGUI.indentLevel--;

            return _dest != null && !string.IsNullOrWhiteSpace(_name);
        }

        private bool OnShowFinalizePage()
        {
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField(@"
設定を確認し、問題なければ「生成」をクリックしてください。
なお、元の Prefab / Material / Texture が変更されることはありません。
".Trim());
            }

            using (new EditorGUI.DisabledGroupScope(true))
            {
                ObjectPicker("対象の Prefab", _original);

                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField($"対象の Renderer Count : {_enabledRenderers.Count}");
                EditorGUILayout.LabelField($"対象の Material Count : {_materials.Count}");
                EditorGUILayout.LabelField($"対象の Texture Count  : {_textures.Count}");
                EditorGUI.indentLevel--;

                EditorGUILayout.LabelField("テクスチャー配置");

                var division = Math.Ceiling(Math.Sqrt(_textures.Count));
                var square = (float) ((EditorGUIUtility.currentViewWidth - 10) / division);

                var k = 0;
                for (var i = 0; i < division; i++)
                {
                    var rect = EditorGUILayout.GetControlRect(GUILayout.Height(square));
                    for (var j = 0; j < division; j++)
                    {
                        if (k + 1 > _textures.Count)
                            break;
                        EditorGUI.DrawPreviewTexture(new Rect(rect.x + j * square, rect.y, square, square), _textures[k++]);
                    }
                }

                EditorGUILayout.LabelField("テクスチャー設定");
                EditorGUI.indentLevel++;
                EnumField("出力サイズ (K)", _size);
                EditorGUILayout.TextField("出力ファイル名", _name);
                EditorGUI.indentLevel--;

                EditorGUILayout.LabelField("グローバル設定");
                EditorGUI.indentLevel++;
                PropertyField(this, nameof(_dest), "出力先ディレクトリ");
                EditorGUI.indentLevel--;
            }

            return true;
        }

        private void OnFinalize()
        {
            var size = 1024;

            switch (_size)
            {
                case AtlasSize.One:
                    size *= 1;
                    break;

                case AtlasSize.Two:
                    size *= 2;
                    break;

                case AtlasSize.Four:
                    size *= 4;
                    break;

                case AtlasSize.Eight:
                    size *= 8;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            var baseDir = AssetDatabase.GetAssetPath(_dest);
            var atlas = CreateAtlasTexture(_textures, size, Path.Combine(baseDir, _name + ".png"));
            CreateAtlasPrefab(_obj, _enabledRenderers, _textures, atlas, Path.Combine(baseDir, _name));
        }

        private void Cleanup()
        {
            DestroyImmediate(_workspace);
            _dest = AssetDatabase.LoadAssetAtPath<DefaultAsset>(AssetDatabase.GUIDToAssetPath(DirectoryGuid));
            _original = null;
            _size = AtlasSize.Four;
            _name = null;
        }

        private static Texture2D CreateTexture2DFromColor(Color color)
        {
            var texture = new Texture2D(128, 128);
            for (var i = 0; i < texture.height; i++)
            for (var j = 0; j < texture.width; j++)
                texture.SetPixel(i, j, color);

            texture.Apply();

            return texture;
        }

        private static Texture2D CreateAtlasTexture(List<Texture2D> textures, int size, string dest)
        {
            var division = (int) Math.Ceiling(Math.Sqrt(textures.Count));
            var square = size / division;
            var atlasTexture = new Texture2D(size, size);

            var currentRenderTexture = RenderTexture.active;

            var k = 0;
            for (var i = division; i > 0; i--)
            {
                var y = i * square - square;
                for (var j = 0; j < division; j++)
                {
                    if (k + 1 > textures.Count)
                        break;

                    var x = j * square;
                    var texture = ResizeTexture(textures[k++], square);

                    for (var m = 0; m < square; m++)
                    for (var n = 0; n < square; n++)
                        atlasTexture.SetPixel(x + n, y + m, texture.GetPixel(n, m));
                }
            }

            RenderTexture.active = currentRenderTexture;

            atlasTexture.Apply(true, false);
            File.WriteAllBytes(dest, atlasTexture.EncodeToPNG());

            DestroyImmediate(atlasTexture);

            AssetDatabase.Refresh();

            var importer = (TextureImporter) AssetImporter.GetAtPath(dest);
            importer.maxTextureSize = size;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Texture2D>(dest);
        }

        private static Texture2D ResizeTexture(Texture2D texture, int size)
        {
            // Graphics.ConvertTexture write pixels to GPU
            var a = new Texture2D(size, size);
            Graphics.ConvertTexture(texture, a);

            // CPU (EncodeToPNG, GetPixel and others) could not read pixels directly from GPU
            var r = RenderTexture.GetTemporary(size, size, 0);
            RenderTexture.active = r;
            Graphics.Blit(a, r);

            var b = new Texture2D(size, size);
            b.ReadPixels(new Rect(0, 0, size, size), 0, 0, false);
            RenderTexture.ReleaseTemporary(r);

            return b;
        }

        private static void CreateAtlasPrefab(GameObject gameObject, List<Renderer> renderers, List<Texture2D> textures, Texture2D atlas, string dest)
        {
            var mat = new Material(Shader.Find("Standard")) { mainTexture = atlas };
            AssetDatabase.CreateAsset(mat, $"{dest}.mat");
            AssetDatabase.Refresh();

            mat = AssetDatabase.LoadAssetAtPath<Material>($"{dest}.mat");

            var division = (int) Math.Ceiling(Math.Sqrt(textures.Count));
            var square = atlas.width / division;
            var meshCaches = new Dictionary<int, Mesh>();
            var meshCounter = 0;

            foreach (var renderer in renderers)
            {
                var r = gameObject.GetComponentsInChildren<Renderer>().First(w => w == renderer);

                if (r is SkinnedMeshRenderer smr)
                    UpdateRendererUVs(smr, division, square, textures, mat, ref meshCounter, meshCaches, dest);

                if (r is MeshRenderer mr)
                    UpdateRendererUVs(mr, division, square, textures, mat, ref meshCounter, meshCaches, dest);
            }

            PrefabUtility.SaveAsPrefabAsset(gameObject, $"{dest}.prefab");
            DestroyImmediate(gameObject);
        }

        private static void UpdateRendererUVs(SkinnedMeshRenderer renderer, int division, int square, List<Texture2D> textures, Material mat, ref int meshCounter, Dictionary<int, Mesh> meshCaches, string dest)
        {
            var mesh = GetOrCreateMeshClone(renderer.sharedMesh, ref meshCounter, meshCaches, dest);
            var uvs = mesh.uv;
            var materials = renderer.sharedMaterials;

            var uvX = (float) square / (division * square);
            var uvY = (float) square / (division * square);

            var alreadyCalculatedIds = new List<int>();

            for (var i = 0; i < mesh.subMeshCount; i++)
            {
                var textureIndex = materials[i].HasProperty("_MainTex") ? textures.Select((w, j) => (Texture: w, Index: j)).FirstOrDefault(w => w.Texture == materials[i].mainTexture).Index : GetColorIndex(materials[i], textures);
                var offsetX = textureIndex % division * square / (float) (division * square);
                var offsetY = 1 - (textureIndex / division + 1) * square / (float) (square * division);

                foreach (var triangle in mesh.GetTriangles(i))
                {
                    if (alreadyCalculatedIds.Contains(triangle))
                        continue;

                    var uv = uvs[triangle];

                    uv.x = uv.x * uvX + offsetX;
                    uv.y = uv.y * uvY + offsetY;

                    alreadyCalculatedIds.Add(triangle);

                    uvs[triangle] = uv;
                }
            }

            mesh.uv = uvs;

            renderer.sharedMesh = mesh;
            renderer.sharedMaterials = Enumerable.Range(0, renderer.sharedMaterials.Length).Select(_ => mat).ToArray();
        }

        private static void UpdateRendererUVs(MeshRenderer renderer, int division, int square, List<Texture2D> textures, Material mat, ref int meshCounter, Dictionary<int, Mesh> meshCaches, string dest)
        {
            var mesh = GetOrCreateMeshClone(renderer.gameObject.GetComponent<MeshFilter>().sharedMesh, ref meshCounter, meshCaches, dest);
            var uvs = mesh.uv;
            var materials = renderer.sharedMaterials;

            var uvX = (float) square / (division * square);
            var uvY = (float) square / (division * square);

            var alreadyCalculatedIds = new List<int>();

            for (var i = 0; i < mesh.subMeshCount; i++)
            {
                var textureIndex = materials[i].HasProperty("_MainTex") ? textures.Select((w, j) => (Texture: w, Index: j)).FirstOrDefault(w => w.Texture == materials[i].mainTexture).Index : GetColorIndex(materials[i], textures);
                var offsetX = textureIndex % division * square / (float) (division * square);
                var offsetY = 1 - (textureIndex / division + 1) * square / (float) (square * division);

                foreach (var triangle in mesh.GetTriangles(i))
                {
                    if (alreadyCalculatedIds.Contains(triangle))
                        continue;

                    var uv = uvs[triangle];

                    uv.x = uv.x * uvX + offsetX;
                    uv.y = uv.y * uvY + offsetY;

                    alreadyCalculatedIds.Add(triangle);

                    uvs[triangle] = uv;
                }
            }

            mesh.uv = uvs;

            renderer.sharedMaterials = Enumerable.Range(0, renderer.sharedMaterials.Length).Select(_ => mat).ToArray();
            renderer.gameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
        }

        private static Mesh GetOrCreateMeshClone(Mesh m, ref int c, Dictionary<int, Mesh> caches, string dest)
        {
            if (caches.ContainsKey(m.GetInstanceID()))
                return caches[m.GetInstanceID()];
            var cloned = Instantiate(m);
            caches.Add(m.GetInstanceID(), cloned);

            AssetDatabase.CreateAsset(cloned, $"{dest}_{c}.asset");
            AssetDatabase.Refresh();

            return AssetDatabase.LoadAssetAtPath<Mesh>($"{dest}_{c++}.asset");
        }

        private static int GetColorIndex(Material mat, List<Texture2D> textures)
        {
            return textures.FindIndex(w => CompareTexture(CreateTexture2DFromColor(mat.color), w));
        }

        private static bool CompareTexture(Texture2D a, Texture2D b)
        {
            var c1 = a.GetPixels();
            var c2 = b.GetPixels();

            if (c1.Length != c2.Length)
                return false;

            return !c1.Where((t, i) => t != c2[i]).Any();
        }

        private enum WizardPages
        {
            Start,

            Initialize,

            SelectRenderers,

            TextureMapping,

            Configuration,

            Finalize
        }

        private enum AtlasSize
        {
            One,

            Two,

            Four,

            Eight
        }

        #region Fields

        private static T EnumField<T>(string label, T value) where T : Enum
        {
            var items = Enum.GetNames(typeof(T))
                            .Select(w => Regex.Replace(w, "(\\B[A-Z])", " $1"))
                            .Select(w => new GUIContent(w))
                            .ToArray();

            return (T) (object) EditorGUILayout.Popup(new GUIContent(label), (int) (object) value, items);
        }

        private static void ErrorField(string error)
        {
            EditorGUILayout.HelpBox(error, MessageType.Error);
        }

        private static T ObjectPicker<T>(string label, T obj) where T : Object
        {
            return EditorGUILayout.ObjectField(new GUIContent(label), obj, typeof(T), true) as T;
        }

        private static void PropertyField(EditorWindow editor, string property, string label = null)
        {
            var so = new SerializedObject(editor);
            so.Update();

            EditorGUILayout.PropertyField(so.FindProperty(property), string.IsNullOrWhiteSpace(label) ? null : new GUIContent(label), true);

            so.ApplyModifiedProperties();
        }

        #endregion

        #region Custom Field

        private class DirectoryFieldAttribute : PropertyAttribute { }

        [CustomPropertyDrawer(typeof(DirectoryFieldAttribute))]
        private class DirectoryFieldDrawer : PropertyDrawer
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

        #endregion
    }
}