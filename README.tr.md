<div align="center">

# 🏃 Son Mesai · Out of Office

**Yapay zekâ işini aldı. Sıra sende.**

Unity 6 ile yapılmış mobil sonsuz koşu oyunu

![Unity](https://img.shields.io/badge/Unity-6000.6-black?logo=unity)
![Platform](https://img.shields.io/badge/platform-Android%20%7C%20iOS-blue)
![Render](https://img.shields.io/badge/render-URP%2017-green)
![UI](https://img.shields.io/badge/UI-UI%20Toolkit-orange)

[🇬🇧 English](README.md) · 🇹🇷 Türkçe

</div>

---

## Oyun hakkında

Yıl 2031. Yapay zekâ bütün işleri almıştır. Sen dev bir şirketin **son insan çalışanısın**. İK robotu **İK-9000** elinde çıkış belgesiyle peşinde; imzayı attırırsa "optimize edilirsin". Tek umudun, sinyalin çekmediği efsanevi **Analog Kasaba**'ya ulaşmak.

**Son Mesai**, üç şeritli yolda koştuğun, engellerden kaçıp altın ve güçlendirme topladığın bir mobil sonsuz koşu oyunudur. Kurumsal dünya ve yapay zekâ jargonuyla dalga geçen hafif, iğneleyici bir mizahı vardır; kimseyi hedef almaz, alay ettiği şey ofis kültürü ve teknoloji modalarıdır.

### Oyunu farklı kılan iki şey

- 🧠 **Seni öğrenen yapay zekâ.** Oyun hangi şeritte ne kadar durduğunu izler ve engelleri sevdiğin şeritlere yöneltir. Hafıza koşular arasında korunur, eski alışkanlıklar zamanla unutulur. Yani kendi verinle eğitilen bir rakibe karşı koşarsın. (Her engel sırasında en az bir şerit her zaman açık kalır.)
- 🤖 **Meslek otomasyonu.** Karakterler mesleklerdir. Yakalanınca oynadığın meslek **otomatikleşir**: kilitlenir ve o mesleğin robotu sonraki koşularda sana engel olarak çıkar. Mesleği altınla geri alırsın; bedel her otomatikleşmede 1,5 katına çıkar.

## Özellikler

- **Üç şeritli koşu:** şerit değiştir, zıpla, kay. Hız 13 m/sn'den 30 m/sn'ye çıkar, zorluk 120 saniyede en yükseğe ulaşır.
- **Tökezle ve kaç:** ilk çarpışmada tökezlersin ve İK-9000 ensene yapışır; 5 saniye içinde ikinci çarpışma yakalanmaktır.
- **4 meslek, 3 yetenek:** Yazılımcı (*Debug*: öndeki engelleri siler), Grafiker (*Yeniden Çiz*: engellerin içinden geçersin), Şoför (*Sür*: otonom kapsüle biner, tek çarpışmayı affeder), Stajyer.
- **8 engel:** Sunucu Kabini, Çerez Duvarı, Robot Süpürge, Kargo Dronu, Otonom Taksi ve Otonom Şehir'in Otonom Otobüs, Kontrol Noktası Bariyeri, Trafik Konisi Robotu. Bazıları şeritler arasında süzülür ya da üstüne gelir.
- **10 güçlendirme:** CAPTCHA, Halüsinasyon, Kıdem Kalkanı, Kargo Dronu, Maaş Mıknatısı, Fazla Mesai x2, Çift Espresso, Prompt Injection (altınları robot toplar), Fişi Çek (robotlar kapanır), Sürpriz Paket.
- **Dünyalar:** Plaza ve **Otonom Şehir** (gün batımı); koşarken mesafeye göre geçilir, gökyüzü, zemin, binalar ve yol kenarı değişir.
- **Öğretici:** ilk koşuda engele yaklaşınca oyun yavaşlar, 3B ok hangi hareketi yapacağını gösterir.
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

**Planlananlar:** müzik, yeni dünyalar (lojistik deposu, veri merkezi, Analog Kasaba), yeni meslekler, görevler ve başarımlar, iOS çıktısı ve **final sürprizi**: İK-9000'in arkasında yeni bir model belirir, onu da işinden etmeye gelmiştir… 😉

## Katkı

Hata bildirimi ve öneriler için [Issues](https://github.com/emrahatalay/Out-of-Office/issues) bölümünü kullanabilirsin. Kod katkısı yapacaksan önce [`AGENTS.md`](AGENTS.md) ve [`docs/MIMARI.md`](docs/MIMARI.md) dosyalarını oku: katman kuralları ve yasaklar orada.

## Lisans ve telif

Henüz bir açık kaynak lisansı eklenmemiştir; lisans dosyası eklenene kadar tüm hakları saklıdır. Oyunun karakterleri, adı ve görselleri özgündür; başka bir oyunun içeriği kopyalanmamıştır.
