# Menü ve ilk bölümler — 0.3.0 (4)

02.11.2026. Kullanıcı D-024/025 ile ana menü, Sonsuz Akış adı ve beş kısa bölümü onayladı. Editör Unity 6000.3.19f1 / URP 17.3.0. Fiziksel Android testi D-017 gereği son aşamada.

## Teslim kapsamı

- Başlangıç sahnesi MainMenu: Leveller, Sonsuz Akış, Ayarlar. Beş bölüm keşif için erişilebilir; gerçek tamamlanma ve en iyi bonus yerel kayda girer. Sonuç ekranından sonraki bölüm ve ana menü; duraklatmada devam veya menü.
- İlk beş bölüm 56/64/76/96/124 m: hareket → bağla toplama → Pulse → zamanlama → karma kullanım. Sayısal içerik ve referans rotalar Docs/Design/FIRST_LEVELS.md içinde.
- Sonsuz Akış 112 m'lik beş tasarlanmış kalıbı seed/index ile çeşitlendirir. Hız değişmez. Skor tam metre + bonus × 100. Çarpışma denemeyi bitirir, en iyi skor azalmaz.
- Görünür aralığın ilerisi yüklenir, gerideki chunk kökü/engel/pickup/zamanlı engel verisi temizlenir. Tüketilen pickup kimlikleri de emekli edilir; geçmiş nesnelerin tamamı tutulmaz. Tekrar deneme ilk parçaları ve sıfır skoru yeniden kurar.
- Ses açık/kapalı ve ses düzeyi kalıcı yerel kayıttadır; mevcut ses kaynağına uygulanır. Kontrol açıklaması vardır. Yeni kontrol, hızlanma, yıldız, para, reklam veya mağaza yayını eklenmez.
- Cila renkleri/Blender modelleri korunur. Ana menüdeki dairesel küre sembolleri teknik UI sprite'ıdır. Gerçek oyun geometrileri aynı Blender FBX'leridir.

## Yürütüm

| Kontrol | Sonuç | Kanıt / sınır |
| --- | --- | --- |
| Unity hazırlığı | GEÇTİ | Prepare exit 0; MainMenu ilk build sahnesi, eski Prototype/Challenge korunur. |
| EditMode | GEÇTİ | 66/66; çekirdek model ve profil JSON/monoton kayıt/ayar sınırları. |
| PlayMode | GEÇTİ | Son kaynaklarla 58/58; toplam 124/124. Results/MenuLevels_test_results.json. |
| Beş bölüm | GEÇTİ | Her bölüm gerçek TetherRushGame ile referans basma/bırakma rotasında Completed; Pulse kapısı/blokta tek hedef açılır, tamamlanma kaydı yalnız başarıda. |
| Menü ve ayarlar | GEÇTİ | Gerçek UI düğmeleriyle menü→bölüm, duraklat→menü, başarı→sonraki bölüm; tek EventSystem, tam enerjide iptal/menü dönüşü Pulse üretmez; ses/volume yeni denemeye aktarılır. |
| Uzun Sonsuz Akış | GEÇTİ | 15 chunk / 1.680 m üzeri; en fazla 4 aktif chunk, izlenen pickup kimliği <80, frame temizliği sonrası MeshRenderer <350; duraklatma, ölümde rekor ve retry sıfırlaması doğrulandı. Bu bir cihaz FPS ölçümü değildir. |
| Gerçek görüntüler | İNCELENDİ | 540×960 menü/leveller/sonsuz/ayarlar/bölüm3/duraklatma/sonsuz koşu ve 540×1170 ana menü. Docs/Art/Previews/Menu_*.png. |
| Windows | GEÇTİ | Strict development build exit 0; output/windows-menu-levels/TetherRush.exe. Build raporu 163.790.952 bayt. |
| Android | GEÇTİ | Strict development build exit 0; output/android/TetherRush-MenuLevels.apk; 37,996,006 bayt. |
| APK kimlik ve imza | GEÇTİ | com.anilcay.tetherrush, 0.3.0 (4), arm64-v8a, min API 25 / target36, portrait; APK Signature Scheme v2=true. |

Görüntüler gerçek URP kamera çıktısıdır; mockup değildir. Ardışık Canvas yakalamalarında aktivasyon ve render modu geçişi öncesi bir frame beklenip önceki planeDistance geri yüklenir. 540 px referans genişlik menüde korunur; uzun ekranda kartlar kesilmez. Telefon çentiği ve gerçek dokunma henüz denenmedi.

Kayıt testleri mevcut kullanıcı PlayerPrefs anahtarını değiştirmez. JSON roundtrip/bozuk kayıtlar Core testinde; gerçek menü akışları enjekte edilen bellek profiliyle yürütüldü. Üretimde PlayerSaveStore, isim alanlı PlayerPrefs anahtarını SetString + Save ile yazar. Cihazda uygulamayı kapat/aç ve insan tarafından ses duyumu kabulü ayrıca bekler.

## Kaynak ve sınır

Görsel tasarım Docs/Art/MENU_DESIGN.md, kayıt sözleşmesi Docs/Planning/SAVE_DATA.md. Önceki M1/M2/cila test kayıtları ve yerel paketler korunur. Sonsuz modun ilk içerik havuzu beş kalıptır; yeni kalıplar ve zorluk ayarları kullanıcı oynanış geri bildirimiyle genişletilebilir. Mağaza sürümü değildir.

APK SHA-256: `6c3e8668f386a10b7e0e77dd4990f9b3d68dbf0f57a4627d802b28c924125f47`. Boyut doğrudan APK dosyasından alındı. Meta/GUID/JSON ve doküman bağlantı kontrolleri geçti; son commit whitespace kontrolü ayrıca yürütülür. Paket fiziksel telefona kurulmadı; test build'idir.
