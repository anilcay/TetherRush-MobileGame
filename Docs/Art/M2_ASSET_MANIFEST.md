# M2 Blender varlık teslimi

Yönetici güncellemesi: 11 modelin Unity import/bounds kontrolü geçti ve oyun içinde render edildi. [Gerçek Unity kareleri](Previews/M2_Foundation.png) ve [doğrulama kaydı](../QA/M2_VALIDATION.md) ilk Blender teslim notlarını tamamlar.

31.10.2026 — D-018/020 kapsamında gerçek Blender üretimi tamamlandı. Blender CLI tarafından bildirilen sürüm **5.2.3 LTS**, hash `e13b728c6880` (2026-10-22). Bu kayıt Unity import kabulü veya fiziksel Android performans ölçümü değildir.

## Dosyalar ve tekrar üretim

- Düzenlenebilir kaynak: `ArtSource/Blender/TetherRushKit.blend` — 146.510 bayt. Her varlık ayrı collection ve merkez root empty; kaynakta Core dışındaki collection'lar viewport'ta gizlidir. Görmek/düzenlemek için ilgili collection'ın gizliliğini kaldırın. Preview sahnesi ve kamera kaynağa eklenmedi.
- Üretim: `Tools/Art/build_blender_kit.py`; Blender `--background --python Tools/Art/build_blender_kit.py` ile kaynak + 11 FBX + sayısal manifest + kontakt sheet yeniden üretilir. Blender API dışında modelleme kütüphanesi veya ücretli araç yoktur.
- Yalnız önizleme: `Tools/Art/render_blender_preview.py`; kaynağı açar, export dosyalarına dokunmadan kontakt sheet üretir.
- Bağımsız yeniden import: `Tools/Art/validate_blender_exports.py`; Blender factory-empty sahneye her FBX'i ayrı yükler, ölçü/merkez/root/üçgen/materyal/static kontrollerini yapar.
- Sayısal kanıtlar: `ArtSource/Blender/export_manifest.json`, `ArtSource/Blender/fbx_validation.json`.
- Gerçek Workbench render: `ArtSource/Blender/preview.png` (1800×1500). Rol renkleri ve okunabilir sunum oranları yalnız kontakt sheet kopyalarında uygulanır; kaynak semantiği korunur.

## Export sözleşmesi

Tüm modeller toplam **1×1×1** bounds'a normalize edildi; root pivot ve bounds merkezi `(0,0,0)`, source root scale `(1,1,1)`. İnce parça istisnası yoktur: TrackTile ve PulseBadge dahil Unity tarafında istenen gerçek X/Y/Z boyuna ölçeklenmelidir. Kaynak Blender koordinatları X sağ, Z yukarı; geometrinin panel ön yüzü çoğunlukla -Y. Normalize öncesi biçim oranları korunmuş dünya ölçüsü olarak değerlendirilmemelidir.

FBX profilinde `axis_forward=-Z`, `axis_up=Y`, `global_scale=1`, `apply_unit_scale=True`, `apply_scale_options=FBX_SCALE_UNITS`, `use_space_transform=True`, `bake_space_transform=False` kullanıldı. `axis_forward`/`axis_up` enum değerleri çalışan Blender export RNA API üzerinden kontrol edildi. Yalnız seçili `MESH`/`EMPTY`, animasyon bake kapalı, kamera/ışık/rig/collider yok, scale negatif değil. Blender metre/unit scale 1; FBX standardı içindeki birim dönüşümü ihracatçı tarafından yazılır. Unity +Y yukarı / +Z ilerleme ve mesh yüz yönü kabulünü yönetici Unity'de doğrulamalıdır; Blender yeniden import testi bunun yerine geçmez.

Toplam 11 FBX **349.572 bayt**, toplam birer örnek **3.000 üçgen**. Geometri triangulate edilip statik olarak teslim edildi. `.blend` Assets altında değildir; `.meta` dosyalarını Unity oluşturur. Git LFS kurulmadı.

| Varlık | Üçgen | Mesh child | FBX bayt | Ayırt edici geometri / kullanım |
| --- | ---: | ---: | ---: | --- |
| Core | 480 | 3 | 34.428 | Yumuşak düşük poligon kabuk, gömme ekvator ve meridyen bantları; toplam yarıçap 0,5'i aşmaz. A/B aynı modelin Body rol rengiyle ayrılır. |
| TrackTile | 220 | 5 | 33.692 | Koyu deck, yükseltilmiş kenar rayları ve iki panel çizgisi; Unity'de ince ve uzun zemin boyutuna ölçeklenir. |
| Barrier | 176 | 4 | 29.916 | Pahlı tam gövde, iki koyu zırh paneli ve yatay uyarı çizgisi. |
| MovingWall | 220 | 5 | 34.348 | Pahlı kayar panel, koyu iç yüz ve üç eğik hareket çizgisi. |
| RotatingArm | 176 | 4 | 29.932 | Merkez gerçek dönme pivotu; enine süpürücü, uç korumaları ve merkez aks. Hareket koddan. |
| EnergyGate | 440 | 3 | 33.996 | Tam kapalı panel: `DoorLeft`, `DoorRight`, `Frame`; her kanadın inset/enerji çizgisi kendi mesh'ine birleştirilmiştir. |
| BreakableBlock | 528 | 4 | 39.052 | `Shard1`–`Shard4`; dar birleşim oluklarıyla kapalı blok. Her shard kendi koyu panelini ve dört parçalı Pulse işaretinin bir çeyreğini içerir. |
| EnergyPickup | 172 | 2 | 24.012 | Açık dairesel enerji yuvası ve merkez yük hücresi; bağ toplaması için bonus kristalden farklı silüet. |
| BonusCrystal | 64 | 2 | 21.356 | Keskin fasetli kristal ve koyu kuşak. |
| PulseBadge | 216 | 3 | 27.884 | Dairesel hedef yuvası ve iki eğik vurgu çubuğu. |
| FinishMarker | 308 | 7 | 40.956 | İki dikme ve dört işaretli üst kiriş; iç geçiş açık. |

## Materyal ve hareket entegrasyonu

Yalnız üç paylaşılan semantik ad: `Body` oyun rol rengi, `Dark` koyu iç/panel, `Accent` açık çizgi/vurgu. Bir mesh en fazla üç slot kullanır; tekstür/atlas yoktur. Kaynak Body turkuaz bir çalışma rengidir. Unity GDD rollerini Body'ye bağlar: turkuaz/mor çekirdekler, kırmızı tehlikeler, altın Pulse hedefleri; mevcut rol materyallerinin kullanımı beklenir. Kesin sayısal palet veya yeni tema kararı alınmadı. Unity shader'ının Blender görünümünü aynen taşıdığı varsayılmadı.

Child mesh vertex verisi ortak root merkezinde normalize edilmiştir; child transform scale 1, rotation 0 ve location 0'dır. Kapı açmak için `DoorLeft`/`DoorRight` yerel konumları karşı yönlere kaydırılır. Shard savurma yönleri gerekirse renderer bounds merkezlerinden hesaplanır; child origin'leri ayrı fizik merkezi değildir. Rig, humanoid ve gömülü animasyon yoktur. Collider ve Pulse/ölüm yetkisi oyun koduna aittir.

## Gerçekleşen doğrulama ve kalan kabul

- CLI üretim exit 0; 11 FBX export başarıyla yazıldı, `.blend` kaydedildi, Workbench kontakt sheet render edildi ve görsel olarak incelendi. İlk arka yüz sunumu düzeltildi; nihai ön yüzler panel/trim dilini gösteriyor.
- Kaynak bounds ölçüleri gerçek vertex/world bounds üzerinden hesaplanıp 1e-5 toleransla kontrol edildi. Core 480, basit öğeler 64–308, gate 440 ve block 528 üçgen; verilen bütçeler içinde.
- Bağımsız FBX yeniden import **11/11 PASS**: toplam bounds her eksende 1 (1e-4 tolerans), bounds/pivot merkezi 0, tek doğru root, beklenen üçgen sayısı, izinli materyal adları, rig ve action yok. FBX dosya sürümü 7400 gözlendi.
- Sandbox nedeniyle Blender extension cache'i kullanıcı AppData'ya yazılamadı uyarısı verdi; export/import ve render tamamlandı. `Material.use_nodes` Blender 6.0 için deprecation uyarısı verdi, mevcut 5.2 üretimini engellemedi.
- Kalan: yönetici Unity import yön/ölçü/materyal-slot eşlemesi, görsel-collider hizası, küçük ekran oynanış kamerası ve sanat kabulü. Fiziksel cihaz FPS/dokunma testi D-017 kapsamında ertelenmiştir.

`PROJECT_STATUS.md` sahipliği yöneticidedir; bu teslimin durum/kanıt/engelleri ana yöneticiye gönderildi, dosya bu ajan tarafından değiştirilmedi.
