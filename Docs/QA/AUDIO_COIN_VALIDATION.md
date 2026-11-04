# Müzik, efektler ve coin — 0.8.0 (9)

04.11.2026. Kullanıcının D-039/040 isteği: hypercasual oyuna uygun sözsüz müzik, daha iyi ses efektleri, yeni toplanabilir coin ve aynı sağ üst simge. [Ses varlıkları](../Audio/AUDIO_ASSETS.md), [entegrasyon](../Audio/AUDIO_INTEGRATION.md), [Blender tasarımı](../Art/COIN_DESIGN.md).

## Uygulama ve kaynaklar

- Özgün Tether Flow: 104 BPM, 32 ölçü, 73,846 saniye; yumuşak elektronik ritim, mallet motifleri, bas ve pad. Vokal, üçüncü taraf sample veya ücretli hizmet kullanılmadı. Kaynak reçete/master ve üretim betiği depoda. Stereo Vorbis müzik Streaming olarak yüklenir; runtime ses dosyaları toplam 1.084.580 bayt. Kaynak OGG 673.576 bayttır; bu sayı build sonrası Unity ses bankası boyutu değildir.
- On kısa mono efekt: enerji, üç coin varyantı, hazır, Pulse, röle, ölüm, başarı, UI. Aynı karede coin çiftleri tek cue olur; 35 ms/25 ms coin/enerji aralığı. PlayOneShot, pickup seslerinin Pulse/sonucu kesmesini önler. Müzik .65 menü/.50 deneme, coin .90 ve diğer oyun efektleri .80 göreli gain ile master'a bağlıdır.
- Tek uygulama müzik kaynağı menü/deneme/retry geçişlerinde korunur; mevcut ses anahtarı/slider ve kalıcı kayıt kullanılır. Oyun pause, arka plan ve odak kaybı ayrı nedenlerdir; biri sürerken diğeri müziği açamaz. Imported klipler runtime tarafından yok edilmez. Eski tarihsel Prototype/Challenge sahneleri ayrıca müzik başlatmaz; güncel giriş MainMenu'dür.
- Blender coin 624 üçgen/tek mesh/Body–Dark–Accent; yuvarlatılmış altın rim, koyu yüz ve iki bağlı çekirdek. .5×.5×.18m, ±12° sakin yalpa ve .07m bob yalnız görsel çocukta; otoriter pickup pozisyonu sabittir. HUD 27 px ve harita 24 px aynı şeffaf Sprite'ı kullanır. Bonus temas yarıçapı .19→.25m görselle eşlenir; coin sayısı/100puan/kayıt alanları korunur.

## Yürütüm kanıtı

| Kontrol | Sonuç / sınır |
| --- | --- |
| Blender bağımsız FBX reimport | PASS; merkez 0 / bounds 1³ / scale 1, 624 tri/üç materyal, collider/armature yok. Önceki18FBX SHA-256 aynı; toplam kit 19 model / 7.716 tri / 25 mesh. |
| Kaynak ses QA | Master ve yeniden çözülmüş OGG 3.544.615 stereo frame. OGG yaklaşık −19,988 LUFS, −7,638 dBFS sample peak; finite, clipping 0; son/ilk örnek adımı .00275 FS. 10 SFX mono 44,1 kHz, uç 50 sample sessiz ve peak ≤.50116 FS. Sayısal kontrol dinleme kabulü değildir. |
| Tekrar üretim / yönetici incelemesi | Tam sentez ve master'dan export OGG/UiClick SHA eşit. Yönetici dosya hashlerini,10 PCM dalgasını,eski 18 FBX'i ve özgün kaynak düzenini bağımsız denetledi. |
| Unity Prepare | GEÇTİ; exit0, kaynaklar derlendi ve importer/meta üretildi. |
| Unity EditMode / PlayMode | GEÇTİ; son importer ile 120/120 EditMode + 120/120 PlayMode = **240/240**. Results/AudioCoin_test_results.json bütün vaka sonuçlarını içerir. |
| Gerçek URP görüntüsü |540×960 AudioCoin_Game/Map kareleri incelendi; dünya coinleri, sağ üst simge ve tamamlanan bölümün 2 coin sayacı eşleşiyor. Bütün modellerin gerçek Unity import/shader/bounds denetimi testtedir. |
| Windows / Android | Windows strict build exit0; rapor 165.333.752 bayt, output/windows-audio-coin/TetherRush.exe. Android strict build exit0; APK **39.158.057 bayt**, 0.8.0 (9), ARM64/API25/36/portrait; v2 imzası doğrulandı. |

Müzik/coin için 6 yeni ses ve 3 yeni coin PlayMode senaryosu vardır: dosya biçimi ve PCM miks payı; tek kaynak/sonlu master; focus/application/game pause birleşimi; menü→koşu→retry→menü kaynak ömrü; unscaled fade; coin çiftleri/cooldown/Random izolasyonu/clip sahipliği; gerçek FBX ve HUD Sprite; pause'da coin pozunun donması/tek toplama/skor; gerçek oyun/harita görüntüsü. Mevcut 25 rota, kayıt göçü/sıralı erişim, röle/kilit ve hızlanan sonsuz regresyonları da tam pakette çalışır. Testler kullanıcı PlayerPrefs kaydını değiştirmez.

## Açık kabul

Üretim önizlemesi `AudioSource/TetherFlow.gameplay-preview.wav` 14 saniyelik temsili bir mikstir; Unity ses kaydı değildir. Bu çalışma sırasında hoparlör/kulaklıkta insan dinlemesi yapılmadı. Batch testleri klip yükleme/kaynak/ayar/PCM ve kontrol akışını doğrular; sesin host DSP'de duyulduğunu veya loop sınırının insan tarafından dinlendiğini kanıtlamaz. Son müzik beğenisi ve oyun içi ses dengesi kullanıcı incelemesine açıktır. Fiziksel Android performans/dokunma/ses kabulü D-017 gereği sonda kalır.

APK SHA-256: `b900b0c9e789349fd90cf8b1c1714f61a0336e0edd4cc95939bea9e817266a06`. Asset/meta/GUID/JSON ve 43 belge bağlantısı kontrolü geçti; staged whitespace denetimi geçti. Build/cache çıktıları Git dışında kalır. Android build raporunun toplam boyutu APK dosya boyutundan ayrıdır; yukarıdaki 39.158.057 bayt gerçek APK dosyasıdır.
