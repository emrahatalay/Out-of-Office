using UnityEngine;

namespace OutOfOffice.EditorTools
{
    public static partial class PlaceholderContentBuilder
    {
        sealed class PlaceholderPrefabs
        {
            public GameObject Developer, Designer, Intern, Chaser;
            public GameObject DevBot, DesignBot;
            public GameObject ServerRack, CookieWall, RobotVacuum, CargoDrone, AutoTaxi;
            public GameObject Coin, Captcha, Hallucination, Shield;
            public GameObject DronePickup, Magnet, Overtime, Espresso, OfficeChair, Package;
            public GameObject Ground;
        }

        // Ölçüler: oyuncu 1.8 m boyunda, kayarken 0.8 m. Zıplama tepesi 2.2 m.
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
            var cyan = Mat("ShieldCyan", new Color(0.25f, 0.85f, 0.95f), 0.6f, 1.5f);
            var espresso = Mat("EspressoBrown", new Color(0.55f, 0.33f, 0.18f), 0.5f, 1.2f);
            var internGrey = Mat("InternGrey", new Color(0.62f, 0.66f, 0.72f));
            var floor = Mat("Floor", new Color(0.3f, 0.33f, 0.4f), 0.1f);
            var floorStripe = Mat("FloorStripe", new Color(0.25f, 0.28f, 0.34f), 0.1f);
            var wall = Mat("Wall", new Color(0.2f, 0.23f, 0.32f), 0.1f);

            return new PlaceholderPrefabs
            {
                // Gerçek modeli kurulmuş karakter (CharacterModelBuilder) geçici küpün yerini alır.
                Developer = CharacterModelBuilder.PrefabOr("Character_Developer", BuildCharacter("Character_Developer", blue, white, dark, false)),
                Designer = CharacterModelBuilder.PrefabOr("Character_Designer", BuildCharacter("Character_Designer", magenta, white, yellow, true)),
                Intern = CharacterModelBuilder.PrefabOr("Character_Intern", BuildIntern(internGrey, white, orange)),
                Chaser = BuildChaser(steel, red, glowWhite),
                DevBot = BuildProfessionBot("Obstacle_DevBot", steel, blue, green),
                DesignBot = BuildProfessionBot("Obstacle_DesignBot", steel, magenta, yellow),
                ServerRack = BuildServerRack(dark, green),
                CookieWall = BuildCookieWall(orange, steel),
                RobotVacuum = BuildRobotVacuum(steel, red),
                CargoDrone = BuildCargoDrone(white, dark, orange),
                AutoTaxi = BuildAutoTaxi(yellow, dark),
                Coin = BuildCoin(yellow),
                Captcha = BuildPowerUp("Pickup_Captcha", green, white),
                Hallucination = BuildPowerUp("Pickup_Hallucination", purple, white),
                Shield = BuildPowerUp("Pickup_Shield", cyan, white),
                DronePickup = BuildPowerUp("Pickup_Drone", orange, white),
                Magnet = BuildPowerUp("Pickup_Magnet", red, white),
                Overtime = BuildPowerUp("Pickup_Overtime", yellow, white),
                Espresso = BuildPowerUp("Pickup_Espresso", espresso, white),
                OfficeChair = BuildOfficeChair(dark, steel),
                Package = BuildPackage(espresso, yellow),
                Ground = BuildGround(floor, floorStripe, white, wall, dark),
            };
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
        static GameObject BuildProfessionBot(string name, Material frame, Material body, Material screen)
        {
            var root = new GameObject(name);
            Part(root.transform, PrimitiveType.Cube, "Base", new Vector3(0f, 0.25f, 0f), new Vector3(1.5f, 0.5f, 1.1f), frame);
            Part(root.transform, PrimitiveType.Cube, "Body", new Vector3(0f, 1.2f, 0f), new Vector3(1.2f, 1.4f, 0.8f), body);
            Part(root.transform, PrimitiveType.Cube, "Screen", new Vector3(0f, 1.35f, -0.41f), new Vector3(0.9f, 0.7f, 0.03f), screen);
            Part(root.transform, PrimitiveType.Cube, "Head", new Vector3(0f, 2.15f, 0f), new Vector3(0.7f, 0.4f, 0.6f), frame);

            BoxTrigger(root, new Vector3(0f, 1.2f, 0f), new Vector3(1.5f, 2.4f, 1.1f));
            CombineByMaterial(root, castShadows: true);
            return SavePrefab(root);
        }

        // Kaykayın karşılığı: tekerlekli ofis sandalyesi. Yalnızca görsel; oyuncuya takılır.
        static GameObject BuildOfficeChair(Material seat, Material metal)
        {
            var root = new GameObject("Accessory_OfficeChair");
            Part(root.transform, PrimitiveType.Cylinder, "Base", new Vector3(0f, 0.06f, 0f), new Vector3(1.1f, 0.05f, 1.1f), metal);
            Part(root.transform, PrimitiveType.Cylinder, "Stem", new Vector3(0f, 0.25f, 0f), new Vector3(0.12f, 0.2f, 0.12f), metal);
            Part(root.transform, PrimitiveType.Cube, "Seat", new Vector3(0f, 0.48f, 0f), new Vector3(0.95f, 0.14f, 0.9f), seat);
            Part(root.transform, PrimitiveType.Cube, "Back", new Vector3(0f, 1.05f, -0.48f), new Vector3(0.9f, 1.1f, 0.12f), seat);
            return SavePrefab(root);
        }

        // Sürpriz paket: kurdeleli kutu. Oyun sonunda açılır.
        static GameObject BuildPackage(Material box, Material ribbon)
        {
            var root = new GameObject("Pickup_Package");
            Part(root.transform, PrimitiveType.Cube, "Box", new Vector3(0f, 1.05f, 0f), new Vector3(0.9f, 0.9f, 0.9f), box);
            Part(root.transform, PrimitiveType.Cube, "RibbonA", new Vector3(0f, 1.05f, 0f), new Vector3(0.94f, 0.94f, 0.2f), ribbon);
            Part(root.transform, PrimitiveType.Cube, "RibbonB", new Vector3(0f, 1.05f, 0f), new Vector3(0.2f, 0.94f, 0.94f), ribbon);

            SphereTrigger(root, new Vector3(0f, 1.05f, 0f), 0.85f);
            CombineByMaterial(root, castShadows: false);
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
            Part(root.transform, PrimitiveType.Cube, "Rack", new Vector3(0f, 1.3f, 0f), new Vector3(1.8f, 2.6f, 1.2f), body);
            for (int i = 0; i < 4; i++)
                Part(root.transform, PrimitiveType.Cube, "Led" + i, new Vector3(0f, 0.6f + i * 0.5f, -0.61f), new Vector3(1.4f, 0.08f, 0.02f), led);

            BoxTrigger(root, new Vector3(0f, 1.3f, 0f), new Vector3(1.7f, 2.6f, 1.1f));
            CombineByMaterial(root, castShadows: true);
            return SavePrefab(root);
        }

        // Altından kayılır: "Çerezleri kabul et" duvarı. Alt kenarı 1.1 m'de.
        static GameObject BuildCookieWall(Material banner, Material post)
        {
            var root = new GameObject("Obstacle_CookieWall");
            Part(root.transform, PrimitiveType.Cube, "Banner", new Vector3(0f, 1.9f, 0f), new Vector3(2.3f, 1.6f, 0.3f), banner);
            Part(root.transform, PrimitiveType.Cube, "PostLeft", new Vector3(-1.1f, 0.55f, 0f), new Vector3(0.12f, 1.1f, 0.12f), post);
            Part(root.transform, PrimitiveType.Cube, "PostRight", new Vector3(1.1f, 0.55f, 0f), new Vector3(0.12f, 1.1f, 0.12f), post);

            BoxTrigger(root, new Vector3(0f, 1.9f, 0f), new Vector3(2.3f, 1.6f, 0.8f));
            CombineByMaterial(root, castShadows: true);
            return SavePrefab(root);
        }

        // Üstünden atlanır: robot süpürge.
        static GameObject BuildRobotVacuum(Material body, Material light)
        {
            var root = new GameObject("Obstacle_RobotVacuum");
            Part(root.transform, PrimitiveType.Cylinder, "Body", new Vector3(0f, 0.22f, 0f), new Vector3(1.7f, 0.22f, 1.7f), body);
            Part(root.transform, PrimitiveType.Cylinder, "Sensor", new Vector3(0f, 0.5f, 0f), new Vector3(0.5f, 0.07f, 0.5f), light);

            BoxTrigger(root, new Vector3(0f, 0.35f, 0f), new Vector3(1.6f, 0.7f, 1.4f));
            CombineByMaterial(root, castShadows: true);
            return SavePrefab(root);
        }

        // Havada süzülür, altından kayılır. Alt kenarı 1.2 m'de.
        static GameObject BuildCargoDrone(Material body, Material rotor, Material cargo)
        {
            var root = new GameObject("Obstacle_CargoDrone");
            Part(root.transform, PrimitiveType.Cube, "Body", new Vector3(0f, 1.95f, 0f), new Vector3(1f, 0.3f, 1f), body);
            Part(root.transform, PrimitiveType.Cube, "Cargo", new Vector3(0f, 1.5f, 0f), new Vector3(0.7f, 0.6f, 0.7f), cargo);
            for (int i = 0; i < 4; i++)
            {
                float x = i % 2 == 0 ? -0.65f : 0.65f;
                float z = i < 2 ? -0.65f : 0.65f;
                Part(root.transform, PrimitiveType.Cylinder, "Rotor" + i, new Vector3(x, 2.15f, z), new Vector3(0.6f, 0.03f, 0.6f), rotor);
            }

            BoxTrigger(root, new Vector3(0f, 1.75f, 0f), new Vector3(1.5f, 1.1f, 1.3f));
            CombineByMaterial(root, castShadows: true);
            return SavePrefab(root);
        }

        // Oyuncuya doğru gelir, şerit değiştirerek kaçılır.
        static GameObject BuildAutoTaxi(Material body, Material glass)
        {
            var root = new GameObject("Obstacle_AutoTaxi");
            Part(root.transform, PrimitiveType.Cube, "Body", new Vector3(0f, 0.6f, 0f), new Vector3(1.8f, 0.9f, 3.6f), body);
            Part(root.transform, PrimitiveType.Cube, "Cabin", new Vector3(0f, 1.35f, 0.2f), new Vector3(1.6f, 0.7f, 1.8f), glass);
            Part(root.transform, PrimitiveType.Cube, "Sensor", new Vector3(0f, 1.85f, 0.2f), new Vector3(0.4f, 0.3f, 0.4f), body);

            BoxTrigger(root, new Vector3(0f, 1f, 0f), new Vector3(1.7f, 2f, 3.5f));
            CombineByMaterial(root, castShadows: true);
            return SavePrefab(root);
        }

        static GameObject BuildCoin(Material material)
        {
            var root = new GameObject("Pickup_Coin");
            Part(root.transform, PrimitiveType.Cylinder, "Disc", new Vector3(0f, 1f, 0f), new Vector3(0.7f, 0.05f, 0.7f), material, new Vector3(90f, 0f, 0f));

            SphereTrigger(root, new Vector3(0f, 1f, 0f), 0.7f);
            CombineByMaterial(root, castShadows: false);
            return SavePrefab(root);
        }

        static GameObject BuildPowerUp(string name, Material core, Material ring)
        {
            var root = new GameObject(name);
            Part(root.transform, PrimitiveType.Sphere, "Core", new Vector3(0f, 1.1f, 0f), new Vector3(0.8f, 0.8f, 0.8f), core);
            Part(root.transform, PrimitiveType.Cylinder, "Ring", new Vector3(0f, 1.1f, 0f), new Vector3(1.15f, 0.03f, 1.15f), ring, new Vector3(90f, 0f, 0f));

            SphereTrigger(root, new Vector3(0f, 1.1f, 0f), 0.8f);
            CombineByMaterial(root, castShadows: false);
            return SavePrefab(root);
        }

        // 30 m'lik ofis koridoru. Orijin parçanın başında, +Z yönünde uzanır.
        static GameObject BuildGround(Material floor, Material stripe, Material line, Material wall, Material desk)
        {
            const float length = 30f;
            const float width = 8.5f;

            var root = new GameObject("Ground_Office");
            SlicedPart(root.transform, "Floor", new Vector3(0f, -0.25f, length * 0.5f), new Vector3(width, 0.5f, length), floor);

            // Hız hissi için enine şeritler, şerit çizgileri ve kolonlar.
            for (int i = 0; i < 6; i++)
                Part(root.transform, PrimitiveType.Cube, "Stripe" + i, new Vector3(0f, 0.005f, 2.5f + i * 5f), new Vector3(width, 0.01f, 0.3f), stripe);

            SlicedPart(root.transform, "LaneLineLeft", new Vector3(-1.25f, 0.012f, length * 0.5f), new Vector3(0.08f, 0.01f, length), line);
            SlicedPart(root.transform, "LaneLineRight", new Vector3(1.25f, 0.012f, length * 0.5f), new Vector3(0.08f, 0.01f, length), line);

            for (int side = -1; side <= 1; side += 2)
            {
                string suffix = side < 0 ? "Left" : "Right";
                SlicedPart(root.transform, "Wall" + suffix, new Vector3(side * 4.6f, 0.6f, length * 0.5f), new Vector3(0.4f, 1.2f, length), wall);

                for (int i = 0; i < 3; i++)
                {
                    float z = 5f + i * 10f;
                    Part(root.transform, PrimitiveType.Cube, $"Pillar{suffix}{i}", new Vector3(side * 5.4f, 2.5f, z), new Vector3(0.8f, 5f, 0.8f), wall);
                    Part(root.transform, PrimitiveType.Cube, $"Desk{suffix}{i}", new Vector3(side * 6.8f, 0.4f, z + 5f), new Vector3(1.6f, 0.8f, 0.9f), desk);
                }
            }

            CombineByMaterial(root, castShadows: false);
            return SavePrefab(root);
        }
    }
}
