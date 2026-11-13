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

        static PowerUpDefinition PowerUp(string file, string id, string nameKey, GameObject pickup, float duration, params EffectDefinition[] effects)
        {
            var powerUp = LoadOrCreate<PowerUpDefinition>($"{DataDir}/PowerUps/{file}.asset");
            powerUp.id = id;
            powerUp.nameKey = nameKey;
            powerUp.pickupPrefab = pickup;
            powerUp.duration = duration;
            powerUp.effects = effects;
            powerUp.spawnWeight = 1f;
            EditorUtility.SetDirty(powerUp);
            return powerUp;
        }
    }
}
