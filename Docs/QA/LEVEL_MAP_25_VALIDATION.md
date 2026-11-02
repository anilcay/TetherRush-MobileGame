# 25 bölümlük harita — 0.4.0 (5)

02.11.2026. D-029/030: 25 bölüm, yalnız numaralı enerji tesisi haritası ve sıralı açılma kullanıcı tarafından onaylı. Unity 6000.3.19f1 / URP 17.3.0. Fiziksel cihaz testi D-017 gereği son aşamada.

## Kapsam ve oyun kuralları

- İlk beş bölümün kimliği/oynanış geometrisi korundu. Sonraki 20 bölüm bağımsız engel dizileri; son bölüm 280 m ve üç ayrı şarj/Pulse hedefi içerir. Bölümler 1–25 olarak görünür; özel isimler gösterilmez.
- İlk bölüm açık başlar; başarı bir sonraki bölümü açar. Tamamlananlar yeniden oynanır. Map düğmesi ve uygulama katmanı kilitli başlangıcı ayrı ayrı engeller. Bölümden Haritaya dön doğrudan haritayı açar; başarıda yeni açılan düğüme odaklanır. Son bölümde sonraki eylemi yoktur.
- Harita 4.004 referans birimlik, 25 büyük numaralı soket ve kıvrımlı enerji hattıdır. Beş görsel tesis bölmesi, tamamlanma tikleri/bonus rekorları, sıradaki düğüm halesi ve kilitli durumlar vardır. Viewport haritayı keser; başlık ve alt açıklama sabittir.
- Önceki v1 kaydının beş tamamlanma/bonus değeri, Sonsuz Akış rekoru ve ses ayarları v2/25 diziye taşınır; yeni 20 değer false/0. Aynı PlayerPrefs anahtarı korunur, yükleme yazma/silme yapmaz. Eski sıradışı tamamlanma sırası en yüksek biten + sonraki düğümü açık tutar; eski ilerleme geri alınmaz.
- Tek kontrol, sabit ileri hız ve mevcut Pulse/çarpışma kuralları korunur. Sonsuz Akış ve ayarlar devam eder. Yıldız/para/reklam sistemi eklenmez.

## Son yürütüm

| Kontrol | Sonuç | Kanıt / sınır |
| --- | --- | --- |
| Unity hazırlığı | GEÇTİ | Prepare exit0, mevcut MainMenu başlangıcı korunur. |
| EditMode | GEÇTİ | 96/96; çekirdek model, profil/v1→v2 göç/sıralı açılma/bozuk kayıt/monoton rekor. |
| PlayMode | GEÇTİ | Son kaynaklarla 86/86; toplam182/182. Results/LevelMap25_test_results.json. |
| 25 gerçek bölüm | GEÇTİ | Her bölüm gerçek TetherRushGame ile SafeRoute basma/bırakma olaylarında Completed. Başarı kaydı ve sonraki bölüm kontrol edilir; maksimum 280m rotalar dahil. |
| Harita erişimi | GEÇTİ | 25 düğüm, ilk1 açık, kilitli25 UI callback ve doğrudan app çağrısıyla başlatılamaz; tamamlananlar tekrar açılır; başarı sonraki düğümü açar. |
| Kaydırma ve dönüş | GEÇTİ | uGUI ScrollRect/Button pointer olaylarında kaydırma sahneyi başlatmaz; yer değiştiren bırakma da reddedilir; gerçek tık başlatır. Haritaya dönüş ve yeni açık düğüm odağı doğrulanır. Telefon dokunması değildir. |
| Görüntü ve oran | İNCELENDİ | Gerçek URP, 540×960 ve540×1170; başlangıç, ilerleme, son kilitli ve25/25 tamamlanmış örnek profiller. Odaktaki13 numara iki viewport'ta da merkezde; ilk/son odak0/1. |
| Windows | GEÇTİ | Strict development build exit0; output/windows-level-map/TetherRush.exe; build raporu163.810.056 bayt. |
| Android | GEÇTİ | Strict development build exit 0; output/android/TetherRush-LevelMap25.apk; 38,005,586 bayt. |
| APK kimlik / imza | GEÇTİ | com.anilcay.tetherrush, 0.4.0 (5), arm64-v8a, min API 25 / target 36, portrait; APK Signature Scheme v2=true. |

## İnceleme bulguları

İlk UI çalıştırmasında özel MaskableGraphic üzerinde CanvasRenderer eksikliği yakalandı; komponent açıkça eklenip RequireComponent ile korundu. Oyun rotalarından önce aynı menü kurulum hatasının yol açtığı başarısızlıklar giderildi. Daha sonra küçük/yeniden boyutlanan viewport'ta ilk düğüm odağı kontrolü yakalandı; ilk/son sınır açıkça belirlenip viewport değişiminde odak korunarak düzeltildi. Son182 test bu düzeltmelerle geçer.

Gerçek görüntüler Docs/Art/Previews/Map25_*.png altındadır. İlerlemeli/tamamlanmış kareler testte enjekte edilen örnek profildir; kullanıcı kaydı değiştirilmez. Dinamik font/clip durumunun ilk offscreen render'da geçici eski kalmasını önlemek için yakalama bir ısınma render'ı ve sonraki frame'den sonra okunur. Görsel çıktı AI mockup değildir.

## Açık kabul

Fiziksel Android performansı/dokunması, çentik, insan ses duyumu ve uygulama kapat/aç kayıt kabulü ertelidir. Testler mevcut kullanıcı PlayerPrefs anahtarını yazmaz/silmez. Sanat beğenisi ve 25 bölümün insan tarafından zorluk ritmi kullanıcı incelemesine açıktır. Kaynak içerik Docs/Design/LEVELS_25.md, harita tasarımı Docs/Art/LEVEL_MAP_DESIGN.md, kayıt sözleşmesi Docs/Planning/SAVE_DATA.md içindedir.

APK SHA-256: `a45d1ca63fb95f28e4cbba0ea4f90e6fbdac920db9ccaae52f8680270e45d9cb`. Boyut APK dosyasından alındı. Önceki paketler korunur; cihazda kurulum yapılmadı. Meta/GUID/JSON ve doküman bağlantıları doğrulandı; son staged whitespace kontrolü commit öncesi yürütülür.
