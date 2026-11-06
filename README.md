# Thatchwick: Generations of a Village

Orta çağ köyünde geçen, canlı ve gerçek zamanlı, Sims benzeri bir yaşam simülasyonu. Oyuncu bir hanedanı nesiller boyu yaşatır. Motor: Unity (2D, URP). Hedef: önce Android, sonra iOS.

Durum: Faz 1, Adım 01 (proje iskeleti). Yol haritası ve tasarım dokümanları Claude Project'inde tutulur.

## Klasör yapısı

```
core/                      dotnet test altyapısı (Unity'siz)
  Thatchwick.Core/        csproj; kaynaklar Assets/Scripts/Core altında
  Thatchwick.Core.Tests/   xUnit testleri (dotnet test ile saniyeler içinde)
Assets/                    Unity projesi (Unity Hub ile açılır; bkz. aşağısı)
.github/workflows/         CI
```

Kural: oyun mantığı `core/` içinde yazılır, Unity tarafı yalnızca görüntü ve girdi katmanıdır. Böylece testler Unity'siz hızlı çalışır.

## Çekirdek testlerini çalıştırma

```
dotnet test core/Thatchwick.Core.Tests
```

Gereken: .NET SDK 8.

## Unity projesini açma

Unity Hub > New project > 2D (URP) şablonu ile bu klasörde proje oluşturulur (ya da boş proje oluşturulup buraya taşınır). `core/Thatchwick.Core` kaynakları Unity'ye `Assets/Scripts/Core` altına bağlanır (ayrıntı: `docs/unity-baglanti.md`).

## Zaman modeli (ilk kodlanan kural seti)

1 oyun günü = 6 gerçek dakika (1x), mevsim = 10 gün, yıl = 40 gün. Hız 1x/2x/3x. Biyoloji oyun saatinde işler.
