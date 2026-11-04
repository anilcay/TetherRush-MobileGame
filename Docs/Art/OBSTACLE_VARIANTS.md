# Engel görsel varyantları

04.11.2026 — Mevcut tesis dili içinde dört Blender görseli. Araştırma sentezi `Docs/Research/OBSTACLE_VARIANTS.md` ana yöneticiye aittir. Bu teslim yeni oynanış veya hareket sistemi tanımlamaz.

| Model | Görsel kütle | Üçgen | Mesh / materyal |
| --- | --- | ---: | --- |
| ClampBarrier | Kapalı taban üzerinde çift çene, koyu kilit rayı ve geniş kırmızı çene kapakları | 348 | 1 / 3 |
| CapacitorBank | Kapalı soket üzerinde üç dik kapasitör, terminal ve bara | 624 | 1 / 3 |
| HydraulicSlider | Kapalı taban üzerinde tek tabla, yatay piston gövdeleri ve kılavuzlar | 424 | 1 / 3 |
| TurbineBeam | Kesintisiz dikdörtgen kiriş üzerinde türbin göbeği ve sekiz yüzey kanadı | 336 | 1 / 3 |
| Toplam | | 1732 | 4 / 12 slot |

## Kaynak ve yeniden üretim

- Düzenlenebilir kaynak: `ArtSource/Blender/ObstacleVariants/ObstacleVariants.blend`. Unity Assets altında `.blend` bulunmaz.
- Üretim: `Tools/Art/build_obstacle_variants.py`; yalnızca dört yeni FBX'i `Assets/TetherRush/Resources/ArtModels/` altına yazar.
- Manifest ve SHA-256: `ArtSource/Blender/ObstacleVariants/export_manifest.json`.
- Bağımsız FBX içe aktarma: `Tools/Art/validate_obstacle_variants.py`; raporu aynı kaynak klasöründe `fbx_validation.json`.
- Görsel pafta: aynı kaynak klasöründe `preview.png`, 1800×1600 PNG.

Blender komutu: `blender --background --python Tools/Art/build_obstacle_variants.py`; ardından `blender --background --python Tools/Art/validate_obstacle_variants.py`. Betikler fabrika sahnesiyle başlar; eski kit betiğini çalıştırmaz. Kaynakta parçalar semantik materyalleri paylaşan tek mesh hâline birleştirilir; mesh Edit Mode ile düzenlenebilir. FBX profilinde forward -Z, up Y, birim ölçek, animasyon kapalıdır.

## Entegrasyon sözleşmesi

Her dosyada model kimliğiyle aynı adlı tek empty root ve tek `<Kimlik>Mesh` çocuk bulunur. Merkez 0,0,0; root scale 1,1,1; tam sınır 1×1×1. Blender Z dikeyi Unity Y olur. Malzeme slotları tam olarak `Body`, `Dark`, `Accent`. Runtime VisualLibrary gövde ve kırmızı tehlike renklerini VisualTheme materyallerinden uygular. Üç materyal tek Renderer içinde üç draw call üretebilir; bu teslim performans ölçümü değildir.

Tüm dört modelde X/Y=-0.5..+0.5 yatay ayak izini kesintisiz dolduran, pahsız, katı temas gövdesi Blender Z=-0.5..+0.16 boyunca vardır. Örnek olarak dünya merkezi 0.65 ve yüksekliği 1.3 uygulamasında tam gövde dünya Y=0..0.858 aralığını doldurur; küre merkezi 0.55..0.65 bu katı bant içindedir. Rotor için örnek merkez 0.55/yükseklik 0.7 uygulamasında tam gövde Y=0.2..0.662 aralığındadır. Gerçek yerleşimde aynı temas bantlarını entegrasyon sorumlusu tekrar denetlemelidir.

Üst yüzeydeki çene arası, kapasitör arası ve türbin kanatları arasındaki alanlar geçiş açıklığı değildir; altta tam kapalı kütle bulunur. TurbineBeam boş X/çapraz değildir; bütün dikdörtgen kiriş mevcut dönen engelin root hareketini izler. HydraulicSlider pistonları görsel yüzey detayıdır; bağımsız düşey pres veya FBX animasyonu yoktur. ClampBarrier çeneleri de bağımsız kapanmaz. Collider, simülasyon ve mevcut Pulse ayrımı ana yönetici entegrasyonuna aittir.

## Tamamlanan doğrulama ve sınırlar

Blender **5.2.3 LTS** ile kaynak kaydı, dört FBX ve render tamamlandı. Bağımsız içe aktarmada **4/4 PASS**: tek root/mesh, birim root scale, sıfır merkez, tam birim sınır, en fazla 1000 üçgen, tam üç semantik materyal ve animasyon/armature yokluğu doğrulandı. Beş yükseklik × beş enine örnek × iki yatay yönde her model için **50 temas kenarı rayı**, dört köşenin .0001 içini de kapsayarak katı gövdenin -.5 dış sınırına temas etti. Bu örnekleme tüm mesh için matematiksel hacim kanıtı değildir; üretimde tam kapalı cube doğrudan yer alır. Eski 14 FBX'in SHA-256 değerleri üretim öncesi/sonrası ve bağımsız doğrulamada eşit kaldı.

Pafta görsel olarak açılıp incelendi. Dört modelin etiketleri, katı ön yüzleri, ayrı üst kütleleri ve kırmızı bantları görünür. İlk paftada üst kapakların eş düzlemli yüzleri görüldü; bunlar geometrik yüzey ayrımıyla düzeltildi ve son pafta yeniden incelendi.

Bu görev Unity, Git, `.meta` düzenlemesi veya `PROJECT_STATUS.md` güncellemesi çalıştırmadı. Unity import, gerçek URP görünümü, parkur entegrasyonu, oynanış ve Android performansı ana yönetici doğrulamasını bekler. Dosya sahipliği: iki yeni üretim/doğrulama betiği, ObstacleVariants kaynak klasörü, dört yeni FBX ve bu doküman. Yönetici raporuna aktarılmak üzere bu teslim iletildi.

Ana yönetici entegrasyon sonucu: dört model gerçek bölümlerde/Sonsuz Akış'ta, 191 test ve Windows/Android build ile doğrulandı. Güncel gerçek Unity kareleri ve sınırlar [doğrulama kaydında](../QA/OBSTACLE_VARIANTS_VALIDATION.md).
