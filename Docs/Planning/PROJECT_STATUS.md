# Proje durumu

Son güncelleme: 04.11.2026.

## Portfolyo sunumu — 0.9.0 medya paketi

D-043: README kısa İngilizce tanıtım, vektör kapak, hareketli önizleme, gerçek oyun/Pulse/röle/harita galerisi ve MP4 bağlantısıyla yenilendi. Beş gerçek 540×960 ekran ve 15 saniyelik 30fps/450 kare H.264 High + 48kHz stereo AAC montajı üretildi. Üç 5s kesit Bölüm 1/8/6 SafeRoute girdileriyle normal 7m/sn çalışır; enerji/konum/hız/kapı durumuna müdahale yok. Ses özgün master ve gerçek olay zamanlarından editoryal miks, DSP kaydı değil. Harita profili yalnız bellekte; kullanıcı kaydı yazılmaz.

Son MP4 3.812.631 bayt; GIF 2.478.200 bayt/150 kare/15sn. Fast-start index ve chunk offsetleri doğrulandı. İki capture testi, yüksek kaliteli yeniden export ve yerel VideoPlayer doğrulaması geçti: 450 kare/15s/30fps,540×960,48kHz stereo; üç decoded kare gerçek kaynaklarla karşılaştırıldı ve görsel incelendi. Native decoder test yardımcısı uçtan uca geçti. Tarayıcı otomasyonu güvenilir URL belirleyemediği için araç tarafından durduruldu; Windows tarayıcı önizlemesi doğrulanmadı. Tekrar tarayıcı otomasyonu yapılmadı.

Capture/verify kaynakları Tools/Portfolio altında isteğe bağlı template olarak saklanır. Yardımcı geçici Video modülü/test referanslarını ve fixture'larını finally içinde temizler; Assets/Packages/ProjectSettings kaynakları 0.9.0 ile birebir aynı. Ürün kodu, bağımlılığı, sürümü ve son 256 regresyon kaydı değişmez; yeni uygulama build'i gerektirmez. Ayrıntılar Docs/Portfolio/MEDIA_NOTES.md ve media-validation.json. Eski uzun Türkçe README, bağlantıları düzeltilerek Docs/DEVELOPMENT_TR.md içine taşındı.

portfolio_capture ve portfolio_readme GPT-6.1 Sol medium ile ayrı kaynak/taslak sahipliğinde çalıştı; ana yönetici native export entegrasyonu, kalite, GIF/MP4 paketleme, decoder/bağlantı kontrolleri, son README, kayıt ve GitHub yayınını yürütür. Telefon kaydı veya manuel insan oynanışı iddiası yoktur.

## Optimizasyon — 0.9.0 (10)

D-042 kapsamında önce kalibre GC.Alloc olay sayacı ve aynı rota CPU ölçümü alındı. GameHud sabit sayı/koç/kart formatını önbellekliyor; MenuHero statik önizlemeyi iki ilk render sonrası saklıyor ve texture kaybında yeniliyor. Temasta muhafazakâr Z ön elemesi, sabit bölüm/öğretici bilgisi, pointer reuse ve chunk kimliğiyle emeklilik var. GameFeedback halka noktalarını toplu yüklüyor, sabit MPB ve gizli aura işini atlıyor; ghost materyal slotlarını tekrar kullanıyor. Kontrol, solver, 25 bölüm/kayıt, ses ve URP MSAA 2/renderScale 1 korunur.

Aynı kontrollü Editor/Mono rotalarında GC.Alloc olayı %97–99, ortalama SimulateStep süresi %43–61 azaldı; bunlar bayt/peak RAM veya telefon FPS ölçümü değildir. Idle 30 karede menü preview 30→0 çizim; sabit sonuç/pause 512 adımda tahsis 0. Tüm karşılaştırılan rota sonu değerleri aynı. Oyun/harita gerçek kareleri 0.8.0 referanslarıyla piksel düzeyinde aynı; menü görsel kabulü ve RT kaybı testi geçti. **120 EditMode +136 PlayMode =256/256** geçti. Windows/Android strict build exit0; APK39.163.089 bayt,0.9.0 (10), ARM64/API25/36/portrait ve v2 imzası doğrulandı. Kalıcı kayıt Docs/QA/OPTIMIZATION_VALIDATION.md ve Results/Optimization_*.json.

optimization_ui, optimization_core ve optimization_render GPT-6.1 Sol medium ile ayrı dosya sahipliklerinde çalıştı; ana yönetici ölçüm altyapısı, kaynak incelemesi, gerçek render karşılaştırması, ortak test/assembly uyarlaması, build/Git teslimini yürütür. İlk iki yeni test fixture sorunu düzeltildi; son tam tur 136/136. Daha büyük model havuzlaması ve fiziksel cihaz GPU/FPS/termal/pil kabulü açık; D-017 gereği telefon talebi yinelenmez.

## Müzik ve coin — 0.8.0 (9)

D-039/040: özgün 73,85 saniyelik sözsüz Tether Flow müziği, 10 kısa ses efekti ve altın coin/HUD simgesi tamamlandı. Müzik tek uygulama kaynağında menü/koşu/retry boyunca sürer; mevcut kalıcı ses anahtarı/düzeyi uygulanır. Pause, odak kaybı ve uygulama arka planı ayrı nedenlerdir. Üç coin ses varyantı ve toplama çiftlerini tek sese indiren aralık sınırı vardır. Kaynak reçete/master, üretim betiği ve sayısal ses QA depoda; runtime ses dosyaları toplam yaklaşık1,085MB.

Blender coin tek mesh/624 üçgen, altın rim/koyu yüz/iki bağlı çekirdek kabartması; oyun, sağ üst HUD ve harita aynı tasarımı kullanır. Hafif yalpa pause'da donar. Bonus temas yarıçapı .19→.25m yeni .5m görselle eşlendi; bonus sayısı/100puan, kayıt ve 25 bölüm korunur. Önceki18FBX hashleri aynıdır. Son kaynaklarla **120 EditMode +120 PlayMode =240/240**, Prepare ve Windows strict build exit0. İki gerçek540×960 URP karesi incelendi. Android strict build exit0; APK39.158.057 bayt, 0.8.0 (9), ARM64/API25/36/portrait ve v2 imzası doğrulandı. Kanıt Docs/QA/AUDIO_COIN_VALIDATION.md ve Results/AudioCoin_test_results.json.

music_sound_assets, audio_runtime ve coin_blender görevleri GPT-6.1 Sol medium ile ayrı dosya sahipliklerinde tamamlandı; ana yönetici App/HUD/harita/importer/miks, gerçek Unity test/render/build ve Git entegrasyonunu yürütür. İnsan dinlemesi bu çalışmada yapılmadı; 14s temsili miks AudioSource/TetherFlow.gameplay-preview.wav. Kullanıcı müzik/coin beğenisini ve ses dengesini Editor'da inceleyecek; fiziksel Android testi D-017 gereği sonda.

## Sade sanat ve etkileşimler — 0.7.0 (8)

D-035–038: kullanıcı önceki engelleri beğenmedi; sade revizyon, yeni mekanikler ve skorla hızlanan Sonsuz Akış istedi. Bağ rölesi ve hareketli Pulse kilidi birlikte ayrıca onaylandı. Dört FBX yeniden üretildi: grafit, alçak omuz/kama/kızak/kiriş ve küçük mercan işaretler; toplam544 üçgen. İki etkileşim tek kontrolle çalışır. İlk öğretimler Bölüm6/8, tekrarlar12/16/20/25; Sonsuz kalıp3/4 de bunları kullanır. Sonsuz tempo500→5500 skor hedefiyle7→11,5m/sn, en fazla0,25m/sn² ivme; pause dondurur/retry7'ye döner. Bölümler7m/sn kalır. Kayıt/25kimlik/sıralı açılma ve ilk5 mekanik içeriği korunur.

Son kaynaklarla Prepare exit0 ve **120 EditMode +111 PlayMode =231/231** geçti. Yeni mekanik öncesi/sonrası ve yüksek hız gerçek URP kareleri incelendi. Windows ve Android strict build exit0; APK38.157.366 bayt,0.7.0 (8), ARM64/API25/36/portrait ve v2 imzası doğrulandı. Kalıcı kanıt Docs/QA/SYSTEMS_VALIDATION.md ve Results/Systems_test_results.json. Alt görevlerin hepsi GPT-6.1 Sol medium; ana yönetici runtime/test/render/build/Git entegrasyonunu yürütür. Fiziksel cihaz ve insan beğeni/zorluk kabulü açık kalır.

## Engel modelleri — 0.6.0 (7)

D-033: kısa araştırma sonrası dört özgün Blender modeli eklendi: kenet bloğu, kapasitör bankası, hidrolik kızak ve türbin kiriş. obstacle_blender model/kaynak/FBX ve bağımsız doğrulamayı, obstacle_qa entegrasyon planı ve yeni6 PlayMode senaryosunu GPT-6.1 Sol medium ile üstlendi. Ana yönetici araştırma, yalnız sunum katmanındaki kararlı model seçimi, kırmızı tehlike materyali, test/render/build ve Git teslimini birleştirdi.

4 FBX reimport ve model başına50 temas rayı geçti; eski14FBX değişmedi. Ek bütçe1732 üçgen/4mesh; kit18 model/8280 üçgen/24mesh. **96 EditMode +95 PlayMode =191/191** geçti. Gerçek dört URP yaklaşım karesi incelendi. İlk bölümde iki statik, dördüncüde iki hareketli model görülebilir; Sonsuz Akış da aynı modelleri kararlı seçer. Course/Core verisi, SafeRoute, temas ve zamanlama kaynakları değişmedi; 25 bölüm, sıralı açılma, Pulse, kayıt ve mevcut UI korunur. Windows ve Android strict build exit0; APK 38.173.530 bayt, 0.6.0 (7), ARM64/API25/36/portrait ve v2 imzası doğrulandı. Kanıt Docs/QA/OBSTACLE_VARIANTS_VALIDATION.md ve Results/ObstacleVariants_test_results.json; kaynak ArtSource/Blender/ObstacleVariants, kareler Docs/Art/Previews/Obstacles_*.png.

## UI yenilemesi — 0.5.0 (6)

D-031 ile kullanıcı başarılı hypercasual UI incelemesi ardından menü ve oyun arayüzünü yenilememizi istedi. Uygulamadan önce yedi resmî ürün/mağaza ve gerçek ekran görselleri incelendi; gözlem/çıkarım ve promo/runtime ayrımı Docs/Research altında kayıtlı. Barlow/OFL tipografi, özgün simgeler, düz koyu/kırık beyaz yüzeyler, tek baskın eylem ortak dil oluşturur. Ana menü gerçek Blender modelleri içerir; harita düz soketlere, HUD 103px üst bilgiye indirildi. Kalıcı alt açıklama kaldırıldı; kısa öğretici ilk beşte, Pulse menzil uyarısı her bölümde korunur.

**96 EditMode + 89 PlayMode = 185/185** geçti. Son kaynaklarla 25 gerçek rota, kayıt göçü, sıralı erişim, harita kaydırma/odak/dönüş, ayarlar, sonuç/sonraki, font Türkçe glyphleri ve Blender preview yaşam döngüsü doğrulandı. 21 gerçek URP karesi 540×960 / 540×1170 olarak alındı; temsilî tüm sayfalar görsel incelendi. Windows strict build exit 0: output/windows-ui-refresh/TetherRush.exe. Android strict build exit 0; APK 38.133.750 bayt, 0.5.0 (6), ARM64, API25/36, portrait ve v2 imzası doğrulandı. Kalıcı kayıt Docs/QA/UI_REFRESH_VALIDATION.md ve Results/UIRefresh_test_results.json. Ekranlar Docs/Art/Previews/UIRefresh_*.png.

25 bölüm/oynanış/kayıt şeması korunur. Kullanıcının görsel beğeni ve insan oynanış incelemesi açıktır; telefon testi D-017 gereği sonda. Yeni bir ekonomi, yıldız veya kontrol eklenmedi.

## 25 bölümlük harita — 0.4.0 (5)

D-029/030 ile 25 bölüm, yalnız numaralı enerji tesisi haritası ve sıralı açılma onaylandı. İçerik, harita UI ve kayıt göçü GPT-6.1 Sol medium ajanlarına ayrı dosya sahiplikleriyle verildi; ana yönetici uygulama erişimi, haritaya dönüş/odak, gerçek rota/etkileşim/render testleri, build ve GitHub teslimini birleştirdi.

İlk beş parkur ve kimlikleri korunur; sonraki 20 parkur bağımsız ritimler ve geç bölümlerde 2–3 Pulse dizisi içerir. 25 düğümlük dikey haritada tamamlanan/sıradaki/kilitli durumlar ayrılır. Başarı sonraki bölümü açar, tamamlananlar tekrar oynanır. Eski v1/5 kayıt v2/25'e mevcut tamamlanma/bonus/sonsuz rekor/ses ayarlarıyla genişler; eski ilerleme geri alınmaz.

**96 EditMode  + 86 PlayMode = 182/182** test geçti. 25 gerçek bölüm tamamlandı; kilitli erişim, uGUI kaydırma/tıkla ayrımı, sonraki açılma, doğrudan haritaya dönüş, v1 göçü ve viewport değişiminde odak doğrulandı. Gerçek 540×960 /540×1170 harita kareleri incelendi. Windows strict build exit 0; Android strict build ve APK v2 imzası doğrulandı. Kalıcı kayıt Docs/QA/LEVEL_MAP_25_VALIDATION.md ve Results/LevelMap25_test_results.json; tasarım Docs/Design/LEVELS_25.md /Docs/Art/LEVEL_MAP_DESIGN.md.

## Menü ve ilk bölümler — 0.3.0 (4)

Kullanıcı D-024/025 ile Leveller / Sonsuz Akış / Ayarlar menüsünü ve beş kısa bölümü onayladı. Menü/HUD, bölüm/sonsuz parça verisi ve kayıt/ayar işleri GPT-6.1 Sol medium ajanlarına ayrı sahipliklerle verildi; ana yönetici uygulama akışı, stream temizliği, gerçek Unity testi/görüntüsü ve teslimi birleştirdi.

MainMenu başlangıç sahnesi, beş bölüm seçimi, sonuçtan sonraki bölüm ve güvenli menü dönüşü hazır. Sonsuz Akış çarpışmaya kadar devam eder; tam metre + bonus ×100 puan, monoton yerel rekor. Ses/volume ve bölüm tamamlanma/bonus kayıtları yerelde saklanır. İlk seçkide bütün bölümler erişilebilir; kilit/yıldız/reklam eklenmedi.

**66 EditMode + 58 PlayMode = 124/124** test geçti. Beş gerçek bölüm tamamlandı; Sonsuz Akış 15 chunk / 1.680 m üzerinde, aktif dünya/collectible belleği sınırları, pause/death/retry ve rekorla doğrulandı. Sekiz gerçek Unity görüntüsü (9:16 ve uzun menü oranı) incelendi. Windows ve Android strict build exit 0; APK v2 imzası doğrulandı. Kayıt Docs/QA/MENU_LEVELS_VALIDATION.md, tasarım Docs/Design/FIRST_LEVELS.md. Fiziksel cihaz ve ses duyumu kabulü ayrıca bekliyor.

## Görsel cila — 0.2.1 (3)

D-021 kapsamındaki görsel üretim tamamlandı: şeftali/lavanta küreler ve normal/hazır hâlde camgöbeği bağ; lacivert tesis, mercan uyarılar ve amber Pulse hedefleri tek VisualTheme paletinde. 14 Blender modeli, cilalanmış gölgeleme/efekt, yeni dar geçit/merkez bariyer görünümleri ve dekoratif ışık kuleleri entegre. Yuvarlatılmış HUD ve alt durum kartı oyun alanını açık tutar. Oyun ve çarpışma kuralları korunur.

29 EditMode + son kaynaklarla 32 PlayMode = **61/61** test geçti. Gerçek Unity görüntüleri Ready/Foundation/Pulse/Challenge/Failed hâllerinde incelendi; skybox ve retry iz sıfırlama düzeltmeleri doğrulandı. Son Windows ve Android build exit 0; APK imzası doğrulandı. Kayıt: Docs/QA/POLISH_VALIDATION.md, vaka özeti Docs/QA/Results/Polish_test_results.json.

Cila tesliminden sonra kullanıcı 02.11.2026 tarihinde menü ve ilk bölümlerin üretimine geçilmesini istedi. Fiziksel cihaz testi D-017 gereği sona bırakıldı.

## Tamamlanan hazırlık

- Boş GitHub deposu mevcut çalışma klasörüne klonlandı; `origin` doğru URL'ye bağlı, başlangıç dalı `main`.
- Tasarım belgesinin 8 sayfası okundu ve görsel olarak incelendi. Özgün PDF ve aranabilir metin `Docs/Source` içinde saklandı.
- Android telefon + dikey ekran kullanıcı tarafından onaylandı.
- Planlama ve M1 uygulama görevleri GPT-6.1 Sol / medium ajanlarına dağıtıldı; dosya sahipliğiyle çakışmalar önlendi. Ana yönetici entegrasyon ve Unity doğrulamasını yürüttü.
- Karar, iş akışı, dosya sahipliği ve kabul ölçütleri için proje kayıtları oluşturuldu.

## Yerel araçlar

| Araç | Gözlenen durum |
| --- | --- |
| Unity | Proje 6000.3.19f1 (7689f4515d75) ile açıldı, C# derlendi ve prototip sahnesi kaydedildi. URP 17.3.0, Input System 1.19.0 kilitlendi. |
| Android desteği | Optimizasyon APK strict build exit0:39.163.089 bayt,0.9.0 (10), ARM64/API25/36/portrait; v2 imzası doğrulandı. output/android/TetherRush-Optimized.apk. ASCII junction/generated-cache callback kullanılır. |
| Blender | 5.2.3 LTS ile19 FBX; yeniCoin624tri/tekmesh/üç materyal, bağımsız reimport geçti. Önceki18FBX aynı; toplam kit7.716tri/25mesh. Kaynak ve paftalar ArtSource/Blender içinde. |
| Git | origin GitHub TetherRush-MobileGame, dal main. Kod, Blender kaynak/FBX, gerçek Unity kareleri ve doğrulama özetleri sürüm kontrolüne alınır; build/cache çıktıları dışarıda tutulur. Son teslim hash'i git log ve yönetim sohbetindeki teslim raporundan izlenir. |
| Unity testleri | Optimizasyon: EditMode120/120 +PlayMode136/136 =256/256. Docs/QA/Results/Optimization_test_results.json; önceki240/231/191/185/182/124/61 kayıtları korunur. |
| Windows önizleme | Optimizasyon strict build exit0, output/windows-optimized/TetherRush.exe. Güncel kareler Docs/Art/Previews/Optimized_*.png. |

## Kilometre taşları

| Aşama | Kapsam | Durum / çıkış koşulu |
| --- | --- | --- |
| M0 – Hazırlık | GDD analizi, planlar, ajan iş dağılımı, GitHub başlangıç kaydı. | Tamamlandı; başlangıç paketi kullanıcı tarafından onaylandı. |
| M1 – Oynanabilir prototip | Daral/genişle, tether enerji, bonus, Pulse kapısı/kırılabilir hedef, ölüm, bitiş/retry, temel HUD. | İlk teslim tamamlandı; kullanıcı devam onayıyla M2'ye geçildi. Bu onay tüm manuel testlerin yürütüldüğü anlamına gelmez. |
| M2 – Görsel ve içerik | Blender kiti, animasyon/VFX/ses ilk seti, hareketli duvar/dönen kol ve ikinci alıştırma. | M2 ilk tesliminden sonra D-021 görsel cila uygulandı. Cila: 61 test, Windows ve imzası doğrulanan Android 0.2.1 build geçti. Son görsel/oynanış kabulü kullanıcıda. |
| Menü ve ilk bölüm paketi | Ana menü, beş öğretici bölüm, Sonsuz Akış, yerel skor/ayarlar. | Kullanıcı kapsamı onayladı; 124 test ve Windows build geçti. Android 0.3.0 (4) ve imzası doğrulandı. |
| 25 bölüm haritası | Numaralı tematik yol, sıralı açılma,20 yeni bölüm, eski kayıt göçü. | 182 test ve Windows build geçti; Android 0.4.0 (5) ve imzası doğrulandı. |
| UI yenilemesi | Referans araştırması, ortak menü/HUD/harita dili, font/simge ve Blender önizleme. | 185 test, gerçek render, Windows ve imzası doğrulanan Android 0.5.0 build geçti. |
| Engel modelleri | Araştırma,4 Blender varyantı ve bölüm/sonsuz görsel seçimi. | 191 test,4 gerçek kare,Windows ve imzası doğrulanan Android0.6.0 build geçti. |
| Sade sanat ve etkileşimler | Bağ rölesi, hareketli Pulse kilidi, skorla artan sonsuz tempo. |231 test,9 gerçek kare,Windows/Android0.7.0 ve APK imzası geçti. |
| Müzik ve coin | Özgün sözsüz döngü/10SFX, Blender coin ve aynı HUD/harita Sprite. |240 test,2 gerçek kare, Windows/Android0.8.0 strict build ve APK imzası geçti. |
| Optimizasyon | HUD/cache, demand preview, temas ön elemesi, stream/efekt maliyeti. |256 test, aynı rota kıyası,3 gerçek kare, Windows/Android0.9.0 strict build ve APK imzası geçti. |
| M3 – Mobil doğrulama | Fiziksel cihaz, performans, ekran oranları, pause/resume, ses ve titreşim. | Cihaz ve hedef bütçeler belirlenecek. |
| M4 – Yayına hazırlık | Mağaza gereksinimleri ve dağıtım. | Kullanıcı ayrıca kapsam/onay verecek. |

## Şu anki sınır

Başlangıç MainMenu; Leveller numaralı 25 düğümlük haritayı açar. Yeni profilde yalnız 1 açık, başarı sıradakini açar. İki mod tek kontrolü ve Pulse kurallarını korur; bölümler sabit7m/sn, Sonsuz Akış skora göre7→11,5m/sn tempoyla ilerler. Önceki prototip/menü build ve kayıtları tarihsel karşılaştırma için korunur. Görsel bölüm editörü, yıldız ekonomisi, reklam veya mağaza yayını eklenmedi. Fiziksel Android performans/dokunma/çentik, uygulama kapat-aç ve ses duyumu kabulü bekler.

## Sıradaki yönetim kararı

Kullanıcı optimize sürümün Editor/Windows akıcılığını ve görünümünü inceleyecek. Yeni müzik/efektlerin duyumu, coin tasarımı, mekanik okunurluğu ve sonsuz zorluk ritmi insan kabulünde açık kalır. Yeni oyun sistemi veya ekonomi eklenmeden kapsam tartışılacak. Android telefon testi D-017 gereği sonda; tekrar cihaz talep edilmeyecek. Oynama README.md, güncel gerçek kareler Docs/Art/Previews/Optimized_*.png altında.
