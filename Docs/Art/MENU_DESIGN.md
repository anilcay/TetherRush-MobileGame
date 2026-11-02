# Ana menü ve oyun HUD'u

02.11.2026 — Onaylı kapsam için uygulama notu. Gerçek Unity görüntüsü ve kullanıcı sanat kabulü ana yönetici doğrulamasını bekler.

## Görsel düzen

540 × 960 dikey uGUI, `LegacyRuntime.ttf`, `Screen.safeArea`. Lacivert yüzeyler, camgöbeği birincil eylem, şeftali/lavanta eş çekirdekler mevcut `VisualTheme` renklerini kullanır. Ücretli veya dış görsel varlık eklenmedi. Tek çalışma zamanı yuvarlatılmış sprite üretimi tüm panel ve kontrollerde paylaşılır; texture ve sprite `OnDestroy` ile bırakılır.

Ana sayfada büyük iki satırlı Tether Rush başlığı, birbirine camgöbeği bağla bağlı stilize çekirdekler, baskın **Leveller** eylemi ve rekoru gösteren **Sonsuz Akış** kartı bulunur. Küçük **Ayarlar** düğmesi üst sağdadır. İç sayfalar tutarlı başlık, alt açıklama ve **Ana menü** geri düğmesi kullanır. Sahte kilit, yıldız, para veya ilerleme ödülü yoktur.

Beş bölüm kartı tek ekranda numara, ad, kısa ders, süre, tamamlanma durumu ve en iyi bonusu gösterir. Tüm kartlar oynanabilir. Veriler menü tarafından üretilmez; uygulama yöneticisinden gelir. Süre dizisi saniyedir ve en yakın tam saniyeye yuvarlanır.

Sonsuz Akış ekranı rekoru, çarpışana kadar sürme kuralını, sabit ileri hızı ve `floor(mesafe) + bonus × 100` skorunu açıklar. Ayarlarda gerçek ses aç/kapat, 0–1 ses düzeyi ve tek dokunuş/Pulse kontrol açıklaması vardır. Ses sürgüsünün dokunma alanı 50 referans piksel yüksekliğindedir.

## Yönetici bağlantısı

`MainMenuView.Initialize(Action<int>, Action, Action<bool>, Action<float>)` yalnızca bir kez çağrılır. Bölüm callback'i sıfır tabanlı indeksi alır. Ses callback'leri yalnızca kullanıcı değişikliğinde tetiklenir; `ShowHome` ve `ShowSettings` başlangıç değerlerini bildirim göndermeden uygular.

`RequestedLevels`, `RequestedHome`, `RequestedEndless`, `RequestedSettings` public `Action` alanlarını yönetici bağlar. Bu olaylar görünüm verisi talebidir. Yönetici karşılık olarak `ShowHome`, `ShowLevels`, `ShowEndless`, `ShowSettings` çağırır. Oynat callback'leri gerçek deneme başlangıcıdır. `Hide` menü Canvas çocuğunu kapatır. Ekranlar ilk kurulumda yaratılır; navigasyonda tekrar üretilmez. Yalnızca ekran boyutu veya güvenli alan değişirse güvenli alan güncellenir.

`GameHud` eski `Initialize` ve `HudState` alanlarını korur. Ek snapshot alanları: `CourseTitle`, `Endless`, `CanGoHome`, `CanAdvance`, `Score`, `BestScore`. `SetNavigation(Action home, Action next)` yeni menü ve sonraki bölüm eylemlerini bağlar. `CanGoHome` hazır/duraklatılmış/başarısız/tamamlanmış durumlarda menü düğmesini açar; koşu sırasında kart kapalıdır. `CanAdvance` yalnızca tamamlanmış durumda ikinci eylemi **Sonraki bölüm** olarak gösterir; eski parkur değiştirme davranışı diğer durumlarda korunur.

Sonsuz HUD bitiş hedefi ve sonlu ilerleme rayını gizler; skor, rekor, tam metre mesafesi ve bonus görünür. Başarısızlık kartı sonuç skorunu ve rekoru tekrar gösterir. Snapshot alanları verilmezse eski prototip davranışı sürer.

## Doğrulama sınırı

Bu alt görevde kaynak, API, callback ve yerleşim hesabı statik olarak incelendi. Unity çalıştırılmadı; derleme, gerçek ekran görüntüsü, dokunma, ses/persistence entegrasyonu ve oran kontrolleri ana yöneticiye aittir. Önerilen kabul: ana menü → beş kart → hazır → oyun → duraklat → menü; tamamlanma → sonraki bölüm; sonsuz çarpışma sonucu/rekor; ses değerlerinin geri dönüşte korunması; çentikli telefon güvenli alanı.

Ana entegrasyon notu: paylaşılan panel sprite'ına ek olarak tek sefer üretilen iki dairesel core/halo sprite'ı kullanılır. Menü CanvasScaler genişlik eşleşmesi 0'dır; 540 referans genişlik uzun telefon oranında korunur. Gerçek 540×960 ve 540×1170 UI kareleri incelendi. 124 test ve build sonuçları Docs/QA/MENU_LEVELS_VALIDATION.md içinde; son kullanıcı sanat kabulü ve fiziksel cihaz incelemesi ayrıca bekler.
