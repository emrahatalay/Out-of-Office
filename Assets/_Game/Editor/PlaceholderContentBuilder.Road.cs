using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace OutOfOffice.EditorTools
{
    /// <summary>
    /// Akıllı yol: açık gri-lila asfalt, kesikli şerit çizgileri, kenar çizgileri, her şeridin
    /// ortasında ışıyan ileri okları, kenar çizgisi boyunca ışıyan kılavuz noktaları; yanlarda
    /// yükseltilmiş bordür ve taş kaldırım. Dokular koddan çizilir (Art/Road), elle düzenlenmez:
    /// değiştirmek için buradaki sayıları değiştirip üreticiyi yeniden çalıştır.
    ///
    /// Asfalt dokusu yolun tam genişliğini (U) ve 10 m'lik bir boyu (V) kaplar, V yönünde tekrar
    /// eder. Işıma maskesi dokunun alfasındadır (Curved Lit kuralı: alfa 0 = tam ışır).
    /// </summary>
    public static partial class PlaceholderContentBuilder
    {
        const string RoadDir = Root + "/Art/Road";

        // Ölçüler (m). Yol x = -RoadHalfWidth..+RoadHalfWidth, zemin y = 0.
        public const float RoadHalfWidth = 4.25f;
        const float RoadTileLength = 10f;
        const float CurbWidth = 0.35f;
        const float CurbHeight = 0.2f;
        /// <summary>Kaldırımın üst yüzü; yan dekor bu yükseklikte durur.</summary>
        const float SidewalkTop = 0.16f;
        const float SidewalkOuter = 22f;
        const float SidewalkTileSize = 2f;

        const int RoadTextureWidth = 1024;
        const int RoadTextureHeight = 1024;
        const int SidewalkTextureSize = 512;

        /// <summary>
        /// Bir dünyanın zemin görünümü: asfalt, çizgi ve ok renkleri, kaldırım, bina paleti. Zemin kurulurken
        /// <see cref="_style"/> bu stile ayarlanır; doku ve materyal adları sonekle ayrışır.
        /// </summary>
        sealed class GroundStyle
        {
            public string Name;
            public string Suffix;
            public Color Asphalt, Marking, Arrow, Stud, SlabA, SlabB, Joint;
            public Color LaneGlow;
            public Color[] Towers;
            public Color ColumnTint;
        }

        // Plaza: açık, serin asfalt (koyu gövdeli engeller ve renkli karakterler üstünde okunur), turkuaz oklar, şeker renkli binalar.
        static readonly GroundStyle PlazaStyle = new GroundStyle
        {
            Name = "Office",
            Suffix = string.Empty,
            Asphalt = new Color(0.47f, 0.47f, 0.58f),
            Marking = new Color(0.96f, 0.97f, 1f),
            Arrow = new Color(0.22f, 0.92f, 1f),
            Stud = new Color(0.3f, 0.95f, 1f),
            SlabA = new Color(0.95f, 0.9f, 0.84f),
            SlabB = new Color(0.98f, 0.86f, 0.82f),
            Joint = new Color(0.72f, 0.68f, 0.7f),
            LaneGlow = new Color(0.1f, 0.78f, 0.82f),
            // Sıra, yan yana ve karşı karşıya gelenler birbirinden ayrışacak şekildedir (sol yakın, sol uzak, sağ yakın, sağ uzak).
            Towers = new[]
            {
                new Color(1f, 0.46f, 0.56f),   // mercan pembesi
                new Color(0.3f, 0.86f, 0.66f), // nane
                new Color(1f, 0.76f, 0.28f),   // bal sarısı
                new Color(0.56f, 0.52f, 1f),   // lavanta
            },
            ColumnTint = new Color(1f, 0.97f, 0.92f),
        };

        // Otonom Şehir: gün batımı. Orta tonlu mor-gri asfalt, pembe oklar, turuncu çiviler, neon renkli binalar.
        static readonly GroundStyle CityStyle = new GroundStyle
        {
            Name = "City",
            Suffix = "_City",
            Asphalt = new Color(0.42f, 0.38f, 0.52f),
            Marking = new Color(1f, 0.92f, 0.96f),
            Arrow = new Color(1f, 0.36f, 0.76f),
            Stud = new Color(1f, 0.64f, 0.3f),
            SlabA = new Color(0.9f, 0.8f, 0.86f),
            SlabB = new Color(0.84f, 0.78f, 0.92f),
            Joint = new Color(0.62f, 0.56f, 0.68f),
            LaneGlow = new Color(1f, 0.36f, 0.76f),
            Towers = new[]
            {
                new Color(0.22f, 0.82f, 0.86f), // turkuaz
                new Color(0.96f, 0.36f, 0.72f), // macenta
                new Color(1f, 0.7f, 0.26f),     // amber
                new Color(0.62f, 0.46f, 1f),    // menekşe
            },
            ColumnTint = new Color(1f, 0.92f, 0.95f),
        };

        static GroundStyle _style = PlazaStyle;

        static Color AsphaltColor => _style.Asphalt;
        static Color MarkingColor => _style.Marking;
        static Color ArrowColor => _style.Arrow;
        static Color StudColor => _style.Stud;
        static Color SlabColorA => _style.SlabA;
        static Color SlabColorB => _style.SlabB;
        static Color JointColor => _style.Joint;

        static Material RoadMaterial()
        {
            var texture = BuildTexture("Road_BaseColor" + _style.Suffix, RoadTextureWidth, RoadTextureHeight, DrawRoad, TextureWrapMode.Clamp, TextureWrapMode.Repeat, 8);
            // Işıma gücü düşük: gündüz çizgiler ve oklar hafifçe parlar, bloom yalnızca okları yayar.
            return TexturedMat("Road" + _style.Suffix, texture, 0.18f, 1.3f);
        }

        static Material SidewalkMaterial()
        {
            var texture = BuildTexture("Sidewalk_BaseColor" + _style.Suffix, SidewalkTextureSize, SidewalkTextureSize, DrawSidewalk, TextureWrapMode.Repeat, TextureWrapMode.Repeat, 4);
            return TexturedMat("Sidewalk" + _style.Suffix, texture, 0.15f, 0f);
        }

        static Material TexturedMat(string name, Texture2D texture, float smoothness, float glow)
        {
            var material = Mat(name, Color.white, smoothness);
            material.SetTexture("_BaseMap", texture);
            material.SetFloat("_GlowStrength", glow);
            EditorUtility.SetDirty(material);
            return material;
        }

        /// <summary>
        /// Yolun bir parçası: asfalt, iki yanda bordür (üstünde turkuaz ışık şeridi) ve kaldırım.
        /// Hepsi Z boyunca 2,5 m'de bir bölünür (kıvrımlı dünya yalnızca köşeleri büker).
        /// </summary>
        static void BuildRoad(Transform root, float length, Material curb, Material glowLine)
        {
            var road = RoadMaterial();
            var sidewalk = SidewalkMaterial();

            // Asfalt: U yolun genişliği boyunca 0-1, V her 10 m'de bir tekrar.
            FlatStrip(root, "Road", -RoadHalfWidth, RoadHalfWidth, 0f, length, 4,
                p => new Vector2((p.x + RoadHalfWidth) / (RoadHalfWidth * 2f), p.z / RoadTileLength), road);

            for (int side = -1; side <= 1; side += 2)
            {
                string suffix = side < 0 ? "Left" : "Right";
                float inner = side * RoadHalfWidth;
                float outer = side * (RoadHalfWidth + CurbWidth);

                SlicedPart(root, "Curb" + suffix, new Vector3((inner + outer) * 0.5f, CurbHeight * 0.5f, length * 0.5f), new Vector3(CurbWidth, CurbHeight, length), curb);
                // Bordürün yola bakan üst kenarı ışır: yolun sınırı uzaktan okunur.
                SlicedPart(root, "CurbGlow" + suffix, new Vector3(inner + side * 0.06f, CurbHeight + 0.01f, length * 0.5f), new Vector3(0.1f, 0.03f, length), glowLine);

                float from = Mathf.Min(outer, side * SidewalkOuter);
                float to = Mathf.Max(outer, side * SidewalkOuter);
                FlatStrip(root, "Sidewalk" + suffix, from, to, SidewalkTop, length, 6,
                    p => new Vector2(p.x / SidewalkTileSize, p.z / SidewalkTileSize), sidewalk);
            }
        }

        /// <summary>
        /// Yere yatık, yukarı bakan düz şerit. Z boyunca 2,5 m'de bir, X boyunca
        /// <paramref name="xSegments"/> parçaya bölünür. Mesh asset değildir; CombineByMaterial birleştirir.
        /// </summary>
        static void FlatStrip(Transform parent, string name, float xMin, float xMax, float y, float length, int xSegments, Func<Vector3, Vector2> uv, Material material, float sliceLength = 2.5f)
        {
            int slices = Mathf.Max(1, Mathf.CeilToInt(length / sliceLength));
            int columns = xSegments + 1;
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();

            for (int row = 0; row <= slices; row++)
            {
                float z = length * row / slices;
                for (int column = 0; column < columns; column++)
                {
                    var position = new Vector3(Mathf.Lerp(xMin, xMax, (float)column / xSegments), y, z);
                    vertices.Add(position);
                    uvs.Add(uv(position));
                }
            }

            for (int row = 0; row < slices; row++)
            {
                for (int column = 0; column < xSegments; column++)
                {
                    int a = row * columns + column;
                    int b = a + columns;
                    triangles.Add(a); triangles.Add(b); triangles.Add(b + 1);
                    triangles.Add(a); triangles.Add(b + 1); triangles.Add(a + 1);
                }
            }

            var mesh = new Mesh { name = name };
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            var normals = new Vector3[vertices.Count];
            for (int i = 0; i < normals.Length; i++) normals[i] = Vector3.up;
            mesh.normals = normals;
            mesh.RecalculateBounds();

            var part = new GameObject(name);
            part.transform.SetParent(parent, false);
            part.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = part.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
        }

        // ------------------------------------------------------------ Doku çizimi

        delegate Color PixelPainter(float u, float v);

        /// <summary>Dokuyu çizer, PNG yazar ve içe aktarma ayarlarını kurar. GUID korunur.</summary>
        static Texture2D BuildTexture(string name, int width, int height, Func<int, int, PixelPainter> painter, TextureWrapMode wrapU, TextureWrapMode wrapV, int aniso)
        {
            EnsureFolder(RoadDir);
            string path = $"{RoadDir}/{name}.png";

            var paint = painter(width, height);
            var pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    pixels[y * width + x] = paint((x + 0.5f) / width, (y + 0.5f) / height);

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.SetPixels32(pixels);
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;
            importer.mipmapEnabled = true;
            importer.isReadable = false;
            // Alfa saydamlık değil ışıma maskesidir.
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = false;
            importer.wrapModeU = wrapU;
            importer.wrapModeV = wrapV;
            importer.filterMode = FilterMode.Trilinear;
            // Yol kameraya çok yatık görünür; anizotropik süzme olmadan birkaç metre ötede bulanıklaşır.
            importer.anisoLevel = aniso;
            importer.maxTextureSize = Mathf.Max(width, height);
            importer.textureCompression = TextureImporterCompression.Compressed;
            foreach (string platform in new[] { "Android", "iPhone" })
            {
                var settings = importer.GetPlatformTextureSettings(platform);
                settings.overridden = true;
                settings.maxTextureSize = Mathf.Max(width, height);
                settings.format = TextureImporterFormat.ASTC_6x6;
                settings.compressionQuality = 50;
                importer.SetPlatformTextureSettings(settings);
            }

            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        /// <summary>Asfalt. u: yolun genişliği (0 sol kenar), v: 10 m'lik boy (yukarı = ileri, +Z).</summary>
        static PixelPainter DrawRoad(int width, int height)
        {
            float pixel = RoadHalfWidth * 2f / width; // bir pikselin metre karşılığı (yumuşatma için)

            return (u, v) =>
            {
                float x = u * RoadHalfWidth * 2f - RoadHalfWidth; // m, yolun ortası 0
                float z = v * RoadTileLength;                    // m, parçanın başı 0

                // Taban: ince taneli asfalt + geniş, belli belirsiz lekeler + tekerlek izleri.
                float grain = (Hash(u * width, v * height) - 0.5f) * 0.07f;
                float patches = (PeriodicNoise(u * 6f, v * 8f, 6, 8) - 0.5f) * 0.08f;
                float wear = 0f;
                for (int lane = -1; lane <= 1; lane++)
                {
                    float center = lane * 2.5f;
                    wear += Bump(Mathf.Abs(x - (center - 0.55f)), 0.22f) + Bump(Mathf.Abs(x - (center + 0.55f)), 0.22f);
                }

                var color = AsphaltColor * (1f + grain + patches - wear * 0.05f);
                float glow = 0f;

                // Derz: her parçanın başında ince koyu çizgi (hız hissi verir).
                float seam = Coverage(Mathf.Min(z, RoadTileLength - z) - 0.025f, pixel);
                color = Color.Lerp(color, color * 0.78f, seam);

                // Kesikli şerit çizgileri (x = ±1,25): 3 m çizgi, 2 m boşluk.
                for (int side = -1; side <= 1; side += 2)
                {
                    float dash = Mathf.Repeat(z, 5f);
                    float along = Mathf.Min(dash - 0.5f, 3.5f - dash); // çizginin içinde pozitif
                    float line = Coverage(0.06f - Mathf.Abs(x - side * 1.25f), pixel) * Coverage(along, pixel);
                    color = Color.Lerp(color, MarkingColor, line);
                    glow = Mathf.Max(glow, line * 0.35f);
                }

                // Kenar çizgileri: düz, bordürün hemen içinde.
                for (int side = -1; side <= 1; side += 2)
                {
                    float line = Coverage(0.07f - Mathf.Abs(x - side * 3.85f), pixel);
                    color = Color.Lerp(color, MarkingColor, line);
                    glow = Mathf.Max(glow, line * 0.25f);

                    // Kılavuz noktaları: kenar çizgisinin iç yanında 2,5 m'de bir ışıyan "kedi gözü".
                    float dz = Mathf.Repeat(z + 1.25f, 2.5f) - 1.25f;
                    float stud = Coverage(0.07f - new Vector2(x - side * 3.62f, dz).magnitude, pixel);
                    color = Color.Lerp(color, StudColor, stud);
                    glow = Mathf.Max(glow, stud);
                }

                // İleri okları: her şeridin ortasında, parça başına bir tane.
                for (int lane = -1; lane <= 1; lane++)
                {
                    float arrow = Coverage(-ArrowDistance(x - lane * 2.5f, z - 5.6f), pixel);
                    color = Color.Lerp(color, ArrowColor, arrow);
                    glow = Mathf.Max(glow, arrow);
                }

                color.a = 1f - glow;
                return color;
            };
        }

        /// <summary>Taş kaldırım: 1 m'lik kare levhalar, aralarında derz, levhadan levhaya hafif renk farkı.</summary>
        static PixelPainter DrawSidewalk(int width, int height)
        {
            float pixel = SidewalkTileSize / width;

            return (u, v) =>
            {
                float x = u * SidewalkTileSize;
                float z = v * SidewalkTileSize;
                int slabX = Mathf.FloorToInt(x);
                int slabZ = Mathf.FloorToInt(z);

                float tone = Hash(slabX * 17.3f + 3f, slabZ * 9.1f + 7f);
                var color = Color.Lerp(SlabColorA, SlabColorB, tone > 0.7f ? 1f : tone * 0.3f);
                color *= 1f + (Hash(u * width, v * height) - 0.5f) * 0.05f + (tone - 0.5f) * 0.04f;

                float fx = x - slabX;
                float fz = z - slabZ;
                float edge = Mathf.Min(Mathf.Min(fx, 1f - fx), Mathf.Min(fz, 1f - fz));
                color = Color.Lerp(color, JointColor, Coverage(0.018f - edge, pixel));
                // Levhanın kenarı hafif pahlı: derzin yanında ince açık çizgi.
                color = Color.Lerp(color, color * 1.06f, Coverage(0.04f - edge, pixel) * 0.6f);

                color.a = 1f;
                return color;
            };
        }

        /// <summary>
        /// İleri (+Z) bakan okun işaretli uzaklığı (m, içeride negatif): gövde dikdörtgeni + üçgen baş.
        /// </summary>
        static float ArrowDistance(float x, float z)
        {
            // Gövde: 0,28 m geniş, z = -0,9..0,25
            float shaft = Mathf.Max(Mathf.Abs(x) - 0.14f, Mathf.Abs(z + 0.325f) - 0.575f);
            // Baş: taban z = 0,15'te 0,9 m geniş, uç z = 1,0
            float headZ = z - 0.15f;
            float slope = 0.45f / 0.85f; // yarım genişlik / boy
            float side = (Mathf.Abs(x) - (0.45f - headZ * slope)) / Mathf.Sqrt(1f + slope * slope);
            float head = Mathf.Max(side, Mathf.Max(-headZ, headZ - 0.85f));
            return Mathf.Min(shaft, head);
        }

        /// <summary>İşaretli uzaklıktan kenar yumuşatmalı kaplama (0-1). d &gt; 0 içeride.</summary>
        static float Coverage(float d, float pixel)
        {
            return Mathf.Clamp01(d / pixel + 0.5f);
        }

        static float Bump(float distance, float radius)
        {
            float t = Mathf.Clamp01(1f - distance / radius);
            return t * t * (3f - 2f * t);
        }

        static float Hash(float x, float y)
        {
            float h = Mathf.Sin(Mathf.Floor(x) * 127.1f + Mathf.Floor(y) * 311.7f) * 43758.5453f;
            return h - Mathf.Floor(h);
        }

        /// <summary>Kenarlarda kesintisiz tekrar eden değer gürültüsü (periyot: px, py hücre).</summary>
        static float PeriodicNoise(float x, float y, int px, int py)
        {
            int x0 = Mathf.FloorToInt(x), y0 = Mathf.FloorToInt(y);
            float fx = x - x0, fy = y - y0;
            fx = fx * fx * (3f - 2f * fx);
            fy = fy * fy * (3f - 2f * fy);

            float Corner(int cx, int cy) => Hash((cx % px + px) % px, (cy % py + py) % py);
            float bottom = Mathf.Lerp(Corner(x0, y0), Corner(x0 + 1, y0), fx);
            float top = Mathf.Lerp(Corner(x0, y0 + 1), Corner(x0 + 1, y0 + 1), fx);
            return Mathf.Lerp(bottom, top, fy);
        }
    }
}
