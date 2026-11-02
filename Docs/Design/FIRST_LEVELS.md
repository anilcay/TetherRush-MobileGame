# İlk beş bölüm ve Sonsuz Akış

Bu içerik kullanıcının onayladığı beş kısa bölüm ve ayrı skor modu kapsamındadır. GDD A/B/C/D sırası korunur; yeni kontrol veya sanat varlığı gerekmez. Ana menüde bölüm seçimi, **Sonsuz Akış** ve ayarlar yönetici entegrasyonunun sorumluluğundadır.

| Bölüm | Mesafe / yaklaşık süre | Öğrenme ve akış |
| --- | --- | --- |
| Daral ve Açıl | 56 m / 8 sn | 16 m dar geçit → 36 m orta bariyer → 48 m dar geçit. Bırakmak daraltır, basılı tutmak açar. |
| Bağı Keşfet | 64 m / 9 sn | 18 m bariyer → 26–42 m beş bağ enerjisi → küre bonusları → 58 m dar geçit. Pulse zorunlu değildir. |
| İlk Pulse | 76 m / 11 sn | 18 m bariyer → 26–42 m enerji → 58 m enerji kapısı → 68 m dar geçit. 54 m'de gerçek bırakma tek Pulse üretir. |
| Zamanlama | 96 m / 14 sn | 24 m hareketli duvar geniş → 46 m yana kayan duvar dar → 70 m dönen kol geniş → 86 m dar geçit. |
| Karma Parkur | 124 m / 18 sn | 14 m dar → 32 m orta → 40–56 m enerji → 70 m işaretli blok → 88 m hareketli duvar → 108 m dönen kol. |

Süreler ileri hız 7 m/sn üzerinden yuvarlanır. Her sonlu deneme dar durumda, boş enerjiyle başlar. Enerji kapasitesi 5; yarı açıklık .65–2.2 m; açılma/kapanma hızı 6 m/sn; küre yarıçapı .38 m; Pulse menzili 5 m. Bu içerik sayısal hareket ayarlarını değiştirmez.

## İçerik sözleşmesi

`LevelCatalog.Get(0..4)` her çağrıda yeni `CourseDefinition`, engel ve toplanabilir nesneleri üretir. Diziler hiçbir zaman null değildir. `SafeRoute`, oyuncu için otomatik kontrol değildir; entegrasyon testlerinin kullanabileceği gerçek basma/bırakma olaylarının mesafe sırasıdır. Her rota bir basma ile başlar ve bırakma ile biter. Dar geçitten önceki geri çekilme mesafeleri en az tam kapanma süresini karşılar.

Normal engeller yalnız küreleri öldürür; bağ içlerinden geçebilir. `Marked` Pulse duyarlılığını, `Gate` mevcut enerji kapısı modelini belirtir. İşaretli blok `Marked=true, Gate=false` olur. Her Pulse hedefinden önce beş enerji parçası eksiksiz bulunur; ilk basma ile kapıdan 4 m önceki bırakma arasında zorunlu başka bırakma yoktur. Tam açıklıkta bulunan ±1.55 m enerji parçaları dar bağla toplanamaz. Son parça ile bırakma arasında en az 10 m vardır.

Bonuslar Pulse gereksinimi değildir; güvenli ana rotanın tamamını toplaması gerekmez. Örneğin ilk bölümde 16 m dar geçit sonrası 22 m'deki geniş bonus çifti için erken açılmak seçilebilir. Bağı Keşfet'in 62 m, İlk Pulse'ın 72 m, Zamanlama'nın 91 m bonus çiftleri son dar geçidin ardından isteğe bağlı tekrar açılmayı ödüllendirir. Bu seçimler yeni engel/enerji gereksinimi yaratmaz.

Kısa HUD ipuçları yaklaşan eylemi söyler. İlk Pulse'ta kapıya kadar basılı tutma ve menzilde bırakma ayrı gösterilir. Zamanlama bölümünde duvarın konumunu okuma, ardından kolun iki yanından geçme öğretilir.

## Sonsuz Akış

`EndlessCourse.CreateChunk(index, seed=1)` 112 m'lik bir parça üretir; `index` sıfırdan başlar ve negatif olamaz. `Length` parçanın uzunluğudur. Engel/pickup Z konumları, zamanlı engellerin `Center.y` değeri, ipucu ve rota mesafeleri **mutlak dünya mesafesidir**. Son mesafe `(index + 1) * 112` üzerinden bulunur; `Length` son mesafe değildir. Kimlik seed ve index içerir.

Beş tasarlanmış kalıp sırayla döner; seed başlangıç sırasını değiştirir. Seed ve index ayrıca parçanın içerik yerleşimini −2…+2 m kaydırır. Global rastgele sayı durumu kullanılmaz. Aynı seed/index aynı veriyi verir; değişken hız veya görünmez zorluk artışı yoktur.

| Kalıp | Akış |
| --- | --- |
| Dar/geniş ritim | Dar → orta → dar → orta → dar |
| Enerji kapısı | Orta → beş enerji → Pulse kapısı → dar → orta |
| Hareket ritmi | Hareketli duvar geniş → yana kayan duvar dar → dönen kol geniş → dar |
| Karma hareket | Dar → orta → hareketli duvar geniş → dar → orta |
| İşaretli hedef | Orta → beş enerji → kırılabilir blok → dar → orta |

Her parça dar giriş ve dar çıkış sağlar. Son bırakma en geç yerel 104 m'dedir; 112 m sınırından önce kapanma tamamlanır. İlk eylem en erken yerel 6 m'de, ilk engel en erken 14 m'de başlar. Önceki parçadan enerji gerekmez; Pulse içeren parçalar kendi beş enerjisini sağlar. Önceki enerji varsa önceki normal bırakmada zararsız Pulse oluşabilir; sonraki hedefin enerjisi yeniden toplanır. Enerjisiz kalma durumunda kapı ölümcül kalır; otomatik geçiş veya sonsuz bekleme yoktur.

Hareketli engel fazları her parçada **mutlak Z / 7** zamanına göre kalibre edilir. Parça yüklenme anı veya yerel parça zamanı kullanılmamalıdır. Dönen kol kürelerin geçişinde ileri eksene paraleldir; yana kayan duvar geçiş anında merkezden 2.2 m uzaktadır.

## Doğrulama ve teslim sınırı

`CourseDefinitionTests` bölüm sayısı/adları, yeni veri örnekleri, koordinat sınırları, sonlu sayılar, pozitif boyutlar, olay aralıkları, her Pulse için eksiksiz şarj ve kesintisiz basma, deterministik parça kimlikleri/verileri ve mutlak zaman fazlarını denetler. Testler yazıldı; bu alt görev Unity süreci başlatmaz ve çalıştırılmış test sonucu iddia etmez.

Yönetici entegrasyonu gerçek `TetherRushGame` üzerinden beş sonlu rotayı ve art arda sonsuz parçaları farklı kare adımlarıyla yürütmelidir. Kamera/engel görünürlüğü, insan tepki süresi ve bonus riskinin gerçek Unity görüntüleriyle değerlendirilmesi gerekir. GDD dışı yeni görsel veya mekanik eklenmedi; mevcut Blender modelleri kullanılır.
