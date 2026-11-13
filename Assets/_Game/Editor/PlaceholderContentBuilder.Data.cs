using OutOfOffice.Data;
using OutOfOffice.Data.Effects;
using OutOfOffice.Data.Movement;
using UnityEditor;
using UnityEngine;

namespace OutOfOffice.EditorTools
{
    public static partial class PlaceholderContentBuilder
    {
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
            var jumpBoost = LoadOrCreate<JumpBoostEffect>($"{DataDir}/Effects/Effect_JumpBoost.asset");
            var attachDrone = Attach("Effect_AttachDrone", prefabs.CarrierDrone, Vector3.zero, OutOfOffice.Core.Run.PlayerPose.Hanging);
            var attachChair = Attach("Effect_AttachChair", prefabs.OfficeChair, Vector3.zero, OutOfOffice.Core.Run.PlayerPose.Seated);

            // ---- Yetenekler
            var debug = Ability("Ability_Debug", "debug", "ability.debug", 18f, 0f, destroy);
            // Yeni kod yazmadan: var olan "hayalet" etkisini kısa süreli kullanan ikinci yetenek.
            var redraw = Ability("Ability_Redraw", "redraw", "ability.redraw", 22f, 3f, ghost);

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

            var devBot = Obstacle("Obstacle_DevBot", "dev_bot", prefabs.DevBot, AvoidType.ChangeLane, botRush, 0f, 1.2f);
            var designBot = Obstacle("Obstacle_DesignBot", "design_bot", prefabs.DesignBot, AvoidType.ChangeLane, botSweep, 0f, 1.2f);

            var developer = Character("Character_Developer", "developer", "char.developer", prefabs.Developer, debug, true, 100, devBot);
            var designer = Character("Character_Designer", "designer", "char.designer", prefabs.Designer, redraw, true, 100, designBot);
            var intern = Character("Character_Intern", "intern", "char.intern", prefabs.Intern, null, false, 0, null);
            SetThemeColor(developer, new Color(0.2f, 0.5f, 0.95f));
            SetThemeColor(designer, new Color(0.9f, 0.3f, 0.7f));
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
                Obstacle("Obstacle_RobotVacuum", "robot_vacuum", prefabs.RobotVacuum, AvoidType.Jump, null, 0f, 0.8f),
                Obstacle("Obstacle_CargoDrone", "cargo_drone", prefabs.CargoDrone, AvoidType.Slide, sweep, 0.2f, 0.6f),
                Obstacle("Obstacle_AutoTaxi", "auto_taxi", prefabs.AutoTaxi, AvoidType.ChangeLane, approach, 0.35f, 0.6f),
            };

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
            EditorUtility.SetDirty(world);

            // ---- Ayarlar ve katalog (ayarlar yalnızca ilk kez oluşturulur; dengelemen korunur)
            LoadOrCreate<GameConfig>(ConfigPath);

            var catalog = LoadOrCreate<ContentCatalog>(CatalogPath);
            catalog.characters = new[] { developer, designer, intern };
            catalog.worlds = new[] { world };

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
                StoreItem("Store_Chair1", "chair_1", "acc.chair", OutOfOffice.Core.Run.RewardKind.Accessory, 1, 150),
                StoreItem("Store_Chair3", "chair_3", "acc.chair", OutOfOffice.Core.Run.RewardKind.Accessory, 3, 400),
                StoreItem("Store_Key1", "key_1", "item.key", OutOfOffice.Core.Run.RewardKind.Key, 1, 300),
                StoreItem("Store_Key3", "key_3", "item.key", OutOfOffice.Core.Run.RewardKind.Key, 3, 800),
            };
            EditorUtility.SetDirty(catalog);

            AssetDatabase.SaveAssets();
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
            character.ability = ability;
            character.canBeAutomated = canBeAutomated;
            character.buybackCost = buybackCost;
            character.automatedObstacle = robot;
            EditorUtility.SetDirty(character);
            return character;
        }

        static ObstacleDefinition Obstacle(string file, string id, GameObject prefab, AvoidType avoidBy, MovementPattern movement, float minDifficulty, float weight)
        {
            var obstacle = LoadOrCreate<ObstacleDefinition>($"{DataDir}/Obstacles/{file}.asset");
            obstacle.id = id;
            obstacle.prefab = prefab;
            obstacle.avoidBy = avoidBy;
            obstacle.movement = movement;
            obstacle.minDifficulty = minDifficulty;
            obstacle.spawnWeight = weight;
            EditorUtility.SetDirty(obstacle);
            return obstacle;
        }

        static void SetThemeColor(CharacterDefinition character, Color color)
        {
            character.themeColor = color;
            EditorUtility.SetDirty(character);
        }

        static StoreItemDefinition StoreItem(string file, string id, string nameKey, OutOfOffice.Core.Run.RewardKind kind, int amount, int price)
        {
            var item = LoadOrCreate<StoreItemDefinition>($"{DataDir}/Store/{file}.asset");
            item.id = id;
            item.nameKey = nameKey;
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
