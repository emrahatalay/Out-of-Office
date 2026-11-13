namespace OutOfOffice.Core.State
{
    public enum GameState
    {
        Boot,
        Menu,
        Running,
        Paused,
        GameOver
    }

    /// <summary>Durumu okumak isteyen sistemler bunu kullanır; değiştirmek için istek sinyali atılır.</summary>
    public interface IGameStateProvider
    {
        GameState Current { get; }
    }
}
