using UnityEngine.Localization;
using UnityEngine.UIElements;

namespace OutOfOffice.UI
{
    /// <summary>
    /// Arayüz metinlerini Unity Localization'a bağlar. Dil değişince metinler paket
    /// tarafından kendiliğinden güncellenir; arayüz kodu dil durumu tutmaz.
    /// </summary>
    public static class UiText
    {
        public const string Table = "Game";
        const char KeyMarker = '#';

        /// <summary>Elemanın metnini çeviri anahtarına bağlar. Boş anahtar metni temizler.</summary>
        public static void Localize(this TextElement element, string key)
        {
            if (element == null) return;

            if (string.IsNullOrEmpty(key))
            {
                element.ClearBinding("text");
                element.text = string.Empty;
                return;
            }

            element.SetBinding("text", new LocalizedString(Table, key));
        }

        /// <summary>
        /// UXML'de metni '#' ile başlayan her eleman çeviriye bağlanır:
        /// text="#ui.play" yazmak yeterlidir, kod gerekmez.
        /// </summary>
        public static void LocalizeTree(VisualElement root)
        {
            root.Query<TextElement>().ForEach(element =>
            {
                string text = element.text;
                if (!string.IsNullOrEmpty(text) && text[0] == KeyMarker) element.Localize(text.Substring(1));
            });
        }
    }
}
