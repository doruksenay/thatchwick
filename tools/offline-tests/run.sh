#!/usr/bin/env bash
# Çekirdek testlerini NuGet olmadan çalıştırır (bulut oturumları). CI'daki `dotnet test` ile aynı test dosyaları,
# aynı xunit aşırı yükleme kuralları (XunitShim.cs). Kullanım: tools/offline-tests/run.sh [SınıfAdıParçası]
# Game katmanı derleme denetimi de koşar (CI'daki ikinci adım).
# Sonda yeni sabit Türkçe metin denetimi çalışır (CI'da PR adımı): dalın origin/main'e göre eklediği metinler.
set -euo pipefail
root="$(cd "$(dirname "$0")/../.." && pwd)"
feed="$root/tools/offline-tests/empty-feed"
dotnet build "$root/core/Thatchwick.Core.OfflineTests" --source "$feed" -v q -nologo -clp:ErrorsOnly
dotnet build "$root/core/Thatchwick.GameCheck" --source "$feed" -v q -nologo -clp:ErrorsOnly
dotnet "$root/core/Thatchwick.Core.OfflineTests/bin/Debug/net8.0/Thatchwick.Core.Tests.dll" "$@"
# Sınıf süzgeciyle çağrıldıysa (tek test) ya da python3/git yoksa atlanır.
if [ "$#" -eq 0 ] && command -v python3 >/dev/null && git -C "$root" rev-parse --git-dir >/dev/null 2>&1; then
  (cd "$root" && python3 tools/loc/sabit_metin.py)
fi
