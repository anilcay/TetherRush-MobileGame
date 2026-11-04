# Relay ve PulseLock görsel geri bildirimi

04.11.2026. Kapsam ve dosya sahipliği: yalnız `MechanicFeedback.cs` ve bu belge. Yeni relay kapısı ve işaretli hareketli PulseLock için sunum katmanı; Core, parkur, fizik, materyal varlıkları, mevcut GameFeedback, Unity meta dosyaları ve ortak proje kayıtları ana yöneticiye aittir. Bağımlılıklar: mevcut Runtime assembly, `VisualTheme.Tether`, `Tether Rush/NeonEffect` shader ve manager tarafından güncellenmiş gerçek görsel pozlar.

## Entegrasyon sözleşmesi

- `TetherRush.Presentation.MechanicFeedback` dünya kurulduktan sonra `Initialize()` alır. Native Material yalnız burada oluşturulur; shader mevcut build dahil etme listesinde kalmalıdır.
- Başarılı relay olayı `RelayTriggered(Vector3 pad, Vector3 gate)` çağırır. Dünya koordinatları kullanılır. Tek ince camgöbeği çizgi parçası 0,35 saniyede pad'den kapıya ilerler; genişlik 0,032 m, başlangıç alfa 0,55. Kamera, ses, parçacık ve tam ekran etkisi yoktur.
- Açılan relay kapısı veya PulseLock için manager, orijinal görseli gizlemeden önce `Retract(Transform source)` çağırır. Kaynak bu anda gerçek hareket/dönüş pozunda olmalıdır. Fizik açılışı manager/model tarafından hemen uygulanır; bu görsel beklenmez.
- Retract yalnız etkin MeshRenderer + MeshFilter çiftlerini kopyalar; `sharedMesh` ve `sharedMaterials` referansları korunur. Component/script/collider/audio ya da kaynak hiyerarşisi Instantiate edilmez. Mesh kopyaları dünya position/rotation/lossyScale değerleriyle, döndürülmemiş ve ölçeği 1 olan bağımsız efekt köküne yerleştirilir. FBX alt meshlerinin farklı dönüş ve ölçekleri korunur. Nonuniform ölçek ile dönüşten doğan gerçek shear Unity TRS ile birebir temsil edilemez; sözleşme position/rotation/lossyScale yakalamasıdır.
- Kaynak merkezinin X işareti yönü belirler (0 için sağ): kopya kökü 0,18 saniyede yumuşak smoothstep ile dünya X = ±8 m konumuna ve 0,12 m aşağıya kayar; ardından tamamen kaldırılır. Kopya kaynak hareketini takip etmez. Kaynak hiçbir zaman değiştirilmez; manager kaynak görünmezken yetkili hareketini sürdürebilir. Opak meshler için sahte alfa materyali üretilmez.
- Her Refresh'te `Tick(paused ? 0 : dt)` çağrılır. Kendi Update, coroutine veya unscaledTime kullanımı yoktur. Sıfır/negatif/NaN/sonsuz dt hiçbir ilerleme üretmez.
- Retry/dünya yenilemesinde `ResetPresentation()` çağrılır. Disable/Destroy bütün kopyaları ve sahip olunan tek çizgi materyalini temizler; tekrar enable materyali yeniden Initialize eder. `ActiveEffectCount` aktif relay + retract toplamıdır.

## Sınırlar ve kabul

Toplam en fazla 8 etkin efekt tutulur; dokuzuncu en eskiyi hemen gizleyip Destroy eder. Oluşturma yalnız olay anındadır; Tick içinde managed dizi/LINQ/materyal tahsisi yoktur. Her retract kaynak alt mesh sayısı kadar mesh görünümü oluşturur; toplam mesh bütçesi kullanılan Blender modelinin renderer sayısına bağlıdır. Meshler ve paylaşılan materyaller yok edilmez/değiştirilmez. Destroy ertelendiği için yaşam süresi biten veya retry ile kaldırılan kökler hemen pasif yapılır.

Statik inceleme: kaynak transformuna yazma, kaynak Instantiate, collider/script kopyalama, materyal asset değişimi, zaman API erişimi ve frame başına managed allocation yok. Unity bu alt görevde başlatılmadı; derleme ve gerçek render **bekliyor**. Ana yönetici pause sırasında poz/çizgi donması, retry'da sıfır kopya, 9 olayda en fazla 8 aktif efekt, farklı dönüş/ölçekli FBX snapshot pozu ve ±8 m yönünü Unity testleriyle doğrulamalı; relay sinyali ve kapının okunabilirliği gerçek dikey kamera karelerinde incelenmeli. Telefon performansı ayrıca bekler. PROJECT_STATUS.md raporunu ana yönetici günceller.
