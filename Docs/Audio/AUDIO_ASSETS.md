# Tether Flow — özgün müzik ve ses varlıkları

04.11.2026. Bu teslim yalnız ses varlıkları, tekrar üretim araçları ve bu belgeyi kapsar. Unity oynatma/mixer, ayarlar, import `.meta` dosyaları ve oyun içi kabul ana yönetici tarafından birleştirilir. Başka ses kütüphanesi, sample, vokal, sanatçı taklidi veya ücretli servis kullanılmadı; bütün dalga şekilleri özgün çevrimdışı sentezdir.

## Müzik

`Assets/TetherRush/Resources/Audio/TetherFlow.ogg`: 32 ölçü, 4/4, 104 BPM, 73,846146 saniye, stereo 48 kHz Vorbis kalite 0,4. D minör merkezinde iki ölçülük Dm9 → Bbmaj7 → Fadd9 → Csus2; sekiz ölçülük soru/cevap mallet melodisi, üçüncü bölümde seyrek oktav cevabı, sıcak yumuşak pad, senkoplu bas, yuvarlak kick, kuru rim ve hafif kaydırılmış shaker. Tesiste akış hissi ve SFX için boşluk hedeflenir. Giriş/çıkış bölümü yoktur; delay ve reverb yansımaları modüler indeksle başa sarılır, böylece son ölçünün kuyruğu ilk ölçüde devam eder.

Kaynaklar:

- `AudioSource/TetherFlow.score.json`: düzenlenebilir tempo/akor/melodi/düzenleme reçetesi.
- `Tools/Audio/render_audio.py`: sabit rastgelelik tohumu, enstrümanlar, SFX, miks, PCM/Vorbis dışa aktarma ve sayısal QA.
- `AudioSource/TetherFlow.master.wav`: 16 bit stereo 48 kHz PCM master; yaklaşık 14,18 MB, Unity `Assets` dışında.
- `AudioSource/TetherFlow.gameplay-preview.wav`: 14 saniye müzik ve temsili UI/enerji/coin/ready/Pulse/röle/başarı miks önizlemesi; gerçek bir Unity kaydı değildir. Runtime başlangıç dengesiyle müzik 0,50 × master 0,55; coinler 0,90 × 0,55; diğer SFX 0,80 × 0,55 çarpılır. Önizleme sonradan yükseltilmez/normalize edilmez.
- `AudioSource/audio-validation.json`: makinece üretilen ses ölçümleri.

## Oyun sesleri

Hepsi `Assets/TetherRush/Resources/Audio/` altında 16 bit mono 44,1 kHz WAV. Kaynak tepe seviyeleri bilinçli olarak -6…-11 dBFS aralığında bırakıldı; çoklu sesler ve runtime ses çarpanı için pay vardır. İlk 2 ms ve son 12 ms sessizdir; iç zarf yumuşak başlar ve biter.

| Dosya / Resources anahtarı | Süre | Tasarım |
|---|---:|---|
| `Energy.wav` / `Audio/Energy` | 0,254 sn | FM renkli, yumuşak elektrik toplama; coinlerden ayrı tını |
| `Coin1.wav` / `Audio/Coin1` | 0,404 sn | D6–A6 iki notalı sıcak metal/mallet |
| `Coin2.wav` / `Audio/Coin2` | 0,404 sn | F6–C7 çeşitlemesi |
| `Coin3.wav` / `Audio/Coin3` | 0,404 sn | A6–D7 çeşitlemesi |
| `Ready.wav` / `Audio/Ready` | 0,584 sn | D5–A5–D6 ölçülü yükseliş |
| `Pulse.wav` / `Audio/Pulse` | 0,504 sn | Kısa yuvarlak düşük vurgu ve hafif üst ışıltı |
| `Relay.wav` / `Audio/Relay` | 0,474 sn | D4–A4–D5 bağlantı cevabı; enerji sesiyle ayrışır |
| `Fail.wav` / `Audio/Fail` | 0,604 sn | D4–A3–D3 yumuşak iniş; sert buzzer yok |
| `Success.wav` / `Audio/Success` | 0,924 sn | D5–F5–A5, son notada hafif D6 eşlik |
| `UiClick.wav` / `Audio/UiClick` | 0,099 sn | Alçak seviyeli kısa filtreli tap |

## Doğrulama ve yeniden üretim

Python 3 + NumPy yeterlidir. FFmpeg, SciPy, SoundFile ve dış indirme kullanılmaz. Vorbis, kurulu Unity Editor'ün `Editor/Data/Tools/FSBTool/x64` dizinindeki Xiph `libvorbis64.dll`, `libvorbisfile64.dll`, `libogg64.dll` API'leri ile kodlanır ve yeniden çözülür. DLL'ler bu depoya kopyalanmaz. Farklı Editor kurulumu için `--codec-dir` verilebilir.

Teslim NumPy 2.3.5 ile üretildi (`Tools/Audio/requirements.txt`). Tam sentez ve mevcut master üzerinden tekrar export karşılaştırmasında müzik OGG'si ile gürültü katmanlı UiClick WAV'ının SHA-256 değerleri birebir eşleşti; kayıt `AudioSource/reproducibility.json` dosyasındadır.

```powershell
& 'C:/Users/anilc/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe' Tools/Audio/render_audio.py
# Akor veya enstrüman değişmediyse mevcut PCM master üzerinden tekrar export/QA:
& 'C:/Users/anilc/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe' Tools/Audio/render_audio.py --reuse-master
```

Üretimde master -20,000 LUFS, çözülmüş OGG yaklaşık -19,988 LUFS; sample peak -7,638 dBFS ölçüldü. LUFS hesabı 48 kHz BS.1770 K ağırlık katsayılarını periyodik FFT filtrelemesiyle uygular; 400 ms/100 ms bloklar ve mutlak/bağıl kapılar kullanılır. Bu araç sample peak ölçer; true-peak ölçümü veya bağımsız sertifikalı loudness aracı yerine geçtiği iddia edilmez. Master ve çözülmüş OGG aynı 3.544.615 stereo frame içerir. OGG'nin son/ilk örnek adımı yaklaşık 0,00275 FS (-51,2 dBFS); döngü sessizliğe fade edilmez. DC, RMS, sample peak, finite değerler, clipping ve her SFX'in uç sessizliği raporlanır. Bütün varlıklarda clipping ve NaN/Inf sıfırdır. Runtime sesleri toplam yaklaşık 1,085 MB.

Sayısal QA duyumsal kabul değildir. Bu alt görevde insan dinlemesi, Unity AudioSource/loop sınırı dinlemesi, Android hoparlör ve kulaklık testi yapılmadı. Önizleme ve master, kullanıcı/ana yönetici dinlemesi için teslim edilir. Unity'de sık tekrarlanan coin/enerji üst üste binmeleri, pause/retry ve müzik/SFX ses dengesi ayrıca doğrulanmalıdır.
