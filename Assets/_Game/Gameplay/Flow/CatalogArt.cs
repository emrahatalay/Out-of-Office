using OutOfOffice.Core.Services;
using OutOfOffice.Data;
using UnityEngine;

namespace OutOfOffice.Gameplay
{
    /// <summary>Arayüze içerik görsellerini verir; görseller tanım asset'lerinde durur.</summary>
    public sealed class CatalogArt : IArtProvider
    {
        readonly ContentCatalog _catalog;

        public CatalogArt(ContentCatalog catalog)
        {
            _catalog = catalog;
        }

        public Texture2D GetCharacterPortrait(string characterId)
        {
            var characters = _catalog.characters;
            for (int i = 0; i < characters.Length; i++)
            {
                if (characters[i] != null && characters[i].id == characterId) return characters[i].portrait;
            }

            return null;
        }

        public Texture2D GetStoreIcon(string itemId)
        {
            var items = _catalog.storeItems;
            if (items == null) return null;

            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] != null && items[i].id == itemId) return items[i].icon;
            }

            return null;
        }
    }
}
