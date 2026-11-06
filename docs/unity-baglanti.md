# Unity bağlantısı

Durum: Unity 6 (6000.3.23f1) 2D (URP) projesi repo kökünde (`Assets`, `Packages`, `ProjectSettings`).

## Çekirdek kod: tek kaynak, iki derleyici

Saf C# mantık `Assets/Scripts/Core/` altındadır ve `Thatchwick.Core.asmdef` ile (UnityEngine referansı yok) Unity'de derlenir.
`core/Thatchwick.Core/Thatchwick.Core.csproj` aynı dosyaları `Compile Include="..\..\Assets\Scripts\Core\**\*.cs"` ile okur, böylece `dotnet test core/Thatchwick.Core.Tests` Unity'siz çalışır.

Kural: UnityEngine kullanan kod `Assets/Scripts/Core` dışında (ör. `Assets/Scripts/Game`) yazılır ve Core'a referans verir; tersi olmaz.

## .meta dosyaları

Unity, yeni klasör ve dosyalar için `.meta` dosyalarını editör açılınca üretir. Ben Unity'yi çalıştıramadığım için `.meta` yazmıyorum: ben dosya ekleyince Unity'de açıp `.meta` dosyalarını commit etmen gerekir (`git add -A`).

## CI için Unity lisansı

`.github/workflows/unity-tests.yml` elle tetiklenir. Lisans bilgisi (UNITY_LICENSE, UNITY_EMAIL, UNITY_PASSWORD) repo Secrets'a eklenir. Güncel yöntem için GameCI'nin "activation" belgesine bakılır (yöntem zamanla değişiyor); adımları o zaman birlikte yaparız.
