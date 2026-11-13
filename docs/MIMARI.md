# Mimari ve kod yapısı

Amaç: yeni karakter, engel, güçlendirme ve dünya neredeyse hiç kod yazmadan eklenebilsin; yeni bir davranış gerektiğinde var olan kod değişmeden tek sınıf eklensin (SOLID, özellikle açık/kapalı ve bağımlılığın tersine çevrilmesi).

## Klasörler

```
Assets/_Game/
├─ Core/              saf C# sözleşmeleri, Unity sahnesinden bağımsız
│  ├─ Signals/        ISignalBus, SignalBus, SubscriptionBag, GameSignals.cs (tüm sinyaller)
│  ├─ State/          GameState, StateMachine<T>, IGameStateProvider
│  ├─ Services/       ISaveService, ILocalizationService, IPoolService
│  └─ Run/            IPlayerStatus, IChaser, IObstacleField, EffectContext, RewardKind...
├─ Data/
│  ├─ Definitions/    GameConfig, CharacterDefinition, ObstacleDefinition, PowerUpDefinition,
│  │                  AbilityDefinition, AccessoryDefinition, ChaserDefinition, WorldDefinition,
│  │                  ContentCatalog, LootTable, StoreItemDefinition, VfxLibrary, EffectDefinition
│  ├─ Effects/        EffectDefinition türevleri (Shield, Flight, Magnet, GhostObstacles...)
│  └─ Movement/       MovementPattern türevleri (SideSweep, Approach)
├─ Gameplay/
│  ├─ Flow/           GameFlow, RunSystem, RunSession, PlayerProfile, CharacterSelection,
│  │                  Workforce, AccessorySystem, RewardSystem, StoreSystem
│  ├─ Player/         PlayerSystem, PlayerStatus, PlayerRig, CollisionSystem, CameraFollowSystem
│  ├─ Chaser/         ChaserSystem
│  ├─ World/          TrackSystem, SpawnDirector, AdaptiveDirector, ObstacleField, PickupField,
│  │                  WorldCurveSystem, PoolWarmupSystem, ObstacleView, PickupView
│  └─ Effects/        EffectRunner, AbilitySystem, TimeFeelSystem, VfxSystem
├─ Infrastructure/    PlayerPrefsSaveService, PoolService, InputReader,
│                     UnityLocalizationService, FrameRateService
├─ UI/                sunucular (*Presenter), ScreenRouter, UiText, RadialProgress, SafeArea
│  ├─ Screens/        GameUI.uxml, GameUI.uss
│  ├─ Theme/          GameTheme.tss, GamePanelSettings.asset
│  └─ Fonts/ Icons/ Sprites/
├─ Bootstrap/         GameLifetimeScope.cs (composition root)
├─ Shaders/           CurvedLit.shader + CurvedWorld.hlsl (kıvrımlı dünya)
├─ Editor/            PlaceholderContentBuilder (parçalı sınıf), CharacterModelBuilder, PerfProbe
├─ Content/           üretilen tanım asset'leri (GameConfig, ContentCatalog, Characters/...)
├─ Art/Placeholder/   üretilen materyal ve mesh'ler
├─ Art/Characters/    gerçek karakter modelleri: <Ad>/<Ad>.fbx + doku + materyal, Animations/ (klipler, denetleyici)
├─ Prefabs/Placeholder/  üretilen prefab'lar
├─ Prefabs/Characters/   CharacterModelBuilder'ın ürettiği gerçek karakter prefab'ları
├─ Localization/      çeviri ayarları ve "Game" tablosu
├─ Scenes/Game.unity  üretilen tek sahne
└─ link.xml           IL2CPP'de oyun assembly'lerini budamadan korur
```

## Assembly bağımlılıkları

| Assembly | Bağlı olduğu |
|---|---|
| `OutOfOffice.Core` | hiçbir şey |
| `OutOfOffice.Data` | Core |
| `OutOfOffice.Gameplay` | Core, Data, VContainer |
| `OutOfOffice.Infrastructure` | Core, VContainer, Localization, InputSystem |
| `OutOfOffice.UI` | Core, VContainer, Localization |
| `OutOfOffice.Bootstrap` | hepsi |
| `OutOfOffice.Editor` | Core, Data, UI, Bootstrap, URP |

UI'ın Data ve Gameplay'e, Gameplay'in UI ve Infrastructure'a referansı **yoktur ve eklenmez**. UI'ın göstereceği her şey sinyalin içinde taşınır (ör. `CharacterCardSignal` adı, rengi, kilit durumunu taşır).

## Sinyal veri yolu

```csharp
public interface ISignalBus
{
    void Fire<T>(in T signal) where T : struct;
    void Fire<T>() where T : struct;                       // veri taşımayanlar için
    IDisposable Subscribe<T>(Action<T> handler) where T : struct;
}
```

- Tüm sinyaller `Core/Signals/GameSignals.cs` içindedir, `readonly struct`'tır.
- Üç tür: **istek** (`StartRunRequestedSignal`, `StorePurchaseRequestedSignal`), **olay** (`PlayerJumpedSignal`, `RunEndedSignal`), **durum yayını** (`ProfileChangedSignal`, `AccessoryChangedSignal`: arayüzün göstereceği güncel değer).
- UI istek atar, oynanış karar verir, sonucu olay ya da durum yayınıyla duyurur. UI hiçbir kuralı bilmez.

Abonelik kalıbı:

```csharp
readonly SubscriptionBag _subscriptions = new SubscriptionBag();

public MySystem(ISignalBus bus)
{
    bus.Subscribe<PlayerStumbledSignal>(_ => OnStumble()).AddTo(_subscriptions);
}

public void Dispose() { _subscriptions.Dispose(); }
```

## Oyun durumu

`GameState` geçişleri: `Boot → Menu`, `Menu → Running`, `Running → Paused | GameOver`, `Paused → Running | Menu`, `GameOver → Running | Menu`. Geçişleri yalnızca `GameFlow` yapar (`StateMachine<GameState>`); geçersiz istek sessizce yok sayılır. Durumu okumak için `IGameStateProvider`, değişimi dinlemek için `GameStateChangedSignal`.

Koşu yaşam döngüsü sinyalleri: `RunStartedSignal` → (`ReviveOfferedSignal` → `RunRevivedSignal` | `ReviveDeclinedSignal`) → `RunEndedSignal` → `RunClearedSignal` (menüye dönünce sahne temizliği).

## Bağımlılık enjeksiyonu (VContainer)

`GameLifetimeScope` somut sınıfları bilen tek yerdir. Sistemler düz C# sınıfıdır; bağımlılıkları kurucudan alır.

Sıra kuralları (karışıklığı bunlar önler):

1. **Kurucu:** sinyallere abone ol. Başka sisteme çağrı yapma.
2. **`IStartable.Start`:** arayüz sunucuları UXML elemanlarını burada bağlar.
3. **`IPostStartable.PostStart`:** ilk durum yayınları burada yapılır (herkes abone olduktan sonra). `GameFlow` menüye burada geçer.
4. **`ITickable.Tick` / `ILateTickable.LateTick`:** kare başı iş. Bir sistem diğerinden sonra çalışmalıysa `GameLifetimeScope`'ta ondan sonra kaydet ve yorumla belirt (ör. `WorldCurveSystem` kameradan sonra).

Kayıt:

```csharp
builder.RegisterEntryPoint<MySystem>();            // Tick/Start/Dispose arayüzleri olan sistem
builder.RegisterEntryPoint<MySystem>().AsSelf();   // ayrıca somut tipiyle de istenecekse
RegisterAlwaysOn<MyListener>(builder);             // yalnızca sinyal dinleyen, Tick'i olmayan
builder.Register<Impl>(Lifetime.Singleton).As<IService>();   // servis
```

## Paylaşılan koşu durumu

- `RunSession`: koşunun canlı verisi (hız, mesafe, zorluk, skor, altın, paket, devam sayısı). Yalnızca `RunSystem` hesaplar, diğerleri okur.
- `PlayerStatus` (`IPlayerStatus`): konum, şerit, kalkan, zıplama çarpanı, mıknatıs yarıçapı, puan çarpanı, uçuş yüksekliği. Etkiler bunun üzerinden çalışır.
- `PlayerProfile`: kalıcı değerler (rekor, toplam altın, giriş kartı).

## Kayıt

`ISaveService` (şimdilik PlayerPrefs, önek `ooo.`). Anahtarlar: `best_score`, `total_coins`, `keys`, `character`, `automated` (virgüllü kimlik listesi), `automated_count.<id>`, `accessory.<id>`. Yeni kalıcı değer eklerken anahtarı bu listeye de yaz. Bulut kaydına geçmek için yalnızca yeni bir `ISaveService` yazılır.

## Çeviri

- Tek tablo: `Game`. Satırlar `PlaceholderContentBuilder.Localization.cs` içindeki `Texts` dizisindedir (anahtar, Türkçe, İngilizce).
- UXML'de `text="#ui.play"` yazılır; `UiText.LocalizeTree` bunu `LocalizedString` bağına çevirir, dil değişince kendiliğinden güncellenir.
- Koddan metin: `label.Localize("anahtar")`. Kendi "geçerli dil" değişkenini tutma; dili `LocalizationSettings.SelectedLocale` yönetir.
- Tanım asset'leri metin değil anahtar taşır (`nameKey`).
- Yeni dil: Locale ekle, `Texts` satırlarına sütun ekle, `BuildLocalization`'daki sütun eşlemesini genişlet.

## Kıvrımlı dünya

- `Shaders/CurvedWorld.hlsl`: `BendWorldPosition`, köşe noktasını kameradan uzaklığın karesiyle yana ve aşağı kaydırır.
- `Shaders/CurvedLit.shader` ("Out of Office/Curved Lit"): URP PBR aydınlatması + bükme; ileri, gölge, derinlik ve derinlik-normal geçişlerinin hepsi aynı bükmeyi uygular.
- `WorldCurveSystem`: kıvrımı koşu boyunca değiştirir, global shader değerlerini yazar, bükülen nesneler erken elenmesin diye kameranın eleme alanını genişletir.
- Yeni bir shader (ör. saydam yüzey, parçacık) dünyada kullanılacaksa `CurvedWorld.hlsl`'i dahil edip aynı işlevi çağırmalıdır. Parçacıklar şu an bükülmüyor.

## Karakter modelleri ve animasyon

Üç meslek (Yazılımcı, Grafiker, Stajyer) Meshy ile üretilmiş, iskeletli gerçek modellerdir. Kovalayan, engeller ve diğer her şey hâlâ geçici küptür.

**Dosyalar**

```
Art/Characters/<Ad>/<Ad>.fbx            iskeletli model (24 kemik, ~8.300 üçgen, tek materyal)
Art/Characters/<Ad>/<Ad>_BaseColor.png  1024 doku (mobilde ASTC 6x6)
Art/Characters/<Ad>/<Ad>.mat            Curved Lit materyali (araç üretir)
Art/Characters/Animations/Run.fbx       koşu klibi
Art/Characters/Animations/Actions.fbx   bekleme, zıplama, kayma klipleri
Art/Characters/Animations/Character.controller   ortak Animator denetleyicisi (araç üretir)
Prefabs/Characters/Character_<Ad>.prefab          oyunun kullandığı prefab (araç üretir)
```

**Kurulum aracı:** `CharacterModelBuilder` (menü: Out of Office > Karakter Modellerini Kur). Tekrar çalıştırmak güvenlidir. Yaptıkları:

- Model: Humanoid iskelet, kemik Transform'ları sahneye çıkmaz (`optimizeGameObjects`), mesh sıkıştırma, köşe başına 2 kemik, teğet yok, okunamaz mesh. Boy `Characters` tablosundaki değere ölçeklenir.
- Doku: en fazla 1024, mipmap'li, Android ve iOS'ta ASTC 6x6.
- Klipler: `Clips` tablosundaki kare aralıkları ve kök hareketi ayarlarıyla kesilir. Klip dosyaları T pozunda açılmadığı için duruş pozu Yazılımcı modelinden kopyalanır (`CopyRestPose`); yoksa bütün klipler kayık oynar.
- Prefab: boş kök + `Model` çocuğu (Animator, avatar, denetleyici).

Klipler Humanoid'dir; **tek klip seti üç karakterde de oynar**. Yeni karakter için klip üretmek gerekmez.

**Animator sözleşmesi.** Parametreleri yalnızca `PlayerRig` yazar; `PlayerSystem` ve diğer sistemler Animator'ı bilmez.

| Parametre | Tür | Anlamı |
|---|---|---|
| `Moving` | bool | Koşuyor mu (Bekleme ↔ Koşu) |
| `Grounded` | bool | Yerde mi; değilse Zıplama durumuna geçilir (varsayılan: doğru) |
| `Sliding` | bool | Kayıyor mu |
| `RunRate` | float | Koşu klibinin oynatma hızı (koşu hızına göre, `GameConfig` sınırlarıyla) |
| `AirPhase` | float | Zıplama klibinin konumu: kalkış 0, tepe 0,5, iniş 1 |

Zıplama klibi süreyle değil `AirPhase` ile ilerler; değer dikey hızdan hesaplanır. Bu yüzden zıplama çarpanı, hızlı iniş ve uçuş (tepe pozunda süzülür) ayrıca ele alınmaz. Zıplama klibinde dikey kök hareketi pozdan çıkarılmıştır: yüksekliği oynanış verir.

Prefab'da Animator yoksa (geçici küp modeller) `PlayerRig` eski elle esnetme yoluna düşer; iki yol aynı `Animate` çağrısını kullanır.

**Kaynak (Meshy görev kimlikleri, yeniden indirme ya da yeni klip için):** rig görevleri Yazılımcı `01a1d695-5eaa-72b3-89bd-e34ed2e843b5`, Grafiker `01a1d695-6d47-7278-8789-c43b52745ff5`, Stajyer `01a1d695-7ae6-7453-ac6e-24c7350d7fec`. Klipler Yazılımcı rig'inden: koşu rig'le birlikte gelir; bekleme (0), zıplama (466) ve kayma (516) tek animasyon görevindedir.

## Performans kuralları

- Koşu döngüsünde bellek ayırma yok; listeler alan (field) olarak tutulup yeniden kullanılır.
- Engel, altın, zemin, efekt havuzdan gelir; yükleme anında `PoolWarmupSystem` ön üretir.
- Aynı materyali kullanan parçalar prefab içinde tek mesh'te birleştirilir (`CombineByMaterial`).
- `Shader.PropertyToID` değerleri `static readonly` alanlarda saklanır.
- Kare hızı `FrameRateService` ile cihazın yenileme hızına ayarlanır (60-120).

## Tarifler

Geçici içerik döneminde tanım asset'lerini `PlaceholderContentBuilder.Data.cs` üretir; aşağıdaki "asset" adımları şimdilik oraya bir satır eklemek demektir. Sonra **Out of Office > Placeholder İçeriği Yeniden Oluştur**.

**Yeni meslek (karakter)**
1. Prefab (yalnızca görsel; çarpışma koddan gelir). Gerçek model için: Humanoid'e uygun, T pozunda iskeletli FBX'i ve dokusunu `Art/Characters/<Ad>/` altına koy, `CharacterModelBuilder.Characters` tablosuna ad ve boy ekle, aracı çalıştır. Üreticide o karakterin satırını `CharacterModelBuilder.PrefabOr(...)` ile sar.
2. `CharacterDefinition`: `id`, `nameKey`, `themeColor`, `prefab`, `ability` (boş olabilir), otomasyon alanları, `automatedObstacle`.
3. `ContentCatalog.characters` listesine ekle.
4. Çeviri satırı (`char.<id>`). Kod yok.

**Yeni engel / düşman**
1. Prefab. Collider yoksa görsel sınırlarına göre kendiliğinden eklenir.
2. `ObstacleDefinition`: `avoidBy` (ChangeLane / Jump / Slide), `movement` (boşsa sabit), `minDifficulty`, `spawnWeight`.
3. `WorldDefinition.obstacles` listesine ekle. Kod yok.

**Yeni güçlendirme**
1. Toplanacak nesnenin prefab'ı.
2. `PowerUpDefinition`: `duration`, `spawnWeight`, `effects` (var olan etkileri birleştir).
3. `WorldDefinition.powerUps` listesine ekle, çeviri satırı (`powerup.<id>`). Kod yok.

**Yeni karakter animasyonu:** klibi (aynı iskeletle) `Art/Characters/Animations/` altına koy, `CharacterModelBuilder.Clips` tablosuna satır ekle, `BuildController`'da durum ve geçişini tanımla. Yeni parametre gerekiyorsa onu yalnızca `PlayerRig` yazsın; değerini hesaplamak için gereken bilgi `Animate` çağrısına eklenir.

**Yeni yetenek ya da aksesuar:** `AbilityDefinition` (bekleme, süre, etkiler) ya da `AccessoryDefinition` (fiyat, süre, etkiler). Yine var olan etkilerin birleşimi.

**Yeni etki türü (tek sınıf)**

```csharp
[CreateAssetMenu(menuName = "Out of Office/Effects/Magnet", fileName = "Effect_Magnet")]
public sealed class MagnetEffect : EffectDefinition
{
    [Min(0f)] public float radius = 7f;

    public override void Apply(EffectContext context) { context.Player.MagnetRadius = radius; }
    public override void Remove(EffectContext context) { context.Player.MagnetRadius = 0f; }
}
```

Etki yalnızca `EffectContext` üzerinden çalışır (oyuncu, kovalayan, engel alanı, sinyaller). Yeni bir yetkiye ihtiyaç varsa önce Core'daki arayüze üye ekle, sonra uygulayan sisteme.

**Yeni hareket deseni:** `MovementPattern`'dan türet, `Evaluate(origin, age, laneWidth, halfTrackWidth)` konumu döndürsün. Oyuncuya doğru gelen desenler `LeadDistance`'ı da ezer.

**Yeni sistem:** Gameplay'de düz sınıf, kurucuda abonelik, gerekiyorsa `ITickable`. `GameLifetimeScope`'a tek satır. Ayar sayıları `GameConfig`'e.

**Yeni ekran ya da arayüz parçası:** UXML'e eleman (ad camelCase, metin `#anahtar`), USS'e sınıf (`docs/TASARIM.md`), UI'da bir `*Presenter` (`IStartable`'da bağla, sinyal dinle, istek sinyali at), `GameLifetimeScope`'a kayıt. Yeni veri gerekiyorsa önce Core'a sinyal ekle.

**Yeni mağaza ürünü:** `StoreItemDefinition` (`kind`, `amount`, `price`) ve `ContentCatalog.storeItems`. Yeni ödül türü gerekiyorsa `RewardKind`'a değer ve `RewardSystem.Grant`'a dal ekle.

**Yeni dünya:** zemin prefab'ı (Z boyunca bölünmüş), `WorldDefinition`, `ContentCatalog.worlds`. Dünyalar arası geçiş henüz yazılmadı.

## Editör araçları

- `PlaceholderContentBuilder` (parçalı): `.cs` (ortak yardımcılar, `Mat`, `Part`, `SlicedPart`, `CombineByMaterial`), `.Prefabs`, `.Data`, `.Vfx`, `.Localization`, `.Scene`.
- `CharacterModelBuilder`: gerçek karakter modellerini kurar (yukarıda "Karakter modelleri ve animasyon"). Çalıştırdıktan sonra tanım asset'leri yeni prefab'ı görsün diye içerik üreticiyi de çalıştır.
- **Out of Office > Oyun Ayarlarını Varsayılana Döndür:** `GameConfig`'i koddaki değerlere çeker (elle yapılan dengeleme silinir).
- `PerfProbe`: Play modunda kare başına bellek ayırma, çizim çağrısı ve kare süresini örnekler (`PerfProbe.Start()`, sonra `PerfProbe.Report()`). Önce/sonra karşılaştırması içindir; editör değerleri cihazdakinden farklıdır.
