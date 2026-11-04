# Etkileşimli sistemlerin çalışma zamanı doğrulaması

`InteractiveRuntimeTests` gerçek `TetherRushGame` ve URP dünyasını kullanır; PlayerPrefs yazmaz. Bütün ilerleme döngüleri sınırlıdır, girdi olayları SafeRoute üzerinden 1/120 saniyelik adımlarla uygulanır. Sonsuz parça değişiminde yeni parçanın cue sayacı sıfırlanır.

| Senaryo | Kabul ölçütü |
| --- | --- |
| Bölüm 6 rölesi | Gerçek geniş geçiş kapıyı açar; sıfır enerji/sıfır Pulse ile bölüm tamamlanır; bölüm hızı 7 m/s kalır. |
| Röleyi kaçırma / retry | Dar geçiş röleyi açmaz; dolu Pulse bağlantılı normal kapıyı açamaz; kapı teması ölümcüldür. Başarılı koşudan retry, röleyi ve efekti sıfırlar; kaçırılan röle tekrar ölümcüldür. |
| Röle düzleminde dar kalma | Pedden yarım metre önce açılmaya başlamak eşikte yeterli genişlik sağlamaz; ped geçildikten sonra tamamen açılmak röleyi geriye dönük tetiklemez. |
| Bölüm 8 hareketli Pulse kilidi | Gerçek beş enerji ve menzilde bırakma kilidi geri çeker; enerji tüketilir; bölüm tamamlanır. Pause mesafeyi ve kısa efekti dondurur; devam efekti bitirir; retry kilidi kapatır. |
| Kısmi enerji / erken bırakma | Ayrı iki test: kısmi enerji Pulse üretmez; erken dolu bırakma enerji tüketse de kilidi açmaz; ikisi de temasta ölür. Kısmi önkoşul gerçek enerji toplandıktan sonra yalnız Model.Release ile hedef eylemi çağrılmadan enerji boşaltılıp iki enerji verilerek izole edilir. |
| Normal hareketli engel | Yakın dolu Pulse normal duvarı açmaz; daralan küreler duvarda ölür. |
| Sonsuz Akış üst hızı | Teste özel 100 bonus enjeksiyonuyla hedef üst hıza çıkarılır; gerçek beş parçalık rota tamamlanır; röle/kilit parçaları geçilir; ivme/hız üst sınırı, efekt/collectible/dünya sınırları, pause ve retry doğrulanır. |

Gerçek URP kareleri `output/previews/Systems_Relay_Before.png`, `Systems_Relay_After.png`, `Systems_PulseLock_Before.png`, `Systems_PulseLock_After.png` ve `Systems_Endless_HighPace.png` olarak yazılır. İlk dört kare yeni sade sanatın ve mekanik durumun karşılaştırmasını sağlar. Sonsuz hız/süre/mesafe örnekleri `Systems_Pace_samples.json` içinde tutulur. Karelerin insan tarafından görsel incelemesi ayrıca gereklidir.

`ObstacleVariantTests` yeni dört engelin yalnız görsel yüksekliğini 0,9 m ve görsel merkezini 0,45 m bekler; XZ footprint, otoriter hareket, materyal, Collider yokluğu, normal engel ölümcüllüğü ve desteklenmeyen görsel rollerin korunması aynı test kapsamındadır. Fiziksel Course hitbox yüksekliği bu değişiklikle yeniden tanımlanmaz.

Bu dosya test planıdır; Unity çalıştırma veya geçmiş başarılı sonuç iddiası değildir. Ana yönetici entegrasyon tamamlandıktan sonra yeni testleri, mevcut 25 rota ve regresyon paketini, gerçek kareleri ve buildleri doğrular. Fiziksel Android oynanış/tepki süresi kabulü bu otomasyonla kanıtlanmaz.
