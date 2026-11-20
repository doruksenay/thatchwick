#!/usr/bin/env bash
# Harita önizlemesi (belge 64 §3): yeni oyunun dünyasını kurar, gerçek görsellerle izometrik PNG çizer.
# Kullanım: tools/map-preview/run.sh [bölge,bölge] [seed=N]   çıktı: art/preview/<bölge>/ (git'e girmez)
# Görseller LFS'te: önce `git lfs pull --include "Assets/Art/Iso/**"`.
set -euo pipefail
root="$(cd "$(dirname "$0")/../.." && pwd)"
regions="${1:-}"
args=()
[ -n "$regions" ] && args+=("regions=$regions")
[ -n "${2:-}" ] && args+=("$2")
dotnet build "$root/tools/map-preview" --source "$root/tools/offline-tests/empty-feed" -v q -nologo -clp:ErrorsOnly
dotnet run --project "$root/tools/map-preview" --no-build -- "out=$root/art/preview/_dump" "${args[@]}"
python3 "$root/tools/map-preview/render.py" "$root/art/preview/_dump" "$root/art/preview" ${regions:+--regions "$regions"} --half --sites
