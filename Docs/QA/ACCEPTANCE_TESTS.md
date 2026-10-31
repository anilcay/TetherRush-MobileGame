# Tether Rush — Kabul senaryoları

Kaynak: `Docs/Source/GDD-extracted.txt`, GDD V2.0, 31.10.2026. Sayfa numaraları kaynak dosyadaki `SAYFA` ayraçlarına karşılık gelir. Bu belge test planıdır; oyun çalıştırılmamış, senaryolar yürütülmemiştir. **Tüm yürütüm durumları: BEKLİYOR.**

GDD kuralı olarak belirtilmeyen davranışlar aşağıda **ÖNERİ / KARAR BEKLİYOR** etiketi taşır. Özellikle ilk milestone kapsamı, sayısal ayarlar, giriş iptali, yeniden başlatma ve aynı karede gerçekleşen olayların önceliği yönetim sohbetinde kullanıcı onayına sunulmalıdır. Öneriler onaylanmadan bağlayıcı GDD şartı veya geçmiş test sonucu sayılmaz.

## Kapsam ve hazırlık

**İlk milestone kapsam önerisi:** tek oynanabilir kısa parkurda otomatik ilerleme, simetrik mesafe kontrolü, statik dar geçit ve orta bariyer, ölümcül küre çarpışması, bağ ile enerji, küre ile bonus, tek seferlik Pulse, işaretli kırılabilir blok ve enerji kapısı, bitiş/hata ekranı, yeniden başlatma ve temel UI. Kaynak: s. 3–6, 8. Bu kapsam ayrımı GDD'de milestone olarak tanımlanmamıştır; onay bekleyen uygulama önerisidir.

**Daha sonraki milestone önerisi:** hareketli duvar ve dönen kol (s. 5–6, 8), ayrıntılı bağ esnemesi/ışık izi/parçacık akışı/şok dalgası/çarpışma animasyonu ve ses-his cilası (s. 4, 7). İlk milestone'da enerji, hazır olma, Pulse ve çarpışma durumları basit ve okunabilir biçimde ayırt edilebilmelidir; ayrıntılı animasyonun ertelenmesi bu işlevsel geri bildirimi kaldırmaz.

Test düzeneklerinde başlangıç enerjisi, tam enerji kapasitesi, minimum/maksimum küre ayrımı, ileri hız, Pulse menzili ve hedef konumları kaydedilmelidir. Sayısal değerler GDD'de verilmediğinden bu belge değer uydurmaz. Simetri/hız toleransı ve menzil sınır ölçümü uygulama öncesinde onaylanmalıdır. Girdi olayları, enerji öncesi/sonrası, Pulse sayısı ve hedef aktivasyonları gözlenebilir olmalıdır; bu gereklilik bir oyun içi debug arayüzü şartı değildir.

## İlk milestone — GDD kuralları

| ID | Kaynak | Hazırlık ve eylem | Beklenen kabul sonucu | Yürütüm |
|---|---|---|---|---|
| M1-01 | s. 3, 8 | Güvenli düz alanda giriş basılı değilken başla; basılı tut, sınırda tutmaya devam et, sonra bırak. | Basılı tutarken küreler açılır; bırakınca yaklaşır. Her iki yönde akıcı geçiş olur; ayrım belirlenmiş iki sınırı aşmaz. İkinci bir kontrol/Pulse düğmesi gerekmez. | BEKLİYOR |
| M1-02 | s. 3 | Açılma ve kapanma boyunca iki kürenin konumlarını ve ortak merkezini izle. | Küreler merkezin iki yanında eşit uzaklıktadır. Mesafe kontrolü ortak merkezin yanal konumunu değiştirmez. Ortak merkez ileri otomatik hareketi sürdürür. | BEKLİYOR |
| M1-03 | s. 3 | Aynı düz alanda önce bırakılmış, sonra basılı girişle ileri hareketi ölç. | İleri hareket kendiliğinden sabit tempodadır; basılı tutma/bırakma ileri hız kontrolü sağlamaz. | BEKLİYOR |
| M1-04 | s. 3, 5–6 | Statik dar geçide daralarak, orta bariyere genişleyerek yaklaş; her birini uygun ayrımla geç. | İki küre de engellere değmeden ilerleyebilir. Dar geçit daralmayı, orta bariyer genişlemeyi gerektiren okunabilir geometridedir. | BEKLİYOR |
| M1-05 | s. 3, 5, 8 | Önce yalnız sol küreyi, ayrı denemede yalnız sağ küreyi ölümcül engele çarptır. | Her iki durumda da tek kürenin ölümcül çarpışması denemeyi bitirir; diğer kürenin güvenli olması denemeyi kurtarmaz. | BEKLİYOR |
| M1-06 | s. 3, 5 | Küreler duvarın iki yanında güvenli kalırken bağ standart orta duvarı kessin. | Bağ standart duvardan geçebilir; bu temas ölüm, duvarın kırılması veya bağa uygulanan fiziksel engelleme üretmez. Küre teması M1-05 kapsamındadır. | BEKLİYOR |
| M1-07 | s. 4–5, 8 | Kısmi/boş enerjiyle, kürelere değmeyen bir enerji parçacığını bağlantı hattıyla kes. Ayrı denemede parçacık hatta değmesin. | Yalnızca hatta değen enerji parçacığı enerji göstergesini doldurur; kaçırılan parçacık enerji kazandırmaz. | BEKLİYOR |
| M1-08 | s. 5–6 | Bağ hattının dışında bir bonus kristale önce sol, ayrı denemede sağ küreyle dokun; ayrıca kristali yalnız bağ kessin. | Her iki küre bonus toplayabilir, bonus sayacı/skoru artar. Yalnız bağ teması bonus toplamaz. Bonus toplamak Pulse için gereken enerji toplamanın yerine geçmez. | BEKLİYOR |
| M1-09 | s. 3–4, 8 | Enerjiyi boş ve ayrı denemede tam kapasitenin altında tut. Basılı tutup gerçek bir bırakma eylemi yap. | Küreler daralır; Pulse oluşmaz ve işaretli hedefler bu eylemle etkinleşmez. | BEKLİYOR |
| M1-10 | s. 3–4, 8 | Basılı tutarken enerji dolsun; basılı tutmaya devam et, ardından bir kez gerçekten bırak. Sonraki karelerde giriş bırakılmış kalsın. | Dolu enerji beklerken kendiliğinden Pulse oluşmaz. Gerçek bırakmada daralma ile birlikte tam bir Pulse oluşur ve enerji tüketilir. Bırakılmış durumun devamı yeni Pulse üretmez. | BEKLİYOR |
| M1-11 | s. 3, 8 | M1-10 sonrasında enerji toplamadan tekrar bas ve bırak. Daha sonra yeniden tam enerji toplayıp tekrar bas ve bırak. | İlk tekrar Pulse üretmez; yeniden dolan enerjiyle sonraki gerçek bırakma bir yeni Pulse üretir. Her Pulse için bir tam enerji tüketimi gerçekleşir. | BEKLİYOR |
| M1-12 | s. 4–5, 8 | Normal ölümcül engel Pulse menzili içindeyken tam enerjiyle bırak; sonra bir küreyi aynı engele değdir. | Pulse normal engeli yok etmez veya etkisizleştirmez. Sonraki ölümcül temas denemeyi bitirir. | BEKLİYOR |
| M1-13 | s. 4–5 | İşaretli kırılabilir bloklardan birini kısa menzil içine, diğerini menzil dışına koy. Tam enerjiyle bırak. | İçerideki işaretli blok parçalanır, dışarıdaki etkilenmez. İşaretsiz normal blok Pulse hedefi olmaz. Menzilin tam sınırındaki sonuç için K-02 kararı gerekir. | BEKLİYOR |
| M1-14 | s. 4–5, 8 | İşaretli enerji kapısını menzil içinde ve ayrı denemede dışında test et; ayrıca boş enerjiyle yalnız daralma/genişleme yap. | Yakın menzilli Pulse kapıyı etkinleştirir/açar. Menzil dışındaki Pulse ve yalnız daralma/genişleme kapıyı açmaz. Menzil hesabı K-02 ile kesinleşir. | BEKLİYOR |
| M1-15 | s. 6, 8 | Her iki küreyi bitişe sağ ulaştır; bonuslu ve bonussuz denemeleri karşılaştır. | Başarı oluşur ve bonus skoru görüntülenir. Bonus olmaması başarıyı engellemez; Pulse kullanımı tek başına zorunlu puan kaynağı değildir. Ek yıldız sistemi GDD'de isteğe bağlıdır, ilk milestone zorunluluğu sayılmaz. | BEKLİYOR |
| M1-16 | s. 4, 7 | Başlangıcı, enerji toplamayı, enerji dolmasını ve bonus toplamayı gözle. | Üst UI bölüm adı, enerji barı ve bonus sayısını gösterir; değerler oyun durumuyla tutarlıdır. Başta kısa “Basılı tut / Bırak” ipucu görünür. Pulse hazır durumu bar/bağ üzerinde okunabilir görsel değişiklikle ayırt edilir. UI yaklaşan engelleri ve bağı takip etmeyi engellemez. Ayrıntılı efektler sonraki milestone'a bırakılabilir. | BEKLİYOR |
| M1-17 | s. 7 | Dar geçit, bariyer ve Pulse kapısına yaklaşırken kamera ve hedef görünümlerini incele. | Hafif yukarıdan arkayı izleyen kamera yaklaşan engeli önceden görmeye izin verir. Normal tehlike ile Pulse hedefi görsel olarak ayırt edilebilir; çarpışmanın nedeni okunabilir. | BEKLİYOR |

## İlk milestone — ürün kararı gerektiren sağlamlık senaryoları

Aşağıdaki davranışlar GDD'de açıkça tanımlı değildir. Her satırın beklenen sonucu **öneridir**; karar onayı ve yürütüm birbirinden ayrıdır. Hiçbir senaryo geçmiş kabul edilmez.

| ID | Dayanak / boşluk | Hazırlık ve eylem | Önerilen kabul sonucu | Karar | Yürütüm |
|---|---|---|---|---|---|
| R-01 | s. 3, 8; terminal durum akışı eksik | Ölümden ve ayrı denemede başarıdan sonra bas/bırak girdileri gönder. | Sonuç tek kez kesinleşir; oynanış ilerlemesi/toplama/Pulse durur. Hata veya başarı sonucu açık gösterilir ve yeniden başlatma erişilebilirdir. Terminal durumlar arasında geçiş olmaz. | BEKLİYOR | BEKLİYOR |
| R-02 | s. 3, 7; restart tanımlı değil | Enerji/bonus biriktir; basılı tutarken öl veya bitir. Yeniden başlat, önceki parmağı bırak ve yeni bas/bırak yap. | Yeni deneme başlangıç konumu, ayrımı, hız, enerji, bonus, hedefler ve UI için tanımlı başlangıç durumuna döner. Eski basılı giriş/bırakma olayı yeni denemede açılma veya Pulse üretmez; yeni giriş normal çalışır. Başlatma UI tıklaması oynanış girdisi sayılmaz. | BEKLİYOR | BEKLİYOR |
| R-03 | s. 3; odak kaybı tanımlı değil | Tam ve kısmi enerjiyle basılı tutarken odak kaybet/uygulamadan çık; sonra odağa dön ve eski dokunuşu bırak. | Odak kaybı gerçek oyuncu bırakması sayılmaz; Pulse üretmez ve enerji tüketmez. Giriş tutulma durumu temizlenir; dönüşte eski dokunuşun devamı/bırakılması kabul edilmez. Yeni fiziksel basış gerekir. Oyun zamanı için öneri: odak yokken duraklat, dönüşte güvenli biçimde sürdür. | BEKLİYOR | BEKLİYOR |
| R-04 | s. 3–4; aynı kare sırası eksik | Enerji kapasiteden bir parçacık eksik olsun; parçacık teması ve gerçek bırakma aynı simülasyon adımına düşsün. Olayları ters teslim sırasıyla da tekrarla. | K-01 önerisi onaylanırsa: o adımdaki geçerli enerji toplama önce çözülür, sonra bırakma tam enerjiyle değerlendirilir; tek Pulse ve tek tüketim olur. Sonuç olay teslim sırasına göre değişmez. | BEKLİYOR | BEKLİYOR |
| R-05 | s. 4–6; zorunlu kapıda kaynak yetmezliği | Kapı öncesi enerjinin bir kısmını kaçır, dolu Pulse'u erken tüket ve ayrı denemede menzil dışında bırak. | K-03 ile seçilen politikaya göre oyuncu anlaşılır bir sonuç alır: ulaşılabilir telafi enerjisi, isteğe bağlı kapı/alternatif rota veya açık hata ve yeniden deneme. Sonsuza kadar kapı önünde kalma ve açıklamasız ilerleyememe olmaz. | BEKLİYOR | BEKLİYOR |

## Onaylanacak kararlar

- **K-01 — Aynı simülasyon adımındaki toplama/bırakma sırası.** GDD s. 3–4 “gösterge doluyken bırakma” der; aynı kare sınırını tanımlamaz. **Öneri:** geçerli toplama → kapasiteye sınırla → gerçek bırakmayı değerlendir → varsa tek Pulse için enerjiyi bir kez tüket. Önceden bırakılmış durumda sonradan dolan enerji otomatik Pulse üretmemeli. Ölüm/bitiş ile bırakma aynı adımda olursa terminal durumun önceliği ayrıca kararlaştırılmalı; öneri ölüm kesinleştiğinde yeni Pulse üretmemektir.
- **K-02 — Pulse menzilinin ölçümü.** GDD s. 4–5 kısa menzil söyler; kaynak noktası, mesafe metriği, hedefin merkezinden/yüzeyinden ölçüm ve eşitlik sınırı belirtilmez. **Öneri:** üretim anındaki küre ortak merkezinden hedefin etkin collider'ındaki en yakın noktaya uzaklık, `uzaklık <= menzil` ise etkilenme; anlık tek tarama. Sayısal menzil kullanıcı onayıyla ayarlansın. Bu onay olmadan tam sınır testi hükme bağlanamaz.
- **K-03 — Enerji kapısında softlock.** GDD s. 5–6 kapının Pulse istediğini belirtir; kaçırılmış veya erken tüketilmiş enerji için kurtarma tarif etmez. **Öneri:** ilk zorunlu kapıdan önce yeterli ve yeniden kazanılabilir enerji sağlayan düzen veya başarısızlığı açıkça sonlandıran düzen seçilsin; kapalı kapıya temasın ölümcül olup olmadığı ayrıca onaylansın. Salt “başlangıçta yeterli parçacık vardı” kanıtı erken tüketim sorununu çözmez. R-05 her kayıp yolunu ayrı sınar.
- **K-04 — Giriş yaşam döngüsü ve yeniden başlatma.** R-01–R-03 davranışları; başlangıç ayrımı/enerjisi, odak dönüşünde devam yöntemi ve yeniden başlatma etkileşimi kullanıcı onayı gerektirir. Öneri başlangıçta minimum ayrım, boş enerji ve sıfır bonustur; bunlar GDD'de açık değer olarak verilmez.
- **K-05 — Enerji/bonus tekrar teması.** GDD s. 4–5 toplama miktarlarını ve nesne yaşam döngüsünü belirtmez. **Öneri:** her parçacık/kristal bir kez tüketilsin; kalıcı overlap veya iki kürenin aynı kristale teması çift kazanç vermesin. Kapasite üstü enerji birikmesin; enerji ve bonus kazançları ayrı ayarlansın. Onaydan sonra M1-07/M1-08 tekrar temas denemeleriyle genişletilsin.

## Sonraki milestone — hareket ve sunum

| ID | Kaynak | Senaryo ve beklenen kabul sonucu | Yürütüm |
|---|---|---|---|
| L-01 | s. 5–6, 8 | Hareketli duvarın farklı fazlarında genişleme/daralma ile güvenli geçiş ve tek küre ölümcül teması denensin. Duvar ileri-geri hareket eder; zamanlama sonucu anlaşılır, temel ölüm ve bağdan geçme kuralları korunur. | BEKLİYOR |
| L-02 | s. 5 | Dönen kolun farklı açılarında geçiş denensin. Dönüş geçiş zamanlaması gerektirir; küreye ölümcül temas denemeyi bitirir. Dönüş hızı/değişim örüntüsü GDD'de sayısal değildir, ayrıca onaylanır. | BEKLİYOR |
| L-03 | s. 4, 7 | Açılma/kapanma, toplama, dolu enerji, Pulse ve çarpışma görsel geri bildirimleri incelensin. Akıcı bağ esnemesi/ışık izi, bağ boyunca enerji akışı, doluyken parlama ve hafif titreşim, kısa şok dalgası ve okunabilir çarpışma tepkisi ilgili olaya bağlı çalışır. Animasyon testinden geçmek mekanik test yerine geçmez. | BEKLİYOR |
| L-04 | s. 7 | Ses/his etkin ortamda hareket, toplama, dolma, Pulse, başarı ve hata dinlensin. GDD'deki kısa elektronik hareket sesi, kısa toplama sesi, dolma ipucu ve kısa Pulse vurgusu ayırt edilir; başarı ve hata sesleri farklıdır. | BEKLİYOR |

## Yürütüm kaydı ve kabul kapısı

Test koşusunda her ID için yapı sürümü, platform/girdi türü, sahne/ayarlar, eylem, gözlenen sonuç ve kanıt kaydedilir. Durum yalnız fiilî yürütümden sonra GEÇTİ/KALDI olarak değiştirilir; karar bekleyen öneri onaylanmadan GEÇTİ sayılmaz. Mobil tek dokunma ve geliştirmede kullanılan fare uyarlaması için desteklenen platformlarda giriş yaşam döngüsü ayrıca denenir.

İlk milestone kabulü için onaylanan kapsamın M1 senaryoları ve onaylanmış R senaryoları yürütülmeli; başarısız sonuçlar ve açık kararlar yönetim sohbetine bildirilmelidir. L senaryolarının BEKLİYOR kalması, sonraki milestone kapsamı kullanıcıca onaylandıysa ilk milestone başarısı gibi raporlanmaz ve kapsam dışı olarak açıkça belirtilir. Bu belgenin mevcut durumu yalnızca **hazırlık / tüm yürütümler BEKLİYOR** durumudur.
