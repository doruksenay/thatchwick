# Unity bağlantısı

## 1. Proje iskeleti (bir kez, senin bilgisayarında)

1. Unity Hub > Installations: güncel Unity 6 LTS. Modüller: Android Build Support (OpenJDK, SDK, NDK dahil) ve WebGL Build Support.
2. Bu repoyu klonla: `git clone https://github.com/doruksenay/thatchwick` (Git LFS kurulu olmalı: `git lfs install`).
3. Unity Hub > New project > **2D (URP)** şablonu. Konum: klonladığın klasörün kendisi olmalı. Hub boş olmayan klasöre proje açmıyorsa projeyi başka yerde oluşturup `Assets`, `Packages`, `ProjectSettings` klasörlerini repo köküne taşı.
4. `git add -A && git commit && git push`. `.gitignore` Library, Temp, Logs gibi klasörleri zaten dışarıda bırakır.

## 2. Çekirdek kodu Unity'ye bağlama

Şu an çekirdek `core/Thatchwick.Core` içinde, dotnet ile test ediliyor. Unity projesi geldiğinde:

1. Kaynaklar `Assets/Scripts/Core/` altına taşınır ve içine `Thatchwick.Core.asmdef` konur (UnityEngine referansı yok, "No Engine References" açık).
2. `core/Thatchwick.Core/Thatchwick.Core.csproj` aynı dosyaları `Compile Include="../../Assets/Scripts/Core/**/*.cs"` ile okur. Böylece tek kaynak iki yerde derlenir: Unity ve `dotnet test`.
3. Bu taşıma ve `.meta` dosyaları Unity'de açıldıktan sonra yapılır; ben Unity'yi çalıştıramadığım için `.meta` dosyalarını elle üretmiyorum.

## 3. CI için Unity lisansı

`.github/workflows/unity-tests.yml` elle tetiklenir ve Unity projesi yoksa atlanır. Lisans bilgisi (UNITY_LICENSE, UNITY_EMAIL, UNITY_PASSWORD) repo Secrets'a eklenir. Güncel yöntem için GameCI'nin "activation" belgesine bakılır (yöntem zamanla değişiyor); adımları o zaman birlikte yaparız.
