# 25 bölümlük enerji hattı

02.11.2026 — Ana menüdeki beş kartın yerine, enerji tesisi temasına ait dikey bir bölüm haritası uygulanır. Yalnızca 1–25 sayıları kullanılır; tek tek bölüm isimleri gösterilmez. Candy Crush benzeri kaydırılan yol yalnızca gezinme referansıdır; görseller projenin kendi prosedürel tesis devreleridir.

## Görsel düzen

- Lacivert zemin ve beşer düğümlük beş tesis bölmesi. Bölme başına ray işaretlerinin sayısı, röle dirseği uzunluğu/yönü ve şeftali/lavanta/amber vurgu döngüsü değişir.
- Camgöbeği enerji hattı tüm düğümleri kesintisiz, yumuşak S kıvrımlarıyla birleştirir. Tamamlanan güzergâh daha parlaktır; kilitli güzergâh düşük kontrastlıdır.
- Her bölüm 84 birim çaplı dairesel düğmedir; 108 birim dış soket ve mevcut gölgeli küre sprite'ı kullanılır. Tamamlanan bölümde çizilmiş onay işareti ve en iyi bonus; sıradaki bölümde sabit enerji halesi; kilitli bölümde soluk düğme ve KİLİTLİ durumu görünür.
- Sabit üst alanda Ana menü, Leveller, tamamlanma sayacı ve durum açıklaması; sabit alt alanda kaydırma ipucu bulunur. Harita orta viewport içinde RectMask2D ile kesilir.

## Davranış ve API

`MainMenuView.ShowLevels` imzası korunur. Başlık içeriği kullanılmaz; bölüm sayısı titles/completed dizilerinin en büyük uzunluğundan alınır. Boşsa description/length dizileri yedektir. Home, Sonsuz Akış ve Ayarlar API'leri korunur.

`SequentialLevels` varsayılan false'tur; uygulama onaylanan sıralı ilerleme için true atar. En yüksek tamamlanan indeks + 1, açık son bölümdür (son bölümde clamp). Kilitli düğme `Button.interactable = false` olur; callback ayrıca erişimi kontrol eder. Uygulama katmanı da doğrulamalıdır.

`FocusLevel(int)` sıfır tabanlı indekse kaydırır; kilitli indekslere de görüntüleme amacıyla odaklanabilir, bölümü başlatmaz. `MapNodeCount` ve `MapScrollPosition` inceleme/test için salt okunurdur. ShowLevels sıradaki bölüme odaklanır; uygulama geri dönüşte son oynanan indeksi tekrar FocusLevel ile seçebilir. Update içinde scroll konumu zorlanmaz.

25 düğümde content yüksekliği **4.004 birim**: 130 alt/üst pay + 24 × 156 adım. Merkezler `y = 130 + i × 156`, `x = width × (0.5 + sin(i × 0.86) × 0.285)` biçiminde hesaplanır. Düğümler ve devre aynı formülü kullanır. 1 altta, 25 üsttedir. Focus normalized Y = clamp01((nodeY − viewportHeight / 2) / (contentHeight − viewportHeight)); 0 alt, 1 üst konumdur.

540×960 referans ve matchWidth=0 korunur. Viewport safe area içinde üstten 174, alttan 72 birim ayrılır; uzun telefonlarda yalnızca görünen harita alanı büyür. Düğme adları `Level N`; alt nesnelerde isim/yıldız/ödül ekonomisi bulunmaz. ScrollRect sürükleme iptaline ek olarak düğme pointer displacement/drag guard kullanır.

## Kaynak ve doğrulama sınırı

Facility/devre/işaretler tek MaskableGraphic mesh'inde üretilir; yeni texture, sprite veya native mesh alanı oluşturulmaz. Küre ve halo sprite sahipliği MainMenuView'dadır ve mevcut OnDestroy temizliği sürer. Harita her açılışta verilerden yeniden kurulur; kare başına animasyon/tahsis yoktur.

Bu alt görevde kaynak incelemesi yapıldı. Unity derlemesi, 25 bölüm kilit sırası, dokunarak kaydırma ve farklı ekran oranlarında gerçek görüntü kabulü ana yöneticinin entegrasyon doğrulamasına aittir; burada çalıştırılmış sayılmaz.

Ana entegrasyon notu: özel MaskableGraphic için CanvasRenderer açıkça eklenir ve RequireComponent ile korunur. Viewport boyutu değiştiğinde seçilen düğüm bir kez yeniden odaklanır; kullanıcı normal kaydırırken konum zorlanmaz. İlk/son düğüm doğrudan alt/üst sınırdır. Bölüm içinden Haritaya dön, tamamlanma sonrası yeni açılan düğüme odaklanır. Gerçek kamera yakalamaları font/maske güncellemeleri için bir render ısınması sonrası okunur. Son doğrulama Docs/QA/LEVEL_MAP_25_VALIDATION.md içindedir.
