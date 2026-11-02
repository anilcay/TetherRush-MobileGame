# Proje durumu

Son güncelleme: 02.11.2026.

## Menü ve ilk bölümler — 0.3.0 (4)

Kullanıcı D-024/025 ile Leveller / Sonsuz Akış / Ayarlar menüsünü ve beş kısa bölümü onayladı. Menü/HUD, bölüm/sonsuz parça verisi ve kayıt/ayar işleri GPT-6.1 Sol medium ajanlarına ayrı sahipliklerle verildi; ana yönetici uygulama akışı, stream temizliği, gerçek Unity testi/görüntüsü ve teslimi birleştirdi.

MainMenu başlangıç sahnesi, beş bölüm seçimi, sonuçtan sonraki bölüm ve güvenli menü dönüşü hazır. Sonsuz Akış çarpışmaya kadar devam eder; tam metre + bonus ×100 puan, monoton yerel rekor. Ses/volume ve bölüm tamamlanma/bonus kayıtları yerelde saklanır. İlk seçkide bütün bölümler erişilebilir; kilit/yıldız/reklam eklenmedi.

**66 EditMode + 58 PlayMode = 124/124** test geçti. Beş gerçek bölüm tamamlandı; Sonsuz Akış 15 chunk / 1.680 m üzerinde, aktif dünya/collectible belleği sınırları, pause/death/retry ve rekorla doğrulandı. Sekiz gerçek Unity görüntüsü (9:16 ve uzun menü oranı) incelendi. Windows ve Android strict build exit 0; APK v2 imzası doğrulandı. Kayıt Docs/QA/MENU_LEVELS_VALIDATION.md, tasarım Docs/Design/FIRST_LEVELS.md. Fiziksel cihaz ve ses duyumu kabulü ayrıca bekliyor.

## Görsel cila — 0.2.1 (3)

D-021 kapsamındaki görsel üretim tamamlandı: şeftali/lavanta küreler ve normal/hazır hâlde camgöbeği bağ; lacivert tesis, mercan uyarılar ve amber Pulse hedefleri tek VisualTheme paletinde. 14 Blender modeli, cilalanmış gölgeleme/efekt, yeni dar geçit/merkez bariyer görünümleri ve dekoratif ışık kuleleri entegre. Yuvarlatılmış HUD ve alt durum kartı oyun alanını açık tutar. Oyun ve çarpışma kuralları korunur.

29 EditMode + son kaynaklarla 32 PlayMode = **61/61** test geçti. Gerçek Unity görüntüleri Ready/Foundation/Pulse/Challenge/Failed hâllerinde incelendi; skybox ve retry iz sıfırlama düzeltmeleri doğrulandı. Son Windows ve Android build exit 0; APK imzası doğrulandı. Kayıt: Docs/QA/POLISH_VALIDATION.md, vaka özeti Docs/QA/Results/Polish_test_results.json.

Cila tesliminden sonra kullanıcı 02.11.2026 tarihinde menü ve ilk bölümlerin üretimine geçilmesini istedi. Fiziksel cihaz testi D-017 gereği sona bırakıldı.

## Tamamlanan hazırlık

- Boş GitHub deposu mevcut çalışma klasörüne klonlandı; `origin` doğru URL'ye bağlı, başlangıç dalı `main`.
- Tasarım belgesinin 8 sayfası okundu ve görsel olarak incelendi. Özgün PDF ve aranabilir metin `Docs/Source` içinde saklandı.
- Android telefon + dikey ekran kullanıcı tarafından onaylandı.
- Planlama ve M1 uygulama görevleri GPT-6.1 Sol / medium ajanlarına dağıtıldı; dosya sahipliğiyle çakışmalar önlendi. Ana yönetici entegrasyon ve Unity doğrulamasını yürüttü.
- Karar, iş akışı, dosya sahipliği ve kabul ölçütleri için proje kayıtları oluşturuldu.

## Yerel araçlar

| Araç | Gözlenen durum |
| --- | --- |
| Unity | Proje 6000.3.19f1 (7689f4515d75) ile açıldı, C# derlendi ve prototip sahnesi kaydedildi. URP 17.3.0, Input System 1.19.0 kilitlendi. |
| Android desteği | Menü/bölüm APK build exit 0: 37.996.006 bayt, 0.3.0 (4), ARM64, min API25 / target36, portrait; APK v2 imzası doğrulandı. output/android/TetherRush-MenuLevels.apk. ASCII junction ve sınırlandırılmış generated-cache callback kullanılır. |
| Blender | 5.2.3 LTS ile 14 FBX, düzenlenebilir kaynak ve pafta. Bağımsız re-import 14/14 ve Unity model kontrolü geçti. Kit 6.548 üçgen, 20 mesh; kaynak ArtSource/Blender içinde. |
| Git | origin GitHub TetherRush-MobileGame, dal main. Kod, Blender kaynak/FBX, gerçek Unity kareleri ve doğrulama özetleri sürüm kontrolüne alınır; build/cache çıktıları dışarıda tutulur. Son teslim hash'i git log ve yönetim sohbetindeki teslim raporundan izlenir. |
| Unity testleri | Menü/bölüm: EditMode 66/66 + PlayMode 58/58 = 124/124. Docs/QA/Results/MenuLevels_test_results.json. Önceki cila 61/61 kaydı korunur. |
| Windows önizleme | Menü/bölüm build exit 0, output/windows-menu-levels/TetherRush.exe. Gerçek UI/oyun kareleri Docs/Art/Previews/Menu_*.png; kullanıcı değerlendirmesi açık. |

## Kilometre taşları

| Aşama | Kapsam | Durum / çıkış koşulu |
| --- | --- | --- |
| M0 – Hazırlık | GDD analizi, planlar, ajan iş dağılımı, GitHub başlangıç kaydı. | Tamamlandı; başlangıç paketi kullanıcı tarafından onaylandı. |
| M1 – Oynanabilir prototip | Daral/genişle, tether enerji, bonus, Pulse kapısı/kırılabilir hedef, ölüm, bitiş/retry, temel HUD. | İlk teslim tamamlandı; kullanıcı devam onayıyla M2'ye geçildi. Bu onay tüm manuel testlerin yürütüldüğü anlamına gelmez. |
| M2 – Görsel ve içerik | Blender kiti, animasyon/VFX/ses ilk seti, hareketli duvar/dönen kol ve ikinci alıştırma. | M2 ilk tesliminden sonra D-021 görsel cila uygulandı. Cila: 61 test, Windows ve imzası doğrulanan Android 0.2.1 build geçti. Son görsel/oynanış kabulü kullanıcıda. |
| Menü ve ilk bölüm paketi | Ana menü, beş öğretici bölüm, Sonsuz Akış, yerel skor/ayarlar. | Kullanıcı kapsamı onayladı; 124 test ve Windows build geçti. Android 0.3.0 (4) ve imzası doğrulandı. |
| M3 – Mobil doğrulama | Fiziksel cihaz, performans, ekran oranları, pause/resume, ses ve titreşim. | Cihaz ve hedef bütçeler belirlenecek. |
| M4 – Yayına hazırlık | Mağaza gereksinimleri ve dağıtım. | Kullanıcı ayrıca kapsam/onay verecek. |

## Şu anki sınır

Yeni başlangıç MainMenu'dür. Beş kısa bölüm ve 112 m'lik beş kalıpla devam eden Sonsuz Akış oynanır. Önceki 180 m temel /130 m zamanlama sahneleri korunur. Hareket ve Pulse kuralları değişmez. Runtime sahne inşası kullanılır; görsel bölüm editörü, yıldız ekonomisi, reklam veya mağaza yayını kapsamda değildir. Fiziksel Android dokunma/performans, uygulama kapat/aç kayıt kabulü, insan tarafından ses duyumu ve son tasarım beğenisi açık kalır.

## Sıradaki yönetim kararı

Kullanıcı menü görünümünü, beş bölümün süre/zorluk ritmini ve Sonsuz Akış skor dengesini bu somut sürümde inceleyecek. Yeni bölüm/kalıp sayısı ve kilit/yıldız sistemi eklenmeden önce kapsam tartışılacak. Android telefon testi D-017 ile son aşamada; tekrar cihaz talep edilmeyecek. Oynama adımları README.md, güncel kareler Docs/Art/Previews/Menu_*.png altında.
