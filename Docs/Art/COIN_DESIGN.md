# Coin — bonus toplama görseli

Kullanıcının kristal modelini ve sağ üst kristal simgesini değiştirme isteği için özgün Blender üretimi. Yeni ekonomi, ödül miktarı veya toplama kuralı tanımlamaz. Runtime/HUD entegrasyonu ana yöneticiye aittir.

## Tasarım

Kalın, yuvarlatılmış altın çerçeve; içeri oturan koyu lacivert yüz; kısa bir bağla birleşen iki yuvarlak çekirdek kabartması. Şekil oyunun iki küresine gönderme yapar. 24 segment çevre ve geniş bevel yüzeyleri kullanılır; küçük yazı, sık çizgi veya doku yoktur. Cyan enerji toplamasından dairesel altın siluetle ayrılır; amber Pulse hedefinin mekanik anlamı değişmez.

Üç materyal rolü tam olarak `Body`, `Dark`, `Accent` adlarını taşır. Blender önizlemesinde Body altın, Dark lacivert, Accent açık altındır. Unity `VisualLibrary` rolleri kendi materyalleriyle değiştirdiğinden gerçek oyun paleti ve görüntüsü ana yöneticinin entegrasyonunda doğrulanmalıdır. HUD PNG renderı kendi rengini taşır; beyaz Image tint ile kullanılmalıdır.

## Dosyalar ve sözleşme

- `ArtSource/Blender/Coin/Coin.blend`: Blender 5.2.3 LTS düzenlenebilir kaynak. Coin/CoinMesh dışa aktarılacak varlıktır. Gizli `IconRenderOnly`, aynı geometrinin eğilmemiş HUD render kopyasıdır; FBX'e girmez. Kamera/ışıklar da FBX'e girmez.
- `Assets/TetherRush/Resources/ArtModels/Coin.fbx`: tek root `Coin`, tek `CoinMesh`, 624 üçgen, üç materyal rolü; armature, animasyon ve collider yok.
- `ArtSource/Blender/Coin/CoinIcon.png`: aynı geometriyle Cycles 32 sample, 256×256 RGBA, şeffaf arka plan, önden ortografik HUD referansı. Ana yönetici bunu Resources UI klasörüne taşıyıp Sprite importunu ayarlar.
- `CoinWorld.png`, `CoinAngle.png`, `CoinEdge.png`: gerçek renderlar; sırasıyla runtime en/boy/derinlik oranı, eğik görünüm ve kenar görünümü.
- `CoinSheet.png` / `CoinIcon_QA22.png`: koyu zemin üzerinde gerçek 22/24/32/48px teknik boyut incelemesi ve 22px thumbnail. Piksel QA'da altın daire ve iki bağlı çekirdek okunur.
- `export_manifest.json`, `fbx_validation.json`: geometri, SHA ve bağımsız yeniden içe aktarma kanıtları.

Blender yüz düzlemi XZ, ön normal -Y. Standart `-Z forward / Y up` FBX eksen dönüşümünden sonra Unity yüz düzlemi XY, ön normal -Z olur. Kaynakta 25° yaw pişirilmiş ve her eksen ayrı normalize edilmiştir: kaynak tam boyut **1×1×1**, merkez **0,0,0**, root ölçek **1,1,1**. Bu kasıtlı kaynak derinliğidir; kaynak küp oranı gerçek coin kalınlığı değildir. `VisualLibrary` normalizasyonu sonrası Unity görsel boyutu **(.5,.5,.18)** kullanıldığında kenar kalınlığı ve eğim doğru okunur. Bu boyutta bağımsız üçgen normal ölçümü ön yüz yaw açısını **17,22°** bulur; Unity normal yaklaşık **(.296,0,-.955)**. Modeli tekrar döndürmek gerekmez.

## Üretim ve doğrulama

`Tools/Art/build_coin.py` Blender background Python ile kaynak, FBX ve dört renderı yeniden üretir. `Tools/Art/validate_coin.py` kaynağı açmadan FBX'i sıfır sahneye yeniden içe aktarır.

Bağımsız kontrol PASS: tek root/mesh, 624 üçgen (≤650), tam materyal adları, merkez ve 1³ bound, scale1, kameraya bakan glyph, runtime eğim, animasyon/armature olmaması ve önceki **18 FBX'in SHA256 ile değişmeden korunması**. Coin FBX SHA da manifest ile aynıdır. HUD PNG ve 22px QA görsel olarak incelendi. Unity ve telefon görünümü bu alt görevde çalıştırılmadı; gerçek renderer paleti/mesafe ve HUD importunu ana yönetici doğrular.

Üretimde harici görsel, yapay zekâ bitmapi veya üçüncü taraf model kullanılmadı. Tüm model ve kabartma proje için kodla Blender'da oluşturuldu.
