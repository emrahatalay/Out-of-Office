using System.IO;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace OutOfOffice.EditorTools
{
    /// <summary>
    /// Android cihazda deneme çıktısı. Unity 6 build profili kullanır: profil yalnızca oyun
    /// sahnesini içerir (şablondan kalan örnek sahne çıktıya girmez); mimari ve betik arka ucu
    /// proje ayarlarından gelir (IL2CPP + ARM64). Çıktı imzasızdır (hata ayıklama anahtarı):
    /// cihaza elle kurulur, mağazaya yüklenemez.
    ///
    /// Çıktı Builds/Android altına yazılır (depoda izlenmez). Kurulum:
    ///   adb install -r "Builds/Android/SonMesai.apk"
    /// </summary>
    public static class AndroidBuild
    {
        const string ProfileName = "Android Test";
        const string ProfilePath = "Assets/Settings/Build Profiles/" + ProfileName + ".asset";
        const string ScenePath = "Assets/_Game/Scenes/Game.unity";
        const string OutputPath = "Builds/Android/SonMesai.apk";

        [MenuItem("Out of Office/Android Test Profilini Oluştur")]
        public static void CreateProfileFromMenu()
        {
            Debug.Log(CreateProfile());
        }

        [MenuItem("Out of Office/Android Test Çıktısı Al (APK)")]
        public static void BuildFromMenu()
        {
            Debug.Log(Build());
        }

        /// <summary>Profil yoksa oluşturur, varsa sahne listesini yeniler. Tekrar çalıştırmak güvenlidir.</summary>
        public static string CreateProfile()
        {
            var existing = BuildProfile.GetBuildProfileAtPath(ProfilePath);
            if (existing != null)
            {
                Configure(existing);
                return "Build profili hazır: " + ProfilePath;
            }

            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
                return "Android Build Support modülü kurulu değil (Unity Hub > Installs > Add modules).";

            GUID platform = default;
            foreach (var module in BuildProfile.GetInstalledPlatformModules())
            {
                if (module.displayName.StartsWith("Android")) platform = module.platformGuid;
            }

            if (platform.Empty()) return "Android platformu bulunamadı.";

            // Profil asset'i hazır olunca (gerekirse paketleri kurduktan sonra) sahne listesi yazılır.
            BuildProfile.CreateBuildProfile(platform, ProfileName, Configure);
            return "Build profili oluşturuluyor: " + ProfilePath;
        }

        public static string Build()
        {
            var profile = BuildProfile.GetBuildProfileAtPath(ProfilePath);
            if (profile == null) return "Önce profili oluştur: Out of Office > Android Test Profilini Oluştur.";
            if (EditorApplication.isPlayingOrWillChangePlaymode) return "Play modundayken çıktı alınmaz.";

            Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerWithProfileOptions
            {
                buildProfile = profile,
                locationPathName = OutputPath,
                options = BuildOptions.None,
            });

            // Boyut dosyadan okunur: rapordaki toplam, APK'ya sıkıştırılmadan önceki içeriğin boyutudur.
            var summary = report.summary;
            float megabytes = File.Exists(OutputPath) ? new FileInfo(OutputPath).Length / (1024f * 1024f) : 0f;
            return $"Android çıktısı: {summary.result}. Süre: {summary.totalTime.TotalMinutes:0.0} dk, APK: {megabytes:0.0} MB, " +
                   $"hata: {summary.totalErrors}, uyarı: {summary.totalWarnings}. Dosya: {OutputPath}";
        }

        static void Configure(BuildProfile profile)
        {
            profile.overrideGlobalScenes = true;
            profile.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssetIfDirty(profile);
        }
    }
}
