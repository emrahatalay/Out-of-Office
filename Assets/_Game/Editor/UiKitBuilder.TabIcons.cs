using UnityEngine;

namespace OutOfOffice.EditorTools
{
    /// <summary>
    /// Menünün alt sekme ikonları: tek renkli çizgi ikon değil, oyunun diliyle renkli küçük çizimler
    /// (kalın koyu dış çizgi, düşey renk geçişi, üstte parlama, altta yumuşak gölge).
    /// Ana Sayfa = kırmızı çatılı ofis evi, Koleksiyon = kravatlı çalışan, Mağaza = kurdeleli hediye,
    /// Ayarlar = metal dişli. Renkler temanın anlamına uyar (docs/TASARIM.md). Dokuda yazı yok.
    /// Ölçüler 256 birimlik tuvale göredir; y aşağı doğru artar.
    /// </summary>
    public static partial class UiKitBuilder
    {
        const int TabIconSize = 192;
        const float IconOutline = 9f;

        delegate void IconPainter(float x, float y, ref Color color, float unit);

        static int BuildTabIcons()
        {
            int count = 0;
            count += Write("tab_home", DrawIcon(TabIconSize, PaintHome), TabIconSize);
            count += Write("tab_collection", DrawIcon(TabIconSize, PaintCollection), TabIconSize);
            count += Write("tab_store", DrawIcon(TabIconSize, PaintStore), TabIconSize);
            count += Write("tab_settings", DrawIcon(TabIconSize, PaintSettings), TabIconSize);
            return count;
        }

        static Color32[] DrawIcon(int size, IconPainter paint)
        {
            float unit = size / 256f;
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float px = (x + 0.5f) / unit - 128f, py = (y + 0.5f) / unit - 128f;
                    var color = new Color(0f, 0f, 0f, 0f);
                    paint(px, py, ref color, unit);
                    pixels[(size - 1 - y) * size + x] = Straight(color);
                }
            }

            return pixels;
        }

        // ------------------------------------------------------------ Ana Sayfa: ofis evi

        static void PaintHome(float x, float y, ref Color c, float u)
        {
            GroundShadow(x, y, ref c, 92f);

            // Baca (çatının arkasında).
            Layer(ref c, RoundedBox(x + 50f, y + 62f, 14f, 26f, 5f), Gradient(y, -88f, -40f, Hex(0xff7888), Hex(0xc92a44)), u);

            // Duvar: krem, alta doğru sıcaklaşır.
            float wall = RoundedBox(x, y - 40f, 72f, 58f, 14f);
            Layer(ref c, wall, Gradient(y, -10f, 98f, Hex(0xfff6e4), Hex(0xf0c49a)), u);

            // Çatı: kırmızı üçgen, yuvarlatılmış; üstte parlama.
            float roof = Triangle(x, y + 2f, 102f, 94f) - 10f;
            Layer(ref c, roof, Gradient(y, -100f, 0f, Hex(0xff8a98), Hex(0xe8344c)), u);
            Shine(ref c, Triangle(x + 18f, y + 30f, 46f, 42f) - 4f, u, 0.35f);

            // Kapı: sarı, topuzlu.
            float door = RoundedBox(x - 24f, y - 66f, 20f, 34f, 9f);
            Layer(ref c, door, Gradient(y, 30f, 100f, Hex(0xffe27a), Hex(0xf0a21a)), u, 7f);
            Over(ref c, Ink, Coverage((Circle(x - 34f, y - 68f, 4.5f)) * u));

            // Pencere: ışıyan camgöbeği, çerçeve çizgili.
            float window = RoundedBox(x + 30f, y - 26f, 20f, 20f, 6f);
            Layer(ref c, window, Gradient(y, 6f, 46f, Hex(0xb8f6ff), Hex(0x2fc6ee)), u, 7f);
            float cross = Mathf.Max(window + 7f, Mathf.Min(Mathf.Abs(x + 30f) - 2.5f, Mathf.Abs(y - 26f) - 2.5f));
            Over(ref c, Ink, Coverage(cross * u));
        }

        // ------------------------------------------------------------ Koleksiyon: kravatlı çalışan

        static void PaintCollection(float x, float y, ref Color c, float u)
        {
            GroundShadow(x, y, ref c, 88f);

            var skin = Gradient(y, -80f, 10f, Hex(0xffdcb8), Hex(0xf1a878));

            // Gövde: mavi gömlek / ceket, omuzlar yuvarlak, alttan tuvalin dışına taşar.
            float body = RoundedBox(x, y - 82f, 84f, 48f, 44f);
            Layer(ref c, body, Gradient(y, 36f, 126f, Hex(0x7d8cff), Hex(0x3f4bd6)), u);

            // Yaka (beyaz üçgenler) ve turuncu kravat.
            float collar = Mathf.Min(Polygon(x, y, CollarLeft), Polygon(x, y, CollarRight));
            Layer(ref c, collar, Hex(0xf4f7ff), u, 6f);
            Layer(ref c, Polygon(x, y, Tie), Gradient(y, 40f, 120f, Hex(0xffa25a), Hex(0xe8601a)), u, 6f);

            // Boyun ve baş.
            Layer(ref c, RoundedBox(x, y - 28f, 18f, 22f, 8f), Hex(0xe99a6a), u, 7f);
            float head = Circle(x, y + 30f, 46f);
            Layer(ref c, head, skin, u);

            // Saç: kahverengi, başın üst yarısı ve kıvırcık tepe.
            float hair = Mathf.Max(Circle(x, y + 38f, 50f), y + 36f);
            hair = Mathf.Min(hair, Circle(x + 30f, y + 70f, 20f));
            hair = Mathf.Min(hair, Circle(x - 2f, y + 80f, 22f));
            hair = Mathf.Min(hair, Circle(x - 32f, y + 66f, 20f));
            Layer(ref c, hair, Gradient(y, -104f, -30f, Hex(0xa65a2c), Hex(0x6a3214)), u);
            Shine(ref c, Circle(x + 14f, y + 78f, 10f), u, 0.3f);

            // Yüz: gözler, yanaklar, gülümseme.
            Over(ref c, Ink, Coverage(Circle(x - 16f, y + 20f, 6.5f) * u));
            Over(ref c, Ink, Coverage(Circle(x + 16f, y + 20f, 6.5f) * u));
            Over(ref c, Hex(0xff8f8f), 0.55f * Coverage(Circle(x - 28f, y + 6f, 8f) * u));
            Over(ref c, Hex(0xff8f8f), 0.55f * Coverage(Circle(x + 28f, y + 6f, 8f) * u));
            float smile = Mathf.Max(Mathf.Abs(Circle(x, y + 8f, 14f)) - 2.2f, -(y + 2f));
            Over(ref c, Ink, Coverage(smile * u));

            // Camgöbeği yaka kartı (oyunun giriş kartı).
            Layer(ref c, RoundedBox(x - 46f, y - 82f, 13f, 16f, 4f), Hex(0x4ee3ff), u, 6f);
        }

        static readonly Vector2[] CollarLeft = { new Vector2(-22f, 40f), new Vector2(0f, 40f), new Vector2(-4f, 64f), new Vector2(-30f, 58f) };
        static readonly Vector2[] CollarRight = { new Vector2(22f, 40f), new Vector2(0f, 40f), new Vector2(4f, 64f), new Vector2(30f, 58f) };
        static readonly Vector2[] Tie = { new Vector2(-8f, 46f), new Vector2(8f, 46f), new Vector2(12f, 104f), new Vector2(0f, 118f), new Vector2(-12f, 104f) };

        // ------------------------------------------------------------ Mağaza: kurdeleli hediye

        static void PaintStore(float x, float y, ref Color c, float u)
        {
            GroundShadow(x, y, ref c, 88f);

            // Kutu ve kapak: canlı mor (sürpriz paketin rengi), kurdele sarı (ödül).
            float box = RoundedBox(x, y - 46f, 74f, 52f, 12f);
            Layer(ref c, box, Gradient(y, -6f, 100f, Hex(0xc77dff), Hex(0x7b34d6)), u);
            float lid = RoundedBox(x, y + 10f, 88f, 22f, 10f);
            Layer(ref c, lid, Gradient(y, -32f, 12f, Hex(0xdc9cff), Hex(0x9a4be6)), u);
            Shine(ref c, RoundedBox(x + 40f, y + 20f, 30f, 5f, 5f), u, 0.4f);

            var gold = Gradient(y, -60f, 100f, Hex(0xffe27a), Hex(0xf5a812));
            Layer(ref c, Mathf.Max(RoundedBox(x, y - 46f, 15f, 52f, 3f), box), gold, u, 7f);
            Layer(ref c, Mathf.Max(RoundedBox(x, y + 10f, 15f, 22f, 3f), lid), gold, u, 7f);

            // Fiyonk: iki ilmek ve düğüm.
            float loopLeft = Ellipse(x + 30f, y + 52f, 30f, 20f);
            float loopRight = Ellipse(x - 30f, y + 52f, 30f, 20f);
            Layer(ref c, loopLeft, gold, u);
            Layer(ref c, loopRight, gold, u);
            Over(ref c, Hex(0xc27a00), Coverage((Ellipse(x + 30f, y + 52f, 13f, 7f)) * u));
            Over(ref c, Hex(0xc27a00), Coverage((Ellipse(x - 30f, y + 52f, 13f, 7f)) * u));
            Layer(ref c, Circle(x, y + 40f, 14f), gold, u, 7f);

            // Pırıltı: camgöbeği dört köşeli yıldız.
            Layer(ref c, Sparkle(x - 78f, y + 76f, 20f), Hex(0x8ff0ff), u, 5f);
        }

        // ------------------------------------------------------------ Ayarlar: metal dişli

        static void PaintSettings(float x, float y, ref Color c, float u)
        {
            GroundShadow(x, y, ref c, 80f);

            float gear = GearShape(x, y, 62f, 8, 18f, 21f);
            var steel = Gradient(y, -96f, 96f, Hex(0xeef2ff), Hex(0x7d89c2));
            Layer(ref c, gear, steel, u);
            Shine(ref c, Mathf.Max(Circle(x, y, 52f), y + 6f), u, 0.18f);

            // İç halka (gömük): koyu, sonra camgöbeği ışıyan göbek.
            Layer(ref c, Circle(x, y, 36f), Gradient(y, -36f, 36f, Hex(0x5a66a8), Hex(0x9aa5dc)), u, 7f);
            Layer(ref c, Circle(x, y, 22f), Gradient(y, -22f, 22f, Hex(0xb8f6ff), Hex(0x24bfe8)), u, 7f);
            Shine(ref c, Circle(x + 7f, y + 8f, 7f), u, 0.7f);
        }

        // ------------------------------------------------------------ Çizim yardımcıları

        /// <summary>Koyu dış çizgi + dolgu. Uzaklık birim cinsinden (içeride eksi).</summary>
        static void Layer(ref Color c, float distance, Color fill, float unit, float outline = IconOutline)
        {
            Over(ref c, Ink, Coverage(distance * unit));
            Over(ref c, fill, Coverage((distance + outline) * unit));
        }

        /// <summary>Şeklin üstüne yarı saydam beyaz parlama.</summary>
        static void Shine(ref Color c, float distance, float unit, float strength)
        {
            Over(ref c, Color.white, Coverage(distance * unit) * strength);
        }

        static void GroundShadow(float x, float y, ref Color c, float halfWidth)
        {
            float d = Ellipse(x, y - 112f, halfWidth, 12f);
            Over(ref c, Ink, 0.28f * (1f - Mathf.SmoothStep(-8f, 10f, d)));
        }

        static Color Gradient(float y, float top, float bottom, Color upper, Color lower)
        {
            return Color.Lerp(upper, lower, Mathf.InverseLerp(top, bottom, y));
        }

        static float Circle(float x, float y, float radius)
        {
            return Mathf.Sqrt(x * x + y * y) - radius;
        }

        /// <summary>Elips için yaklaşık uzaklık (kenar yumuşatmaya yeter).</summary>
        static float Ellipse(float x, float y, float rx, float ry)
        {
            float k = Mathf.Sqrt(x * x / (rx * rx) + y * y / (ry * ry));
            return (k - 1f) * Mathf.Min(rx, ry);
        }

        /// <summary>Tepesi yukarıda ikizkenar üçgen: taban yarı genişliği ve yükseklik; taban y=0'da.</summary>
        static float Triangle(float x, float y, float halfBase, float height)
        {
            // Koordinat: taban y=0, tepe y=-height (y aşağı).
            var a = new Vector2(-halfBase, 0f);
            var b = new Vector2(halfBase, 0f);
            var t = new Vector2(0f, -height);
            return PolygonDistance(new Vector2(x, y), a, t, b);
        }

        static float PolygonDistance(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            TriVerts[0] = a;
            TriVerts[1] = b;
            TriVerts[2] = c;
            return Polygon(p.x, p.y, TriVerts);
        }

        static readonly Vector2[] TriVerts = new Vector2[3];

        /// <summary>Çokgenin işaretli uzaklığı (köşeler sırayla).</summary>
        static float Polygon(float x, float y, Vector2[] v)
        {
            var p = new Vector2(x, y);
            float d = Vector2.SqrMagnitude(p - v[0]);
            float sign = 1f;
            for (int i = 0, j = v.Length - 1; i < v.Length; j = i, i++)
            {
                var e = v[j] - v[i];
                var w = p - v[i];
                var b = w - e * Mathf.Clamp01(Vector2.Dot(w, e) / Vector2.Dot(e, e));
                d = Mathf.Min(d, Vector2.Dot(b, b));
                bool c1 = p.y >= v[i].y, c2 = p.y < v[j].y, c3 = e.x * w.y > e.y * w.x;
                if ((c1 && c2 && c3) || (!c1 && !c2 && !c3)) sign = -sign;
            }

            return sign * Mathf.Sqrt(d);
        }

        /// <summary>Dişli: gövde yarıçapı, diş sayısı, diş boyu ve yarı genişliği.</summary>
        static float GearShape(float x, float y, float radius, int teeth, float toothLength, float toothHalfWidth)
        {
            float body = Circle(x, y, radius);
            float angle = Mathf.Atan2(y, x);
            float sector = 2f * Mathf.PI / teeth;
            float local = Mathf.Repeat(angle + sector * 0.5f, sector) - sector * 0.5f;
            float r = Mathf.Sqrt(x * x + y * y);
            float lx = Mathf.Cos(local) * r, ly = Mathf.Sin(local) * r;
            float tooth = RoundedBox(lx - radius - toothLength * 0.4f, ly, toothLength, toothHalfWidth, 6f);
            return Mathf.Min(body, tooth);
        }

        /// <summary>Dört köşeli pırıltı yıldızı.</summary>
        static float Sparkle(float x, float y, float size)
        {
            float ax = Mathf.Abs(x), ay = Mathf.Abs(y);
            // İçbükey kenarlı yıldız: |x|^0.5 + |y|^0.5 = size^0.5
            float k = Mathf.Sqrt(ax) + Mathf.Sqrt(ay) - Mathf.Sqrt(size);
            return k * Mathf.Sqrt(size) * 0.9f;
        }
    }
}
