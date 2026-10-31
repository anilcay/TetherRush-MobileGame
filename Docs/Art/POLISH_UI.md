# HUD cila teslimi

31.10.2026 — D-021/022 kapsamında, oyun kurallarına müdahale etmeden sunum katmanı.

## Dosyalar ve bağımlılıklar

- `Assets/TetherRush/Scripts/Runtime/Presentation/VisualTheme.cs`: ortak semantik renk kaynağı. Şeftali/lavanta küre kimliği; camgöbeği bağ/enerji, amber hedef, pembe-kırmızı tehlike ve lacivert zemin.
- `Assets/TetherRush/Scripts/Runtime/Presentation/GameHud.cs`: çalışma zamanında uGUI Canvas, GraphicRaycaster, safe-area kökü, düğmeler ve gerekirse InputSystemUIInputModule/EventSystem üretir. Bağımlılıklar mevcut Runtime assembly içindeki Core, Unity UI ve Input System'dir.

## Entegrasyon API'si

Namespace: `TetherRush.Presentation`.

`GameHud.Initialize(bool advanced, Action startOrResume, Action pause, Action toggleAudio, Action otherCourse)` bir kere çağrılır. Callback'ler ana oyun yöneticisine aittir; HUD model, sahne veya ses durumunu doğrudan değiştirmez. `GameHud.Canvas` girdi raycast kontrolü için açıktır.

Her kare `Present(HudState state, float dt)` çağrılır. `HudState`: Core `RunPhase Phase`; bool `AdvancedCourse`, `AudioEnabled`, `CanSwitchCourse`; float `Distance`, `FinishDistance`; int `Energy`, `EnergyCapacity`, `Bonus`, `ActivatedTargets`; string `Hint`, `FailureReason`.

Eski Canvas ve eski UI kurulum/güncelleme kodu yönetici entegrasyonunda kaldırılmalıdır; iki HUD aynı anda üretilmemeli. Eski EventSystem varsa yeni bir tane eklenmez. Sahne ömrü boyunca HUD ve kökü aynı oyun yöneticisi altında tutulmalıdır. Native sprite/texture yalnız Initialize sırasında oluşturulur ve OnDestroy ile serbest bırakılır.

## Yerleşim ve davranış

- 540×960 referans; ekran boyutu veya Android safe area değişince kök yeniden oturur.
- Üst enstrüman paneli 140 birim: başlık/parkur, ses/duraklat, mesafe/bonus/hedef, yumuşak ilerleme rayı, segmentli enerji. 35 birimlik ipucu hapı panelden ayrıdır.
- Alt durum kartı 220 birim, güvenli alanın altından 45–265 arasında; ortadaki oyun ve küreler açıkta kalır. Kart arka planı dokunmayı yakalar, tam ekran perde oluşturulmaz.
- Hazır, duraklatılmış, başarısız ve tamamlanmış durumların kısa Türkçe metinleri ve tek birincil eylemi vardır. İzinli parkur değişimi hazır/başarısız/tamamlanmış hâllerde ikinci düğme olarak görünür. Duraklatmada görünmez; birincil düğme genişler.
- Düğmeler 48 birim yüksekliğinde; metin ve dekor raycast yakalamaz. Ses her zaman erişilebilir. Duraklatma yalnız koşarken görünür.
- Pulse düğmesi yoktur. Kontrol hatırlatıcısı basılı tut/açıl ve bırak/daral şeklindedir; hazır kartı tam enerji ve menzilde bırakmayı açıklar.
- Paylaşılan, kenarları yumuşatılmış dokuz dilimli sprite: harici font, resim veya ücretli varlık gerektirmez. LegacyRuntime fontunun desteklediği Türkçe ve sade karakterler kullanılır.

## Kabul ve doğrulama durumu

Statik inceleme: belirtilen API, callback sınırı, safe-area güncellemesi, renk rolleri, kaynak serbest bırakma ve raycast hedefleri kodda kontrol edildi. Bu alt görevde Unity çalıştırılmadı; derleme, gerçek ekran görüntüsü, notched/tall Android oranları, duraklat/devam ve düğmelerin oyun girdisini engellemesi ana yönetici entegrasyonunda doğrulanmalıdır. Fiziksel cihaz kabulü henüz yapılmadı.

Ana yönetici teslim kayıtlarını `PROJECT_STATUS.md` dosyasına ekler; bu alt görev dosyayı değiştirmez.
