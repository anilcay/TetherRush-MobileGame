# Yerel ilerleme ve ses kaydı

M3 uygulaması beş kısa bölümün tamamlanma durumunu ve en iyi bonusunu, Sonsuz Akış en iyi skorunu, ses açık/kapalı tercihini ve ses düzeyini cihazda saklar. Hesap veya çevrimiçi servis içermez.

## Sözleşme

- `TetherRush.Core.PlayerProgress`: `LevelCount = 5`; bölüm indeksleri 0–4. `IsCompleted`, `BestBonus`, `CompleteLevel` geçersiz indekslerde `ArgumentOutOfRangeException` üretir. Negatif bonus/skor kabul edilmez. Tamamlanma ve rekorlar yalnız yükselir. `CompleteLevel`/`RecordEndless` yalnız kayıt değiştiğinde `true` döner.
- İlk ayarlar: ses açık, düzey `0.55`. `Volume` sonlu değerleri 0–1 aralığına sıkıştırır; NaN/sonsuz atamalar `0.55` olur.
- `TetherRush.PlayerSaveStore.Load()` ve `Save(PlayerProgress)` yalnız `TetherRush.Progress.v1` PlayerPrefs anahtarını kullanır. Diğer tercihler silinmez veya değiştirilmez. `Save(null)` hata üretir.
- `CalculateScore(distance, bonus)`: `floor(distance) + bonus * 100`. Negatif mesafe/bonus sıfırlanır; NaN mesafe sıfırdır; pozitif sonsuz ve taşma `int.MaxValue` değerine doyar. Ara hesap çift duyarlıklı olduğundan tamsayı taşması oluşmaz.

## JSON v1

```json
{"version":1,"audioEnabled":true,"volume":0.55,"bestEndlessScore":0,"completed":[false,false,false,false,false],"bestBonus":[0,0,0,0,0]}
```

Core yalnız `System` kullanır; Unity ve harici JSON paketlerine bağımlı değildir. Küçük şemaya özel okuyucu alan sırasından ve JSON boşluklarından bağımsızdır; tüm altı alan zorunludur. Tekrarlı/bilinmeyen alanlar, bilinmeyen şema sürümü, yanlış dizi uzunluğu, negatif/taşan kayıtlar, tamamlanmamış bölümde pozitif bonus, bozuk/kesik JSON ve 4096 karakterden büyük girdi atomik olarak varsayılan kayda döner. Sonlu sayısal ses düzeyi sınır dışındaysa sıkıştırılır; sayısal olmayan/sonsuz kayıt bozuk kabul edilir. JSON sayıları kültürden bağımsız yazılır.

Sürüm değiştiğinde ayrı açık göç kodu gerekir. Gelecek sürüm verisi v1 tarafından yorumlanmaz. Kayıt cihaz içi kolaylık verisidir; şifreleme, hile önleme veya bulut yedeklemesi sağlamaz.

## Entegrasyon ve doğrulama

Yüklemeyi başlangıçta bir kez yapın. Kaydı bölüm bitişinde, Sonsuz Akış çarpışmasında yeni rekor oluştuğunda ve ses ayarı değiştiğinde yazın; her karede yazmayın. Yaşam döngüsü callback'inde bekleyen değişiklikler varsa yazılabilir. `PlayerPrefs.Save()` bu anlamlı olaylarda diske aktarır.

`PlayerProgressTests` varsayılanları, beş bölümün JSON gidiş/dönüşünü, Türkçe kültürde sayıları, monoton kayıtları, indeks/negatif değer hatalarını, ses sınırlarını, bozuk/bilinmeyen/kesik/aşırı büyük kayıtları ve tamsayı rekor sınırını kapsar. `PlayerSaveStoreTests` 13 skor sınırını kapsar; gerçek PlayerPrefs anahtarına dokunmaz.

Alt görevde Core ve Runtime kaynakları PowerShell `Add-Type` ile derlendi; yalnız geçici bellek içi PlayerPrefs taklidi kullanıldı. Geçerli JSON çıktısı bağımsız `ConvertFrom-Json` ile okundu; gidiş/dönüş, tüm kesilme noktaları, bilinmeyen sürüm, kayıt sarmalayıcısı ve yedi skor sınırı geçti. Bu, gerçek Unity/PlayerPrefs disk doğrulaması değildir. Unity EditMode/PlayMode test çalıştırma ana yöneticiye aittir; alt görev Unity başlatmadı.
