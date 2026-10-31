# Tether Rush

Unity ile geliştirilecek, ortak bir enerji bağıyla bağlı iki küreyi tek parmakla yönettiğimiz 3D mobil parkur oyunu.

## Durum

Görsel cila sürümü **0.2.1 (3)**: şeftali/lavanta küreler, camgöbeği bağ, yenilenmiş 14 Blender modeli, efektler, tesis atmosferi ve kompakt HUD. 180 m temel + 130 m zamanlama parkuru var. **61/61 Unity testi**, Windows build ve imzası doğrulanan Android test build geçti. Son doğrulama ve paket durumu [cila kaydında](Docs/QA/POLISH_VALIDATION.md); fiziksel Android cihaz testi son aşamaya ertelendi.

- Hedef: **Android telefon, dikey ekran** (kullanıcı onayı: 31.10.2026).
- Tasarım kaynağı: [GDD v2.0](Docs/Source/Tether_Rush_GDD_v2.pdf), 31.10.2026.
- Yönetim merkezi: kullanıcının ana Codex sohbeti.
- Alt ajanlar: **GPT-6.1 Sol / medium**.
- Kaynak depo: https://github.com/anilcay/TetherRush-MobileGame

## Proje kayıtları

- [Durum ve iş listesi](Docs/Planning/PROJECT_STATUS.md)
- [Kararlar ve onay bekleyen öneriler](Docs/Planning/DECISIONS.md)
- [İş akışı ve ajan görevleri](Docs/Planning/WORKFLOW.md)
- [Unity teknik planı](Docs/Planning/TECHNICAL_PLAN.md)
- [Blender ve animasyon hattı](Docs/Art/ART_PIPELINE.md)
- [Kabul testleri](Docs/QA/ACCEPTANCE_TESTS.md)
- [M2 doğrulama ve test sonuçları](Docs/QA/M2_VALIDATION.md)
- [Blender varlık manifesti](Docs/Art/M2_ASSET_MANIFEST.md)
- [Görsel cila doğrulaması](Docs/QA/POLISH_VALIDATION.md)
- [Güncel Blender kiti](Docs/Art/POLISH_ASSETS.md)
- [Gerçek Unity görüntüleri](Docs/Art/Previews/Polish_Foundation.png)

## Unity'de aç ve oyna

1. Unity Hub → Projects → Add ile bu deponun kök klasörünü seç. Editor: **6000.3.19f1**.
2. İlk açılışta paket çözümleme ve import işlemlerinin bitmesini bekle. URP **17.3.0**, Input System **1.19.0**; çözümlenen sürümler `Packages/packages-lock.json` içinde.
3. `Assets/TetherRush/Scenes/Prototype.unity` sahnesini aç. Sahne yoksa **Tether Rush → Prepare Project** menüsünü kullan.
4. Game görünümünü **9:16** yap, Play'e bas, ardından oyun içindeki **Başla** düğmesine tıkla.
5. Oyun alanında **sol fareyi veya Space'i basılı tut: açıl; bırak: daral**. Enerji doluysa bırakmak aynı zamanda bir Pulse üretir. Space yalnız Editor kolaylığıdır; mobilde tek dokunma vardır.

Dar geçitte dar kal; orta bariyerden önce açıl. Enerjiyi geniş bağla, altın bonusları kürelerle topla. Sarı Pulse hedefine yaklaşınca **MENZİLDE** ipucunda bırak. Erken Pulse sonrası kapalı hedefe çarpma denemeyi bitirir; **Yeniden dene** ile baştan başla. Kırmızı engeller Pulse ile açılmaz.

Başlangıç veya sonuç ekranındaki **Zamanlama** düğmesi ikinci 130 m alıştırmayı açar. Burada ilk hareketli duvarı geniş, yana kayan ikinci duvarı dar, dönen kolu geniş geç. **Temel parkur** düğmesi ilk bölüme döner. `Challenge.unity` doğrudan da açılabilir. Ses düğmesi seçimi iki parkur arasında korunur; uygulama yeniden açılınca ses açık başlar.

Sahne oyun dünyasını Play sırasında kurar; Edit görünümünde boş görünmesi beklenir. Sayısal parkur yerleşimi `PrototypeLayout.cs` içindedir. Hareket/enerji ayarları Play'e girmeden önce sahnedeki **Tether Rush — M1 → Tuning** alanından düzenlenir; yeni nesnelerin varsayılanları `RunTuning.cs` içindedir. Bu M1 yaklaşımı son bölüm tasarım aracı değildir.

## Tekrarlanabilir kontroller

Unity Editor'ü kapattıktan sonra proje kökünde PowerShell ile:

```powershell
./Tools/Unity.ps1 -Action Prepare
./Tools/Unity.ps1 -Action EditMode
./Tools/Unity.ps1 -Action PlayMode
./Tools/Unity.ps1 -Action Android
```

`-UnityPath` farklı kurulum yolu verebilir. Log/XML çıktıları `output/validation`, güncel test APK'sı `output/android/TetherRush-Polish.apk` altında oluşur; build/cache çıktıları Git'e alınmaz. Android desteği için Unity Hub'da Android Build Support, SDK/NDK ve OpenJDK kurulu olmalıdır. `-Action Windows` güncel önizlemeyi `output/windows-polish/TetherRush.exe` altında üretir. Eski M1/M2 çıktıları karşılaştırma için ayrı klasörlerde korunur.

Android araçları Türkçe karakter içeren proje yollarını kabul etmez. Bu klasör için `-Action Android` aynı depoya işaret eden geçici bir ASCII junction yolunu otomatik kullanır; kaynaklar taşınmaz veya kopyalanmaz. Bağlantı TEMP altında kalır ve tekrar kullanılır. Editor'dan doğrudan Android build almak için de ASCII bir proje yolu gerekir; mevcut klasörde Editor oynanışı etkilenmez.

Fiziksel Android cihaz testi kullanıcı kararıyla son aşamaya ertelendi. Editor ve otomatik testler gerçek cihaz performansı/dokunma doğrulaması yerine geçmez. Güncel yürütüm sonuçları [proje durumunda](Docs/Planning/PROJECT_STATUS.md) kaydedilir.
