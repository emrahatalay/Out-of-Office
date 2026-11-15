# Mimari ve kod yapısı

Amaç: yeni karakter, engel, güçlendirme ve dünya neredeyse hiç kod yazmadan eklenebilsin; yeni bir davranış gerektiğinde var olan kod değişmeden tek sınıf eklensin (SOLID, özellikle açık/kapalı ve bağımlılığın tersine çevrilmesi).

## Klasörler

```
Assets/_Game/
├─ Core/              saf C# sözleşmeleri, Unity sahnesinden bağımsız
│  ├─ Signals/        ISignalBus, SignalBus, SubscriptionBag, GameSignals.cs (tüm sinyaller)
│  ├─ State/          GameState, StateMachine<T>, IGameStateProvider
│  ├─ Services/       ISaveService, ILocalizationService, IPoolService, IArtProvider,
│  │                  IRenderQuality, GraphicsQuality
│  └─ Run/            IPlayerStatus, IChaser, IObstacleField, EffectContext, RewardKind...
├─ Data/
│  ├─ Definitions/    GameConfig, CharacterDefinition, ObstacleDefinition, PowerUpDefinition,
│  │                  AbilityDefinition, AccessoryDefinition, ChaserDefinition, WorldDefinition,
│  │                  ContentCatalog, LootTable, StoreItemDefinition, VfxLibrary, SfxLibrary,
│  │                  EffectDefinition
│  ├─ Effects/        EffectDefinition türevleri (Shield, Flight, Magnet, GhostObstacles...)
│  └─ Movement/       MovementPattern türevleri (SideSweep, Approach)
├─ Gameplay/
│  ├─ Flow/           GameFlow, RunSystem, RunSession, PlayerProfile, CharacterSelection,
│  │                  Workforce, AccessorySystem, RewardSystem, StoreSystem, CatalogArt,
│  │                  SettingsSystem
│  ├─ Player/         PlayerSystem, PlayerStatus, PlayerRig, CollisionSystem, CameraFollowSystem
│  ├─ Chaser/         ChaserSystem
│  ├─ World/          TrackSystem, SpawnDirector, AdaptiveDirector, ObstacleField, PickupField,
│  │                  WorldCurveSystem, PoolWarmupSystem, ObstacleView, PickupView
│  └─ Effects/        EffectRunner, AbilitySystem, TimeFeelSystem, VfxSystem, SfxSystem
├─ Infrastructure/    PlayerPrefsSaveService, PoolService, InputReader,
│                     UnityLocalizationService, FrameRateService, UrpRenderQuality
├─ UI/                sunucular (*Presenter), ScreenRouter, UiText, RadialProgress, SafeArea
│  ├─ Screens/        GameUI.uxml, GameUI.uss
│  ├─ Theme/          GameTheme.tss, GamePanelSettings.asset
│  ├─ Fonts/ Icons/   yazı ailesi, tek renkli çizgi ikonlar (SVG)
│  ├─ Sprites/        temanın 9 dilimli panel ve buton görselleri (UiKitBuilder üretir)
│  ├─ Portraits/      karakter çizimleri (PortraitBuilder gerçek modelden çeker)
│  └─ Items/          mağaza görselleri (ItemIconBuilder)
├─ Bootstrap/         GameLifetimeScope.cs (composition root)
├─ Shaders/           CurvedLit.shader + CurvedWorld.hlsl (kıvrımlı dünya), Sky.shader (gündüz gökyüzü),
│                     BlobShadow.shader (damla gölge)
├─ Editor/            PlaceholderContentBuilder (parçalı sınıf), CharacterModelBuilder, PropModelBuilder,
│                     PortraitBuilder, ItemIconBuilder, UiKitBuilder, ModelSnapshot, AndroidBuild, PerfProbe
├─ Content/           üretilen tanım asset'leri (GameConfig, ContentCatalog, Characters/...)
├─ Art/Placeholder/   üretilen materyal ve mesh'ler
├─ Art/Characters/    gerçek karakter modelleri: <Ad>/<Ad>.fbx + doku + materyal, Animations/ (klipler, denetleyiciler)
├─ Art/Props/         gerçek engel, eşya ve dekor modelleri: <Ad>.fbx + <Ad>_BaseColor.png + <Ad>.mat
├─ Art/Atmosphere/    üretilen gökyüzü materyali, yansıma küp haritası, post-processing profili
├─ Art/Road/          üretilen yol ve kaldırım dokuları ile materyalleri (PlaceholderContentBuilder.Road)
├─ Audio/Sfx/         sentezlenmiş ses efektleri (Tools/Audio/make_sfx.py üretir)
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

**Görseller istisnadır:** sinyaller nesne taşımaz, bu yüzden arayüz bir çizimi sinyaldeki kimlikle Core'daki `IArtProvider`'dan sorar (`GetCharacterPortrait(id)`, `GetStoreIcon(id)`). Uygulaması `CatalogArt`'tır (Gameplay); görselin kendisi tanım asset'inde durur (`CharacterDefinition.portrait`, `StoreItemDefinition.icon`).

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

`ISaveService` (şimdilik PlayerPrefs, önek `ooo.`). Anahtarlar: `best_score`, `total_coins`, `keys`, `character`, `automated` (virgüllü kimlik listesi), `automated_count.<id>`, `accessory.<id>`, `sound` (1 açık, 0 kapalı), `quality` (`GraphicsQuality`: 0 düşük, 1 orta, 2 yüksek). Seçili dili Localization paketi kendisi kaydeder. Yeni kalıcı değer eklerken anahtarı bu listeye de yaz. Bulut kaydına geçmek için yalnızca yeni bir `ISaveService` yazılır.

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
- `Shaders/BlobShadow.shader` ("Out of Office/Blob Shadow"): damla gölge. Işıksız, zemini çarparak koyulaştırır (multiply), sisle beyaza karışır; aynı bükmeyi uygular.
- `CurvedLit` ayrıca dokunun alfasındaki maskeyle neon bölgeleri ışıtır (`_GlowStrength`; "Eşya ve çevre modelleri").
- `CurvedLit` mesh'in **köşe rengini boya olarak** çarpar (ışıma boyanmaz). Köşe rengi olmayan mesh'te Unity beyaz verir, yani hiçbir şey değişmez. Binaların farklı renkleri böyle verilir ("Eşya ve çevre modelleri", "Boyanabilir modeller").
- Yeni bir shader (ör. saydam yüzey, parçacık) dünyada kullanılacaksa `CurvedWorld.hlsl`'i dahil edip aynı işlevi çağırmalıdır. Parçacıklar şu an bükülmüyor.

## Karakter modelleri ve animasyon

Üç meslek (Yazılımcı, Grafiker, Stajyer) ve kovalayan İK-9000, Meshy ile üretilmiş iskeletli gerçek modellerdir. Görsel kural: `docs/GORSEL_DIL.md`.

**Dosyalar**

```
Art/Characters/<Ad>/<Ad>.fbx            iskeletli model (24 kemik, ~8.300 üçgen, tek materyal)
Art/Characters/<Ad>/<Ad>_BaseColor.png  1024 doku (mobilde ASTC 6x6)
Art/Characters/<Ad>/<Ad>.mat            Curved Lit materyali (araç üretir)
Art/Characters/Animations/Run.fbx       koşu klibi
Art/Characters/Animations/Actions.fbx   bekleme, zıplama, kayma klipleri
Art/Characters/Animations/Actions2.fbx  tökezleme, yakalanma, oturma, asılma klipleri
Art/Characters/Animations/Character.controller   oyuncunun Animator denetleyicisi (araç üretir)
Art/Characters/Animations/Chaser.controller      kovalayanın denetleyicisi: koşu ve bekleme (araç üretir)
Prefabs/Characters/Character_<Ad>.prefab          oyunun kullandığı prefab (araç üretir)
Prefabs/Characters/Chaser_HR9000.prefab           kovalayan (araç üretir)
```

**Kurulum aracı:** `CharacterModelBuilder` (menü: Out of Office > Karakter Modellerini Kur). Tekrar çalıştırmak güvenlidir. Yaptıkları:

- Model: Humanoid iskelet, kemik Transform'ları sahneye çıkmaz (`optimizeGameObjects`), mesh sıkıştırma, köşe başına 2 kemik, teğet yok, okunamaz mesh. Boy `Characters` tablosundaki değere ölçeklenir.
- Doku: en fazla 1024, mipmap'li, Android ve iOS'ta ASTC 6x6. Alfası ışıma maskesidir (aşağıda "Eşya ve çevre modelleri").
- Klipler: `Clips` tablosundaki kare aralıkları ve kök hareketi ayarlarıyla kesilir. Klip dosyaları T pozunda açılmadığı için duruş pozu Yazılımcı modelinden kopyalanır (`CopyRestPose`); yoksa bütün klipler kayık oynar.
- Prefab: boş kök + `Model` çocuğu (Animator, avatar, denetleyici).

Klipler Humanoid'dir; **tek klip seti bütün karakterlerde oynar** (İK-9000 dahil: oyuncunun koşu ve bekleme kliplerini kullanır). Yeni karakter için klip üretmek gerekmez.

**Animator sözleşmesi.** Parametreleri yalnızca `PlayerRig` yazar; `PlayerSystem` ve diğer sistemler Animator'ı bilmez.

| Parametre | Tür | Anlamı |
|---|---|---|
| `Moving` | bool | Koşuyor mu (Bekleme ↔ Koşu) |
| `Grounded` | bool | Yerde mi; değilse Zıplama durumuna geçilir (varsayılan: doğru) |
| `Sliding` | bool | Kayıyor mu |
| `RunRate` | float | Koşu klibinin oynatma hızı (koşu hızına göre, `GameConfig` sınırlarıyla) |
| `AirPhase` | float | Zıplama klibinin konumu: kalkış 0, tepe 0,5, iniş 1 |
| `Pose` | int | Takılı görselin istediği duruş: 0 yok, 1 oturuyor (sandalye), 2 asılı (dron) |
| `Caught` | bool | Yakalandı; geriye düşer ve yerde kalır |
| `Stumble` | tetik | Engele çarpıp sendeledi |

Duruşu takılan görsel belirler: `AttachVisualEffect.pose` alanı `IPlayerAttachments.Attach` ile `PlayerRig`'e ulaşır. Duruş veren görsel karakterin boyuna göre ölçeklenir (`Animator.humanScale / GameConfig.poseReferenceHumanScale`): oturma ve asılma klipleri karakterle birlikte küçüldüğü için oturak kalçaya, tutamak ellere denk gelir. Konum `localOffset`'ten gelir. Oturma kaydında karakter merkezden yana ve geriye kaymış oturduğu için klibin yatay konumu pozda tutulmaz (gövde ağırlık merkeziyle kökün üstüne gelir); kalça yine kökün gerisinde kaldığından sandalye 22 cm geriye takılır. Duruş varken gövde şerit değiştirirken yatmaz (takılı görselden ayrılmasın diye). Yeni bir binek ya da taşıyıcı için kod gerekmez; etki asset'inde `pose` seçilir. Tökezleme ve yakalanmayı `PlayerSystem` sinyallerden (`PlayerStumbledSignal`, `GameStateChangedSignal`) alıp `PlayerRig.Stumble()` ve `SetCaught()` ile iletir.

Kovalayanın tek parametresi `Caught`'tır; `ChaserSystem` yazar (oyuncuyu yakalayıp `ChaserDefinition.catchGap` kadar gerisine gelince koşmayı bırakır).

Zıplama klibi süreyle değil `AirPhase` ile ilerler; değer dikey hızdan hesaplanır. Bu yüzden zıplama çarpanı, hızlı iniş ve uçuş (tepe pozunda süzülür) ayrıca ele alınmaz. Zıplama klibinde dikey kök hareketi pozdan çıkarılmıştır: yüksekliği oynanış verir.

Prefab'da Animator yoksa (geçici küp modeller) `PlayerRig` eski elle esnetme yoluna düşer; iki yol aynı `Animate` çağrısını kullanır.

**Kaynak (Meshy görev kimlikleri, yeniden indirme ya da yeni klip için):** rig görevleri `Tools/ModelPipeline/props.json` içinde "characters" altındadır. Klipler Yazılımcı rig'inden üretildi: koşu rig'le birlikte gelir; bekleme (0), zıplama (466), kayma (516) bir animasyon görevinde, tökezleme (416), düşme (366), oturma (33), asılma (478) ikinci görevdedir (parantez içi Meshy animasyon kimliği).

## Eşya ve çevre modelleri

Engeller, toplananlar, aksesuar ve dekor da Meshy ile üretildi (25 model). Altın tek istisnadır: 160 üçgenlik diski ve dokusunu betik kurar (`make_coin.py -- <çıktı klasörü>`; doku koddan çizilir: doygun altın, ışığı dokuya gömülü kabartma kenar, çip amblemi).

**Hat (depo kökünde `Tools/ModelPipeline`, Blender gerekir):**

| Dosya | İşi |
|---|---|
| `props.json` | Her modelin Meshy görevi, hedef ölçüsü (metre), yönü, doku boyutu, üçgen bütçesi, ışıma kuralı |
| `convert_prop.py` | Meshy GLB → yön, ölçek, orijin, dilimleme (Z boyunca 4 m'den uzunsa), doku, dokusuz FBX |
| `make_coin.py` | Altının mesh'i ve dokusu |
| `convert_character_texture.py` | Rig'li karakterin gömülü dokusunu ayrı dosyaya çıkarır |
| `pipeline_common.py` | Doku işleri: UV adaları arasındaki boşluğu doldurma, küçültme, ışıma maskesi |
| `run_all.sh` | Bütün eşyaları kaynak GLB klasöründen yeniden üretir |

Kaynak GLB'ler depoda tutulmaz; `props.json`'daki görev kimliğiyle Meshy'den yeniden indirilir.

**Işıma maskesi.** Meshy yalnızca taban rengi verir. Hat, dokudaki parlak neon pikselleri (ton, doygunluk, parlaklık kuralıyla) bulur ve maskeyi dokunun alfa kanalına yazar: alfa 1 = ışımaz, alfa 0 = tam ışır. `Curved Lit` shader'ı `_GlowStrength` ile bu bölgeleri ışıtır; ayrı ışıma dokusu gerekmez. Alfasız dokuda alfa 1 okunduğu için eski materyaller etkilenmez.

**Unity tarafı.** `PropModelBuilder` (menü: Out of Office > Eşya ve Çevre Modellerini Kur) içe aktarma ayarlarını ve materyalleri kurar. Prefab kurmaz: içerik üretici her prefab'da `Model(root, "<Ad>", ...)` ile gerçek modeli ister, yoksa geçici küpleri kurar; çarpışma kutusu iki durumda da aynıdır. Zemin parçasında dekor modelleri materyal başına tek mesh'te birleşir (parça başına 9 draw call, ~13.000 üçgen).

**Boyanabilir modeller (binalar).** `PropSpec`'te `tintable: true` olan modelin dokusundan açık, renksiz bir kopya üretilir (`<Ad>_Neutral.png`; kaynak doku değişmez, ışıma maskesi korunur) ve materyal onu kullanır. Rengi yerleştirirken verilen boya belirler: `PropModelBuilder.TryAddModel(..., tint: renk)` mesh'in bir kopyasının köşe rengine boyayı yazar. Kopya asset değildir; `CombineByMaterial` ile birleştirilen bir mesh'e katılmalıdır (zemin parçasındaki binalar gibi). Böylece aynı iki kule modeli tek materyalle, ek draw call olmadan istenen sayıda renkte durur. Renkler `PlaceholderContentBuilder.Prefabs.cs` içindeki `TowerPalette` dizisindedir.

**Yön kuralı.** Hat, modelin önünü gelen oyuncuya (Unity'de -Z) çevirir; oyuncuyla aynı yöne bakması gerekenler (`"face": "forward"`: sandalye, taşıyıcı dron) +Z'ye bakar. Blender'da -Y, FBX'ten sonra Unity'de +Z olur.

## Arayüz görselleri

Arayüzdeki her görsel koddan üretilir; elle çizilmiş dosya yoktur. Görünüm kuralı: `docs/TASARIM.md`.

| Araç | Ürettiği | Nasıl |
|---|---|---|
| `UiKitBuilder` | `UI/Sprites`: `panel`, `slot`, `btn_primary`, `btn_accent`, `btn_secondary`, `frame_selected`, `spot`, sekme ikonları `tab_home`, `tab_collection`, `tab_store`, `tab_settings` (`UiKitBuilder.TabIcons.cs`); `UI/Items/AccessCard` | İşaretli uzaklık (SDF) ile çizer: dış çizgi, taban, geçiş, parlama. Renk ve ölçüler `Plate` tablosunda, sekme ikonları kendi çizim işlevlerindedir |
| `PortraitBuilder` | `UI/Portraits/<Ad>.png` (512) | `Prefabs/Characters/Character_<Ad>` prefab'ını bekleme pozunda, kendi ışığıyla, saydam zemine çeker; başı kadraja ortalar |
| `ItemIconBuilder` | `UI/Items/<Ad>.png` (256) | `Icons` tablosundaki eşya modelini (`Art/Props`) çeker; modeli olmayanı `UiKitBuilder` çizer |
| `ModelSnapshot` | (ortak yardımcı) | `PreviewRenderUtility` ile çizim, 2x çizip küçültme, saydam PNG, içe aktarma ayarı (mobilde ASTC 4x4) |

- Portre ve eşya görselleri gerçek modelden çekildiği için model ya da doku değişince aracı yeniden çalıştırmak yeter.
- Görseli tanım asset'ine içerik üretici bağlar: portre prefab adıyla eşleşir (`Character_Developer` -> `Developer.png`), mağaza görseli `StoreItem(...)` satırındaki adla.
- Portre açısı bekleme klibinin yönüne bağlıdır (`PortraitBuilder.Yaw`): klipte karakter -X'e dönük durur.
- Kıvrımlı dünya değerleri (`_OooCurve`) çekimden önce sıfırlanır; yoksa son Play'den kalan kıvrım modeli büker.
- Play modundayken USS ya da UXML kaydedilirse Unity arayüzü yeniden kurar ve sunucuların bağları kopar (arayüz kaybolur). Bu yalnızca editörde olur; Play'i yeniden başlat.

## Ses

- `SfxLibrary` (Data, `Content/SfxLibrary.asset`): olay → klip eşlemesi (`SfxEvent`, ses düzeyi, rastgele perde payı), ortak ses düzeyi, aynı anda çalabilen ses sayısı, altın serisi ayarları.
- `SfxSystem` (Gameplay/Effects): yalnızca sinyal dinler, `VfxSystem`'in ses karşılığıdır. Açılışta `[Sfx]` nesnesine sabit sayıda `AudioSource` kurar ve sesleri sırayla onlardan çalar; koşu sırasında nesne üretilmez. Sesler 2B'dir.
- Arayüz Gameplay'i tanımadığı için düğme sesi ayrı sinyalle değil, düğmelerin zaten attığı **istek sinyalleriyle** (`StartRunRequestedSignal`, `StorePurchaseRequestedSignal`...) çalar. İstek atmayan arayüz hareketleri (menü sekmeleri) sessizdir.
- Art arda toplanan altının sesi her seferinde biraz incelir (`coinStreak*` alanları).
- Klipler kayıt değildir: `Tools/Audio/make_sfx.py` (yalnızca Python standart kütüphanesi) sentezler ve `Assets/_Game/Audio/Sfx` altına yazar. Üretici (`PlaceholderContentBuilder.Audio.cs`) içe aktarma ayarlarını (tek kanal, ADPCM, bellekte açık) ve `SfxLibrary`'yi kurar.
- Ses Ayarlar sekmesinden açılıp kapanır (`SettingsSystem`, `AudioListener.volume`). Müzik henüz yok.

## Ayarlar

Menünün Ayarlar sekmesi: dil, ses, görüntü kalitesi ve geliştirici kartı.

- `SettingsPresenter` (UI) yalnızca istek sinyali atar: `LanguageSelectRequestedSignal(kod)`, `SoundSetRequestedSignal`, `QualitySetRequestedSignal`, `DeveloperPageRequestedSignal`. Seçili seçeneği `LocaleChangedSignal` ve `SettingsChangedSignal`'dan öğrenir.
- Dil: `UnityLocalizationService` seçer; paket kaydeder.
- Ses ve kalite: `SettingsSystem` (Gameplay) kayda yazar (`sound`, `quality`), uygular ve `SettingsChangedSignal` atar. Kalite Initialize'da, ilk karede uygulanır; sinyal PostStart'ta atılır (arayüz Start'ta abone olur).
- Kalitenin sayısal karşılığı `GameConfig`'tedir (`renderScaleLow/Medium/High`, `postProcessingOnLow`, `defaultQuality`). Nasıl uygulanacağını `IRenderQuality` bilir; URP karşılığı `UrpRenderQuality` (Infrastructure) render ölçeğini etkin URP asset'ine, post-processing'i kameraya yazar ve kapanışta asset'in özgün ölçeğini geri koyar (editörde Play'den sonra asset değişik kalmasın).
- Geliştirici kartının metinleri çeviri tablosundadır (`ui.dev.*`, GitHub profilinden), bağlantı `GameConfig.developerUrl`. Avatar yok; baş harfler gösterilir.
- Yeni seçenek: Core'a istek sinyali, `SettingsSystem`'e alan ve kayıt anahtarı, UXML'e buton, `SettingsPresenter`'daki tabloya satır.

## Gölge ve zemin

- **Gerçek zamanlı gölge kapalıdır** (mobilde gölge geçişi sahneyi ikinci kez çizer). Üretici URP asset'lerinde gölgeyi kapatır (`ApplyRenderSettings`) ve güneşi gölgesiz kurar. Yerine her karakterin, robotun, engelin ve güçlendirmenin altında **damla gölge** vardır: `Blob Shadow` shader'lı tek quad (`PlaceholderContentBuilder.Shadows.cs`).
- Prefab'lardaki gölgeyi üretici koyar (`BlobShadow(root, boyut)`); oyuncunun ve kovalayanın gölgesini `PlayerRig` ve `ChaserSystem` `VfxLibrary.blobShadowPrefab`'tan kurar. Oyuncunun gölgesi zıplarken yerde kalır, küçülür ve soluklaşır (`VfxLibrary`: `shadowFadeHeight`, `airShadowScale`, `airShadowStrength`).
- **Zemin "akıllı yol"dur** (`PlaceholderContentBuilder.Road.cs`): asfalt, kenarda bordür ve ışık şeridi, iki yanda kaldırım. Dokular koddan çizilir (`Art/Road`; PNG yazılır, içe aktarma ayarı kurulur): asfalt tanesi ve lekeleri, tekerlek izleri, kesikli şerit çizgileri, kenar çizgileri, ışıyan kılavuz noktaları ve şerit ortasında ışıyan oklar (ışıma alfada). Doku 10 m'de bir tekrar eder; zemin parçası 30 m'dir.

## Performans kuralları

- Koşu döngüsünde bellek ayırma yok; listeler alan (field) olarak tutulup yeniden kullanılır.
- Engel, altın, zemin, efekt havuzdan gelir; yükleme anında `PoolWarmupSystem` ön üretir.
- Aynı materyali kullanan parçalar prefab içinde tek mesh'te birleştirilir (`CombineByMaterial`).
- `Shader.PropertyToID` değerleri `static readonly` alanlarda saklanır.
- Kare hızı `FrameRateService` ile cihazın yenileme hızına ayarlanır (60-120).

## Tarifler

Geçici içerik döneminde tanım asset'lerini `PlaceholderContentBuilder.Data.cs` üretir; aşağıdaki "asset" adımları şimdilik oraya bir satır eklemek demektir. Sonra **Out of Office > Placeholder İçeriği Yeniden Oluştur**.

**Yeni meslek (karakter)**
1. Prefab (yalnızca görsel; çarpışma koddan gelir). Gerçek model için: Humanoid'e uygun, T pozunda iskeletli FBX'i ve dokusunu `Art/Characters/<Ad>/` altına koy, `CharacterModelBuilder.Characters` tablosuna ad ve boy ekle, aracı çalıştır. Üreticide o karakterin satırını `CharacterModelBuilder.PrefabOr(...)` ile sar. Sonra **Karakter Portrelerini Üret**: portre kendiliğinden çıkar.
2. `CharacterDefinition`: `id`, `nameKey`, `themeColor` (portrenin zemin rengi), `portrait`, `prefab`, `ability` (boş olabilir), otomasyon alanları, `automatedObstacle`.
3. `ContentCatalog.characters` listesine ekle.
4. Çeviri satırı (`char.<id>`). Kod yok.

**Yeni engel / düşman**
1. Prefab. Collider yoksa görsel sınırlarına göre kendiliğinden eklenir. Gerçek model için: `docs/GORSEL_DIL.md`'deki reçeteyle üret, `Tools/ModelPipeline/props.json`'a satır ekle, hattı çalıştırıp çıktıyı `Art/Props` altına koy, `PropModelBuilder.Props` tablosuna ekle, üreticideki prefab işlevinde `Model(root, "<Ad>", ...)` çağır.
2. `ObstacleDefinition`: `avoidBy` (ChangeLane / Jump / Slide), `movement` (boşsa sabit), `minDifficulty`, `spawnWeight`.
3. `WorldDefinition.obstacles` listesine ekle. Kod yok.

**Yeni güçlendirme**
1. Toplanacak nesnenin prefab'ı.
2. `PowerUpDefinition`: `duration`, `spawnWeight`, `effects` (var olan etkileri birleştir).
3. `WorldDefinition.powerUps` listesine ekle, çeviri satırı (`powerup.<id>`). Kod yok.

**Yeni karakter animasyonu:** klibi (aynı iskeletle) `Art/Characters/Animations/` altına koy, `CharacterModelBuilder.Clips` tablosuna satır ekle, `BuildController`'da durum ve geçişini tanımla. Yeni parametre gerekiyorsa onu yalnızca `PlayerRig` yazsın; değerini hesaplamak için gereken bilgi `Animate` çağrısına eklenir.

**Yeni ses efekti:** `Tools/Audio/make_sfx.py` içindeki `build` sözlüğüne klibi ekle (ya da hazır bir WAV'ı `Audio/Sfx` altına koy), betiği çalıştır. Yeni bir olaysa `SfxEvent`'e değer, `SfxSystem` kurucusuna tek abonelik satırı ekle. Üreticide `BuildSfx`'e `AddSfx(...)` satırı yaz, içeriği yeniden üret.

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

**Yeni ekran ya da arayüz parçası:** UXML'e eleman (ad camelCase, metin `#anahtar`), USS'e sınıf (`docs/TASARIM.md`; önce var olan bileşenlere bak), UI'da bir `*Presenter` (`IStartable`'da bağla, sinyal dinle, istek sinyali at), `GameLifetimeScope`'a kayıt. Yeni veri gerekiyorsa önce Core'a sinyal ekle; yeni bir görsel türü gerekiyorsa `IArtProvider`'a üye ekle.

**Yeni mağaza ürünü:** `StoreItemDefinition` (`kind`, `amount`, `price`, `icon`) ve `ContentCatalog.storeItems`. Görsel için `ItemIconBuilder.Icons` tablosuna eşyanın modelini ekle (modeli yoksa `UiKitBuilder`'a çizimini yaz) ve **Eşya Görsellerini Üret**'i çalıştır; `icon` boşsa satırda türün çizgi ikonu görünür. Yeni ödül türü gerekiyorsa `RewardKind`'a değer ve `RewardSystem.Grant`'a dal ekle.

**Yeni dünya:** zemin prefab'ı (Z boyunca bölünmüş), `WorldDefinition`, `ContentCatalog.worlds`. Dünyalar arası geçiş henüz yazılmadı.

## Editör araçları

- `PlaceholderContentBuilder` (parçalı): `.cs` (ortak yardımcılar, `Mat`, `Part`, `SlicedPart`, `CombineByMaterial`), `.Prefabs`, `.Data`, `.Vfx`, `.Audio` (ses klipleri ve `SfxLibrary`), `.Localization`, `.Scene`, `.Atmosphere` (gökyüzü, güneş, sis, yansıma, post-processing; görünüm kuralı `docs/GORSEL_DIL.md`), `.Road` (akıllı yol: mesh ve prosedürel dokular), `.Shadows` (damla gölge, URP gölgesini kapatma).
- `CharacterModelBuilder`: gerçek karakter modellerini kurar (yukarıda "Karakter modelleri ve animasyon"). Çalıştırdıktan sonra tanım asset'leri yeni prefab'ı görsün diye içerik üreticiyi de çalıştır.
- `PropModelBuilder`: eşya ve çevre modellerinin içe aktarma ayarlarını, materyallerini ve boyanabilir modellerin renksiz dokusunu kurar (yukarıda "Eşya ve çevre modelleri").
- **Out of Office > Oyun Ayarlarını Varsayılana Döndür:** `GameConfig`'i koddaki değerlere çeker (elle yapılan dengeleme silinir).
- `PortraitBuilder`, `ItemIconBuilder`, `UiKitBuilder`: arayüz görselleri (yukarıda "Arayüz görselleri").
- `AndroidBuild`: Android deneme çıktısı. "Android Test" build profilini (yalnızca oyun sahnesi) oluşturur ve `Builds/Android/SonMesai.apk` üretir (`AGENTS.md`, "Android çıktısı").
- `PerfProbe`: Play modunda kare başına bellek ayırma, çizim çağrısı ve kare süresini örnekler (`PerfProbe.Start()`, sonra `PerfProbe.Report()`). Önce/sonra karşılaştırması içindir; editör değerleri cihazdakinden farklıdır. Editörün kendisi menüde bile kare başına ~45 KB ayırır; oyunun payını görmek için aynı ölçümü menüde de alıp farka bak, ortancayı (`med`) oku.
