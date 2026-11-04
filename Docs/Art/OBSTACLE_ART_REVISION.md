# Engel sanat revizyonu — 04.11.2026

Kullanıcının önceki dört modeli fazla pembe, dikdörtgen ve kalabalık bulması üzerine görsel kaynaklar yeniden üretildi. Ana yöneticinin Duet, Smash Hit ve Stack Ball incelemesinden aktardığı çıkarım; büyük okunabilir siluet, sakin gövde ve sınırlı eylem işaretidir. Araştırma ve oyun mekaniği kararları ana yöneticiye aittir.

## Kapsam ve sonuç

Yalnız dört mevcut modelin Blender kaynağı ve kontrollü FBX export'u değişti. ClampBarrier tek geniş eğimli omuz; CapacitorBank tek eğimli monolit; HydraulicSlider tek yön çizgili temiz kapak; TurbineBeam kesintisiz alçak kiriş ve sade merkez göbeği oldu. Çene, üç kapasitör, piston ayrıntıları ve sekiz türbin kanadı kaldırıldı. Gövde graphite blue `(0.25, 0.32, 0.39)`, oyuklar `(0.06, 0.10, 0.15)`, ince işaretler desatüre mercan `(0.83, 0.40, 0.44)` kullanır. Unity runtime rengi ana yönetici tarafından uygulanır.

| Model | Üçgen | Mesh | Materyal |
| --- | ---: | ---: | --- |
| ClampBarrier | 116 | 1 | Body / Dark / Accent |
| CapacitorBank | 116 | 1 | Body / Dark / Accent |
| HydraulicSlider | 116 | 1 | Body / Dark / Accent |
| TurbineBeam | 196 | 1 | Body / Dark / Accent |
| Toplam | 544 | 4 | |

Önceki toplam 1732 üçgendi. Her model tek empty root, tek mesh child, merkez sıfır, root scale 1 ve tam normalize 1×1×1 sınır taşır. `.meta` dosyaları değiştirilmedi; eski 14 FBX SHA-256 ile korundu. Kaynak `.blend` Assets dışında tutulur.

## Temas sözleşmesi

Kapalı gövde yatay X/Y ayak izini tam olarak `-.5..+.5` doldurur; pahsız dikdörtgen bölüm Blender Z `-.5..+.14` boyunca korunur. Üstteki omuzlar yalnız +.14 üzerindedir. Ana yöneticinin planladığı dünya Y merkezi .45 ve yükseklik .9 ile tam temas bölümü dünya **Y=0..0.576**, toplam üst sınır .9 olur. Küre merkezi .55, normalize +.111111 düzeyindedir. FBX kendi başına Unity collider veya oyun fiziği içermez; gerçek X/Z temas hesabı ana yöneticiye aittir.

## Doğrulama

Blender **5.2.3 LTS** ile son kaynak ve dört FBX üretildi. Bağımsız import **4/4 PASS**: root/mesh sayısı, tam sınır/merkez, birim ölçek, üç semantik materyal, üçgen bütçesi ve armature/animasyon yokluğu doğrulandı. Her modelde yedi yükseklik × beş enine nokta × iki yatay yön = **70 ray** kontrol edildi. Örnek yükseklikler `-.49, -.10, 0, .10, .111111, .13, .14`; dört köşenin .0001 içini kapsayan sınır rayları pahsız ayak izinin -.5 kenarını buldu. Eski 14 FBX hash'i bağımsız doğrulamada da eşit kaldı. Bu örnekleme tüm geometrinin matematiksel hacim kanıtı değildir.

Gerçek önceki Unity karelerinin dördü incelendi. Son 1800×1600 2×2 Blender paftası model rollerinin dünya oranlarıyla açılıp görsel olarak incelendi. İlk yeni paftadaki rotor göbeği/çizgi eşdüzlem hatası, yalnız üst plato indirilip işaretler yüzeyden ayrılarak düzeltildi; son paftada temiz göbek ve görünür rotor/kızak çizgileri doğrulandı. Blender paftası Unity ışıklandırması veya gerçek oyun kamerası kanıtı değildir; son oyun kareleri ana yönetici doğrulamasını bekler.

## Dosyalar ve yeniden üretim

- Üretim: `Tools/Art/build_obstacle_variants.py`
- Bağımsız import: `Tools/Art/validate_obstacle_variants.py`
- Düzenlenebilir kaynak: `ArtSource/Blender/ObstacleVariants/ObstacleVariants.blend`
- Son pafta: `ArtSource/Blender/ObstacleVariants/preview.png`
- Export hash/ayarları: `ArtSource/Blender/ObstacleVariants/export_manifest.json`
- Bağımsız sonuç: `ArtSource/Blender/ObstacleVariants/fbx_validation.json`
- FBX: `Assets/TetherRush/Resources/ArtModels/` altında aynı dört kimlik.

Üretim komutu: `blender --background --python Tools/Art/build_obstacle_variants.py`; ardından `blender --background --python Tools/Art/validate_obstacle_variants.py`.

Bu alt görev Unity, Git, oyun kodu, ortak planlama belgeleri veya yeni mekanik modeli üretmedi. Dört FBX son doğrulamadan sonra donduruldu ve ana yöneticiye Unity import için bildirildi. `PROJECT_STATUS.md` güncellemesi ana yöneticiye bırakıldı. Fiziksel Android performansı bu sanat doğrulamasının kapsamında değildir.

Ana yönetici sonucu: son kaynaklarla231 test ve iki platform build geçti; gerçek oyun kareleri incelendi. Ayrıntı ve açık kabul [SYSTEMS_VALIDATION.md](../QA/SYSTEMS_VALIDATION.md) içinde.
