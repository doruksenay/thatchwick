unity-import: pending

# 211 Karakter arayüzü seti

Durum: `blocked` (0 generation harcandı, PixelLab çağrılmadı). Masaüstü kopyası: yerel (bulutta yapılamaz).

## Neden durdu
`create_ui_asset` `style_image_base64` parametresini yalnızca satır içi metin olarak alıyor. Tarifin zorunlu stil girdisi (210 parşömen paneli, 27 KB PNG, ~36 KB base64) güvenilir biçimde aktarılamıyor; bozulursa sessizce yanlış stille üretim olurdu. Stil görselsiz üretim tarife aykırı olduğundan çağrı yapılmadı. Ayrıntı: iş dosyasında `## Sorular`.

- Yön: `yon:` satırı yok, varsayılan `parsomen`.
- `get_balance`: 4848,9 / 5000 (Tier 2, yenilenme 2026-12-18); plan `art/budget.md` ile uyumlu.
- Dosya teslimi ve doğrulama yok.
