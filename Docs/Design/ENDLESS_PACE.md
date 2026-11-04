# Sonsuz Akış: skorla hızlanma

Kullanıcının son isteği Sonsuz Akış'ta skor arttıkça ileri hızın kademeli artmasıdır. Bu istek yalnız Sonsuz Akış için önceki sabit hız uygulamasını değiştirir. 25 bölümde `RunModel.Tick(dt)` mevcut `RunTuning.ForwardSpeed` değerini, varsayılan 7 m/s'yi kullanmaya devam eder. Kontrol, simetrik açılma, enerji, Pulse ve ölüm kuralları değişmez.

## İlk ayar

Bu sayılar insan oynanış incelemesi için ilk teknik ayardır; ayrı bir kullanıcı kabulü veya fiziksel cihaz doğrulaması değildir.

| Parametre | Değer |
| --- | --- |
| Başlangıç hızı | 7 m/s |
| Alışma aralığı | İlk 500 skor, 7 m/s hedef |
| Hızlanma aralığı | 500–5500 skor |
| Üst hız | 11,5 m/s |
| Azami ivme | 0,25 m/s² |
| Skor kaynağı | Mevcut tam metre + bonus ×100 |

`p = clamp((skor - 500) / 5000, 0, 1)`; `hedef = 7 + 4,5 × p² × (3 - 2p)`. Bu eğri iki uçta yumuşaktır, skora göre azalmadan ilerler ve 5500 skorda üst sınıra ulaşır. Hedef hız örnekleri: 500 →7; 1750 →7,703125; 3000 →9,25; 4250 →10,796875; 5500 →11,5 m/s.

`EndlessPace.Advance(dt, score)` hızı hedefe doğru saniyede en fazla 0,25 m/s yükseltir. Bonusla gelen 100 puan hedefi değiştirir, anlık hızı sıçratmaz. Skor gerilerse hız düşmez; yalnız `Reset()` yeniden 7 m/s yapar. Sıfır zaman adımı hızı değiştirmez. Bileşen Unity bağımlılığı içermez. Zaman adımı sonlu ve negatif olmayan, skor negatif olmayan değerdir; geçersiz girişler durum değişmeden reddedilir.

## Entegrasyon sözleşmesi

Ana çalışma zamanı entegrasyonu ayrı dosya sahipliğindedir. Her Running alt adımında önce `pace.Advance(step, Score)`, ardından `Model.Tick(step, pace.CurrentSpeed)` uygulanır; sonradan toplanan bonus en geç sonraki alt adımda hedefe yansır. Ready, Paused, Failed ve Completed sırasında pace ilerletilmez. Yeni deneme ve retry `Model.StartRun()` ile birlikte `pace.Reset()` çağırır. Bölüm oyununda pace kullanılmaz. Skor ve kayıt şeması değiştirilmez.

`RunModel.Tick(dt, forwardSpeed)` yalnız ileri mesafeyi seçilen hızla ilerletir; açılma/kapanma hızı ve Pulse davranışı aynıdır. Verilen hız pozitif ve sonlu olmalıdır. `ActualElapsedTime` yalnız Running sırasında gerçek simülasyon süresini toplar; reset/retry sıfırlar ve pause/terminal durumlar dondurur. Bu alan tanılama içindir.

Hareketli engellerin mevcut SafeRoute zamanlaması `Distance / 7` parkur fazına göre kalibre edilmiştir. Hız değişince görsel ve temas örnekleri aynı başlangıç/bitiş mesafelerinden hesaplanan bu fazı kullanmalıdır. `ActualElapsedTime` ile engel fazı değiştirilmez; aksi hâlde mevcut güvenli rotalar farklı engel konumlarıyla karşılaşabilir. Çalışma zamanı aynı mesafe fazını görsel, swept temas ve hedef sorgularında tutarlı kullanmalıdır.

## Güvenlik ve doğrulama sınırı

11,5 m/s üst sınırında 4 m civarı etkili Pulse yaklaşma aralığı yaklaşık 0,35 saniyedir; bu geometrik ilk kontrol, insan tepki süresi kabulü değildir. Küre yarıçapı, hedef konumu ve alt adım sırası gerçek menzili belirler. 6 m/s açılma hızıyla yarım açıklığın 0,65'ten 2,2 m'ye geçişi yaklaşık 0,258 saniyedir ve üst hızda yaklaşık 2,97 m ileri yol alır. Bu nedenle mevcut rotaların üst hızdaki oynanabilirliği çalışma zamanı testleriyle ayrıca doğrulanmalıdır.

`EndlessPaceTests` eğri/üst sınır, bonus sıçraması, ivme, reset, geçersiz girdiler, sabit hedefte adım tutarlılığı, eski Tick davranışı, değişken hız ve gerçek süre/pause/terminal sözleşmesini kapsar. Alt ajan Unity çalıştırmamıştır; testlerin çalıştırılması, gerçek Sonsuz Akış rotaları ve build doğrulaması ana yönetici teslimindedir. Bu doküman çalışma zamanı testlerinin geçtiğini iddia etmez.
