# Runner UI referans araştırması

Tarih: 02.11.2026. Araştırma teslimidir; uygulama veya görsel yön onayı değildir.

## Kapsam ve yöntem

- Sahiplik: yalnız bu belge. Unity, oyun kodu, karar kaydı, proje durumu ve Git işlemleri bu görevin dışında.
- Bağımlılık: `Docs/Planning/DECISIONS.md` ve `PROJECT_STATUS.md` okundu. Tek kontrol, 25 bölüm, sıralı açılma, Sonsuz Akış ve mevcut ses ayarları korunacak.
- Kabul: dört gerçek oyunun resmî mağazasıyla ölçek doğrulama; gerçek görsel inceleme; reklam kompozisyonunu HUD'dan ayırma; görünmeyen ekranları açıkça belirtme; Tether Rush için somut öneriler.
- Google Play bağlantıları web aracıyla açıldı. Resmî CDN görselleri ve Apple sayfaları arka plandaki tarayıcıda ekran görüntüsüyle görsel olarak incelendi. Aşağıdaki görüntü kimlikleri, mağaza karuselindeki birden başlayan sıralamadır; mağaza ileride bunları değiştirebilir.
- Görseller yalnız inceleme içindir. Rakip ikon, yazı, arayüz resmi veya stok varlık oyun dosyalarına alınmadı. Terminal üzerinden geçici görsel indirme ağ kısıtı nedeniyle başarısız oldu; görsel inceleme tarayıcıyla tamamlandı.
- İndirme eşikleri başarı ölçeği seçimidir; aktif oyuncu, gelir, UI kalitesinin nedensel kanıtı veya güncel liste liderliği anlamına gelmez. Android ve iOS mağaza görselleri aynı sürümü göstermeyebilir.

## 1. Going Balls

[Google Play, Supersonic](https://play.google.com/store/apps/details?id=com.pronetis.ironball2&hl=en): 100M+ indirme. [App Store](https://apps.apple.com/us/app/going-balls/id1499081620) gerçek HUD için daha yararlı.

**Görsel kanıt:** Play #1, #2, #7 incelendi; altta büyük tanıtım bandı var, üst HUD yok. Bunlar buton referansı sayılmadı. Apple #1–3'te solda dairesel duraklatma; üst ortada ince beyaz ilerleme çubuğu/mavi dolum; altında küçük kalın beyaz bölüm numarası görülüyor. Sağda dikey küre göstergeleri ve ayrı pembe sayaç var. Orta ve alt alan parkura bırakılmış; sayılar kart tablosuna dönüşmüyor.

[Apple #1 doğrudan görsel](https://is1-ssl.mzstatic.com/image/thumb/Purple126/v4/f9/fd/d9/f9fdd992-705c-1558-d866-571043b1cf4d/7428f055-4da6-4c62-8753-9b63c0ee2f32_0_Gcity_1242x2688.png/230x498bb.webp).

**Sınır:** ana menü, sonuç ve ayarlar görünmedi; sağdaki kürelerin işlevi görselden doğrulanmadı. Kesin piksel dolgusu ölçülmedi.

## 2. Bridge Race

[Google Play, Supersonic](https://play.google.com/store/apps/details?id=com.Garawell.BridgeRace&hl=en): 500M+ indirme. [App Store](https://apps.apple.com/us/app/bridge-race/id1543845882).

**Görsel kanıt:** Play #1, #2, #9 ve Apple ilk iki tanıtım resmi incelendi. Mavi/yeşil karakterler, geniş açık yol, alt bölümde mavi damalı tanıtım bandı; beyaz kalın harflerin koyu konturu var. Tanıtım sahneleri top itme gösteriyor. Apple'ın ilk iki önizlemesinde renkli bloklar ve köprü kurma görülüyor; kullanılan karelerde HUD yok. Eylemin anlaşılması karakter, yol ve toplama renginden geliyor.

[Play #1 doğrudan görsel](https://play-lh.googleusercontent.com/zCx3IhkKwb9eXQ8zmr6DrrRwaBcy4Txb2a5GaztgfrX4bG-QzkpxtARPxI8WFX8PuLJkJV2TIwz8-6ed5fNGLLg=w600).

**Sınır:** tanıtım bandı oyun butonu kabul edilemez. Ana menü, gerçek ilerleme HUD'ı, sonuç, ayarlar veya dokunma alanı ölçüleri doğrulanmadı. Bu oyun HUD geometrisi için güçlü kaynak değildir.

## 3. Tall Man Run

[Google Play, Supersonic](https://play.google.com/store/apps/details?id=com.VectorUpGames.TallManRun&hl=en): 100M+ indirme. [App Store](https://apps.apple.com/us/app/tall-man-run/id1599121822) #1–4 gerçek oyun UI'ı gösteriyor.

**Görsel kanıt:** Apple görsellerinde bölüm numarası üst ortada; sağ üstte alt alta iki küçük açık renk sayaç ve uçlarında para/kristal simgesi var. Beyaz kalın yazı koyu kenarla okunuyor. Gövde metni ve geniş HUD panelleri bulunmuyor. Kapılardaki büyük artı/eksi sayıları çevrede yer alıyor; bilgi karar noktasında. Play #1, #2, #5 ise altta sloganlı tanıtım kompozisyonu; bunları gerçek HUD saymıyoruz.

[Apple #1 doğrudan görsel](https://is1-ssl.mzstatic.com/image/thumb/PurpleSource116/v4/81/dc/10/81dc1032-c198-9e4e-d73f-73c3fd6e7049/6eb28f42-541b-4902-b76a-f64e32da0494_01BE4AF4-DDA7-4651-A45F-B54DFF561844.png/230x498bb.webp).

**Sınır:** start, sonuç ve ayarlar gözlenmedi. Görselden ilerleme çubuğu veya ana CTA kuralı çıkarılamaz.

## 4. Aquapark.io

[Google Play, Voodoo](https://play.google.com/store/apps/details?id=com.cassette.aquapark&hl=en): 100M+ indirme. [App Store](https://apps.apple.com/us/app/aquapark-io/id1453989822).

**Görsel kanıt:** Play #1, #2, #7 ve Apple ilk üç resmi incelendi. İlk resimde üstte çok büyük sarı/mor sıralama işareti, ortada kaydırak, altta tanıtım sözcüğü var. İkinci resimde havadan parkurun tamamı okunuyor. Koleksiyon resmi üç sütunlu karakter ızgarası; seçili hücre renk ve tikle ayrılıyor. İnce açık çerçeveler karakterleri ayırıyor, uzun açıklama yok.

[Play #1 doğrudan görsel](https://play-lh.googleusercontent.com/vle5wlPRnFK5G70i0AY_OVrXx2_PT2UZZrnc5IYJUAqry0pKpAeQsvSo_BUJUwygsOF_hRt5sFHD9xzVhJo73w=w600).

**Sınır:** resimler reklam düzenlemesi içeriyor. Sıralama işaretinin güncel gerçek HUD boyutu, koleksiyon etkileşimi, ana menü, başarı/ölüm ve ayarlar doğrulanmadı.

## Karşılaştırmanın güvenilir sonucu

Gerçek oyun HUD'ı için en güçlü kanıt Going Balls ve Tall Man Run'ın Apple görselleri. Dört oyunun mağaza sunumunda da eylem ve hedef büyük yer kaplıyor; metinler kısa. Ancak mağaza tanıtımındaki iri sloganları oyuna taşımak bu araştırmanın ters yorumu olur. Dört oyunda ortak olduğu ileri sürülebilecek bir ana menü, retry, ayarlar veya sonuç ekranı kalıbı gözlenmedi. Bu ekranlara aşağıdaki yaklaşım Tether Rush için tasarım önerisidir.

### Ek gerçek HUD görselleri ve gözlenen bilgi bütçesi

- Going Balls [Apple #2](https://is1-ssl.mzstatic.com/image/thumb/Purple126/v4/22/63/1d/22631dcd-6fff-4e83-2351-4f2148952c04/ad26bc7f-c294-4d3a-ad57-eceb0066951e_1_Galien_1242x2688.png/230x498bb.webp), [Apple #3](https://is1-ssl.mzstatic.com/image/thumb/Purple126/v4/76/6d/3d/766d3d63-fa90-b3e3-c128-2cf54188ff53/2416fc58-8437-47d5-9ea1-37694a9b8b6b_2_Ghexagon_1242x2688.png/230x498bb.webp).
- Tall Man Run [Apple #2](https://is1-ssl.mzstatic.com/image/thumb/PurpleSource116/v4/62/5e/07/625e0799-7085-e63e-eb37-7c9cd0f525d8/098a7af1-d549-49fa-8fe4-0c13a97e293b_F0125877-93FF-435A-B4A4-AA7FA95687DA.png/230x498bb.webp), [Apple #3](https://is1-ssl.mzstatic.com/image/thumb/PurpleSource126/v4/56/aa/93/56aa931b-2304-67df-025f-4f45003c68bb/86ddf85a-f2e0-4833-9023-60ee8930e088_55018DF0-E765-4DD2-B0F2-45AEAF3064A3.png/230x498bb.webp), [Apple #4](https://is1-ssl.mzstatic.com/image/thumb/PurpleSource116/v4/c2/10/9a/c2109a2e-44e4-c0cc-09b5-ceae9d855cbd/db1937ae-5919-40bd-b41b-d37d825d94c1_21F807A8-410B-4D0A-B57D-D213AC6F4CD1.png/230x498bb.webp).

| Görünür HUD | Üst bant | Orta/alt bölge | Okuma önceliği |
| --- | --- | --- | --- |
| Going Balls Apple #1–3 | Bir duraklatma, bir ilerleme/bölüm grubu, sağ kenarda durum/sayaç | Parkur ve oyuncu; alt sabit açıklama paneli yok | Önce parkur, sonra ilerleme; yardımcı bilgi kenarda |
| Tall Man Run Apple #1–4 | Bir bölüm etiketi, iki küçük kaynak sayacı | Karakter ve kapılardaki yerel karar sayıları | Önce karakter/kapı, sonra bölüm/kaynak |
| Bridge Race / Aquapark incelenen kreatifleri | Gerçek HUD bütçesi güvenilir biçimde ölçülemez | Eylem sahnesi ve reklam bandı | Pazarlama hiyerarşisi; oyun içi buton/hud ölçüsü çıkarılmaz |

## Tether Rush'a uygulanabilir öneri

### Görsel dil

- Oyunun kendi tesisini ana yüzey yap: lacivert çevre, şeftali/mor küreler, camgöbeği bağ, mercan tehlike ve amber özel hedef. Her vurguya başka bir anlam verme; HUD'daki enerji de bağın rengiyle ilişkilensin.
- Koyu panel üstüne koyu panel yığma. Metin okunuyorsa doğrudan sahne üstünde göster; yalnız gerçek etkileşim veya veri grubu gerektiğinde yüzey kullan.
- Tek okunaklı, kalın ağırlığı olan sans ailesi; Türkçe karakter desteği zorunlu. Küçük ve geniş aralıklı başlıklar, sloganlar, ikincil açıklama paragrafları ve sahte teknik etiketleri kaldır.
- Gölgeler yerleşimi ayırsın; her kenara neon/parıltı ekleme. Koyu temada beyaz/kırık beyaz yazının kontrastını önce dene; gerekirse ince gölge ekle. Renk tek başına kilit/tamamlanma göstergesi olmasın.

### Ekranlar

| Ekran | Önerilen kompozisyon | Gerekçe / köken |
| --- | --- | --- |
| Ana menü | Tether Rush başlığı; tesis içinde görünen iki küre/bağ; güçlü tek `Leveller` eylemi, daha sessiz `Sonsuz Akış`, küçük ayarlar simgesi. Sonraki bölüm varsa ana eylemin yanında yalnız numarası. | Oyun kimliğini sahne taşır. Bu ekran doğrudan rakiplerde gözlenmedi; onaylı modlara göre öneridir. |
| 25 bölüm haritası | Dikey tesis rotası; büyük numaralı düğümler, sıradaki düğümde en güçlü vurgu; tamamlanana tik, kilitliye kilit ve daha sessiz renk. Üstte geri ve kısa başlık. | Yeni ekonomi eklemeden mevcut ilerlemeyi okunur yapar. Aquapark seçili hücresindeki renk+tik ayrımı yalnız durum kodlamasına örnek. |
| Bölüm HUD'ı | Sol üst duraklatma; üst merkez kısa bölüm numarası ve ince ilerleme; sağ üst kristal simgesi+sayı. Alt kenara yakın tek ince enerji göstergesi; tam enerjide kısa `HAZIR` durumu. | Üst merkez + açık oyun alanı Going Balls/Tall Man gözleminden; enerji Tether Rush mekaniğine özgü öneri. |
| Sonsuz HUD | Bölüm ilerlemesi yerine önde skor, küçük mesafe; kristal sayısı ve enerji aynı yerlerinde. | Aynı bilginin modlar arası konumu korunur. |
| Başarı | Sahne görünür; kısa başarı başlığı, bölüm numarası, kristal sonucu; birincil `Sonraki`, ikincil harita. Son bölümde uygun tamamlanma dönüşü. | Doğrulanmış rakip sonuç referansı yok; mevcut akış için öneri. |
| Başarısızlık | Kısa başlık; büyük `Tekrar Dene`, küçük harita/menü. Sonsuzda skor ve rekor. | Hızlı retry mevcut kapsamla uyumlu. Ölümü uzun açıklamayla bekletmez. |
| Ayarlar / pause | Tek yüzey; açıkça etiketli ses anahtarı ve ses düzeyi. Pause'da en güçlü `Devam`, sonra tekrar/çıkış. | Rakip ayar ekranı gözlenmedi; mevcut işlevlerden türetilmiş öneri. |

### İlk uygulama için ölçüler — rakiplerden ölçülmüş değerler değildir

540×960 tasarım referansında dış yatay boşluk 24, ana buton yüksekliği 64–72, ikonun görünür alanı 24–28 ve dokunma alanı en az 56 birimle başlanabilir. Ana buton etiketi 24–28, ekran başlığı 34–40, HUD sayısı 24–28, ikincil metin 18–20 aralığında denensin. Aynı rol aynı ölçüyü kullansın. Butonun iki yanında en az 20–24 birim iç boşluk bırak; küçük ikonları dış sınıra yapıştırma. Bunlar Unity referans birimleridir, Android dp kabul testi değildir.

540×960 ve 540×1170 oranında gerçek render ile kontrol: üstte safe-area payı; kürelerin hareket koridoru ve yaklaşan hedefler açık; en uzun Türkçe etiketler taşmıyor; sonuç CTA'ları aynı yatay hizada; ayarlarda her kontrolün etiketi görünür. Fiziksel telefonda okunabilirlik ve dokunma ölçüleri ayrıca doğrulanmalı.

## Uygulama kabul maddeleri ve açıklar

- Alt açıklama kartı ve yinelenen durum cümleleri oyun alanından kalkar; öğretici ipucu yalnız ilk ihtiyaç anında kısa gösterilir.
- Enerji tam dolmadan Pulse çağrısı yapılmaz; gerçek bırakma tek Pulse üretir. Enerji göstergesi ikinci bir düğme gibi görünmez.
- Aynı köşe yarıçapı, çizgi kalınlığı, font rolleri ve ikon yaklaşımı tüm ekranlarda kullanılır. Mağaza/SDK tarzı promosyon kutuları, sahte ödüller, rozet koleksiyonu ve anlamsız slogan eklenmez.
- Araştırma görselleri arayüz çizimi için referanstır; rakibin marka, varlık, tipografi düzeni veya buton görseli kopyalanmaz.
- Dört oyunun ana menü/sonuç/ayar ekranları bu kaynaklarla doğrulanamadı. Bu eksik, hayalî örneklerle doldurulmadı. Gerçek HUD için iki güçlü örnek bulundu; diğer iki oyun daha çok görsel odak ve reklam/HUD ayrımı sağladı.
- Bu görevde test veya Unity render çalıştırılmadı; araştırma ve kaynak görsel incelemesi tamamlandı. Ana yöneticiye teslim edilecek tek değişiklik bu belgedir.
