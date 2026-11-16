using UnityEngine;

namespace OutOfOffice.EditorTools
{
    public static partial class PlaceholderContentBuilder
    {
        sealed class PlaceholderPrefabs
        {
            public GameObject Developer, Designer, Driver, Intern, Chaser;
            public GameObject DevBot, DesignBot, DriverBot;
            public GameObject ServerRack, CookieWall, RobotVacuum, CargoDrone, AutoTaxi;
            public GameObject AutoBus, GateBarrier, TrafficCone;
            public GameObject Coin, Captcha, Hallucination, Shield;
            public GameObject DronePickup, Magnet, Overtime, Espresso, OfficeChair, DriverPod, Package, CarrierDrone;
            public GameObject PromptInjection, Unplug, TutorialArrow;
            public GameObject Ground, CityGround, DepotGround, DataCenterGround, VillageGround;
            public GameObject ForkliftBot, CrateWall, CraneGantry, BackupBot, DiskStack, CableGate, OldTractor, HayBales, VillageGate;
        }

        // Ölçüler: oyuncu 1.8 m boyunda, kayarken 0.8 m. Zıplama tepesi 3.2 m (GameConfig.jumpHeight).
        // Bir şerit 2.5 m genişliğinde. Tüm prefab'ların orijini zeminde, şeridin ortasındadır.
        static PlaceholderPrefabs BuildPrefabs()
        {
            var white = Mat("White", new Color(0.93f, 0.95f, 0.98f));
            var glowWhite = Mat("GlowWhite", new Color(0.93f, 0.97f, 1f), 0.6f, 1.6f);
            var dark = Mat("Dark", new Color(0.13f, 0.15f, 0.2f));
            var red = Mat("RobotRed", new Color(0.92f, 0.2f, 0.25f), 0.6f, 0.9f);
            var steel = Mat("Steel", new Color(0.36f, 0.4f, 0.48f), 0.7f);
            var blue = Mat("DeveloperBlue", new Color(0.2f, 0.5f, 0.95f));
            var magenta = Mat("DesignerMagenta", new Color(0.9f, 0.3f, 0.7f));
            var yellow = Mat("Yellow", new Color(1f, 0.8f, 0.15f), 0.6f, 1.1f);
            var orange = Mat("CookieOrange", new Color(0.95f, 0.55f, 0.15f));
            var green = Mat("CaptchaGreen", new Color(0.25f, 0.85f, 0.45f), 0.6f, 1.5f);
            var purple = Mat("HallucinationPurple", new Color(0.62f, 0.35f, 0.95f), 0.6f, 1.5f);
            var pink = Mat("PromptPink", new Color(0.98f, 0.35f, 0.6f), 0.6f, 1.5f);
            var amber = Mat("DriverAmber", new Color(1f, 0.55f, 0.1f));
            var cyan = Mat("ShieldCyan", new Color(0.25f, 0.85f, 0.95f), 0.6f, 1.5f);
            var espresso = Mat("EspressoBrown", new Color(0.55f, 0.33f, 0.18f), 0.5f, 1.2f);
            var internGrey = Mat("InternGrey", new Color(0.62f, 0.66f, 0.72f));
            // Yolun bordürü krem beton, üst kenarındaki şerit turkuaz ışır. Asfalt ve kaldırım
            // dokuludur (PlaceholderContentBuilder.Road.cs).
            var curb = Mat("Curb", new Color(0.93f, 0.91f, 0.88f), 0.2f);
            var laneGlow = Mat("LaneGlow", new Color(0.1f, 0.78f, 0.82f), 0.5f, 0.7f);

            return new PlaceholderPrefabs
            {
                // Gerçek modeli kurulmuş karakter (CharacterModelBuilder) geçici küpün yerini alır.
                Developer = CharacterModelBuilder.PrefabOr("Character_Developer", BuildCharacter("Character_Developer", blue, white, dark, false)),
                Designer = CharacterModelBuilder.PrefabOr("Character_Designer", BuildCharacter("Character_Designer", magenta, white, yellow, true)),
                Driver = CharacterModelBuilder.PrefabOr("Character_Driver", BuildCharacter("Character_Driver", amber, white, dark, false)),
                Intern = CharacterModelBuilder.PrefabOr("Character_Intern", BuildIntern(internGrey, white, orange)),
                Chaser = CharacterModelBuilder.PrefabOr("Chaser_HR9000", BuildChaser(steel, red, glowWhite)),
                DevBot = BuildProfessionBot("Obstacle_DevBot", "DevBot", steel, blue, green),
                DesignBot = BuildProfessionBot("Obstacle_DesignBot", "DesignBot", steel, magenta, yellow),
                DriverBot = BuildProfessionBot("Obstacle_DriverBot", "DriverBot", steel, amber, cyan),
                ServerRack = BuildServerRack(dark, green),
                CookieWall = BuildCookieWall(orange, steel),
                RobotVacuum = BuildRobotVacuum(steel, red),
                CargoDrone = BuildCargoDrone(white, dark, orange),
                AutoTaxi = BuildAutoTaxi(yellow, dark),
                AutoBus = BuildAutoBus(green, dark),
                GateBarrier = BuildGateBarrier(red, dark),
                TrafficCone = BuildTrafficCone(orange, dark),
                ForkliftBot = BuildForkliftBot(steel, orange),
                CrateWall = BuildCrateWall(dark, orange),
                CraneGantry = BuildCraneGantry(orange, dark),
                BackupBot = BuildBackupBot(steel, green),
                DiskStack = BuildDiskStack(dark, green),
                CableGate = BuildCableGate(green, dark),
                OldTractor = BuildOldTractor(green, dark),
                HayBales = BuildHayBales(yellow, orange),
                VillageGate = BuildVillageGate(espresso, orange),
                Coin = BuildCoin(yellow),
                Captcha = BuildPowerUp("Pickup_Captcha", "Captcha", green, white),
                Hallucination = BuildPowerUp("Pickup_Hallucination", "Hallucination", purple, white),
                Shield = BuildPowerUp("Pickup_Shield", "Shield", cyan, white),
                DronePickup = BuildPowerUp("Pickup_Drone", "DronePickup", orange, white),
                Magnet = BuildPowerUp("Pickup_Magnet", "Magnet", red, white),
                Overtime = BuildPowerUp("Pickup_Overtime", "Overtime", yellow, white),
                Espresso = BuildPowerUp("Pickup_Espresso", "Espresso", espresso, white),
                PromptInjection = BuildPowerUp("Pickup_PromptInjection", "PromptInjection", pink, white),
                Unplug = BuildPowerUp("Pickup_Unplug", "Unplug", steel, yellow),
                TutorialArrow = BuildTutorialArrow(Mat("TutorialArrow", new Color(0.1f, 0.8f, 1f), 0.5f, 0.9f)),
                OfficeChair = BuildOfficeChair(dark, steel),
                DriverPod = BuildDriverPod(dark, amber),
                CarrierDrone = BuildCarrierDrone(white, dark),
                Package = BuildPackage(espresso, yellow),
                Ground = BuildGround(PlazaStyle, curb, laneGlow, dark),
                CityGround = BuildGround(CityStyle, curb, Mat("LaneGlowCity", CityStyle.LaneGlow, 0.5f, 0.7f), dark),
                VillageGround = BuildGround(VillageStyle, curb, Mat("LaneGlowVillage", VillageStyle.LaneGlow, 0.5f, 0.7f), dark),
                DataCenterGround = BuildGround(DataCenterStyle, curb, Mat("LaneGlowDataCenter", DataCenterStyle.LaneGlow, 0.5f, 0.7f), dark),
                DepotGround = BuildGround(DepotStyle, curb, Mat("LaneGlowDepot", DepotStyle.LaneGlow, 0.5f, 0.7f), dark),
            };
        }

        /// <summary>
        /// Gerçek modeli kuruluysa (PropModelBuilder) köke görsel olarak ekler ve true döner.
        /// Dönen değer false ise çağıran geçici parçaları kurar. Çarpışma şekli her iki
        /// durumda da aynıdır; oynanış modele göre değişmez. Gerçek zamanlı gölge kapalıdır;
        /// gölgeyi damla gölge verir (PlaceholderContentBuilder.Shadows.cs).
        /// </summary>
        static bool Model(GameObject root, string prop)
        {
            return PropModelBuilder.TryAddModel(root.transform, prop, castShadows: false);
        }

        // Karakter ve kovalayan prefab'ları yalnızca görseldir (+Z ileri bakar).
        static GameObject BuildCharacter(string name, Material body, Material visor, Material accent, bool beret)
        {
            var root = new GameObject(name);
            Part(root.transform, PrimitiveType.Capsule, "Body", new Vector3(0f, 0.9f, 0f), new Vector3(0.7f, 0.9f, 0.7f), body);
            Part(root.transform, PrimitiveType.Cube, "Visor", new Vector3(0f, 1.5f, 0.3f), new Vector3(0.42f, 0.14f, 0.16f), visor);

            if (beret)
                Part(root.transform, PrimitiveType.Sphere, "Beret", new Vector3(0.05f, 1.84f, 0f), new Vector3(0.62f, 0.16f, 0.62f), accent);
            else
                Part(root.transform, PrimitiveType.Cube, "Backpack", new Vector3(0f, 1.05f, -0.36f), new Vector3(0.46f, 0.56f, 0.22f), accent);

            return SavePrefab(root);
        }

        // Yeteneksiz, her zaman oynanabilen yedek karakter: kısa boylu, elinde kahve.
        static GameObject BuildIntern(Material body, Material visor, Material cup)
        {
            var root = new GameObject("Character_Intern");
            Part(root.transform, PrimitiveType.Capsule, "Body", new Vector3(0f, 0.8f, 0f), new Vector3(0.62f, 0.8f, 0.62f), body);
            Part(root.transform, PrimitiveType.Cube, "Visor", new Vector3(0f, 1.32f, 0.27f), new Vector3(0.38f, 0.12f, 0.14f), visor);
            Part(root.transform, PrimitiveType.Cylinder, "Coffee", new Vector3(0.38f, 0.95f, 0.2f), new Vector3(0.16f, 0.12f, 0.16f), cup);
            return SavePrefab(root);
        }

        // Otomatikleşen mesleğin robotu: mesleğin renginde gövde, parlayan ekran.
        static GameObject BuildProfessionBot(string name, string prop, Material frame, Material body, Material screen)
        {
            var root = new GameObject(name);
            bool real = Model(root, prop);
            if (!real)
            {
                Part(root.transform, PrimitiveType.Cube, "Base", new Vector3(0f, 0.25f, 0f), new Vector3(1.5f, 0.5f, 1.1f), frame);
                Part(root.transform, PrimitiveType.Cube, "Body", new Vector3(0f, 1.2f, 0f), new Vector3(1.2f, 1.4f, 0.8f), body);
                Part(root.transform, PrimitiveType.Cube, "Screen", new Vector3(0f, 1.35f, -0.41f), new Vector3(0.9f, 0.7f, 0.03f), screen);
                Part(root.transform, PrimitiveType.Cube, "Head", new Vector3(0f, 2.15f, 0f), new Vector3(0.7f, 0.4f, 0.6f), frame);
            }

            BlobShadow(root, new Vector2(2f, 1.5f));
            BoxTrigger(root, new Vector3(0f, 1.2f, 0f), new Vector3(1.5f, 2.4f, 1.1f));
            if (!real) CombineByMaterial(root, castShadows: false);
            return SavePrefab(root);
        }

        // Kaykayın karşılığı: tekerlekli ofis sandalyesi. Yalnızca görsel; oyuncuya takılır.
        static GameObject BuildOfficeChair(Material seat, Material metal)
        {
            var root = new GameObject("Accessory_OfficeChair");
            if (!Model(root, "OfficeChair"))
            {
                Part(root.transform, PrimitiveType.Cylinder, "Base", new Vector3(0f, 0.06f, 0f), new Vector3(1.1f, 0.05f, 1.1f), metal);
                Part(root.transform, PrimitiveType.Cylinder, "Stem", new Vector3(0f, 0.25f, 0f), new Vector3(0.12f, 0.2f, 0.12f), metal);
                Part(root.transform, PrimitiveType.Cube, "Seat", new Vector3(0f, 0.48f, 0f), new Vector3(0.95f, 0.14f, 0.9f), seat);
                Part(root.transform, PrimitiveType.Cube, "Back", new Vector3(0f, 1.05f, -0.48f), new Vector3(0.9f, 1.1f, 0.12f), seat);
            }

            return SavePrefab(root);
        }

        // Öğreticinin 3B oku: +X yönüne bakan üç ışıyan şevron (>). Yön, çalışma anında döndürülerek verilir.
        static GameObject BuildTutorialArrow(Material glow)
        {
            var root = new GameObject("Tutorial_Arrow");
            for (int i = 0; i < 3; i++)
            {
                float x = -i * 0.75f;
                Part(root.transform, PrimitiveType.Cube, "ChevronUp" + i, new Vector3(x - 0.32f, 0.32f, 0f), new Vector3(0.95f, 0.24f, 0.24f), glow, new Vector3(0f, 0f, -45f));
                Part(root.transform, PrimitiveType.Cube, "ChevronDown" + i, new Vector3(x - 0.32f, -0.32f, 0f), new Vector3(0.95f, 0.24f, 0.24f), glow, new Vector3(0f, 0f, 45f));
            }

            CombineByMaterial(root, castShadows: false);
            return SavePrefab(root);
        }

        // Şoför'ün yeteneğinde bindiği açık tepeli otonom kapsül. Yalnızca görsel; oyuncuya takılır.
        static GameObject BuildDriverPod(Material body, Material trim)
        {
            var root = new GameObject("Accessory_DriverPod");
            if (!Model(root, "DriverPod"))
            {
                Part(root.transform, PrimitiveType.Cube, "Body", new Vector3(0f, 0.4f, 0f), new Vector3(1.5f, 0.5f, 2.2f), body);
                Part(root.transform, PrimitiveType.Cube, "Seat", new Vector3(0f, 0.8f, -0.4f), new Vector3(0.8f, 0.5f, 0.8f), trim);
            }

            return SavePrefab(root);
        }

        // Jetpack'in karşılığı: oyuncuyu taşıyan dron. Yalnızca görsel; oyuncu tutamağına asılır.
        // Tutamak, asılma pozundaki ellerin yüksekliğindedir (model hattında "lift").
        static GameObject BuildCarrierDrone(Material body, Material rotor)
        {
            var root = new GameObject("Attachment_CarrierDrone");
            if (!Model(root, "CarrierDrone"))
            {
                Part(root.transform, PrimitiveType.Cube, "Body", new Vector3(0f, 2.5f, 0f), new Vector3(1f, 0.3f, 1f), body);
                Part(root.transform, PrimitiveType.Cube, "Handle", new Vector3(0f, 2.15f, 0f), new Vector3(0.7f, 0.06f, 0.06f), rotor);
                for (int i = 0; i < 4; i++)
                {
                    float x = i % 2 == 0 ? -0.65f : 0.65f;
                    float z = i < 2 ? -0.65f : 0.65f;
                    Part(root.transform, PrimitiveType.Cylinder, "Rotor" + i, new Vector3(x, 2.7f, z), new Vector3(0.6f, 0.03f, 0.6f), rotor);
                }
            }

            return SavePrefab(root);
        }

        // Sürpriz paket: kurdeleli kutu. Oyun sonunda açılır.
        static GameObject BuildPackage(Material box, Material ribbon)
        {
            var root = new GameObject("Pickup_Package");
            bool real = Model(root, "Package");
            if (!real)
            {
                Part(root.transform, PrimitiveType.Cube, "Box", new Vector3(0f, 1.05f, 0f), new Vector3(0.9f, 0.9f, 0.9f), box);
                Part(root.transform, PrimitiveType.Cube, "RibbonA", new Vector3(0f, 1.05f, 0f), new Vector3(0.94f, 0.94f, 0.2f), ribbon);
                Part(root.transform, PrimitiveType.Cube, "RibbonB", new Vector3(0f, 1.05f, 0f), new Vector3(0.2f, 0.94f, 0.94f), ribbon);
            }

            BlobShadow(root, new Vector2(0.95f, 0.95f));
            SphereTrigger(root, new Vector3(0f, 1.05f, 0f), 0.85f);
            if (!real) CombineByMaterial(root, castShadows: false);
            return SavePrefab(root);
        }

        static GameObject BuildChaser(Material body, Material head, Material eye)
        {
            var root = new GameObject("Chaser_HR9000");
            Part(root.transform, PrimitiveType.Cube, "Body", new Vector3(0f, 1.05f, 0f), new Vector3(1f, 1.5f, 0.7f), body);
            Part(root.transform, PrimitiveType.Cube, "Head", new Vector3(0f, 2.15f, 0f), new Vector3(0.8f, 0.55f, 0.65f), head);
            Part(root.transform, PrimitiveType.Cube, "Eye", new Vector3(0f, 2.18f, 0.33f), new Vector3(0.56f, 0.1f, 0.03f), eye);
            Part(root.transform, PrimitiveType.Cube, "ArmLeft", new Vector3(-0.65f, 1.3f, 0.25f), new Vector3(0.2f, 0.2f, 0.9f), body);
            Part(root.transform, PrimitiveType.Cube, "ArmRight", new Vector3(0.65f, 1.3f, 0.25f), new Vector3(0.2f, 0.2f, 0.9f), body);
            // Elindeki çıkış belgesi.
            Part(root.transform, PrimitiveType.Cube, "ExitPaper", new Vector3(0.65f, 1.45f, 0.75f), new Vector3(0.34f, 0.44f, 0.02f), eye);
            return SavePrefab(root);
        }

        // Şerit değiştirerek kaçılır: tam boy sunucu kabini.
        static GameObject BuildServerRack(Material body, Material led)
        {
            var root = new GameObject("Obstacle_ServerRack");
            bool real = Model(root, "ServerRack");
            if (!real)
            {
                Part(root.transform, PrimitiveType.Cube, "Rack", new Vector3(0f, 1.3f, 0f), new Vector3(1.8f, 2.6f, 1.2f), body);
                for (int i = 0; i < 4; i++)
                    Part(root.transform, PrimitiveType.Cube, "Led" + i, new Vector3(0f, 0.6f + i * 0.5f, -0.61f), new Vector3(1.4f, 0.08f, 0.02f), led);
            }

            BlobShadow(root, new Vector2(2.1f, 1.5f));
            BoxTrigger(root, new Vector3(0f, 1.3f, 0f), new Vector3(1.7f, 2.6f, 1.1f));
            if (!real) CombineByMaterial(root, castShadows: false);
            return SavePrefab(root);
        }

        // Altından kayılır: "Çerezleri kabul et" duvarı. Alt kenarı 1.1 m'de.
        static GameObject BuildCookieWall(Material banner, Material post)
        {
            var root = new GameObject("Obstacle_CookieWall");
            bool real = Model(root, "CookieWall");
            if (!real)
            {
                Part(root.transform, PrimitiveType.Cube, "Banner", new Vector3(0f, 1.9f, 0f), new Vector3(2.3f, 1.6f, 0.3f), banner);
                Part(root.transform, PrimitiveType.Cube, "PostLeft", new Vector3(-1.1f, 0.55f, 0f), new Vector3(0.12f, 1.1f, 0.12f), post);
                Part(root.transform, PrimitiveType.Cube, "PostRight", new Vector3(1.1f, 0.55f, 0f), new Vector3(0.12f, 1.1f, 0.12f), post);
            }

            BlobShadow(root, new Vector2(2.7f, 0.9f));
            BoxTrigger(root, new Vector3(0f, 1.9f, 0f), new Vector3(2.3f, 1.6f, 0.8f));
            if (!real) CombineByMaterial(root, castShadows: false);
            return SavePrefab(root);
        }

        // Üstünden atlanır: robot süpürge.
        static GameObject BuildRobotVacuum(Material body, Material light)
        {
            var root = new GameObject("Obstacle_RobotVacuum");
            bool real = Model(root, "RobotVacuum");
            if (!real)
            {
                Part(root.transform, PrimitiveType.Cylinder, "Body", new Vector3(0f, 0.22f, 0f), new Vector3(1.7f, 0.22f, 1.7f), body);
                Part(root.transform, PrimitiveType.Cylinder, "Sensor", new Vector3(0f, 0.5f, 0f), new Vector3(0.5f, 0.07f, 0.5f), light);
            }

            BlobShadow(root, new Vector2(2.1f, 2.1f));
            BoxTrigger(root, new Vector3(0f, 0.35f, 0f), new Vector3(1.6f, 0.7f, 1.4f));
            if (!real) CombineByMaterial(root, castShadows: false);
            return SavePrefab(root);
        }

        // Havada süzülür, altından kayılır. Alt kenarı 1.2 m'de.
        static GameObject BuildCargoDrone(Material body, Material rotor, Material cargo)
        {
            var root = new GameObject("Obstacle_CargoDrone");
            // Dört pervane ayrı çocuk nesne olarak ayrılır; ObstacleDefinition.spinParts onları döndürür.
            bool real = PropModelBuilder.TryAddSplitModel(root.transform, "CargoDrone", new[]
            {
                new PropModelBuilder.SplitPart("Rotor0", new Vector3(-0.62f, 2.07f, -0.42f), Vector3.up, 0.27f, 0.15f),
                new PropModelBuilder.SplitPart("Rotor1", new Vector3(0.62f, 2.07f, -0.42f), Vector3.up, 0.27f, 0.15f),
                new PropModelBuilder.SplitPart("Rotor2", new Vector3(-0.62f, 2.07f, 0.42f), Vector3.up, 0.27f, 0.15f),
                new PropModelBuilder.SplitPart("Rotor3", new Vector3(0.62f, 2.07f, 0.42f), Vector3.up, 0.27f, 0.15f),
            });
            if (!real)
            {
                Part(root.transform, PrimitiveType.Cube, "Body", new Vector3(0f, 1.95f, 0f), new Vector3(1f, 0.3f, 1f), body);
                Part(root.transform, PrimitiveType.Cube, "Cargo", new Vector3(0f, 1.5f, 0f), new Vector3(0.7f, 0.6f, 0.7f), cargo);
                for (int i = 0; i < 4; i++)
                {
                    float x = i % 2 == 0 ? -0.65f : 0.65f;
                    float z = i < 2 ? -0.65f : 0.65f;
                    Part(root.transform, PrimitiveType.Cylinder, "Rotor" + i, new Vector3(x, 2.15f, z), new Vector3(0.6f, 0.03f, 0.6f), rotor);
                }
            }

            BlobShadow(root, new Vector2(1.7f, 1.7f));
            BoxTrigger(root, new Vector3(0f, 1.75f, 0f), new Vector3(1.5f, 1.1f, 1.3f));
            if (!real) CombineByMaterial(root, castShadows: false);
            return SavePrefab(root);
        }

        // Otonom Şehir: yavaş yavaş gelen otonom otobüs; şerit değiştirerek kaçılır.
        static GameObject BuildAutoBus(Material body, Material trim)
        {
            var root = new GameObject("Obstacle_AutoBus");
            bool real = Model(root, "AutoBus");
            if (!real)
            {
                Part(root.transform, PrimitiveType.Cube, "Body", new Vector3(0f, 1.25f, 0f), new Vector3(2f, 2f, 3.2f), body);
                Part(root.transform, PrimitiveType.Cube, "Skirt", new Vector3(0f, 0.35f, 0f), new Vector3(2.1f, 0.5f, 3.3f), trim);
            }

            BlobShadow(root, new Vector2(2.5f, 3.6f));
            BoxTrigger(root, new Vector3(0f, 1.25f, 0f), new Vector3(2f, 2.5f, 2.9f));
            if (!real) CombineByMaterial(root, castShadows: false);
            return SavePrefab(root);
        }

        // Otonom Şehir: kontrol noktası bariyeri. Kolu yüksektir, altından kayılır.
        static GameObject BuildGateBarrier(Material arm, Material post)
        {
            var root = new GameObject("Obstacle_GateBarrier");
            bool real = Model(root, "GateBarrier");
            if (!real)
            {
                Part(root.transform, PrimitiveType.Cube, "Arm", new Vector3(0f, 1.4f, 0f), new Vector3(2.8f, 0.4f, 0.3f), arm);
                Part(root.transform, PrimitiveType.Cube, "PostLeft", new Vector3(-1.4f, 0.8f, 0f), new Vector3(0.35f, 1.6f, 0.35f), post);
                Part(root.transform, PrimitiveType.Cube, "PostRight", new Vector3(1.4f, 0.8f, 0f), new Vector3(0.35f, 1.6f, 0.35f), post);
            }

            BlobShadow(root, new Vector2(3.1f, 0.9f));
            // Yalnızca kol çarpar (şerit genişliği kadar); kayan oyuncu altından geçer.
            BoxTrigger(root, new Vector3(0f, 1.45f, 0f), new Vector3(2.4f, 0.9f, 0.7f));
            if (!real) CombineByMaterial(root, castShadows: false);
            return SavePrefab(root);
        }

        // Lojistik Deposu: forklift robotu. Şerit değiştirerek kaçılır; üstünden de atlanır.
        static GameObject BuildForkliftBot(Material body, Material fork)
        {
            var root = new GameObject("Obstacle_ForkliftBot");
            bool real = Model(root, "ForkliftBot");
            if (!real)
            {
                Part(root.transform, PrimitiveType.Cube, "Body", new Vector3(0f, 0.8f, 0.2f), new Vector3(1.8f, 1.2f, 1.4f), body);
                Part(root.transform, PrimitiveType.Cube, "Fork", new Vector3(0f, 0.25f, -1f), new Vector3(1.4f, 0.2f, 0.9f), fork);
            }

            BlobShadow(root, new Vector2(2.6f, 2.2f));
            BoxTrigger(root, new Vector3(0f, 0.85f, 0f), new Vector3(2.2f, 1.7f, 1.7f));
            if (!real) CombineByMaterial(root, castShadows: false);
            return SavePrefab(root);
        }

        // Lojistik Deposu: alçak koli duvarı; üstünden atlanır.
        static GameObject BuildCrateWall(Material crate, Material strap)
        {
            var root = new GameObject("Obstacle_CrateWall");
            bool real = Model(root, "CrateWall");
            if (!real)
            {
                Part(root.transform, PrimitiveType.Cube, "Crates", new Vector3(0f, 0.65f, 0f), new Vector3(2.3f, 1.3f, 0.6f), crate);
                Part(root.transform, PrimitiveType.Cube, "Strap", new Vector3(0f, 0.65f, 0f), new Vector3(2.35f, 0.2f, 0.65f), strap);
            }

            BlobShadow(root, new Vector2(2.8f, 1.2f));
            BoxTrigger(root, new Vector3(0f, 0.65f, 0f), new Vector3(2.3f, 1.3f, 0.8f));
            if (!real) CombineByMaterial(root, castShadows: false);
            return SavePrefab(root);
        }

        // Lojistik Deposu: vinç kapısı. Üst kiriş çarpar, altından kayılır (bariyerle aynı oyun mantığı).
        static GameObject BuildCraneGantry(Material beam, Material leg)
        {
            var root = new GameObject("Obstacle_CraneGantry");
            bool real = Model(root, "CraneGantry");
            if (!real)
            {
                Part(root.transform, PrimitiveType.Cube, "Beam", new Vector3(0f, 1.8f, 0f), new Vector3(2.8f, 0.8f, 1.6f), beam);
                Part(root.transform, PrimitiveType.Cube, "LegLeft", new Vector3(-1.2f, 0.7f, 0f), new Vector3(0.4f, 1.4f, 1.2f), leg);
                Part(root.transform, PrimitiveType.Cube, "LegRight", new Vector3(1.2f, 0.7f, 0f), new Vector3(0.4f, 1.4f, 1.2f), leg);
            }

            BlobShadow(root, new Vector2(3.2f, 2.4f));
            // Yalnızca kiriş çarpar (şerit genişliği kadar); kayan oyuncu altından geçer.
            BoxTrigger(root, new Vector3(0f, 1.7f, 0f), new Vector3(2.4f, 1.0f, 1.2f));
            if (!real) CombineByMaterial(root, castShadows: false);
            return SavePrefab(root);
        }

        // Veri Merkezi: yedekleme robotu. Üstünde sunucu rafı taşır; şerit değiştirerek kaçılır.
        static GameObject BuildBackupBot(Material body, Material screen)
        {
            var root = new GameObject("Obstacle_BackupBot");
            bool real = Model(root, "BackupBot");
            if (!real)
            {
                Part(root.transform, PrimitiveType.Cube, "Body", new Vector3(0f, 1.1f, 0f), new Vector3(1.6f, 2f, 1.1f), body);
                Part(root.transform, PrimitiveType.Cube, "Screen", new Vector3(0f, 1.6f, -0.56f), new Vector3(1f, 0.6f, 0.03f), screen);
            }

            BlobShadow(root, new Vector2(2.2f, 1.7f));
            BoxTrigger(root, new Vector3(0f, 1.2f, 0f), new Vector3(2f, 2.4f, 1.4f));
            if (!real) CombineByMaterial(root, castShadows: false);
            return SavePrefab(root);
        }

        // Veri Merkezi: disk kasası yığını; üstünden atlanır.
        static GameObject BuildDiskStack(Material blades, Material handle)
        {
            var root = new GameObject("Obstacle_DiskStack");
            bool real = Model(root, "DiskStack");
            if (!real)
            {
                Part(root.transform, PrimitiveType.Cube, "Blades", new Vector3(0f, 0.65f, 0f), new Vector3(2.2f, 1.3f, 2f), blades);
                Part(root.transform, PrimitiveType.Cube, "Handles", new Vector3(0f, 0.9f, -1f), new Vector3(2f, 0.15f, 0.1f), handle);
            }

            BlobShadow(root, new Vector2(2.8f, 2.8f));
            BoxTrigger(root, new Vector3(0f, 0.65f, 0f), new Vector3(2.2f, 1.3f, 2f));
            if (!real) CombineByMaterial(root, castShadows: false);
            return SavePrefab(root);
        }

        // Veri Merkezi: kablo kapısı. Üst kablo yatağı çarpar, altından kayılır.
        static GameObject BuildCableGate(Material cables, Material leg)
        {
            var root = new GameObject("Obstacle_CableGate");
            bool real = Model(root, "CableGate");
            if (!real)
            {
                Part(root.transform, PrimitiveType.Cube, "Tray", new Vector3(0f, 1.8f, 0f), new Vector3(2.8f, 0.7f, 0.8f), cables);
                Part(root.transform, PrimitiveType.Cube, "LegLeft", new Vector3(-1.3f, 0.7f, 0f), new Vector3(0.4f, 1.4f, 0.6f), leg);
                Part(root.transform, PrimitiveType.Cube, "LegRight", new Vector3(1.3f, 0.7f, 0f), new Vector3(0.4f, 1.4f, 0.6f), leg);
            }

            BlobShadow(root, new Vector2(3.4f, 1.2f));
            // Yalnızca kablo yatağı çarpar; kayan oyuncu altından geçer.
            BoxTrigger(root, new Vector3(0f, 1.65f, 0f), new Vector3(2.4f, 0.7f, 0.9f));
            if (!real) CombineByMaterial(root, castShadows: false);
            return SavePrefab(root);
        }

        // Analog Kasaba: eski traktör. Yavaşça yaklaşır; şerit değiştirerek kaçılır.
        static GameObject BuildOldTractor(Material body, Material wheel)
        {
            var root = new GameObject("Obstacle_OldTractor");
            bool real = Model(root, "OldTractor");
            if (!real)
            {
                Part(root.transform, PrimitiveType.Cube, "Body", new Vector3(0f, 1f, 0f), new Vector3(1.6f, 1.4f, 2f), body);
                Part(root.transform, PrimitiveType.Cube, "Wheels", new Vector3(0f, 0.5f, 0.3f), new Vector3(2.1f, 1f, 0.9f), wheel);
            }

            BlobShadow(root, new Vector2(2.6f, 2.2f));
            BoxTrigger(root, new Vector3(0f, 1.1f, 0f), new Vector3(2.1f, 2.2f, 1.5f));
            if (!real) CombineByMaterial(root, castShadows: false);
            return SavePrefab(root);
        }

        // Analog Kasaba: saman balyaları; üstünden atlanır.
        static GameObject BuildHayBales(Material straw, Material rope)
        {
            var root = new GameObject("Obstacle_HayBales");
            bool real = Model(root, "HayBales");
            if (!real)
            {
                Part(root.transform, PrimitiveType.Cube, "Bales", new Vector3(0f, 0.65f, 0f), new Vector3(1.7f, 1.3f, 1f), straw);
                Part(root.transform, PrimitiveType.Cube, "Rope", new Vector3(0f, 0.65f, 0f), new Vector3(1.75f, 0.15f, 1.05f), rope);
            }

            BlobShadow(root, new Vector2(2.2f, 1.6f));
            BoxTrigger(root, new Vector3(0f, 0.65f, 0f), new Vector3(1.9f, 1.3f, 1f));
            if (!real) CombineByMaterial(root, castShadows: false);
            return SavePrefab(root);
        }

        // Analog Kasaba: kasaba kapısı. Üst kiriş çarpar, altından kayılır.
        static GameObject BuildVillageGate(Material wood, Material trim)
        {
            var root = new GameObject("Obstacle_VillageGate");
            bool real = Model(root, "VillageGate");
            if (!real)
            {
                Part(root.transform, PrimitiveType.Cube, "Beam", new Vector3(0f, 1.9f, 0f), new Vector3(2.9f, 0.6f, 0.9f), wood);
                Part(root.transform, PrimitiveType.Cube, "PostLeft", new Vector3(-1.2f, 0.9f, 0f), new Vector3(0.4f, 1.8f, 0.6f), trim);
                Part(root.transform, PrimitiveType.Cube, "PostRight", new Vector3(1.2f, 0.9f, 0f), new Vector3(0.4f, 1.8f, 0.6f), trim);
            }

            BlobShadow(root, new Vector2(3.3f, 1.4f));
            // Yalnızca kiriş çarpar; kayan oyuncu altından geçer.
            BoxTrigger(root, new Vector3(0f, 1.8f, 0f), new Vector3(2.4f, 0.8f, 1f));
            if (!real) CombineByMaterial(root, castShadows: false);
            return SavePrefab(root);
        }

        // Otonom Şehir: tekerlekli trafik konisi robotu; üstünden atlanır.
        static GameObject BuildTrafficCone(Material cone, Material baseMat)
        {
            var root = new GameObject("Obstacle_TrafficCone");
            bool real = Model(root, "TrafficCone");
            if (!real)
            {
                Part(root.transform, PrimitiveType.Cylinder, "Base", new Vector3(0f, 0.2f, 0f), new Vector3(0.9f, 0.1f, 0.9f), baseMat);
                Part(root.transform, PrimitiveType.Cylinder, "Cone", new Vector3(0f, 0.6f, 0f), new Vector3(0.5f, 0.35f, 0.5f), cone);
            }

            BlobShadow(root, new Vector2(1.2f, 1.2f));
            BoxTrigger(root, new Vector3(0f, 0.5f, 0f), new Vector3(0.85f, 1f, 0.85f));
            if (!real) CombineByMaterial(root, castShadows: false);
            return SavePrefab(root);
        }

        // Oyuncuya doğru gelir, şerit değiştirerek kaçılır.
        static GameObject BuildAutoTaxi(Material body, Material glass)
        {
            var root = new GameObject("Obstacle_AutoTaxi");
            // Dört tekerlek ayrı çocuk nesne olarak ayrılır (yatay eksen etrafında döner).
            bool real = PropModelBuilder.TryAddSplitModel(root.transform, "AutoTaxi", new[]
            {
                new PropModelBuilder.SplitPart("Wheel0", new Vector3(-0.77f, 0.43f, -0.84f), Vector3.right, 0.40f, 0.22f),
                new PropModelBuilder.SplitPart("Wheel1", new Vector3(0.77f, 0.43f, -0.84f), Vector3.right, 0.40f, 0.22f),
                new PropModelBuilder.SplitPart("Wheel2", new Vector3(-0.77f, 0.43f, 0.84f), Vector3.right, 0.40f, 0.22f),
                new PropModelBuilder.SplitPart("Wheel3", new Vector3(0.77f, 0.43f, 0.84f), Vector3.right, 0.40f, 0.22f),
            });
            if (!real)
            {
                Part(root.transform, PrimitiveType.Cube, "Body", new Vector3(0f, 0.6f, 0f), new Vector3(1.8f, 0.9f, 3.6f), body);
                Part(root.transform, PrimitiveType.Cube, "Cabin", new Vector3(0f, 1.35f, 0.2f), new Vector3(1.6f, 0.7f, 1.8f), glass);
                Part(root.transform, PrimitiveType.Cube, "Sensor", new Vector3(0f, 1.85f, 0.2f), new Vector3(0.4f, 0.3f, 0.4f), body);
            }

            // Gerçek model kısa ve yüksek bir robotaksidir (2,5 m); kutu görselden uzun olmasın.
            BlobShadow(root, new Vector2(2.2f, real ? 3.2f : 4f));
            BoxTrigger(root, new Vector3(0f, 1f, 0f), new Vector3(1.7f, 2f, real ? 2.4f : 3.5f));
            if (!real) CombineByMaterial(root, castShadows: false);
            return SavePrefab(root);
        }

        static GameObject BuildCoin(Material material)
        {
            var root = new GameObject("Pickup_Coin");
            bool real = Model(root, "Coin");
            if (!real)
                Part(root.transform, PrimitiveType.Cylinder, "Disc", new Vector3(0f, 1f, 0f), new Vector3(0.7f, 0.05f, 0.7f), material, new Vector3(90f, 0f, 0f));

            SphereTrigger(root, new Vector3(0f, 1f, 0f), 0.7f);
            if (!real) CombineByMaterial(root, castShadows: false);
            return SavePrefab(root);
        }

        static GameObject BuildPowerUp(string name, string prop, Material core, Material ring)
        {
            var root = new GameObject(name);
            bool real = Model(root, prop);
            if (!real)
            {
                Part(root.transform, PrimitiveType.Sphere, "Core", new Vector3(0f, 1.1f, 0f), new Vector3(0.8f, 0.8f, 0.8f), core);
                Part(root.transform, PrimitiveType.Cylinder, "Ring", new Vector3(0f, 1.1f, 0f), new Vector3(1.15f, 0.03f, 1.15f), ring, new Vector3(90f, 0f, 0f));
            }

            BlobShadow(root, new Vector2(0.9f, 0.9f));
            SphereTrigger(root, new Vector3(0f, 1.1f, 0f), 0.8f);
            if (!real) CombineByMaterial(root, castShadows: false);
            return SavePrefab(root);
        }

        // 30 m'lik yol parçası. Orijin parçanın başında, +Z yönünde uzanır.
        // Asfalt, bordür ve kaldırım koddan kurulur (dilimli ve ek yeri kusursuz olmalı;
        // PlaceholderContentBuilder.Road.cs); dekor gerçek modellerdir. Hepsi materyal başına tek
        // mesh'te birleşir.
        static GameObject BuildGround(GroundStyle style, Material curb, Material line, Material desk)
        {
            const float length = 30f;
            _style = style;

            var root = new GameObject("Ground_" + style.Name);
            BuildRoad(root.transform, length, curb, line);

            bool realDecor = PropModelBuilder.Exists("Column");
            for (int side = -1; side <= 1; side += 2)
            {
                string suffix = side < 0 ? "Left" : "Right";
                if (realDecor)
                {
                    BuildDecor(root.transform, side);
                    continue;
                }

                for (int i = 0; i < 3; i++)
                {
                    float z = 5f + i * 10f;
                    Part(root.transform, PrimitiveType.Cube, $"Pillar{suffix}{i}", new Vector3(side * 5.4f, SidewalkTop + 2.5f, z), new Vector3(0.8f, 5f, 0.8f), curb);
                    Part(root.transform, PrimitiveType.Cube, $"Desk{suffix}{i}", new Vector3(side * 6.8f, SidewalkTop + 0.4f, z + 5f), new Vector3(1.6f, 0.8f, 0.9f), desk);
                }
            }

            CombineByMaterial(root, castShadows: false);
            var prefab = SavePrefab(root);
            _style = PlazaStyle;
            return prefab;
        }

        // Yolun bir yanındaki dekor. Modeller gelen oyuncuya (-Z) bakar; yan dekor yola doğru çevrilir.
        // İki yan aynı dizilmez: sol ve sağ yarım parça kaydırılır, kuleler yer değiştirir.
        static void BuildDecor(Transform root, int side)
        {
            var style = _style;
            float shift = side < 0 ? 0f : 5f;

            // Yol kenarı: her 10 m'de bir nesne (kolon, akıllı lamba, projektör).
            for (int i = 0; i < 3; i++)
            {
                var position = new Vector3(side * style.RoadsideX, SidewalkTop, 5f + i * 10f);
                if (style.RoadsideTinted) PropModelBuilder.TryAddModel(root, style.Roadside, false, position, side * style.RoadsideYaw, ColumnTint);
                else PropModelBuilder.TryAddModel(root, style.Roadside, false, position, side * style.RoadsideYaw);
            }

            // İki büyük yol kenarı nesnesi; iki yan aynı yerde durmaz (shift).
            PropModelBuilder.TryAddModel(root, style.PropA, false, new Vector3(side * style.PropAX, SidewalkTop, 9f + shift), side * style.PropAYaw);
            PropModelBuilder.TryAddModel(root, style.PropB, false, new Vector3(side * style.PropBX, SidewalkTop, 19f + shift), side * style.PropBYaw);

            // Binalar: her kopya ayrı renkte (zemin stilinin paleti). Yakın sıra ve daha geride uzak sıra.
            string near = side < 0 ? style.NearLeft : style.NearRight;
            string far = side < 0 ? style.FarLeft : style.FarRight;
            int first = side < 0 ? 0 : 2;
            PropModelBuilder.TryAddModel(root, near, false, new Vector3(side * style.NearX, SidewalkTop, 7f + shift), side * 90f, TowerPalette[first]);
            PropModelBuilder.TryAddModel(root, far, false, new Vector3(side * style.FarX, SidewalkTop, 22f - shift), side * 90f, TowerPalette[first + 1]);
        }

        // Binaların renkleri ve kolonların gövdesi zemin stilinden gelir (GroundStyle). Neon çizgileri dokudan gelir.
        static Color[] TowerPalette => _style.Towers;
        static Color ColumnTint => _style.ColumnTint;
    }
}
