# Harita UI yenilemesi

02.11.2026. Kapsam: yalnız `Assets/TetherRush/Scripts/Runtime/Presentation/LevelMapView.cs` ve bu rapor. Bağımlılıklar: önceden tamamlanan `Docs/Research/UI_SYNTHESIS.md`, `UI_REFERENCE_ARCADE.md`, `UI_REFERENCE_RUNNERS.md`; ana yöneticinin sağladığı `UiResources.Body`, `Display`, `CircleSprite` ve `UiIcon.Glyph`/`UiGlyph` sözleşmesi. Ortak kaynak/ikon/font dosyaları, testler, Unity, build, Git ve `PROJECT_STATUS.md` ana yöneticiye aittir.

## Uygulanan görünüm

- Koyu tesis şeridinde tek 3px sürekli rota: ilerlenen kısım cyan, erişilmeyen kısım slate. Beş büyük bay kartı, raylar ve dekoratif bloklar kaldırıldı.
- Düz 94px bölüm yüzleri; baskın 40px Barlow Condensed ExtraBold sayılar. Tamamlanan kırık beyaz + koyu sayı + küçük check, sıradaki cyan + koyu sayı + tek ince cyan halka, kilitli mat slate + sessiz sayı + küçük lock. Durum yalnız renkle anlatılmaz.
- Eski küre ve halo sprite parametreleri API'de kalır; harita düz ortak `CircleSprite` kullanır. Bevel, blur/glow ve her düğüm altındaki durum/bonus yazıları kaldırıldı.
- 72×72 geri dokunma alanında 28px Back ikonu, 38px `Leveller` başlığı ve kısa `12/25` ilerleme bilgisi. Lejant yok.
- Sabit alt satır yalnız odaklanan bölüm numarasını; odaklanan bölüm tamamlanmışsa mevcut en iyi bonusu crystal ikonu ve sayıyla gösterir. Tekrarlanan kaydırma talimatı yok.
- Palet: Background `#0D1722`, Surface `#1A2A3B`, Paper `#F5F2E9`, Muted `#8393A3`, Line `#314356`, cyan mevcut Tether rengi. Font stilleri gerçek font ağırlıkları için Normal.

## Korunan davranış ve kabul

`Initialize`, `SetLevels`, `FocusLevel`, `NodeCount`, `ScrollPosition` ve `LevelMapCircuit.SetRoute` imzaları korunur. 25 düğüm 4004px içerikte aynı sinüslü konum formülünü kullanır; 1 altta, 25 üstte. Düğüm Button nesneleri hâlâ `Level N` adında ve 94px çapındadır. Sıralı kilit, tamamlananı tekrar oynama, index callback'i, ScrollRect ayarları, sürükleme/tıklama koruması, odak formülü ve gerçek viewport değişiminde yeniden odaklama korunur. Header/footer maskelenen viewport dışında kalır.

`LevelMapCircuit` hâlâ `MaskableGraphic` ve `[RequireComponent(typeof(CanvasRenderer))]` kullanır; oluşturulurken açık `AddComponent<CanvasRenderer>()` da korunur. İkonlar da CanvasRenderer ile oluşturulur, raycast almaz. Yol mesh'i viewport maskesiyle kırpılır.

Doğrulama: kaynak dosya ve mevcut PlayMode çağrı sözleşmeleri salt okunur incelendi; yukarıdaki sabitler, isimler, API ve davranış kodları karşılaştırıldı. Bu uzman görevinde test/Unity/render/build çalıştırılmadı. Ana yönetici entegrasyonda yeni ve ilerlemiş profil, 1/13/25 odakları, 540×960/540×1170 render, kilit/yeniden oynama, drag guard ve viewport resize regresyonlarını çalıştırmalı. Görsel kabul gerçek Unity kareleriyle yapılmalı; bu rapor render kabulü iddia etmez.

Engel: yerel bağımsız derleme yapılmadı; ortak UiResources/UiIcon ana yönetici entegrasyonuna bağlı. Başka dosya veya oyun mekaniği değiştirilmedi. `PROJECT_STATUS.md` güncellemesi ana yöneticide.
