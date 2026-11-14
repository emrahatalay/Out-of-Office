using System.IO;
using UnityEditor;
using UnityEngine;

namespace OutOfOffice.EditorTools
{
    /// <summary>
    /// Karakter portrelerini gerçek 3B modellerden üretir: prefab'ı bekleme pozunda, kendi
    /// ışığıyla, saydam zemine çizer ve UI/Portraits altına PNG yazar. Menü ve koleksiyon
    /// kartları bu çizimleri kullanır; model ya da doku değişince aracı yeniden çalıştırmak yeter.
    ///
    /// Sıra: Karakter Modellerini Kur -> Karakter Portrelerini Üret -> Placeholder İçeriği
    /// Yeniden Oluştur (tanım asset'indeki "portrait" alanını üretici bağlar).
    /// </summary>
    public static class PortraitBuilder
    {
        const string PrefabDir = "Assets/_Game/Prefabs/Characters";
        const string OutputDir = "Assets/_Game/UI/Portraits";
        const string PrefabPrefix = "Character_";

        /// <summary>Çıktının kenarı (piksel).</summary>
        const int Size = 512;

        // Kadraj: başın üstünden göğsün altına kadar (boyun bu oranı), üç çeyrek önden.
        const float FramedBodyFraction = 0.46f;
        const float Headroom = 0.05f;
        // Bekleme klibinde karakter -X'e (menü kamerasına) dönük durur; kamera o yanın biraz
        // önüne konur. Klibin yönü değişirse bu açı da değişmelidir.
        const float Yaw = -66f;
        const float FieldOfView = 20f;
        /// <summary>Bekleme klibinin portrede kullanılan anı (0-1).</summary>
        const float PoseTime = 0.35f;

        [MenuItem("Out of Office/Karakter Portrelerini Üret")]
        public static void BuildAllFromMenu()
        {
            Debug.Log(BuildAll());
        }

        public static string BuildAll()
        {
            ModelSnapshot.EnsureFolder(OutputDir);
            ModelSnapshot.ResetWorldCurve();

            int built = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { PrefabDir }))
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                string prefabName = Path.GetFileNameWithoutExtension(prefabPath);
                if (!prefabName.StartsWith(PrefabPrefix)) continue;

                var portrait = Render(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath));
                ModelSnapshot.Save(portrait, PortraitPath(prefabName.Substring(PrefabPrefix.Length)), Size);
                Object.DestroyImmediate(portrait);
                built++;
            }

            return $"Portreler hazır: {built}. Tanım asset'lerine bağlanması için Placeholder İçeriği Yeniden Oluştur'u çalıştır.";
        }

        /// <summary>Adı verilen karakterin portresi ("Developer"); üretilmemişse null.</summary>
        public static Texture2D Load(string characterName)
        {
            return AssetDatabase.LoadAssetAtPath<Texture2D>(PortraitPath(characterName));
        }

        static string PortraitPath(string characterName) { return $"{OutputDir}/{characterName}.png"; }

        static Texture2D Render(GameObject prefab)
        {
            var preview = new PreviewRenderUtility();
            try
            {
                var instance = preview.InstantiatePrefabInScene(prefab);
                instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                Pose(instance);

                var bounds = ModelSnapshot.VisualBounds(instance);
                float height = bounds.size.y;
                float span = height * (FramedBodyFraction + Headroom);
                var target = new Vector3(bounds.center.x, bounds.max.y + height * Headroom - span * 0.5f, bounds.center.z);
                float distance = span * 0.5f / Mathf.Tan(FieldOfView * 0.5f * Mathf.Deg2Rad);

                var camera = preview.camera;
                camera.transform.position = target + Quaternion.Euler(0f, Yaw, 0f) * Vector3.forward * distance;
                camera.transform.LookAt(target);
                camera.fieldOfView = FieldOfView;
                camera.nearClipPlane = 0.1f;
                camera.farClipPlane = distance + height * 4f;
                ModelSnapshot.SetupLights(preview, Yaw);

                // İlk çizimde baş kadrajın ortasına denk gelmeyebilir (duruşta kollar ve çanta bir
                // yana taşar). Üst yarının ağırlık merkezi ölçülür, kamera o kadar kaydırılıp yeniden çizilir.
                var draft = ModelSnapshot.Shoot(preview, Size);
                float offset = HeadOffset(draft);
                Object.DestroyImmediate(draft);
                camera.transform.position += camera.transform.right * (offset * span / Size);
                return ModelSnapshot.Shoot(preview, Size);
            }
            finally
            {
                preview.Cleanup();
            }
        }

        /// <summary>Çizimin üst yarısındaki (baş ve omuzlar) dolu piksellerin yatay merkezinin ortadan sapması, piksel.</summary>
        static float HeadOffset(Texture2D image)
        {
            var pixels = image.GetPixels32();
            double sum = 0, weight = 0;
            for (int y = Size / 2; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    int alpha = pixels[y * Size + x].a;
                    sum += (double)alpha * (x + 0.5);
                    weight += alpha;
                }
            }

            return weight > 0 ? (float)(sum / weight - Size * 0.5) : 0f;
        }

        /// <summary>Modeli T pozundan çıkarıp bekleme pozuna alır (Animator düzenleme modunda bir kez örneklenir).</summary>
        static void Pose(GameObject instance)
        {
            var animator = instance.GetComponentInChildren<Animator>();
            if (animator == null || animator.runtimeAnimatorController == null) return;

            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.Rebind();
            animator.Play("Idle", 0, PoseTime);
            animator.Update(0f);
        }
    }
}
