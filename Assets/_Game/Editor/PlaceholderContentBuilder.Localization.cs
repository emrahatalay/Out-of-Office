using System.Linq;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace OutOfOffice.EditorTools
{
    public static partial class PlaceholderContentBuilder
    {
        const string TableName = "Game";

        // anahtar, Türkçe, İngilizce. Yeni dil eklemek: Locale ekle, tabloya sütunu gir.
        static readonly string[][] Texts =
        {
            new[] { "ui.title", "SON MESAİ", "OUT OF OFFICE" },
            new[] { "ui.tagline", "Yapay zekâ işini aldı. Sıra sende.", "AI took your job. You're next." },
            new[] { "ui.play", "OYNA", "PLAY" },
            new[] { "ui.character", "MESLEK", "PROFESSION" },
            new[] { "ui.best", "En iyi", "Best" },
            new[] { "ui.score", "SKOR", "SCORE" },
            new[] { "ui.coins", "Altın", "Coins" },
            new[] { "ui.paused", "DURAKLATILDI", "PAUSED" },
            new[] { "ui.resume", "DEVAM ET", "RESUME" },
            new[] { "ui.menu", "ANA MENÜ", "MAIN MENU" },
            new[] { "ui.gameover.eyebrow", "İK-9000 SENİ YAKALADI", "HR-9000 CAUGHT YOU" },
            new[] { "ui.gameover", "OPTİMİZE EDİLDİN", "YOU'VE BEEN OPTIMIZED" },
            new[] { "ui.retry", "TEKRAR DENE", "TRY AGAIN" },
            new[] { "ui.newbest", "Yeni rekor!", "New record!" },
            new[] { "ui.automated", "OTOMATİKLEŞTİ", "AUTOMATED" },
            new[] { "ui.automated.suffix", "OTOMATİKLEŞTİ", "HAS BEEN AUTOMATED" },
            new[] { "ui.buyback", "İŞİNİ GERİ AL", "BUY JOB BACK" },
            new[] { "char.intern", "Stajyer", "Intern" },
            new[] { "char.developer", "Yazılımcı", "Developer" },
            new[] { "char.designer", "Grafiker", "Designer" },
            new[] { "char.driver", "Şoför", "Driver" },
            new[] { "ability.debug", "Debug", "Debug" },
            new[] { "ability.redraw", "Yeniden Çiz", "Redraw" },
            new[] { "ability.drive", "Sür", "Drive" },
            new[] { "powerup.captcha", "CAPTCHA", "CAPTCHA" },
            new[] { "powerup.hallucination", "Halüsinasyon", "Hallucination" },
            new[] { "powerup.shield", "Kıdem Kalkanı", "Seniority Shield" },
            new[] { "powerup.drone", "Kargo Dronu", "Cargo Drone" },
            new[] { "powerup.magnet", "Maaş Mıknatısı", "Salary Magnet" },
            new[] { "powerup.overtime", "Fazla Mesai x2", "Overtime x2" },
            new[] { "powerup.espresso", "Çift Espresso", "Double Espresso" },
            new[] { "powerup.prompt_injection", "Prompt Injection", "Prompt Injection" },
            new[] { "powerup.unplug", "Fişi Çek", "Pull the Plug" },
            new[] { "powerup.package", "Sürpriz Paket", "Surprise Package" },
            new[] { "tutorial.left", "Sola kaydır!", "Swipe left!" },
            new[] { "tutorial.right", "Sağa kaydır!", "Swipe right!" },
            new[] { "tutorial.jump", "Zıpla! Yukarı kaydır", "Jump! Swipe up" },
            new[] { "tutorial.slide", "Kay! Aşağı kaydır", "Slide! Swipe down" },
            new[] { "tutorial.go", "Altınları topla, İK-9000'den kaç!", "Grab the gold, outrun HR-9000!" },
            new[] { "ui.continue", "DEVAM ET", "CONTINUE" },
            new[] { "ui.giveup", "VAZGEÇ", "GIVE UP" },
            new[] { "ui.revive.hint", "Giriş kartını okut, kaldığın yerden devam et.", "Swipe your access card to continue where you fell." },
            new[] { "ui.rewards", "PAKETLERDEN ÇIKANLAR", "FROM YOUR PACKAGES" },
            new[] { "ui.store", "MAĞAZA", "STORE" },
            new[] { "ui.tab.home", "ANA SAYFA", "HOME" },
            new[] { "ui.tab.collection", "KOLEKSİYON", "COLLECTION" },
            new[] { "ui.tab.missions", "GÖREVLER", "MISSIONS" },
            new[] { "ui.missions", "GÜNLÜK GÖREVLER", "DAILY MISSIONS" },
            new[] { "ui.missions.hint", "Her gün yeni görevler. Mesaiye devam!", "New missions every day. Keep clocking in!" },
            new[] { "ui.mission.claim", "AL", "CLAIM" },
            new[] { "ui.mission.claimed", "ALINDI", "DONE" },
            new[] { "mission.coins", "Altın topla", "Collect coins" },
            new[] { "mission.distance", "Metre koş", "Run meters" },
            new[] { "mission.jumps", "Zıpla", "Jump" },
            new[] { "mission.slides", "Kay", "Slide" },
            new[] { "mission.lanes", "Şerit değiştir", "Switch lanes" },
            new[] { "mission.powerups", "Güçlendirme topla", "Grab power-ups" },
            new[] { "mission.obstacles", "Engel yok et", "Smash obstacles" },
            new[] { "mission.runs", "Mesaiye gir (koşu yap)", "Clock in (finish runs)" },
            new[] { "mission.score", "Tek koşuda puan yap", "Score in a single run" },
            new[] { "ui.tab.settings", "AYARLAR", "SETTINGS" },
            new[] { "ui.settings", "AYARLAR", "SETTINGS" },
            new[] { "ui.settings.language", "Dil", "Language" },
            new[] { "ui.settings.language.tr", "TÜRKÇE", "TÜRKÇE" },
            new[] { "ui.settings.language.en", "ENGLISH", "ENGLISH" },
            new[] { "ui.settings.sound", "Ses", "Sound" },
            new[] { "ui.settings.on", "AÇIK", "ON" },
            new[] { "ui.settings.off", "KAPALI", "OFF" },
            new[] { "ui.settings.quality", "Görüntü kalitesi", "Graphics quality" },
            new[] { "ui.settings.quality.low", "DÜŞÜK", "LOW" },
            new[] { "ui.settings.quality.medium", "ORTA", "MEDIUM" },
            new[] { "ui.settings.quality.high", "YÜKSEK", "HIGH" },
            // Geliştirici kartı (bilgiler GitHub profilinden; bağlantı GameConfig.developerUrl).
            new[] { "ui.dev", "GELİŞTİRİCİ", "DEVELOPER" },
            new[] { "ui.dev.initials", "EA", "EA" },
            new[] { "ui.dev.name", "Emrah Atalay", "Emrah Atalay" },
            new[] { "ui.dev.role", "Fullstack Geliştirici", "Fullstack Developer" },
            new[] { "ui.dev.location", "İstanbul, Türkiye", "Istanbul, Türkiye" },
            new[] { "ui.dev.github", "GITHUB PROFİLİ", "GITHUB PROFILE" },
            new[] { "ui.collection", "MESLEKLER", "PROFESSIONS" },
            new[] { "ui.collection.hint", "Oynamak istediğin mesleği seç.", "Pick the profession you want to play." },
            new[] { "ui.selected", "SEÇİLİ", "SELECTED" },
            new[] { "ui.close", "KAPAT", "CLOSE" },
            // Kodda "key" olarak geçen devam hakkı, oyunda Giriş Kartı olarak görünür.
            new[] { "item.key", "Giriş Kartı", "Access Card" },
            new[] { "acc.chair", "Sandalye", "Chair" },
            new[] { "chaser.hr9000", "İK-9000", "HR-9000" },
            new[] { "world.plaza", "Plaza", "Plaza" },
            new[] { "world.autonomous_city", "Otonom Şehir", "Autonomous City" },
            new[] { "ui.newarea", "YENİ BÖLGE", "NEW AREA" },
        };

        static string BuildLocalization()
        {
            EnsureFolder(LocalizationDir);

            var settings = LocalizationEditorSettings.ActiveLocalizationSettings;
            if (settings == null)
            {
                string guid = AssetDatabase.FindAssets("t:LocalizationSettings", new[] { "Assets" }).FirstOrDefault();
                settings = guid != null
                    ? AssetDatabase.LoadAssetAtPath<LocalizationSettings>(AssetDatabase.GUIDToAssetPath(guid))
                    : null;

                if (settings == null)
                {
                    settings = ScriptableObject.CreateInstance<LocalizationSettings>();
                    settings.name = "Localization Settings";
                    AssetDatabase.CreateAsset(settings, $"{LocalizationDir}/Localization Settings.asset");
                }

                LocalizationEditorSettings.ActiveLocalizationSettings = settings;
            }

            var turkish = EnsureLocale("tr");
            var english = EnsureLocale("en");

            // Açılış dili: önce oyuncunun son seçimi, sonra cihaz dili, o da yoksa İngilizce.
            var selectors = settings.GetStartupLocaleSelectors();
            if (!selectors.Any(selector => selector is PlayerPrefLocaleSelector))
                selectors.Insert(0, new PlayerPrefLocaleSelector());
            foreach (var specific in selectors.OfType<SpecificLocaleSelector>())
                specific.LocaleId = english.Identifier;
            EditorUtility.SetDirty(settings);

            var collection = LocalizationEditorSettings.GetStringTableCollection(TableName);
            if (collection == null)
            {
                EnsureFolder($"{LocalizationDir}/Tables");
                collection = LocalizationEditorSettings.CreateStringTableCollection(TableName, $"{LocalizationDir}/Tables");
            }

            // Sütun sırasına güvenme; her tabloyu dil koduyla eşleştir.
            foreach (var table in collection.StringTables)
            {
                string code = table.LocaleIdentifier.Code;
                int column = code == turkish.Identifier.Code ? 1 : code == english.Identifier.Code ? 2 : -1;
                if (column < 0) continue;

                foreach (var row in Texts) table.AddEntry(row[0], row[column]);
                EditorUtility.SetDirty(table);
            }

            EditorUtility.SetDirty(collection);
            EditorUtility.SetDirty(collection.SharedData);
            LocalizationEditorSettings.EditorEvents.RaiseCollectionModified(typeof(PlaceholderContentBuilder), collection);
            AssetDatabase.SaveAssets();

            return $"Çeviri: {Texts.Length} anahtar x {collection.StringTables.Count} dil.";
        }

        static Locale EnsureLocale(string code)
        {
            var locale = LocalizationEditorSettings.GetLocale(code);
            if (locale != null) return locale;

            EnsureFolder($"{LocalizationDir}/Locales");
            locale = Locale.CreateLocale(code);
            AssetDatabase.CreateAsset(locale, $"{LocalizationDir}/Locales/{code}.asset");
            LocalizationEditorSettings.AddLocale(locale);
            return locale;
        }
    }
}
