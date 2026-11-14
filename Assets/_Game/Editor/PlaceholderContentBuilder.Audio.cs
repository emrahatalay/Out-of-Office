using System.Collections.Generic;
using System.IO;
using OutOfOffice.Data;
using UnityEditor;
using UnityEngine;

namespace OutOfOffice.EditorTools
{
    public static partial class PlaceholderContentBuilder
    {
        const string SfxDir = Root + "/Audio/Sfx";
        const string SfxLibraryPath = Root + "/Content/SfxLibrary.asset";

        // Ses efektleri. Klipleri Tools/Audio/make_sfx.py sentezler; gerçek seslere geçerken aynı
        // adla dosyayı değiştirmek ya da SfxLibrary asset'indeki klip alanını değiştirmek yeterlidir.
        static void BuildSfx()
        {
            var entries = new List<SfxEntry>();
            AddSfx(entries, SfxEvent.Jumped, "sfx_jump", 0.5f, 0.05f);
            AddSfx(entries, SfxEvent.Slid, "sfx_slide", 0.45f, 0.05f);
            AddSfx(entries, SfxEvent.LaneChanged, "sfx_lane", 0.3f, 0.08f);
            AddSfx(entries, SfxEvent.CoinCollected, "sfx_coin", 0.5f, 0f);
            AddSfx(entries, SfxEvent.PowerUpCollected, "sfx_powerup", 0.6f, 0f);
            AddSfx(entries, SfxEvent.Stumbled, "sfx_stumble", 0.8f, 0.04f);
            AddSfx(entries, SfxEvent.ShieldBroken, "sfx_shield_break", 0.6f, 0.03f);
            AddSfx(entries, SfxEvent.Caught, "sfx_caught", 0.7f, 0f);
            AddSfx(entries, SfxEvent.UiClicked, "sfx_click", 0.5f, 0f);

            var library = LoadOrCreate<SfxLibrary>(SfxLibraryPath);
            library.clips = entries.ToArray();
            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();
        }

        static void AddSfx(List<SfxEntry> entries, SfxEvent trigger, string file, float volume, float pitchVariation)
        {
            string path = $"{SfxDir}/{file}.wav";
            if (!File.Exists(path))
            {
                Debug.LogWarning($"Ses dosyası yok, atlandı: {path} (Tools/Audio/make_sfx.py üretir)");
                return;
            }

            ConfigureSfxClip(path);
            entries.Add(new SfxEntry
            {
                trigger = trigger,
                clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path),
                volume = volume,
                pitchVariation = pitchVariation,
            });
        }

        /// <summary>Kısa efekt ayarı: tek kanal, bellekte açık (gecikmesiz çalar), ADPCM (küçük ve ucuz).</summary>
        static void ConfigureSfxClip(string path)
        {
            var importer = (AudioImporter)AssetImporter.GetAtPath(path);
            var settings = importer.defaultSampleSettings;

            bool upToDate = importer.forceToMono
                            && !importer.loadInBackground
                            && settings.loadType == AudioClipLoadType.DecompressOnLoad
                            && settings.compressionFormat == AudioCompressionFormat.ADPCM
                            && settings.preloadAudioData;
            if (upToDate) return;

            importer.forceToMono = true;
            importer.loadInBackground = false;
            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.ADPCM;
            settings.preloadAudioData = true;
            importer.defaultSampleSettings = settings;
            importer.SaveAndReimport();
        }
    }
}
