# Görsel cila doğrulaması — 0.2.1 (3)

31.10.2026. Kullanıcı D-021 kapsamında güçlü görsel cila istedi; sonraki iş paketi bu tur bitene kadar başlatılmaz. Android fiziksel cihaz testi D-017 ile son aşamaya ertelidir.

## Görsel kapsam

- Tek `VisualTheme` paleti: şeftali/lavanta küreler ve hem normal hem hazır durumda camgöbeği bağ; lacivert tesis, mercan tehlike işaretleri, amber Pulse hedefleri.
- 11 modelin Blender'da cilalanması, SplitPylon/ChevronBarrier engel görünümleri ve dekoratif RelayPylon ile 14 gerçek FBX. 6.548 üçgen; tekrarlanan track iki renderer; kapı üç, kırılabilir hedef dört parça kalır.
- Yüzeylerde yönlü aydınlatma, kenar yansıması ve uzaklık sisi; gradient tesis atmosferi ve deck yüzeyine yerleşen yumuşak küre gölgeleri.
- Ayrı yumuşak bağ katmanı, küre halkaları/izleri, enerji/bonus/Pulse ve hedef parçalanma cilası. Sabit efekt havuzları ve duraklatma/yeniden deneme temizliği korunur.
- 540x960 referanslı, safe area uyumlu, yuvarlatılmış HUD; küçük alt durum kartı oyun objelerini görünür tutar. Ek kontrol veya Pulse düğmesi eklenmez.
- Dar geçit/orta bariyer görselleri değişir; çarpışma boyutları, parkur yerleşimi, ileri hız ve tüm oyun kuralları korunur. Yeni çevre kuleleri dekoratiftir.

## Doğrulama durumu

| Kontrol | Sonuç | Kanıt / sınır |
| --- | --- | --- |
| Blender export / bağımsız re-import | GEÇTİ | 14/14; 6.548 üçgen, 20 mesh. ArtSource/Blender/fbx_validation.json. |
| Unity Prepare / C# / shader | GEÇTİ | Prepare exit 0; 5 shader Editor grafik cihazında destekleniyor. Runtime shader referansları build'e dahil. |
| EditMode | GEÇTİ | 29/29 çekirdek sözleşmesi. |
| PlayMode | GEÇTİ | Son kaynaklarla 32/32; toplam 61/61. Results/Polish_test_results.json. |
| Gerçek Unity görüntüleri | İNCELENDİ | 540×960 URP; Ready, Foundation, Pulse, Challenge, Failed. Docs/Art/Previews/Polish_*.png. |
| Windows önizleme | GEÇTİ | Son kaynaklarla strict development build exit 0; output/windows-polish/TetherRush.exe. Build raporu 163.735.297 bayt. |
| Android test paketi | GEÇTİ | Build exit 0; output/android/TetherRush-Polish.apk; 37,924,078 bayt. |
| APK kimlik / imza | GEÇTİ | com.anilcay.tetherrush, 0.2.1 (3), arm64-v8a, min API 25 / target 36, portrait. apksigner verify: v2=true. |
| Meta / GUID / JSON / whitespace | GEÇTİ | Eksik meta veya yinelenen GUID yok; kaynak JSON'lar ve git diff --check geçiyor. |
| Fiziksel cihaz | ERTELENDİ | D-017; gerçek Android FPS/dokunma/ses kabulü açık. |

Kaydedilen kareler AI mockup değildir; gerçek Unity URP kamera çıktısıdır. Son sanat beğenisi kullanıcıya aittir. Windows test paketi aynı bilgisayarda denenebilir; otomatik build'in başarılı olması insanın uygulamayı oynadığı anlamına gelmez.

## Görsel inceleme bulguları

İlk kamera incelemesinde arka planın kamera temizleme rengiyle düz kaldığı saptandı. Kamera üzerinde açık Skybox materyali ve native skybox vertex yolu kullanılarak düzeltildi; son karelerde lacivert ufuk geçişi görüldü. Gölgeler track tam sınırı yerine gerçek Deck mesh üstünden konumlanır.

Yeniden deneme sırasında core izlerinin eski parkur sonundan uzama riski giderildi: önce simülasyon köklerinin başlangıç pozu görsellere uygulanır, sonra efekt havuzları/izleri sıfırlanır. Mevcut izolasyon testine bütün wake noktalarının yeni başlangıç pozunda kaldığı denetimi eklendi.

## Kalan kabul

Fiziksel cihaz performansı ve gerçek dokunma kullanıcı kararıyla ertelendi. Son görsel beğeni ve kontrol/ses hissi kullanıcı tarafından Unity Editor veya Windows önizleme üzerinden değerlendirilecek. Batch kamera görüntüleri gerçek URP çıktısıdır; cihaz FPS kanıtı değildir.

APK SHA-256: `9796ca33d4aebaf652484347ac291fb28e4e183c51216ccdb426d1b18df38339`. Development test paketidir; fiziksel cihazda çalıştırılmadı. Build raporunun toplam dosya boyutu yerine doğrudan üretilen APK dosyasının boyutu kaydedildi. Önceki M1/M2 paketleri korundu.
