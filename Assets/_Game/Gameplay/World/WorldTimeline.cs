using OutOfOffice.Data;

namespace OutOfOffice.Gameplay
{
    /// <summary>
    /// Dünyaların koşu boyunca sırası: hangi dünya hangi metreden başlar. Zemin, engel dizilimi ve hava
    /// "şu mesafede hangi dünya" sorusunu buradan sorar. Katalogdaki liste startDistance'a göre artan olmalı.
    /// </summary>
    public sealed class WorldTimeline
    {
        readonly WorldDefinition[] _worlds;

        public WorldTimeline(ContentCatalog catalog)
        {
            _worlds = catalog.worlds ?? new WorldDefinition[0];
        }

        public int Count => _worlds.Length;

        public WorldDefinition Get(int index)
        {
            return _worlds[index < 0 ? 0 : index >= _worlds.Length ? _worlds.Length - 1 : index];
        }

        /// <summary>Verilen mesafedeki (z) dünyanın sırası.</summary>
        public int IndexAt(float distance)
        {
            int index = 0;
            for (int i = 1; i < _worlds.Length; i++)
            {
                if (distance >= _worlds[i].startDistance) index = i;
                else break;
            }

            return index;
        }

        public WorldDefinition WorldAt(float distance)
        {
            return _worlds.Length == 0 ? null : _worlds[IndexAt(distance)];
        }
    }
}
