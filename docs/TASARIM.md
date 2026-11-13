# Tasarım dili

Hedef his: karanlık, teknolojik bir kurumsal dünya; üstünde canlı, oyuncu dostu vurgular. Modern mobil koşu oyunlarının netliğinde, ama görsel kimliği kendine ait.

## Renkler

Arayüz renkleri `Assets/_Game/UI/Screens/GameUI.uss` başındaki değişkenlerdir. Yeni renk gerekiyorsa önce bunlardan biri iş görür mü bak; koda ya da USS'e elle renk kodu yazma.

| Değişken | Değer | Kullanım |
|---|---|---|
| `--color-primary` | `#ff4d5f` | Ana eylem (OYNA), tehlike, İK-9000 |
| `--color-accent` | `#ffc83d` | Altın, ödül, seçili sekme, rekor |
| `--color-tech` | `#4ee3ff` | Yetenek, güçlendirme, giriş kartı, adet sayıları |
| `--color-text` | `#f3f6ff` | Ana metin |
| `--color-muted` | `#93a0c2` | İkincil metin, pasif ikon |
| `--color-surface` | `rgba(12,17,36,0.74)` | Çip ve panel zemini |
| `--color-surface-strong` | `rgba(18,25,52,0.92)` | Diyalog ve kart zemini |
| `--color-border` | `rgba(160,180,255,0.22)` | İnce kenarlık |
| `--color-overlay` | `rgba(5,8,18,0.78)` | Duraklatma ve oyun sonu karartması |

Anlam sabittir: kırmızı = tehlike / ana eylem, sarı = para ve ödül, camgöbeği = teknoloji ve yetenek. Aynı rengi başka anlamda kullanma.

Sahne arka planı ve sis rengi: `#121729` (koyu lacivert). Sis 45 m'de başlar, 125 m'de kapanır.

## Yazı

- Tek yazı ailesi: **Chakra Petch** (Medium, SemiBold, Bold), `Assets/_Game/UI/Fonts`, OFL lisanslı. Başka font ekleme.
- Başlıklar ve buton yazıları BÜYÜK HARF, Bold, harf aralığı açık. Gövde metni SemiBold.
- Boyutlar 1080x1920 tasarım ölçüsüne göredir: skor ve logo 68-76, buton 48 (büyük buton 72), başlık 40-44, gövde 30-36, etiket 28.
- Türkçe büyük harfe dikkat: "İ" ve "I" ayrıdır (OPTİMİZE, MAĞAZA).

## Boşluk ve biçim

- Boşluk ölçeği: `--space-xs` 8, `--space-sm` 16, `--space-md` 28, `--space-lg` 48. Aradaki değerleri uydurma.
- Köşeler belirgin yuvarlak; çipler hap biçimli. Keskin köşeli kutu yok.
- Derinlik gölgeyle değil görselle verilir: butonlar ve kartlar 9 dilimli PNG'dir (`UI/Sprites`: `btn_primary`, `btn_secondary`, `btn_accent`, `card`, `glow`, `fade_top`, `fade_bottom`). Butonların altında kalın bir "taban" vardır; basılınca %96'ya küçülür.
- İkonlar tek renkli, çizgi tarzı SVG'dir (`UI/Icons`, 24x24 viewBox, ~2,2 kalınlık, beyaz). Rengi USS'te `-unity-background-image-tint-color` ile verilir. Yeni ikon aynı tarzda çizilir; emoji ya da renkli görsel kullanılmaz.

## Bileşenler

Var olan sınıfları kullan, benzerini yeniden yazma.

| Bileşen | Sınıflar |
|---|---|
| Buton | `.btn` + `.btn-primary` / `.btn-secondary` / `.btn-accent`, büyük için `.btn-large`; yazı `.btn-label` |
| Çip (üst bardaki sayaçlar) | `.chip`, `.chip-value`, `.chip-button` |
| Kart ve diyalog | `.card`, `.card--dialog`, `.dialog-title`, `.dialog-hint` |
| Üst başlık yazısı | `.eyebrow`, tehlike için `.eyebrow--danger` |
| Sekme çubuğu | `.tab-bar`, `.tab`, `.tab--active` |
| Koleksiyon kartı | `.char-card`, `.char-card--selected`, `.char-avatar--locked` |
| Mağaza satırı | `.store-row`, `.store-name`, `.store-amount` |
| İkon | `.icon` + `.icon-<ad>`; boyut `.icon-small` / `.icon-large` |
| Yetenek halkası | `RadialProgress` özel elemanı (`.ability-ring`) |

Adlandırma: sınıflar kebab-case, durum ekleri `--` ile (`.tab--active`, `.screen--out`); UXML `name` değerleri camelCase (`playButton`).

## Ekranlar

Tek UXML (`GameUI.uxml`), dört ekran: `menuScreen`, `hudScreen`, `pauseScreen`, `gameOverScreen`. Hangisinin görüneceğine `ScreenRouter` oyun durumuna göre karar verir; geçiş 0,22 sn'lik soluklaşma ve hafif kaymadır.

- **Menü:** altta sekme çubuğu (Ana Sayfa, Koleksiyon, Mağaza). Üstte altın ve giriş kartı sayaçları, dil düğmesi. Ana sayfada logo, seçili meslek ve büyük OYNA butonu.
- **HUD:** üstte duraklat, skor, altın. Altta aksesuar ve yetenek düğmesi. Etkin güçlendirme rozeti. Oyun alanını kapatmayacak kadar az şey.
- **Duraklatma ve oyun sonu:** karartma üstünde tek kart. Oyun sonunda önce devam teklifi (varsa), sonra skor, rekor ve paketlerden çıkanlar.

Ekran kenarlarında `SafeArea` kullanılır (çentik ve köşeler). Panel 1080x1920'ye göre ölçeklenir (genişlik-yükseklik dengesi 0,5).

## Metin tonu

Kısa, büyük harfli, emir kipinde düğmeler: OYNA, TEKRAR DENE, VAZGEÇ. Durum metinleri kurumsal dille iğneler: "OPTİMİZE EDİLDİN", "OTOMATİKLEŞTİ", "İŞİNİ GERİ AL". Açıklama cümleleri bir satırı geçmez. Her metin Türkçe ve İngilizce yazılır; İngilizcesi çeviri kokmamalı, aynı espriyi kendi dilinde yapmalı.

## 3B görünüm

Üç meslek gerçek modeldir; geri kalan her şey geçici, küp ve silindirlerden kurulu şekillerdir ve gerçek modellerle değişecek.

**Karakter dili:** renkli, stilize mobil koşu oyunu çizgisi ile cyberpunk karışımı. İri baş, büyük ayakkabı, sade ve okunaklı biçimler; doygun tek bir ana renk (mesleğin tema rengi) ve üstünde neon ayrıntılar (devre çizgisi, parlayan taban, AR gözlük, kimlik kartı). Gerçekçi oran, kirli ya da karanlık "distopya" dokusu yok; karakter koyu sahnede en parlak şeydir. Her meslek kıyafetinden tanınır: Yazılımcı mavi kapüşonlu, kulaklık ve sırt çantası; Grafiker macenta ceket, sarı bere ve sırtında çizim tableti; Stajyer bol gri gömlek, turuncu kravat, boyun askısında kimlik kartı ve kahve bardağı. Yeni karakter aynı reçeteyle üretilir (T pozu, boş eller, tek parça gövde).

Geçerken korunacak dil:

- Koyu, soğuk zemin ve duvarlar; üstünde doygun renkli, okunaklı nesneler.
- Oyuncunun kaçması ya da toplaması gereken her şey ışıldar (emisyon + bloom): robotun kırmızısı, altının sarısı, güçlendirmelerin kendi rengi.
- Her nesne siluetinden tanınır. Engelin nasıl geçileceği (zıpla, kay, şerit değiştir) biçiminden anlaşılmalı.
- Mesleklerin birer tema rengi vardır (Yazılımcı mavi, Grafiker macenta, Stajyer gri); robotları aynı rengi taşır.
- İçinden geçilebilir (hayalet) engeller açık camgöbeğine boyanır.

## His

- Kamera arkadan takip eder, hız arttıkça görüş açısı 60°'den 74°'ye açılır.
- Çarpışmada kısa sarsıntı ve ağır çekim; yakalanmada daha uzun.
- Toplama, tökezleme, iniş, kalkan kırılması ve yakalanma parçacık efektiyle vurgulanır.
- Ses henüz yok.

Bütün bu sayılar `GameConfig`'dedir.

## Yapma

- Başka bir oyunun arayüz çizimini, ikonunu, karakterini ya da adını kopyalama. Düzen benzeyebilir, çizim özgün olmalı.
- Düz beyaz ya da açık temalı ekran yapma; oyun baştan sona koyu temadır.
- Tek ekranda üçten fazla vurgu rengi kullanma.
