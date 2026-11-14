using UnityEditor;
using UnityEngine;

namespace OutOfOffice.EditorTools
{
    /// <summary>
    /// Mağaza ve ödül görsellerini üretir (UI/Items). Oyunda modeli olan eşya (ofis sandalyesi,
    /// altın) gerçek modelinden çekilir, yani oyuncu mağazada koşuda göreceği şeyin aynısını
    /// görür. Modeli olmayan eşya (giriş kartı) arayüz temasının diliyle çizilir (UiKitBuilder).
    ///
    /// Sıra: Eşya ve Çevre Modellerini Kur -> Eşya Görsellerini Üret -> Placeholder İçeriği
    /// Yeniden Oluştur (tanım asset'indeki "icon" alanını üretici bağlar).
    /// </summary>
    public static class ItemIconBuilder
    {
        const string OutputDir = "Assets/_Game/UI/Items";
        const int Size = 256;
        const float FieldOfView = 20f;

        readonly struct IconSpec
        {
            /// <summary>Çıktı dosyasının adı.</summary>
            public readonly string Name;
            /// <summary>Art/Props altındaki model (PropModelBuilder).</summary>
            public readonly string Prop;
            /// <summary>Kameranın modelin çevresindeki açısı ve yukarıdan bakışı (derece). 0 = +Z yönünden.</summary>
            public readonly float Yaw, Pitch;

            public IconSpec(string name, string prop, float yaw, float pitch)
            {
                Name = name;
                Prop = prop;
                Yaw = yaw;
                Pitch = pitch;
            }
        }

        static readonly IconSpec[] Icons =
        {
            // Sandalye oyuncuyla aynı yöne (+Z) bakar; önden, hafif yandan ve üstten görülür.
            new IconSpec("OfficeChair", "OfficeChair", 32f, 16f),
            // Altının yüzü gelen oyuncuya (-Z) dönüktür.
            new IconSpec("Coin", "Coin", 205f, 8f),
        };

        [MenuItem("Out of Office/Eşya Görsellerini Üret")]
        public static void BuildAllFromMenu()
        {
            Debug.Log(BuildAll());
        }

        public static string BuildAll()
        {
            ModelSnapshot.EnsureFolder(OutputDir);
            ModelSnapshot.ResetWorldCurve();

            int built = 0;
            foreach (var spec in Icons)
            {
                var icon = Render(spec);
                if (icon == null)
                {
                    Debug.LogWarning($"Model yok, görsel atlandı: {spec.Prop}");
                    continue;
                }

                ModelSnapshot.Save(icon, IconPath(spec.Name), Size);
                Object.DestroyImmediate(icon);
                built++;
            }

            // Modeli olmayan eşya çizilir.
            var card = UiKitBuilder.DrawAccessCard(Size);
            ModelSnapshot.Save(card, IconPath("AccessCard"), Size);
            Object.DestroyImmediate(card);
            built++;

            return $"Eşya görselleri hazır: {built}. Tanım asset'lerine bağlanması için Placeholder İçeriği Yeniden Oluştur'u çalıştır.";
        }

        /// <summary>Adı verilen eşyanın görseli ("OfficeChair"); üretilmemişse null.</summary>
        public static Texture2D Load(string iconName)
        {
            return AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath(iconName));
        }

        static string IconPath(string iconName) { return $"{OutputDir}/{iconName}.png"; }

        static Texture2D Render(IconSpec spec)
        {
            var preview = new PreviewRenderUtility();
            try
            {
                var root = new GameObject(spec.Name);
                preview.AddSingleGO(root);
                if (!PropModelBuilder.TryAddModel(root.transform, spec.Prop, false)) return null;

                // Model kadrajı dolduracak uzaklıktan, sınır küresine göre çerçevelenir.
                var bounds = ModelSnapshot.VisualBounds(root);
                float radius = bounds.extents.magnitude;
                float distance = radius / Mathf.Sin(FieldOfView * 0.5f * Mathf.Deg2Rad) * 0.9f;

                var camera = preview.camera;
                camera.transform.position = bounds.center + Quaternion.Euler(-spec.Pitch, spec.Yaw, 0f) * Vector3.forward * distance;
                camera.transform.LookAt(bounds.center);
                camera.fieldOfView = FieldOfView;
                camera.nearClipPlane = 0.05f;
                camera.farClipPlane = distance + radius * 4f;
                ModelSnapshot.SetupLights(preview, spec.Yaw);

                return ModelSnapshot.Shoot(preview, Size);
            }
            finally
            {
                preview.Cleanup();
            }
        }
    }
}
