using System.Linq;
using OutOfOffice.Bootstrap;
using OutOfOffice.Data;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

namespace OutOfOffice.EditorTools
{
    public static partial class PlaceholderContentBuilder
    {
        static void BuildScene()
        {
            EnsureFolder(SceneDir);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Yeni sahne açmak kullanılmayan asset'leri bellekten atar; bu yüzden
            // asset'ler sahne açıldıktan SONRA yüklenir.
            var config = AssetDatabase.LoadAssetAtPath<GameConfig>(ConfigPath);
            var catalog = AssetDatabase.LoadAssetAtPath<ContentCatalog>(CatalogPath);
            var vfxLibrary = AssetDatabase.LoadAssetAtPath<VfxLibrary>(VfxLibraryPath);
            if (config == null || catalog == null || vfxLibrary == null)
                throw new System.InvalidOperationException("GameConfig, ContentCatalog ya da VfxLibrary bulunamadı.");

            // ---- Işık, gökyüzü, sis, yansıma (PlaceholderContentBuilder.Atmosphere.cs)
            BuildAtmosphere();

            // ---- Kamera
            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.backgroundColor = FogColor;
            camera.fieldOfView = config.baseFieldOfView;
            // Sis 125 m'de tamamen kapanır; ötesini çizmeye gerek yok.
            camera.farClipPlane = 140f;
            camera.allowHDR = true;
            cameraObject.AddComponent<AudioListener>();
            cameraObject.transform.position = config.menuCameraOffset;
            cameraObject.transform.LookAt(config.menuLookAt);

            var cameraData = camera.GetUniversalAdditionalCameraData();
            cameraData.renderPostProcessing = true;
            // FXAA: render ölçeği düşük mobilde kenar tırtıklarını ucuza yumuşatır.
            cameraData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;

            BuildRain(cameraObject.transform);

            // ---- Post-processing
            var volume = new GameObject("Global Volume").AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = BuildVolumeProfile();

            // ---- Arayüz (UI Toolkit)
            var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            if (panel == null)
            {
                panel = ScriptableObject.CreateInstance<PanelSettings>();
                AssetDatabase.CreateAsset(panel, PanelSettingsPath);
            }

            panel.themeStyleSheet = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(ThemePath);
            panel.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            panel.referenceResolution = new Vector2Int(1080, 1920);
            panel.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            panel.match = 0.5f;
            EditorUtility.SetDirty(panel);

            var document = new GameObject("UI").AddComponent<UIDocument>();
            document.panelSettings = panel;
            document.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();

            // ---- Composition root
            var scope = new GameObject("[Game]").AddComponent<GameLifetimeScope>();
            var serialized = new SerializedObject(scope);
            serialized.FindProperty("config").objectReferenceValue = config;
            serialized.FindProperty("catalog").objectReferenceValue = catalog;
            serialized.FindProperty("vfxLibrary").objectReferenceValue = vfxLibrary;
            serialized.FindProperty("gameCamera").objectReferenceValue = camera;
            serialized.FindProperty("uiDocument").objectReferenceValue = document;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, ScenePath);

            // Oyun sahnesi build listesinin başında olsun.
            var others = EditorBuildSettings.scenes.Where(entry => entry.path != ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) }.Concat(others).ToArray();

        }
    }
}
