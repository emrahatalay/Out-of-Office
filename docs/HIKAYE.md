# Hikâye ve oynanış

Oyunun adı Türkçede **Son Mesai**, diğer dillerde **Out of Office**.

## Hikâye

Yıl 2031. Yapay zekâ bütün işleri almıştır. Oyuncu dev bir şirketin son insan çalışanıdır. İK robotu **İK-9000** (EN: HR-9000) elinde çıkış belgesiyle peşindedir; imzayı attırırsa oyuncu "optimize edilir". Hedef, sinyalin çekmediği efsanevi **Analog Kasaba**'ya ulaşmaktır.

Slogan: "Yapay zekâ işini aldı. Sıra sende." Yakalanınca ekranda "İK-9000 SENİ YAKALADI / OPTİMİZE EDİLDİN" yazar.

**Planlanan final sürprizi (henüz yapılmadı):** belli bir mesafeden sonra İK-9000'in arkasında daha yeni bir model belirir; onu da işinden etmeye gelmiştir. İK-9000 oyuncunun safına geçer ve oynanabilir karakter olarak açılır.

**Planlanan dünyalar:** Plaza (var) → otonom şehir → lojistik deposu → veri merkezi → Analog Kasaba.

## Ton

Kurumsal dünya ve yapay zekâ jargonuyla dalga geçen, hafif ve iğneleyici bir mizah. Kimseyi hedef almaz; alay edilen şey ofis kültürü ve teknoloji modalarıdır.

Yeni içerik adlandırırken aynı kaynaktan beslen: ofis hayatı (mesai, kıdem, maaş, stajyer, giriş kartı, espresso) ve yapay zekâ / internet terimleri (CAPTCHA, halüsinasyon, prompt injection, çerez, debug). Türün klasik nesnelerini doğrudan kopyalama, bu dünyadaki karşılığını bul. Örnekler: kaykay → ofis sandalyesi, jetpack → kargo dronu, anahtar → giriş kartı.

## Çekirdek döngü

Üç şeritli yolda oyuncu kendiliğinden ileri koşar. Şerit değiştirir, zıplar, kayar. Hız 13 m/sn'den 30 m/sn'ye çıkar; zorluk 120 saniyede en yükseğe ulaşır. Puan gidilen yoldan ve altınlardan (altın başına 10) gelir.

**Tökezleme ve yakalanma:** ilk çarpışmada oyuncu tökezler ve İK-9000 ensesine yapışır. 5 saniye içinde ikinci çarpışma yakalanmaktır. Çarpmadan geçen sürede robot geri düşer.

**Koşu sonu:** yakalanınca giriş kartı varsa devam teklifi çıkar. Devam edilmezse koşu biter, toplanan paketler açılır, sonuç ekranı gelir.

## Oyunu farklı kılan iki mekanik

**Seni öğrenen yapay zekâ (`AdaptiveDirector`).** Oyun, oyuncunun hangi şeritte ne kadar kaldığını izler ve engelleri sevdiği şeritlere yöneltir. Hafıza koşular arasında korunur, eski alışkanlıklar zamanla unutulur. Oyuncu kendi verisiyle eğitilen bir rakibe karşı oynar. Her engel sırasında en az bir şerit her zaman açık kalır.

**Meslek otomasyonu (`Workforce`).** Karakterler mesleklerdir. Oyuncu yakalanınca oynadığı meslek "otomatikleşir": kilitlenir ve o mesleğin robotu sonraki koşularda engel olarak çıkar. Meslek altınla geri alınır; bedel 100 altından başlar ve her otomatikleşmede 1,5 katına çıkar. **Stajyer** otomatikleşmez; kilitli meslek seçiliyse onun yerine oynanır.

## Meslekler

| Meslek | Yetenek | Otomatikleşince çıkan robot |
|---|---|---|
| Yazılımcı | **Debug:** öndeki 45 m içindeki engelleri siler. 18 sn bekleme. | Üstüne doğru koşan robot |
| Grafiker | **Yeniden Çiz:** 3 sn boyunca engellerin içinden geçilir. 22 sn bekleme. | Şeritler arasında gidip gelen robot |
| Şoför | **Sür:** 6 sn boyunca açık tepeli otonom kapsüle biner; tek çarpışmayı affeder (kalkan + araç görseli). 25 sn bekleme. | Hızla (9 m/sn) üstüne doğru süren robot |
| Stajyer | Yok | Otomatikleşmez |

## Engeller

| Engel | Nasıl kaçılır | Davranış |
|---|---|---|
| Sunucu Kabini | Şerit değiştir | Sabit |
| Çerez Duvarı ("Çerezleri kabul et") | Kay | Sabit |
| Robot Süpürge | Zıpla | Sabit |
| Kargo Dronu | Kay | Şeritler arasında süzülür; zorluk 0,2'den sonra |
| Otonom Taksi | Şerit değiştir | Oyuncuya doğru gelir; zorluk 0,35'ten sonra |

## Güçlendirmeler

Yolda toplanır, süreleri bitince etkisi kalkar.

| Güçlendirme | Etki | Süre |
|---|---|---|
| CAPTCHA | İK-9000 "Ben robot değilim" kutusunda takılır, geride kalır | 6 sn |
| Halüsinasyon | Engeller sahteleşir, içlerinden geçilir | 5 sn |
| Kıdem Kalkanı | Tek çarpışmayı affeder | 12 sn |
| Kargo Dronu | Oyuncuyu havaya kaldırır, engellerin üstünden uçar (jetpack karşılığı) | 8 sn |
| Maaş Mıknatısı | Altınları çeker | 10 sn |
| Fazla Mesai x2 | Puanı ikiye katlar | 12 sn |
| Çift Espresso | Daha yükseğe zıplatır | 10 sn |
| Prompt Injection | Önündeki 30 m içindeki altınları, hangi şeritte olursa olsun, oyuncu yerine robot toplar (mıknatıstan farkı: şerit sınırı yok, menzil uzun) | 8 sn |
| Fişi Çek | Kovalayan ve bütün robotlar (meslek robotları, süpürge, dron, taksi) durur ve çarpmaz | 5 sn |
| Sürpriz Paket | Anında sayılır, içeriği koşu sonunda açılır | - |

## Aksesuar, paket, giriş kartı, mağaza

- **Ofis Sandalyesi** (kaykay karşılığı): oyuncu istediği an kullanır. 20 sn boyunca tek çarpışmayı affeder; çarpınca sandalye kırılır. Stoklanır.
- **Sürpriz paket:** koşuda toplanır, koşu bitince açılır. İçinden 40-150 altın, 1-2 sandalye ya da 1 giriş kartı çıkar (altın en olası, kart en nadir).
- **Giriş Kartı** (türdeki anahtarın karşılığı): yakalanınca kartı okutup kaldığın yerden devam edersin. İlk devam 1 kart, aynı koşuda her devamda iki katı. Teklif 6 sn ekranda kalır; devam edince öndeki 45 m engelden temizlenir. Kodda `Key` / `Keys` olarak geçer.
- **Mağaza:** altınla sandalye (1 adet 150, 3 adet 400) ve giriş kartı (1 adet 300, 3 adet 800) satılır. Gerçek parayla satış yok.

## Görsel numara: kıvrımlı dünya

Yol koşu boyunca sağa ya da sola kıvrılıyor, arada düzleşiyor ve uzakta aşağı eğiliyor gibi görünür. Tamamen görseldir: dünya gerçekte düzdür, şeritler ve çarpışmalar etkilenmez.

## Bu belgedeki sayılar

Sayılar yazıldığı andaki ayarlardır. Doğrusu her zaman `Assets/_Game/Content/GameConfig.asset` ile tanım asset'lerinde ve `PlaceholderContentBuilder.Data.cs`'tedir.
