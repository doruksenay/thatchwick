# Thatchwick: Generations of a Village

Orta çağ köyünde geçen, canlı ve gerçek zamanlı, Sims benzeri bir yaşam simülasyonu. Oyuncu bir hanedanı nesiller boyu yaşatır. Motor: Unity (2D, URP). Hedef: önce Android, sonra iOS.

Durum: oynanabilir prototip (ihtiyaçlar ve işler, yapı modu, çiftçilik ve hayvanlar, aile ve hanedan, köy, katlar, yerel kayıt). Yol haritası ve tasarım dokümanları Claude Project'inde tutulur.

## Klasör yapısı

```
core/                      dotnet test altyapısı (Unity'siz)
  Thatchwick.Core/        csproj; kaynaklar Assets/Scripts/Core altında
  Thatchwick.Core.Tests/   xUnit testleri (dotnet test ile saniyeler içinde)
Assets/                    Unity projesi (Unity Hub ile açılır; bkz. aşağısı)
  Scripts/Core/            oyun mantığı (UnityEngine yok; hem Unity hem dotnet test derler)
  Scripts/Game/            Unity katmanı: görüntü, girdi, arayüz, kayıt dosyası
  Resources/Content/       veri odaklı içerik (items.json, life.json, appearance.json)
.github/workflows/         CI
```

Kural: oyun mantığı `Assets/Scripts/Core` içinde yazılır (UnityEngine'e bağımlı değildir), Unity tarafı (`Assets/Scripts/Game`) yalnızca görüntü ve girdi katmanıdır. `core/` klasöründe kaynak yoktur; aynı dosyaları Unity'siz derleyip sınayan csproj ve testler vardır. Böylece testler Unity'siz hızlı çalışır.

## Çekirdek testlerini çalıştırma

```
dotnet test core/Thatchwick.Core.Tests
```

Gereken: .NET SDK 8.

## Unity projesini açma

Unity Hub > Add ile bu klasör açılır (Unity 6, 2D URP; sürüm `ProjectSettings/ProjectVersion.txt` içinde). Ayrıntı: `docs/unity-baglanti.md`.

## Kayıt

Tek yerel kayıt `Application.persistentDataPath/thatchwick_save.json` dosyasıdır. Yazma önce `.tmp` dosyasına yapılır, bir önceki kayıt `.bak` olarak kalır; ana dosya okunamazsa yedekten yüklenir (`Core/Save/SaveFiles.cs`).

## Zaman modeli (ilk kodlanan kural seti)

1 oyun günü = 6 gerçek dakika (1x), mevsim = 10 gün, yıl = 40 gün. Hız 1x/2x/3x. Biyoloji oyun saatinde işler.
