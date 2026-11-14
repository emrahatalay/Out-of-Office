# Son Mesai (Out of Office)

Unity 6 ile yapılan mobil sonsuz koşu oyunu. Yapay zekâ herkesin işini almıştır; İK robotu **İK-9000** şirketin son insan çalışanını kovalar. Oyuncu üç şeritte koşar, engellerden kaçar, altın ve güçlendirme toplar.

Bu dosya projede çalışan her yapay zekâ ajanı (Claude Code, Codex vb.) içindir. İşe başlamadan önce tamamını oku. Ayrıntılar `docs/` altındadır:

| Dosya | Ne zaman okunur |
|---|---|
| `docs/HIKAYE.md` | Hikâye, oynanış kuralları, içerik listesi. Yeni içerik ya da metin eklerken. |
| `docs/TASARIM.md` | Arayüz ve görsel dil, renkler, yazı tonu. Arayüze ya da görsele dokunurken. |
| `docs/GORSEL_DIL.md` | 3B modellerin tasarım kuralı, bütçeleri ve üretim reçetesi. Model üretirken ya da değiştirirken. |
| `docs/MIMARI.md` | Kod yapısı, kalıplar, "şunu nasıl eklerim" tarifleri. Kod yazarken. |
| `docs/DURUM.md` | Yapılanlar, eksikler, bilinen sorunlar. Sıradaki işi seçerken. |

Bir şeyi değiştirdiğinde ilgili belgeyi de aynı işte güncelle. Belge ile kod çelişirse kod doğrudur; belgeyi düzelt.

## Ortam

- **Unity 6000.6.8f1**, URP 17.6, yeni Input System (eski `Input` sınıfı kapalı), UI Toolkit, Unity Localization 1.5.14, VContainer 1.19 (git commit'ine sabitli).
- Hedef: Android ve iOS, dikey ekran. Android'de IL2CPP + ARM64.
- Tek sahne: `Assets/_Game/Scenes/Game.unity`. Oyunun tüm kodu ve içeriği `Assets/_Game` altında.
- Depo: `https://github.com/emrahatalay/Out-of-Office.git`, ana dal `main`.
- Unity'nin yeni özelliklerini tercih et (proje sahibinin isteği). Eski API yerine Unity 6 karşılığını kullan; emin değilsen paket kaynağına (`Library/PackageCache`) ya da güncel belgeye bak, ezberden yazma.

Editörde oynama: ok tuşları / WASD (şerit, zıpla, kay), Space (yetenek), E (aksesuar), Esc (duraklat). Dokunmatikte kaydırma; çift dokunuş aksesuar.

## Mimari, tek bakışta

Her klasör ayrı bir assembly'dir; bağımlılık yalnızca aşağıdan yukarı akar.

```
Core            saf sözleşmeler: sinyaller, durum makinesi, servis arayüzleri (hiçbir şeye bağlı değil)
Data            ScriptableObject tanımları, etkiler, hareket desenleri   -> Core
Gameplay        oynanış sistemleri                                      -> Core, Data
Infrastructure  kayıt, havuz, girdi, çeviri, kare hızı                   -> Core
UI              UI Toolkit sunucuları (presenter)                        -> Core
Bootstrap       GameLifetimeScope: somut sınıfları bilen TEK yer         -> hepsi
Editor          içerik üretici ve araçlar
```

- **Sistemler birbirini tanımaz.** Haberleşme tipli sinyal veri yolu (`ISignalBus`) üzerinden olur.
- **UI ile Gameplay birbirine bağlı değildir.** İkisi de yalnızca Core'daki sinyalleri bilir.
- **Oyun durumunu yalnızca `GameFlow` değiştirir.** Diğerleri `...RequestedSignal` atar, sonucu `GameStateChangedSignal` ile öğrenir.
- **İçerik veridir.** Karakter, engel, güçlendirme, aksesuar, dünya, mağaza ürünü birer ScriptableObject'tir; yeni içerik için kod yazılmaz.
- **Yeni davranış = tek yeni sınıf.** Yeni etki `EffectDefinition`'dan, yeni hareket `MovementPattern`'dan türer; var olan kod değişmez.

## Kurallar

1. **Katmanlara uy.** asmdef'e yeni referans eklemeden önce dur: Gameplay'den UI'ya, UI'dan Gameplay'e, Core'dan herhangi bir yere referans eklenmez. İhtiyaç varsa Core'a sinyal ya da arayüz ekle.
2. **Sayılar `GameConfig`'de durur.** Hız, süre, mesafe, olasılık gibi ayarları koda gömme; `GameConfig`'e `[Tooltip]` ile ekle.
3. **Metinler çeviri tablosundadır.** Her metin için `Game` tablosuna Türkçe ve İngilizce satır ekle (`PlaceholderContentBuilder.Localization.cs`). UXML'de `text="#anahtar"` yazmak yeterli.
4. **Sık üretilen nesneler havuzdan gelir** (`IPoolService`). Koşu sırasında `Instantiate` / `Destroy` yok.
5. **Koşu döngüsünde bellek ayırma yok.** `Tick` içinde LINQ, `new` liste, string birleştirme, lambda yakalama yapma.
6. **Sinyaller `readonly struct`'tır**, adları olaysa geçmiş zaman (`PlayerJumpedSignal`), istekse `...RequestedSignal`.
7. **Abonelikler bırakılır.** `SubscriptionBag` kullan, `Dispose`'da boşalt.
8. **Kod dili:** tanımlayıcılar İngilizce, yorumlar ve `[Tooltip]` metinleri Türkçe. C# 9 (dosya kapsamlı namespace yok). Namespace `OutOfOffice.*`.
9. **Dünyadaki her yüzey `Out of Office/Curved Lit` shader'ını kullanır** (gökyüzü hariç; o `Out of Office/Sky`). Yoksa kıvrılan yolun üstünde düz kalır. Z boyunca uzun mesh'ler birkaç metrede bir bölünmüş olmalı.
10. **Her 3B model `docs/GORSEL_DIL.md`'ye uyar:** tombul ve renkli koşu oyunu çizgisi + neon cyberpunk, üç renk, ışıyan parça, dokuda yazı yok, tek materyal, bütçe içinde.
11. **Geçici içerik koddan üretilir.** Prefab, materyal, tanım asset'i, çeviri tablosu ve sahne `PlaceholderContentBuilder` ile kurulur. Bunları değiştirmek için üreticiyi değiştir, sonra yeniden çalıştır (aşağıya bak).

## Yasaklar

- `OnGUI`, uGUI Canvas, TextMesh Pro kullanma. Arayüz yalnızca UI Toolkit'tir.
- UXML'de `style="..."` yazma; USS'te `gap`, `z-index`, `box-shadow`, `linear-gradient`, `border` kısaltması kullanma (Unity USS'inde yok).
- Eski `UnityEngine.Input`, `FindObjectOfType`, singleton / `static` oyun durumu, `GameObject.Find` ile sistem bulma yok. Bağımlılık kurucudan (constructor) gelir.
- Oyun sistemleri için `MonoBehaviour` yazma. Sistemler düz C# sınıfıdır ve VContainer giriş noktasıdır. `MonoBehaviour` yalnızca sahnedeki nesneye yapışması gereken küçük görünümler içindir (`ObstacleView`, `PickupView`, `TriggerRelay`).
- `Packages/manifest.json` dosyasını elle düzenleme. Paketleri Unity'nin paket yöneticisiyle ekle.
- `.meta` dosyalarını silme ya da elle yazma; asset taşırken `.meta`'sıyla birlikte taşı.
- `Assets/_Game/Art/Placeholder`, `Art/Atmosphere`, `Prefabs/Placeholder`, `Prefabs/Characters`, `Content`, `Localization/Tables`, `Scenes/Game.unity`, `UI/Sprites`, `UI/Portraits`, `UI/Items` ve `Art/Props` ile `Art/Characters` altındaki materyal, denetleyici ve `_Neutral` dokularında elle kalıcı değişiklik yapma: üretici ya da araçlar yeniden çalışınca üzerine yazar. (`GameConfig.asset` istisna; ayarları korunur.)
- Kaydı (PlayerPrefs, `ooo.` önekli) testlerde kalıcı bozma. Test öncesi değerleri sakla, sonra geri yükle.
- Proje sahibinin e-posta adresini, API anahtarlarını ya da kişisel bilgilerini koda, belgeye ya da dış servislere yazma.
- Subway Surfers'ın ya da başka bir oyunun karakterini, adını, görselini, arayüz çizimini kopyalama. Tür aynı, içerik özgün.

## İçerik üretici

Menü: **Out of Office > Placeholder İçeriği Yeniden Oluştur** (`PlaceholderContentBuilder.BuildAll()`).

Sırasıyla arayüz görsellerini ayarlar, prefab'ları, tanım asset'lerini, görsel ve ses efektlerini, çeviri tablosunu ve sahneyi baştan kurar; GUID'ler korunur. Şunları **baştan yazar**: `ContentCatalog` listeleri, `World_Plaza` engel ve güçlendirme listeleri, sahnenin tamamı. Bu yüzden yeni içerik şimdilik üreticiye (`PlaceholderContentBuilder.Data.cs`) eklenir. Gerçek modellere geçilince bu akış değişecek; `docs/DURUM.md`'ye bak.

Açık sahnede kaydedilmemiş değişiklik varken ya da Play modundayken çalıştırma.

**Gerçek modeller** iki araçla kurulur; ikisi de içe aktarma ayarlarını ve materyalleri üretir, elle değil araçtan değiştirilir:

- **Out of Office > Karakter Modellerini Kur** (`CharacterModelBuilder.BuildAll()`): üç meslek ve İK-9000. Kaynak `Assets/_Game/Art/Characters`; Animator denetleyicilerini ve `Prefabs/Characters` altındaki prefab'ları da üretir.
- **Out of Office > Eşya ve Çevre Modellerini Kur** (`PropModelBuilder.BuildAll()`): engeller, toplananlar, aksesuar, dekor. Kaynak `Assets/_Game/Art/Props`.

**Arayüz görselleri** üç araçla üretilir:

- **Out of Office > Karakter Portrelerini Üret** (`PortraitBuilder.BuildAll()`): menü ve koleksiyondaki çizimleri gerçek karakter modellerinden çeker (`UI/Portraits`).
- **Out of Office > Eşya Görsellerini Üret** (`ItemIconBuilder.BuildAll()`): mağaza görsellerini gerçek eşya modellerinden çeker, modeli olmayanı (giriş kartı) çizer (`UI/Items`).
- **Out of Office > Arayüz Görsellerini Üret** (`UiKitBuilder.BuildAll()`): temanın panel ve buton görsellerini çizer (`UI/Sprites`).

Sıra: önce model araçları, sonra portre ve eşya görselleri, en son içerik üretici (görselleri tanım asset'lerine o bağlar). Üretici çarpışma kutularını kendi koyar, görsel olarak gerçek modeli (yoksa geçici küpü) kullanır. Kaynak FBX ve dokuları `Tools/ModelPipeline` betikleri Meshy çıktısından üretir. Ayrıntı: `docs/MIMARI.md`, "Karakter modelleri ve animasyon" ile "Eşya ve çevre modelleri".

**Ses efektleri** `Tools/Audio/make_sfx.py` ile sentezlenir (`Assets/_Game/Audio/Sfx`); hangi olayda hangisinin çalacağını üretici `SfxLibrary` asset'ine yazar. Ayrıntı: `docs/MIMARI.md`, "Ses".

## Android çıktısı

Menü: **Out of Office > Android Test Profilini Oluştur**, sonra **Out of Office > Android Test Çıktısı Al (APK)** (`AndroidBuild.CreateProfile()`, `AndroidBuild.Build()`).

- Unity 6 build profili kullanılır: `Assets/Settings/Build Profiles/Android Test.asset`. Profil yalnızca oyun sahnesini içerir; IL2CPP + ARM64 proje ayarlarından gelir.
- Çıktı `Builds/Android/SonMesai.apk` (depoda izlenmez), hata ayıklama anahtarıyla imzalıdır: cihaza elle kurulur (`adb install -r "Builds/Android/SonMesai.apk"`), mağazaya yüklenemez.
- Çıktı almak editörün platformunu Android'e çevirir (dokular yeniden içe aktarılır) ve dakikalar sürer; proje sahibi editörü kullanıyorsa önce haber ver. Editör başka platformdaysa araç önce platformu değiştirir ve çıktıyı ikinci çalıştırmada alır (yoksa çeviri içeriği yanlış platform için paketlenir, oyun cihazda başlamaz).
- Çıktıyı cihazda doğrula: kur, başlat, `adb logcat -s Unity` ile hataya bak. "Çıktı alındı" tek başına "çalışıyor" demek değildir.

## Doğrulama

Bir işi "bitti" demeden önce:

1. **Derle ve konsola bak.** Hata ve uyarı sıfır olmalı.
2. **Play modunda dene.** Değişen davranışı gerçekten çalıştır; yalnızca derlenmesi yetmez.
3. **Arayüz ya da görsel değiştiyse ekran görüntüsü al** ve bak.
4. **Neyi denemediğini açıkça söyle.**

Editör açıksa Unity CLI ile (proje `com.unity.pipeline` paketini içerir):

```bash
P="/Users/emrah/Unity Projects/Out of Office"
unity status --project-path "$P"                                   # editör hazır mı, Play'de mi
unity command script recompile --project-path "$P"                 # derle
unity command console get --project-path "$P" -- --tail 60 --level warn
unity command script eval --project-path "$P" -- --file /tmp/x.cs --timeout 60   # editörde C# çalıştır
unity command editor.playmode start --project-path "$P"            # stop ile durdur
unity command capture game_view --project-path "$P" -- --source camera --overlay true --width 720 --height 1280 --output-path "$P/Temp/shot.png"
```

Dikkat edilecekler:

- **Proje sahibi editörde oynuyor olabilir.** Derlemeden, içerik üretmeden ya da Play'e girip çıkmadan önce `unity status` ile bak. Onun Play oturumunu haber vermeden durdurma.
- Editör odakta değilken Play donar. Test için `unity command editor set_auto_tick` ile açık tut, iş bitince `-- --enabled false` ile kapat.
- Ekran görüntüsünde Game penceresi yerine Simulator açıksa `--source screen` çalışmaz; `--source camera --overlay true` kullan.
- Çalışan sisteme `eval` içinden ulaşmak: `VContainer.Unity.LifetimeScope.Find<OutOfOffice.Bootstrap.GameLifetimeScope>().Container.Resolve<T>()`.
- İçerik üretmek: `return OutOfOffice.EditorTools.PlaceholderContentBuilder.BuildAll();`

Editör kapalıysa ya da CLI yoksa bunu söyle ve proje sahibinden Unity'de denemesini iste; denenmemiş işi denenmiş gibi sunma.

## Git

- Commit ve push yalnızca proje sahibi isteyince yapılır.
- `main` üzerinde doğrudan commit atma: dal aç, commit'le, sonra `main`'e birleştir (şimdiye kadar hızlı ileri sarma ile yapıldı).
- Commit mesajları Türkçe, ilk satır ne değiştiğini söyler.
- `Assets/TextMesh Pro`, `.idea`, `.DS_Store` commit edilmez. Her asset `.meta`'sıyla birlikte commit edilir.

## Proje sahibiyle çalışma

- Emrah Türkçe konuşur; cevaplar **Türkçe, kısa ve öz** olsun. Uzun açıklama istemezse yapma.
- C#/.NET geçmişi var; mimariyi önemsiyor (SOLID, gevşek bağlılık, genişletilebilirlik). Kısayol yerine yapıya uyan çözümü seç.
- Belirsiz ve geri dönüşü pahalı kararlarda önce sor; küçük kararları kendin ver ve ne seçtiğini söyle.
- Yapmadığın ya da doğrulamadığın şeyi yapılmış gibi anlatma.
