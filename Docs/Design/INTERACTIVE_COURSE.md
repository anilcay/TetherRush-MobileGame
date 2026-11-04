# Etkileşimli parkur — bağ rölesi ve Pulse kilidi

Kullanıcı iki yeni mekanizmayı onayladı. Kontrol tek dokunuştur: basılı tutmak simetrik açar, bırakmak daraltır; dolu enerjiyle gerçek bırakma tek Pulse üretir. Normal ölümcül engeller Pulse ile açılmaz.

## Bağ rölesi

İletken pedin düzleminden en az 1,5 m yarı açıklıkla geçen bağ, aynı parkur parçasındaki kimliği eşleşen kapıyı açar. Enerji gerekmez ve tüketilmez. Kapı açılmadan önce iki küre için de ölümcüldür; Pulse hedefi değildir. Ped ile kapı arası 14 m; açılma işareti ve kablo görseli bağlantıyı açıklar. Kimlikler her `BuildContent` çağrısında ayrı çözülür; sonsuz parçalar birbirinin rölesini tetiklemez.

İlk öğretim Bölüm 6: dar geçit 14, basış 30, ped 40, kapı 54, bırakış 68. Bölüm 12'de ikinci kullanım ped 88 / kapı 102. Sonsuz Akış'ın üçüncü kalıbında basış 26 / ped 42 / kapı 56 / bırakış 66; aynı 112 m parça boyu, deterministik hash ve ±2 m jitter korunur.

## Pulse kilidi

Amber işaretli hareketli kiriş, beş yeni enerjiyi topladıktan sonra kısa menzilde gerçek bırakmayla geri çekilir. Kısmi enerji ve menzil dışı Pulse kilidi açmaz. Kirişin yarı boyutları X/Z = 3,6 / 0,35 m; yatay salınım 0,3 m ve açısal hız 0,6 rad/s. Bütün salınım boyunca koridoru kapatır; enerjisiz açık bir şeritten geçme yolu yoktur. `PulseLock` yalnızca davranış işaretidir; mevcut hareket ve OBB/süpürme matematiği korunur.

İlk öğretim Bölüm 8: kilit 74, enerji 42–58, kesintisiz basış 24'ten menzilde bırakış 70'e kadar. Tekrarlar Bölüm 16/20/25; mevcut statik Pulse dizilerindeki tek hedef yerine kullanılır. Bölüm 25'te toplam üç Pulse gerekir. Sonsuz Akış'ın beşinci kalıbında kilit 58, enerji 26–42, basış 8 / bırakış 54. Önceki parçadan enerji gerektirmez.

## Güvenlik payı ve kapsam

Her yeni röleye giden son basış en az 10 m öncedir; 11,5 m/s hız üst sınırında tam açılma yaklaşık 2,97 m sürer. Her Pulse kilidine giden beş enerji aynı kesintisiz basışta, kilitten 32–16 m önce bulunur. Bırakış kilitten 4 m önce, ön yüzüne 3,65 m mesafede ve 5 m Pulse menzilindedir. Kilit açılmadan çarpışmayı atlama yoktur.

İlk beş bölümün mevcut veri içeriği, 25 bölüm kimliği/uzunlukları ve kayıt ilerlemesi korunur. Statik normal engeller ve normal hareketli duvarlar/dönen kollar değişmez. Yeni veri testleri `InteractiveCourseTests` içinde yerel röle eşleşmesini, geniş tutuşu, enerji dizisini, tüm fazlarda koridor kapanmasını ve sonsuz beşli çevrimde dağılımı sınar. Gerçek Unity çalıştırma ve oynanış doğrulaması ana yöneticinin entegrasyon testlerine bağlıdır; bu belge tek başına testin geçtiği iddiası değildir.
