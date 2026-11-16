using OutOfOffice.Data;
using OutOfOffice.Data.Effects;
using OutOfOffice.Data.Movement;
using UnityEditor;
using UnityEngine;

namespace OutOfOffice.EditorTools
{
    public static partial class PlaceholderContentBuilder
    {
        /// <summary>Otonom Şehir koşunun kaçıncı metresinde başlar (Plaza ~70 sn sürer).</summary>
        const float CityStartDistance = 1200f;

        static void BuildData(PlaceholderPrefabs prefabs)
        {
            // ---- Etkiler: güçlendirme ve yeteneklerin yapı taşları
            var stun = LoadOrCreate<StunChaserEffect>($"{DataDir}/Effects/Effect_StunChaser.asset");
            var ghost = LoadOrCreate<GhostObstaclesEffect>($"{DataDir}/Effects/Effect_GhostObstacles.asset");
            var shield = LoadOrCreate<ShieldEffect>($"{DataDir}/Effects/Effect_Shield.asset");
            var destroy = LoadOrCreate<DestroyObstaclesEffect>($"{DataDir}/Effects/Effect_DestroyObstacles.asset");
            destroy.range = 45f;
            EditorUtility.SetDirty(destroy);

            var flight = LoadOrCreate<FlightEffect>($"{DataDir}/Effects/Effect_Flight.asset");
            var magnet = LoadOrCreate<MagnetEffect>($"{DataDir}/Effects/Effect_Magnet.asset");
            var scoreX2 = LoadOrCreate<ScoreMultiplierEffect>($"{DataDir}/Effects/Effect_ScoreX2.asset");
            var collectPackage = LoadOrCreate<CollectPackageEffect>($"{DataDir}/Effects/Effect_CollectPackage.asset");
            var autoCollect = LoadOrCreate<AutoCollectEffect>($"{DataDir}/Effects/Effect_AutoCollect.asset");
            autoCollect.range = 30f;
            EditorUtility.SetDirty(autoCollect);
            var unplug = LoadOrCreate<UnplugRobotsEffect>($"{DataDir}/Effects/Effect_UnplugRobots.asset");
            var jumpBoost = LoadOrCreate<JumpBoostEffect>($"{DataDir}/Effects/Effect_JumpBoost.asset");
            var attachDrone = Attach("Effect_AttachDrone", prefabs.CarrierDrone, Vector3.zero, OutOfOffice.Core.Run.PlayerPose.Hanging);
            // Oturan karakterin ağırlık merkezi kökün üstündedir, kalçası onun gerisinde kalır;
            // sandalye, sırt çantası sırtlığa dayanacak kadar geriye alınır (Play'de ölçüldü).
            var attachChair = Attach("Effect_AttachChair", prefabs.OfficeChair, new Vector3(0f, 0f, -0.22f), OutOfOffice.Core.Run.PlayerPose.Seated);

            var attachPod = Attach("Effect_AttachPod", prefabs.DriverPod, new Vector3(0f, 0f, 0.1f), OutOfOffice.Core.Run.PlayerPose.Seated);

            // ---- Yetenekler
            var debug = Ability("Ability_Debug", "debug", "ability.debug", 18f, 0f, destroy);
            // Yeni kod yazmadan: var olan "hayalet" etkisini kısa süreli kullanan ikinci yetenek.
            var redraw = Ability("Ability_Redraw", "redraw", "ability.redraw", 22f, 3f, ghost);

            // Şoför: otonom kapsüle biner; süre boyunca tek çarpışmayı affeder (kalkan + araç görseli).
            var drive = Ability("Ability_Drive", "drive", "ability.drive", 25f, 6f, shield, attachPod);

            // ---- Karakterler
            // ---- Meslek robotları: meslek otomatikleşince engel olarak çıkar.
            // Davranışları yeni kod değil, var olan hareket desenlerinin yeni ayarlarıdır.
            var botRush = LoadOrCreate<ApproachPattern>($"{DataDir}/Movement/Movement_BotRush.asset");
            botRush.speed = 5f;
            EditorUtility.SetDirty(botRush);

            var botSweep = LoadOrCreate<SideSweepPattern>($"{DataDir}/Movement/Movement_BotSweep.asset");
            botSweep.lanes = 1f;
            botSweep.frequency = 0.5f;
            EditorUtility.SetDirty(botSweep);

            var botDash = LoadOrCreate<ApproachPattern>($"{DataDir}/Movement/Movement_BotDash.asset");
            botDash.speed = 9f;
            EditorUtility.SetDirty(botDash);

            var devBot = Obstacle("Obstacle_DevBot", "dev_bot", prefabs.DevBot, AvoidType.ChangeLane, botRush, 0f, 1.2f, robot: true);
            var designBot = Obstacle("Obstacle_DesignBot", "design_bot", prefabs.DesignBot, AvoidType.ChangeLane, botSweep, 0f, 1.2f, robot: true);

            var driverBot = Obstacle("Obstacle_DriverBot", "driver_bot", prefabs.DriverBot, AvoidType.ChangeLane, botDash, 0f, 1f, robot: true);

            var developer = Character("Character_Developer", "developer", "char.developer", prefabs.Developer, debug, true, 100, devBot);
            var designer = Character("Character_Designer", "designer", "char.designer", prefabs.Designer, redraw, true, 100, designBot);
            var driver = Character("Character_Driver", "driver", "char.driver", prefabs.Driver, drive, true, 100, driverBot);
            var intern = Character("Character_Intern", "intern", "char.intern", prefabs.Intern, null, false, 0, null);
            SetThemeColor(developer, new Color(0.2f, 0.5f, 0.95f));
            SetThemeColor(designer, new Color(0.9f, 0.3f, 0.7f));
            SetThemeColor(driver, new Color(1f, 0.55f, 0.1f));
            SetThemeColor(intern, new Color(0.5f, 0.55f, 0.62f));

            // ---- Kovalayan
            var chaser = LoadOrCreate<ChaserDefinition>($"{DataDir}/Chasers/Chaser_HR9000.asset");
            chaser.id = "hr9000";
            chaser.nameKey = "chaser.hr9000";
            chaser.prefab = prefabs.Chaser;
            EditorUtility.SetDirty(chaser);

            // ---- Hareket desenleri
            var sweep = LoadOrCreate<SideSweepPattern>($"{DataDir}/Movement/Movement_DroneSweep.asset");
            sweep.lanes = 1f;
            sweep.frequency = 0.35f;
            EditorUtility.SetDirty(sweep);

            var approach = LoadOrCreate<ApproachPattern>($"{DataDir}/Movement/Movement_TaxiApproach.asset");
            approach.speed = 8f;
            EditorUtility.SetDirty(approach);

            // ---- Engeller
            var obstacles = new[]
            {
                Obstacle("Obstacle_ServerRack", "server_rack", prefabs.ServerRack, AvoidType.ChangeLane, null, 0f, 1f),
                Obstacle("Obstacle_CookieWall", "cookie_wall", prefabs.CookieWall, AvoidType.Slide, null, 0f, 0.8f),
                Obstacle("Obstacle_RobotVacuum", "robot_vacuum", prefabs.RobotVacuum, AvoidType.Jump, null, 0f, 0.8f, robot: true),
                Obstacle("Obstacle_CargoDrone", "cargo_drone", prefabs.CargoDrone, AvoidType.Slide, sweep, 0.2f, 0.6f, robot: true),
                Obstacle("Obstacle_AutoTaxi", "auto_taxi", prefabs.AutoTaxi, AvoidType.ChangeLane, approach, 0.35f, 0.6f, robot: true),
            };

            // Otonom Şehir engelleri: yeni üçü + Plaza'dan şehre yakışanlar (taksi, dron, süpürge).
            var busApproach = LoadOrCreate<ApproachPattern>($"{DataDir}/Movement/Movement_BusApproach.asset");
            busApproach.speed = 4f;
            EditorUtility.SetDirty(busApproach);
            var cityObstacles = new[]
            {
                Obstacle("Obstacle_AutoBus", "auto_bus", prefabs.AutoBus, AvoidType.ChangeLane, busApproach, 0f, 1f, robot: true),
                Obstacle("Obstacle_GateBarrier", "gate_barrier", prefabs.GateBarrier, AvoidType.Slide, null, 0f, 0.9f),
                Obstacle("Obstacle_TrafficCone", "traffic_cone", prefabs.TrafficCone, AvoidType.Jump, null, 0f, 0.9f, robot: true),
                obstacles[4], // otonom taksi
                obstacles[3], // kargo dronu
                obstacles[2], // robot süpürge
            };

            // Dönen parçalar: dron pervaneleri (dönüş yönü karşılıklı) ve taksi tekerlekleri (yere doğru gelirken ileri yuvarlanır).
            obstacles[3].spinParts = new[]
            {
                new SpinPart { childName = "Rotor0", axis = Vector3.up, degreesPerSecond = 1500f },
                new SpinPart { childName = "Rotor1", axis = Vector3.up, degreesPerSecond = -1500f },
                new SpinPart { childName = "Rotor2", axis = Vector3.up, degreesPerSecond = -1500f },
                new SpinPart { childName = "Rotor3", axis = Vector3.up, degreesPerSecond = 1500f },
            };
            obstacles[4].spinParts = new[] { new SpinPart { childName = "Wheel", axis = Vector3.right, degreesPerSecond = -720f } };
            EditorUtility.SetDirty(obstacles[3]);
            EditorUtility.SetDirty(obstacles[4]);

            // ---- Güçlendirmeler
            var powerUps = new[]
            {
                PowerUp("PowerUp_Captcha", "captcha", "powerup.captcha", prefabs.Captcha, 6f, stun),
                PowerUp("PowerUp_Hallucination", "hallucination", "powerup.hallucination", prefabs.Hallucination, 5f, ghost),
                PowerUp("PowerUp_Shield", "shield", "powerup.shield", prefabs.Shield, 12f, shield),
                // Jetpack karşılığı: uçuş + tepede dron görseli. Daha nadir çıkar.
                PowerUp("PowerUp_Drone", "drone", "powerup.drone", prefabs.DronePickup, 8f, 0.5f, flight, attachDrone),
                PowerUp("PowerUp_Magnet", "magnet", "powerup.magnet", prefabs.Magnet, 10f, magnet),
                PowerUp("PowerUp_Overtime", "overtime", "powerup.overtime", prefabs.Overtime, 12f, scoreX2),
                PowerUp("PowerUp_Espresso", "espresso", "powerup.espresso", prefabs.Espresso, 10f, jumpBoost),
                // Daha nadir: Prompt Injection altınları toplatır, Fişi Çek robotları kapatır.
                PowerUp("PowerUp_PromptInjection", "prompt_injection", "powerup.prompt_injection", prefabs.PromptInjection, 8f, 0.6f, autoCollect),
                PowerUp("PowerUp_Unplug", "unplug", "powerup.unplug", prefabs.Unplug, 5f, 0.6f, stun, unplug),
                // Sürpriz paket: süresi 0, anında sayılır; içeriği oyun sonunda açılır.
                PowerUp("PowerUp_Package", "package", "powerup.package", prefabs.Package, 0f, 2f, collectPackage),
            };

            // ---- Dünya
            var world = LoadOrCreate<WorldDefinition>($"{DataDir}/Worlds/World_Plaza.asset");
            world.id = "plaza";
            world.nameKey = "world.plaza";
            world.groundSegmentPrefab = prefabs.Ground;
            world.segmentLength = 30f;
            world.chaser = chaser;
            world.obstacles = obstacles;
            world.powerUps = powerUps;
            world.coinPrefab = prefabs.Coin;
            world.startDistance = 0f;
            world.atmosphere = PlazaAtmosphere();
            EditorUtility.SetDirty(world);

            // Otonom Şehir: Plaza'dan sonra gelir. İçerik (zemin, dekor, engel) ayrıca özelleşir; şimdilik Plaza'nın içeriğini
            // kullanır ve yalnızca havası farklıdır.
            var city = LoadOrCreate<WorldDefinition>($"{DataDir}/Worlds/World_AutonomousCity.asset");
            city.id = "autonomous_city";
            city.nameKey = "world.autonomous_city";
            city.groundSegmentPrefab = prefabs.CityGround;
            city.segmentLength = 30f;
            city.chaser = chaser;
            city.obstacles = cityObstacles;
            city.powerUps = powerUps;
            city.coinPrefab = prefabs.Coin;
            city.startDistance = CityStartDistance;
            city.atmosphere = CityAtmosphere();
            EditorUtility.SetDirty(city);

            // ---- Ayarlar ve katalog (ayarlar yalnızca ilk kez oluşturulur; dengelemen korunur)
            LoadOrCreate<GameConfig>(ConfigPath);

            var catalog = LoadOrCreate<ContentCatalog>(CatalogPath);
            catalog.characters = new[] { developer, designer, driver, intern };
            catalog.worlds = new[] { world, city };
            catalog.tutorialArrow = prefabs.TutorialArrow;

            // ---- Aksesuar: kaykay karşılığı. Kalkan + sandalye görseli; çarpınca sandalye kırılır.
            var chair = LoadOrCreate<AccessoryDefinition>($"{DataDir}/Accessories/Accessory_OfficeChair.asset");
            chair.id = "office_chair";
            chair.nameKey = "acc.chair";
            chair.price = 150;
            chair.duration = 20f;
            chair.effects = new EffectDefinition[] { shield, attachChair };
            EditorUtility.SetDirty(chair);
            catalog.accessories = new[] { chair };

            // ---- Sürpriz paket ödülleri: ağırlık arttıkça çıkma olasılığı artar.
            var loot = LoadOrCreate<LootTable>($"{DataDir}/LootTable_Package.asset");
            loot.entries = new[]
            {
                new LootEntry { kind = OutOfOffice.Core.Run.RewardKind.Coins, min = 40, max = 150, weight = 5f },
                new LootEntry { kind = OutOfOffice.Core.Run.RewardKind.Accessory, min = 1, max = 2, weight = 3f },
                new LootEntry { kind = OutOfOffice.Core.Run.RewardKind.Key, min = 1, max = 1, weight = 2f },
            };
            EditorUtility.SetDirty(loot);
            catalog.packageLoot = loot;

            // ---- Mağaza ürünleri
            catalog.storeItems = new[]
            {
                StoreItem("Store_Chair1", "chair_1", "acc.chair", "OfficeChair", OutOfOffice.Core.Run.RewardKind.Accessory, 1, 150),
                StoreItem("Store_Chair3", "chair_3", "acc.chair", "OfficeChair", OutOfOffice.Core.Run.RewardKind.Accessory, 3, 400),
                StoreItem("Store_Key1", "key_1", "item.key", "AccessCard", OutOfOffice.Core.Run.RewardKind.Key, 1, 300),
                StoreItem("Store_Key3", "key_3", "item.key", "AccessCard", OutOfOffice.Core.Run.RewardKind.Key, 3, 800),
            };

            // ---- Günlük görev havuzu: her gün farklı türden 3 tanesi seçilir (MissionSystem).
            // Hedefler bir koşunun ~60-90 sn sürdüğü, 3-5 koşunun bir günün hafif oyunu olduğu varsayımıyla;
            // anahtar ödülü yalnızca zor hedeflerde.
            catalog.missions = new[]
            {
                Mission("coins_60", "mission.coins", OutOfOffice.Core.Run.MissionMetric.Coins, 60, OutOfOffice.Core.Run.RewardKind.Coins, 50),
                Mission("coins_200", "mission.coins", OutOfOffice.Core.Run.MissionMetric.Coins, 200, OutOfOffice.Core.Run.RewardKind.Coins, 150),
                Mission("distance_2000", "mission.distance", OutOfOffice.Core.Run.MissionMetric.Distance, 2000, OutOfOffice.Core.Run.RewardKind.Coins, 60),
                Mission("distance_5000", "mission.distance", OutOfOffice.Core.Run.MissionMetric.Distance, 5000, OutOfOffice.Core.Run.RewardKind.Key, 1),
                Mission("jumps_20", "mission.jumps", OutOfOffice.Core.Run.MissionMetric.Jumps, 20, OutOfOffice.Core.Run.RewardKind.Coins, 40),
                Mission("slides_15", "mission.slides", OutOfOffice.Core.Run.MissionMetric.Slides, 15, OutOfOffice.Core.Run.RewardKind.Coins, 40),
                Mission("lanes_40", "mission.lanes", OutOfOffice.Core.Run.MissionMetric.LaneChanges, 40, OutOfOffice.Core.Run.RewardKind.Coins, 40),
                Mission("powerups_4", "mission.powerups", OutOfOffice.Core.Run.MissionMetric.PowerUps, 4, OutOfOffice.Core.Run.RewardKind.Coins, 60),
                Mission("obstacles_8", "mission.obstacles", OutOfOffice.Core.Run.MissionMetric.Obstacles, 8, OutOfOffice.Core.Run.RewardKind.Key, 1),
                Mission("runs_4", "mission.runs", OutOfOffice.Core.Run.MissionMetric.Runs, 4, OutOfOffice.Core.Run.RewardKind.Coins, 60),
                Mission("score_2500", "mission.score", OutOfOffice.Core.Run.MissionMetric.ScoreInRun, 2500, OutOfOffice.Core.Run.RewardKind.Coins, 100),
            };
            EditorUtility.SetDirty(catalog);

            AssetDatabase.SaveAssets();
        }

        static MissionDefinition Mission(string id, string nameKey, OutOfOffice.Core.Run.MissionMetric metric, int target, OutOfOffice.Core.Run.RewardKind rewardKind, int rewardAmount)
        {
            var mission = LoadOrCreate<MissionDefinition>($"{DataDir}/Missions/Mission_{id}.asset");
            mission.id = id;
            mission.nameKey = nameKey;
            mission.metric = metric;
            mission.target = target;
            mission.rewardKind = rewardKind;
            mission.rewardAmount = rewardAmount;
            EditorUtility.SetDirty(mission);
            return mission;
        }

        static AbilityDefinition Ability(string file, string id, string nameKey, float cooldown, float duration, params EffectDefinition[] effects)
        {
            var ability = LoadOrCreate<AbilityDefinition>($"{DataDir}/Abilities/{file}.asset");
            ability.id = id;
            ability.nameKey = nameKey;
            ability.cooldown = cooldown;
            ability.duration = duration;
            ability.effects = effects;
            EditorUtility.SetDirty(ability);
            return ability;
        }

        static CharacterDefinition Character(string file, string id, string nameKey, GameObject prefab, AbilityDefinition ability,
            bool canBeAutomated, int buybackCost, ObstacleDefinition robot)
        {
            var character = LoadOrCreate<CharacterDefinition>($"{DataDir}/Characters/{file}.asset");
            character.id = id;
            character.nameKey = nameKey;
            character.prefab = prefab;
            // Portre, prefab'ıyla aynı adı taşır ("Character_Developer" -> "Developer"); yoksa boş kalır.
            character.portrait = PortraitBuilder.Load(file.Replace("Character_", string.Empty));
            character.ability = ability;
            character.canBeAutomated = canBeAutomated;
            character.buybackCost = buybackCost;
            character.automatedObstacle = robot;
            EditorUtility.SetDirty(character);
            return character;
        }

        static ObstacleDefinition Obstacle(string file, string id, GameObject prefab, AvoidType avoidBy, MovementPattern movement, float minDifficulty, float weight, bool robot = false)
        {
            var obstacle = LoadOrCreate<ObstacleDefinition>($"{DataDir}/Obstacles/{file}.asset");
            obstacle.id = id;
            obstacle.prefab = prefab;
            obstacle.avoidBy = avoidBy;
            obstacle.movement = movement;
            obstacle.minDifficulty = minDifficulty;
            obstacle.spawnWeight = weight;
            obstacle.isRobot = robot;
            EditorUtility.SetDirty(obstacle);
            return obstacle;
        }

        static void SetThemeColor(CharacterDefinition character, Color color)
        {
            character.themeColor = color;
            EditorUtility.SetDirty(character);
        }

        /// <param name="icon">UI/Items altındaki görselin adı (ItemIconBuilder); üretilmemişse boş kalır.</param>
        static StoreItemDefinition StoreItem(string file, string id, string nameKey, string icon, OutOfOffice.Core.Run.RewardKind kind, int amount, int price)
        {
            var item = LoadOrCreate<StoreItemDefinition>($"{DataDir}/Store/{file}.asset");
            item.id = id;
            item.nameKey = nameKey;
            item.icon = ItemIconBuilder.Load(icon);
            item.kind = kind;
            item.amount = amount;
            item.price = price;
            EditorUtility.SetDirty(item);
            return item;
        }

        static AttachVisualEffect Attach(string file, GameObject prefab, Vector3 offset, OutOfOffice.Core.Run.PlayerPose pose)
        {
            var effect = LoadOrCreate<AttachVisualEffect>($"{DataDir}/Effects/{file}.asset");
            effect.prefab = prefab;
            effect.localOffset = offset;
            effect.pose = pose;
            EditorUtility.SetDirty(effect);
            return effect;
        }

        static PowerUpDefinition PowerUp(string file, string id, string nameKey, GameObject pickup, float duration, params EffectDefinition[] effects)
        {
            return PowerUp(file, id, nameKey, pickup, duration, 1f, effects);
        }

        static PowerUpDefinition PowerUp(string file, string id, string nameKey, GameObject pickup, float duration, float weight, params EffectDefinition[] effects)
        {
            var powerUp = LoadOrCreate<PowerUpDefinition>($"{DataDir}/PowerUps/{file}.asset");
            powerUp.id = id;
            powerUp.nameKey = nameKey;
            powerUp.pickupPrefab = pickup;
            powerUp.duration = duration;
            powerUp.effects = effects;
            powerUp.spawnWeight = weight;
            EditorUtility.SetDirty(powerUp);
            return powerUp;
        }
    }
}
