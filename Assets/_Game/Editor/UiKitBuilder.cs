using System.IO;
using UnityEditor;
using UnityEngine;

namespace OutOfOffice.EditorTools
{
    /// <summary>
    /// Arayüz temasının görsellerini koddan üretir: panel, kart yuvası, butonlar, seçim
    /// çerçevesi ve portre ışığı. Hepsi 9 dilimli PNG'dir ve 2x çizilir (USS'te
    /// -unity-slice-scale 0.5px). Biçim dili oyunun kendisiyle aynıdır: kalın koyu dış çizgi,
    /// tombul yuvarlak köşe, altta kalın taban, üstte parlama (docs/TASARIM.md).
    ///
    /// Rengi ya da biçimi değiştirmek için buradaki tabloyu değiştir ve aracı yeniden çalıştır;
    /// dosya adları aynı kaldığı için USS'e dokunmak gerekmez (köşe yarıçapı değişirse USS'teki
    /// dilim değerleri de güncellenmelidir).
    /// </summary>
    public static class UiKitBuilder
    {
        const string SpriteDir = "Assets/_Game/UI/Sprites";

        // Dış çizgi ve gölgenin rengi; USS'teki --color-ink ile aynıdır.
        static readonly Color Ink = Hex(0x12163d);

        sealed class Plate
        {
            public int Size = 256;
            /// <summary>Şeklin çevresindeki boş pay; gölge buraya düşer.</summary>
            public float Padding = 22f;
            public float Radius = 60f;
            public float Outline = 8f;
            public Color OutlineColor = Ink;
            /// <summary>Gövdenin üst ve alt rengi (düşey geçiş).</summary>
            public Color Top, Bottom;
            /// <summary>Gövdenin altından görünen tabanın kalınlığı ve rengi.</summary>
            public float Base = 18f;
            public Color BaseColor;
            /// <summary>Üst yarıdaki parlamanın gücü (0 = yok).</summary>
            public float Gloss;
            /// <summary>Üst kenarın içindeki ince açık çizginin gücü.</summary>
            public float Rim = 0.3f;
            public float ShadowBlur = 16f, ShadowOffset = 8f, ShadowAlpha = 0.3f;
            /// <summary>İçi boş çerçeve: yalnızca dış çizgi ve çevresinde ışıma çizilir.</summary>
            public bool Hollow;
            public float GlowBlur, GlowAlpha;
        }

        [MenuItem("Out of Office/Arayüz Görsellerini Üret")]
        public static void BuildAllFromMenu()
        {
            Debug.Log(BuildAll());
        }

        public static string BuildAll()
        {
            int count = 0;

            // Büyük panel: sayfalar ve diyaloglar.
            count += Write("panel", Draw(new Plate
            {
                Size = 320, Padding = 30f, Radius = 72f,
                Top = Hex(0x3f4bb8), Bottom = Hex(0x2c3492), Base = 14f, BaseColor = Hex(0x1c2265),
                Rim = 0.28f, ShadowBlur = 22f, ShadowAlpha = 0.35f,
            }), 320);

            // Panelin içindeki yuva: koleksiyon kartı, mağaza satırı, isim plakası.
            count += Write("slot", Draw(new Plate
            {
                Padding = 14f, Radius = 48f, Outline = 6f,
                Top = Hex(0x5562d6), Bottom = Hex(0x4450bd), Base = 10f, BaseColor = Hex(0x262d86),
                Rim = 0.32f, ShadowBlur = 10f, ShadowOffset = 4f, ShadowAlpha = 0.25f,
            }), 256);

            // Butonlar: kırmızı ana eylem, sarı ödül / satın alma, mavi ikincil.
            count += Write("btn_primary", Draw(Button(Hex(0xff7888), Hex(0xf23a52), Hex(0xa51c36))), 256);
            count += Write("btn_accent", Draw(Button(Hex(0xffe27a), Hex(0xffb81f), Hex(0xc27a00))), 256);
            count += Write("btn_secondary", Draw(Button(Hex(0x7485ff), Hex(0x4856e8), Hex(0x262e96))), 256);

            // Seçili kartın çerçevesi.
            count += Write("frame_selected", Draw(new Plate
            {
                Padding = 20f, Radius = 54f, Outline = 10f, OutlineColor = Hex(0xffc83d),
                Hollow = true, GlowBlur = 16f, GlowAlpha = 0.55f, ShadowAlpha = 0f, Base = 0f,
            }), 256);

            // Portrenin arkasındaki yumuşak ışık.
            count += Write("spot", DrawSpot(256, 0.6f), 256);

            AssetDatabase.Refresh();
            PlaceholderContentBuilder.ConfigureUiSprites();
            return $"Arayüz görselleri hazır: {count}. Klasör: {SpriteDir}";
        }

        static Plate Button(Color top, Color bottom, Color baseColor)
        {
            return new Plate { Top = top, Bottom = bottom, BaseColor = baseColor, Gloss = 0.24f, Rim = 0.5f };
        }

        // ------------------------------------------------------------ Çizim

        static Color32[] Draw(Plate plate)
        {
            int size = plate.Size;
            var pixels = new Color32[size * size];

            // Görüntü koordinatı: x sağa, y aşağı. Gövde üstte, taban onun altında, gölge en altta.
            float centerX = size * 0.5f;
            float halfWidth = size * 0.5f - plate.Padding;
            float top = plate.Padding;
            float bottom = size - plate.Padding - plate.Base;
            float centerY = (top + bottom) * 0.5f;
            float halfHeight = (bottom - top) * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float px = x + 0.5f, py = y + 0.5f;
                    float body = RoundedBox(px - centerX, py - centerY, halfWidth, halfHeight, plate.Radius);
                    float lowered = RoundedBox(px - centerX, py - centerY - plate.Base, halfWidth, halfHeight, plate.Radius);
                    float shape = Mathf.Min(body, lowered);

                    // Renk alfayla önceden çarpılmış olarak biriktirilir.
                    var color = new Color(0f, 0f, 0f, 0f);

                    if (plate.ShadowAlpha > 0f)
                    {
                        float shadow = RoundedBox(px - centerX, py - centerY - plate.Base - plate.ShadowOffset, halfWidth, halfHeight, plate.Radius);
                        Over(ref color, Ink, plate.ShadowAlpha * (1f - Mathf.SmoothStep(-plate.ShadowBlur * 0.5f, plate.ShadowBlur, shadow)));
                    }

                    if (plate.Hollow)
                    {
                        float ring = Mathf.Abs(body + plate.Outline * 0.5f) - plate.Outline * 0.5f;
                        Over(ref color, plate.OutlineColor, plate.GlowAlpha * (1f - Mathf.SmoothStep(0f, plate.GlowBlur, ring)));
                        Over(ref color, plate.OutlineColor, Coverage(ring));
                    }
                    else
                    {
                        Over(ref color, plate.OutlineColor, Coverage(shape));
                        Over(ref color, plate.BaseColor, Coverage(shape + plate.Outline));

                        float inner = body + plate.Outline;
                        float t = Mathf.Clamp01((py - top) / (bottom - top));
                        Over(ref color, Color.Lerp(plate.Top, plate.Bottom, t), Coverage(inner));

                        if (plate.Gloss > 0f)
                        {
                            // Üst yarıyı kaplayan, aşağı doğru sönen açık bant.
                            float inset = plate.Outline + 8f;
                            float glossHalfHeight = halfHeight * 0.5f - inset * 0.5f;
                            float gloss = RoundedBox(px - centerX, py - (top + inset + glossHalfHeight), halfWidth - inset, glossHalfHeight, Mathf.Max(4f, plate.Radius - inset));
                            float fade = 1f - Mathf.Clamp01((py - top - inset) / (glossHalfHeight * 2f)) * 0.55f;
                            Over(ref color, Color.white, Coverage(gloss) * plate.Gloss * fade);
                        }

                        if (plate.Rim > 0f)
                        {
                            float rim = Coverage(inner) - Coverage(inner + 4f);
                            float upper = 1f - Mathf.Clamp01((py - top) / halfHeight);
                            Over(ref color, Color.white, rim * plate.Rim * upper);
                        }
                    }

                    pixels[(size - 1 - y) * size + x] = Straight(color);
                }
            }

            return pixels;
        }

        static Color32[] DrawSpot(int size, float strength)
        {
            var pixels = new Color32[size * size];
            float half = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Mathf.Sqrt((x + 0.5f - half) * (x + 0.5f - half) + (y + 0.5f - half) * (y + 0.5f - half)) / half;
                    float alpha = 1f - Mathf.SmoothStep(0f, 1f, distance);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha * alpha * strength);
                }
            }

            return pixels;
        }

        /// <summary>
        /// Giriş kartı (devam hakkı): boyun askılı personel kimliği. Oyunda modeli olmadığı için
        /// temanın diliyle çizilir: kalın koyu dış çizgi, alt taban, camgöbeği (teknoloji) vurgu.
        /// Dokuda yazı yoktur; satırlar çizgiyle anlatılır.
        /// </summary>
        internal static Texture2D DrawAccessCard(int size)
        {
            var cardWhite = Hex(0xf4f7ff);
            var cardShade = Hex(0xc3cbea);
            var tech = Hex(0x4ee3ff);
            var techDark = Hex(0x1aa6cf);
            var strap = Hex(0xff8a3d);
            var line = Hex(0x9aa5d6);

            // Ölçüler 256 piksellik tuvale göredir; başka boyutta aynı oranla ölçeklenir.
            float unit = size / 256f;
            float outline = 7f * unit;
            const float tilt = -9f;
            float cos = Mathf.Cos(tilt * Mathf.Deg2Rad), sin = Mathf.Sin(tilt * Mathf.Deg2Rad);

            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Tuvalin ortasına göre, kartla birlikte döndürülmüş koordinat (birim: 256'lık tuval).
                    float dx = (x + 0.5f) / unit - 128f, dy = (y + 0.5f) / unit - 128f;
                    float px = dx * cos - dy * sin, py = dx * sin + dy * cos;

                    var color = new Color(0f, 0f, 0f, 0f);

                    // Gövde: dikey kart, altında taban.
                    float body = RoundedBox(px, py - 18f, 72f, 88f, 22f);
                    float lowered = RoundedBox(px, py - 28f, 72f, 88f, 22f);
                    float shape = Mathf.Min(body, lowered);

                    Over(ref color, Ink, 0.3f * (1f - Mathf.SmoothStep(-6f, 14f, RoundedBox(px, py - 36f, 72f, 88f, 22f))));

                    // Boyun askısı: kartın üstünden tuvalin dışına çıkar.
                    float strapShape = RoundedBox(px, py + 104f, 15f, 44f, 6f);
                    Over(ref color, Ink, Coverage(strapShape * unit));
                    Over(ref color, strap, Coverage((strapShape + 7f) * unit));

                    Over(ref color, Ink, Coverage(shape * unit));
                    Over(ref color, cardShade, Coverage((shape + 7f) * unit));
                    Over(ref color, cardWhite, Coverage((body + 7f) * unit));

                    // Üst bant (camgöbeği) ve askının geçtiği yuva.
                    float band = Mathf.Max(body + 7f, py + 38f);
                    Over(ref color, tech, Coverage(band * unit));
                    Over(ref color, Ink, Coverage(RoundedBox(px, py + 54f, 20f, 6f, 6f) * unit));

                    // Fotoğraf: camgöbeği kare, içinde baş ve omuz.
                    float photo = RoundedBox(px + 22f, py - 4f, 30f, 32f, 10f);
                    Over(ref color, Ink, Coverage(photo * unit));
                    Over(ref color, techDark, Coverage((photo + 5f) * unit));
                    float head = new Vector2(px + 22f, py + 12f).magnitude - 11f;
                    float shoulders = Mathf.Max(new Vector2(px + 22f, py - 30f).magnitude - 24f, photo + 5f);
                    Over(ref color, cardWhite, Coverage(head * unit));
                    Over(ref color, cardWhite, Coverage(shoulders * unit));

                    // Satırlar (yazı yerine çizgi) ve alttaki çip.
                    Over(ref color, line, Coverage(RoundedBox(px - 38f, py + 14f, 18f, 5f, 5f) * unit));
                    Over(ref color, line, Coverage(RoundedBox(px - 42f, py - 6f, 14f, 5f, 5f) * unit));
                    Over(ref color, line, Coverage(RoundedBox(px, py - 52f, 50f, 5f, 5f) * unit));
                    Over(ref color, tech, Coverage(RoundedBox(px - 16f, py - 76f, 34f, 9f, 6f) * unit));
                    Over(ref color, Ink, Coverage(RoundedBox(px + 40f, py - 76f, 9f, 9f, 9f) * unit));

                    pixels[(size - 1 - y) * size + x] = Straight(color);
                }
            }

            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }

        /// <summary>Yuvarlak köşeli kutunun işaretli uzaklığı: içeride eksi, dışarıda artı (piksel).</summary>
        static float RoundedBox(float x, float y, float halfWidth, float halfHeight, float radius)
        {
            radius = Mathf.Min(radius, Mathf.Min(halfWidth, halfHeight));
            float qx = Mathf.Abs(x) - (halfWidth - radius);
            float qy = Mathf.Abs(y) - (halfHeight - radius);
            float outside = new Vector2(Mathf.Max(qx, 0f), Mathf.Max(qy, 0f)).magnitude;
            return outside + Mathf.Min(Mathf.Max(qx, qy), 0f) - radius;
        }

        /// <summary>Uzaklığı bir piksellik yumuşak kenara çevirir.</summary>
        static float Coverage(float distance)
        {
            return Mathf.Clamp01(0.5f - distance);
        }

        static void Over(ref Color destination, Color source, float alpha)
        {
            alpha = Mathf.Clamp01(alpha * source.a);
            destination.r = source.r * alpha + destination.r * (1f - alpha);
            destination.g = source.g * alpha + destination.g * (1f - alpha);
            destination.b = source.b * alpha + destination.b * (1f - alpha);
            destination.a = alpha + destination.a * (1f - alpha);
        }

        static Color32 Straight(Color premultiplied)
        {
            if (premultiplied.a <= 0.0001f) return new Color32(0, 0, 0, 0);
            return new Color(premultiplied.r / premultiplied.a, premultiplied.g / premultiplied.a, premultiplied.b / premultiplied.a, premultiplied.a);
        }

        static Color Hex(int rgb)
        {
            return new Color32((byte)(rgb >> 16), (byte)((rgb >> 8) & 0xff), (byte)(rgb & 0xff), 255);
        }

        static int Write(string name, Color32[] pixels, int size)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.SetPixels32(pixels);
            File.WriteAllBytes($"{SpriteDir}/{name}.png", texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            return 1;
        }
    }
}
