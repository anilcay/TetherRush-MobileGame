# M2 görsel / ses geri bildirimi teslimi

Yönetici güncellemesi: entegrasyon, shader importu, 60 Unity testi, dört gerçek kamera görüntüsü ve Windows M2 build geçti. Güncel sonuçlar [M2 doğrulama kaydında](../QA/M2_VALIDATION.md); aşağıdaki metin ajanın ilk teslim ve entegrasyon notlarını korur. Kapı mesh kopyalarının tüm alt materyalleri desteklenir; native MaterialPropertyBlock Initialize içinde oluşturulur.

31.10.2026. Kapsam: `GameFeedback.cs`, iki yerel URP shader ve bu entegrasyon notu. Oyun modeli, girdiler, fizik kökleri, hedef etkinliği ve seviye içeriği değiştirilmedi. Bağımlılık: mevcut runtime asmdef'in Core ve UnityEngine erişimi; URP 17.3 `Core.hlsl`; manager tarafından üretilen görsel child mesh'ler.

## Entegrasyon

- `TetherRush.Presentation.GameFeedback` manager GameObject'ine eklenir. Dünya kurulduktan sonra bir kez `Initialize(leftCore, rightCore, tether, camera, tetherMaterial, tuning.PulseRadius)` çağrılır. `tetherMaterial` API uyumu için alınır; shared materyal hiçbir zaman değiştirilmez.
- Kamera üzerinde **bir** AudioListener manager tarafından eklenmelidir. Initialize kamera üzerindeki mevcut AudioSource'u kullanır; yoksa tek kaynak ekler. Ses klipleri bir kez oluşturulur. Kısa cue'lar birbirinin yerini alır; aynı karede birden çok toplama ses yığını yaratmaz. `AudioEnabled` bool property HUD aç/kapat düğmesine bağlanabilir; varsayılan açık.
- Manager kamera temel pozunu ve core/tether kök pozlarını güncelledikten sonra `Tick(dt, held, ready, phase)` çağırır. Paused sırasında `dt = 0`; ayrıca phase koruması vardır. Ready ve terminal durumlarda pozitif dt ile efektler yürür; bu dt model simülasyonuna gönderilmez. GameFeedback'in kendi Update'i yoktur.
- Başlatma / yeniden denemede `ResetPresentation()` çağrılır. Yalnız başarılı model toplamasında `EnergyCollected(position)` veya `BonusCollected(position)`; yalnız başarılı gerçek Pulse olayında `Pulse(center)` çağrılır. Hazır olma geçişinin sesi Tick içinde türetilir.
- Yalnız modelin gerçekten açtığı hedef için, görsel kapatılmadan **önce** `TargetOpened(view, isGate)` çağrılır. `Failed()` ve `Completed()` phase geçişinde bir kez çağrılır. Hiçbir API model durumunu değiştirmez.
- Eski manager Pulse LineRenderer kaldırılmalı ya da kapalı tutulmalı; iki halka beraber çizilmemeli. Yeni halka tam `pulseRadius` değerine ömrünün %65'inde ulaşır ve kalan sürede menzilde sönümlenir. Genişleyen görsel halkada oynanış teması hesaplanmaz.
- `Tether Rush/NeonSurface` ve `Tether Rush/NeonEffect` shader'larını Always Included listesine ekleyin. Principal mesh materyalleri NeonSurface kullanabilir; `_BaseColor`, `_EmissionColor` desteklenir. Efekt havuzu kendi ayrı transparan materyalini kullanır.
- Yeni Presentation / Shaders klasörlerinin `.meta` dosyalarını ana yönetici üretsin (klasör `.meta` dosyaları ajan dosya sahipliğinin üst dizinindedir). Kaynak dosya `.meta` dosyaları teslimde mevcut.

## Görsel davranış ve sınırlar

- Teal enerji, altın bonus/hedef, teal-altın hazır bağı; child core mesh dönüşü. Hazır titreşim yalnız emisyon shimmer'ıdır: tether uzunluğu, kök pozu, simetri ve collider değişmez. Core'un yalnız kendi root renderer'ı varsa dönüş uygulanmaz.
- Enerji için dörder küçük çizgi bağı takip ederek merkeze akar; bonus için beş küçük çizgi. Kamera sallanmaz; ekran kaplayan efekt yoktur.
- Gate renderer adlarında `DoorLeft`/`DoorRight` ayrı yönlere kayar; `Frame` sabit gerçek modelde kalır ve geçici kopyaya alınmaz. Block shard renderer'ları kısa dışa kayarak söner. Her açılışta en çok 10 renderer kopyalanır; mesh/collider/GameObject hiyerarşisi kopyalanmaz. Mesh yoksa küçük altın halka kullanılır.
- Sabit havuz: 28 çizgi parçası, 4 halka, 24 hedef mesh kopyası. Olaylarda Instantiate/primitive üretimi yoktur. Aktif havuz doluysa eski slot yeniden kullanılır. Havuzlar ve sahibi olunan materyal/sesler OnDestroy'da temizlenir.
- Sesler procedural sinüs harmonikleriyle bir kez oluşturulur; her örnek teorik olarak 0.30 altında, AudioSource hacmi 0.55. Bir kaynak/tek çalan klip sınırı vardır. Harici ses dosyası veya lisans bağımlılığı yoktur.
- NeonSurface tek pass, opak, UnityPerMaterial CBUFFER içeren sabit yumuşak yön ışığı shader'ıdır; gerçek ışık/gölge gerektirmez. Bevel normalleri derinlik verir. NeonEffect tek pass alpha blend / ZWrite Off'tur. MPB shared materyallere yazmayı önler; MPB kullanan renderer'ların SRP batching avantajı azalabilir (bu az sayıdaki geri bildirim renderer'ı için bilinçli sınır).

## Doğrulama ve açık kabul

Kod statik olarak incelendi: RunModel çağrısı/collider oluşturma yok, core root'larına yazma yok, efekt ömrü Tick dt ile ilerler, shader import yolu yerel URP paketine gider. Bu ajan Unity'yi çalıştırmadı; derleme, shader importu, Blender mesh vertex rengi/şeffaflığı, ses duyumu ve gerçek Unity görüntüsü **doğrulanmadı**. Yönetici entegrasyon sonrası C# + shader derleme, tam menzil halkası, pause freeze, retry temizliği, gate kanatları, fail/success ve HUD ses düğmesini Editor'da doğrulamalıdır. Fiziksel Android performans/ses kabulü ayrıca açıktır.

Ana yönetici PROJECT_STATUS.md'ye bu kapsamı, çalıştırılmayan görsel/ses doğrulamasını ve entegrasyon sonucunu raporlamalıdır.
