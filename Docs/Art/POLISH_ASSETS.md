# Görsel cila Blender varlıkları

31.10.2026 — D-021/022/023 kapsamında. Kaynak `ArtSource/Blender/TetherRushKit.blend`, tekrar üretim `Tools/Art/build_blender_kit.py`, kontrollü çıktı `Assets/TetherRush/Resources/ArtModels/*.fbx`.

11 mevcut model iyileştirildi; SplitPylon, ChevronBarrier ve RelayPylon eklendi. Tesis dili: pah kırılmış zırh kabukları, koyu girinti panelleri, sınırlı sinyal şeritleri, temiz lacivert pist. Çekirdekte koyu ekvator/meridyen ve küçük kutup lensleri var. Enerji toplanabilirinin soketi ve bonus kristalin fasetleri farklı siluetler sunar. Kapı menteşe plakaları ve kırılabilir hedefin dış zırh parçaları kendi animasyon çocuklarına katıldı.

| Model | Üçgen | Mesh |
| --- | ---: | ---: |
| Core | 920 | 1 |
| TrackTile | 444 | 2 |
| Barrier | 396 | 1 |
| MovingWall | 396 | 1 |
| RotatingArm | 352 | 1 |
| EnergyGate | 616 | 3 |
| BreakableBlock | 704 | 4 |
| EnergyPickup | 560 | 1 |
| BonusCrystal | 104 | 1 |
| PulseBadge | 472 | 1 |
| FinishMarker | 396 | 1 |
| SplitPylon | 484 | 1 |
| ChevronBarrier | 352 | 1 |
| RelayPylon | 352 | 1 |
| Toplam | 6548 | 20 |

Statik parçalar tek mesh içindeki semantik malzeme bölümlerine birleştirildi; her dekoratif parçaya ayrı Renderer gerekmez. Pist Deck ve TrackTrims olarak iki mesh tutar. Çoklu malzeme bölümleri yine ayrı draw call üretebilir; bu sayı bir performans ölçümü değildir.

## Entegrasyon sözleşmesi

- Her FBX tek isimli root, sıfır merkezli 1×1×1 tam sınır, birim root scale içerir. Blender Z dikey ekseni Unity Y olur. FBX forward -Z/up Y, statik ve animasyonsuzdur.
- Body, Dark, Accent malzeme adları korunur; gerçek oyun renkleri ve ışıldama çalışma anında VisualTheme/shader tarafından değiştirilir. Kaynakta bütün modeller ortak semantik malzemeleri paylaşır. Kontak paftası rol bazında renklendirilmiş görsel örnektir.
- EnergyGate tam olarak DoorLeft, DoorRight, Frame mesh çocuklarını; BreakableBlock tam olarak Shard1, Shard2, Shard3, Shard4 mesh çocuklarını korur.
- TrackTile ana yürünebilir mesh adı Deck. Normalize ana Deck üstü Unity Y=0.25757576, dekoratif ray üstü Y=0.5. Dünya Deck üstü = trackCenterY + trackHeight × 0.25757576. Gölge düzlemi bu değerin biraz üstüne yerleştirilmelidir. Tüm sınırın üstü yürünebilir yüzey değildir.
- SplitPylon mevcut 2.35×1.3×1 yan engel hitboxına; ChevronBarrier mevcut 2.5×1.3×1.2 merkez hitboxına ölçeklenir. Her ikisinin merkez yüksekliğindeki kütlesi kapalıdır; yüzey kanalları geçiş açıklığı değildir. Fizik kurallarını FBX değiştirmez.
- RelayPylon yalnızca çarpışmasız pist dışı dekor için üretilmiştir.

## Doğrulama ve sınır

Blender 5.2.3 LTS ile kaynak kaydı, 14 FBX export ve 1800×1800 PNG pafta üretimi tamamlandı. `preview.png` görsel olarak incelendi: 14 varlık, etiketler, kapalı engel yüzleri ve farklı toplanabilir siluetleri görünür. `fbx_validation.json` bağımsız yeniden içe aktarmada 14/14 PASS: sınırlar, merkez, root, üçgenler, materyaller, animasyonsuz yapı ve özel çocuk adları doğrulandı. Deck yüzey yüksekliği ayrıca doğrulandı.

Gerçek Unity aydınlatması/shader görünümü ve Android performansı bu alt görevde çalıştırılmadı; ana yönetici entegrasyon testlerini yürütür. Hiçbir Unity süreci, git işlemi veya .meta düzenlemesi yapılmadı. PROJECT_STATUS.md ana yöneticiye ait olduğundan bu teslim raporu oraya aktarılmak üzere iletildi.
