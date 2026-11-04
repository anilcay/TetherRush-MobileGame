# UI yenilemesi — 0.5.0 (6)

04.11.2026. Unity 6000.3.19f1 / URP 17.3.0. Kullanıcının D-031 isteği: başarılı hypercasual UI örneklerini araştırıp menü ve oyun arayüzünü temaya uygun biçimde yenilemek. Araştırma 02.11'de uygulamadan önce tamamlandı; [sentez](../Research/UI_SYNTHESIS.md), [arcade](../Research/UI_REFERENCE_ARCADE.md), [runner](../Research/UI_REFERENCE_RUNNERS.md).

## Değişiklik ve kapsam

Barlow/OFL fontları ve özgün vektör simgeler ortak UiResources/UiIcon ile kullanılır. Ana menü gerçek Blender Core/TrackTile önizlemesi ve tek baskın Leveller eylemi içerir. Harita düz 94px numaralı soket ve tek rota kullanır; dekoratif bay, glow ve tekrarlanan durum yazıları kaldırıldı. Ayarlar ve Sonsuz Akış aynı düz yüzey dilindedir.

HUD üstte 103px; marka/metre/hedef tekrarları, büyük üst pano ve sabit alt açıklama kaldırıldı. İlk beş bölümde ipucu 2,2 saniye görünür, Pulse menzil uyarısı her bölümde geçerlidir. Sonuçta bir baskın eylem vardır. 25 bölüm, sıralı açılma, kayıt v2, tek tut/bırak kontrolü ve Pulse/çarpışma kuralları korunur.

## Yürütüm

| Kontrol | Sonuç | Kanıt / sınır |
| --- | --- | --- |
| Prepare | GEÇTİ | exit 0; font/sprite/3D kaynak importu ve C# derlemesi. |
| EditMode | GEÇTİ | 96/96; çekirdek, 25 bölüm verisi, kayıt göçü, sıralı açılma. |
| PlayMode | GEÇTİ | 89/89; toplam 185/185. Son kaynak sonuç özeti Results/UIRefresh_test_results.json. |
| Gerçek oynanış | GEÇTİ | 25 SafeRoute gerçek basma/bırakma ile Completed; Pulse/fatal önceliği, pause/retry, Sonsuz Akış stream sınırları. |
| UI etkileşim | GEÇTİ | Menü/ayarlar, kilitli erişim, haritaya dönüş, odak/viewport resize, sürükleme/tıklama ayrımı ve sonraki bölüm. |
| Yeni UI kabulü | GEÇTİ | Lisanslı fontların Türkçe glyphleri, sprite border, ilk/geç öğretici ve sürekli Pulse menzil uyarısı, görünür kontrol yükseklikleri, Pulse butonu olmaması, Blender önizlemenin doğru koordinat ve Home dışı kapanması. |
| Gerçek görüntü | İNCELENDİ | 540×960 / 540×1170 URP. Ana menü, ayarlar, Sonsuz, harita başlangıç/ilerleme/son/tam, oyun, pause, başarı ve ölüm. Docs/Art/Previews/UIRefresh_*.png. |
| Windows strict build | GEÇTİ | exit 0; output/windows-ui-refresh/TetherRush.exe; build raporu 164.069.363 bayt. |
| Android strict build | GEÇTİ | exit 0; output/android/TetherRush-UIRefresh.apk; gerçek APK 38,133,750 bayt. |
| APK kimlik/imza | GEÇTİ | com.anilcay.tetherrush, 0.5.0 (6), arm64-v8a, min API25 / target36, portrait; apksigner v2=true. |

## Bulunan ve giderilen sorunlar

İlk render'da 70px başlığın 78px alanı font satır yüksekliğine yetmedi; alan 110px yapıldı. Slider'ın driven anchors'ı tutamacı uzatıyordu; 28px merkezlenmiş handle alanı kullanıldı. Öğretici enerji durumunu tekrar etmek yerine ipucunun eylem kısmını gösterir. Blender model kütüphanesi bounds normalizasyonunu dünya başlangıcında yaptığından önizleme sahnesi kurulduktan sonra izole 500x koordinatına taşınır; konum testi eklendi.

İlk PlayMode 88/89 sonucu teslim sonucu değildir: yeni sonuç yakalama senaryosu pause sonrası iptal edilen basışı yeniden yapmadan devam ediyordu. Test gerçek kullanıcı gibi yeni basış gönderir. Üretim kontrol kuralı değiştirilmedi. Sonuç dinleyicisinin rekoru güncellemesinden sonra elle simülasyonda HUD sıfır zaman adımıyla yenilenir; capture gerçek yeni rekoru gösterir. Menü/harita offscreen URP yakalaması font/clip için iki render ve ara frame kullanır.

## Sınır

Referans oyunlar kurulup oynanmadı; resmî mağazaların gerçek ekran görselleri ve tanıtım kompozisyonları ayrı değerlendirildi. Referansların menü/ayar/sonuç sayfaları gözlenmiş gibi sunulmaz. Dış görseller repoya/oyuna alınmadı. Yeni tasarımın beğeni ve insan tarafından okunurluk kabulü kullanıcıda; fiziksel Android performansı, dokunma, çentik, ses duyumu ve kapat/aç kabulü D-017 gereği son aşamada. Testler kullanıcı kayıt anahtarını yazmaz/silmez.

APK SHA-256: `ddc535aa724841180fcd2cbe3886c3ea90b8b228339df32e0795f5c64be297f8`. Build/cache dosyaları Git dışındadır. Meta/GUID, JSON, yerel doküman bağlantıları ve staged whitespace kontrolü geçti.
