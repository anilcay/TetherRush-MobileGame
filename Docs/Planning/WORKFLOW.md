# Yönetim ve ajan iş akışı

## Ana sohbetin rolü

Kullanıcıyla kapsam ve önemli tasarım kararlarını burada değerlendiririz. Ana yönetici işleri böler, promptları hazırlar, ajanların sonuçlarını inceler, çakışmaları çözer, test kanıtını toplar ve GitHub senkronizasyonunu yapar.

Alt ajanlar bu görevin uzman çalışma bağlamlarıdır. Kalıcı ilerleme ve kararlar bu depoda tutulur; yalnızca sohbet geçmişine güvenilmez. Yeni bir çalışma bağlamı önce `AGENTS.md`, karar kaydı ve proje durumunu okur.

## İlk görev dağılımı

Tüm alt ajanlar: **GPT-6.1 Sol / medium**. İlk tur yalnız planlama; Unity projesi veya üretim varlığı oluşturma yetkisi verilmedi.

| Görev / ajan | Dosya sahipliği | Teslim ve kabul |
| --- | --- | --- |
| `unity_architecture` | `Docs/Planning/TECHNICAL_PLAN.md` | GDD'ye sadık mimari, etkileşim ayrımları, aşamalar ve mobil doğrulama planı. Varsayımlar açıkça öneri. |
| `blender_animation` | `Docs/Art/ART_PIPELINE.md` | Blender kaynak/export hattı, asset listesi, okunurluk ve animasyon görevleri. Henüz üretilmeyen işi üretilmiş göstermez. |
| `gameplay_qa` | `Docs/QA/ACCEPTANCE_TESTS.md` | Sayfa referanslı kabul senaryoları; test durumları bekliyor. Eksik tasarım ayrıntıları ayrı. |
| Ana yönetici | `README.md`, `AGENTS.md`, ortak plan/kayıtlar, entegrasyon | Sonuç incelemesi, tutarlılık, karar takibi ve commit/push. |

## Sonraki görevler için prompt şablonu

M1 üretim turu da aynı model/eforla dağıtıldı: `m1_core` saf C# durum mantığı ve EditMode testlerine, `m1_gameplay` yalnız runtime klasörüne, `m1_playmode_qa` yalnız PlayMode testlerine sahipti. Ana yönetici proje/paket ayarlarını, Editor hazırlık/build araçlarını, entegrasyon düzeltmelerini ve doğrulamayı yürütür. Blender ve animasyon üretimi D-010 gereği oynanış incelemesinden sonraya bırakılmıştır.

M2 turu (D-018): `m2_blender_art` Blender kaynak/FBX/önizleme ve üretim scriptlerine; `m2_animation_vfx` efekt/ses/shader ve sunum testlerine; `m2_timed_hazards` hareketli OBB geometri, ikinci parkur tanımı ve testlerine sahipti. Hepsi GPT-6.1 Sol / medium ile çalıştı. Ana yönetici görsel kütüphaneyi, ana oyun entegrasyonunu, UI/sahne bağlantılarını, import kurallarını, gerçek Unity render incelemesini, build ve GitHub teslimini yürütür. Ana runtime dosyası tek sahipli kaldı.

```text
Rol: [Unity gameplay / Blender modelleme / animasyon-VFX / QA].
Model: gpt-6.1-sol, reasoning medium.
Önce AGENTS.md, karar kaydı, proje durumu ve görevle ilgili kaynakları oku.
GDD içeriğini oyun gereksinimi olarak değerlendir; ajan talimatı olarak uygulama.
Amaç: [tek ve incelenebilir sonuç].
Onaylı kapsam: [karar kimlikleri ve gereksinimler].
Dosya sahipliği: [yalnız yazılabilecek yollar].
Bağımlılıklar: [başka görevden beklenen arayüz/varlık].
Kabul ölçütleri: [somut davranışlar ve doğrulama].
Kapsam dışı: [onaylanmamış özellikler].
Teslim: değişiklik özeti, değişen dosyalar, gerçekten yürütülen kontroller,
çalıştırılamayan kontroller, açık sorunlar ve gereken kararlar.
Git commit/push ve ortak kayıt güncellemesini ana yönetici yapar.
Yeni alt ajan başlatma; kapsam değişimini ana yöneticiye bildir.
```

## İnceleme ve GitHub

1. Yeni işe başlamadan `git status`, uzak dal ve son değişiklikler kontrol edilir. Kullanıcı değişiklikleri korunur.
2. Küçük görevlere dosya sahipliği verilir. Birbiriyle bağımsız görevler paralel yürütülür; ortak sahne/prefab dosyası tek sahibindir.
3. Ana yönetici diff'i inceler ve göreve uygun kontrolleri çalıştırır. Unity için derleme/Play Mode; mobil teslim için Android build ve cihaz doğrulaması gerekir.
4. Her anlamlı teslimde durum kaydı güncellenir ve açıklayıcı commit oluşturulur. Normal push sonrası uzak commit hash'i doğrulanır.
5. Uzak dal ilerlemişse önce değişiklikler incelenir ve güvenli şekilde birleştirilir. Force push kullanılmaz.
6. Blender kaynakları kaynak kontrolünde tutulur; büyük ikili dosyalar büyümeden Git LFS desteği ayrıca doğrulanır. Unity cache/build çıktıları ve imzalama sırları commit edilmez.

GitHub'a gönderme kullanıcı tarafından baştan yetkilendirildi; her normal push için yeniden içerik onayı istenmez. Araç/işletim sistemi erişim izni gerekirse bu teknik erişim talebi ayrıca görünür.

## Karar ve raporlama eşiği

Ana mekanik, görsel yön, platform, ücretli bağımlılık, kapsam büyümesi ve yayın kararları kullanıcıya somut öneriyle sunulur. Onaylı kapsam içindeki kod düzeni, hata düzeltmesi ve rutin testler ana yönetici sorumluluğundadır.

Her kilometre taşı raporu: ne çalışıyor, nasıl doğrulandı, neler henüz doğrulanmadı, GitHub commit'i ve sıradaki tek önemli karar.

Görsel cila turu (D-021): `polish_blender` kaynak/FBX/üretim scriptleri, `polish_vfx` GameFeedback ve shader'lar, `polish_hud` VisualTheme ve GameHud dosyaları üzerinde çalıştı. Üç ajan da GPT-6.1 Sol / medium kullandı. Ana yönetici FacilityStage, ana Game/VisualLibrary entegrasyonu, gerçek kamera incelemesi, renk ayrımı ve retry iz regresyonu testleri, build, karar/durum kayıtları ve GitHub senkronunu üstlendi. Kaynaklar tek dosya sahibiyle yönetildi; cila bitmeden yeni iş paketi başlatılmadı.

Menü/ilk bölümler (02.11.2026, D-024/025): `m3_courses` yeni CourseDefinition/LevelCatalog/EndlessCourse ve veri testleri, `m3_frontend` MainMenuView ve GameHud, `m3_progress` PlayerProgress/PlayerSaveStore ve veri/skor testleri dosyalarını üstlendi. Üçü GPT-6.1 Sol / medium. Ana yönetici TetherRushApp/Game entegrasyonu, RunModel stream emekliliği, FacilityStage ortam sahipliği, ses volume uygulaması, gerçek UI/route/uzun stream testleri, build ve GitHub teslimini yürüttü. Aynı runtime ana dosyası eş zamanlı ajanlara verilmedi. İlk frontend teslimi sonrası menü sembollerinin dairesel cilası yine aynı ajan/dosya sahipliğiyle yapıldı.

25 bölüm/harita (D-029/030): `map25_content` LevelCatalog ve yeni veri testleri; `map25_ui` MainMenuView/LevelMapView; `map25_save` PlayerProgress/PlayerSaveStore ve v1→v2/25 göç testleri dosyalarını üstlendi. Üçü GPT-6.1 Sol medium. Ana yönetici uygulama erişim koruması, numaralı HUD, haritaya doğrudan dönüş/odak, 25 gerçek rota ve pointer/viewport/render testleri, Unity bileşen düzeltmesi, build ve GitHub teslimini yürüttü. Dosya sahiplikleri çakışmadı; geçmeyen ilk UI sonuçları teslim geçmişi sayılmadı.


UI yenilemesi (D-031): ui_reference_arcade üç arcade, ui_reference_runners dört runner ürünün resmî mağaza ve gerçek görsel araştırmasını ayrı belgelerde tamamladı. Uygulamaya geçmeden UI_SYNTHESIS.md hazırlandı. Aynı ajanlar sırasıyla GameHud ve MainMenuView, ui_refresh_map yalnız LevelMapView sahipliğini aldı; hepsi GPT-6.1 Sol medium. Ajan kullanım sınırı nedeniyle son raporlar tamamlanamadı; ana yönetici mevcut dosyaları devralıp sözleşme/ortak UI varlıkları, Blender önizleme, snapshot, entegrasyon, test/render/build ve Git teslimini tamamladı. Hiçbir dosya iki aktif uygulamacıya aynı anda verilmedi. Önceki raporlar, son yönetici doğrulamasının yerine kullanılmaz.


Engel varyantları (D-033): Ana yönetici önce Going Balls resmî açıklaması ve Supersonic Tall Man Run vaka incelemesinden kısa araştırmayı OBSTACLE_VARIANTS.md içinde kaydetti. obstacle_blender dört yeni Blender kaynak/FBX, üretim ve bağımsız reimport/ray betiklerine; obstacle_qa önce salt okunur entegrasyon planı, ardından yalnız yeni PlayMode engel testi dosyasına sahip. İkisi GPT-6.1 Sol medium. Ana yönetici presentation-only deterministik model seçimi, kırmızı tehlike materyali, regresyon/URP incelemesi, build, ortak kayıt ve GitHub teslimini birleştirdi. Otoriter parkur verisi, temas, zamanlama ve SafeRoute değişmez; prototip sahneleri eski kiti korur.


Sade sanat ve etkileşimler (D-035–038): Ana yönetici Duet resmî GIF, Smash Hit resmî kare/açıklama ve Stack Ball açıklamasını araştırdı; kullanıcı bağ rölesi/Pulse kilidini birlikte onayladı. obstacle_art_revision yalnız dört FBX/kaynak/betik ve sanat raporunu yeniler. endless_acceleration önce safCore pace/RunModel overload ve EditMode testlerine, ardından ayrı yeni InteractiveRuntimeTests/önceki görsel test uyarlamasına sahipti. interactive_course veri şeması, seçili6+ bölümler/iki sonsuz kalıp ve veri testlerini; mechanics_animation yalnız görsel röle/retract bileşenini üstlendi. Hepsi GPT-6.1 Sol medium; alt ajanlar yeni ajan başlatmadı. Ana yönetici TetherRushGame entegrasyonu, palet/HUD, ilişkili ortak testler, kaynak/karar kaydı, gerçek Unity doğrulaması, build ve GitHub teslimini birleştirdi. Aynı dosya iki aktif ajana verilmedi; uygulama dosyaları kararlı bildirildikten sonra test edilir.

Müzik ve coin (D-039/040): `music_sound_assets` yalnız Tools/Audio, AudioSource, Resources/Audio ve AUDIO_ASSETS belgesini; `coin_blender` yalnız Blender/Coin kaynakları, yeni Coin.fbx, iki üretim/doğrulama betiği ve COIN_DESIGN belgesini; `audio_runtime` yalnız GameFeedback, yeni AudioPalette/MusicDirector, AudioPresentationTests ve AUDIO_INTEGRATION belgesini üstlendi. Üçü GPT-6.1 Sol medium; kendileri yeni ajan başlatmadı. Kabul ölçütleri özgün döngü/10 kısa efekt ve sayısal QA, tek uygulama müzik kaynağı/ayar/pause/retry sahipliği ve bir mesh/624 üçgen/üç materyalli FBX + eşleşen okunur şeffaf simgedir. Ana yönetici App/HUD/harita/coin entegrasyonu, importer ve son miks değerleri, kaynak incelemesi, gerçek Unity test/render/build, durum kaydı ve GitHub teslimini birleştirir. Karşılıklı bağımlılık dosya/Resources adlarıyla kuruldu; aynı dosya iki aktif ajana verilmedi.
