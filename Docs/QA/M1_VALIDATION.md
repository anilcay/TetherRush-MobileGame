# M1 doğrulama kaydı

Tarih: 31.10.2026. Yapı: Unity 6000.3.19f1 / URP 17.3.0 / Input System 1.19.0. Tablodaki durumlar gerçekten yürütülen kontrolü gösterir; test kaynaklarının varlığı tek başına başarı kanıtı değildir.

| Kontrol | Durum | Kanıt / sınır |
| --- | --- | --- |
| Unity proje hazırlığı ve C# derlemesi | GEÇTİ | Prepare exit 0; sahne kaydedildi. `output/validation/Prepare.log` |
| EditMode çekirdek sözleşmeleri | GEÇTİ | Unity Test Runner: toplam 29, geçti 29, kaldı 0; `output/validation/EditMode.xml`. |
| PlayMode authored parkur ve etkileşimler | GEÇTİ | Unity Test Runner: toplam 11, geçti 11, kaldı 0; `output/validation/PlayMode.xml`. |
| Windows önizleme build | GEÇTİ | Exit 0; `output/windows/TetherRush.exe`, toplam yapı boyutu 163.069.062 bayt. |
| Manuel görsel inceleme | BEKLİYOR | Önizleme açıldı; pencere küçültülmüş olduğundan otomatik ekran incelemesi tamamlanmadı. Kullanıcının Editor incelemesi bekleniyor. |
| Windows başlangıç çalıştırması | GEÇTİ | Önizleme uygulaması açıldı; ürüne ait Player.log içinde exception/error eşleşmesi yok. Bu görsel inceleme değildir. |
| Android ARM64 test APK | GEÇTİ | Build exit 0; `output/android/TetherRush-M1.apk`, 37.533.184 bayt. Cihazda çalıştırılmadı. |
| APK kimlik, mimari, ekran ve imza | GEÇTİ | aapt: `com.anilcay.tetherrush`, 0.1.0 (1), arm64-v8a, min API 25 / target 36, screenOrientation=1 (portrait). apksigner verify exit 0, APK Signature Scheme v2=true. |
| Fiziksel Android cihaz, gerçek dokunma ve performans | ERTELENDİ | Kullanıcı D-017 ile son aşamaya bıraktı. |
| Kullanıcının Editor oynanış değerlendirmesi | BEKLİYOR | Kontrol hissi, kamera ve zamanlama kullanıcı tarafından incelenecek. |

## Otomatik kapsam

- `RunModelTests`: enerji sınırı ve tek tüketim, dolu/eksik enerjiyle bırakma, iptal, terminal durumlar, pause/resume, reset, sabit hareket, mesafe sınırları ve geçersiz parametreler.
- `PrototypeFlowTests`: gerçek enerji parçacıkları ve bonuslarla iki Pulse hedefini açıp bitirme; iki temel yanlış ayrımda çarpışma; erken Pulse sonrası kapalı kapıda kayıp; normal tehlikenin Pulse'a bağışıklığı; dünya/reset; pause/resume; bağın normal bariyerden geçmesi; eksik enerjiyle bırakma.
- Yavaş kare kontrolleri: yarım saniyelik adımda bariyerin atlanmaması; yarım saniyelik bırakma karesinde Pulse'un gecikmeden açması ve daralmanın aynı karede başlaması.

Normal hazard testinde, authored enerji bölgesinden önce Pulse'u izole etmek için çekirdeğe test içinde enerji verilir. Diğer tam rota testleri gerçek parkur toplanabilirlerini kullanır. Test API'si üretimin aynı simülasyon yolunu sürer; oyun içi hile veya ayrı test fiziği yoktur.

## Tekrarlama

Unity kapalıyken `Tools/Unity.ps1 -Action EditMode` ve `-Action PlayMode`. Test Runner kendi çıkışını yapar; `-quit` eklenmez. Loglar ve XML çıktıları yerel `output/validation` klasöründe, kaynak kontrolü dışındadır. Manuel görsel inceleme ve fiziksel cihaz kontrolleri otomatik sonuçlardan ayrı tutulur.

## Kurulum notu

İlk denemede Unity lisansı etkin değildi; kullanıcı Unity Personal lisansını etkinleştirdikten sonra Editor açıldı. Eski şablonun Input System 1.12.0 paketi bu Editor API'siyle derlenmedi; kurulu Editor paket manifestindeki 1.19.0 sürümüne sabitlendi. Paket kaynakları elle yamalanmadı.

Unity'nin [komut satırı test belgesi](https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/test-framework-introduction/running-tests/run-tests-from-command-line) ve [URP uyumluluk belgesi](https://docs.unity.com/en-us/engine/6000.3/manual/render-pipelines/universal-render-pipeline/requirements) kullanılan kontrol akışının resmî başvurularıdır; gerçek sürümler yerel kurulum ve paket kilidinden doğrulanır.

İlk Windows build'i, oyun paketlemesi sonrasındaki `Unity.PerformanceTesting.Editor.TestRunBuilder.Cleanup` içinde OneDrive'ın ReadOnly Resources klasörünü silemediği için başarısız oldu. `Assets/Resources/.gitkeep` klasörü önceden var olan kalıcı bir proje klasörü olarak korur; paketin kendi kaynaklarına veya dosya sistemi izinlerine müdahale edilmez. Yeniden build sonucu yukarıdaki tabloda ayrıca kaydedilir.

İlk Android denemesi, proje yolundaki Türkçe karakterler nedeniyle araç ön kontrolünde durdu. `Tools/Unity.ps1 -Action Android` aynı çalışma klasörüne işaret eden, hedefi doğrulanan geçici ASCII junction kullanır. Böylece kaynaklar taşınmadan Android ön koşul kontrolü geçildi; nihai paket sonucu tablodan izlenir.

## Teslim paketleri

- Android: `output/android/TetherRush-M1.apk` — Development/debug test sürümü, yayın paketi değildir.
- APK SHA-256: `FE077EDC9AF42F9BBC1EE75CD58019A257E8E1F88D46602B90439D569F256309`.
- Windows: `output/windows/TetherRush.exe` ve yanındaki veri dosyaları birlikte kullanılır.
- Build çıktıları, imzalama sırları ve geçici performans kayıtları Git'e alınmaz. Unity projesi, paket kilidi, testler ve build araçları kaynak kontrolündedir.
- C# kodu son doğrulamadan sonra davranış olarak değiştirilmedi. Sonraki değişiklikler açılış/ayar talimatları ve bu doğrulama kaydıdır.
