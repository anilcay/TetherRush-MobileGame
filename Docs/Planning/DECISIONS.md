# Karar kaydı

Tarih: 31.10.2026. `ONAYLI` kullanıcı isteğini/onayını, `ÖNERİ` henüz onaylanmamış seçimi, `SONRA` gelecekte ele alınacak konuyu ifade eder.

| Kimlik | Durum | Karar / öneri | Dayanak |
| --- | --- | --- | --- |
| D-001 | ONAYLI | Oyun Unity ile geliştirilecek. | Kullanıcının ilk isteği. |
| D-002 | ONAYLI | Ana sohbet yönetim merkezi; önemli kararlar burada tartışılıp onaylanacak. | Kullanıcının ilk isteği. |
| D-003 | ONAYLI | Alt görevler uzman ajanlara; model GPT-6.1 Sol, reasoning medium. | Kullanıcının ilk isteği. |
| D-004 | ONAYLI | 3D tasarım/modelleme Blender ile; gerektiğinde animasyon görevi ayrılacak. | Kullanıcının ilk isteği. |
| D-005 | ONAYLI | Çalışmalar belirtilen GitHub deposuna normal commit/push ile senkronize edilecek. | Kullanıcının ilk isteği. |
| D-006 | ONAYLI | İlk hedef Android telefon, dikey ekran. | Kullanıcının platform sorusuna yanıtı. |
| D-007 | TASARIM TEMELİ | GDD v2.0 oyun kuralları; Pulse ve çarpışma ayrımları korunacak. Yeni öneriler ayrı etiketlenecek. | Sağlanan 8 sayfalık belge. |
| D-008 | ÖNERİ | Yerelde kurulu Unity 6000.3.19f1 ve URP ile başlamak; sürümü ProjectVersion.txt ve paket kilidiyle sabitlemek. | .19f1 kurulumu ve AndroidPlayer klasörü doğrulandı; henüz proje oluşturulmadı. |
| D-009 | ÖNERİ | M1: tek kısa prototip bölümünde dar geçit, orta bariyer, bağla enerji, küreyle bonus, Pulse kapısı, işaretli kırılabilir hedef ve bitiş/retry. | Ana mekaniği en küçük uçtan uca akışta değerlendirmek. |
| D-010 | ÖNERİ | M1 geçici geometrilerle; oynanış onayından sonra Blender varlıkları, animasyon/VFX ve hareketli engeller. | Sanat işini oynanış ölçülerine göre üretmek. |
| D-011 | ÖNERİ | İlk görsel hedef GDD'deki koyu parkur, turkuaz/mor çekirdekler ve ölçülü neon. Sayısal palet ve üretim bütçeleri sanat incelemesinde kesinleşecek. | GDD sayfa 7 görseli ve açıklaması. |
| D-012 | SONRA | Test telefonu/modeli, asgari Android sürümü, kesin kare hızı ve performans bütçesi. | Fiziksel cihaz bilgisi henüz verilmedi; 60 FPS yalnızca önerilen hedeftir. |
| D-013 | SONRA | Reklam/IAP, hesap, analitik, mağaza yayını, iOS ve çevrimiçi servisler. | GDD bu özellikleri istemiyor; ilk prototip kapsamına eklenmedi. |

## M1 onay paketi

Öneri: D-008, D-009 ve D-010 birlikte ilk üretim kapsamını oluşturur. İlk teslim Android dikey ekranda çalışan bir test sürümü ve aynı akışı Editor'da çalıştırma talimatı olacaktır. APK üretimi ve cihaz testi yapılmadan bu teslim tamamlandı sayılmaz.

Süre, küre mesafeleri, ileri hız, Pulse yarıçapı ve enerji miktarları GDD'de sayısal olarak sabit değil. Bunlar ayarlanabilir prototip parametreleri olacak; test sonucu ile kesinleşecek. Rutin parametre ayarları ana yönetici tarafından yürütülür; ana mekaniği veya deneyimi değiştiren seçimler kullanıcıya sunulur. Alt planlardaki sayısal ayar onayları her sabit için ayrı kullanıcı izni anlamına gelmez.

## Tasarım incelemesinde çözülecek ayrıntılar

- Zorunlu Pulse kapısında erken enerji harcayan oyuncu nasıl yeniden dener? Enerjisiz kapıda süresiz bekleme olmamalı.
- Aynı fizik adımında enerji dolması ve bırakma gerçekleşirse değerlendirme sırası ne olmalı?
- Pulse hedefleri menzile hangi ölçütle girer? Kod ve görsel halka aynı ölçütü kullanmalı.
- Uygulama odağı kaybolması veya duraklatma, oyuncu bırakışı sayılmamalı; geri dönüşte giriş durumu temizlenmeli.

Teknik ayrıntılar için teknik plan ve kabul senaryoları; oyun davranışını değiştiren seçimler için ana sohbette karar kaydı kullanılır.
