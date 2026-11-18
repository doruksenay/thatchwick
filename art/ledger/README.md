# Defter dosyaları

Paralel üretimde (2026-11-18, `art/budget.md` "Paralel üretim") her iş harcamasını kendi dosyasına yazar: `art/ledger/NNN.md`.
Böylece aynı anda çalışan işçiler aynı tabloya yazıp çakışmaz. `tools/art/queue_ready.py` bu dosyaları `art/budget.md` tablosuyla birlikte okur.

Biçim (`art/budget.md` ile aynı):

| Tarih | İş | Araç | Harcanan | Kalan (get_balance) |
|---|---|---|---|---|
