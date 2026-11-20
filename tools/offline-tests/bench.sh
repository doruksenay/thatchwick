#!/usr/bin/env bash
# Çekirdek performans ölçümü (Release). Gerçek başlangıç dünyasında simülasyon adımı, yol bulma, oda tespiti, kayıt.
# Kullanım: tools/offline-tests/bench.sh   (BENCH_HZ=60 ile kare başına adım ölçülür; varsayılan 10)
set -euo pipefail
root="$(cd "$(dirname "$0")/../.." && pwd)"
feed="$root/tools/offline-tests/empty-feed"
dotnet build "$root/core/Thatchwick.Core.OfflineTests" -c Release --source "$feed" -v q -nologo -clp:ErrorsOnly
dotnet "$root/core/Thatchwick.Core.OfflineTests/bin/Release/net8.0/Thatchwick.Core.Tests.dll" --bench
