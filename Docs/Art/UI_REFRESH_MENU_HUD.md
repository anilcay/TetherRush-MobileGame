# Menü ve HUD yenilemesi

04.11.2026. Araştırma 02.11'de, uygulama ve doğrulama 04.11'de tamamlanır. Kaynak ayrımı ve önerilerin gerekçesi [araştırma sentezinde](../Research/UI_SYNTHESIS.md).

## Görsel uygulama

Menü tek satır Tether Rush başlığı, gerçek Blender Core/TrackTile modellerinin ayrı kamera önizlemesi, bölüm ilerlemesi ve tek baskın Leveller eylemi kullanır. Sonsuz Akış daha sessiz bir satır; Ayarlar sağ üstte özgün gear ikonu. Pazarlama sloganları ve iç içe kartlar kaldırıldı. Aynı düz yüzeyler Sonsuz Akış ve Ayarlar sayfalarında sürer.

Barlow SemiBold gövde ve Barlow Condensed ExtraBold başlıklar gerçek ağırlıklarıyla kullanılır. OFL ve kaynak bildirimi Assets/TetherRush/ThirdParty/Barlow altında. İkonlar özgün UiIcon mesh geometrisidir; başka oyunun görseli varlık olarak alınmadı. Yuvarlak ve 8px köşeli panel teknik sprite'ları yerel olarak rasterize edildi.

Oyun üst HUD'u 103px içinde: duraklatma, bölüm/ince progress veya Sonsuz skoru/rekoru, kristal sayısı, gerekiyorsa beş Pulse hücresi. Kalıcı alt açıklama ve büyük üst pano yok. İlk beş bölümde değişen kısa ipucu 2,2 saniye görünür; MENZİLDE uyarısı öğretici kapalı olsa da her bölümde görünür. Pulse düğmesi yok. Sonuçta tek anlamlı sayı ve tek baskın sonraki/tekrar/devam eylemi kullanılır. Harita/ana menü ikincil eylemdir.

Küçük simgelerin dokunma bölgeleri 72px, bölüm düğümleri 94px; 540px referans genişliğiyle düzenlenir. Safe area ve genişliğe göre ölçekleme korunur. Fiziksel Android dokunma/çentik kabulü bu raporla iddia edilmez.

MenuHero kendi 768×480 RenderTexture, kamera, malzemeler ve Blender örneklerine sahiptir. Kamera yalnız Home görünürken etkin; ayarlar/harita/oyunda kapanır. Kaynaklar OnDestroy'da bırakılır. Oynanış modeli, dünya paleti, engel hitboxları, 25 bölüm ve kayıt v2 değişmez.

## Doğrulama

Ana yöneticinin gerçek Unity render/test/build sonucu [UI_REFRESH_VALIDATION.md](../QA/UI_REFRESH_VALIDATION.md) içinde kaydedilir. Kaynak incelemesi tek başına görsel kabul sayılmaz.
