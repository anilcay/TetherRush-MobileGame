# Optimizasyon — ölçüm ve doğrulama

Kullanıcı 04.11.2026 tarihinde oyunun optimizasyonunu istedi. Kapsam: aynı kontrol, temas sonuçları, 25 bölüm, kayıt, görsel kalite ve ses korunarak CPU/GC ve gereksiz çizim işini azaltmak. Android fiziksel cihaz testi D-017 gereği sonda; bu çalışma telefon FPS veya termal/pil ölçümü değildir.

## Yöntem

`OptimizationMeasurements` aynı Unity 6000.3.19f1 Editor/Mono ortamında Bölüm 1, Bölüm 25 ve sekiz parçalık Sonsuz Akış rotasını 1/60 s adımla yürütür. İlk 60 adım ısınmadır. Yalnız `SimulateStep` çağrısı zamanlanır; rota üretimi, assertion, dosya yazımı, kareler arası render ve ertelenmiş Destroy bu süreye dahil değildir. Ortalama/p50/p95/en yüksek süre ile toplam/en yüksek kare tahsis olayı saklanır. Bekleyen sonuç/pause için 512 sıfır zaman adımı ayrıca ölçülür. Sonsuz ölçüm gerçek parça spawn/retire ve mekanik olaylarını da içerir; bu yüzden sürekli akışa ait sıfır tahsis iddiasına dönüştürülmez.

Tahsis ölçümü `ProfilerRecorder`, `GC.Alloc`, `CollectOnlyOnCurrentThread` kullanır. Her çağrıdan önce recorder sıfırlanır, sonra durdurulur; native GC.Alloc örneklerinin sayısı alınır; marker Value süre taşıdığından bayt gibi yorumlanmaz. Ölçüm bilinen 4096 baytlık dizinin en az bir GC.Alloc olayı ürettiği doğrulanarak kalibre edilir ve örnek tamponu taşması reddedilir. İlk denenen `GC.GetAllocatedBytesForCurrentThread` bu Editor'da sıfır döndürdüğünden o ilk sonuçlar geçerli GC kanıtı olarak kullanılmaz. CPU örnekleri cihaz kare süresi veya FPS değildir.

Ana menü sekiz ısınma karesi ardından 30 kare bekletilir; `RenderPipelineManager.beginCameraRendering` üzerinden yalnız `Menu preview camera` çizimleri sayılır. Bu ölçü sabit önizleme için gereksiz yeniden çizimi doğrudan denetler; genel GPU süresini ölçmez.

```powershell
$env:TETHER_RUSH_PERF_LABEL = 'baseline' # karşılaştırmada 'optimized'
./Tools/Unity.ps1 -Action PlayMode -TestFilter TetherRush.Tests.OptimizationMeasurements
```

Ham veriler `output/performance` altında oluşur; incelenen kıyas kalıcı Results kaydına alınır. Varsayılan etiket `current`; tam regresyon önce/sonra kanıtını ezmez. Testler kullanıcı profilini yazmaz.

## Uygulama

1. HUD'da bağımlı değer değişince metin üretimi; koç metni ve sonuç yerleşimi önbelleği. Menüde sabit Blender önizlemesi için demand render, texture kaybında yeniden üretim.
2. Uzak statik/hareketli engelleri muhafazakâr Z aralığıyla eleme; ayrıntılı temas matematiği ve fatal→relay→pickup→release sırasını koruma. Sabit bölüm başlığını ve öğretici koşulunu önbellekleme. Sonsuz retirement üyeliğini açık tutma ve geçici listeleri azaltma.
3. Efektlerde kalıcı koordinat tamponları/toplu yükleme, değişmeyen materyal property yazımlarını atlama, görünmeyen aura geometrisini işlememe. Büyük pooling kapsamı ancak ölçüm ve yaşam döngüsü doğrulamasıyla alınır.

Etkin pipeline zaten Mobile URP'dir: HDR/depth/opaque kapalı, MSAA 2 ve renderScale 1. Kalite düşürerek kazanç gösterilmeyecek. PC adlı kullanılmayan asset'teki SSAO, etkin pipeline maliyeti olarak sayılmaz.

## Ölçülen sonuç

Windows Editor/Mono, Direct3D12 / NVIDIA GeForce GTX1650, aynı seed/rota ve aynı sayaçla bir önce/sonra koşusu. Bunlar telefon FPS sonuçları değildir. Valid baseline kaynak commit'i 025edda; ham veriler Results/Optimization_baseline.json ve Optimization_comparison.json içinde.

| Senaryo | Ölçülen adım | GC.Alloc olayı önce→sonra | SimulateStep ortalama µs önce→sonra |
| --- | ---: | ---: | ---: |
| Bölüm 1 |420|11.596→341 (−%97,06)|133,13→76,07 (−%42,86)|
| Bölüm 25 |2341|73.290→446 (−%99,39)|164,53→90,83 (−%44,79)|
| Sonsuz 8 parça |6552|432.102→12.919 (−%97,01)|177,49→69,46 (−%60,87)|
| Bölüm 1/25 bekleme |512/512|3.584/3.584→0/0|32,90/32,90→14,95/19,33|
| Sonsuz pause |512|3.072→0|39,98→21,58|

Sabit menü önizlemesi 30 karede 30→0 kamera çizimi. İlk açılışta iki warm pass ve görünürlük/context kaybı sonrası gerekli yenileme sürer. Efekt halo 96 ayrı nokta çağrısından 2 toplu yüklemeye, aktif ring 64 çağrıdan 1 toplu yüklemeye geçti; aynı koordinatlar ve Pulse %65 menzil kuralı testtedir. Stable core/tether MPB, görünmeyen terminal aura ve ghost materyal-slot tekrarları azaltıldı. Temas ön elemesi ayrıntılı solver'ı değiştirmez; dünya Z yuvarlama payı, rotating diagonal ve ContactPrecision korunur. Sonsuz retirement kayıt üyeliğiyle yapılır ve geçici listeler kaldırıldı.

Her senaryoda adım sayısı, phase, distance, half separation, hız, bonus, Pulse, açılan target/röle sayısı, aktif chunk ve takip edilen collectible sayısı birebir aynı. Ölçü toplam tahsis olayıdır; bayt veya peak RAM değildir. Chunk spawn/olay kareleri hâlâ tahsis yapabilir. Görsel model havuzlaması ve cihaz GPU/termal/pil ölçümü bu turun kanıtı olarak sunulmaz.

## Kabul ve regresyon

İlk tam turda 134/136 PlayMode geçti; iki yeni test fixture'ı düzeltildi: eski MPB verisini silmeden marker ekleme ve RT'nin ertelenmiş Destroy'unu bir kare bekleme. Üretim kaynağında bu nedenle değişiklik gerekmedi. Son PlayMode turu **136/136** geçti; **120/120 EditMode +136/136 PlayMode =256/256** geçti; Windows strict build exit0; rapor 165.338.899 bayt, output/windows-optimized/TetherRush.exe. Android strict build exit0; APK **39.163.089 bayt**,0.9.0 (10), ARM64/API25/36/portrait ve v2 imzası doğrulandı. Prepare her iki build içinde geçti. Gerçek oyun ve harita kareleri önceki 0.8.0 referanslarıyla piksel düzeyinde aynı. Ana menü 0.5.0 referansına karşı ortalama RGB farkı 0,0083/255, görsel inceleme geçti; ayrıntı Results/Optimization_comparison.json. Yeni kareler Docs/Art/Previews/Optimized_*.png.

Kapsam: mevcut 25 rota/kayıt/sıralı erişim/uzun Sonsuz Akış; 240 seeded static/timed differential sweep ve tangent/precision/rotating tip/büyük Z değişiklikler; fatal+release önceliği; kalibre edilmiş sabit HUD allocation 0; gerçek URP cached preview/RT.Release/hide-show/root enable/cleanup; bulk aura/ring geometri, MPB/fallback/reset, terminal transient ve ghost 1→3→1 slot. Testler profil kaydını yazmaz.

Teknik dayanak: [Unity UGUI optimizasyon rehberi](https://unity.com/how-to/unity-ui-optimization-tips), [GC tahsislerinin izlenmesi](https://docs.unity3d.com/jp/current/Manual/performance-track-garbage-collection.html), [thread'e özel GC.Alloc örneği](https://docs.unity3d.com/ja/current/ScriptReference/Unity.Profiling.ProfilerRecorderOptions.CollectOnlyOnCurrentThread.html), [URP render isteği](https://docs.unity3d.com/ja/current/ScriptReference/Rendering.RenderPipeline.StandardRequest.html). Bunlar yöntem kaynağıdır; oyunda ölçülmemiş kazanç için kanıt değildir.

APK SHA-256: `b215ac0db51ba22ab32a328c340002f7ced89fcf4cc7ecec9e9b9bd0de2fb141`. Son tam test vakaları Results/Optimization_test_results.json içinde. Kaynak/asset/meta/GUID/JSON,43 belge bağlantısı ve staged whitespace kontrolleri geçti. Build/cache, test runner geçici sahneleri ve imzalama dosyaları Git dışında kalır.
