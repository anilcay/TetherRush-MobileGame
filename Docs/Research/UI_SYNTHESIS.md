# UI araştırmasından uygulama kararı

02.11.2026. Kullanıcı menü ve oyun UI dilini yapay/generik buldu; başarılı hypercasual örnekleri inceleyip ortak bir yönü oyuna uygulamamızı istedi. Araştırma uygulamadan önce yapıldı. Ayrıntılı örnek/URL matrisi UI_REFERENCE_ARCADE.md ve UI_REFERENCE_RUNNERS.md içindedir.

## Örneklem ve kanıt

Yedi resmî mağaza ürünü: Stack Ball ve Bridge Race 500M+; Helix Jump, Going Balls, Tall Man Run, Aquapark.io ve Color Bump 100M+ Android indirme sınıfı. Bunlar mağaza eşikleri, güncel sıralama veya başarıya UI'nın neden olduğu iddiası değildir.

- [Stack Ball](https://play.google.com/store/apps/details?id=com.azurgames.stackball): gerçek oyun içerikli karelerde ince mevcut→sonraki progress ve tek skor, arka plakasız bilgi.
- [Going Balls Apple görselleri](https://apps.apple.com/us/app/going-balls/id1499081620): küçük pause simgesi, ince progress, bölüm etiketi ve kenar sayaçları; merkez oyun alanı.
- [Tall Man Run](https://apps.apple.com/us/app/tall-man-run/id1599121822): bölüm numarası ve küçük köşe sayaçları, pano biçiminde üst HUD yok.
- [Helix Jump](https://play.google.com/store/apps/details?id=com.h8games.helixjump) ve [Color Bump](https://play.google.com/store/apps/details?id=com.colorup.game): sahne baskın; Helix harita benzeri numaralı yüzeyler tanıtım içinde, Color Bump küçük yüzde örneği. Reklam bandı, sticker veya el işareti gerçek HUD parçası sayılmadı.
- [Bridge Race](https://play.google.com/store/apps/details?id=com.Garawell.BridgeRace) ve [Aquapark](https://play.google.com/store/apps/details?id=com.aquapark.game): incelenen görseller çoğunlukla promo kompozisyonu; gerçek menü/ayar/sonuç için kanıt oluşturmaz.

Örnekler kurularak oynanmadı. Mağaza görsellerinin sürüm tarihi her zaman verilmez. Menü/ayar/sonuç düzenini bu oyunlarda gözlenmiş gibi sunmuyoruz; bu sayfalara ilişkin seçimler kendi ürün akışımız için tasarım çıkarımıdır. Dış görseller oyun varlığı veya repo içeriği olarak kullanılmaz.

## Tasarım yönü

Temel dünya aynı enerji tesisi: lacivert pist, şeftali/lavanta çekirdekler, cyan bağ. UI kimliği düz koyu yüzey, kırık beyaz eylem, cyan durum vurgusu ve teknik/sportif tipografiyle kurulur. Barlow SemiBold gövde, Barlow Condensed ExtraBold başlık; [Google Fonts resmî kaynağı](https://github.com/google/fonts/tree/main/ofl/barlow), OFL lisansı repoda. Windows sistem fontu veya başka oyunun ikonu kullanılmaz.

1. **Menü:** Tek satır wordmark, gerçek Blender core/track kompozisyonu, tek baskın Leveller butonu. Sonsuz Akış ikincil sade satır; ayarlar gear ikonu. Slogan, özellik pazarlama kartı ve yinelenen kontrol açıklaması yok.
2. **Oyun:** Üst bant en fazla yaklaşık110 referans piksel (eski sabit197px yerine). Pause ikonu, bölüm+ince progress, kristal+bonus ve gerekiyorsa beş hücreli tek Pulse göstergesi. Oyun markası, metre/hedef sayaç tekrarı ve sabit alt bant yok. Öğretici ilk beş bölümde kısa süreli; Pulse menzil uyarısı her bölümde öncelikli. Pulse butonu eklenmez.
3. **Sonuç:** Düz sonuç başlığı ve tek anlamlı sayı. Başarıda Sonraki bölüm baskın, tekrar/harita ikincil; hatada Tekrar dene baskın. Duraklatıldı açıklaması kısa. Motivasyon sloganı ve birbirine eş üç kart yok.
4. **Harita:** 25 sayısal düğüm/sıralı açılma/scroll/odak aynen. Daha düz tamamlanmış soket, tek current halkası, mat kilitli durum ve küçük check/lock; tekrarlanan durum etiketleri/bay dekoru/blur glow kaldırılır. Harita tipi tanınır kalır.
5. **Ayarlar:** Fonksiyonel ses satırları ve kontrol açıklaması. Uygulanmayan titreşim/müzik/ekonomi seçenekleri yok.

Tüm küçük ikonlar aynı vektör çizgi diliyle oluşturulur. Görünen ikon yaklaşık26px olsa da hitbox72 referans piksel hedeflenir; 540px tasarım/360dp örneğinde48dp'ye karşılık gelir. Bu fiziksel cihaz sertifikası değildir. [Android hedef boyutu kaynağı](https://support.google.com/accessibility/android/answer/7101858) en az48dp önerir.

## Dosya sahipliği ve kabul

Ana yönetici UiResources/UiIcon/font importu, gerçek Blender menü önizlemesi, App snapshot bağlantıları, test/render/build ve Git teslimi. Uzmanlar MainMenuView, GameHud ve LevelMapView dosyalarını ayrı sahiplikle uygular. Kontrol/Pulse/25 bölüm/kayıt v2 değişmez. Kabul gerçek Unity önce/sonra kareleri, tüm regresyon testleri, HUD alanı/işlevsellik ve build sonucudur; yalnız renk değişimi yeterli sayılmaz.
