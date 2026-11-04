# Engel çeşitleri — kısa araştırma ve üretim seçimi

04.11.2026. Kullanıcı yeni engel modelleri araştırılıp tasarlanarak oyuna eklenmesini istedi. Araştırma model üretiminden önce yapıldı; mevcut hareket/çarpışma ve tek kontrol kuralları temel alındı.

## Referans ve uygunluk

- [Going Balls resmî mağazası](https://play.google.com/store/apps/details?id=com.pronetis.ironball2) engel çeşitleri arasında sallanan çekiçler, rampalar, büyük sarkaçlar ve boşlukları sayıyor. Bu liste yayıncının açıklamasıdır; bütün engellerin oynanarak test edildiği anlamına gelmez. Bizim çıkarımımız: farklı kütle ve mekanik siluetleri aynı parkurda görsel çeşitlilik sağlar. Yönlendirme, zıplama, düşme veya hız değiştirme isteyen rampalar/boşluklar Tether'ın sabit merkezli tut/bırak kontrolüne uygun değil.
- [Supersonic'in Tall Man Run vaka incelemesi](https://supersonic.com/learn/case-studies/tall-man-run/) 13.06.2022 tarihli geliştirme yazısında dönen platform, bariyer ve tuzak çeşitlerinin eklenmesini anlatır. Bu tarihsel bir üretim örneğidir; tek başına belirli bir engelin başarıya neden olduğunu kanıtlamaz. Çıkarım: mevcut merkez/yan bariyer ve zamanlama ailesine farklı okunabilir mekanik kabuklar eklemek uygundur.

Bu küçük araştırma yeni fizik sistemi gerektirmeyen dört üretim fikrine indirgenir. Başka oyunların modelleri/görselleri kopyalanmaz; özgün Blender kaynakları üretilir.

| Yeni model | Okunan siluet | Oyundaki rol |
| --- | --- | --- |
| ClampBarrier | Zırhlı kenet ve kalın çene bloğu | Dar geçidin mevcut yan engeli; bırakıp daralma. |
| CapacitorBank | Dolu gövde üzerinde belirgin kapasitör/soğutma podları | Mevcut merkez bariyeri; basılı tutup iki yandan geçme. |
| HydraulicSlider | Piston ve dolu itici plaka | Mevcut yatay kayan duvar; aynı konum/faz/zaman. Dikey ezici davranışı yok. |
| TurbineBeam | Kesintisiz geniş kiriş ve türbin göbeği/kanatçıkları | Mevcut dönen kol; aynı OBB ve dönüş. Boş X biçimli yanlış geçiş alanı yok. |

Tesisin mevcut lacivert/koyu metal ve mercan ölümcül sinyal dili korunur. Yeni normal engeller Pulse ile açılmaz. Amber, yalnız mevcut işaretli Pulse hedeflerine aittir. Küre/bağ renkleri değişmez.

## Üretim sınırı ve kabul

Her model Blender'da, Assets dışında düzenlenebilir kaynakla; normalize merkezli FBX, Body/Dark/Accent yuvaları, tek statik mesh ve sınırlı üçgen bütçesiyle üretilecek. Dolu tehlike gövdesi otoriter dikdörtgen temas hacmiyle tutarlı kalmalı. Kozmetik parçalar yeni collider üretmez. Dönen/kayan modellerin hareketini mevcut simülasyon kökleri sürer; bağımsız rastgele animasyon eklenmez.

Harita, 25 bölüm, SafeRoute girdileri, süreler, enerji ve kayıt şeması korunur. Seçili normal engeller görsel olarak çeşitlendirilir; yeni mekaniğe ihtiyaç duyulursa ayrıca kullanıcı kararı gerekir. Kabul bağımsız FBX reimport, Unity model/rota/temas kontrolleri, gerçek oyun kareleri ve Windows/Android build sonucudur. Blender paftası tek başına oyun entegrasyonu sayılmaz.
