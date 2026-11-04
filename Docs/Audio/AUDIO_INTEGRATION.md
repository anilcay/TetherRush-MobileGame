# Ses entegrasyonu

## Kapsam ve sahiplik

Bu uygulama mevcut tek dokunma kontrolünü, simülasyonu, Pulse kurallarını, VFX'i ve kayıt şemasını korur. Ses sunum katmanındadır. Özgün ses varlıkları `Assets/TetherRush/Resources/Audio` altında, üretim kaynakları ayrı tutulur. `AudioPalette`, Resources kliplerini bir kez yükler ve ortak referanslar sunar; bu varlıklar hiçbir runtime bileşeni tarafından Destroy edilmez.

## Sürekli müzik

`MusicDirector`, uygulama GameObject'i üzerinde tek, 2D, loop AudioSource oluşturur. `Initialize(bool enabled, float masterVolume)` tekrar çağrıldığında yeni source oluşturmaz. Menü kamerası veya deneme nesnesi müziğin sahibi değildir; menü → bölüm → retry → menü geçişinde klip yeniden başlatılmaz. Sahne içindeki mevcut AudioListener kullanılır; yeni listener veya DontDestroyOnLoad eklenmez. Eski doğrudan oyun sahnesi ayrıca müzik başlatmaz.

`SetPreferences(bool,float)` mevcut ses anahtarını ve master düzeyini kullanır. Master 0–1'e sıkıştırılır; NaN/Infinity varsayılan .55'e döner. Mute, oynatma başını sıfırlamaz. Menü hedefi .65 × master, deneme ve sonuç hedefi .50 × master. Varsayılan master .55 ile müzik çarpanları .3575/.275 olur. `Update`, `Time.unscaledDeltaTime` ile .35/s yumuşak geçiş uygular; kaynak ilk açılışta sessizden yükselir. Bunlar -20 LUFS kaynak için ilk miks ayarıdır; insan dinlemesiyle son denge kabulü ayrıca yapılır.

`SetContext(bool inRun, bool paused)` yalnız yerel source'u Pause/UnPause eder; Stop/Play ile yeniden başlatmaz. Oyun pause, uygulama pause, odak kaybı ve component devre dışı durumu ayrı nedenlerdir. Odak dönüşü hâlen oyun pause durumundaysa müziği açmaz. Global `AudioListener.pause` ve `AudioListener.volume` değiştirilmez. UI tıklaması `PlayUiClick()` ile aynı source üzerinde düşük düzeyli OneShot'tır; loop klibi değişmez.

## Oyun efektleri

`GameFeedback` aynı tek kamera AudioSource'unu korur. Enerji, üç coin varyantı, hazır, Pulse, röle, hata ve başarı ayrı içe aktarılmış seslerdir. PlayOneShot, pickup seslerinin Pulse/sonuç sinyalini kesmesini önler. Enerji .8, coin .9, röle ve diğer oyun efektleri .8 göreli gain kullanır; bunlar mevcut master düzeyiyle çarpılır. Menü tıklaması müzik kaynağı üzerinden 1.2 göreli gain kullanır; ayrıca o kaynağın o andaki ses düzeyi uygulanır.

Aynı karedeki coin çiftleri tek ses olur. Ardışık coin sesi aralığı en az 35 ms; enerji aralığı en az 25 ms. Coin varyantları deterministik 1→2→3 döngüsündedir; Unity Random kullanılmaz. Sessize alındığında da sıra/cooldown mantığı ilerler. `ResetPresentation` coin sırasını/cooldown'ları sıfırlar ve yalnız SFX one-shot'larını durdurur; müzik kaynağına dokunmaz. Oyun Tick'inin pause davranışı korunmuştur.

Yalnız eksik SFX varlığı varsa eski kısa sentez yöntemi acil fallback üretir; fallback listesi instance'a aittir ve OnDestroy'da temizlenir. İçe aktarılmış klipler bu listeye girmez. Müzik eksikse ek sentez müziği veya ikinci kaynak üretilmez.

## Doğrulama

`AudioPresentationTests`: stereo 60–90 s müzik, kısa mono SFX, tek kaynak/reinitialize, sonlu master düzeyi, mute, bağımsız pause nedenleri, unscaled fade, uygulama menü/deneme/retry dolaşımı, coin çiftleri/cooldown, Random izolasyonu ve imported clip sahipliği için testler eklendi. Bunlar host DSP'nin batch modunda ses çaldığını veya timeSamples ilerlemesini varsaymaz. Ses dosyasının dalga/veri/loop analizi üretim raporunda ayrı yapılır; CompressedInMemory/streaming müziğe GetData dayatılmaz.

`AudioImportRules` müziği stereo Vorbis/Streaming, kısa efektleri mono PCM/DecompressOnLoad olarak alır; örnekleme frekansı korunur, preload etkindir. Kaynak efektler zaten mono olduğundan Force To Mono kapalıdır; otomatik mono normalizasyonu kaynakların miks payını yükseltmez. PlayMode testi içe aktarılmış PCM verisinin .2–.52 FS tepe aralığını da denetler. Böylece uzun stereo müzik tamamen PCM belleğe açılmaz. Uygulamanın mevcut kalıcı ses anahtarı ve slider'ı her iki katmana uygulanır. Batch testlerinde ses mute edilir; gerçek Editor/oyuncu oturumunda profil tercihi kullanılır.

Alt görev Unity'yi çalıştırmadı; ana yöneticinin entegrasyon sonrası sonuçları `Docs/QA/AUDIO_COIN_VALIDATION.md` ve PROJECT_STATUS içinde kaydedilir. Hoparlör/kulaklıkta insan ses dengesi ve fiziksel Android kontrolü ayrı kabul adımıdır.
