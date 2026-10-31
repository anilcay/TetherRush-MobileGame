# M2 doğrulama kaydı

31.10.2026. Unity 6000.3.19f1 / URP 17.3.0 / Input System 1.19.0. M2, kullanıcının devam onayıyla üretilmiştir; bu onay ayrı manuel testlerin yapıldığı anlamına gelmez. Fiziksel Android cihaz testi D-017 ile son aşamaya ertelenmiştir.

## Kapsam

- M1 temel parkuru (180 m) korunur; 130 m zamanlama parkurunda iki hareketli duvar ve bir dönen kol eklenir.
- 11 gerçek Blender modeli, düzenlenebilir kaynak, yeniden üretim/export/validasyon scriptleri ve 3.000 üçgenlik kaynak kit.
- Yeni shader'larla 3D yüzey gölgeleme; enerji akışı, hazır bağ, Pulse, kapı/blok açılışı, kayıp/başarı geri bildirimi.
- Tek ses kaynağında procedural kısa sesler; ses düğmesi ve iki parkur arasında seçim. Ek Pulse kontrolü yoktur.
- Görsel animasyon kökleri ve hareketli engel çarpışmaları ayrı tutulur. Hareketli engellerin zamanı `Model.Distance / ForwardSpeed` üzerinden hesaplanır; pause/retry tutarlılığı korunur.

## Yürütüm

| Kontrol | Durum | Kanıt / sınır |
| --- | --- | --- |
| Blender üretim ve bağımsız FBX re-import | GEÇTİ | Blender 5.2.3 LTS; 11/11 merkez pivot, 1x1x1 bounds, üçgen/materyal kontrolü. `ArtSource/Blender/fbx_validation.json`. |
| Unity C# / shader import / sahne hazırlığı | GEÇTİ | Prepare exit 0; Challenge sahnesi ve iki build sahnesi mevcut. |
| EditMode çekirdek testleri | GEÇTİ | Unity Test Runner 29/29. |
| PlayMode testleri | GEÇTİ | Son kaynaklarla 31/31; toplam 60/60. Kalıcı vaka özeti `Results/M2_test_results.json`. |
| Unity görsel inceleme | GEÇTİ | Gerçek URP kamera çıktılarında Ready, dolu enerji, Pulse ve Challenge incelendi; düzeltme sonrası kapı ipucu tekrar kontrol edildi. Kareler `Docs/Art/Previews/M2_*.png`. |
| Windows M2 build | GEÇTİ | Build exit 0; `output/windows-m2/TetherRush.exe`, yapı boyutu 163.432.714 bayt. |
| Android M2 test build | GEÇTİ | Build exit 0; `output/android/TetherRush-M2.apk`, 37.807.716 bayt. |
| APK kimlik / mimari / imza | GEÇTİ | aapt: `com.anilcay.tetherrush`, 0.2.0 (2), arm64-v8a, min API 25 / target 36, portrait. apksigner verify exit 0, APK Signature Scheme v2=true. |
| Cihaz performansı / gerçek dokunma | ERTELENDİ | Android telefon yok; D-017. Editor testi cihaz performans kanıtı değildir. |
| Son sanat ve kontrol hissi kabulü | KULLANICI İNCELEMESİ | Somut Unity görüntüleri ve oynanabilir build üzerinden değerlendirilir. |

## Otomatik test kapsamı

29 çekirdek sözleşmesi, M1'in 11 parkur testi, 12 sürekli hareketli OBB geometri vakası, 5 gerçek ikinci parkur testi ve 3 model/efekt/render testi vardır. İki rotanın bitirilebilirliği, yanlış ayrımda ölüm, hareketli engel tunneling, gerçek OBB köşe ıskası, kolun yatay fazda geniş kürelere erişmesi, pause/retry pozları, görsel kök izolasyonu, tek ses kaynağı ve efekt temizliği kontrol edilir.

Kamera testleri mock çizim üretmez: `RenderPipeline.SupportsRenderRequest` ile gerçek URP başlatılır, `SingleCameraRequest` ile 540x960 RenderTexture'a oyun ve HUD çizilir, GPU çıktısı PNG olarak okunur. Dört kare `output/previews/M2_*.png` altında üretilir; onaylanan teslim kareleri `Docs/Art/Previews` altında saklanır. Batch testlerinde ses susturulur; ses duyumu ayrıca insan incelemesine açıktır.

FBX importu animasyon/collider üretmez; eksen dönüşümü mesh verisine uygulanır, kaynak normalleri alınır. Oyun görselleri kontrol edilen semantik Body/Dark/Accent materyallerine bağlanır. Kaynak kitin gölgeli Blender görseli Unity ekran görüntüsü yerine kabul edilmez.

## Teknik başvurular

[Unity ModelImporter](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityeditor/modelimporter) import seçeneklerini; [Unity render isteği API'si](https://docs.unity.com/en-us/engine/6000.0/script-reference/unityengine/rendering/renderpipeline/submitrenderrequest) kamera çıktısı alma yolunu açıklar. Efektlerin native MaterialPropertyBlock nesnesi MonoBehaviour field initializer'ında değil, Initialize sırasında oluşturulur.

Android'in ilk M2 paketleme denemesinde Gradle `mergeDebugNativeLibs`, eski üretilmiş `arm64-v8a` klasörünün OneDrive ReadOnly bayrağı nedeniyle durdu. `AndroidCacheCompatibility` yalnız doğrulanan `Library/Bee/Android/Prj/IL2CPP/Gradle/{unityLibrary,launcher}/build` altındaki dizinlerde bu bayrağı kaldırır. Kaynak, SDK, dışa aktarılmış bağımsız proje veya ACL değiştirmez; dosya silmez. Runtime kodu bu build uyumluluk düzeltmesinde değişmedi. Son paket sonucu tabloda ayrıca kaydedilir.

Yeniden build'de callback 419 üretilmiş dizindeki ReadOnly bayrağını temizledi ve Android build başarıyla tamamlandı. APK SHA-256: `783EB78CE07A714FC5C060D20FF42B56B722FB0A2EE3CB6E09CB4C5B897E65A8`. Bu debug/development test paketidir; cihazda çalıştırma veya mağaza yayını yapılmadı. M1 build çıktıları karşılaştırma için korundu.
