# Sade engeller ve yeni etkileşimler

04.11.2026. Kullanıcı önceki dört engelin görünümünü beğenmedi; göz yormayan revizyon, kapı kırma benzeri yeni mekanikler ve Sonsuz Akış'ta skora bağlı hız artışı istedi. Araştırma üretim yönü seçilmeden önce resmî kaynaklarla yapıldı. Bağ rölesi ve Pulse kilidi ayrıca kullanıcı tarafından birlikte onaylandı.

## Referanslar ve çıkarım

- [Duet resmî sitesi](https://www.duetgame.com/) / [App Store](https://apps.apple.com/us/app/duet-game/id634235735): iki eşzamanlı nesne ve sade engel kompozisyonu. Resmî sitedeki update2_1 GIF'inin 20/150 karesi doğrudan incelendi: siyah zemin, düz açık renkli engel yüzleri, iki renkli oyuncu ve küçük izler. Çıkarım: engelin işlevini siluet ve kontrastla anlat; her yüzeye dekor ekleme. Duet'in dönme kontrolü oyuna taşınmaz.
- [Smash Hit resmî mağazası](https://play.google.com/store/apps/details?id=com.mediocre.smashhit): zamanlama ve kırılabilir yüzeylere odaklanıyor. Resmî ekran görüntüsü incelendi: büyük tek cam yüzeyi, malzemeyle ayrılan hedef ve kırılma sonucu. Çıkarım: etkileşimin sonucu görünür olmalı; parçacık yoğunluğu kopyalanmayacak. Tether'da tek sakin geri çekilme ve kısa sinyal yeterli.
- [Stack Ball resmî mağazası](https://play.google.com/store/apps/details?id=com.azurgames.stackball): açıklama normal tehlike ile özel kırma durumunu ayırıyor. Çıkarım: mercan normal tehlike, amber Pulse hedefi, cyan bağ rölesi olarak farklı etkileşimleri ayır. Oyunun kontrolü veya ateş topu kuralı alınmaz.

Oyunlar kurulup oynanmadı; mağaza metni ile doğrudan incelenen görsel ayrıdır. Referans görseller yalnız geçici araştırma klasöründe tutulur, kaynak depoya ve oyun varlığına alınmaz. Tasarım önerileri başarı/retention garantisi değildir.

## Önceki görünümün teşhisi

Gerçek Obstacles_*.png karelerinde büyük mercan üst kapaklar ve yüksek kesintisiz blok yüzleri baskındı. Küçük mekanik ayrıntılar normal kamerada okunmuyordu. Yeni yön, grafit gövde, yumuşak üst profil, tek küçük uyarı şeridi ve 0,9 m toplam görsel yüksekliktir. Değişmeyen X/Z temas hacminin küre merkez yüksekliğinde dolu kalması zorunludur; yuvarlatma, görünmez köşe çarpışması üretemez.

## Onaylı mekanikler

**Bağ rölesi:** geniş bağın iki iletkeni kapsayarak ped düzleminden geçmesi, 14 m ilerideki bağlantılı kapıyı açar. Enerji tüketmez, Pulse ile açılmaz. Zemindeki ince hat ve cyan sinyal sebep/sonuç ilişkisini gösterir. İlk öğretim Bölüm 6, tekrar Bölüm 12 ve sonsuz kalıp 3.

**Pulse kilidi:** amber işaretli kayan kiriş koridoru kapatır. Beş enerjiyle menzil içi gerçek bırakma, tek Pulse ile kilidi güvenli yana çeker. Kısmi veya erken Pulse etkisizdir; normal hareketli engeller etkilenmez. İlk öğretim Bölüm 8, tekrar 16/20/25 ve sonsuz kalıp 4. Yeni düğme yoktur.

**Sonsuz hız:** kullanıcı isteği, GDD'deki sabit hız kuralını yalnız Sonsuz Akış için değiştirir. Bölümler 7 m/sn kalır. İlk ayar 500 skordan sonra yumuşak hedef eğrisiyle 7→11,5 m/sn; 5500 skorda tavan ve 0,25 m/sn² ivme sınırı. Bonus hız sıçraması yaratmaz. Duraklama dondurur, tekrar 7'ye döner. Hareketli engellerin uzamsal fazı distance/7 ile korunur; yeni hızda engellerin de zamanlaması hızlanır ve mevcut güvenli geçiş ritmi bozulmaz.

Bu sayılar ilk dengeleme ayarıdır. Kabul; gerçek 25 bölüm ve yüksek hızlı sonsuz rotalar, eksik röle/enerji ve erken bırakma hataları, pause/retry/temizleme, gerçek URP kareleri ve Windows/Android build ile yapılır. Telefon testi kullanıcı kararıyla son aşamada kalır.
