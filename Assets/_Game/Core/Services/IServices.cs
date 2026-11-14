using UnityEngine;

namespace OutOfOffice.Core.Services
{
    /// <summary>Kalıcı anahtar-değer kaydı. Arkası PlayerPrefs, dosya ya da bulut olabilir.</summary>
    public interface ISaveService
    {
        int GetInt(string key, int fallback = 0);
        void SetInt(string key, int value);
        string GetString(string key, string fallback = "");
        void SetString(string key, string value);
        void Flush();
    }

    /// <summary>Ayarlar sekmesindeki görüntü kalitesi seçenekleri. Sayısal karşılıkları GameConfig'tedir.</summary>
    public enum GraphicsQuality
    {
        Low,
        Medium,
        High
    }

    /// <summary>Görüntü kalitesini motora uygular. Arkası URP; oynanış hangi boru hattı olduğunu bilmez.</summary>
    public interface IRenderQuality
    {
        /// <param name="renderScale">Sahnenin çizildiği çözünürlük oranı (1 = ekran).</param>
        /// <param name="postProcessing">Renk ayarı, bloom gibi son işlemler açık mı.</param>
        void Apply(float renderScale, bool postProcessing);
    }

    /// <summary>Koddan üretilen metinler için çeviri. Sabit etiketler LocalizeStringEvent kullanır.</summary>
    public interface ILocalizationService
    {
        string CurrentCode { get; }
        string Get(string key);
    }

    /// <summary>
    /// Arayüzün göstereceği içerik görselleri. Arayüz veri katmanını tanımaz ve sinyaller nesne
    /// taşımaz; bu yüzden görsel, sinyaldeki kimlikle buradan sorulur.
    /// </summary>
    public interface IArtProvider
    {
        /// <summary>Kimliği verilen mesleğin portresi; tanımlı değilse null.</summary>
        Texture2D GetCharacterPortrait(string characterId);

        /// <summary>Kimliği verilen mağaza ürününün görseli; tanımlı değilse null.</summary>
        Texture2D GetStoreIcon(string itemId);
    }

    /// <summary>Sık üretilip yok edilen nesneler (engel, altın, zemin) buradan alınır.</summary>
    public interface IPoolService
    {
        GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation);
        void Despawn(GameObject instance);

        /// <summary>Havuzda en az bu kadar hazır kopya olmasını sağlar.</summary>
        void Prewarm(GameObject prefab, int count);
    }
}
