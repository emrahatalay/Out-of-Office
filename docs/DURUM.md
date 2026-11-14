# Durum

Son güncelleme: 14 Kasım 2026. Bir iş bitince ya da yeni bir sorun bulununca bu dosyayı güncelle.

## Yapılanlar

- Çekirdek koşu: üç şerit, zıplama, kayma, girdi tamponu, hızlanma, zorluk eğrisi.
- Kovalayan robot (İK-9000): tökezlemede yaklaşır, ikinci çarpışmada yakalar.
- Öğrenen engel yönetmeni (şerit alışkanlığına göre engel dizer).
- 5 engel, 2 meslek robotu, 8 güçlendirme (liste `docs/HIKAYE.md`'de).
- 3 meslek (Yazılımcı, Grafiker, Stajyer), 2 yetenek.
- Meslek otomasyonu ve artan bedelle geri alma.
- Aksesuar (Ofis Sandalyesi), sürpriz paketler, giriş kartıyla devam, altınla mağaza.
- Arayüz: sekmeli menü (Ana Sayfa, Koleksiyon, Mağaza), HUD, duraklatma, oyun sonu. Türkçe ve İngilizce.
- **Arayüz teması (UI kit):** koyu dış çizgili, tabanlı, parlamalı panel ve butonlar; hepsi koddan üretilir (`UiKitBuilder`). Meslek portreleri gerçek karakter modellerinden (`PortraitBuilder`), eşya görselleri gerçek eşya modellerinden çekilir (`ItemIconBuilder`); giriş kartı temayla çizilir. Altın, giriş kartı ve sandalye her yerde aynı görselle gösterilir.
- His: kamera takibi, görüş açısı, sarsıntı, ağır çekim, parçacık efektleri.
- Kıvrımlı dünya (yol sağa/sola kıvrılır, uzakta aşağı eğilir).
- **Bütün modeller gerçek** (Meshy ile üretildi; kural `docs/GORSEL_DIL.md`): üç meslek ve İK-9000 iskeletli; 5 engel, 2 meslek robotu, altın, 7 güçlendirme, sürpriz paket, ofis sandalyesi, taşıyıcı dron ve dekor (kolon, masa, pano, iki kule). Yalnızca zemin ve bariyer koddan kurulur.
- Karakter animasyonları: bekleme, koşu, zıplama, kayma, tökezleme, yakalanınca düşme, sandalyede oturma, drona asılma. Tek klip seti bütün karakterlerde ortak; İK-9000 koşu ve beklemeyi kullanır.
- Model hattı (`Tools/ModelPipeline`): ölçek, yön, doku boşluğu doldurma, alfada ışıma maskesi, dokusuz FBX.
- Sahne havası: gündüz gökyüzü (bulutlu), güneş ve dolgu ışığı, ufuk rengi sis, açık zemin, şeker renkli binalar (aynı iki kule modeli köşe rengiyle boyanır), krem kolonlar, ölçülü post-processing.
- Ses efektleri: zıplama, kayma, şerit değiştirme, altın, güçlendirme, tökezleme, kalkan kırılması, yakalanma, düğme (`SfxSystem`, `SfxLibrary`; klipler `Tools/Audio/make_sfx.py` ile sentezlenir).
- Performans: havuzlama ve ön üretim, materyal başına birleştirilmiş mesh'ler. Ölçümde oynanış sistemleri koşu sırasında bellek ayırmıyordu (aksesuar, mağaza ve kıvrımlı dünyadan önce ölçüldü).

## Eksikler

Hikâyede ve tasarımda olup yapılmayanlar:

- **Müzik** ve ses ayarı (kısma, kapatma) yok. Ses efektleri sentezlenmiş ilk sürümdür; kulakla dinlenip ayarlanmadı (aşağıya bak).
- Menü sekmeleri düğme sesi çıkarmaz (istek sinyali atmadıkları için).
- **Eksik animasyonlar:** şerit değiştirme (yalnızca gövde yatması var), İK-9000'in yakalama hareketi (başına gelip bekler), engellerin kendi hareketi (dron pervanesi, taksi tekerleği dönmüyor).
- **Zemin ve bariyer** hâlâ düz renkli, koddan kurulan kutular; dokulu gerçek zemin yok.
- **Güçlendirmeler:** Prompt Injection, Fişi Çek.
- **Meslek:** Şoför (ve başka meslekler).
- **Diğer dünyalar** ve dünyalar arası geçiş: otonom şehir, lojistik deposu, veri merkezi, Analog Kasaba.
- **Final sürprizi:** yeni robot modelinin gelişi, İK-9000'in oynanabilir olması.
- Görevler, günlük ödül, başarımlar, lider tablosu.
- Reklam ve uygulama içi satın alma.
- Öğretici (ilk açılışta nasıl oynanır).
- Ayarlar ekranı (şu an yalnızca dil düğmesi var).

## Bilinen sorunlar ve doğrulanmamışlar

- **Android çıktısı alındı (14 Kasım 2026), cihazda henüz çalıştırılmadı.** `AndroidBuild` aracıyla "Android Test" profilinden: IL2CPP, ARM64, 57,5 MB APK, 4,7 dakika, hata yok. Derleme uyarıları oyunun kodundan değil paketlerden geliyor (`com.unity.ai.inference` shader'ı, TextMesh Pro). Açılıyor mu, kare hızı, dokunmatik girdi, çentik, ses: hepsi cihazda denenmeli. iOS çıktısı hiç alınmadı.
- **Çıktı almak proje ayarlarına dokunuyor:** URP asset'lerindeki ön eleme alanları ve `ProjectSettings` (ön yüklenen asset'ler) güncellenir; bunlar commit edildi. `UnityConnectSettings`'teki `m_Enabled` çıktı sırasında 1'e döndü, geri alındı (servisler kapalı kalsın diye); tekrar dönerse bilerek karar verilmeli.
- Çıktıdan sonra editörün etkin platformu Android'dir (etkin profil "Android Test").
- **Yeni butonlar** (mağaza, koleksiyon, devam) testlerde sinyalle tetiklendi; gerçek tıklamayla uçtan uca denenmedi.
- **Çift dokunuş** (aksesuar) arayüzdeki çift dokunuşlarda da tetiklenebilir.
- **Uçuş kamerası** (`cameraFlightFollow = 0.8`) görsel olarak doğrulanmadı.
- **Halüsinasyon** sırasında hayalet engel kısa süre kameranın önünü kapatabiliyor.
- **Kıvrımlı dünya:** parçacık efektleri bükülmüyor; ekran kenarında nesnelerin aniden belirip belirmediğine hareket halinde bakılmadı. Zemin dilimlendiği için üçgen sayısı arttı, ölçülmedi.
- **Giriş kartı** devam ekranında ve İngilizce arayüzde gözle kontrol edilmedi (mağaza ve üst barda Türkçe kontrol edildi).

- **Modeller ve animasyonlar** editörde, Play modunda gözle denendi: koşu, zıplama, kayma, tökezleme, yakalanma, sandalye, dron uçuşu, üç karakter. **Gerçek cihazda denenmedi.**
- **Bellek ayırma (14 Kasım 2026, editör, `PerfProbe`):** kare başına ortanca ~45 KB; animasyonlu gerçek modellerle, animasyonsuz küp modelle ve menüde (koşu yokken) aynı. Yani bu editörün kendi yükü; animasyon ve yeni modeller ölçülebilir ek ayırma yapmıyor. Oyunun payını editörden ayıran ölçüm (build'de profil) yapılmadı.
- **Çizim yükü (editör):** koşuda ~150-190 draw call, 32 set-pass, ~220-250 bin üçgen (gölge ve derinlik geçişleri dahil). Zemin parçası başına ~13.000 üçgen dekor var; düşük uçlu cihazda ağır gelirse `BuildDecor`'daki dekor sayısı azaltılır.
- **Sandalyede oturuş** üç karakterde üstten ve yandan bakılarak doğrulandı (14 Kasım 2026): gövde şeridin ortasında (yana sapma ≤ 1 cm), sandalye 22 cm geride. Yazılımcı'nın sırt çantası sırtlığa birkaç cm girer.
- **Kayma pozu** çarpışma kapsülünden (0,8 m) alçak ama yana doğru geniş; Çerez Duvarı'nın altından geçerken görsel taşma olup olmadığına bakılmadı.
- **Havada yakalanma:** görsel yere indirilir ama düşüş animasyonu yoktur, bir karede iner.
- **Meslek robotlarının kolları** çarpışma kutusundan (1,5 m) taşar (model ~2,2 m geniş); kola değmek çarpışma sayılmaz.
- **Parçacıklar** kıvrımlı dünyayla bükülmüyor (kameraya yakın oldukları için göze batmıyor).
- **Ses efektleri (14 Kasım 2026)** yalnızca programla doğrulandı: Play'de zıplama, şerit değiştirme ve düğme olaylarında doğru klibin doğru düzeyde çaldığı okundu, dosyaların düzeyi ölçüldü. **Kimse kulakla dinlemedi;** tını ve ses düzeyi dengesi (`SfxLibrary`) dinlenerek ayarlanmalı.
- **Gündüz sahnesi ve yeni arayüz (14 Kasım 2026)** editörde ekran görüntüsüyle denendi: menü (TR/EN), koleksiyon, mağaza, HUD, duraklatma, oyun sonu, otomatikleşmiş meslek. Devam teklifi (giriş kartıyla) ve paket ödülleri satırı yeni temayla görülmedi. Düğmeler gerçek dokunuşla değil sinyal ve "gönder" olayıyla tetiklendi. Cihazda denenmedi.
- **Play modunda USS / UXML kaydedilirse** arayüz kaybolur (Unity arayüzü yeniden kurar, sunucuların bağı kopar). Yalnızca editörde olur; Play'i yeniden başlat.
- **Aksesuar ikonu** (`.icon-chair`) USS'te sabit sandalye görselidir. İkinci bir aksesuar eklenirse görsel veriden gelmelidir (`AccessoryChangedSignal` kimlik taşımıyor).
- **Binalar** parça başına dört renkte, 30 m'de bir aynı sırayla tekrar eder.
- **Zemin mesh'leri** artık silinip yeniden oluşturulmuyor, yerinde güncelleniyor: eski yöntem ara sıra prefab'daki mesh bağını koparıyordu (zemin görünmüyordu). Üç ardışık üretimde doğrulandı.
- **Karakter FBX'leri** (4 model + 3 klip dosyası) Meshy'den geldiği gibi duruyor ve içlerinde kullanılmayan 2K doku gömülü (dosya başına ~3,5 MB, toplam ~30 MB depo yükü). Build'e girmez; iskelet bozulmasın diye yeniden dışa aktarılmadı. Eşya modelleri dokusuzdur (toplam ~6 MB).
- **Kaynak GLB'ler depoda yok;** `Tools/ModelPipeline/props.json`'daki Meshy görev kimlikleriyle yeniden indirilir (Meshy görevleri bir süre sonra silinebilir).

## Yayın öncesi yapılacaklar

- Paket kimliği hâlâ şablonun varsayılanı (`com.UnityTechnologies...`), şirket adı `DefaultCompany`, sürüm `0.1.0`. Değişmeli.
- Uygulama ikonu ve açılış ekranı yok.
- `Assets/TextMesh Pro` kullanılmıyor ve depoda izlenmiyor; silinebilir.
- `Assets/Scenes` (şablondan kalan örnek sahne) gereksizse build listesinden ve projeden çıkarılmalı.
- Kayıt PlayerPrefs'te; cihaz değişince kaybolur. Bulut kaydı düşünülmeli.
- Yerelde birleştirilmiş eski dallar duruyor: `oynanis-arayuz-otomasyon`, `aksesuar-magaza-menu`, `giris-karti-kivrimli-yol`.

## Geçici içerikten gerçek içeriğe geçiş

Modeller için geçiş yapıldı: `CharacterModelBuilder` ve `PropModelBuilder` gerçek modelleri hazırlar; üretici çarpışma kutusunu koyar ve görsel olarak gerçek modeli kullanır (yoksa geçici küpe düşer). "Placeholder" adı klasörlerde ve sınıfta kaldı; artık yalnızca zemin, bariyer ve yedek küpler geçicidir.

Prefab, tanım asset'i ve sahne hâlâ `PlaceholderContentBuilder` ile koddan üretiliyor ve her çalıştırmada katalog ile sahne baştan yazılıyor. Bu akıştan çıkıp elle düzenlenen prefab'lara geçilecekse:

1. Modeli içe aktar, materyalini `Out of Office/Curved Lit` shader'ına al, prefab'ını `Assets/_Game/Prefabs` altında (Placeholder dışında) kur.
2. İlgili tanım asset'indeki `prefab` alanına bağla.
3. Üretici bu alanı yeniden geçici prefab'a çevirmesin diye üreticideki o satırı "gerçek prefab varsa onu kullan" şekline çevir (karakterlerdeki `PrefabOr` kalıbı). Karakterler dışındaki içerik için bu henüz yapılmadı.
