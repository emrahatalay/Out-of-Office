<div align="center">

# 🏃 Son Mesai · Out of Office

**Yapay zekâ işini aldı. Sıra sende.**
**AI took your job. Now it's your turn.**

Unity 6 ile yapılmış mobil sonsuz koşu oyunu · An endless runner for mobile, built with Unity 6

![Unity](https://img.shields.io/badge/Unity-6000.6-black?logo=unity)
![Platform](https://img.shields.io/badge/platform-Android%20%7C%20iOS-blue)
![Render](https://img.shields.io/badge/render-URP%2017-green)
![UI](https://img.shields.io/badge/UI-UI%20Toolkit-orange)
![Languages](https://img.shields.io/badge/dil%20%2F%20language-TR%20%7C%20EN-red)

[🇹🇷 Türkçe](#-türkçe) · [🇬🇧 English](#-english)

</div>

---

# 🇹🇷 Türkçe

## Oyun hakkında

Yıl 2031. Yapay zekâ bütün işleri almıştır. Sen dev bir şirketin **son insan çalışanısın**. İK robotu **İK-9000** elinde çıkış belgesiyle peşinde; imzayı attırırsa "optimize edilirsin". Tek umudun, sinyalin çekmediği efsanevi **Analog Kasaba**'ya ulaşmak.

**Son Mesai**, üç şeritli yolda koştuğun, engellerden kaçıp altın ve güçlendirme topladığın bir mobil sonsuz koşu oyunudur. Kurumsal dünya ve yapay zekâ jargonuyla dalga geçen hafif, iğneleyici bir mizahı vardır; kimseyi hedef almaz, alay ettiği şey ofis kültürü ve teknoloji modalarıdır.

### Oyunu farklı kılan iki şey

- 🧠 **Seni öğrenen yapay zekâ.** Oyun hangi şeritte ne kadar durduğunu izler ve engelleri sevdiğin şeritlere yöneltir. Hafıza koşular arasında korunur, eski alışkanlıklar zamanla unutulur. Yani kendi verinle eğitilen bir rakibe karşı koşarsın. (Her engel sırasında en az bir şerit her zaman açık kalır.)
- 🤖 **Meslek otomasyonu.** Karakterler mesleklerdir. Yakalanınca oynadığın meslek **otomatikleşir**: kilitlenir ve o mesleğin robotu sonraki koşularda sana engel olarak çıkar. Mesleği altınla geri alırsın; bedel her otomatikleşmede 1,5 katına çıkar.

## Özellikler

- **Üç şeritli koşu:** şerit değiştir, zıpla, kay. Hız 13 m/sn'den 30 m/sn'ye çıkar, zorluk 120 saniyede en yükseğe ulaşır.
- **Tökezle ve kaç:** ilk çarpışmada tökezlersin ve İK-9000 ensene yapışır; 5 saniye içinde ikinci çarpışma yakalanmaktır.
- **3 meslek, 2 yetenek:** Yazılımcı (*Debug*: öndeki engelleri siler), Grafiker (*Yeniden Çiz*: engellerin içinden geçersin), Stajyer.
- **5 engel:** Sunucu Kabini, Çerez Duvarı, Robot Süpürge, Kargo Dronu, Otonom Taksi. Bazıları şeritler arasında süzülür ya da üstüne gelir.
- **8 güçlendirme:** CAPTCHA, Halüsinasyon, Kıdem Kalkanı, Kargo Dronu, Maaş Mıknatısı, Fazla Mesai x2, Çift Espresso, Sürpriz Paket.
- **Ofis Sandalyesi** (aksesuar), **Giriş Kartıyla devam**, **sürpriz paketler** ve altınla çalışan bir **mağaza**. Gerçek parayla satış yok.
- **Kıvrımlı dünya:** yol koşu boyunca sağa sola kıvrılır, uzakta aşağı eğilir. Tamamen görsel bir hiledir; çarpışmalar etkilenmez.
- **Türkçe ve İngilizce** arayüz, sekmeli menü (Ana Sayfa, Koleksiyon, Mağaza, Ayarlar), görüntü kalitesi ayarı.
- **Tombul ve renkli koşu oyunu çizgisi + neon cyberpunk** görsel dil; modeller [Meshy](https://www.meshy.ai) ile üretilip proje hattından geçirildi.

### Kontroller

| | Dokunmatik | Editör / klavye |
|---|---|---|
| Şerit değiştir | Sağa / sola kaydır | ← → / A D |
| Zıpla | Yukarı kaydır | ↑ / W |
| Kay | Aşağı kaydır | ↓ / S |
| Meslek yeteneği | Arayüz düğmesi | Space |
| Aksesuar (sandalye) | Çift dokunuş | E |
| Duraklat | Arayüz düğmesi | Esc |

## Teknik bakış

| | |
|---|---|
| Motor | **Unity 6000.6.8f1**, URP 17.6 |
| Girdi | Yeni Input System |
| Arayüz | UI Toolkit (uGUI, TextMesh Pro ve `OnGUI` kullanılmaz) |
| Yerelleştirme | Unity Localization 1.5.14 (TR / EN) |
| Bağımlılık enjeksiyonu | VContainer 1.19 |
| Hedef | Android (IL2CPP, ARM64) ve iOS, dikey ekran |

### Mimari

Her klasör ayrı bir assembly'dir; bağımlılık yalnızca aşağıdan yukarı akar.

```
Core            saf sözleşmeler: sinyaller, durum makinesi, servis arayüzleri
Data            ScriptableObject tanımları, etkiler, hareket desenleri   -> Core
Gameplay        oynanış sistemleri                                      -> Core, Data
Infrastructure  kayıt, havuz, girdi, çeviri, kare hızı                   -> Core
UI              UI Toolkit sunucuları (presenter)                        -> Core
Bootstrap       GameLifetimeScope: somut sınıfları bilen TEK yer         -> hepsi
Editor          içerik üretici ve araçlar
```

Tasarım ilkeleri:

- **Sistemler birbirini tanımaz.** Haberleşme tipli sinyal veri yolu (`ISignalBus`) üzerinden olur.
- **UI ile Gameplay birbirine bağlı değildir.** İkisi de yalnızca Core'daki sinyalleri bilir.
- **Oyun durumunu yalnızca `GameFlow` değiştirir.**
- **İçerik veridir.** Karakter, engel, güçlendirme, aksesuar, dünya ve mağaza ürünü birer ScriptableObject'tir; yeni içerik için kod yazılmaz.
- **Yeni davranış = tek yeni sınıf.** Yeni etki `EffectDefinition`'dan, yeni hareket `MovementPattern`'dan türer.
- Sayılar `GameConfig`'de durur, sık üretilen nesneler havuzdan gelir, koşu döngüsünde bellek ayırma yapılmaz.

## Projeyi çalıştırma

1. **Unity 6000.6.8f1** (Unity Hub) ve Android / iOS build desteğini kur.
2. Depoyu klonla:
   ```bash
   git clone https://github.com/emrahatalay/Out-of-Office.git
   ```
3. Klasörü Unity Hub'dan aç; paketler otomatik çözülür.
4. `Assets/_Game/Scenes/Game.unity` sahnesini aç ve **Play**'e bas.

### Android çıktısı

Menüden **Out of Office > Android Test Profilini Oluştur**, ardından **Out of Office > Android Test Çıktısı Al (APK)**. Çıktı `Builds/Android/SonMesai.apk` olarak oluşur (hata ayıklama anahtarıyla imzalıdır) ve cihaza şöyle kurulur:

```bash
adb install -r "Builds/Android/SonMesai.apk"
```

### İçerik üreticileri

Prefab, materyal, tanım asset'i, çeviri tablosu ve sahne koddan üretilir. Menüdeki araçlar sırasıyla: *Karakter Modellerini Kur*, *Eşya ve Çevre Modellerini Kur*, *Karakter Portrelerini Üret*, *Eşya Görsellerini Üret*, *Arayüz Görsellerini Üret* ve en son **Placeholder İçeriği Yeniden Oluştur**. Ayrıntı için [`docs/MIMARI.md`](docs/MIMARI.md).

## Belgeler

| Dosya | İçerik |
|---|---|
| [`docs/HIKAYE.md`](docs/HIKAYE.md) | Hikâye, oynanış kuralları, içerik listesi |
| [`docs/TASARIM.md`](docs/TASARIM.md) | Arayüz ve görsel dil, renkler, yazı tonu |
| [`docs/GORSEL_DIL.md`](docs/GORSEL_DIL.md) | 3B model tasarım kuralları ve üretim reçetesi |
| [`docs/MIMARI.md`](docs/MIMARI.md) | Kod yapısı, kalıplar, "şunu nasıl eklerim" tarifleri |
| [`docs/DURUM.md`](docs/DURUM.md) | Yapılanlar, eksikler, bilinen sorunlar |
| [`AGENTS.md`](AGENTS.md) | Projede çalışan yapay zekâ ajanları için kurallar |

## Durum ve yol haritası

Oyun geliştirme aşamasındadır (sürüm 0.1.0). Android'de Samsung bir cihazda çalıştırıldı; iOS çıktısı henüz alınmadı. Güncel liste için [`docs/DURUM.md`](docs/DURUM.md).

**Planlananlar:** müzik, yeni dünyalar (otonom şehir, lojistik deposu, veri merkezi, Analog Kasaba), yeni meslekler (Şoför), yeni güçlendirmeler (Prompt Injection, Fişi Çek), görevler ve başarımlar, öğretici ve **final sürprizi**: İK-9000'in arkasında yeni bir model belirir, onu da işinden etmeye gelmiştir… 😉

## Katkı

Hata bildirimi ve öneriler için [Issues](https://github.com/emrahatalay/Out-of-Office/issues) bölümünü kullanabilirsin. Kod katkısı yapacaksan önce [`AGENTS.md`](AGENTS.md) ve [`docs/MIMARI.md`](docs/MIMARI.md) dosyalarını oku: katman kuralları ve yasaklar orada.

## Lisans ve telif

Henüz bir açık kaynak lisansı eklenmemiştir; lisans dosyası eklenene kadar tüm hakları saklıdır. Oyunun karakterleri, adı ve görselleri özgündür; başka bir oyunun içeriği kopyalanmamıştır.

---

# 🇬🇧 English

## About the game

The year is 2031. AI has taken every job. You are the **last human employee** of a giant corporation. The HR robot **HR-9000** is chasing you with a termination form; if it gets your signature, you get "optimized". Your only hope is to reach the legendary **Analog Town**, a place with no signal.

**Out of Office** (*Son Mesai* in Turkish) is a mobile endless runner: run down a three-lane road, dodge obstacles, and collect gold and power-ups. Its humor is light and dry, poking fun at corporate culture and AI buzzwords. It never targets people, only office life and tech fads.

### Two things that set it apart

- 🧠 **An AI that learns you.** The game tracks which lane you stay in and steers obstacles toward your favorites. Its memory persists between runs, and old habits fade over time. You are racing an opponent trained on your own data. (At least one lane is always left open in every obstacle row.)
- 🤖 **Job automation.** Characters are professions. When you get caught, the profession you were playing gets **automated**: it is locked, and a robot of that profession shows up as an obstacle in later runs. You buy the job back with gold, and the price rises 1.5× with each automation.

## Features

- **Three-lane running:** switch lanes, jump, slide. Speed climbs from 13 to 30 m/s and difficulty peaks at 120 seconds.
- **Stumble and flee:** the first hit makes you stumble and HR-9000 latches onto you; a second hit within 5 seconds means you are caught.
- **3 professions, 2 abilities:** Developer (*Debug* clears obstacles ahead), Designer (*Redraw* lets you pass through obstacles), and the Intern.
- **5 obstacles:** Server Cabinet, Cookie Wall, Robot Vacuum, Cargo Drone, Autonomous Taxi. Some drift between lanes or drive toward you.
- **8 power-ups:** CAPTCHA, Hallucination, Seniority Shield, Cargo Drone, Salary Magnet, Overtime x2, Double Espresso, Surprise Package.
- **Office Chair** (accessory), **continue with a Keycard**, **surprise packages**, and a gold-powered **store**. No real-money purchases.
- **Curved world:** the road bends left and right and curves downward in the distance. It is a purely visual trick; lanes and collisions are unaffected.
- **Turkish and English** UI, tabbed menu (Home, Collection, Store, Settings), and a graphics quality setting.
- A **chubby, colorful runner style with neon cyberpunk** visual language; models were generated with [Meshy](https://www.meshy.ai) and run through the project's own pipeline.

### Controls

| | Touch | Editor / keyboard |
|---|---|---|
| Change lane | Swipe left / right | ← → / A D |
| Jump | Swipe up | ↑ / W |
| Slide | Swipe down | ↓ / S |
| Profession ability | On-screen button | Space |
| Accessory (chair) | Double tap | E |
| Pause | On-screen button | Esc |

## Tech overview

| | |
|---|---|
| Engine | **Unity 6000.6.8f1**, URP 17.6 |
| Input | New Input System |
| UI | UI Toolkit (no uGUI, TextMesh Pro, or `OnGUI`) |
| Localization | Unity Localization 1.5.14 (TR / EN) |
| Dependency injection | VContainer 1.19 |
| Targets | Android (IL2CPP, ARM64) and iOS, portrait |

### Architecture

Every folder is its own assembly, and dependencies only flow upward.

```
Core            pure contracts: signals, state machine, service interfaces
Data            ScriptableObject definitions, effects, movement patterns   -> Core
Gameplay        gameplay systems                                          -> Core, Data
Infrastructure  save data, pooling, input, localization, frame rate        -> Core
UI              UI Toolkit presenters                                     -> Core
Bootstrap       GameLifetimeScope: the ONLY place that knows concrete types -> all
Editor          content builders and tools
```

Design principles:

- **Systems don't know each other.** They communicate through a typed signal bus (`ISignalBus`).
- **UI and Gameplay are decoupled.** Both only know the signals in Core.
- **Only `GameFlow` changes the game state.**
- **Content is data.** Characters, obstacles, power-ups, accessories, worlds, and store items are ScriptableObjects; new content needs no new code.
- **New behavior = one new class.** A new effect derives from `EffectDefinition`, a new movement from `MovementPattern`.
- Numbers live in `GameConfig`, frequently spawned objects come from pools, and the run loop allocates no memory.

## Getting started

1. Install **Unity 6000.6.8f1** (via Unity Hub) with Android / iOS build support.
2. Clone the repository:
   ```bash
   git clone https://github.com/emrahatalay/Out-of-Office.git
   ```
3. Open the folder in Unity Hub; packages resolve automatically.
4. Open `Assets/_Game/Scenes/Game.unity` and press **Play**.

### Android build

Use the menu **Out of Office > Android Test Profilini Oluştur** (create the Android test profile), then **Out of Office > Android Test Çıktısı Al (APK)** (build the APK). The output is `Builds/Android/SonMesai.apk`, signed with the debug key. Install it on a device with:

```bash
adb install -r "Builds/Android/SonMesai.apk"
```

### Content builders

Prefabs, materials, definition assets, the localization table, and the scene are generated from code. The menu tools run in this order: *Karakter Modellerini Kur* (character models), *Eşya ve Çevre Modellerini Kur* (prop and environment models), *Karakter Portrelerini Üret* (portraits), *Eşya Görsellerini Üret* (item icons), *Arayüz Görsellerini Üret* (UI kit), and finally **Placeholder İçeriği Yeniden Oluştur** (rebuild content). Details in [`docs/MIMARI.md`](docs/MIMARI.md) (written in Turkish).

## Documentation

The in-repo documentation is written in Turkish.

| File | Contents |
|---|---|
| [`docs/HIKAYE.md`](docs/HIKAYE.md) | Story, gameplay rules, content list |
| [`docs/TASARIM.md`](docs/TASARIM.md) | UI and visual language, colors, tone of voice |
| [`docs/GORSEL_DIL.md`](docs/GORSEL_DIL.md) | 3D model design rules and production recipe |
| [`docs/MIMARI.md`](docs/MIMARI.md) | Code structure, patterns, "how do I add X" recipes |
| [`docs/DURUM.md`](docs/DURUM.md) | What is done, what is missing, known issues |
| [`AGENTS.md`](AGENTS.md) | Rules for AI agents working on the project |

## Status and roadmap

The game is in active development (version 0.1.0). It has been run on a Samsung Android device; an iOS build has not been made yet. See [`docs/DURUM.md`](docs/DURUM.md) for the current list.

**Planned:** music, new worlds (autonomous city, logistics warehouse, data center, Analog Town), new professions (Driver), new power-ups (Prompt Injection, Pull the Plug), quests and achievements, a tutorial, and the **final twist**: a newer model appears behind HR-9000, and it has come to replace *it* too… 😉

## Contributing

Use [Issues](https://github.com/emrahatalay/Out-of-Office/issues) for bug reports and suggestions. If you plan to contribute code, read [`AGENTS.md`](AGENTS.md) and [`docs/MIMARI.md`](docs/MIMARI.md) first: the layering rules and prohibitions are there.

## License and credits

No open-source license has been added yet; until a license file is added, all rights are reserved. The characters, name, and visuals are original; no content from any other game has been copied.
