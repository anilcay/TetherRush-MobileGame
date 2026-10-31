# Tether Rush — Sanat ve Animasyon Hazırlığı

Durum: Hazırlık belgesi; gerçek varlık üretilmedi. Yönetici ajan `C:/Program Files (x86)/Steam/steamapps/common/Blender/blender.exe` yolunda Blender executable bulunduğunu bildirdi; dosya sürüm bilgisi 5.2 olarak okundu, uygulama çalıştırılmadı. Bu belge Blender veya Unity çalıştırıldığı, paket kurulduğu ya da import testi yapıldığı anlamına gelmez. İlk hedef kullanıcı onayıyla Android telefon ve dikey ekrandır (D-006).

Kaynak: `Docs/Source/GDD-extracted.txt`, GDD V2.0, 31.10.2026. Aşağıdaki **ÖNERİ** maddeleri onaylanmış üretim kararları değildir; önemli kararlar ana yönetim sohbetinde kullanıcı onayına sunulur.

## GDD ile sabit görsel ve mekanik çerçeve

- Basit geometriler, koyu kontrastlı parkur ve ölçülü neon efektler kullanılacak. Hafif yukarıdan takip eden arka kamerada sıradaki engel ve iki küre arasındaki bağ okunabilmeli.
- Çekirdek A, çekirdek B, enerji bağı, tehlike ve Pulse hedefi ayrı görsel rollerdir. Kaynak metinde sayısal renk kodları yoktur; bu belgede onaylı palet veya renk kodu tanımlanmamıştır.
- Yönetici ajanın PDF görsel inceleme gözlemi: çekirdek A ve enerji bağı turkuaz, çekirdek B mor, tehlike pembe/kırmızı, Pulse hedefi sarı/altın görünür. Bu nitel renk referansıdır; sayısal renk kodu veya ayrıca onaylanmış üretim paleti değildir.
- İki küre merkezden simetrik açılır ve kapanır. Normal ölümcül engeller enerji bağını durdurmaz; kürelerden birinin ölümcül çarpışması denemeyi bitirir.
- Enerji parçacığı bağ temasıyla, bonus kristal küre temasıyla toplanır. Görseller bu iki toplama koşulunu karıştırmamalı.
- Dolu enerjide bırakma, daralmayla birlikte bir kez Pulse üretir ve enerjiyi tüketir. Ayrı Pulse düğmesi yoktur. Pulse yalnızca kısa menzildeki özel işaretli hedefleri etkiler; normal ölümcül engelleri yok etmez.
- Bağ esnemesi ve ışık izi, toplamada bağ boyunca akış, hazır durumda renk değişimi/hafif titreşim ve parlama, Pulse sırasında kısa şok dalgası, çarpışmada kısa ve okunabilir tepki gerekir.

## ÖNERİ — Blender → FBX → Unity teslim akışı

1. **Kaynak:** Düzenlenebilir `.blend` dosyası ana sanat kaynağı olsun. Yalnız gerekli mesh, pivot ve varsa gerçekten gerekli animasyonlar kaynakta bulunsun. Unity'ye doğrudan `.blend` üzerinden otomatik dönüşüm yerine kontrollü FBX teslimi yapılsın.
2. **Ölçek ve yön:** Blender'da Metric / Unit Scale `1.0`; 1 Blender birimi = 1 metre kabul edilsin. Unity hedefi `+Y` yukarı, `+Z` parkur ilerleyişi, `+X` sağ olsun. Blender kaynağı `+Z` yukarı çalışsın; ihracatta `Forward: -Z`, `Up: Y`, Scale `1.0` başlangıç profili denensin. Blender/Unity sürümleri doğrulanmadan bu profil kesin kabul edilmesin; çift eksen dönüşümü uygulanmasın.
3. **Dönüşümler:** Mesh ölçeği ve dönüşü export öncesi uygulansın; negatif ölçek ve beklenmedik parent dönüşümleri temizlensin. Pivot amacı korunarak origin ayarlansın. Gerçek oyun ölçüleri, küre çapı, geçit genişliği ve Pulse menzili kod/bölüm tasarımıyla birlikte onaylansın.
4. **Pivotlar:** Küre origin'i merkezde; parkur parçası origin'i giriş orta noktasında ve zemin seviyesinde; bariyer origin'i taban ortasında; dönen kol origin'i gerçek dönme ekseninde olsun. Kapının hareketli kanadı ayrı mesh ve menteşe/kayma ekseniyle teslim edilsin. İki küreyi aynı mesh içinde birleştirmekten kaçınılsın.
5. **Kontrollü FBX:** Yalnız seçili teslim nesneleri dışa aktarılsın. Kamera/ışık ve gereksiz yardımcılar hariç tutulsun. Statik varlıklarda animasyon bake kapalı olsun. Gerekli bir obje klibi varsa yalnız o klip açık aralık ve örnekleme bilgisiyle ayrı teslim edilsin. Export profili Blender sürümüyle kaydedilsin; dosya adında sürüm yönetimini zorlaştıran `final_final` benzeri ekler kullanılmasın.
6. **Unity import:** Import ölçeği ve yönü önce bir küp/küre referansıyla doğrulansın; 1 metrelik referans Unity'de 1 birim olmalı. Statik varlıklarda Rig/Animation kapalı olsun. Normaller, sert/yumuşak kenarlar, gerekiyorsa tangent üretimi ve materyal slotları görsel olarak kontrol edilsin. Materyaller Unity'de paylaşılan az sayıda materyale bağlansın; Blender shader görünümünün aynen taşındığı varsayılmasın.
7. **Prefab ve kabul:** Görsel mesh, oyun collider'ı ve davranış ayrı sorumluluklar olarak prefab içinde birleştirilsin. Kaynak dosyadan tekrar export/import denemesi, pivot/eksen/ölçek, eksik materyal, mobil kamera silüeti ve collider hizası kabul kontrolüne dahil edilsin. Collider boyutları dekoratif glow'a göre belirlenmesin.

### ÖNERİ — Adlandırma ve konumlar

Örnek varlık kimlikleri: `Core_A`, `Core_B`, `Track_Straight`, `Obstacle_CenterBarrier`, `Obstacle_NarrowPass`, `Obstacle_MovingWall`, `Obstacle_RotatingArm`, `Pickup_Energy`, `Pickup_BonusCrystal`, `Pulse_BreakableBlock`, `Pulse_EnergyGate`, `Finish_Marker`.

- Kaynak: `ArtSource/Blender/<AssetId>.blend`
- Teslim: `Assets/TetherRush/Art/Models/<AssetId>.fbx`
- Mesh: `SM_<AssetId>`; materyal: `M_<Role>`; prefab: `PF_<AssetId>`.
- Gerekli obje animasyon klibi: `AN_<AssetId>_<Action>`; efekt: `VFX_<Action>`.

Bu yollar yalnız öneridir; bu hazırlıkta klasörler veya varlık dosyaları oluşturulmamıştır.

## ÖNERİ — Küçük başlangıç varlık listesi

| Grup | İlk teslim kapsamı | Amaç |
| --- | --- | --- |
| Çekirdekler | Bir ortak küre mesh'i, A/B görünüm varyasyonu | İki çekirdeği takip edebilmek |
| Parkur | Düz zemin modülü ve kenar elemanı | Koyu, sade ve okunabilir rota |
| Tehlikeler | Dar geçit yan bloğu, orta bariyer, hareketli duvar, dönen kol | Daral/genişle/zamanla kararları |
| Toplanabilirler | Enerji parçacığı ve bonus kristal | Bağ teması ile küre temasını ayırmak |
| Pulse hedefleri | İşaretli kırılabilir blok ve enerji kapısı | Sadece Pulse ile etkileşen nesneleri belirtmek |
| Bitiş | Sade bitiş işareti | Başarı noktasını açık göstermek |
| Efekt parçaları | Gerekirse basit halka/fragment mesh'i | Kod/efekt sistemiyle Pulse ve kırılma sunumu |

## ÖNERİ — Tehlike ve Pulse hedefi okunurluğu

- Renge ek olarak biçim ve işaret kullanılsın: Pulse hedefinde tekrar eden bir hedef sembolü/enerji yuvası; normal tehlikede farklı ve sabit bir silüet dili önerilir. Sembol ve palet onay bekler.
- Normal engeller Pulse sırasında kaybolma, kırılma veya devre dışı kalma izlenimi vermesin. Pulse hedefleri etkileşim öncesi ve sonrası belirgin iki duruma sahip olsun.
- Enerji parçacığı bağ hattına hitap eden görünüme, bonus kristal farklı silüete sahip olsun. Glow ve ışık izi gerçek nesne sınırlarını saklamasın.
- Hazır durumu yalnız renk değişimine bağlı kalmasın; GDD'deki parlama ve hafif titreşimle desteklensin. Hareketli duvarın yönü ve dönen kolun geçiş aralığı efektler altında kaybolmasın.
- Pulse halkasının görünen yayılımı kodun kısa etki menziliyle eşleşsin; dekoratif yayılım erişilemeyen bir hedefi etkilenebilir göstermesin. Renkler ve okunurluk gerçek oyun kamerasında, küçük ekran ölçeğinde incelensin.

## ÖNERİ — Mobil geometri ve materyal başlangıç bütçeleri

Aşağıdaki sayılar GDD şartı değildir; hedef cihaz, render pipeline ve performans ölçümü henüz belli olmadığından prototip varsayımlarıdır. Onay ve cihaz profili sonrası revize edilmelidir. Üçgenler Unity import sonrası sayılır.

| Varlık / sistem | Başlangıç varsayımı |
| --- | --- |
| Tek küre | 300–800 üçgen; yakın kamerada silüet yeterliliği kontrolü |
| Basit engel veya parkur modülü | 12–300 üçgen |
| Kapı veya kırılabilir blok bütünü | 300–1.200 üçgen; kırılma parçaları dahil kontrol |
| Bonus kristal | 24–120 üçgen |
| Ekranda görünen toplam geometri | İlk prototipte 50.000 üçgenin altında başlangıç hedefi |
| Materyaller | Mesh başına tercihen 1 slot; sahnede yaklaşık 6–10 paylaşılan materyalle başlangıç |
| Dokular | Öncelik düz renk/vertex color; gerekirse 512–1.024 px ortak atlas |
| Efektler | Küçük sayıda kısa ömürlü parçacık; transparan katman ve ekran kaplaması ölçülerek sınırlandırılsın |

ÖNERİ: Neon görünümü öncelikle emisyon ve kontrollü efektlerle kurulsun. Gerçek zamanlı ışık, bloom, gölge ve saydam materyal maliyeti cihazda ölçülmeden gerekli kabul edilmesin. Basit collider'lar tercih edilsin; görsel mesh collider zorunluluğu yaratılmasın. Bu bütçeler FPS garantisi vermez.

## ÖNERİ — Animasyon görevleri ve sorumluluk sınırı

| Görev | Sanat / animasyon hazırlığı | Kodun yönettiği davranış |
| --- | --- | --- |
| Küreler | Açılma/daralma sırasında okunur küçük yüzey/emisyon tepkisi; gerekirse dekoratif dönüş | İleri hareket, simetrik ayrılma, mesafe sınırları, yumuşatma ve çarpışma konumu |
| Tether | Hat kesiti, parlama, ışık izi ve toplama akışı görünümü | İki uç konumu, güncel bağ hattı, toplama testi ve enerji değeri |
| Ready | Renk/parlama geçişi ve hafif titreşim sunumu | Enerji dolu durumu, hazır giriş/çıkış olayları; sahte hazır sinyali verilmemesi |
| Pulse | Kısa halka/şok dalgası ve vurgu; okunur sönüş | Bırakma koşulu, tek tetik, enerji tüketimi, menzil ve hedef seçimi |
| Gate / kırılabilir blok | Kapı açılma sunumu, az sayıda kırılma parçası ve kapanış durumu | Etkinleşme koşulu, collider açılma anı ve geçiş izni |
| Hareketli duvar / dönen kol | Doğru pivot ve hareket yönünü anlatan görsel ipucu | Hareket eğrisi, hız, faz ve gerçek tehlike konumu |
| Collision / finish | Kısa darbe ve ayrı başarı vurgusu | Deneme bitişi, giriş kilidi, iki kürenin bitiş kontrolü ve skor olayı |

ÖNERİ: Mekanik zamanlama kod tarafından yönetilsin. Görsel efektler enerji/ready/Pulse/isabet/bitiş olaylarını dinlesin; animasyon klibinin sonu veya Animation Event hasar, enerji tüketimi ya da kapı geçiş yetkisinin tek otoritesi olmasın. Hareketli collider ve görünüm aynı kod durumundan beslensin. Efekt süresi, hazır titreşimi, gate açılma süresi ve çarpışma tepkisi için kesin sayılar GDD'de verilmediğinden burada sabitlenmemiştir.

ÖNERİ: Küreler ve mekanik nesneler için humanoid rig veya humanoid Animator gerekmez. Basit transform, materyal parametresi, çizgi/mesh ve parçacık sunumu yeterliyse bu yol kullanılsın. Ancak gerçek bir deformasyon/klip ihtiyacı doğrulanırsa uygun obje animasyonu veya Generic rig ayrı görev olarak açılabilir.

## Onay ve doğrulama bekleyenler

Sayısal palet ve semboller; hedef cihaz ve performans hedefi; Unity/render pipeline ile Blender sürümleri; bulunan Blender executable'ın çalışabilirliği; dünya ölçüleri ve kamera; FBX profil testi; geometri/materyal bütçeleri; kaynak/teslim yolları ve efekt süreleri. Bu belge üretim başlangıcı onayı yerine geçmez.
