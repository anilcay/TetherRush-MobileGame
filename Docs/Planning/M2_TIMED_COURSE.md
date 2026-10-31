# M2 zamanlama alıştırması — entegrasyon teslimi

Kapsam: M1'e dokunmayan ikinci 130 m alıştırma; iki yatay kayan duvar ve bir dönen kol. Dosya sahipliği: `Scripts/Runtime/Course/*`, `Tests/PlayMode/TimedHazardTests.cs`, bu belge. Ana yönetici oyun entegrasyonunu, Unity doğrulamasını ve PROJECT_STATUS güncellemesini yapar. Core, asmdef ve TetherRushGame değiştirilmedi. Yeni giriş mekaniği yoktur.

## Entegrasyon

- `TimedCourseLayout.Create()` her deneme için yeni tanımlar döndürür. Başlangıç dar geçidi z20'de ana yönetici kurar; bitiş `TimedCourseLayout.FinishDistance = 130`.
- Tek yetkili zaman `Model.Distance / tuning.ForwardSpeed`. Çarpışmada önceki ve sonraki mesafeden iki zaman üret; her küre için eski/yeni `(x,z)` ve gerçek SphereRadius ile `HitsSweptSphere` çağır. Fatal temas Pulse'dan ve bitişten önce çözülür. Tether ve Pulse bu engelleri etkilemez.
- Mevcut en fazla 1/120 s alt adımlar korunur; sınıf büyük aralıklar için de içten adaptif tarar. Kürenin açılma sınırına eriştiği kırılmaları ana simülasyon alt adımları çözmeye devam eder; API doğrusal küre segmenti varsayar.
- Görsel konum `(PositionAt(t).x, .65, PositionAt(t).y)`, ölçü `(2*HalfExtents.x, 1.3, 2*HalfExtents.y)` ve Y dönüşü `-AngleAt(t)*Rad2Deg`. Yetkili X/Z OBB ile görsel birebir örtüşür. Dönme merkezi değişmez; duvarın yalnız X konumu sinüsle değişir.
- Pause mesafeyi dondurduğu için engeller de donar. Retry mesafeyi sıfırlar. Time.time, delta biriktirme veya GameObject dönüşünden geometri türetme kullanılmaz.
- Ekranda engel görünürken mevcut Hint gösterilir. İpucunu geçişten en az 21 m önce başlatmak, varsayılan hızda 3 s bildirim verir; kamera da en az bu uzaklığı göstermelidir. Önerilen başlangıçlar: z24, z54, z79.

## Geometri ve hassasiyet

Küre merkezinin gerçek döndürülmüş kutuya signed distance değeri hesaplanır; genişletilmiş AABB kullanılmaz. Her zaman aralığının orta noktasındaki boşluk, aralığın yarısındaki toplam olası göreli yer değiştirmeden büyükse aralık kesin güvenlidir. Toplam sınır kürenin doğrusal yol uzunluğu + süre * rijit hareket hızı; duvarda hız sınırı amplitude*abs(angularSpeed), kolda köşe yarıçapı*abs(angularSpeed). Güvensiz aralıklar ikiye bölünür. Temas ve uç noktada temas ölümcüldür; son konservatif aralıkta olası fazla ölüm boşluğu en çok **0.0005 dünya birimi**. Sınıf enerji/Pulse durumu bilmez. NaN/Infinity, negatif zaman/yarıçap, ters zaman, sıfır/negatif kutu boyutu açık exception üretir; sıfır süre geçerlidir ve mekânsal segmenti yine tarar.

## Bilinen güvenli rota

Varsayılan hız 7, yarıçap .38, yarı açıklık dar .65 / geniş 2.2 için:

| Mesafe | Zaman | Eylem / durum |
| --- | --- | --- |
| 0–25 | 0–3.57 s | Dar; z20 giriş geçidi. |
| 25–28 | 3.57–4 s | Basılı tut, genişle. Gerçek SeparationSpeed=6 ile daha erken tamamlanır. |
| 28–55 | 4–7.86 s | Geniş kal; ilk duvar z45'te t6.43 s civarı merkezde. En büyük dış kenarı 1.30, küre iç kenarı 1.82. |
| 55–58 | 7.86–8.29 s | Bırak, daral. |
| 58–85 | 8.29–12.14 s | Dar kal; ikinci duvar z75'te t10.71 s civarı x2.2. |
| 85–88 | 12.14–12.57 s | Basılı tut, genişle. |
| 88–130 | 12.57–18.57 s | Geniş kal; z100 kolu t14.29 s geçişinde ileri doğrultuya hizalanır. Yaklaşma ve uzaklaşma da sürekli OBB taramasıyla güvenlidir. Bitişe ilerle. |

İlk duvara dar girme ve ikinci duvara geniş girme gerçekten ölümcüldür. Kolun yarı uzunluğu 2.7, toplam boyu 5.4 m'dir; yatay fazda x±2.2 geniş kürelere de ulaşır. Güvenli geçiş t14.29 s civarında ileri doğrultuya hizalanmasına bağlıdır; geniş açıklık tüm açılarda güvenli değildir. Ayrı test, yatay fazda iki geniş küreye de temas ettiğini, gerçek yaklaşma/geçiş/uzaklaşmanın tasarlanan fazda güvenli olduğunu doğrular. Fazlar t=0'a bağlı sabittir; başka ForwardSpeed seçilirse ikinci duvar ve kol zamanlaması yeniden doğrulanmalıdır. M1 içeriği değişmez.

## Kabul ve doğrulama

12 NUnit PlayMode vakası: bilinen sinüs fazları, gerçek OBB/AABB köşe ayrımı, iki yönden tarama, hareketli duvarın sabit küreyi kesmesi, tam devirli kol aralığı, yuvarlatılmış kutu köşesi, tam temas, küçük küre/hızlı hareket, deterministik zaman/reset, iki küreli güvenli rota ve hatalı rota teması, kolun geniş kürelere yatay fazda erişmesi ve doğru fazdaki güvenli geçişi, hatalı girdiler. Bu ajan Unity çalıştırmadı; testler ana yöneticinin Unity runner doğrulamasını bekler. Manuel kamera/3 s okunabilirlik ve gerçek cihaz incelemesi açık kalır.

2.7 m kol güncellemesinden sonra Unity başlatılmadan .NET 8 geçici konsol harness'inde gerçek `TimedHazard.cs` ve `TimedCourseLayout.cs`, kurulu UnityEngine.CoreModule referansıyla derlenip çalıştırıldı. Gerçek `HitsSweptSphere` üzerinden 42 tam rota taraması (3 engel × 7 segment × 2 küre) ve 6 yatay faz/varış/yaklaşma-uzaklaşma kontrolü: **48/48 geçti**. Bu, Unity PlayMode test sonucunun yerine geçmez.
