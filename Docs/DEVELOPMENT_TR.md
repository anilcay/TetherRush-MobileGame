# Tether Rush

Unity ile geliştirilecek, ortak bir enerji bağıyla bağlı iki küreyi tek parmakla yönettiğimiz 3D mobil parkur oyunu.

## Durum

Güncel paket **0.9.0 (10)**: HUD metin önbelleği, sabit menü önizlemesinde demand render, muhafazakâr temas ön elemesi ve toplu efekt güncellemeleri. Aynı kontrollü Editor rotalarında GC.Alloc olayları **%97–99**, ortalama oyun döngüsü işlem süresi **%43–61** azaldı; bu ölçüm telefon FPS veya RAM yüzdesi değildir. Oynanış, 25 bölüm/kayıt, görsel kalite ve ses korunur. **256/256 Unity testi**, Windows ve imzası doğrulanan Android build geçti. [Doğrulama ve ölçüm sınırları](QA/OPTIMIZATION_VALIDATION.md); fiziksel cihaz testi son aşamadadır.

- Hedef: **Android telefon, dikey ekran** (kullanıcı onayı: 31.10.2026).
- Tasarım kaynağı: [GDD v2.0](Source/Tether_Rush_GDD_v2.pdf), 31.10.2026.
- Yönetim merkezi: kullanıcının ana Codex sohbeti.
- Alt ajanlar: **GPT-6.1 Sol / medium**.
- Kaynak depo: https://github.com/anilcay/TetherRush-MobileGame

## Proje kayıtları

- [Optimizasyon doğrulaması](QA/OPTIMIZATION_VALIDATION.md)
- [Önce/sonra ölçümleri](QA/Results/Optimization_comparison.json)
- [Optimize gerçek menü karesi](Art/Previews/Optimized_Menu.png)

- [Müzik/coin doğrulaması](QA/AUDIO_COIN_VALIDATION.md)
- [Özgün müzik ve efektler](Audio/AUDIO_ASSETS.md)
- [Ses entegrasyonu](Audio/AUDIO_INTEGRATION.md)
- [Coin Blender tasarımı](Art/COIN_DESIGN.md)
- [Müzik/efekt miks önizlemesi — 14 sn](../AudioSource/TetherFlow.gameplay-preview.wav)
- [Gerçek oyun/HUD görüntüsü](Art/Previews/AudioCoin_Game.png)

- [Yeni sistemler doğrulaması](QA/SYSTEMS_VALIDATION.md)
- [Sade tasarım ve mekanik araştırması](Research/INTERACTIVE_REDESIGN.md)
- [Engel sanat revizyonu](Art/OBSTACLE_ART_REVISION.md)
- [Bağ rölesi / Pulse kilidi](Design/INTERACTIVE_COURSE.md)
- [Sonsuz tempo ayarı](Design/ENDLESS_PACE.md)

- [Engel modelleri doğrulaması](QA/OBSTACLE_VARIANTS_VALIDATION.md)
- [Kısa engel araştırması](Research/OBSTACLE_VARIANTS.md)
- [Blender engel varlıkları](Art/OBSTACLE_VARIANTS.md)
- [Gerçek oyun içi kenet modeli](Art/Previews/Obstacles_ClampBarrier_Level1.png)

- [UI yenilemesi doğrulaması](QA/UI_REFRESH_VALIDATION.md)
- [UI araştırma ve sentezi](Research/UI_SYNTHESIS.md)
- [Yeni menü ve HUD tasarımı](Art/UI_REFRESH_MENU_HUD.md)
- [Yeni harita UI tasarımı](Art/UI_REFRESH_MAP.md)
- [Yeni gerçek Unity menü görüntüsü](Art/Previews/UIRefresh_Menu_Home.png)

- [25 bölüm haritası doğrulaması](QA/LEVEL_MAP_25_VALIDATION.md)
- [25 bölüm içeriği](Design/LEVELS_25.md)
- [Harita görsel tasarımı](Art/LEVEL_MAP_DESIGN.md)
- [Önceki menü/bölüm doğrulaması](QA/MENU_LEVELS_VALIDATION.md)
- [İlk beş bölüm ve Sonsuz Akış tasarımı](Design/FIRST_LEVELS.md)
- [Menü tasarımı](Art/MENU_DESIGN.md)
- [Durum ve iş listesi](Planning/PROJECT_STATUS.md)
- [Kararlar ve onay bekleyen öneriler](Planning/DECISIONS.md)
- [İş akışı ve ajan görevleri](Planning/WORKFLOW.md)
- [Unity teknik planı](Planning/TECHNICAL_PLAN.md)
- [Blender ve animasyon hattı](Art/ART_PIPELINE.md)
- [Kabul testleri](QA/ACCEPTANCE_TESTS.md)
- [M2 doğrulama ve test sonuçları](QA/M2_VALIDATION.md)
- [Blender varlık manifesti](Art/M2_ASSET_MANIFEST.md)
- [Görsel cila doğrulaması](QA/POLISH_VALIDATION.md)
- [Güncel Blender kiti](Art/POLISH_ASSETS.md)
- [Gerçek Unity harita görüntüsü](Art/Previews/Map25_Progress.png)

## Unity'de aç ve oyna

1. Unity Hub → Projects → Add ile bu deponun kök klasörünü seç. Editor: **6000.3.19f1**.
2. İlk açılışta paket çözümleme ve import işlemlerinin bitmesini bekle. URP **17.3.0**, Input System **1.19.0**; çözümlenen sürümler `Packages/packages-lock.json` içinde.
3. `Assets/TetherRush/Scenes/MainMenu.unity` sahnesini aç. Sahne yoksa **Tether Rush → Prepare Project** menüsünü kullan.
4. Game görünümünü **9:16** yap ve Play'e bas. **Leveller** düğmesinden haritayı aç. Açık bir numaraya dokununca koşu başlar; haritayı sürükleyebilir veya fare tekeriyle kaydırabilirsin.
5. Oyun alanında **sol fareyi veya Space'i basılı tut: açıl; bırak: daral**. Enerji doluysa bırakmak aynı zamanda tek Pulse üretir. Space yalnız Editor kolaylığıdır; mobilde tek dokunma vardır.

Haritada **1–25** numaraları vardır. Yeni profilde yalnız 1 açıktır. Bir bölümü iki küreyle bitirmek sonraki düğümü açar; tamamlananlar tekrar oynanabilir. Başarı ve en iyi bonus cihazda saklanır. Sonuç ekranında **Sonraki bölüm**, duraklatma/sonuç ekranında **Haritaya dön** bulunur. Harita dönüşte son oynanan veya yeni açılan düğüme odaklanır. Tamamlanan, sıradaki ve kilitli durumlar sayı, renk ve check/lock işaretleriyle ayrılır.

İlk bölümden itibaren sade omuz/kama engelleri, dördüncüde kayan/dönen tipler görülebilir. **Bölüm 6** bağ rölesini öğretir: zemindeki iki cyan iletkeni geniş bağla kapsa; ilerideki bağlı kapı açılır. **Bölüm 8** amber hareketli Pulse kilidini öğretir: beş enerjiyi sakla, MENZİLDE yazısında bırak. Röleye Pulse işlemez; normal engellere de Pulse işlemez.

İlk beşin oynanış yerleşimleri korunur. 20 yeni parkur; dar/geniş ritmi, hareketli duvar, dönen kol ve birden fazla bağımsız Pulse dizisini birleştirir. En uzun bölüm 280 m'dir. Ayrı bölüm isimleri kullanılmaz. Önceki beş bölümlük v1 kayıt, 25 bölümlük v2 yapıya kayıpsız genişler; eski ilerleme geri alınmaz.

**Sonsuz Akış** ekranından **Akışa başla**: skora göre temposu artan, beş kalıpla çeşitlenen parkurda çarpışana kadar skor topla. Her tam metre 1, her coin 100 puandır. İlk500 skordan sonra tempo artar, 5500 hedefinde11,5m/sn tavanına yaklaşır; hız değişimi en fazla0,25m/sn² ile yumuşatılır. En iyi skor deneme sonunda saklanır. Tekrar deneme skor/parkur/hızı sıfırlar, rekoru korur.

**Ayarlar** ses açık/kapalı, ses düzeyi ve kontrol açıklamasını içerir. Tercihler uygulama yeniden açılınca korunur. Kamera ve GUI görüntüleri otomatik testte gerçek URP'den alınır; ses duyumu ve fiziksel cihaz kabulü ayrıca bekler.

Dar geçitte dar kal; orta bariyerden önce açıl. Enerjiyi geniş bağla, altın bonusları kürelerle topla. Amber Pulse hedefine yaklaşınca **MENZİLDE** ipucunda bırak. Kapalı hedefe çarpma denemeyi bitirir; **Yeniden dene** ile baştan başla. Kırmızı engeller Pulse ile açılmaz.

Önceki 180 m temel prototip ve 130 m zamanlama alıştırması `Prototype.unity` / `Challenge.unity` sahnelerinde tarihsel karşılaştırma için korunur. Yeni başlangıç **MainMenu** sahnesidir. Sahne oyun dünyasını Play sırasında kurar; Edit görünümünde boş olması beklenir. Yeni içerik `LevelCatalog` / `EndlessCourse` verisindedir; bu ilk sürüm bir görsel bölüm editörü içermez.

## Tekrarlanabilir kontroller

Unity Editor'ü kapattıktan sonra proje kökünde PowerShell ile:

```powershell
./Tools/Unity.ps1 -Action Prepare
./Tools/Unity.ps1 -Action EditMode
./Tools/Unity.ps1 -Action PlayMode
./Tools/Unity.ps1 -Action Android
```

`-UnityPath` farklı kurulum yolu verebilir. Log/XML çıktıları `output/validation`, güncel test APK'sı `output/android/TetherRush-Optimized.apk` altında oluşur; build/cache çıktıları Git'e alınmaz. Android desteği için Unity Hub'da Android Build Support, SDK/NDK ve OpenJDK kurulu olmalıdır. `-Action Windows` güncel önizlemeyi `output/windows-optimized/TetherRush.exe` altında üretir. Eski M1/M2/cila/ilk menü çıktıları karşılaştırma için ayrı klasörlerde korunur.

Android araçları Türkçe karakter içeren proje yollarını kabul etmez. Bu klasör için `-Action Android` aynı depoya işaret eden geçici bir ASCII junction yolunu otomatik kullanır; kaynaklar taşınmaz veya kopyalanmaz. Bağlantı TEMP altında kalır ve tekrar kullanılır. Editor'dan doğrudan Android build almak için de ASCII bir proje yolu gerekir; mevcut klasörde Editor oynanışı etkilenmez.

Fiziksel Android cihaz testi kullanıcı kararıyla son aşamaya ertelendi. Editor ve otomatik testler gerçek cihaz performansı/dokunma doğrulaması yerine geçmez. Güncel yürütüm sonuçları [proje durumunda](Planning/PROJECT_STATUS.md) kaydedilir.
