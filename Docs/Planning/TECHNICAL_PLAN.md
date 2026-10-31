# Tether Rush — Teknik Hazırlık Planı

Güncel durum: Kullanıcı D-008/009/010/014 ile Unity 6000.3.19f1 + URP, ilk prototip kapsamı ve kapalı kapıda başarısızlık davranışını onayladı; uygulama başladı. Android + dikey ekran onaylı. Bu belge ilk mimari değerlendirmeyi korur; güncel onay ve prototip ayarları için DECISIONS.md esas alınır. Alt bölümlerdeki eski 'onay bekliyor' notları bu kaydın yerine geçmez.
Kaynak: `Docs/Source/GDD-extracted.txt`, Tether Rush GDD V2.0, 31.10.2026.
Bu planın kendisi bir test sonucu değildir. GDD'deki sabit kuralları değiştirmez. Onay kaydında kesinleşmeyen mimari ve üretim seçimleri öneri olarak kalır. Çözümlenen temel sürümler: URP 17.3.0, Input System 1.19.0, Unity Test Framework 1.6.0. Yerel eski şablondaki Input System 1.12.0, bu Editor'de ReservedCFE derleme hatası verdiğinden Editor paket manifestinin önerdiği 1.19.0 kullanıldı.

## 1. Değiştirilmeyecek oynanış sözleşmesi

- Tek dokunma kontrolü vardır; ek Pulse düğmesi yoktur. Basılı tutmak iki küreyi açar, bırakmak daraltır.
- Küreler sabit tempoyla otomatik ilerler. Ortak merkez çevresinde simetrik açılır; oyuncu merkezi yönlendirmez. İki mesafe sınırı arasında geçiş akıcıdır.
- Enerji doluyken gerçekleşen gerçek bir bırakma kenarı, daralmaya ek olarak tam bir Pulse üretir ve enerjiyi tüketir. Enerji dolu değilse yalnız daralma olur. Parmağın bırakılmış kalması tekrar Pulse üretmez.
- Bağ yalnız enerji parçacığı toplar. Standart duvarlar bağın hareketini engellemez; bağ bonus kristal toplamaz ve ölümcül engel çarpışması üretmez.
- Kürelerin toplama etkileşimi yalnız bonus kristallerle, ölümcül çarpışma etkileşimi yalnız fatal engellerledir. Küre temasından enerji kazanılmaz.
- İki küreden herhangi birinin fatal teması denemeyi bitirir. Başarı için iki kürenin de bitişe ulaşması gerekir.
- Pulse kısa menzilde yalnız işaretli kırılabilir blokları ve enerji kapılarını etkiler. Normal hazard Pulse tarafından silinmez, kapatılmaz veya etkisizleştirilmez.
- Bonus kristaller skoru artırır; Pulse için gerekli değildir. Yıldız değerlendirmesi GDD'de olasılık olarak geçer, kesin kapsam değildir.

## 2. Sürüm ve platform kararı

**ÖNERİ / ONAY BEKLİYOR:** Proje editörünü kurulu `Unity 6000.3.19f1` sürümüne kilitlemek. Bu kurulumda AndroidPlayer bulunması hazırlık maliyetini azaltır. Kullanıcı ilk hedefi **Android telefon + dikey ekran** olarak onayladı (D-006). `6000.3.12f1` ve `6000.3.9f1` ile aynı projeyi dönüşümlü açmamak.

**ÖNERİ / ONAY BEKLİYOR:** Mobil 3D için URP kullanmak. Kesin URP paket sürümü şu an doğrulanmadı; rakam uydurulmamalı. Proje oluşturma onayı sonrasında seçilen editörün URP şablonuyla çözümlenen uyumlu paket sürümü incelenip onaylanmalı; ardından `Packages/manifest.json`, `Packages/packages-lock.json` ve `ProjectSettings/ProjectVersion.txt` sürüm kontrolüne alınmalı. Paket yükseltmeleri ayrı karar ve doğrulama gerektirir.

**ONAYLI:** Android telefon ve dikey ekran. **DAHA SONRA BELİRLENECEK:** minimum işletim sistemi, hedef cihaz sınıfı, güvenli alan test cihazları, kare hızı hedefi ve mağaza dağıtım kapsamı. iOS ilk prototip kapsamında değildir.

## 3. Önerilen mimari ve sorumluluk sınırları

ÖNERİ: Küçük, açık sorumlulukları olan MonoBehaviour adaptörleri ile Unity'den bağımsız test edilebilir C# durum mantığını birleştirmek. İlk prototip için ECS, genel servis çerçevesi veya kapsamlı olay altyapısı kurmamak.

| Bileşen | Sorumluluk ve sınır |
| --- | --- |
| RunState | Hazırlık, oynama, duraklama, kayıp ve başarı geçişleri; terminal sonuç yalnız bir kez yayımlanır. |
| InputAdapter | Tek aktif parmağın basma, gerçek bırakma ve iptal olaylarını ayırır; ham Unity olayını oyun komutuna çevirir. |
| PairMotor | Sabit ileri hareket, ortak merkez ve simetrik mesafe hesabı; enerji veya skor bilmez. |
| EnergyState | Enerji kapasitesi, ekleme, doluluk ve atomik tüketim; görsel efekt veya dokunmatik API bilmez. |
| ReleaseResolver | Geçerli bırakmayı bir kez işler; o anda dolu enerji varsa bir Pulse isteği ve tüketim üretir. |
| InteractionResolver | Küre, bağ ve Pulse temaslarını türlerine göre doğrular; tekrar bildirimleri ayıklar. |
| PulseResolver | Pulse anındaki menzili sorgular; yalnız açıkça yetkili hedeflere etki uygular. |
| LevelFlow | Başlatma, tekrar deneme ve iki kürenin bitiş durumu; parkur tanımlarını kullanır. |
| Presentation | Kamera, enerji barı, skor, ses, titreşim ve efektler; oynanışın yetkili durumunu değiştirmez. |

ÖNERİ: Ayarlanabilir hareket/enerji/Pulse değerleri ve parkur tanımları için ScriptableObject verisi kullanmak; çalışma zamanı enerji ve skorunu bu varlıklara yazmamak. Sayısal hız, mesafe sınırları, geçiş süresi, enerji kapasitesi, parçacık katkısı, Pulse menzili ve kamera değerleri **ONAY BEKLİYOR**; ilk denemelerdeki değerler yalnız ÖNERİ/prototip ayarı olarak kaydedilir.

### Olay sırası ve tekrar güvenliği

ÖNERİ: Girdi komutlarını kontrollü simülasyon adımında tüketmek; hareket ve temas çözümünden sonra terminal sonucu değerlendirip sunumu güncellemek. Aynı adımda enerji toplama ile bırakmanın sırası belgelenmeli; önerilen sıra hareket/temas ile enerji kazanımı, ardından o adımın gerçek bırakmasının çözülmesidir. Bu eşzamanlılık tercihi **ONAY BEKLİYOR**.

Enerji tüketimi ve Pulse isteği tek durum geçişi olmalı. Aynı bırakma birden fazla callback ile gelse de bir Pulse oluşmalı. Aynı parçacık ve bonus kristal birden fazla collider tarafından bildirilse de yalnız bir kez toplanmalı. Aynı Pulse aynı hedefi bir kez etkiler. Kayıp/başarı sonrası bekleyen girdi ve temaslar sonuç üretmez. Aynı adımda fatal temas ve bitiş çatışması için fatal sonuca öncelik verilmesi **ÖNERİ / ONAY BEKLİYOR**.

### Odak kaybı ve duraklama

ÖNERİ: `ReleasedByPlayer` ile `CanceledBySystem` ayrı komutlar olmalı. Uygulama arka plana geçtiğinde, odak kaybında, işletim sistemi dokunmayı iptal ettiğinde veya duraklama açıldığında aktif dokunuş iptal edilir; bu olay **Pulse değildir ve enerji tüketmez**. Devam edildiğinde eski dokunuşun sonlanma callback'i gerçek bırakma kabul edilmez. Yeni bir basma görülene kadar Pulse için bırakma kabul edilmez. Böylece enerji doluyken uygulama değiştirmenin sahte Pulse üretmesi engellenir.

ÖNERİ: Duraklatmada simülasyon ve giriş durumu dondurulur; devam sonrası yeni basmaya kadar hedef mesafe dar konuma döner. Devam anındaki mesafe davranışının ayrıntısı **ONAY BEKLİYOR**; enerji korunması ve sahte release engeli teknik kabul koşuludur. İkinci parmak ilk parmağın kontrolünü devralmaz; çoklu dokunma politikası da açıkça doğrulanır.

## 4. Fizik ve etkileşim ayrımı

ÖNERİ: Kontrollü kinematik hareket ve basit collider'lar; PhysX yay sistemi ile serbestçe savrulan küreler yerine simetrisi doğrudan hesaplanan çift. Bağ görüntüsü ile bağın toplama geometrisi ayrı tutulur. Hızlı hareketlerde kaçırılmış temasları önlemek için önceki ve yeni konumlar arasındaki süpürülmüş küre/bağ geometrisi sorgulanır; gerçek cihaz maliyetine göre ayrıntılandırılır.

| Kaynak | Fatal hazard | Enerji parçacığı | Bonus kristal | İşaretli Pulse hedefi |
| --- | --- | --- | --- | --- |
| Küre | Denemeyi bitirir | Etkileşmez | Bir kez skor kazandırır | Doğrudan etkinleştirmez |
| Bağ toplama alanı | Etkileşmez | Bir kez enerji kazandırır | Etkileşmez | Etkileşmez |
| Pulse sorgusu | Etkileşmez | Etkileşmez | Etkileşmez | Menzildeyse bir kez etkiler |

ÖNERİ: Layer maskeleri bu tabloya göre dar tutulmalı; ayrıca bileşen/tür doğrulaması yapılmalı. Görsel çizginin standart duvara değmesi fiziksel engelleme yaratmamalı. Kapalı enerji kapısı ve kırılabilir bloğun küreye temasının fatal olup olmadığı GDD'de açık değil; **ONAY BEKLİYOR**. Normal hazard ile aynı yıkım arayüzüne gelişigüzel bağlanmamalılar.

### Zorunlu kapıda enerjisiz softlock riski

GDD'de enerji kapısı Pulse ister, Pulse yalnız dolu enerjiden ve bırakmayla oluşur. Zorunlu kapıdan önce enerji eksikse veya oyuncu dolu enerjiyi erken harcarsa, ilerleme kapıya kilitlenebilir. Yalnız yeterli sayıda parçacık koymak erken harcama sorununu çözmez.

ÖNERİ: Parkur doğrulaması zorunlu kapıya yaklaşımda elde edilebilir enerji miktarını, gerçek toplama geometrisini, açılma süresini, Pulse menzilini ve bırakma penceresini kontrol etmeli. İlk Pulse öğretim alanında enerji toplama ve kapı sırası tek başına denenmeli. Erken Pulse ve kaçırılan parçacık senaryoları ayrıca test edilmeli.

**ONAY BEKLİYOR:** Hatalı enerji kullanımı sonrası tasarım davranışı. Seçenekler; güvenli yeniden enerji kazanma yolu, açıkça tanımlı başarısızlık ve tekrar deneme veya kapıyı isteğe bağlı güzergâha taşıma. Hiçbiri kendiliğinden uygulanmaz. Otomatik enerji verme, kapıyı ücretsiz açma, normal hazard'ı Pulse ile kaldırma veya sonsuza kadar bekletme çözüm olarak kabul edilmez. Kapalı kapının fatal teması seçilse bile oyuncunun anlaşılır biçimde yeniden deneyebilmesi doğrulanmalıdır. Bu karar kapanmadan zorunlu kapılı bölüm üretimi ilerlemez.

## 5. Önerilen sahne ve klasör düzeni

```text
Assets/TetherRush/
  Scenes/       Bootstrap, Prototype, Gameplay
  Scripts/      Core, Input, Movement, Interactions, Level, Presentation
  Data/         Tuning, Levels
  Prefabs/      Player, Hazards, Collectibles, PulseTargets, UI
  Art/          Materials, Models, VFX
  Audio/
  Tests/        EditMode, PlayMode
```

ÖNERİ: Bootstrap ortak başlangıcı, Prototype mekanik laboratuvarını, Gameplay bölüm akışını taşır. İlk küçük prototipte yalnız gerekli sahneler açılır; bu düzen mevcut dosyaların yaratıldığı anlamına gelmez. Çekirdek mantık ve testleri ayrı assembly tanımlarıyla ayırmak; görsel varlık referanslarını sahne/Prefab düzeyinde tutmak önerilir. Parkur parçası Prefab'ları veriyle yerleştirilir; prosedürel veya sonsuz parkur GDD'de istenmediği için başlangıç kapsamına eklenmez.

## 6. Mobil performans yaklaşımı

- ÖNERİ: URP'nin sade ileri render yolu, basit geometriler, paylaşılan malzemeler ve ölçülü neon. Bloom/şeffaf VFX/gölgeler gerçek cihazda ölçülerek açılır; dolu enerji ve Pulse okunabilirliği pahalı efektlere bağımlı olmaz.
- ÖNERİ: Dinamik ışık ve gerçek zamanlı gölge sayısını sınırlamak; katmanlı saydam bağ ve Pulse efektlerinde overdraw'ı incelemek. Kesin kalite bütçeleri **ONAY BEKLİYOR**.
- ÖNERİ: Tekrarlanan parçacık, toplama ve Pulse efektlerini havuzlamak; oyun sırasında sürekli oluşturma/yok etme ve kare başına managed allocation kaynaklarını azaltmak.
- ÖNERİ: Fizik sorgularını ilgili maskelerle sınırlamak, yeniden kullanılan sonuç tamponlarını taşma kontrolüyle kullanmak. Hız ve collider boyutlarını test ederek tünelleme riskini ölçmek.
- ÖNERİ: Unity Profiler ile CPU/GPU, GC, bellek, fizik ve UI maliyetini gerçek hedef cihazda izlemek. Editör FPS'i kabul kanıtı sayılmaz. Ekran çözünürlüğü, kare süresi/bellek bütçeleri ve termal test süresi cihaz kapsamıyla birlikte **ONAY BEKLİYOR**.
- ÖNERİ: Enerji barı ve sayaçları yalnız durum değişince güncellemek; güvenli alan ve farklı ekran oranlarını onaylanan yönde kontrol etmek.

## 7. Üretim aşamaları ve karar kapıları

1. **Hazırlık ve kararlar:** Sabit kurallar, platform/yön, sürüm, kapı başarısızlığı ve ilk prototip kapsamını kullanıcıyla onaylamak. Çıktı: onaylı karar listesi; şu anki aşama.
2. **Teknik iskelet:** Ancak onaydan sonra Unity/URP projesini oluşturmak; sürümü kilitlemek, giriş ve durum sınırlarını kurmak. Çıktı: açılan boş sahne ve hedef cihazda temel build.
3. **Küçük oynanabilir prototip:** Basit geometrilerle dar geçit, orta bariyer, enerji toplama, bonus ve Pulse kapısı/kırılabilir hedef. Çıktı: aşağıdaki koşulları sağlayan kısa parkur.
4. **Etkileşim ve ritim doğrulama:** Hareketli duvar/dönen kol, kamera okunabilirliği, kapı hata senaryoları, tekrar deneme. Çıktı: GDD'nin öğretim sırasına uygun bölüm taslağı.
5. **Görsel/ses geçişi ve cihaz ölçümü:** Varlıkları yerleştirmek; geri bildirim, UI ve mobil bütçeleri doğrulamak. Çıktı: onaylanan cihazlarda ölçüm kaydı.
6. **İçerik ve sürüm adayı:** Onaylanan bölüm sayısı ve dağıtım kapsamıyla içerik üretimi, regresyon ve paketleme. Bölüm sayısı, tarihler, mağaza/servis entegrasyonları **ONAY BEKLİYOR**; reklam/analitik gibi yeni kapsamlar varsayılmaz.

## 8. İlk küçük oynanabilir prototip kabul ölçütleri

Bu kapsam ve tüm sayısal ayarlar **ÖNERİ / ONAY BEKLİYOR**; sabit GDD kurallarını sınayan kontroller değişmez.

- Basılı tutma ile simetrik açılma, bırakma ile akıcı daralma; sabit merkez ve ileri hız doğrulanır. Dar geçit ile orta bariyer aynı kontrolle geçilebilir.
- Enerji parçacığına yalnız küre dokunduğunda enerji artmaz; bağ değdiğinde bir kez artar. Bonus kristale yalnız bağ değdiğinde skor artmaz; küre değdiğinde bir kez artar.
- Eksik enerjiyle bırakma Pulse üretmez. Dolu enerjiyle gerçek bırakma tam bir Pulse üretir ve enerji tüketilir. Parmağı bırakılmış tutmak, yinelenen callback ve aynı anda iki hedef görülmesi ek Pulse üretmez.
- Menzildeki işaretli kırılabilir blok ve enerji kapısı etkilenir; menzil dışı hedef etkilenmez. Normal hazard Pulse sonrasında tehlikeli kalır.
- Her iki küre ayrı ayrı fatal çarpışmayla denemeyi bitirebilir. Bağın standart hazard içinden geçmesi tek başına kayıp oluşturmaz. Bitiş yalnız iki küre ulaştığında başarı verir.
- Enerji doluyken odak kaybı, uygulama değişimi, duraklama ve dokunma iptali Pulse veya enerji tüketimi yaratmaz. Devam sonrası yeni basma/bırakma normal çalışır.
- Erken Pulse, kaçırılmış enerji ve zorunlu kapıya eksik enerjiyle varış seçilmiş tasarım davranışına gider; deneme ne ilerleyen ne bitebilen belirsiz bir durumda kalmaz. Bu seçim henüz onaylanmadıysa ilgili kabul maddesi tamamlandı sayılmaz.
- Tekrar deneme enerji, bonus, kapı, kırılabilir blok ve giriş durumunu temiz başlangıca getirir; eski olaylar yeni denemeyi etkilemez.
- Kamera yaklaşan engeli ve Pulse hedefini okunur gösterir; enerji doluluğu ve ölüm nedeni ayırt edilir. Hedef cihaz, ekran yönü ve performans bütçesi onaylanınca gerçek cihazdaki build ölçümü kabul kaydına eklenir.

ÖNERİ: Saf durum mantığı için EditMode testleriyle enerji sınırı, tek release, tüketim ve iptal davranışını; PlayMode testleriyle etkileşim matrisi, kapı menzili, reset ve sonuç geçişlerini doğrulamak. Gerçek dokunma/arka plana alma ve görsel okunabilirlik cihazda manuel denenir. Amaç uygulamayı tekrar eden testler değil, GDD sözleşmesini ve kritik hata sınırlarını sınamaktır.
