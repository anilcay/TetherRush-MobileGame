# Engel modelleri — 0.6.0 (7)

04.11.2026. Kullanıcının D-033 isteğiyle kısa araştırma sonrası dört yeni Blender modeli tasarlanıp mevcut parkur rollerine entegre edildi. [Araştırma ve seçim](../Research/OBSTACLE_VARIANTS.md), [Blender sözleşmesi](../Art/OBSTACLE_VARIANTS.md), [QA planı](OBSTACLE_VARIANTS_PLAN.md).

## Değişiklik

ClampBarrier dar geçit yan bloğu, CapacitorBank merkez engeli, HydraulicSlider yatay kayan duvar ve TurbineBeam dönen kiriş görünümü sağlar. Model seçimi yalnız BuildContent sunumunda yapılır. Açık sabit karakter hash'i ve çeyrek metreye nicemlenmiş dünya Z anahtarı, UnityRandom tüketmeden seçim yapar; simülasyon konumuna nicemleme uygulanmaz. Aynı dar geçidin iki tarafı aynı modeli alır. Eski kit seçkide kalır; legacy prototipler kendi görünümünü korur. Normal engellerin gövdesi/Accent yuvaları mevcut kırmızı tehlike materyallerini alır, Pulse hedefleri aynı kalır.

CourseDefinition/LevelCatalog/EndlessCourse/TimedHazard ve Core kaynakları değişmedi. Bütün temas hacimleri, SafeRoute, hareket fazları, hızlar, enerji ve kayıt şeması aynı. Yeni FBX animasyonu veya collider yok; mevcut root hareketi yeni mesh'i sürer. Sonsuz Akış aynı seed/index ile aynı modeli seçer ve emekli chunk ile temizler.

## Doğrulama kaydı

| Kontrol | Durum | Kanıt |
| --- | --- | --- |
| Blender üretim / reimport | GEÇTİ | 4/4; toplam 1732 üçgen, 4 mesh. Model başına 50 yatay temas rayı. ArtSource/Blender/ObstacleVariants/fbx_validation.json. |
| Eski varlıklar | GEÇTİ | Önceki 14 FBX SHA-256 değerleri değişmedi. Yeni kit toplam18 model/8280 üçgen/24 mesh. |
| Unity Prepare | GEÇTİ | exit0; kontrollü FBX importu ve derleme. |
| EditMode | GEÇTİ | 96/96; veri, çekirdek, kayıt ve göç regresyonu. |
| PlayMode | GEÇTİ | 95/95; yeni6 + mevcut89. Toplam191/191. 25 gerçek bölüm ve Sonsuz Akış regresyonları dahil. Results/ObstacleVariants_test_results.json. |
| Gerçek URP görüntü | İNCELENDİ | Dört gerçek 540×960 kare Docs/Art/Previews/Obstacles_*.png. İlk bölüm z16/36 kenet/kapasitör; dördüncü z24/70 kızak/türbin. Results/ObstacleVariants_inventory.json. |
| Windows strict build | GEÇTİ | exit0; output/windows-obstacle-variants/TetherRush.exe; build raporu164.215.015 bayt. |
| Android strict build / imza | GEÇTİ | exit0; gerçek APK 38,173,530 bayt. com.anilcay.tetherrush, 0.6.0 (7), ARM64, minAPI25/target36, portrait, v2=true. |

Blender paftası modelleme incelemesidir; gerçek oyun görünümü yerine sunulmaz. Bağımsız ray örnekleri tam bir hacim ispatı değildir; temas bandında doğrudan tam dikdörtgen taban geometrisi vardır. Gerçek Unity importu/ölçeği ve otoriter hareket eşleşmesi ayrıca test edilir.

## Açık kabul

Görsel beğeni ve engellerin insan tarafından anlık okunurluğu kullanıcı incelemesinde. Fiziksel Android performans/dokunma/çentik ve ses duyumu D-017 gereği sonda. Testler kullanıcı kaydını değiştirmez. Uygulama ve lisanslı UI aynı kalır; bu paket görsel engel çeşitliliğidir.

İlk PlayMode94/95 sonucu teslim sonucu değildir: yeni materyal testinde aynı görünen rengin shader float dönüşündeki çok küçük farkı tam eşitlik reddetti. Karşılaştırma 0.00001 hassasiyetle düzeltildi; üretim kodu değiştirilmedi ve tam95 yeniden geçti. Yeni6 senaryo gerçek FBX/renk/ölçek, deterministik seçim ve çift taraf uyumu, root hareket/pause/retry, emekli chunk sonrası Sonsuz görünümü, Pulse/fatal ve dört gerçek SafeRoute yakalamasını doğrular.

APK SHA-256: `c437d24a38a5f84be330f79fb2c3eae941feeb4d1ce55b2e6d7d3b027b68c9f2`. Kaynak/meta/GUID/JSON/doküman bağlantıları ve son staged whitespace kontrolü geçti. Build/cache dosyaları Git dışında tutulur.
