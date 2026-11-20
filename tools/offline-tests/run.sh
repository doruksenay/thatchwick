#!/usr/bin/env bash
# Çekirdek testlerini NuGet olmadan çalıştırır (bulut oturumları). CI'daki `dotnet test` ile aynı test dosyaları,
# aynı xunit aşırı yükleme kuralları (XunitShim.cs). Kullanım: tools/offline-tests/run.sh [SınıfAdıParçası]
# Game katmanı derleme denetimi de koşar (CI'daki ikinci adım).
set -euo pipefail
root="$(cd "$(dirname "$0")/../.." && pwd)"
feed="$root/tools/offline-tests/empty-feed"
dotnet build "$root/core/Thatchwick.Core.OfflineTests" --source "$feed" -v q -nologo -clp:ErrorsOnly
dotnet build "$root/core/Thatchwick.GameCheck" --source "$feed" -v q -nologo -clp:ErrorsOnly
dotnet "$root/core/Thatchwick.Core.OfflineTests/bin/Debug/net8.0/Thatchwick.Core.Tests.dll" "$@"
