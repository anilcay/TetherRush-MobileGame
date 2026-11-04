# Engel varyantları — entegrasyon ve QA incelemesi

04.11.2026. Salt okunur kaynak incelemesi; bu rapor öneridir, çalıştırılmış Unity testi veya görsel kabul değildir. Kapsam: ClampBarrier, CapacitorBank, HydraulicSlider, TurbineBeam mevcut dört engel rolüne yeni Blender görünümü sağlar. Dosya sahipliği yalnız bu rapor; runtime/test entegrasyonu ana yönetici, Blender kaynak/FBX ayrı sanat ajanı. Bağımlılık: dört FBX'in import edilmesi. Kabul: yeni görseller gerçek bölümlerde ve Sonsuz Akış'ta görünürken yetkili geometri, rota, faz, hız, Pulse ve kayıtlar aynı kalır.

## En güvenli deterministik eşleme

| Yeni model | Mevcut rol / değişmeyecek dünya ölçüsü | Kaynak referansı (inceleme başlangıcı) |
| --- | --- | --- |
| ClampBarrier | Narrow iki yan bloğu; her biri 2.35×1.3×1, X=±2.425 | LevelCatalog.cs:185–189 |
| CapacitorBank | Center; 2.5×1.3×1.2, X=0 | LevelCatalog.cs:191–194 |
| HydraulicSlider | MovingWall; geniş geçiş 1.3×1.3×0.9, yan geçiş 0.9×1.3×0.9 | LevelCatalog.cs:218–230 |
| TurbineBeam | RotatingArm; 5.4×1.3×0.28 | LevelCatalog.cs:232–237 |

Referanslar `Assets/TetherRush/Scripts/Runtime/Course/` altındadır. Yeni mekanik türü eklemeyin: TimedHazardKind yalnız MovingWall/RotatingArm olarak kalsın. VisualKind sonuna dört değer eklemek mevcut sayısal kimlikleri korur (Art/VisualLibrary.cs:7–10).

Yönetici entegrasyon seçimi: yalnız sunum katmanında ObstacleVisuals resolver; course.Id + dünya Z + mevcut engel türü ile sabit seçim. Yalnız BuildContent bu resolver'ı çağırır; yetkili CourseDefinition/TimedHazard verisine alan eklenmez ve legacy prototype eski görünümünü korur. Her Narrow çiftinin iki tarafı aynı varyant olmalı: key X, nesne sırası veya instance ID içermemeli. Marked/gate ve desteklenmeyen spec eski görünümü döndürmeli. Hash için C# string.GetHashCode yerine açık sabit karakter hash'i kullanın; Unity Random tüketmeyin. Dünya Z'nin gösterimini culture'a bağlı stringe dönüştürmeyin. İlk beş dahil mevcut tüm rota ve mekanik veri aynı kalır.

EndlessCourse.cs:14–17 içindeki hash/pattern/jitter ve CourseBuilder offset'i aynen korunsun. Aynı seed/index her çağrıda aynı görünümü vermeli. Resolver görsel key'inde koordinatı nicemliyorsa bunu yalnız hash key'ine uygulasın; gerçek konum/offset/faz değişmesin. Key hash'inin yeterli çeşitlilik verdiğini envanter testiyle kanıtlayın: hash tabanlı seçim belli bir rolün yeni görünümünü küçük örneklerde garanti etmez.

## Gizli entegrasyon noktaları ve riskler

- HazardSpec zaten Visual alanına sahip (CourseDefinition.cs:20–26). TimedHazard'da böyle bir alan yok (TimedHazard.cs:9–15); yönetici seçimi bunu değiştirmeden Kind uyumluluğunu doğrulayan sunum resolver'ı kullanır. Böylece Core olan enum default'u veya eksik kopyalanan yeni alan riski oluşmaz.
- TetherRushGame.cs:375 ve :422 iki ayrı timed görünüm oluşturma yolu. İlki legacy advanced course olarak eski Kind görünümünü koruyacak; ikincisi tüm katalog/sonsuz içerik olduğundan resolver'ın uygulanacağı yerdir. Static seçim de BuildContent :419'da çözülür. Course verisinde Visual alanının yeni enum'u göstermemesi bu mimaride normaldir: görünüm çeşitliliği testi resolver/gerçek instance üzerinde yapılmalıdır.
- Sonsuz stream derin kopya oluşturmaz: TetherRushGame.cs:446 taze chunk verisi üretir; :454 görünümü kurar; :458–460 Data.TimedHazards referanslarını ve TimedViews'i aynı sırayla yeni array'lere AddRange yapar. Görsel seçimini BuildContent öncesi yapın; yeniden stream toplamasında geometriyi kopyalama, offset ekleme veya fazı yeniden kurma gerekmiyor. Yeni alan sonradan kopyalanacaksa kopyaya dahil edilmeli. Emekli chunk'ın view ve verisinin aynı anda listeden çıkması korunmalı.
- ModelVisual danger-accent koşulu TetherRushGame.cs:302–303 yalnız eski beş tehlike görünümünü tanır. Dört yeni modeli ekleyin; aksi hâlde yeni modellerin Accent slotları gümüşe düşer ve ölümcül sinyal dili zayıflar. Normal modeller amber Pulse işareti almamalı. AddHazard :309 marked/gate görünümünü zaten zorlar; bunu koruyun.
- VisualLibrary.cs:20–70 tüm renderer birleşik sınırını her eksende bağımsız normalleştirir; sahnedeki fizik ölçüsü model ölçeğiyle belirlenmez. Statik Bounds TetherRushGame.cs:311, çarpışma :157–164; timed HalfExtents TimedHazard.cs:80–86 yetkilidir. Yeni FBX'in dış çıkıntıları birleşik sınırı büyütürse ana ölümcül gövde sıkışır ve boş görünen alanda ölüm oluşur. Gövdeler küre yüksekliğinde kapalı, koridor kenarı net, yüzey girintileri geçilebilir yarık izlenimi vermeyecek şekilde üretilmeli. Özellikle CapacitorBank ayrı sütunlar arasında gerçek geçit gösteriyorsa uygun değildir; TurbineBeam tüm dairesel disk olarak tasarlanmamalı, dar uzun OBB'yi okumalıdır.
- FBX: sıfır merkez, 1×1×1 birleşik sınır, unit scale, collider/Animator/clip yok; Body/Dark/Accent slot adları korunur. HydraulicSlider'ın bütün mesh'i yetkili duvarla birlikte kayar: parkura sabit görünecek ayak/rail/piston yuvası bu mesh'e eklenmemeli. TurbineBeam uzun ekseni Unity X, merkez pivot Y dönüşü olmalı. Kod :535–540 mesafe/ileri hızından PositionAt ve AngleAt alır; Unity Y açısı negatif radyan→derece dönüşümüdür. Çocuk Animator ya da wall-clock dönüş ilavesi geometriyi görünümden koparır. Pause/retry bu tek zaman kaynağını korur.

## Hedefli otomatik kontroller

1. PresentationSmokeTests.cs:42–45 `AllFourteen...` adı ve `EqualTo(14)` sabiti 18'e güncellenmeli. Enum üzerinden mevcut FBX test döngüsü yeni modellerin import, collider yokluğu, mesh, merkez ve unit sınırlarını otomatik kapsar (:46–72). Kod/araç taramasında başka sabit 14-model testi bulunmadı; tarihsel 14 model raporlarını geriye dönük değiştirmeyin.
2. Ayrı görsel sözleşme testi: dört yeni Resource yolu, normal danger Accent materyali, hedef boyuta ölçeklenmiş renderer birleşik sınırı ve pozisyonu, legacy timed görünümü. Üretim kodunu aynalayan test yerine gerçekten import edilmiş mesh ve gerçek game instance üzerinde doğrulayın.
3. Görsel eşleme testi: 25 bölüm ve farklı seed'lerde en az 20 chunk; aynı girdide aynı çözülmüş görünüm, Unity Random.state korunması, Narrow çiftinde aynı model, marked/gate ve desteklenmeyen geometride eski model, dört yeni tipin erişilebilirliği. Mevcut CourseDefinitionTests.Fingerprint (:149–156) Visual'i içermez; mevcut determinism testi tek başına görünüm regresyonunu yakalamaz. Level25DataTests.GameplaySignature (:141–154) bilerek görseli dışlar; o ayrımı koruyun. Resolver için supported spec'i yalnız Kind ile kabul etmeyin: beklenen mevcut ölçü ve normal tehlike rolünü doğrulamak, gelecekte farklı geometrinin yanlış model almasından korur.
4. Değişmeyen oynanış için baseline karşılaştırması: Id/title/length/UsesEnergy; bütün hazard Position/Size/Marked/Gate; pickup Position/Bonus; timed Kind/Center/HalfExtents/Amplitude/AngularSpeed/Phase; cue Distance/Press ve hint dizisi aynı olmalı. Mevcut fingerprint tam hareket parametrelerini içermez. Üretimden önce eski veriden alınan sabit snapshot/hash ile karşılaştırma daha güçlüdür; yalnız yeni veriyi iki kez üretmek davranış korunmasını kanıtlamaz.
5. İlgili mevcut kontroller: CourseDefinitionTests ve Level25DataTests tümü; MenuLevelFlowTests.AuthoredLevelCanCompleteAndPersistsOnlyRealCompletion (25 rota), EndlessCrossesFifteenChunksWithBoundedWorldAndRetryResets (:81), TimedHazardTests, ChallengeFlowTests.PauseFreezesHazardPoseAndRestartRestoresInitialPoseAndRun (:77), PrototypeFlowTests.FullPulseDoesNotDestroyNormalCenterHazard (:148). Yeni timed görseller için gerçek instance'ın merkez/rotation eşleşmesi, pause sabitliği ve retry başlangıç pozu ayrıca gözlenmeli; eski legacy testi tek başına yeni art seçimlerini kapsamaz.

## Gerçek render planı

Ana oyun kamerasını değiştirmeden 540×960 ve 540×1170 gerçek URP kareleri; TetherRushGame.cs:542–543 kamera `(0,10,distance−11)` noktasından `(0,0.1,distance+8)` hedefini izler. Normal oyuncu açısı esas kabul görüntüsüdür. Ek üstten/XZ kare yalnız collider-debug karşılaştırması için; ürün görünümü olarak sunulmasın.

Bölüm 9 dört rolü bir arada içerir: wall z20 ve z44 (yaklaşım mesafeleri yaklaşık 10/34), arm z68 (yaklaşım 58 ve güvenli geçiş 68), Narrow z92 (yaklaşım 82), Center z116 (yaklaşım 106). Ancak yönetici hash resolver'ının sonucu bilinmeden bunların dördünün yeni model olduğu söylenemez: önce 25 bölümün çözülmüş model envanteri çıkarılıp her yeni model için gerçek bölüm/Z kaydı seçilmeli. SafeRoute ile gerçek bas/bırak olayları yürütülerek ilerlenmeli; mesafeyi doğrudan teleport etmek zamanlama/ölüm kabulü sayılmaz. Geniş ve dar slider ölçüsü ikisi de gösterilsin. Dönen kol yatay ve eğik pozda ayrıca yakalanarak uzun eksen/pivot ve yanlış disk izlenimi kontrol edilsin. İkili Narrow'ın tamamı kadrajda olmalı; gövde/zemin ve iki küre arasındaki kontrast incelenmeli.

Sonsuz görüntüsü için başlangıç seed'i ile ilk on chunk'ın model envanteri kaydedilsin ve yeni timed modelleri içeren gerçek chunk yaklaşımında kare alınsın. Üretilen chunk jitter'ı yaklaşım mesafesine eklensin; varsayılan kalıp indeksini tahmin edip sabitlemeyin. Stream emekliliğinden sonra yeni timed modelin doğru pose ile kaldığı, tekrar başlatmada aynı model dizisinin döndüğü doğrulansın.

## Teslim durumu

Kaynak incelemesi tamamlandı; bu alt görev hiçbir Unity süreci, test, build veya render çalıştırmadı. Kod, kaynak art, .meta, Git ve PROJECT_STATUS değiştirilmedi. Ana yöneticinin yeni modellerle gerçek Unity doğrulaması, görsel inceleme ve proje durumu kaydı açık bağımlılıktır.

## Ek teslim — yeni PlayMode kontrolleri

Ana yönetici ek görevlendirmesiyle yalnız `Assets/TetherRush/Tests/PlayMode/ObstacleVariantTests.cs` oluşturuldu: **6 test**. Unity/test/build bu ajan tarafından çalıştırılmadı; test sonuçları henüz doğrulanmadı. Önceki salt okunur teslim ifadesi bu ek görev öncesini anlatır. `.meta` Unity import'u ve PROJECT_STATUS ana yöneticide kalır.

- Resolver tekrarlanabilirliği, Unity Random durumunun korunması, desteklenmeyen/marked roller ve boş course kimliği.
- 25 bölüm +20 sonsuz chunk envanteri, eşleşen dar geçit çifti, dört yeni modelin gerçek oyun instance'ındaki import mesh referansı, dünya sınırı/merkez ve gerçek Danger materyal rengi. Dönen OBB için Mesh.bounds köşeleri root-local uzaya dönüştürülür ve root dünya ölçeği uygulanır; dönen dünya AABB'si hitbox ölçüsüyle karşılaştırılmaz.
- Yeni timed modellerin mesafe/7 zamanındaki gerçek root pozisyonu, dönüşü, ölçeği; pause, resume, restart.
- Gerçek Sonsuz Akış beş chunk boyunca SafeRoute ile ilerler; ilk chunk emekli olduktan sonra yeni timed mesh referansı/pozları ve aktif chunk sınırı kontrol edilir.
- Yeni normal CapacitorBank yakınında dolu enerji Pulse'u hedefi açmaz; dar ilerleyen küre gerçek temasta denemeyi bitirir. Bu özel Pulse kontrolü enerji önkoşulunu Model.CollectEnergy ile kurar; render/oynanabilirlik testi gerçek SafeRoute girdileriyle ilerler.
- Gerçek URP oyun kameralarıyla dört yaklaşım karesi; her seçilen rota çekimden sonra tamamlanır. PNG yardımcı işlevi iki warm submit/frame ve finally restorasyonu kullanır; döngüler sınırlıdır ve PlayerPrefs kullanılmaz.

Mevcut resolver sonucu beklenen kareler: `output/previews/Obstacles_ClampBarrier_Level1.png` (engel Z16, yaklaşım8), `Obstacles_CapacitorBank_Level1.png` (Z36, yaklaşım28), `Obstacles_HydraulicSlider_Level4.png` (Z24, yaklaşım16), `Obstacles_TurbineBeam_Level4.png` (Z70, yaklaşım62). Seçim sabit dosya adlarına bağlı değil, gerçek katalog envanterinden yapılır. Tamamlanan capture testinin `output/previews/Obstacles_inventory.json` çıktısı seçilen bölüm/Z ve gerçek çekim mesafesini kaydeder. Renderlar ancak ana yönetici testi çalıştırdıktan sonra mevcut sayılır; insan görsel incelemesi ayrıca gerekir.

Ana yöneticinin ilk Unity çalıştırması: 95 PlayMode testinden 94 geçti; tek hata yeni materyal renk kontrolünde tam Color eşitliğiydi. Görünen RGBA değerleri aynı olsa da materyal float geri okumasındaki küçük fark nedeniyle kontrol `Vector4.Distance < 0.00001` toleransına düzeltildi. Üretim kodu değişmedi. Diğer beş yeni test geçti; dört gerçek kare üretildi ve ana yönetici tarafından görsel incelendi. Tolerans düzeltmesi sonrası tam 95 test tekrarının sonucu ana yönetici doğrulama kaydına aittir; bu ajan Unity çalıştırmadı.
