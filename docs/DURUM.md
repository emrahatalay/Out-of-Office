# Durum

Son güncelleme: 13 Kasım 2026. Bir iş bitince ya da yeni bir sorun bulununca bu dosyayı güncelle.

## Yapılanlar

- Çekirdek koşu: üç şerit, zıplama, kayma, girdi tamponu, hızlanma, zorluk eğrisi.
- Kovalayan robot (İK-9000): tökezlemede yaklaşır, ikinci çarpışmada yakalar.
- Öğrenen engel yönetmeni (şerit alışkanlığına göre engel dizer).
- 5 engel, 2 meslek robotu, 8 güçlendirme (liste `docs/HIKAYE.md`'de).
- 3 meslek (Yazılımcı, Grafiker, Stajyer), 2 yetenek.
- Meslek otomasyonu ve artan bedelle geri alma.
- Aksesuar (Ofis Sandalyesi), sürpriz paketler, giriş kartıyla devam, altınla mağaza.
- Arayüz: sekmeli menü (Ana Sayfa, Koleksiyon, Mağaza), HUD, duraklatma, oyun sonu. Türkçe ve İngilizce.
- His: kamera takibi, görüş açısı, sarsıntı, ağır çekim, parçacık efektleri.
- Kıvrımlı dünya (yol sağa/sola kıvrılır, uzakta aşağı eğilir).
- Üç mesleğin gerçek 3B modeli (Meshy ile üretildi; renkli stilize + cyberpunk): iskeletli, mobil için sıkıştırılmış. Bekleme, koşu, zıplama ve kayma animasyonları; tek klip seti üç karakterde ortak.
- Performans: havuzlama ve ön üretim, materyal başına birleştirilmiş mesh'ler. Ölçümde oynanış sistemleri koşu sırasında bellek ayırmıyordu (aksesuar, mağaza ve kıvrımlı dünyadan önce ölçüldü).

## Eksikler

Hikâyede ve tasarımda olup yapılmayanlar:

- **Ses ve müzik:** hiç yok.
- **Gerçek modeller:** üç meslek dışında her şey (İK-9000, meslek robotları, engeller, güçlendirmeler, zemin) hâlâ geçici küp ve silindir.
- **Eksik karakter animasyonları:** tökezleme, yakalanma, şerit değiştirme, sandalyede oturma, dronla uçma. Şimdilik tökezlemede koşu sürer, yakalanınca bekleme pozuna geçer, uçarken zıplamanın tepe pozunda durur.
- **Güçlendirmeler:** Prompt Injection, Fişi Çek.
- **Meslek:** Şoför (ve başka meslekler).
- **Diğer dünyalar** ve dünyalar arası geçiş: otonom şehir, lojistik deposu, veri merkezi, Analog Kasaba.
- **Final sürprizi:** yeni robot modelinin gelişi, İK-9000'in oynanabilir olması.
- Görevler, günlük ödül, başarımlar, lider tablosu.
- Reklam ve uygulama içi satın alma.
- Öğretici (ilk açılışta nasıl oynanır).
- Ayarlar ekranı (şu an yalnızca dil düğmesi var).

## Bilinen sorunlar ve doğrulanmamışlar

- **Android / iOS build'i hiç alınmadı.** Her şey yalnızca editörde denendi; mobil render ayarlarıyla da denenmedi.
- **Yeni butonlar** (mağaza, koleksiyon, devam) testlerde sinyalle tetiklendi; gerçek tıklamayla uçtan uca denenmedi.
- **Çift dokunuş** (aksesuar) arayüzdeki çift dokunuşlarda da tetiklenebilir.
- **Uçuş kamerası** (`cameraFlightFollow = 0.8`) görsel olarak doğrulanmadı.
- **Halüsinasyon** sırasında hayalet engel kısa süre kameranın önünü kapatabiliyor.
- **Kıvrımlı dünya:** parçacık efektleri bükülmüyor; ekran kenarında nesnelerin aniden belirip belirmediğine hareket halinde bakılmadı. Zemin dilimlendiği için üçgen sayısı arttı, ölçülmedi.
- **Giriş kartı** devam ekranında ve İngilizce arayüzde gözle kontrol edilmedi (mağaza ve üst barda Türkçe kontrol edildi).

- **Karakter modelleri** yalnızca editörde, Play modunda denendi (bekleme, koşu, zıplama, kayma; üç karakter). Cihazda, ofis sandalyesi takılıyken ve dron uçuşunda gözle kontrol edilmedi. Animasyon eklendikten sonra kare başına bellek ayırma yeniden ölçülmedi.
- **Kayma pozu** çarpışma kapsülünden (0,8 m) alçak ama yana doğru geniş; Çerez Duvarı'nın altından geçerken görsel taşma olup olmadığına bakılmadı.
- **Karakter FBX'leri** Meshy'den geldiği gibi duruyor ve içlerinde kullanılmayan 2K doku gömülü (dosya başına ~3,5 MB, toplam ~20 MB depo yükü). Build'e girmez; depoyu küçültmek için Blender'dan dokusuz yeniden dışa aktarılabilir.

## Yayın öncesi yapılacaklar

- Paket kimliği hâlâ şablonun varsayılanı (`com.UnityTechnologies...`), şirket adı `DefaultCompany`, sürüm `0.1.0`. Değişmeli.
- Uygulama ikonu ve açılış ekranı yok.
- `Assets/TextMesh Pro` kullanılmıyor ve depoda izlenmiyor; silinebilir.
- `Assets/Scenes` (şablondan kalan örnek sahne) gereksizse build listesinden ve projeden çıkarılmalı.
- Kayıt PlayerPrefs'te; cihaz değişince kaybolur. Bulut kaydı düşünülmeli.
- Yerelde birleştirilmiş eski dallar duruyor: `oynanis-arayuz-otomasyon`, `aksesuar-magaza-menu`, `giris-karti-kivrimli-yol`.

## Geçici içerikten gerçek içeriğe geçiş

Karakterler için geçiş yapıldı: `CharacterModelBuilder` gerçek prefab'ı kurar, üretici de gerçek prefab varsa onu bağlar (`CharacterModelBuilder.PrefabOr`). Diğer içerik için durum aşağıdaki gibi.

Şu an prefab, materyal, tanım asset'i ve sahne `PlaceholderContentBuilder` ile koddan üretiliyor ve her çalıştırmada katalog ile sahne baştan yazılıyor. Gerçek modeller gelince:

1. Modeli içe aktar, materyalini `Out of Office/Curved Lit` shader'ına al, prefab'ını `Assets/_Game/Prefabs` altında (Placeholder dışında) kur.
2. İlgili tanım asset'indeki `prefab` alanına bağla.
3. Üretici bu alanı yeniden geçici prefab'a çevirmesin diye üreticideki o satırı "gerçek prefab varsa onu kullan" şekline çevir (karakterlerdeki `PrefabOr` kalıbı). Karakterler dışındaki içerik için bu henüz yapılmadı.
