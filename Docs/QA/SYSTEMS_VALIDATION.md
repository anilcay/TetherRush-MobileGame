# Sade sanat, etkileşimler ve sonsuz tempo — 0.7.0 (8)

04.11.2026. D-035–038: kullanıcı görsel revizyonu ve skorla hızlanmayı istedi; bağ rölesi ve Pulse kilidini birlikte onayladı. [Araştırma](../Research/INTERACTIVE_REDESIGN.md), [sanat](../Art/OBSTACLE_ART_REVISION.md), [etkileşimli içerik](../Design/INTERACTIVE_COURSE.md), [hız ayarı](../Design/ENDLESS_PACE.md).

## Uygulama

- Dört model sade grafit omuz/kama/kızak/kiriş olarak yeniden üretildi; büyük mercan yüzeyler ve küçük makine ayrıntıları kaldırıldı. Görsel yükseklik 1,3→0,9 m; X/Z temas boyutları korunur. Yeni dört model toplam544 üçgen; eski14 ile kit7092 üçgen/24mesh. Aktif bölüm/sonsuz rotalarında aynı sakin kit kullanılır.
- Bağ rölesi, ped düzleminden yarı açıklık en az1,5m ile geçilince kendi kapısını açar. Enerji/Pulse tüketmez; eksik ped Pulse ile telafi edilemez. İnce zemin bağlantısı, kısa cyan sinyal, sakin geri çekilme ve tek ses onayı vardır. Bölüm6/12 ve sonsuz kalıp3.
- Amber hareketli Pulse kilidi bütün koridoru kapatır. Beş enerji, gerçek bırakma ve5m menzil koşulu ile güvenli yana çekilir. Normal hareketli engeller Pulse almaz. Bölüm8/16/20/25 ve sonsuz kalıp4. Bölüm25'in üç ayrı Pulse dizisi korunur.
- Sonsuz tempo ilk500 skorda7m/sn, 500–5500 arası yumuşak hedefle11,5m/sn tavana çıkar; ivme en fazla0,25m/sn². Bonus anlık hız sıçraması yapmaz. Bölümlerde7m/sn sabittir; pause dondurur, retry7'ye döner. Mesafe/7 fazı, engel görünümü ve sürekli OBB temasında aynı kullanılır.
- Yeni görsel efektler8 ile sınırlı; görsel kopyalar collider/script/ses kopyalamaz, source pozunu değiştirmez ve pause/retry/disable'da temizlenir. Kayıt şeması/sıralı açılma/25 bölüm kimliği ve ilk5 bölümün mekanik içeriği korunur. Önceki yerel rekor silinmez.

## Son yürütüm

| Kontrol | Durum | Kanıt |
| --- | --- | --- |
| Blender reimport | GEÇTİ | 4/4; model başına70 ray, gerçek küre merkezi normalize+.111111 dahil. Eski14FBX SHA-256 aynı. |
| Unity Prepare | GEÇTİ | exit0; son FBX, yeni Core/Runtime ve testler derlendi. |
| EditMode | GEÇTİ | 120/120; yeni24 tempo/hız/clock senaryosu ve96 regresyon. |
| PlayMode | GEÇTİ | Son kaynaklarla111/111; toplam231/231. Yeni etkileşim/kaynak sınırı/üst hızlı5chunk ve mevcut25 bölüm dahil. Results/Systems_test_results.json. |
| Gerçek URP | İNCELENDİ | 9 gerçek540×960 kare: röle/kilit öncesi-sonrası, yüksek tempo ve4 sade engel. Docs/Art/Previews/Systems_*.png. |
| Windows strict build | GEÇTİ | exit0; output/windows-systems/TetherRush.exe; rapor164.151.552 bayt. |
| Android strict build / imza | GEÇTİ | exit0; APK 38,157,366 bayt. com.anilcay.tetherrush,0.7.0 (8), ARM64/API25/36/portrait; v2=true. |

## Kabul sınırı

Hız eğrisi ilk dengelemedir; insan refleks/zorluk ve görsel beğeni kabulü kullanıcı incelemesinde. Üst hız testi skor önkoşulunu bonus enjeksiyonuyla kurar, ardından gerçek parkur girdileri ve fizik ile ilerler; gerçek oyuncu ekonomisi/başarı oranı ölçümü değildir. Fiziksel Android performans/dokunma/çentik, ses duyumu ve kapat-aç kabulü D-017 gereği sonda. Testler kullanıcı PlayerPrefs kaydını yazmaz veya silmez.

İlk111 PlayMode çalışmasında110 geçti; tek hata, önceki görsel stream kontrolünün açılan Pulse kilidinin gizlenen otoriter kaynak mesh'ini görünür aramasıydı. Kaynak chunk emekliliğine kadar korunur, yeni retract kopyası ayrı yaşar. Test kaynak instance'ını inactive dahil arayarak otoriter poz/ölçek doğrulamasını korur; gizlenme/açılma/retry ayrıca yeni testte sınanır. İkinci ve ped görünürlüğü sonrası üçüncü111/111 geçti. Son görsel inceleme, açık kilit yönergesini açık durumuna ve röle ped rengini cyan'a düzeltti; sonrası kareler geri çekilme tamamlanınca alınır.

Son açık-kilit mesajı ve ped okunurluğuyla111/111 yeniden geçti. Yüksek skor önkoşullu gerçek5chunk koşusunda hız18s civarında11,5m/sn tavana geldi, sonraki örneklerde sabit kaldı. Ham örnekler Results/Systems_Pace_samples.json; ilk500 skor alışma/bonus sıçraması eğrisi ayrıca safCore testlerinde doğrulandı.

APK SHA-256: `2db3abfa4c7380d89dfb3c50f604252da50de8ab36e569820af8880bb81d8694`. Asset/meta/GUID/JSON/doküman bağlantıları ve son staged whitespace kontrolü geçti. Build/cache ve dış referans görseller Git dışında tutulur.
