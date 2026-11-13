using OutOfOffice.Core.Run;
using OutOfOffice.Core.Services;
using OutOfOffice.Core.Signals;
using OutOfOffice.Data;
using OutOfOffice.Gameplay;
using OutOfOffice.Infrastructure;
using OutOfOffice.UI;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using VContainer.Unity;

namespace OutOfOffice.Bootstrap
{
    /// <summary>
    /// Composition root: somut sınıfları bilen TEK yer. Yeni bir sistem eklemek ya da bir
    /// servisi (kayıt, reklam, analitik) başkasıyla değiştirmek burada tek satırdır.
    /// </summary>
    public sealed class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] GameConfig config;
        [SerializeField] ContentCatalog catalog;
        [SerializeField] VfxLibrary vfxLibrary;
        [SerializeField] Camera gameCamera;
        [SerializeField] UIDocument uiDocument;

        protected override void Configure(IContainerBuilder builder)
        {
            // Veri ve sahne nesneleri
            builder.RegisterInstance(config);
            builder.RegisterInstance(catalog);
            builder.RegisterInstance(vfxLibrary);
            builder.RegisterInstance(gameCamera);
            builder.RegisterInstance(uiDocument);

            // Altyapı (hepsi arayüz arkasında)
            builder.Register<SignalBus>(Lifetime.Singleton).As<ISignalBus>();
            builder.Register<PlayerPrefsSaveService>(Lifetime.Singleton).As<ISaveService>();
            builder.Register<PoolService>(Lifetime.Singleton).As<IPoolService>();
            builder.RegisterEntryPoint<UnityLocalizationService>();
            builder.RegisterEntryPoint<InputReader>();
            builder.RegisterEntryPoint<FrameRateService>();

            // Koşunun paylaşılan durumu
            builder.Register<RunSession>(Lifetime.Singleton);
            builder.Register<PlayerStatus>(Lifetime.Singleton).AsSelf().As<IPlayerStatus>();
            builder.Register<PlayerRig>(Lifetime.Singleton);
            builder.Register<EffectContext>(Lifetime.Singleton);
            builder.RegisterEntryPoint<PlayerProfile>().AsSelf();
            builder.Register<Workforce>(Lifetime.Singleton);

            // Oynanış sistemleri
            builder.RegisterEntryPoint<GameFlow>();
            builder.RegisterEntryPoint<CharacterSelection>().AsSelf();
            builder.RegisterEntryPoint<RunSystem>();
            builder.RegisterEntryPoint<PlayerSystem>();
            builder.RegisterEntryPoint<ChaserSystem>();
            builder.RegisterEntryPoint<ObstacleField>().AsSelf();
            builder.RegisterEntryPoint<PickupField>().AsSelf();
            builder.RegisterEntryPoint<TrackSystem>();
            builder.RegisterEntryPoint<AdaptiveDirector>();
            builder.RegisterEntryPoint<SpawnDirector>();
            builder.RegisterEntryPoint<EffectRunner>().AsSelf();
            builder.RegisterEntryPoint<CameraFollowSystem>();
            builder.RegisterEntryPoint<PoolWarmupSystem>();

            // His ve efektler (sadece sinyal dinler; çıkarılsalar oyun aynen çalışır)
            builder.RegisterEntryPoint<TimeFeelSystem>();
            builder.RegisterEntryPoint<VfxSystem>();
            RegisterAlwaysOn<AbilitySystem>(builder);
            RegisterAlwaysOn<CollisionSystem>(builder);

            // Arayüz
            builder.RegisterEntryPoint<ScreenRouter>();
            builder.RegisterEntryPoint<MenuPresenter>();
            builder.RegisterEntryPoint<HudPresenter>();
            builder.RegisterEntryPoint<PausePresenter>();
            builder.RegisterEntryPoint<GameOverPresenter>();
        }

        /// <summary>Yalnızca sinyal dinleyen (Tick'i olmayan) sistemler için: kimse istemese de kurulur.</summary>
        static void RegisterAlwaysOn<T>(IContainerBuilder builder) where T : class
        {
            builder.Register<T>(Lifetime.Singleton);
            builder.RegisterBuildCallback(container => container.Resolve<T>());
        }
    }
}
