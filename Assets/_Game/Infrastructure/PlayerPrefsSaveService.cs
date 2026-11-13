using OutOfOffice.Core.Services;
using UnityEngine;

namespace OutOfOffice.Infrastructure
{
    /// <summary>ISaveService'in PlayerPrefs karşılığı. Bulut kaydına geçmek için yalnızca bu sınıf değişir.</summary>
    public sealed class PlayerPrefsSaveService : ISaveService
    {
        const string Prefix = "ooo.";

        public int GetInt(string key, int fallback = 0) { return PlayerPrefs.GetInt(Prefix + key, fallback); }
        public void SetInt(string key, int value) { PlayerPrefs.SetInt(Prefix + key, value); }
        public string GetString(string key, string fallback = "") { return PlayerPrefs.GetString(Prefix + key, fallback); }
        public void SetString(string key, string value) { PlayerPrefs.SetString(Prefix + key, value); }
        public void Flush() { PlayerPrefs.Save(); }
    }
}
