# 25 bölüm içerik paketi

02.11.2026. Kullanıcının 25 bölüm ve yalnız numaralı bölüm isteği doğrultusunda `LevelCatalog` 25 parkur sunar. Görünen başlıklar **Bölüm 1–25**; önceki `level-1`…`level-5` kimlikleri ve ilk beş parkurun tüm oynanış verileri korunur. Bu belge harita görsel tasarımını değil bölüm içeriğini tanımlar.

İleri hız 7 m/sn, küre yarıçapı .38 m, yarı açıklık .65–2.2 m, açılma hızı 6 m/sn, Pulse menzili 5 m, enerji kapasitesi beştir. Oynanış mekaniği, hız artışı veya yıldız eklenmez. Sıralı bölüm açılması D-030 ile harita/uygulama katmanında onaylandı. Sonraki 20 parkur bağımsız engel zaman çizgileridir; tek bir parkurun uzatılmış kopyaları değildir.

| Bölüm | Uzunluk | Yaklaşık süre | İçerik ritmi | Pulse hedefi |
| --- | ---: | ---: | --- | ---: |
| 1 | 56 m | 8 sn | İlk dar/geniş geçişler; özgün düzen | 0 |
| 2 | 64 m | 9 sn | Bağla enerji ve bonus; özgün düzen | 0 |
| 3 | 76 m | 11 sn | İlk enerji kapısı; özgün düzen | 1 |
| 4 | 96 m | 14 sn | İki hareketli duvar ve kol; özgün düzen | 0 |
| 5 | 124 m | 18 sn | İlk karma parkur; özgün düzen | 1 |
| 6 | 124 m | 18 sn | Dar/merkez dönüşleri, geniş ve yan duvar | 0 |
| 7 | 132 m | 19 sn | İki merkez geçişi, kol ve dar bitiş | 0 |
| 8 | 144 m | 21 sn | Dar başlangıç, kapı, geniş duvar | 1 |
| 9 | 152 m | 22 sn | Geniş/yan duvar/kol zinciri ve merkez dönüşü | 0 |
| 10 | 160 m | 23 sn | İki geniş tutuş, yan duvarla dar bitiş | 0 |
| 11 | 168 m | 24 sn | İşaretli blok, kol, son merkez | 1 |
| 12 | 176 m | 25 sn | Merkez/duvar tutuşu, yan duvar, kol | 0 |
| 13 | 184 m | 26 sn | Kapı sonrası yan duvar, geniş duvar ve kol | 1 |
| 14 | 192 m | 27 sn | Sekiz engelde sıklaşan dar/geniş okuma | 0 |
| 15 | 200 m | 29 sn | Merkezden enerjiye, bloktan iki duvara ve kola | 1 |
| 16 | 208 m | 30 sn | Kapı ve blok için ayrı dolum; arada merkez/dar geçit | 2 |
| 17 | 216 m | 31 sn | Dokuz engelde uzun tutuş ve iki yan duvar | 0 |
| 18 | 224 m | 32 sn | Blok ve kapı; her Pulse sonrası geniş duvar | 2 |
| 19 | 232 m | 33 sn | Dokuz engelde duvar/merkez/kol karşılaşmaları | 0 |
| 20 | 240 m | 34 sn | Kapı, yan duvar, kol, dar geçit, yeni dolumla blok | 2 |
| 21 | 248 m | 35 sn | İlk blok sonrası uzun duvar/kol okuma zinciri | 1 |
| 22 | 256 m | 37 sn | Kapı/kol/yan duvar, yeni dolumla blok ve merkez | 2 |
| 23 | 264 m | 38 sn | Geniş/dar başlangıç, blok, duvar/kol, kapı | 2 |
| 24 | 272 m | 39 sn | Kapı/yan duvar/kol, blok ve son dar/geniş zinciri | 2 |
| 25 | 280 m | 40 sn | Üç ayrı dolum ve Pulse, aralarda dar/yan geçiş, geniş bitiş | 3 |

Yeni parkurlarda durum değiştiren normal girdi engelden 10 m önce gelir: yaklaşık 1.43 sn hazırlık, tam açıklık değişimi yaklaşık .26 sn. Yeni 20 parkurda kararlar arasında en az 8 m/1.14 sn bulunur; çoğunlukla daha uzundur. Tüm 25 bölümün veri testi alt sınırı 6 m/.86 sn'dir. Bitişlerde referans girdi bırakılmıştır. Son engel bitişten en az 8 m öncedir; son geniş tutuş bitişten en az 6 m önce bırakılır.

Her yeni Pulse dizisi hedefin 40 m önünde bir merkez bariyeri, 32/28/24/20/16 m önünde beş enerji parçacığı içerir. Gerçek bırakma hedefin 4 m önündedir; son parçacıkla bırakma arasında 12 m/1.71 sn vardır. Önceki Pulse sonrasındaki enerji tekrar kullanılamaz: her hedefin hemen önceki kesintisiz tutuşunda beş yeni parça vardır. Kapı ve işaretli blok aynı onaylı Pulse kuralını kullanır.

Hareketli duvar ve kol fazları mevcut `CourseBuilder` üzerinden dünya Z/7 zamanı ile hesaplanır. `EndlessCourse` ve ortak primitive sözleşmesi değiştirilmedi. Her `Get` yeni parkur, engel, pickup ve hareketli engel nesneleri üretir.

Doğrulama: `Level25DataTests` 25 başlık/kimlik, gerçek dönüşümlü girdi, tepki aralığı, yeni enerji dizileri, veri bağımsızlığı, uzunluk dışında benzersiz oynanış imzaları ve dünya zamanı fazlarını kontrol eder. Gerçek Unity rota, derleme ve PlayMode çalıştırması ana yönetici entegrasyonunda yapılacaktır; bu içerik görevi bunların geçtiğini iddia etmez. İnsan tarafından zorluk ritmi ve Android dokunma kabulü ayrıca değerlendirilir.
