# 3B görsel dil kuralı

Oyundaki her model bu kurala uyar. Yeni model üretmeden ya da var olanı değiştirmeden önce oku.

**Tek cümleyle:** oyuncak gibi tombul ve renkli mobil koşu oyunu çizgisi ile neon cyberpunk ofis dünyasının karışımı.

## İki kaynak

| Kaynak | Ondan alınan |
|---|---|
| Renkli mobil koşu oyunları (Subway Surfers'ın da içinde olduğu tür) | Abartılı tombul oranlar, kalın yuvarlak pahlar, doygun renkler, boyanmış gibi temiz yüzey, uzaktan okunan siluet, neşeli ton |
| Cyberpunk | Koyu gövdeli makineler, neon ışık çizgileri, hologram ekranlar, görünür teknoloji parçaları, kurumsal distopya mizahı |

Karışımın dengesi: **biçim birinciden, ışık ikinciden gelir.** Nesne önce sevimli ve tombul bir oyuncaktır; cyberpunk ona ışığı, ekranı ve koyu gövdeyi verir. **Dünya ise aydınlık ve neşelidir** (gündüz, mavi gökyüzü, şeker renkli binalar); koyu gövdeli robotlar ve makineler bu açık sahnede kendiliğinden öne çıkar.

Ne değildir: gerçekçi oran, kirli ya da paslı yüzey, kan, korku, gri-kahverengi "distopya" paleti. Başka bir oyunun karakteri, aracı ya da nesnesi kopyalanmaz; tür aynı, içerik özgündür.

## Kurallar

1. **Tombul biçim.** Kalın gövde, yuvarlatılmış köşe, iri baş ve ayak, kısa ve kalın uzuv. İnce çubuk, tel, sivri uç yok (mesh'te kaybolur, uzaktan okunmaz).
2. **Üç renk.** Bir nesnede en fazla: koyu gövde (gunmetal ya da lacivert), **tek** doygun ana renk, bir neon vurgu. Karakterlerde ana renk baskındır; engellerde koyu gövde baskındır.
3. **Neonun rengi anlam taşır** (arayüzle aynı, `docs/TASARIM.md`):

   | Renk | Anlam | Örnek |
   |---|---|---|
   | Kırmızı | Tehlike, kovalayan | İK-9000, robot süpürge |
   | Sarı | Para ve ödül | Altın, sürpriz paket, fazla mesai |
   | Camgöbeği | Teknoloji, yardımcı güç | Kalkan, sandalye, yazılımcı |
   | Yeşil | Doğrulama, sunucu | CAPTCHA, sunucu kabini |
   | Mor | Yanılsama | Halüsinasyon |
   | Turuncu | Kargo, çerez, kahve | Kargo dronu, çerez duvarı, espresso |
   | Macenta | Tasarım | Grafiker ve robotu |
   | Pembe | Komut, istem | Prompt Injection |

4. **Her nesne ışıldar.** En az bir ışıyan parça olur (şerit, ekran, göz, kenar). Oyuncunun kaçması ya da toplaması gereken şey sahnenin en parlak şeyidir; dekor ondan sönüktür.
5. **Siluet oynanışı söyler.** Altı boş nesnenin altından kayılır, alçak nesnenin üstünden atlanır, tam boy nesneden şerit değiştirilerek kaçılır. Biçim bu okumayı bozmaz.
6. **Yazı yok.** Dokuda harf, rakam, logo bulunmaz; anlam ikonla verilir (onay işareti, çerez, şimşek). Oyun iki dillidir, doku çevrilemez.
7. **Boyalı yüzey.** Yalnızca taban rengi dokusu; normal, metalik, pürüzlülük haritası yok. Parlaklık materyalden tek sayıyla gelir.
8. **Mizah ofisten ve yapay zekâdan.** Nesne ne olduğunu ofis hayatı ya da internet terimiyle anlatır (`docs/HIKAYE.md`, "Ton").

## Teknik bütçe

Mobil hedef. Aşılacaksa gerekçesi `docs/DURUM.md`'ye yazılır.

| Tür | Üçgen | Doku | Not |
|---|---|---|---|
| Oyuncu karakteri, İK-9000 | ≤ 8.500 | 1024 | İskeletli, köşe başına 2 kemik |
| Büyük engel (taksi, meslek robotu) | ≤ 3.000 | 512 | |
| Engel (kabin, duvar, dron, süpürge) | ≤ 2.000 | 512 | |
| Güçlendirme, paket, aksesuar | ≤ 1.200 | 256 | |
| Altın | ≤ 300 | 256 | Ekranda onlarcası olur; mesh'i ve dokusunu betik kurar (160 üçgen, doku koddan çizilir) |
| Dekor (kolon, masa, pano) | ≤ 1.500 | 512 | |
| Bina (kule) | ≤ 600 | 1024 | Boyanabilir: her kopya ayrı pastel renk alır. Doku büyük çünkü 26 m'lik yüzeye yayılır; 256'da bulanıktı |

- Model başına **tek materyal, tek doku**. Shader her zaman `Out of Office/Curved Lit`.
- Doku ASTC 6x6 (Android, iOS), mipmap'li.
- **Işıma maskesi dokunun alfa kanalındadır:** alfa 1 = ışımaz, alfa 0 = tam ışır. Ayrı ışıma dokusu yok. Maskeyi model hattı üretir (aşağıda).
- Z boyunca 4 m'den uzun mesh 2,5 m'de bir bölünür (kıvrımlı dünya yalnızca köşe noktalarını büker).

## Sahnenin havası

Modeller kadar bağlayıcıdır; hepsi `PlaceholderContentBuilder.Atmosphere.cs` içinde koddan kurulur.

- **Gündüz, açık ve iç açıcı.** Gökyüzü parlak maviden açık ufka iner; ufkun üstündeki kuşakta yavaşça kayan pamuk bulutlar vardır (`Out of Office/Sky` shader'ı, doku kullanmaz).
- **Gökyüzüne yere basan şekil çizilmez** (bina silueti, dağ). Yol uzakta aşağı kıvrıldığı için böyle bir şekil havada asılı görünür. Ufkun altı ufuk renginde kalır.
- **Sis ufuk rengiyle aynıdır** (72 m'de başlar, 140 m'de kapanır). Uzaktaki nesne gökyüzüne karışarak kaybolur; aniden belirmez.
- **Binalar şeker renklidir.** Aynı iki kule modeli boyanarak dizilir: mercan pembesi, nane, bal sarısı, lavanta (`TowerPalette`). Yan yana ve karşı karşıya gelen binalar farklı renktedir. Kolonlar krem beyazıdır, neon çizgilerini korur. Yeni bina rengi pastel ve doygun olmalı; gri, kahverengi, koyu lacivert bina yok.
- **Zemin "akıllı yol"dur:** açık gri-mor asfalt (tane, leke, tekerlek izi), beyaz kesikli şerit ve kenar çizgileri, şerit ortasında turkuaz ışıyan oklar, kenarda ışıyan kılavuz noktaları. Kenarda krem bordür ve turkuaz ışık şeridi, iki yanda açık taş kaldırım (binalar kaldırımda durur). Otonom şehir bulvarı hissi; dokuda yazı yok.
- **İki ışık, gerçek gölge yok.** Arkadan sıcak güneş, karşıdan gök mavisi dolgu ışığı (gölgede kalan yüzler kararmaz). Gölge haritası kapalıdır; her karakterin, robotun, engelin altında yumuşak, serin tonlu (siyah değil) bir **damla gölge** durur. Ortam ışığı üstten mavi, yandan açık gri, alttan zeminden seken ton.
- **Post-processing ölçülüdür:** bloom yalnızca ışıyan parçaları yayar (eşik yüksek), kontrast ve doygunluk belirgin artar (soluk görünmesin), gölgeler hafif koyulur, kenar karartması çok hafiftir. Alan derinliği, hareket bulanıklığı, renk sapması, film greni yok (mobilde pahalı ve görüntüyü bulandırır).
- **Yağmur yok.**
- Okunurluk sırası değişmez: toplanacak ve kaçılacak nesneler > oyuncu > dekor > gökyüzü. Açık sahnede bunu parlaklık değil **renk doygunluğu ve koyu gövde** sağlar: engeller koyu gövdeli ve neonludur, dekor pastel ve soluktur.

## Üretim reçetesi (Meshy)

1. **Konsept görsel:** `nano-banana-pro`, nesne tek başına, açık gri fon, yumuşak düz ışık, üç çeyrek önden. Karakter ve iki ayaklı robot için önden, T pozu, eller boş.
2. **3B:** görselden, `meshy-t2.5` (smart topology) + bütçedeki üçgen hedefi + `meshy-n1` doku. Rig olacaksa `pose_mode: t-pose`.
3. **Rig (yalnızca iki ayaklılar):** Meshy rig; klipler Humanoid olduğu için ortak set kullanılır (`docs/MIMARI.md`).
4. **Optimizasyon:** `Tools/ModelPipeline` betikleri: yön, ölçek ve orijin, doku boşluklarını doldurma ve küçültme, ışıma maskesi, dokusuz FBX (`props.json`'a satır eklenir).
5. **Unity:** `PropModelBuilder` / `CharacterModelBuilder` içe aktarma ayarlarını ve materyali kurar.

İstem şablonu (İngilizce yazılır, köşeli parantezler doldurulur):

```
[Nesnenin ne olduğu ve oranı: tall / wide / flat]. [Koyu gövde] body, [ana renk] [parçalar],
glowing [neon rengi] [ışıyan parçalar]. [Varsa ikon]. Stylized 3D mobile game asset in a
colorful chunky toy-like cartoon style mixed with cyberpunk: thick rounded shapes, bold
readable silhouette, smooth clean surfaces, hand-painted look, saturated glowing neon accents.
Soft even studio lighting, no harsh shadows, plain light grey background, single object,
centered, three-quarter front view, whole object visible, no text, no letters, no logos.
```

## Kontrol listesi

Model oyuna girmeden önce:

- [ ] Uzaktan, küçükken ne olduğu anlaşılıyor mu?
- [ ] Üç renk kuralına uyuyor mu; neonun rengi anlamına uygun mu?
- [ ] Işıyan parçası var mı, bloom ile parlıyor mu?
- [ ] Dokuda yazı var mı? (Olmamalı.)
- [ ] Bütçenin içinde mi (üçgen, doku, tek materyal)?
- [ ] Çarpışma kutusuyla aynı boyda mı; oynanış okuması (zıpla, kay, kaç) doğru mu?
- [ ] Kıvrılan yolda düzgün bükülüyor mu?
