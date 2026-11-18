unity-import: pending

# 372 Ağaçlar yeniden (rapor)

Yan yana sayfası: [372-yanyana.png](372-yanyana.png) (4 referans + 3 teslim). Çim zemin üstünde büyütülmüş görünüm: [372-cim-ustu.png](372-cim-ustu.png). Teslim edilmeyen adaylar: `art/results/372-red/`.
Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız kılavuz + maske (`footprint_guide.py`, tuval 96x112, `--n 1`, yükseklikler 44/88/76/44/40/84/44), `high top-down`, `low detail`, `basic shading`, `single color outline`, tarifteki ek metin (muted greens, no ground vb.). Referanssız kılavuz, 320 ile aynı yöntem. Her çıktıya `style.py fit --family bitki` uygulandı.
Çağrılar: 7 ilk deneme x 1 gen + 5 ikinci deneme x 1 gen (yalnızca biçim tutmayanlar: meşe genç, meşe olgun, huş, erik, söğüt genç) = **12 gen** (cost_max 14). Hız sınırı hatası olmadı. `get_balance` sonrası: 4826,9 (paralel şeritler de harcıyor).

## Teslim edilenler (3/7), `Assets/Art/Iso/_aday/`
| Dosya | Boyut | Pivot x / y (alttan) | Opak sınır | Renk | Doygunluk (fit öncesi→sonrası) | Stil denetimi | validate_iso |
|---|---|---|---|---|---|---|---|
| iso_tree_oak_young_se.png (2. deneme) | 96x112 | 40 / 24 | x11-68, y29-95 | 15 | x0,82 → x1,15 | UYARI (palet dışı, kilit yok) | geçti |
| iso_tree_pine_young_se.png | 96x112 | 40 / 24 | x26-63, y29-76 | 16 | x1,00 → x0,35 | UYARI (ailesinden soluk) | geçti |
| iso_tree_willow_grown_se.png | 96x112 | 40 / 24 | x11-68, y7-100 | 21 | x1,00 → x1,00 | UYARI (palete uzak) | geçti |

Yarı saydam piksel 0. Işık sol-üst: sol/sağ parlaklık meşe 49/47, çam 28/42, söğüt 60/58 (çam çok koyu; sağ yarı daha açık, ışık yönü tam doğrulanamadı). Zemin parçası olmadığı için döşeme testi yok. Gölge pişirilmemiş.
Not: çam genç sivri ve tanınıyor ama çok koyu, ince ve düşük renkli (soluk uyarısı); meşe genç kısa gövdeli, koyu yeşil bir taç (olgun meşenin 1,5 katı kontrolü yapılamadı, olgun meşe teslim edilmedi).

## Teslim edilmeyenler
| Dosya | Neden |
|---|---|
| meşe olgun (1. ve 2. deneme) | 1. denemede tabanda elmas zemin parçası; 2. denemede çim ve toprak bloğu (daha kötü). `no ground` metni kılavuzdaki taban elmasını silmedi |
| huş olgun (1. ve 2.) | Tabanda elmas zemin parçası iki denemede de var |
| erik genç (1. ve 2.) | Tabanda yeşil çim tüfü iki denemede de var |
| söğüt genç (1. ve 2.) | 1: seyrek çıplak ağaç, söğüt değil; 2: gövdesiz çalı |
| meşe genç (1. deneme) | Gövdesiz yeşil küme (2. deneme teslim edildi) |

## Sorunlar ve öneri
- Referanssız kılavuzdaki düz renkli taban elması, büyük ağaçlarda (meşe, huş) ve erikte modeli taban parçası çizmeye itiyor. Metin bunu bastırmadı. Öneri (kod/araç değişikliği olmadan, ayrı iş): kılavuzu çok daha küçük (ör. 16x8) ya da yalnızca gövde genişliğinde bir işaretle vermek, ya da taban parçasını `edit_image`/`inpaint` ile temizlemek; ya da başarılı 301 erik olgun örneğini referans alan başka bir yöntem.
- Meşe olgun hâlâ yok (başlangıç haritasındaki 14 ağacın türü). 370 için meşe olgun gerekli: yeni bir iş gerekir.
