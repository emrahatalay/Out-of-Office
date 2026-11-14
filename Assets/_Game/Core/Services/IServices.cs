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
