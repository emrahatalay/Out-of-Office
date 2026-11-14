# Tasarım dili

Hedef his: aydınlık, neşeli bir gündüz şehri; içinde koyu gövdeli, neon ışıklı kurumsal robotlar. Arayüz, oyundaki nesnelerle aynı dili konuşan tombul bir oyun arayüzü takımıdır (UI kit): kalın koyu dış çizgi, yuvarlak köşe, altta kalın taban, üstte parlama. Modern mobil koşu oyunlarının netliğinde, ama görsel kimliği kendine ait.

## Renkler

Arayüz renkleri `Assets/_Game/UI/Screens/GameUI.uss` başındaki değişkenlerdir. Yeni renk gerekiyorsa önce bunlardan biri iş görür mü bak; koda ya da USS'e elle renk kodu yazma.

| Değişken | Değer | Kullanım |
|---|---|---|
| `--color-primary` | `#ff4d5f` | Ana eylem (OYNA), tehlike, İK-9000 |
| `--color-accent` | `#ffc83d` | Altın, ödül, satın alma, seçili sekme ve kart |
| `--color-tech` | `#4ee3ff` | Yetenek, güçlendirme, giriş kartı, adet sayıları |
| `--color-text` | `#ffffff` | Ana metin |
| `--color-muted` | `#b9c2f2` | İkincil metin, pasif ikon |
| `--color-ink` | `#12163d` | Dış çizgi, sekme çubuğu, rozet zemini (görsellerdeki dış çizgiyle aynı) |
| `--color-ink-soft` | `rgba(18,22,61,0.88)` | Çip zemini |
| `--color-ink-shadow` | `rgba(18,22,61,0.5)` | Yazı gölgesi |
| `--color-well` | `rgba(14,18,58,0.5)` | Panel içindeki oyuk (istatistik, yetenek etiketi) |
| `--color-line` | `rgba(255,255,255,0.2)` | Çiplerin ince kenarlığı |
| `--color-on-accent` | `#3a2600` | Sarı zeminin üstündeki yazı ve ikon |
| `--color-showcase` | `#8794f2` | Mağaza görselinin vitrin zemini |
| `--color-locked` | `#9aa2cf` | Otomatikleşen mesleğin portresini soluklaştıran boya |
| `--color-overlay` | `rgba(14,18,58,0.74)` | Duraklatma ve oyun sonu karartması |

Panel ve butonların kendi renkleri (çivit mavisi panel, kırmızı / sarı / mavi buton) görsellerin içindedir; `UiKitBuilder`'daki tabloda durur.

Anlam sabittir: kırmızı = tehlike / ana eylem, sarı = para ve ödül, camgöbeği = teknoloji ve yetenek. Mavi (ikincil buton, şerit başlık) nötrdür. Aynı rengi başka anlamda kullanma.

Sahne: gündüz gökyüzü (parlak maviden açık ufka), sis ufuk renginde (`#B3E0FF` dolayı), 72 m'de başlar, 140 m'de kapanır. Ayrıntı `docs/GORSEL_DIL.md`, "Sahnenin havası".

## Yazı

- Tek yazı ailesi: **Chakra Petch** (Medium, SemiBold, Bold), `Assets/_Game/UI/Fonts`, OFL lisanslı. Başka font ekleme.
- Başlıklar ve buton yazıları BÜYÜK HARF, Bold, harf aralığı açık. Gövde metni SemiBold.
- Boyutlar 1080x1920 tasarım ölçüsüne göredir: skor ve logo 68-76, buton 48 (büyük buton 72), başlık 40-44, gövde 30-36, etiket 28.
- Türkçe büyük harfe dikkat: "İ" ve "I" ayrıdır (OPTİMİZE, MAĞAZA).

## Boşluk ve biçim

- Boşluk ölçeği: `--space-xs` 8, `--space-sm` 16, `--space-md` 28, `--space-lg` 48. Aradaki değerleri uydurma.
- Köşeler belirgin yuvarlak; çipler hap biçimli. Keskin köşeli kutu yok.
- **Biçimli parçalar 9 dilimli görseldir** (`UI/Sprites`; `UiKitBuilder` koddan üretir, elle düzenlenmez): `panel`, `slot`, `btn_primary`, `btn_accent`, `btn_secondary`, `frame_selected`, `spot`. Hepsinde aynı reçete: koyu dış çizgi, düşey renk geçişi, altta kalın "taban", butonlarda üstte parlama. Görseller 2x çizilir (`-unity-slice-scale: 0.5px`); aynı görselin küçük hali için ölçek düşürülür (satın alma butonu, etkin sekme).
- **Düz parçalar USS rengiyle çizilir:** çipler (`--color-ink-soft` + ince kenarlık), oyuklar (`--color-well`), rozetler.
- Basılan her şey küçülür (%92-96).
- **Sahnenin üstünde çıplak duran yazı** (logo, skor) `.over-scene` alır: koyu dış çizgi ve gölge. Gündüz gökyüzünde beyaz yazı başka türlü okunmaz. Panelin içindeki yazıya dış çizgi verilmez.
- Eylem ikonları (oynat, duraklat, ev, kupa, kilit...) tek renkli, çizgi tarzı SVG'dir (`UI/Icons`, 24x24 viewBox, ~2,2 kalınlık, beyaz). Rengi USS'te `-unity-background-image-tint-color` ile verilir. Yeni ikon aynı tarzda çizilir; emoji kullanılmaz. Sarı butonun üstünde çizgi ikon koyu olur.
- **Alt sekme ikonları renkli çizimdir** (çizgi ikon değil): kırmızı çatılı ev, kravatlı çalışan, kurdeleli mor hediye, metal dişli (`UI/Sprites/tab_*`, `UiKitBuilder` çizer). Temanın diliyle: kalın koyu dış çizgi, düşey renk geçişi, parlama, altta gölge. Boyanmazlar; etkin olmayan sekmede hafif soluk, etkin sekmede tam renk ve biraz büyük. Yeni sekme ikonu da aynı dille çizilir.
- **İçerik görselleri gerçektir:** meslek portresi karakterin kendi 3B modelinden (`UI/Portraits`), eşya görseli eşyanın oyundaki modelinden çekilir (`UI/Items`). Oyunda modeli olmayan eşya (giriş kartı) aynı tema diliyle çizilir. Yerine çizgi ikon ya da temsili şekil konmaz.
- **Eşya her yerde aynı görselle gösterilir:** altın, giriş kartı ve sandalye için `.icon-coin`, `.icon-key`, `.icon-chair` sınıfları mağazadaki görselin küçüğünü kullanır (sayaç çipleri, satın alma butonları, oyun sonu ödülleri, HUD). Bu ikonlar boyanmaz.

## Bileşenler

Var olan sınıfları kullan, benzerini yeniden yazma.

| Bileşen | Sınıflar |
|---|---|
| Buton | `.btn` + `.btn-primary` / `.btn-secondary` / `.btn-accent`, büyük için `.btn-large`; yazı `.btn-label` (sarıda `.btn-label-dark`) |
| Küçük satın alma butonu | `.buy-button`, `.buy-label` |
| Kare ikon butonu | `.icon-button` (duraklat, önceki / sonraki) |
| Çip (sayaçlar) | `.chip`, `.chip-value`, `.chip-label`, `.chip-button` |
| Panel ve diyalog | `.card`, `.card--dialog`, `.page--panel`; başlığı `.ribbon` + `.ribbon-label` (tehlike için `.ribbon--danger`) |
| Portre | `.portrait-tile` (zemini mesleğin tema rengi), içinde `.portrait-spot` ve `.portrait-image`; kilitli için `.portrait-image--locked` |
| İsim plakası (ana sayfa) | `.nameplate`, `.nameplate-portrait`, `.selector-info`, `.selector-name` |
| Üst başlık yazısı | `.eyebrow`, tehlike için `.eyebrow--danger`, sahne üstünde `.eyebrow--over-scene` |
| Sekme çubuğu | `.tab-bar`, `.tab`, `.tab--active`; ikon `.tab-icon` + `.tab-icon--<sekme>` |
| Koleksiyon kartı | `.char-card`, `.char-card--selected` (çerçeve ve "SEÇİLİ" etiketi), `.char-lock`, `.char-price` |
| Mağaza satırı | `.store-row`, `.store-icon` + `.store-icon-image`, `.store-name`, `.store-amount` |
| Ayar satırı | `.setting-row` (yuva görseli), başlığı `.setting-head` + `.setting-icon` + `.setting-label`; seçenekler `.segment-group` > `.segment` + `.segment-label`, seçili `.segment--active` (sarı) |
| Geliştirici kartı | `.dev-card`, `.dev-avatar` + `.dev-initials`, `.dev-info` > `.dev-name` / `.dev-role` / `.dev-meta`, `.dev-button`, `.version-label` |
| İkon | `.icon` + `.icon-<ad>`; boyut `.icon-small` / `.icon-large` |
| Yetenek halkası | `RadialProgress` özel elemanı (`.ability-ring`) |

Adlandırma: sınıflar kebab-case, durum ekleri `--` ile (`.tab--active`, `.screen--out`); UXML `name` değerleri camelCase (`playButton`).

## Ekranlar

Tek UXML (`GameUI.uxml`), dört ekran: `menuScreen`, `hudScreen`, `pauseScreen`, `gameOverScreen`. Hangisinin görüneceğine `ScreenRouter` oyun durumuna göre karar verir; geçiş 0,22 sn'lik soluklaşma ve hafif kaymadır.

- **Menü:** altta sekme çubuğu (Ana Sayfa, Koleksiyon, Mağaza, Ayarlar); etkin sekme sarı ve çubuktan yukarı taşar. Üstte altın ve giriş kartı sayaçları.
  - **Ana sayfa:** logo, rekor, ortada sahnedeki gerçek karakter, altında isim plakası (portre, meslek adı, yetenek ya da "OTOMATİKLEŞTİ"), aksesuar satırı ve büyük OYNA butonu. Menü kamerası karakteri logo ile plakanın arasına oturtur (`GameConfig.menuCameraOffset`, `menuLookAt`); araya eleman eklenirse karakterin üstünü kapatmamalı.
  - **Koleksiyon:** her meslek için portreli kart. Seçili kart sarı çerçeve ve "SEÇİLİ" etiketi alır; otomatikleşen mesleğin portresi soluklaşır, köşesine kilit ve altına geri alma bedeli gelir.
  - **Mağaza:** her ürün bir satır: vitrin karesinde ürünün gerçek görseli, adı, adedi, sarı satın alma butonu.
  - **Ayarlar:** üç ayar satırı (Dil: TÜRKÇE / ENGLISH, Ses: AÇIK / KAPALI, Görüntü kalitesi: DÜŞÜK / ORTA / YÜKSEK); seçili seçenek sarıdır. Dil adları her dilde kendi dilinde yazılır. Altta "GELİŞTİRİCİ" başlığıyla geliştirici kartı (baş harfli yuvarlak avatar, ad, rol, konum), GitHub profilini açan mavi buton ve sürüm.
  - Koleksiyon ve mağaza paneli içeriği kadar yer kaplar; altında sahne görünür.
- **HUD:** üstte duraklat, skor, altın. Altta aksesuar ve yetenek düğmesi. Etkin güçlendirme rozeti. Oyun alanını kapatmayacak kadar az şey; karartma ya da geçiş bandı yok.
- **Duraklatma ve oyun sonu:** karartma üstünde tek panel, üst kenarında şerit başlık (oyun sonunda kırmızı). Oyun sonunda önce devam teklifi (varsa), sonra skor, rekor ve paketlerden çıkanlar.

Ekran kenarlarında `SafeArea` kullanılır (çentik ve köşeler). Panel 1080x1920'ye göre ölçeklenir (genişlik-yükseklik dengesi 0,5). Başlık boyutu en uzun dile göre seçilir: İngilizce "OUT OF OFFICE" tek satıra sığmalıdır.

## Metin tonu

Kısa, büyük harfli, emir kipinde düğmeler: OYNA, TEKRAR DENE, VAZGEÇ. Durum metinleri kurumsal dille iğneler: "OPTİMİZE EDİLDİN", "OTOMATİKLEŞTİ", "İŞİNİ GERİ AL". Açıklama cümleleri bir satırı geçmez. Her metin Türkçe ve İngilizce yazılır; İngilizcesi çeviri kokmamalı, aynı espriyi kendi dilinde yapmalı.

## 3B görünüm

Karakterler, İK-9000, engeller, toplananlar ve dekor gerçek modeldir; yalnızca zemin ve bariyer koddan kurulur. Modellerin bağlayıcı kuralı, bütçeleri ve üretim reçetesi `docs/GORSEL_DIL.md`'dedir; aşağısı özetidir.

**Karakter dili:** renkli, stilize mobil koşu oyunu çizgisi ile cyberpunk karışımı. İri baş, büyük ayakkabı, sade ve okunaklı biçimler; doygun tek bir ana renk (mesleğin tema rengi) ve üstünde neon ayrıntılar (devre çizgisi, parlayan taban, AR gözlük, kimlik kartı). Gerçekçi oran, kirli ya da karanlık "distopya" dokusu yok; karakter sahnedeki en doygun renkli şeydir. Her meslek kıyafetinden tanınır: Yazılımcı mavi kapüşonlu, kulaklık ve sırt çantası; Grafiker macenta ceket, sarı bere ve sırtında çizim tableti; Stajyer bol gri gömlek, turuncu kravat, boyun askısında kimlik kartı ve kahve bardağı. Yeni karakter aynı reçeteyle üretilir (T pozu, boş eller, tek parça gövde).

Korunacak dil:

- Açık, pastel bir dünya; üstünde koyu gövdeli ve neonlu, okunaklı nesneler.
- Oyuncunun kaçması ya da toplaması gereken her şey ışıldar (emisyon + bloom): robotun kırmızısı, altının sarısı, güçlendirmelerin kendi rengi.
- Her nesne siluetinden tanınır. Engelin nasıl geçileceği (zıpla, kay, şerit değiştir) biçiminden anlaşılmalı.
- Mesleklerin birer tema rengi vardır (Yazılımcı mavi, Grafiker macenta, Stajyer gri); robotları aynı rengi taşır, portrelerinin zemini de o renktir.
- Zemin açık mavi-lila taş, bariyer krem; şerit çizgileri ve bariyerin üst kenarı turkuaz. Yolun iki yanında krem kolonlar, çalışma masaları, hologram panolar; arkada şeker renkli binalar (pembe, nane, sarı, lavanta).
- İçinden geçilebilir (hayalet) engeller açık camgöbeğine boyanır.

## His

- Kamera arkadan takip eder, hız arttıkça görüş açısı 60°'den 74°'ye açılır.
- Çarpışmada kısa sarsıntı ve ağır çekim; yakalanmada daha uzun.
- Toplama, tökezleme, iniş, kalkan kırılması ve yakalanma parçacık efektiyle vurgulanır.
- Karakter animasyonları: bekleme, koşu, zıplama, kayma, tökezleme, yakalanınca düşme, sandalyede oturma, drona asılma.
- Gündüz ışığı, pamuk bulutlar, hafif bloom ve canlı doygunluk sahneye neşe verir.
- **Ses efektleri:** zıplama, kayma, şerit değiştirme, altın (art arda toplanınca incelir), güçlendirme, tökezleme, kalkan kırılması, yakalanma, düğme. Sesler kısa, yumuşak ve çizgi film tadındadır; sert, metalik ya da korkutucu ses yok. Müzik henüz yok.

Bütün bu sayılar `GameConfig`'dedir (ses düzeyleri `SfxLibrary`'de).

## Yapma

- Başka bir oyunun arayüz çizimini, ikonunu, karakterini ya da adını kopyalama. Düzen benzeyebilir, çizim özgün olmalı.
- Beyaz ya da açık zeminli panel yapma: paneller çivit mavisidir, yazı beyazdır. Açık olan sahnedir, arayüz değil.
- Dış çizgisiz, tabansız düz kutu buton yapma; buton her zaman `UiKitBuilder` görselidir.
- Tek ekranda üçten fazla vurgu rengi kullanma.
- Sahneyi karartan bant ya da geçiş koyma (eski `fade` görselleri kaldırıldı); okunurluk yazının dış çizgisinden gelir.
