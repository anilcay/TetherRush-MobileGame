# Arcade UI referans araştırması

Araştırma: 02.11.2026, 17:12 UTC (20:12 Türkiye). Kapsam: yalnız araştırma; sahibi `UI_REFERENCE_ARCADE.md`. Bağımlılık: onaylı kararlar, mevcut üç gerçek Unity karesi, resmî Google Play ürün sayfaları ve onların görsel CDN bağlantıları. Kabul: üç doğrulanmış 100M+ Android arcade örneği, gerçek piksel incelemesi, gözlem/çıkarım ayrımı ve eksik kanıtların açık kaydı. Kod, varlık veya oyun kapsamı değişmedi.

## Kaynak doğrulaması

| Kod | Resmî ürün / yayıncı | Google Play indirme sınıfı | Sayfa |
| --- | --- | --- | --- |
| H | Helix Jump / VOODOO, `com.h8games.helixjump` | 100M+ | [Google Play](https://play.google.com/store/apps/details?hl=en_US&id=com.h8games.helixjump) |
| S | Stack Ball - Crash Platforms / CASUAL AZUR GAMES, `com.azurgames.stackball` | 500M+ | [Google Play](https://play.google.com/store/apps/details?hl=en_US&id=com.azurgames.stackball) |
| C | Color Bump 3D: ASMR ball game / CASUAL AZUR GAMES, `com.colorup.game` | 100M+ | [Google Play](https://play.google.com/store/apps/details?hl=en_US&id=com.colorup.game) |

Bu değerler mağazanın Android indirme aralıklarıdır; kesin kullanıcı sayısı veya küresel sıralama değildir. Benzer adlı `com.colorbump3d.game` ve `com.ulkaden.stackball` farklı ürünler olduğundan kullanılmadı. Sayfalar aynı gün açıldı; mağaza görsellerinin hangi uygulama sürümünde çekildiği belirtilmiyor.

## Gerçekte görülenler

**H — Helix Jump.** H1/H2 aynı kompozisyonun farklı oranları: merkezde büyük oyun nesnesi, üstte kırmızı boss bandı, 42 ve BEST sayısı. Alt pazarlama bandı ve ateş çıkartması oyuna ait HUD olarak değerlendirilmedi. H3 oyun üstünde sayı/efekt ve altta tanıtım bandı gösteriyor. H4'te beyaz merkez eksen boyunca yeşil 15/16 numaralı yuvarlak platformlar, bir el işareti ve küçük sandık var; alt mor tanıtım bandı, büyük konum simgesi ve bildirim işareti de bulunuyor. Bu, harita biçimli sunum kanıtıdır; gerçek haritanın etkileşimi veya kilit açma kuralı kanıtı değildir. Can/enerji/ayarlar/sonuç ekranı görülmedi.

**S — Stack Ball.** S1/S2 aynı oyun anının farklı oranları: üstte 10 ve 11 daireleri arasında ince ilerleme çizgisi, altında 575; oyun nesnesi ekranın büyük bölümünü kaplıyor. S3 aynı düzeni 3→4 ve 8; S4 6→7 ve 262 ile tekrar ediyor. Sabit HUD plakası, metinli kontrol öğretisi, CTA veya görünür enerji/can göstergesi yok. Birden çok kare düzen tutarlılığına kanıt sağlar; progress dolumunun gerçek zamanlı davranışı bir durağan kareden ölçülemez. Menü, ayarlar ve sonuç/modal görülmedi.

**C — Color Bump 3D.** C1/C2 mor parkur, aynı renkte küçük parçalar, sarı büyük bloklar ve beyaz/mavi izli küre gösteriyor; ikisi benzer anın farklı oranı. C3'te küçük beyaz yüzde 29 görünüyor. C4 başka engel şekilleriyle aynı oyun ağırlığını koruyor. İlerleme barı, enerji/can sayacı, ana menü, ayarlar, sonuç/CTA görünmüyor. Küre izi ve engel karşıtlığı sahne okunurluğu için incelenebilir; mağaza karelerinde HUD bulunmaması ürünün tamamında bulunmadığı anlamına gelmez.

Bu üç ürün hakkındaki gözlemler ürün başına 200 sözcük altında tutulmuştur; tanıtım metinleri yeniden üretilmedi.

## İnceleme matrisi ve kanıt sınırı

| UI konusu | Bu örneklemde doğrulanan | Tether Rush için çıkarım / sınır |
| --- | --- | --- |
| Ana menü | Üç üründe de gerçek ana menü doğrulanmadı | Ana menü tasarımı bu oyunlardan kopyalanmış gibi sunulamaz. |
| Oyun HUD | S: ince üst progress + çıplak skor; H: üst sayılar; C: küçük yüzde | Oyun alanına öncelik, az katman. Her oyuna aynı HUD uygulanmıyor. |
| Bölüm ilerleme | S: mevcut/sonraki sayılar ve çizgi | Tether'da mevcut bölüm + kısa ilerleme çizgisi önerisi. |
| Enerji analoğu | Görsellerden işlevi doğrulanmış enerji göstergesi yok | Pulse enerjisi özgün gereksinim; tek bir küçük gösterge olarak tasarlanmalı. |
| Can sayacı | İncelenen karelerde görünmüyor | Yeni can/kalp ekonomisi önerilmez; tek fatal temas kuralı korunur. |
| Harita | H4: numaralı yuvarlak yüzeyler ve merkez yol | Numara ve yol ilişkisi kullanılabilir; 25 bölüm ve sıralı açılma Tether'ın kendi şartı. |
| İkonlar | H4 el/konum/bildirim işaretleri, ancak tanıtım kompozisyonu | Settings/pause için işlev temelli özgün ikon; reklam çıkartmalarını UI kanıtı sayma. |
| Sonuç, modal, CTA | Runtime sonuç/CTA doğrulanmadı | Sonraki bölüm / Tekrar dene / Harita hiyerarşisi yerel akıştan türetilmeli. |
| Ayarlar | Doğrulanmadı | Ses/titreşim gibi onaylı işlevleri tut; referans adına yeni ayar ekleme. |

Mağaza görselleri editoryal seçilmiş veya tanıtım için düzenlenmiş olabilir. Oyun kurulmadı; hareket, giriş gecikmesi, dokunma alanları, animasyonlar ve reklam akışı test edilmedi. Özellikle Helix alt bantları oyun menüsüyle karıştırılmamalı.

## Mevcut Tether karelerinin teşhisi

`Docs/Art/Previews/Map25_Menu_Home.png`, `Map25_Menu_Level3.png`, `Map25_Progress.png` doğrudan `view_image` ile incelendi. “AI ürünü” değerlendirmesi üretim yöntemine ilişkin tespit değil; aşağıdaki görünür tasarım sorunlarını anlatıyor:

1. **Ana menü bir tanıtım sayfası hiyerarşisi kuruyor.** Büyük iki satırlı başlık, üst slogan, “İKİ KÜRE. TEK BAĞ.” kartı, kart içi komut, CTA altı açıklama ve en alttaki slogan aynı ekranda. Oyuna giriş yerine ürün anlatımı öne çıkıyor. S1–S4'te oyun nesnesi ve kısa üst bilgi baskın. Öneri: logo + sahne içindeki iki küre + Leveller birincil eylemi; Sonsuz Akış ikincil ve Ayarlar küçük kontrol. İşlevler korunur.
2. **HUD kart ve metin tekrarına dayanıyor.** Üst geniş panelde oyun adı/bölüm/iki metin düğmesi/metre/bonus/hedef/progress/enerji; altındaki ikinci pill'de tekrar Pulse hazır bilgisi; en altta tekrar basılı tut/bırak komutu. Bunlar parkurun önünü görsel olarak daraltıyor. Öneri: kısa üst progress, küçük bonus, pause ikonu ve tek Pulse enerji göstergesi; öğretici yalnız gerektiğinde. Pulse butonu eklenmez.
3. **Harita durumları fazla sayıda görsel dille tekrar ediliyor.** Yuvarlaklarda çok katmanlı bevel, cyan çevre, glow, sarı halka, check rozeti ve alt durum etiketleri; ayrıca üstte lejant var. 13 numaralı sıradaki düğümün sarı çerçeve+cyan glow birleşimi semantiği zayıflatıyor. Öneri: tamamlanan küçük check, sıradaki tek belirgin halka, kilitli mat ve küçük kilit; düz yol. Mevcut 25 sayı ve sıralı açılma aynen kalır.

## Uygulama öncesi önerilen kararlar

Bunlar araştırma çıkarımıdır; görsel yönün kullanıcıya sunulmasının yerini tutmaz.

- Kart sayısını azalt; ayırt edici kimliği iki küre ve bağın gerçek kompozisyonundan kur. Yuvarlak köşeli panel her bilgiye uygulanmasın.
- Boyut/konum ve kontrast ile hiyerarşi kur; neon yalnız enerji veya sıradaki bölüm durumunda anlam taşısın. Arka plan efektleri bilgiyle yarışmasın.
- Oyun sırasında “neredeyim / bitiş ne kadar / Pulse hazır mı” tek bakışta çözülsün. Metin öğretisi öğrenme anıyla sınırlansın.
- Sadeleşme Leveller, Sonsuz Akış, Ayarlar ya da 25 düğümlü haritayı kaldırmaz. Dolu enerjide gerçek bırakma tek Pulse üretir; ikinci düğme yoktur. Normal engelleri Pulse ile yok etme veya ek can sistemi yoktur.
- Menü, harita, düşük/dolu enerji HUD, pause, başarısızlık ve başarı birlikte değerlendirilmelidir. Araştırmada görülmeyen sonuç/ayar ekranı davranışı rakiplerde varmış gibi sunulmamalıdır.

## Piksel inceleme günlüğü

Aşağıdaki dosyalar resmî sayfanın “Screenshot image” bağlantılarından elde edildi. Tam görüntüler yalnız geçici klasöre indirildi, `view_image` ile incelendi; proje varlığı olarak kullanılmadı. CDN'de `=w1080` yalnız çözünürlük isteğidir. H1/S1/C1 sırasıyla `%TEMP%/helix-ui-reference.png`, `stack-ui-reference.png`, `color-ui-reference.png`; kalanlar `arcade-ui-0.png`–`arcade-ui-8.png`. Hepsi yukarıdaki araştırma zamanında görüldü.

| Kod | Resmî sayfa bağlantı sırası / gözlem | Tam görsel URL |
| --- | --- | --- |
| H1 | H screenshot 12, boss/tanıtım | [Görsel](https://play-lh.googleusercontent.com/Ox9BI-F53S5NVdPuLnB0C-neO_XmpOPLqgeYDV9HDWnKmaiNETW73jU5sRgMScOmTHnhgwC2cWJCvX1_jl4M=w1080) |
| H2 | H screenshot 18, H1 varyantı | [Görsel](https://play-lh.googleusercontent.com/9wjO7USCiPHJB193L89R7BHmp7kZJ6ypQCLhOuhgQYPpFubI8EQcMbzUVNJKlAy6qYbRpfAXiKm9efs3uBXJgDI=w1080) |
| H3 | H screenshot 19, efekt/tanıtım | [Görsel](https://play-lh.googleusercontent.com/y5jmI9XyMrRQ3txMJKyXH0tT67WYw-tGqFSqf9jhT-G98fuXdu06dLM8DRy1mXtumIPBR2VGwFiEVWqT5Nh-hg=w1080) |
| H4 | H screenshot 20, numaralı yol/tanıtım | [Görsel](https://play-lh.googleusercontent.com/cnsv7R88S848hPImU9r9Z5XBwJ1ZPRUZr67kWLkHrHtzQDq31zbHbKIR3dz60EA0_scEHIGmzXMv0ghw7ooF=w1080) |
| S1 | S screenshot 11, oyun HUD | [Görsel](https://play-lh.googleusercontent.com/WEBFPXYLNkRpsf2cP1dZeuIqMqZYqobKCnzWd1-e8_gLUN4g8_ZZEg5hRceXRLF6wh7Q3wLnxZpX-BSjrUuX6w=w1080) |
| S2 | S screenshot 19, S1 varyantı | [Görsel](https://play-lh.googleusercontent.com/u5DhSYOGu0bj80T63qqwLaZ1NQvwXDn5jMCqc4kI6tLKZwEaj3pxzz0-MvzXC2wyZjpBWBO6nIvNqZdDQ9ejQg=w1080) |
| S3 | S screenshot 20, oyun HUD | [Görsel](https://play-lh.googleusercontent.com/YWMVMpuNQVMyr6vv1E6B0c4HVfbkJOZdByaEk-ATW3PbykoIjn6MoKJak96Cyb2V1Hu1DyabZSmSZH0eNvzA-A=w1080) |
| S4 | S screenshot 21, oyun HUD | [Görsel](https://play-lh.googleusercontent.com/7_VGs3brW4-QC3oFlosWk4F8KxHl3q6oUbmEpSM_sNWtv2U3kZdY-bnlso_9e_qPFQCFAuC27aw6bAHOy6KG=w1080) |
| C1 | C screenshot 11, parkur | [Görsel](https://play-lh.googleusercontent.com/--p0pljT_2q-b3mhXgkD7o7VC1kLE5GLPIoZ2PQTz1Up3qBiYOPDEmS9jDWGaA8guRgoH9dJ0SuVQ-gT3j1Z5nk=w1080) |
| C2 | C screenshot 16, C1 varyantı | [Görsel](https://play-lh.googleusercontent.com/spjZM9HXxalHj7CjZsJK8-ILw9exImubJkA_26asG9kNqx3iaH-AKLZ4MqnA6rlqVxBRN6KJh_xqjyxcHRtg=w1080) |
| C3 | C screenshot 17, yüzde ilerleme | [Görsel](https://play-lh.googleusercontent.com/PvYNnwXwj7x-tO3Sd2AHjaxQ9kOMzYmF_cfpDjj_YEoWLbbFGupWutptYJvtcKcFbZjo2QPqn7rCLsQHv1Hvh9I=w1080) |
| C4 | C screenshot 18, silindirler | [Görsel](https://play-lh.googleusercontent.com/fAqFCMD1OAPRjiM-PuEYGDLrUW99pkQZamomomA9ks4B34CAn0pvp9Prf4ud2gQCT9FAM3g8cWnUqKIf19hENRQ=w1080) |

Numaralar web aracının açılan sayfadaki bağlantı kimlikleridir; gelecekte sayfa sırası değişebilir. URL'ler asıl izlenebilir kayıttır. Web görsel açıcı birkaç CDN bağlantısında cache miss verdi. Sandbox içi doğrudan indirme ağ erişimi vermedi; salt okunur public CDN indirmesi escalation ile başarılı oldu. IAB alt ajan screenshot sınırlaması aşılmaya çalışılmadı; dosya inceleme kullanıldı. Üç ürün için 12 dosya incelendi, bunların bazıları aynı karenin varyantıdır; 12 farklı oyun durumu iddia edilmez.

Teslim: bu belge. Doğrulama: resmî sayfa/ürün kimliği, indirme sınıfları, 12 görselin piksel incelemesi ve üç yerel kare. Açık sınır: runtime menü/ayar/sonuç kanıtı ve gerçek cihaz etkileşim testi yok. `PROJECT_STATUS.md` ilk/ana yönetim raporu yalnız ana yönetici tarafından güncellenir.
