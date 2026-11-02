# Yerel ilerleme ve ses kaydı

Uygulama 25 bölümün tamamlanma durumunu ve en iyi bonusunu, Sonsuz Akış en iyi skorunu, ses açık/kapalı tercihini ve ses düzeyini cihazda saklar. Eski beş bölümlük kayıtlar açık v1 → v2 göçüyle korunur. Hesap veya çevrimiçi servis içermez.

## Sözleşme

- `TetherRush.Core.PlayerProgress`: `LevelCount = 25`; bölüm indeksleri 0–24. `IsCompleted`, `BestBonus`, `CompleteLevel`, `IsUnlocked` geçersiz indekslerde `ArgumentOutOfRangeException` üretir. Negatif bonus/skor kabul edilmez. Tamamlanma ve rekorlar yalnız yükselir. `CompleteLevel`/`RecordEndless` yalnız kayıt değiştiğinde `true` döner. Önceden var olan public API imzaları korunur.
- Yeni `HighestUnlockedLevel` sıfır tabanlıdır: hiç tamamlanma yoksa 0; aksi hâlde `min(24, en yüksek tamamlanmış indeks + 1)`. `IsUnlocked(index)` bu sınıra kadar `true` döner. Eski sırasız tamamlanmalar korunur; aradaki tamamlanmamış bölümler tamamlanmış sayılmaz. Bu yalnız hesaplanan erişim bilgisidir; kayıt katmanı `CompleteLevel` çağrılarını sıraya göre engellemez. Menüde kilit uygulanıp uygulanmayacağı uygulama katmanının kararıdır.
- İlk ayarlar: ses açık, düzey `0.55`. `Volume` sonlu değerleri 0–1 aralığına sıkıştırır; NaN/sonsuz atamalar `0.55` olur.
- `TetherRush.PlayerSaveStore.Load()` ve `Save(PlayerProgress)` yalnız `TetherRush.Progress.v1` PlayerPrefs anahtarını kullanır. Anahtarın adı korunur; içerik v1 veya v2 olabilir. Yükleme diski değiştirmez; sonraki normal `Save` aynı anahtara v2 yazar. Diğer tercihler silinmez veya değiştirilmez. `Save(null)` hata üretir.
- `CalculateScore(distance, bonus)`: `floor(distance) + bonus * 100`. Negatif mesafe/bonus sıfırlanır; NaN mesafe sıfırdır; pozitif sonsuz ve taşma `int.MaxValue` değerine doyar. Ara hesap çift duyarlıklı olduğundan tamsayı taşması oluşmaz.

## JSON v1 → v2

```json
{"version":1,"audioEnabled":true,"volume":0.55,"bestEndlessScore":0,"completed":[false,false,false,false,false],"bestBonus":[0,0,0,0,0]}
```

Yukarıdaki eski v1 biçimi yalnız **tam beşer** `completed`/`bestBonus` elemanıyla geçerlidir. İlk beş eleman aynen kopyalanır; 5–24 indeksleri `false`/`0` olarak başlatılır. Ses tercihi, ses düzeyi ve Sonsuz Akış rekoru korunur. Yazıcı daima `"version":2` ve **tam 25'er** eleman üretir; diğer alanlar ve anlamları aynıdır. V2 içinde beş eleman veya v1 içinde 25 eleman geçerli sayılmaz. Şema sürümü alanların sonunda da bulunabilir.

Core yalnız `System` kullanır; Unity ve harici JSON paketlerine bağımlı değildir. Küçük şemaya özel okuyucu alan sırasından ve JSON boşluklarından bağımsızdır; tüm altı alan zorunludur. Tekrarlı/bilinmeyen alanlar, 1/2 dışındaki şema sürümü, yanlış dizi uzunluğu, eksik/null diziler, negatif/taşan kayıtlar, tamamlanmamış bölümde pozitif bonus, bozuk/kesik JSON ve 4096 karakterden büyük girdi atomik olarak 25 bölümlük varsayılan kayda döner. En yüksek tamsayı rekorlarıyla dolu 25 bölüm de bu boyut sınırına sığar. Sonlu sayısal ses düzeyi sınır dışındaysa sıkıştırılır; sayısal olmayan/sonsuz kayıt bozuk kabul edilir. JSON sayıları kültürden bağımsız yazılır.

Gelecek sürüm verisi yorumlanmaz; yeni sürüm için ayrı açık göç kodu gerekir. Eski uygulama sürümüne geri dönüş v2 kaydını okuyamaz; geriye doğru uygulama sürümü düşürme desteği yoktur. Kayıt cihaz içi kolaylık verisidir; şifreleme, hile önleme veya bulut yedeklemesi sağlamaz.

## Entegrasyon ve doğrulama

Yüklemeyi başlangıçta bir kez yapın. Kaydı bölüm bitişinde, Sonsuz Akış çarpışmasında yeni rekor oluştuğunda ve ses ayarı değiştiğinde yazın; her karede yazmayın. Yaşam döngüsü callback'inde bekleyen değişiklikler varsa yazılabilir. `PlayerPrefs.Save()` bu anlamlı olaylarda diske aktarır.

`PlayerProgressTests` varsayılanları, 25 bölümün JSON gidiş/dönüşünü, Türkçe kültürde sayıları, monoton kayıtları, indeks/negatif değer hatalarını, ses sınırlarını, bozuk/bilinmeyen/kesik/aşırı büyük kayıtları ve tamsayı rekor sınırını kapsar. `Level25SaveMigrationTests` gerçek eski beş elemanlı JSON sabitinden tüm alanların göçünü, sırasız eski tamamlanmaları, erişim hesabını, 25 bölümün en yüksek değerlerle gidiş/dönüşünü, göçten sonra monoton kayıtları, eksik alanları ve v1/v2 dizi uzunluklarını kapsar. Bu testler yalnız bellekte çalışır; üretim PlayerPrefs anahtarına yazmaz/silmez. `PlayerSaveStoreTests` 13 skor sınırını kapsar; gerçek PlayerPrefs anahtarına dokunmaz.

25 bölümlük göçte PowerShell `Add-Type` ile Core ve iki NUnit test sınıfı derlendi; mevcut Unity paketindeki NUnit assertion'ları ve test case verileriyle reflection üzerinden **37 PlayerProgressTests + 30 Level25SaveMigrationTests = 67/67** vaka geçti. Core/Runtime ayrıca yalnız bellek içi PlayerPrefs taklidiyle derlendi: eski anahtardan v1 yüklemenin yazma yapmadığı, normal Save'in aynı anahtara v2 yazdığı ve bir kez flush yaptığı doğrulandı. Çıktı bağımsız `ConvertFrom-Json` ile okunarak iki dizinin 25 elemanlı olduğu kontrol edildi. Gerçek PlayerPrefs'e dokunulmadı. Bu, gerçek Unity test runner veya disk doğrulaması değildir; Unity EditMode/PlayMode test çalıştırma ana yöneticiye aittir. Alt görev Unity başlatmadı.
