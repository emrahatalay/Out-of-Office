using OutOfOffice.Data;

namespace OutOfOffice.Gameplay
{
    /// <summary>Tek bir koşunun canlı verisi. Sistemler buradan okur; hesaplamayı RunSystem yapar.</summary>
    public sealed class RunSession
    {
        public WorldDefinition World;
        public CharacterDefinition Character;

        public float Speed;
        public float Distance;
        public float Elapsed;
        /// <summary>0: koşunun başı, 1: en zor.</summary>
        public float Difficulty;
        public int Coins;
        public int Score;

        public void Begin(WorldDefinition world, CharacterDefinition character, float startSpeed)
        {
            World = world;
            Character = character;
            Speed = startSpeed;
            Distance = 0f;
            Elapsed = 0f;
            Difficulty = 0f;
            Coins = 0;
            Score = 0;
        }
    }
}
