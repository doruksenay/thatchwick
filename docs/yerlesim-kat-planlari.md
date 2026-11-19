# Yerleşim: kat planları (önce / sonra)

Ekran görüntüsü alınamayan bulut oturumunda (bkz. `docs/yerlesim-ilerleme.md`) her oda ve kat Unity'siz kurulup ASCII planla
çıkarıldı. Önce: Aşama A'daki elle koordinatlı yerleşim (commit 8346cac). Sonra: şablonlar (layouts.json, tohum 1).
Bu dosya `tools/layout-report` ile yeniden üretilir: `dotnet run --project tools/layout-report -- dump=all`.

Okuma: x sağa, y aşağı (kuzey yukarı). Kenar: `--`/`|` duvar, `DD`/`D` kapı, `==`/`=` pencere, `::` çit. Karo: iki harfli
eşya kısaltması; ön yüzlü eşyada ikinci harf yön oku (`^` N = ekranda se, `<` W = sw: kameraya dönük; `>` E, `v` S arkası dönük),
`.` boş, `,,` yol, `~~` su, boşluk = karo yok (merdiven boşluğu). Ekranda bu plan 45° döner: kuzey duvarı sağ-yukarı, batı duvarı
sol-yukarı; kamera güneydoğu-güneybatıdan (alttan) bakar.

## Pazar (Kerem)

### Önce
```
== general kat 0 (önce) ==
+  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .
+  +--+--+--+--+==+--+--+--+  
  .|Ps Ps Sj Sj  . Ps Ps Sc| .
+  +  +  +  +  +  +  +  +  +  
  .| .  .  .  .  .  .  .  .| .
+  +  +  +  +  +  +  +  +  +  
  .| .  .  .  .  .  .  . Ba| .
+  +  +  +  +  +  +  +  +  +  
  .| .  . Pc Pc  .  .  . Ba= .
+  +  +  +  +  +  +  +  +  +  
  .|Fb  .  .  .  .  .  .  .| .
+  +  +  +  +  +  +  +  +  +  
  .| .  .  .  .  .  .  .  .| .
+  +--+==+--+DD+==+--+--+==+  
  .  .  .  . ,,  .  .  .  .  .
   Ba=Fıçı, Fb=Odun sepeti, Pc=Hazırlık tezgâhı, Ps=Kiler rafı, Sc=Tohum kasası, Sj=Küp
```
### Sonra
```
== Pazar (Kerem) kat 1 ==
+  +  +  +  +  +  +  +  +  +  
                              
+  +--+--+--+--+==+--+--+--+  
   |Ca        . Bs  .  . D<|  
+  +  +  +  +  +  +  +  +  +  
   | .  .  .  . Bs  .  . D<|  
+  +  +  +  +  +  +  +  +  +  
   | .  .  .  . B<  .  .  .|  
+  +  +  +  +  +  +  +  +  +  
   | .  .  .  .  .  .  .  .=  
+  +  +  +  +  +  +  +  +  +  
   | .  .  .  .  .  .  .  .|  
+  +  +  +  +  +  +  +  +  +  
   | .  .  . St C<  .  . B^|  
+  +--+==+--+==+==+--+--+==+  
                              
   Ba=Leğen ve ibrik, Be=Başucu sehpası, Bs=Saman yatak, Ca=Şamdan, Cc=Oymalı sandalye, Dc=Çeyiz sandığı, St=Sini sehpa
== Pazar (Kerem) kat 0 ==
+  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .
+  +--+--+--+--+==+--+--+--+  
  .| . Su Su Sc  . Sd Sd Wi| .
+  +  +  +  +  +  +  +  +  +  
  .| .  .  .  .  .  .  . P<| .
+  +  +  +  +  +  +  +  +  +  
  .|Sj  .  .  .  .  .  . P<| .
+  +  +  +  +  +  +  +  +  +  
  .|Br  . So So  .  .  .  .= .
+  +  +  +  +  +  +  +  +  +  
  .|Br  .  .  .  .  .  .  .| .
+  +  +  +  +  +  +  +  +  +  
  .|Sj  . Wi  .  . P^ P^ Ca| .
+  +--+==+--+DD+==+--+--+==+  
  .  .  .  . ,,  .  .  .  .  .
   Br=Fıçı, Ca=Şamdan, Ps=Kiler rafı, Sc=Tohum kasası, Sd=Bodrum merdiveni, Sj=Küp, So=Satış tezgâhı, Su=Ahşap merdiven (üst kat), Wi=Hasır sepet
== Pazar (Kerem) kat -1 ==
+  +  +  +  +  +  +  +  +  +  
                              
+  +--+--+--+--+--+--+--+--+  
   |Fs  . Gs  .  .  . Gs  .|  
+  +  +  +  +  +  +  +  +  +  
   | .  .  .  .  .  .  .  .|  
+  +  +  +  +  +  +  +  +  +  
   |Gs  .  .  .  .  .  .  .|  
+  +  +  +  +  +  +  +  +  +  
   | .  .  .  .  .  .  .  .|  
+  +  +  +  +  +  +  +  +  +  
   | .  .  .  .  .  .  .  .|  
+  +  +  +  +  +  +  +  +  +  
   |C^  .  .  .  . C^  .  .|  
+  +--+--+--+--+--+--+--+--+  
                              
   Cw=Sandık, Fs=Un çuvalı, Gs=Tahıl çuvalı
```

## Demirci (Orhan)

### Önce
```
== smithy kat 0 (önce) ==
+  +  +  +  +  +  +  +  +  
  .  .  .  . ,,  . ,, ,, ,,
+  +--+==+--+DD+==+--+--+  
  .|Fo Fo  .  .  .  . Fb|,,
+  +  +  +  +  +  +  +  +  
  .|Fo Fo  .  .  .  .  .| .
+  +  +  +  +  +  +  +  +  
  .| .  .  .  .  . Bl Bl| .
+  +  +  +  +  +  +  +  +  
  .| .  .  .  .  . Bl Bl= .
+  +  +  +  +  +  +  +  +  
  .|Th Th  .  . Wh  . Wb| .
+  +  +  +  +  +  +  +  +  
  .| .  .  .  .  .  .  .| .
+  +--+--+--+--+--+--+--+  
  .  .  .  .  .  .  .  .  .
   Bl=Çiçek ocağı, Fb=Odun sepeti, Fo=Demir ocağı ve örs, Th=Su çekici, Wb=Su fıçısı, Wh=Bileği taşı
```
### Sonra
```
== Demirci (Orhan) kat 0 ==
+  +  +  +  +  +  +  +  +  
  .  .  .  . ,,  . ,, ,, ,,
+  +--+==+--+DD+==+--+--+  
  .| .  .  .  .  . Wh  .|,,
+  +  +  +  +  +  +  +  +  
  .| .  .  .  .  .  .  .| .
+  +  +  +  +  +  +  +  +  
  .| .  .  .  .  .  .  .| .
+  +  +  +  +  +  +  +  +  
  .|Ip  .  .  .  .  .  .= .
+  +  +  +  +  +  +  +  +  
  .| .  . Qt F^ F^ A^  .| .
+  +  +  +  +  +  +  +  +  
  .| .  . Fb F^ F^ Bo  .| .
+  +--+--+--+--+--+--+--+  
  .  .  .  .  .  .  .  .  .
   An=Örs, Bo=Körük, Fb=Odun sepeti, Fo=Demir ocağı ve örs, Ip=Nal ve demir yığını, Qt=Su teknesi (demirci), Wh=Bileği taşı
```

## Marangoz / mobilyacı

### Önce
```
== carpenter kat 0 (önce) ==
+  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .
+  +--+--+--+--+==+--+--+--+  
  .|Wo Wo  . Wo Wo Fb  .  .| .
+  +  +  +  +  +  +  +  +  +  
  .| .  .  .  .  .  .  .  .= .
+  +  +  +  +  +  +  +  +  +  
  .| .  .  .  .  .  .  .  .D,,
+  +  +  +  +  +  +  +  +  +  
  .| .  .  .  .  .  .  .  .| .
+  +  +  +  +  +  +  +  +  +  
  .|Cb Cb Tp Tp  . Cp Cc  .= .
+  +  +  +  +  +  +  +  +  +  
  .| .  .  .  .  .  .  .  .| .
+  +--+--+--+--+--+--+--+--+  
  .  .  .  .  .  .  .  .  .  .
   Cb=Sandık-bank, Cc=Oymalı sandalye, Cp=Boyalı sandalye, Fb=Odun sepeti, Tp=Boyalı masa, Wo=Marangoz tezgâhı
```
### Sonra
```
== Marangoz / mobilyacı kat 1 ==
+  +  +  +  +  +  +  +  +  +  
                              
+  +--+--+--+--+==+--+--+--+  
   | .        .  .  .  . Ca|  
+  +  +  +  +  +  +  +  +  +  
   | .  .  .  .  .  .  .  .=  
+  +  +  +  +  +  +  +  +  +  
   |Bs Bs B^  .  .  .  .  .=  
+  +  +  +  +  +  +  +  +  +  
   | .  .  .  .  .  . St  .|  
+  +  +  +  +  +  +  +  +  +  
   | .  .  .  .  .  . C^  .=  
+  +  +  +  +  +  +  +  +  +  
   | . C^  . B^  .  .  .  .|  
+  +--+--+--+--+--+--+--+--+  
                              
   Ba=Leğen ve ibrik, Be=Başucu sehpası, Bs=Saman yatak, Ca=Şamdan, Cc=Oymalı sandalye, Cw=Sandık, St=Sini sehpa
== Marangoz / mobilyacı kat 0 ==
+  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .
+  +--+--+--+--+==+--+--+--+  
  .| . Su Su  .  . Lp  . W<| .
+  +  +  +  +  +  +  +  +  +  
  .| . Uf  .  .  . Lp  . W<= .
+  +  +  +  +  +  +  +  +  +  
  .| .  .  .  .  .  .  .  .D,,
+  +  +  +  +  +  +  +  +  +  
  .| .  .  .  .  .  .  . C<| .
+  +  +  +  +  +  +  +  +  +  
  .| .  .  .  . Tp Tp  .  .= .
+  +  +  +  +  +  +  +  +  +  
  .| .  .  .  . C^ C^ W^ W^| .
+  +--+--+--+--+--+--+--+--+  
  .  .  .  .  .  .  .  .  .  .
   Cc=Oymalı sandalye, Cw=Sandık, Lp=Kereste yığını, Su=Ahşap merdiven (üst kat), Tp=Boyalı masa, Uf=Yarım kalmış dolap, Wo=Marangoz tezgâhı
```

## Meyhane / han

### Önce
```
== tavern kat 0 (önce) ==
+  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  . ,, ,,  .
+  +--+--+--+--+--+==+--+--+--+--+  
  .|Hs Co Pc Pc  .  .  . Bb Bb Bb| .
+  +  +  +  +  +  +  +  +  +  +  +  
  .|Fb  .  .  .  .  .  .  . ,, Ba|,,
+  +  +  +  +  +  +  +  +  +  +  +  
  .| . Be Be  .  . Be Be  . ,, Ba|,,
+  +  +  +  +  +  +  +  +  +  +  +  
  .| . Tt Tt Tt  . Tt Tt Tt  .  .=,,
+  +  +  +  +  +  +  +  +  +  +  +  
  .| . Be Be  .  . Be Be  .  .  .|,,
+  +  +  +  +  +  +  +  +  +  +  +  
  .| .  .  .  .  .  .  .  .  . Ss|,,
+  +  +  +  +  +  +  +  +  +  +  +  
  .|Ca  .  .  .  .  .  .  .  .  .|,,
+  +--+==+--+--+==+DD+--+==+--+--+  
  .  .  .  .  .  . ,,  .  .  . ,, ,,
   Ba=Fıçı, Bb=Mayalama fıçısı, Be=Uzun bank, Ca=Şamdan, Co=Bakır kazan seti, Fb=Odun sepeti, Hs=Taş ocak, Pc=Hazırlık tezgâhı, Ss=Şarkıcı kürsüsü, Tt=Sehpa masa
```
### Sonra
```
== Meyhane / han kat 1 ==
+  +  +  +  +  +  +  +  +  +  +  +  
                                    
+  +--+--+--+--+--+==+--+--+--+--+  
   | .  .  .  . Bs| .  .  .  . B<|  
+  +  +  +  +  +  +  +  +  +  +  +  
   |    .  . B< Bs|Bs Bs B^  .  .|  
+  +  +  +  +  +  +  +  +  +  +  +  
   |   B^  .  . C<| .  .  .  . C^|  
+  +--+--+DD+--+--+--+--+DD+--+--+  
   | .  .  .  .  .  .  .  .  .  .=  
+  +--+--+DD+--+--+--+--+DD+--+--+  
   |Bs Bs  .  . B<| .  .  . Sw  .|  
+  +  +  +  +  +  +  +  +  +  +  +  
   | . B<  .  .  .| .  .  .  .  .|  
+  +  +  +  +  +  +  +  +  +  +  +  
   | .  .  .  . C<|Bs Bs  .  . C^|  
+  +--+==+--+--+==+==+--+==+--+--+  
                                    
   Ba=Leğen ve ibrik, Be=Başucu sehpası, Bs=Saman yatak, Cw=Sandık, Sw=Tahta tabure
== Meyhane / han kat 0 ==
+  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  . ,, ,,  .
+  +--+--+--+--+--+==+--+--+--+--+  
  .| .  .  . Sd Sd Bb  .  .  . Bb| .
+  +  +  +  +  +  +  +  +  +  +  +  
  .|Su  .  . Sw  .  .  .  .  .  .|,,
+  +  +  +  +  +  +  +  +  +  +  +  
  .|Su  .  . Ht  . Ht  .  .  .  .|,,
+  +  +  +  +  +  +  +  +  +  +  +  
  .|Br  . Sw Ht H< Ht H<  .  . Co=,,
+  +  +  +  +  +  +  +  +  +  +  +  
  .| .  .  . Ht H< Ht H<  .  . H<|,,
+  +  +  +  +  +  +  +  +  +  +  +  
  .|Br  .  .  .  .  .  .  .  . Fb|,,
+  +  +  +  +  +  +  +  +  +  +  +  
  .|Ss  . P^ P^  .  .  .  .  . Ca|,,
+  +--+==+--+--+==+DD+--+==+--+--+  
  .  .  .  .  .  . ,,  .  .  . ,, ,,
   Bb=Mayalama fıçısı, Br=Fıçı, Ca=Şamdan, Co=Bakır kazan seti, Fb=Odun sepeti, Hb=Han sırası, Hs=Taş ocak, Ht=Uzun han masası, Pc=Hazırlık tezgâhı, Sd=Bodrum merdiveni, Ss=Şarkıcı kürsüsü, Su=Ahşap merdiven (üst kat), Sw=Tahta tabure
== Meyhane / han kat -1 ==
+  +  +  +  +  +  +  +  +  +  +  +  
                                    
+  +--+--+--+--+--+--+--+--+--+--+  
   | . Sj  . Sj  .  .  .  . Bb  .|  
+  +  +  +  +  +  +  +  +  +  +  +  
   | .  .  .  .  .  .  .  .  .  .|  
+  +  +  +  +  +  +  +  +  +  +  +  
   |Br  .  .  .  .  .  .  .  .  .|  
+  +  +  +  +  +  +  +  +  +  +  +  
   | .  .  .  .  .  .  .  .  .  .|  
+  +  +  +  +  +  +  +  +  +  +  +  
   |Bb  .  .  .  .  .  .  .  .  .|  
+  +  +  +  +  +  +  +  +  +  +  +  
   | .  .  .  .  .  .  .  .  .  .|  
+  +  +  +  +  +  +  +  +  +  +  +  
   |Bb  .  .  . P^ P^  .  .  .  .|  
+  +--+--+--+--+--+--+--+--+--+--+  
                                    
   Bb=Mayalama fıçısı, Br=Fıçı, Ps=Kiler rafı, Sj=Küp
```

## Değirmen ve fırın (Hatice)

### Önce
```
== mill kat 0 (önce) ==
+  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .
+  +--+--+--+==+--+--+--+  
  .|Mi Mi  .  . Qu  . Sc| .
+  +  +  +  +  +  +  +  +  
  .|Mi Mi  .  .  .  . Ba|Ww
+  +  +  +  +  +  +  +  +  
  .| .  .  .  .  .  .  .|Ww
+  +  +  +  +  +  +  +  +  
  .| .  .  .  .  . So So= .
+  +  +  +  +  +  +  +  +  
  .| . Tw Tw  .  .  .  .| .
+  +  +  +  +  +  +  +  +  
  .| .  .  .  .  .  .  .| .
+  +--+==+--+DD+==+--+--+  
  .  .  .  . ,,  .  .  .  .
   Ba=Fıçı, Mi=Değirmen taşı, Qu=El değirmeni, Sc=Tohum kasası, So=Taş fırın, Tw=Tahta masa, Ww=Su çarkı
```
### Sonra
```
== Değirmen ve fırın (Hatice) kat 1 ==
+  +  +  +  +  +  +  +  +  
                           
+  +--+--+--+==+--+--+--+  
   | .        .  .  . Ca|  
+  +  +  +  +  +  +  +  +  
   | .  .  .  . Sm  .  .|  
+  +  +  +  +  +  +  +  +  
   |Bs  .  .  . C^  .  .|  
+  +  +  +  +  +  +  +  +  
   |Bs B^  .  .  .  .  .=  
+  +  +  +  +  +  +  +  +  
   | .  .  .  .  .  .  .|  
+  +  +  +  +  +  +  +  +  
   | . B^  . C^  .  .  .|  
+  +--+==+--+==+==+--+--+  
                           
   Ba=Leğen ve ibrik, Be=Başucu sehpası, Bs=Saman yatak, Ca=Şamdan, Cc=Oymalı sandalye, Cw=Sandık, Sm=Kütük sehpa
== Değirmen ve fırın (Hatice) kat 0 ==
+  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .
+  +--+--+--+==+--+--+--+  
  .| . Su Su Mi Mi Fs Fs| .
+  +  +  +  +  +  +  +  +  
  .| .  .  . Mi Mi  . Sc|Ww
+  +  +  +  +  +  +  +  +  
  .| .  .  .  .  .  . K<|Ww
+  +  +  +  +  +  +  +  +  
  .|Sd  .  .  .  .  . K<= .
+  +  +  +  +  +  +  +  +  
  .|Sd  .  .  .  .  . S<| .
+  +  +  +  +  +  +  +  +  
  .|Qu  .  .  . B^  . S<| .
+  +--+==+--+DD+==+--+--+  
  .  .  .  . ,,  .  .  .  .
   Bd=Ekmek rafı, Fs=Un çuvalı, Kt=Hamur teknesi, Mi=Değirmen taşı, Qu=El değirmeni, Sc=Tohum kasası, Sd=Bodrum merdiveni, Sn=Taş fırın, Su=Ahşap merdiven (üst kat), Ww=Su çarkı
== Değirmen ve fırın (Hatice) kat -1 ==
+  +  +  +  +  +  +  +  +  
                           
+  +--+--+--+--+--+--+--+  
   |Gs  .  .  . Gs  . Fs|  
+  +  +  +  +  +  +  +  +  
   | .  .  .  .  .  .  .|  
+  +  +  +  +  +  +  +  +  
   |Fs  .  .  .  .  .  .|  
+  +  +  +  +  +  +  +  +  
   |Fs  .  .  .  .  .  .|  
+  +  +  +  +  +  +  +  +  
   |Fs  .  .  .  .  .  .|  
+  +  +  +  +  +  +  +  +  
   | .  .  .  .  .  .  .|  
+  +--+--+--+--+--+--+--+  
                           
   Fs=Un çuvalı, Gs=Tahıl çuvalı
```

## Kâtip ve ulak

### Önce
```
== scribe kat 0 (önce) ==
+  +  +  +  +  +  +  +  
 ,,  .  .  .  .  .  .  .
+  +--+--+--+==+--+--+  
 ,,|Sw Sw Sw Sw  . Ca| .
+  +  +  +  +  +  +  +  
 ,,| .  .  .  .  .  .= .
+  +  +  +  +  +  +  +  
 ,,| .  . Tp Tp  .  .D,,
+  +  +  +  +  +  +  +  
 ,,| .  . Cc  .  .  .| .
+  +  +  +  +  +  +  +  
 ,,|Cw  .  .  .  .  .= .
+  +--+--+--+--+--+--+  
 ,,  .  .  .  .  .  .  .
   Ca=Şamdan, Cc=Oymalı sandalye, Cw=Sandık, Sw=Raf, Tp=Boyalı masa
```
### Sonra
```
== Kâtip ve ulak kat 1 ==
+  +  +  +  +  +  +  +  
                        
+  +--+--+--+==+--+--+  
   | .        . B< Bs|  
+  +  +  +  +  +  +  +  
   | .  .  .  . Sm Bs=  
+  +  +  +  +  +  +  +  
   | .  .  .  . C^  .=  
+  +  +  +  +  +  +  +  
   | .  .  .  .  .  .|  
+  +  +  +  +  +  +  +  
   |Ca  .  . D^ D^ B^=  
+  +--+--+--+--+--+--+  
                        
   Ba=Leğen ve ibrik, Be=Başucu sehpası, Bs=Saman yatak, Ca=Şamdan, Ch=Tahta sandalye, Dc=Çeyiz sandığı, Sm=Kütük sehpa
== Kâtip ve ulak kat 0 ==
+  +  +  +  +  +  +  +  
 ,,  .  .  .  .  .  .  .
+  +--+--+--+==+--+--+  
 ,,| . Su Su  .  . W<| .
+  +  +  +  +  +  +  +  
 ,,| . Tp C<  .  . C<= .
+  +  +  +  +  +  +  +  
 ,,| . Tp  .  .  .  .D,,
+  +  +  +  +  +  +  +  
 ,,| .  .  .  .  . S<| .
+  +  +  +  +  +  +  +  
 ,,| . Sd Sd S^ S^  .= .
+  +--+--+--+--+--+--+  
 ,,  .  .  .  .  .  .  .
   Cc=Oymalı sandalye, Cw=Sandık, Sd=Bodrum merdiveni, Sh=Raf, Sr=Rulo rafı, Su=Ahşap merdiven (üst kat), Tp=Boyalı masa, Wl=Yazı kürsüsü (rahle)
== Kâtip ve ulak kat -1 ==
+  +  +  +  +  +  +  +  
                        
+  +--+--+--+--+--+--+  
   | .  .  .  .  . S<|  
+  +  +  +  +  +  +  +  
   | .  .  .  .  . S<|  
+  +  +  +  +  +  +  +  
   | .  .  .  .  . S<|  
+  +  +  +  +  +  +  +  
   | .  .  .  .  . S<|  
+  +  +  +  +  +  +  +  
   | .  .  .  .  . S<|  
+  +--+--+--+--+--+--+  
                        
   Sh=Raf, Sl=Rulo sandığı
```

## Otacı ve ebe

### Önce
```
== herb kat 0 (önce) ==
+  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .
+  +--+--+--+==+--+--+--+  
  .|Po Po Po  . Dr Dr  .| .
+  +  +  +  +  +  +  +  +  
  .| .  .  .  .  .  .  .| .
+  +  +  +  +  +  +  +  +  
  .| .  .  .  .  .  .  .|Ta
+  +  +  +  +  +  +  +  +  
  .|Tw Tw  .  .  . Bs Bs= .
+  +  +  +  +  +  +  +  +  
  .| .  .  .  .  .  .  .| .
+  +  +  +  +  +  +  +  +  
  .| .  .  .  .  .  . Bi| .
+  +--+==+--+DD+==+--+--+  
  .  .  .  . ,,  .  .  .  .
   Bi=Leğen ve ibrik, Bs=Saman yatak, Dr=Kurutma askısı, Po=İksir rafı, Ta=Elma ağacı, Tw=Tahta masa
```
### Sonra
```
== Otacı ve ebe kat 1 ==
+  +  +  +  +  +  +  +  +  
                           
+  +--+--+--+==+--+--+--+  
   |Ca        .  .  . D<|  
+  +  +  +  +  +  +  +  +  
   | .  .  .  .  .  . D<|  
+  +  +  +  +  +  +  +  +  
   |Bs Bs  .  .  .  .  .|  
+  +  +  +  +  +  +  +  +  
   | . B<  .  .  .  .  .=  
+  +  +  +  +  +  +  +  +  
   | . St Sw  .  .  .  .|  
+  +  +  +  +  +  +  +  +  
   | .  .  .  .  .  . B<|  
+  +--+==+--+==+==+--+--+  
                           
   Ba=Leğen ve ibrik, Be=Başucu sehpası, Bs=Saman yatak, Ca=Şamdan, Dc=Çeyiz sandığı, St=Sini sehpa, Sw=Tahta tabure
== Otacı ve ebe kat 0 ==
+  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .
+  +--+--+--+==+--+--+--+  
  .| . Su Su Dr  .  .  .| .
+  +  +  +  +  +  +  +  +  
  .| .  .  . Dr  .  . P<| .
+  +  +  +  +  +  +  +  +  
  .| .  . Tw Sw  .  . B<|Ta
+  +  +  +  +  +  +  +  +  
  .|Sd  . Tw  .  .  .  .= .
+  +  +  +  +  +  +  +  +  
  .|Sd  .  .  .  .  .  .| .
+  +  +  +  +  +  +  +  +  
  .|Bs Bs  .  .  . P^  .| .
+  +--+==+--+DD+==+--+--+  
  .  .  .  . ,,  .  .  .  .
   Ba=Leğen ve ibrik, Bs=Saman yatak, Dr=Kurutma askısı, Po=İksir rafı, Sd=Bodrum merdiveni, Su=Ahşap merdiven (üst kat), Sw=Tahta tabure, Ta=Elma ağacı, Tw=Tahta masa
== Otacı ve ebe kat -1 ==
+  +  +  +  +  +  +  +  +  
                           
+  +--+--+--+--+--+--+--+  
   |Sj  .  .  . Sj  .  .|  
+  +  +  +  +  +  +  +  +  
   | .  .  .  .  .  .  .|  
+  +  +  +  +  +  +  +  +  
   | .  .  .  .  .  .  .|  
+  +  +  +  +  +  +  +  +  
   | .  .  .  .  .  . P<|  
+  +  +  +  +  +  +  +  +  
   | .  .  .  .  .  . P<|  
+  +  +  +  +  +  +  +  +  
   | .  . P^ P^  .  .  .|  
+  +--+--+--+--+--+--+--+  
                           
   Ps=Kiler rafı, Sj=Küp
```

## Çömlekçi (Ayşe)

### Önce
```
== potter kat 0 (önce) ==
+  +  +  +  +  +  +  +  
 ,, ,, ,, ,,  .  .  .  .
+  +--+==+DD+--+==+--+  
  .|Bt  .  . Sw Sw  .| .
+  +  +  +  +  +  +  +  
  .| .  .  .  .  .  .| .
+  +  +  +  +  +  +  +  
  .|Pw  .  .  .  .  .| .
+  +  +  +  +  +  +  +  
  .| .  .  .  .  .  .= .
+  +  +  +  +  +  +  +  
 ,,|,, ,, ,, ,, Ki Ki|,,
+  +  +  +  +  +  +  +  
  .|Sj Sj  .  . Ki Ki| .
+  +--+--+--+--+--+--+  
  .  .  .  .  .  .  .  .
   Bt=Fıçı küvet, Ki=Tuğla fırını, Pw=Çömlekçi çarkı, Sj=Küp, Sw=Raf
```
### Sonra
```
== Çömlekçi (Ayşe) kat 1 ==
+  +  +  +  +  +  +  +  
                        
+  +--+==+==+--+==+--+  
   | .  . Sm C<  . Ca|  
+  +  +  +  +  +  +  +  
   |B^  .  .  .  .  .|  
+  +  +  +  +  +  +  +  
   |Bf Bf  .  .  .  .|  
+  +  +  +  +  +  +  +  
   |    .  .  .  .  .=  
+  +  +  +  +  +  +  +  
   |    .  .  .  . B<|  
+  +  +  +  +  +  +  +  
   | .  .  . C^  .  .|  
+  +--+--+--+--+--+--+  
                        
   Ba=Leğen ve ibrik, Be=Başucu sehpası, Bf=Tahta karyola, Ca=Şamdan, Cc=Oymalı sandalye, Cw=Sandık, Sm=Kütük sehpa
== Çömlekçi (Ayşe) kat 0 ==
+  +  +  +  +  +  +  +  
 ,, ,, ,, ,,  .  .  .  .
+  +--+==+DD+--+==+--+  
  .|Pd Pd  .  . Cy Pd| .
+  +  +  +  +  +  +  +  
  .|Sj  .  . Sw  . Pd| .
+  +  +  +  +  +  +  +  
  .| .  .  .  .  .  .| .
+  +  +  +  +  +  +  +  
  .|Su  .  .  .  .  .= .
+  +  +  +  +  +  +  +  
 ,,|Su  .  .  .  .  .|,,
+  +  +  +  +  +  +  +  
  .| .  . P^  .  .  .| .
+  +--+--+--+--+--+--+  
  .  .  .  .  .  .  .  .
   Cy=Kil teknesi, Pd=Kuruyan çömlek rafı, Pw=Çömlekçi çarkı, Sj=Küp, Su=Ahşap merdiven (üst kat), Sw=Tahta tabure
```

## Terzi ve dokumacı

### Önce
```
== tailor kat 0 (önce) ==
+  +  +  +  +  +  +  +  +  
  . ,, ,, ,,  .  .  .  .  .
+  +--+==+DD+--+==+--+--+  
  .| .  .  .  .  .  .  .| .
+  +  +  +  +  +  +  +  +  
  .| .  .  .  . Tw Tw Cw| .
+  +  +  +  +  +  +  +  +  
  .|Lo Lo  .  .  .  .  .| .
+  +  +  +  +  +  +  +  +  
  .| .  .  .  .  .  .  .= .
+  +  +  +  +  +  +  +  +  
  .|Sp  . St  .  .  . Dv| .
+  +  +  +  +  +  +  +  +  
  .| .  .  .  . Cl Cl  .| .
+  +--+--+--+--+--+--+--+  
  .  .  .  .  .  .  .  .  .
   Cl=Elbise dolabı, Cw=Sandık, Dv=Boya kazanı, Lo=Dokuma tezgâhı, Sp=Çıkrık, St=Tahta tabure, Tw=Tahta masa
```
### Sonra
```
== Terzi ve dokumacı kat 1 ==
+  +  +  +  +  +  +  +  +  
                           
+  +--+==+==+--+==+--+--+  
   |Ca  .  . Bs Bs  .  .|  
+  +  +  +  +  +  +  +  +  
   | .  .  . B<  .  .  .|  
+  +  +  +  +  +  +  +  +  
   | .  .  .  .  .  .  .|  
+  +  +  +  +  +  +  +  +  
   |    .  .  .  .  .  .=  
+  +  +  +  +  +  +  +  +  
   |    . Sw  .  .  .  .|  
+  +  +  +  +  +  +  +  +  
   | .  . St D^ D^ B^  .|  
+  +--+--+--+--+--+--+--+  
                           
   Ba=Leğen ve ibrik, Be=Başucu sehpası, Bs=Saman yatak, Ca=Şamdan, Dc=Çeyiz sandığı, St=Sini sehpa, Sw=Tahta tabure
== Terzi ve dokumacı kat 0 ==
+  +  +  +  +  +  +  +  +  
  . ,, ,, ,,  .  .  .  .  .
+  +--+==+DD+--+==+--+--+  
  .|Ga  .  .  .  . Yb  .| .
+  +  +  +  +  +  +  +  +  
  .| .  .  .  .  .  . S<| .
+  +  +  +  +  +  +  +  +  
  .| .  .  . C<  .  . L<| .
+  +  +  +  +  +  +  +  +  
  .|Su  .  . C< Sw  . L<= .
+  +  +  +  +  +  +  +  +  
  .|Su  .  .  .  .  .  .| .
+  +  +  +  +  +  +  +  +  
  .| .  . C^ C^  . C^ C^| .
+  +--+--+--+--+--+--+--+  
  .  .  .  .  .  .  .  .  .
   Cb=Kumaş topları rafı, Cl=Elbise dolabı, Ct=Kesim masası, Ga=Elbise askısı, Lo=Dokuma tezgâhı, Sp=Çıkrık, Su=Ahşap merdiven (üst kat), Sw=Tahta tabure, Yb=İplik ve yumak sepeti
```

## Saraç ve tabakhane

### Önce
```
== saddler kat 0 (önce) ==
+  +  +  +  +  +  +  +  +  
  .  .  .  . ,,  .  .  .  .
+  +--+==+--+DD+==+--+--+  
  .|Sb Sb  .  .  . Sr Sr| .
+  +  +  +  +  +  +  +  +  
  .| .  .  .  .  .  .  .| .
+  +  +  +  +  +  +  +  +  
  .| .  .  .  .  .  .  .| .
+  +  +  +  +  +  +  +  +  
  .| .  .  .  .  .  .  .= .
+  +  +  +  +  +  +  +  +  
  .| .  .  .  . Tw Tw  .| .
+  +  +  +  +  +  +  +  +  
  .| .  .  .  .  .  . Ba| .
+  +--+--+--+--+--+--+--+  
  .  .  .  .  .  .  .  .  .
   Ba=Fıçı, Sb=Saraç tezgâhı, Sr=Eyer sehpası, Tw=Tahta masa
```
### Sonra
```
== Saraç ve tabakhane kat 1 ==
+  +  +  +  +  +  +  +  +  
                           
+  +--+==+--+==+==+--+--+  
   |Bs Bs  .  .  .  . Ca|  
+  +  +  +  +  +  +  +  +  
   | . B<  .  .  .  .  .|  
+  +  +  +  +  +  +  +  +  
   | .  . Sm  .  .  .  .|  
+  +  +  +  +  +  +  +  +  
   |    . Sw  .  .  .  .=  
+  +  +  +  +  +  +  +  +  
   |    .  .  .  .  .  .|  
+  +  +  +  +  +  +  +  +  
   | .  .  . B^  . D^ D^|  
+  +--+--+--+--+--+--+--+  
                           
   Ba=Leğen ve ibrik, Be=Başucu sehpası, Bs=Saman yatak, Ca=Şamdan, Dc=Çeyiz sandığı, Sm=Kütük sehpa, Sw=Tahta tabure
== Saraç ve tabakhane kat 0 ==
+  +  +  +  +  +  +  +  +  
  .  .  .  . ,,  .  .  .  .
+  +--+==+--+DD+==+--+--+  
  .|Hr  . Sa  .  .  . Sa| .
+  +  +  +  +  +  +  +  +  
  .|Bt  .  .  .  .  .  .| .
+  +  +  +  +  +  +  +  +  
  .| .  .  .  .  .  .  .| .
+  +  +  +  +  +  +  +  +  
  .|Su  .  .  .  .  .  .= .
+  +  +  +  +  +  +  +  +  
  .|Su  .  . Tw  .  . S<| .
+  +  +  +  +  +  +  +  +  
  .| .  .  . Tw  .  . S<| .
+  +--+--+--+--+--+--+--+  
  .  .  .  .  .  .  .  .  .
   Bt=Fıçı küvet, Hr=Deri askısı, Sa=Eyer sehpası, Se=Saraç tezgâhı, Su=Ahşap merdiven (üst kat), Tw=Tahta masa
```

## Balıkçı

### Önce
```
== fisher kat 0 (önce) ==
+  +  +  +  +  +  +  +  
 ~~  .  .  .  . ,,  .  .
+  +--+--+--+==+--+--+  
 ~~| .  . Sw Sw  . Ba| .
+  +  +  +  +  +  +  +  
  .= .  .  .  .  . Sa| .
+  +  +  +  +  +  +  +  
 ,,D .  .  .  .  .  .| .
+  +  +  +  +  +  +  +  
 ,,| .  .  . Tw Tw  .| .
+  +  +  +  +  +  +  +  
 ,,= . Ft  .  .  .  .| .
+  +--+--+--+--+--+--+  
 ,,  .  .  .  . ,,  .  .
   Ba=Fıçı, Ft=Balık sepeti (tuzak), Sa=Tuzlama fıçısı, Sw=Raf, Tw=Tahta masa
```
### Sonra
```
== Balıkçı kat 1 ==
+  +  +  +  +  +  +  +  
                        
+  +--+--+--+==+--+--+  
   |Bs Bs  .        .|  
+  +  +  +  +  +  +  +  
   = . B<  .  .  .  .|  
+  +  +  +  +  +  +  +  
   = .  .  .  .  .  .|  
+  +  +  +  +  +  +  +  
   | .  .  .  .  . St|  
+  +  +  +  +  +  +  +  
   =B^  . D^ D^  . C^|  
+  +--+--+--+--+--+--+  
                        
   Ba=Leğen ve ibrik, Be=Başucu sehpası, Bs=Saman yatak, Cc=Oymalı sandalye, Dc=Çeyiz sandığı, St=Sini sehpa
== Balıkçı kat 0 ==
+  +  +  +  +  +  +  +  
 ~~  .  .  .  . ,,  .  .
+  +--+--+--+==+--+--+  
 ~~|Fi Tw  . Su Su Sb| .
+  +  +  +  +  +  +  +  
  .=Br Tw  .  .  .  .| .
+  +  +  +  +  +  +  +  
 ,,D .  .  .  .  . Sd| .
+  +  +  +  +  +  +  +  
 ,,| .  .  .  .  . Sd| .
+  +  +  +  +  +  +  +  
 ,,=Fi  .  .  .  .  .| .
+  +--+--+--+--+--+--+  
 ,,  .  .  .  . ,,  .  .
   Br=Fıçı, Fi=Balık sepeti, Sb=Tuzlama fıçısı, Sd=Bodrum merdiveni, Su=Ahşap merdiven (üst kat), Tw=Tahta masa
== Balıkçı kat -1 ==
+  +  +  +  +  +  +  +  
                        
+  +--+--+--+--+--+--+  
   | .  . Sb Bc  . Sb|  
+  +  +  +  +  +  +  +  
   |Sb  .  .  .  .  .|  
+  +  +  +  +  +  +  +  
   | .  .  .  .  .  .|  
+  +  +  +  +  +  +  +  
   |Br  .  .  .  .  .|  
+  +  +  +  +  +  +  +  
   |Br  .  .  .  .  .|  
+  +--+--+--+--+--+--+  
                        
   Bc=Salamura küpü, Br=Fıçı, Sb=Tuzlama fıçısı
```

## Taşçı

### Önce
```
== mason kat 0 (önce) ==
+  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .
+  +--+--+--+==+--+--+--+  
  .|Sn Sn  . Wo Wo  .  .| .
+  +  +  +  +  +  +  +  +  
  .| .  .  .  .  .  .  .= .
+  +  +  +  +  +  +  +  +  
  .| .  .  .  .  .  .  .D,,
+  +  +  +  +  +  +  +  +  
  .| .  .  .  .  .  .  .| .
+  +  +  +  +  +  +  +  +  
  .|Tw Tw  .  .  .  .  .= .
+  +  +  +  +  +  +  +  +  
  .| .  .  .  .  .  . Ba| .
+  +--+--+--+--+--+--+--+  
  .  .  .  .  .  .  .  .  .
   Ba=Fıçı, Sn=Taş yontma tezgâhı, Tw=Tahta masa, Wo=Marangoz tezgâhı
```
### Sonra
```
== Taşçı kat 0 ==
+  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .
+  +--+--+--+==+--+--+--+  
  .| .  . Sk  .  . Wh  .| .
+  +  +  +  +  +  +  +  +  
  .|Sk  .  .  .  .  .  .= .
+  +  +  +  +  +  +  +  +  
  .| .  .  .  .  .  .  .D,,
+  +  +  +  +  +  +  +  +  
  .| .  .  .  .  .  .  .| .
+  +  +  +  +  +  +  +  +  
  .| .  .  .  .  .  .  .= .
+  +  +  +  +  +  +  +  +  
  .| .  . A^ A^  . W^ W^| .
+  +--+--+--+--+--+--+--+  
  .  .  .  .  .  .  .  .  .
   Aa=Taş yontma tezgâhı, Sk=Taş blokları, Wh=Bileği taşı, Wo=Marangoz tezgâhı
```

## Çiftlik evi

### Önce
```
== farmhouse kat 1 (önce) ==
+  +  +  +  +  +  +  +  +  +  +  
                                 
+  +--+--+==+--+--+--+==+--+--+  
   |Bs Bs Cw  . Bs Bs Cw  .  .|  
+  +  +  +  +  +  +  +  +  +  +  
   | .  .  .  .  .  .  .     .|  
+  +  +  +  +  +  +  +  +  +  +  
   | .  .  .  .  .  .  .     .|  
+  +  +  +  +  +  +  +  +  +  +  
   = .  .  .  .  .  .  .  .  .=  
+  +  +  +  +  +  +  +  +  +  +  
   | .  .  .  .  .  .  .  . Ac|  
+  +  +  +  +  +  +  +  +  +  +  
   | .  .  .  .  .  .  .  .  .|  
+  +  +  +  +  +  +  +  +  +  +  
   |Bs Bs  . Sw Sw St  .  . Ca|  
+  +--+--+==+--+--+--+==+--+--+  
                                 
   Ac=Minderli koltuk, Bs=Saman yatak, Ca=Şamdan, Cw=Sandık, St=Tahta tabure, Sw=Raf
== farmhouse kat 0 (önce) ==
+  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .
+  +--+--+==+--+--+--+==+--+--+  
  .| .  . Ps Ps Hs Ck  .  . Wb| .
+  +  +  +  +  +  +  +  +  +  +  
  .| . Sd  .  .  .  .  . Su  .| .
+  +  +  +  +  +  +  +  +  +  +  
  .| . Sd  .  .  .  .  . Su  .| .
+  +  +  +  +  +  +  +  +  +  +  
  .= .  . Ch Tw Tw Ch  .  .  .= .
+  +  +  +  +  +  +  +  +  +  +  
  .|Bi  .  .  .  .  .  .  .  .| .
+  +  +  +  +  +  +  +  +  +  +  
  .| .  .  .  .  .  .  .  . Ac| .
+  +  +  +  +  +  +  +  +  +  +  
  .|Ca  .  .  .  .  . St  . Cw| .
+  +--+--+==+--+DD+--+==+--+--+  
  .  .  .  .  . ,,  .  .  .  .  .
   Ac=Minderli koltuk, Bi=Leğen ve ibrik, Ca=Şamdan, Ch=Tahta sandalye, Ck=Pişirme kazanı, Cw=Sandık, Hs=Taş ocak, Ps=Kiler rafı, Sd=Bodrum merdiveni, St=Tahta tabure, Su=Ahşap merdiven (üst kat), Tw=Tahta masa, Wb=Su fıçısı
== farmhouse kat -1 (önce) ==
+  +  +  +  +  +  +  +  +  +  +  
                                 
+  +--+--+--+--+--+--+--+--+--+  
   |Ba  .  . Sa Ps Ps  . Ba Sc|  
+  +  +  +  +  +  +  +  +  +  +  
   |Ba  .  .  .  .  .  .  .  .|  
+  +  +  +  +  +  +  +  +  +  +  
   | .  .  .  .  .  .  .  . Bb|  
+  +  +  +  +  +  +  +  +  +  +  
   | .  .  .  .  .  .  .  .  .|  
+  +  +  +  +  +  +  +  +  +  +  
   | .  .  .  . Br  .  .  . Bt|  
+  +  +  +  +  +  +  +  +  +  +  
   | .  .  .  .  .  .  .  .  .|  
+  +  +  +  +  +  +  +  +  +  +  
   |Wo Wo  .  .  . Sw Sw  .  .|  
+  +--+--+--+--+--+--+--+--+--+  
                                 
   Ba=Fıçı, Bb=Mayalama fıçısı, Br=Ahır feneri, Bt=Fıçı küvet, Ps=Kiler rafı, Sa=Tuzlama fıçısı, Sc=Tohum kasası, Sw=Raf, Wo=Marangoz tezgâhı
```
### Sonra
```
== Çiftlik evi kat 1 ==
+  +  +  +  +  +  +  +  +  +  +  
                                 
+  +--+--+==+--+--+--+==+--+--+  
   | . Bs Bs  . Bs  .  .  .  .|  
+  +  +  +  +  +  +  +  +  +  +  
   | .  .  .  . Bs B^  .     .|  
+  +  +  +  +  +  +  +  +  +  +  
   | .  .  .  .  .  .  .     .|  
+  +  +  +  +  +  +  +  +  +  +  
   =Bs Bs  .  .  .  .  .  .  .=  
+  +  +  +  +  +  +  +  +  +  +  
   | .  .  .  .  .  .  .  .  .|  
+  +  +  +  +  +  +  +  +  +  +  
   | .  . Sw  .  .  .  .  .  .|  
+  +  +  +  +  +  +  +  +  +  +  
   |Ca C^  . C^ S^ S^  .  .  .|  
+  +--+--+==+--+--+--+==+--+--+  
                                 
   Be=Başucu sehpası, Bs=Saman yatak, Ca=Şamdan, Cw=Sandık, Sh=Raf, Sw=Tahta tabure
== Çiftlik evi kat 0 ==
+  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .
+  +--+--+==+--+--+--+==+--+--+  
  .|Ca  .  .  .  .  .  .  . H<| .
+  +  +  +  +  +  +  +  +  +  +  
  .| . Sd  .  .  .  .  . Su Cp| .
+  +  +  +  +  +  +  +  +  +  +  
  .| . Sd  .  . Sw  .  . Su  .| .
+  +  +  +  +  +  +  +  +  +  +  
  .= .  .  .  . Tw C<  .  .  .= .
+  +  +  +  +  +  +  +  +  +  +  
  .|Wb  .  .  . Tw C<  .  .  .| .
+  +  +  +  +  +  +  +  +  +  +  
  .| .  .  .  .  .  .  .  . C<| .
+  +  +  +  +  +  +  +  +  +  +  
  .|P^ P^ B^  .  .  .  .  .  .| .
+  +--+--+==+--+DD+--+==+--+--+  
  .  .  .  .  . ,,  .  .  .  .  .
   Ba=Leğen ve ibrik, Ca=Şamdan, Ch=Tahta sandalye, Cp=Pişirme kazanı, Cw=Sandık, Hs=Taş ocak, Ps=Kiler rafı, Sd=Bodrum merdiveni, Su=Ahşap merdiven (üst kat), Sw=Tahta tabure, Tw=Tahta masa, Wb=Su fıçısı
== Çiftlik evi kat -1 ==
+  +  +  +  +  +  +  +  +  +  +  
                                 
+  +--+--+--+--+--+--+--+--+--+  
   |Br  .  . Bb  . Sc  . Br  .|  
+  +  +  +  +  +  +  +  +  +  +  
   |Sb  . Bl  .  .  .  .  .  .|  
+  +  +  +  +  +  +  +  +  +  +  
   | .  .  .  .  .  .  .  .  .|  
+  +  +  +  +  +  +  +  +  +  +  
   | .  .  .  .  .  .  .  .  .|  
+  +  +  +  +  +  +  +  +  +  +  
   | .  .  .  .  .  .  .  .  .|  
+  +  +  +  +  +  +  +  +  +  +  
   |Br  .  .  .  .  .  .  . P<|  
+  +  +  +  +  +  +  +  +  +  +  
   | .  .  . W^ W^  .  .  . P<|  
+  +--+--+--+--+--+--+--+--+--+  
                                 
   Bb=Mayalama fıçısı, Bl=Ahır feneri, Br=Fıçı, Ps=Kiler rafı, Sb=Tuzlama fıçısı, Sc=Tohum kasası, Wo=Marangoz tezgâhı
```

## Köylü evi 1

### Önce
```
== home:1 kat 0 (önce) ==
+  +  +  +  +  +  
  .  .  .  .  .  .
+  +--+--+--+--+  
  .|St  .  .  .| .
+  +  +  +  +  +  
  .| .  .  .  .= .
+  +  +  +  +  +  
| .|Cw  . Bs Bs| .
+  +--+DD+--+--+  
| .  . ,,  .  .  .
   Bs=Saman yatak, Cw=Sandık, St=Tahta tabure
```
### Sonra
```
== Köylü evi 1 kat 0 ==
+  +  +  +  +  +  
  .  .  .  .  .  .
+  +--+--+--+--+  
  .|Sw Bs Bs  .| .
+  +  +  +  +  +  
  .| .  .  .  .= .
+  +  +  +  +  +  
| .|C^  . Cp H^| .
+  +--+DD+--+--+  
| .  . ,,  .  .  .
   Bs=Saman yatak, Cp=Pişirme kazanı, Cw=Sandık, Hs=Taş ocak, Sw=Tahta tabure
```

## Köylü evi 2

### Önce
```
== home:2 kat 0 (önce) ==
+  +  +  +  +  +  
  .  .  .  .  .  .
+  +--+--+--+--+  
  .|St  .  .  .| .
+  +  +  +  +  +  
  .| .  .  .  .= .
+  +  +  +  +  +  
  .|Cw  . Bs Bs|Tr
+  +--+DD+--+--+  
  .  . ,, ,,  .  .
   Bs=Saman yatak, Cw=Sandık, St=Tahta tabure, Tr=Armut ağacı
```
### Sonra
```
== Köylü evi 2 kat 0 ==
+  +  +  +  +  +  
  .  .  .  .  .  .
+  +--+--+--+--+  
  .| . Sw Bs  .| .
+  +  +  +  +  +  
  .| .  . Bs  .= .
+  +  +  +  +  +  
  .|H^  . Cp  .|Tr
+  +--+DD+--+--+  
  .  . ,, ,,  .  .
   Bs=Saman yatak, Cp=Pişirme kazanı, Hs=Taş ocak, Sw=Tahta tabure, Tr=Armut ağacı
```

## Köylü evi 3

### Önce
```
== home:3 kat 0 (önce) ==
+  +  +  +  +  +  
 ,, ,, ,,  .  .  .
+  +--+DD+--+--+  
 ,,|St  .  .  .| .
+  +  +  +  +  +  
 ,,| .  .  .  .= .
+  +  +  +  +  +  
 ,,|Cw  . Bs Bs| .
+  +--+--+--+--+  
 ,,  .  .  .  .  .
   Bs=Saman yatak, Cw=Sandık, St=Tahta tabure
```
### Sonra
```
== Köylü evi 3 kat 0 ==
+  +  +  +  +  +  
 ,, ,, ,,  .  .  .
+  +--+DD+--+--+  
 ,,|Sw  .  . Bf| .
+  +  +  +  +  +  
 ,,| .  .  . Bf= .
+  +  +  +  +  +  
 ,,|C^ Cp  . H<| .
+  +--+--+--+--+  
 ,,  .  .  .  .  .
   Bf=Tahta karyola, Cp=Pişirme kazanı, Cw=Sandık, Hs=Taş ocak, Sw=Tahta tabure
```

## Köylü evi 4

### Önce
```
== home:4 kat 0 (önce) ==
+  +  +  +  +  +  
  .  . ,,  .  .  .
+  +--+DD+--+--+  
  .|St  .  .  .| .
+  +  +  +  +  +  
  .| .  .  .  .= .
+  +  +  +  +  +  
  .|Cw  . Bs Bs| .
+  +--+--+--+--+  
  .  .  .  .  .  .
   Bs=Saman yatak, Cw=Sandık, St=Tahta tabure
```
### Sonra
```
== Köylü evi 4 kat 0 ==
+  +  +  +  +  +  
  .  . ,,  .  .  .
+  +--+DD+--+--+  
  .|Sw  . Bf  .| .
+  +  +  +  +  +  
  .| .  . Bf  .= .
+  +  +  +  +  +  
  .|Cp H^  .  .| .
+  +--+--+--+--+  
  .  .  .  .  .  .
   Bf=Tahta karyola, Cp=Pişirme kazanı, Hs=Taş ocak, Sw=Tahta tabure
```

## Köylü evi 5

### Önce
```
== home:5 kat 0 (önce) ==
+  +  +  +  +  +  
  .  . ,, ,, ,, ,,
+  +--+DD+--+--+  
  .|St  .  .  .|,,
+  +  +  +  +  +  
 ,,|,,  .  .  .= .
+  +  +  +  +  +  
  .|Cw  . Bs Bs| .
+  +--+--+--+--+  
  .  .  .  .  .  .
   Bs=Saman yatak, Cw=Sandık, St=Tahta tabure
```
### Sonra
```
== Köylü evi 5 kat 0 ==
+  +  +  +  +  +  
  .  . ,, ,, ,, ,,
+  +--+DD+--+--+  
  .|Sw  . Bs  .|,,
+  +  +  +  +  +  
 ,,| .  . Bs  .= .
+  +  +  +  +  +  
  .|Cp H^  .  .| .
+  +--+--+--+--+  
  .  .  .  .  .  .
   Bs=Saman yatak, Cp=Pişirme kazanı, Hs=Taş ocak, Sw=Tahta tabure
```

## Köylü evi 6

### Önce
```
== home:6 kat 0 (önce) ==
+  +  +  +  +  +  
  . ,, ,,  . ,, ,,
+  +--+DD+--+--+  
  .|St  .  .  .|Rb
+  +  +  +  +  +  
  .| .  .  .  .= .
+  +  +  +  +  +  
  .|Cw  . Bs Bs|Fr
+  +--+--+--+--+  
  .  .  .  .  .  .
   Bs=Saman yatak, Cw=Sandık, Fr=Böğürtlen çalısı, Rb=Yağmur fıçısı, St=Tahta tabure
```
### Sonra
```
== Köylü evi 6 kat 0 ==
+  +  +  +  +  +  
  . ,, ,,  . ,, ,,
+  +--+DD+--+--+  
  .| .  . Sw  .|Rb
+  +  +  +  +  +  
  .|Bs  .  . C<= .
+  +  +  +  +  +  
  .|Bs Cp H^  .|Fr
+  +--+--+--+--+  
  .  .  .  .  .  .
   Bs=Saman yatak, Cp=Pişirme kazanı, Cw=Sandık, Fr=Böğürtlen çalısı, Hs=Taş ocak, Rb=Yağmur fıçısı, Sw=Tahta tabure
```

## Dış mekân: şehir, köy yolu, çiftlik girişi

Aynı okuma. `::` çit ya da bahçe kapısı; ağaçlar `T?` (Tb huş, To meşe, Tp çam, Tw söğüt, Ta/Tc/Tr/Tl/Tn meyve), `Re` saz,
`Ss` dükkân tabelası, `Si` yol tabelası. Kutu çizilecek (görseli 432'de bekleyen) eşyalar da planda görünür.

### Şehir
Önce:
```
== Şehir (önce) ==
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . ,, ,, ,,  .  . To  .  .  .  .  .  .  .  .  . Tb  .  .  .  .  . ~~ ~~ ~~  .  .  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  . Tb  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . ,, ,,  .  .  .  .  .  .  . Tb  .  .  .  .  .  .  .  .  .  .  . ~~ ~~ ~~  .  .  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . ,, ,,  .  .  .  .  .  .  .  .  .  .  .  .  . To  .  .  .  .  . ~~ ~~ ~~  .  .  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . ,, ,, ,,  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . ~~ ~~ ~~  .  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+--+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+--+--+--+  +  +--+--+--+==+--+--+--+  +  +  +  +  +  +  +  +::+::+::+::+::+::+::+::+  +  
  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .|Sw  .  .  .| .  .  .  .  .  .  .  .  .  . ,, ,,  .  .  .  .|Sw  .  .  .| .  .|Mi Mi  .  . Qu  . Sc| .  .  . ~~ ~~ ~~  .  .: .  .  .  .  .  .  .  .: .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .| .  .  .  .= .  .  .  .  .  .  .  .  .  . ,, ,,  .  .  .  .| .  .  .  .= .  .|Mi Mi  .  .  .  . Ba|Ww Ww  . ~~ ~~ ~~  .  .: . Ft Ft  .  . Wt Wt  .: .  .
+  +  +  +  +  +  +  +--+--+--+==+--+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .|Ps Ps Ps  . Dr Dr  .| .|Cw  . Bs Bs| .  .  .  .  .  .  .  .  .  . ,, ,,  .  .  .  .|Cw  . Bs Bs|Tp  .| .  .  .  .  .  .  .|Ww Ww  . ~~ ~~ ~~  .  .: .  .  .  .  .  .  .  .: .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+DD+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+DD+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .| .  .  .  .  .  .  .| .  . ,,  .  .  .  .  .  .  .  .  .  .  .  . ,, ,,  .  .  .  .  . ,, ,,  .  .  .| .  .  .  .  . So So= .  .  .  . ~~ ~~ ~~  .: .  .  .  .  .  .  .  .: .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .| .  .  .  .  .  .  .|Ta  . ,,  .  . Gb Gb  .  .  .  .  .  .  .  . ,, ,,  . ,, ,, ,, ,, ,,  .  .  .  .| . Tw Tw  .  .  .  .| .  .  .  . ~~ ~~ ~~  .: .  .  .  .  .  .  . Sr: .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .|Tw Tw  .  .  . Bs Bs= .  . ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,,  .  .  .  . ,,  .  .  .  .  .| .  .  .  .  .  .  .| .  .  .  . ~~ ~~ ~~  .: .  .  .  .  .  .  .  .: .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+==+--+DD+==+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .| .  .  .  .  .  .  .| .  .  .  .  .  .  .  .  .  .  .  . ,,  .  . ,, ,,  .  .  .  . ,,  .  .  .  .  .  .  .  . ,,  .  .  .  .  .  .  . ~~ ~~ ~~  .: . Hb Hb  .  .  . Ms  .: .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .| .  .  .  .  .  . Be| .  .  .  .  .  .  .  .  .  .  .  .  .  .  . ,, ,,  . ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,,  .  .  .  .  .  .  . ~~ ~~ ~~  .: .  .  .  .  .  .  .  .: .  .
+  +  +  +  +  +  +  +--+==+--+DD+==+--+--+  +  +  +  +  +  +  +--+--+--+--+--+==+--+--+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  . ,,  .  .  .  .  .  .  .  .  .  .|Hs Cp Pc Pc  .  .  . Bb Bb Bb| .  .  .  .  .  .  .  . ,,  .  .  .  .  .  .  .  .  .  .  .  . ~~ ~~ ~~  .:Bl  .  .  .  .  .  .  .: .  .
+  +  +  +  +  +  +  +  +  +  +  +  +::+::+::+::+::+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +::+::+::+::+::+::+::+::+  +  
  .  .  .  .  .  .  .  .  .  . ,,  .: .  .  .  . Bh: .  .  .  .|Fb  .  .  .  .  .  .  . ,, Ba|,, ,,  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . ~~ ~~ ~~  .  .  .  . ,,  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+--+--+--+==+--+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  . ,, ,,: . Fh  . Fh  .: .  .  .  .| . Bn Bn  .  . Bn Bn  . ,, Ba|,, ,,  .|Pa Pa Sj Sj  . Pa Pa Sc| .  .  .  .  .  . Tr  .  .  . ~~ ~~ ~~  .  .  .  . ,,  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  . ,, ,,  .: .  .  .  .  .: .  .  .  .| . Tt Tt Tt  . Tt Tt Tt  .  .=,, ,,  .| .  .  .  .  .  .  .  .| .  .  .  .  .  .  .  .  .  . ~~ ~~ ~~  .  .  .  . ,,  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  . ,,  .: . Fh  . Fh  .: .  .  .  .| . Bn Bn  .  . Bn Bn  .  .  .|,, ,,  .| .  .  .  .  .  .  . Ba| .  .  .  .  .  .  .  .  .  . ~~ ~~ ~~  .  .  .  . ,,  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +::+::+::+::+::+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  . ,,  .  .  .  .  .  .  .  .  .  .| .  .  .  .  .  .  .  .  . Ss|,, ,,  .| .  . Pc Pc  .  .  . Ba= .  .  .  .  .  .  .  .  .  . ~~ ~~ ~~  .  .  .  . ,,  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+--+--+==+--+--+  +  +  +  +  
  . ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,,  .  .  .  .  .  .  .  .  .|Ca  .  .  .  .  .  .  .  .  .|,, ,,  .|Fb  .  .  .  .  .  .  .| .  .  .  .  .  .  .  .  .  . ~~ ~~ ~~| .  . Sh Sh  . Ba| .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+==+--+--+==+DD+--+==+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  . ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,,  .  .  .  .  .  .  .  .  .  .  .  .  .  . ,,  .  .  . ,, ,, ,,  .| .  .  .  .  .  .  .  .| .  .  .  .  .  .  .  .  . ~~ ~~ ~~  .= .  .  .  .  . Sb| .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+==+--+DD+==+--+--+==+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  . ,,  .  . ,, ,,  .  .  .  .  .  .  .  .  .  .  .  .  .  . ,,  .  .  .  . ,, ,,  .  .  .  . ,,  .  .  .  .  .  .  .  .  .  .  .  .  . ~~ ~~ ~~ ,,D .  .  .  .  .  .| .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  . ,,  . ,, ,, ,, ,, ,, ,, ,, ,, ,, ,,  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . ,,  .  .  .  .  .  .  .  .  .  .  .  . Tr ~~ ~~ ~~ ,,| .  .  . Tw Tw  .| .  .  .  .  .
+  +  +  +  +--+==+--+DD+==+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .|Fo Fo  .  .  .  . Fb|,, ,, ,, ,, ,, ,, ,, ,,  .  .  .  .  .  .  . Ma Ma  .  .  .  .  .  . Gb Gb  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . ~~ ~~ ~~ ,,= . Fi  .  .  .  .| .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+--+--+--+--+--+  +  +  +  +  
  .  .  .  .|Fo Fo  .  .  .  .  .| .  . ,, ,, ,,  . ,, ,,  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . ~~ ~~ ~~ ,,  .  .  .  . ,,  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .| .  .  .  .  . Bo Bo| .  . ,, ,,  .  . ,, ,,  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . ~~ ~~ ~~  . ,,  .  .  .  . ,,  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .| .  .  .  .  . Bo Bo= . ,, ,, ,,  .  . ,, ,, ,, ,, ,, ,, ,,  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . ~~ ~~ ~~  . ,,  .  .  .  . ,,  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .|Th Th  .  . Wh  . Wb| .  . ,, ,,  .  . ,, ,, ,, ,, ,, ,, ,,  .  .  .  .  .  .  . We  .  .  .  .  .  .  . ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,,  .  .  . ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,,  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  . To  .  .| .  .  .  .  .  .  .| .  . ,, ,,  .  .  .  .  .  .  . ,,  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,,  .  .  . ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,,  .
+  +  +  +  +--+--+--+--+--+--+--+  +  +  +  +--+--+--+==+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  . ,, ,,|Sh Sh Sh Sh  . Ca| .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . ,,  .  . ~~ ~~ ~~  .  .  .  .  .  . ,,  .  .  .  . ,,  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  . To  .  .  .  .  .  .  .  .  .  . ,, ,,| .  .  .  .  .  .= .  .  .  .  .  . Gb Gb  .  .  .  .  .  . Bf  .  .  .  .  .  .  .  .  .  .  .  .  . ~~ ~~ ~~  .  .  .  .  .  . ,,  .  .  .  . ,,  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  . ,, ,,| .  . Tl Tl  .  .D,, ,, ,, ,,  .  .  .  .  .  .  .  .  .  .  .  .  .  . ,,  .  .  .  .  .  .  . Tr  . ~~ ~~ ~~  .  .  .  .  .  . ,,  .  .  .  . ,,  .  .
+  +  +  +  +--+--+--+--+==+--+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .|Wo Wo  . Wo Wo Fb  .  .| . ,, ,,| .  . Cc  .  .  .| .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . ,, ,, ,,  .  .  .  .  .  .  .  .  . ~~ ~~ ~~  .  . Tr  .  .  . ,,  .  .  .  . ,,  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+==+DD+--+==+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .| .  .  .  .  .  .  .  .= . ,, ,,|Cw  .  .  .  .  .= .  .  .  .  .  .  . ,,  .  . ,, ,,  .  .  .  .| .  .  .  .  .  .  .| .  .  .  . ~~ ~~ ~~  .  .  .  .  .  .  . ,,  .  .  .  . ,,  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+--+--+--+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .| .  .  .  .  .  .  .  .D,, ,, ,,  .  .  .  .  .  .  .  .  . ,, ,, ,, ,, ,, ,, ,, ,, ,,  .  .  .  .| .  .  .  . Tw Tw Cw| .  .  .  . ~~ ~~ ~~  .  .  .  .  .  .  . ,,  .  .  .  . ,,  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+==+--+DD+==+--+--+  +  +  +  
  .  .  .  .| .  .  .  .  .  .  .  .| . ,, ,,  .  .  .  .  .  .  .  .  . ,,  .  .  .  .  .  . ,, ,,  .  .  .  .|Lo Lo  .  .  .  .  .| .  .  .  . ~~ ~~ ~~  .  .  .  .|Sa Sa  .  .  . Sr Sr| . ,,  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+DD+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .|Cb Cb Tl Tl  . Ch Cc  .= . ,, ,, ,, ,,  .  .  .  .  .  .|Sw  .  .  .| .  .  .  . ,, ,,  .  .  .  .| .  .  .  .  .  .  .= .  .  .  . ~~ ~~ ~~  .  .  .  .| .  .  .  .  .  .  .| . ,,  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+DD+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .| .  .  .  .  .  .  .  .| . ,, ,,|Sw  .  .  .| .  .  .  .| .  .  .  .= .  .  .  . ,, ,,  .  .  .  .|Sp  . Sw  .  .  . Dv| .  .  .  . ~~ ~~ ~~  .  .  .  .| .  .  .  .  .  .  .| . ,,  .  .
+  +  +  +  +--+--+--+--+--+--+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  . ,, ,,| .  .  .  .= .  .  .  .|Cw  . Bs Bs| . Tc  .  . ,, ,,  .  .  .  .| .  .  .  . Cl Cl  .| .  .  .  . ~~ ~~ ~~  .  .  .  .| .  .  .  .  .  .  .= . ,,  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+--+--+--+  +  +  +  +  +  +  +  +  +  +--+--+--+--+--+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  . Sm Sm  . Fb  .  .  .  . ,, ,,|Cw  . Bs Bs| . Te  .  .  .  .  .  .  .  .  .  . ,, ,, ,, ,, ,,  .  .  .  .  .  .  .  .  .  .  .  . ~~ ~~ ~~  .  .  .  .| .  .  .  . Tw Tw  .| . ,,  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+--+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  . Sm Sm  .  .  .  .  .  . ,, ,,  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,,  .  .  .  .  . ~~ ~~ ~~  .  .  .  .| .  .  .  .  .  . Ba| . ,,  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+==+DD+--+==+--+  +  +  +  +  +  +  +  +  +--+--+--+--+--+--+--+  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  . ,, ,,  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . ,,  . ,, ,,  .  .  .  .|Bt  .  . Sh Sh  .| .  . ~~ ~~ ~~  .  .  .  .  .  .  .  .  .  .  .  . ,,  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  . ,, ,,  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . ,, ,,  .  .  .  .| .  .  .  .  .  .| .  . ~~ ~~ ~~  .  .  .  .  . Tn Tn  . Tn Tn  . Bt ,,  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+--+--+==+--+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  . To  .  .  .  . ,, ,, ,, ,, ,, ,, ,, ,, ,,  .  .  .  .  .  .|St St  . Wo Wo  .  .| .  .  .  .  .  . ,, ,,  .  .  .  .|Pw  .  .  .  .  .| .  . ~~ ~~ ~~  .  .  .  .  .  .  .  .  . ,,  . ,, ,,  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  . ,, ,,  . ,, ,,  .  .  . ,, ,,  .  .  .  .  .  .| .  .  .  .  .  .  .= .  .  . ,,  . ,, ,, ,,  .  .  .  .| .  .  .  .  .  .= . ,, ~~ ~~ ~~  .  .  .  .  .  .  . ,, ,, ,, ,, ,, ,,  .  .
+  +  +  +  +  +--+DD+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+DD+--+--+  +  +  +  +  
  .  .  .  .  .|Sw  .  .  .|Rb  .  .  . ,, ,, ,,  .  .  .  .  .| .  .  .  .  .  .  .D,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,,|,, ,, ,, ,, Ki Ki|,, ,, ,,  .  .  .  .  .  .  .  .|Sw  .  .  .|,, ,,  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .| .  .  .  .= .  .  .  .  .  .  .  .  .  .  .  .| .  .  .  .  .  .  .| .  . ,,  .  .  . ,, ,,  .  .  .  .|Sj Sj  .  . Ki Ki| .  . ,,  .  .  . ,, ,, ,, ,, ,,|,,  .  .  .= .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+--+--+--+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .|Cw  . Bs Bs|Fr  .  .  .  .  .  .  .  .  .  .  .|Tw Tw  .  .  .  .  .= .  .  .  .  .  . ,, ,,  .  .  .  .  .  .  .  .  .  .  .  .  . ~~ ~~ ~~  .  .  .  .  .|Cw  . Bs Bs| .  .  .  .  .
+  +  +  +  +  +--+--+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+--+--+--+  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . Lk Lk  .  .| .  .  .  .  .  . Ba| .  .  .  .  .  . ,, ,,  .  .  .  .  .  .  .  .  .  .  .  .  . ~~ ~~ ~~  .  .  .  .  .  .  .  .  .  . Ta  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+--+--+--+--+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . Lk Lk  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . ,, ,,  .  .  .  .  .  .  .  .  .  .  . Tr  . ~~ ~~ ~~  .  .  .  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  . Tb  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . ,, ,,  .  .  .  .  .  .  .  .  .  .  .  .  .  . ~~ ~~ ~~ Tr  .  .  .  .  . To  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . ,, ,, ,,  .  .  .  .  .  .  .  .  .  .  .  .  .  . ~~ ~~ ~~  .  .  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . ,, ,,  .  .  .  .  .  .  .  .  .  .  .  .  .  . ~~ ~~ ~~  .  .  .  .  . To  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .
   Ba=Fıçı, Bb=Mayalama fıçısı, Be=Leğen ve ibrik, Bf=Bekçi ateşi, Bh=Arı kovanı, Bl=Ahır feneri, Bn=Uzun bank, Bo=Çiçek ocağı, Bs=Saman yatak, Bt=Fıçı küvet, Ca=Şamdan, Cb=Sandık-bank, Cc=Oymalı sandalye, Ch=Boyalı sandalye, Cl=Elbise dolabı, Cp=Bakır kazan seti, Cw=Sandık, Dr=Kurutma askısı, Dv=Boya kazanı, Fb=Odun sepeti, Fh=Yabani otlar, Fi=Balık sepeti (tuzak), Fo=Demir ocağı ve örs, Fr=Böğürtlen çalısı, Ft=Yemlik, Gb=Bahçe bankı, Hb=Ot tınazı, Hs=Taş ocak, Ki=Tuğla fırını, Lk=Kireç ocağı, Lo=Dokuma tezgâhı, Ma=Pazar tezgâhı, Mi=Değirmen taşı, Ms=Üç ayaklı tabure, Pa=Kiler rafı, Pc=Hazırlık tezgâhı, Ps=İksir rafı, Pw=Çömlekçi çarkı, Qu=El değirmeni, Rb=Yağmur fıçısı, Sa=Saraç tezgâhı, Sb=Tuzlama fıçısı, Sc=Tohum kasası, Sh=Raf, Sj=Küp, Sm=Su hızarı, So=Taş fırın, Sp=Çıkrık, Sr=Eyer sehpası, Ss=Şarkıcı kürsüsü, St=Taş yontma tezgâhı, Sw=Tahta tabure, Ta=Elma ağacı, Tb=Huş, Tc=Kiraz ağacı, Te=Erik ağacı, Th=Su çekici, Tl=Boyalı masa, Tn=Tabak germesi, To=Meşe, Tp=Armut ağacı, Tr=Söğüt, Tt=Sehpa masa, Tw=Tahta masa, Wb=Su fıçısı, We=Kuyu, Wh=Bileği taşı, Wo=Marangoz tezgâhı, Wt=Su yalağı, Ww=Su çarkı
```
Sonra:
```
== Şehir (bölge) ==
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  . To  .  .  .  .  .  .  .  .  .  .  .  .  . Tb  .  .  .  . Tp  .  .  .  .  .  .  . ,, ,, ,,  .  .  .  . To  .  . Tc  .  .  .  .  .  .  .  .  .  . ~~ ~~ ~~  .  .  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  . To  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . ,, ,,  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . To  .  .  . ~~ ~~ ~~ Re  .  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . Tb  .  .  . ,, ,,  .  . To  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . ~~ ~~ ~~  .  .  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  . Tb  .  .  .  .  .  .  .  .  .  .  .  .  .  . Cl Cl  .  .  .  .  .  .  .  .  .  . ,, ,, ,,  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . ~~ ~~ ~~  .  .  .  .  . To  .  . To  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+--+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+--+--+--+  +  +--+--+--+==+--+--+--+  +  +  +  +  +  +  +  +::+::+::+::+::+::+::+::+  +  
  .  .  .  .  . Tb  .  .  .  .  .  .  .  .  .|Sw Bs Bs  .| .  .  .  .  .  .  .  .  .  . ,, ,,  .  .  . G<| . Sw Bs  .| .  .| . Su Su Mi Mi Fs Fs|Wo Tw  . ~~ ~~ ~~  .  .: .  .  .  .  .  .  .  .: .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  . Rb  .  .  .  .  .  .  .| .  .  .  .= .  .  .  .  .  .  .  .  .  . ,, ,,  . Tr  . G<| .  . Bs  .=Wo Wo| .  .  . Mi Mi  . Sc|Wo  .  . ~~ ~~ ~~ Re  .:Ms  .  .  .  .  .  .  .: .  .
+  +  +  +  +  +  +  +--+--+--+==+--+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .| . Su Su Dr  .  .  .| .|C^  . Cp H^| .  .  .  .  .  .  .  .  .  . ,, ,,  .  .  .  .|H^  . Cp  .| .  .| .  .  .  .  .  . K<|Gs  .  . ~~ ~~ ~~  . Sh: .  .  .  .  .  .  .  .: .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+DD+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+DD+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .| .  .  . Dr  .  . P<| .  . ,,  .  .  .  .  .  .  .  .  .  .  .  . ,, ,,  .  .  .  .  . ,, ,,  .  .  .|Sd  .  .  .  .  . K<= .  . Re  . ~~ ~~ ~~  .: .  .  .  .  . Bl  .  .: .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  . To  .  .  .| .  . Ta Sw  .  . B<| .  . ,,  .  .  . ,,  .  .  .  .  .  .  .  . ,, ,,  . ,, ,, ,, ,, ,,  .  .  .  .|Sd  .  .  .  .  . S<| .  .  .  . ~~ ~~ ~~ Re: .  .  .  .  .  .  . Wt: .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .|Sd  . Ta  .  .  .  .= .  . ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,,  . ,,  .  . ,,  .  .  .  .  .|Qu  .  .  . B^  . S<| .  .  .  . ~~ ~~ ~~  .: . Sr Hb  .  .  .  . Wt: .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+==+--+DD+==+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .|Sd  .  .  .  .  .  .| .  .  .  .  .  .  .  .  .  . Hp  . ,,  .  . ,, ,, ,, ,,  .  . ,,  .  .  .  .  .  .  .  . ,,  .  .  .  .  .  .  . ~~ ~~ ~~  .: .  .  .  .  .  .  .  .: .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .|Bs Bs  .  .  . P^  .| .  .  .  .  .  .  .  .  . G^ G^  .  . Sw  . ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, Ss  .  .  .  .  .  . ~~ ~~ ~~  .:Hb Ft Ft  .  .  .  .  .: .  .
+  +  +  +  +  +  +  +--+==+--+DD+==+--+--+  +  +  +  +  +  +  +--+--+--+--+--+==+--+--+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  . ,,  .  .  .  .  .  .  .  .  .  .| .  .  . Sd Sd Bb  .  .  . Bb|,, ,, Si  .  .  .  .  . ,,  .  .  .  .  .  .  .  .  .  .  . Tw ~~ ~~ ~~  .: .  .  .  .  .  .  .  .: .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +::+::+::+::+::+::+::+::+  +  
  .  . To  .  .  .  .  .  . Ss ,,  .  .  .  .  .  .  .  .  . Ba|Su  .  . Sw  .  .  .  .  .  .|,, ,,  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . ~~ ~~ ~~  .  .  .  . ,,  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +::+::+::+::+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+--+--+--+==+--+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  . ,,: . Fh Fh Fh: .  . Bh  .  . Ba|Su  .  . Ht  . Ht  .  .  .  .|,, ,,  .| . Su Su Sc  . Sd Sd Wb| .  .  .  .  .  .  .  .  .  . ~~ ~~ ~~  . Tw  .  . ,,  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  . To  .  .  .  .  .  . ,, ,,: . Fh Fh Fh: .  .  .  .  .  .|Ba  . Sw Ht H< Ht H<  .  . Co=,, ,,  .| .  .  .  .  .  .  . P<| .  .  .  .  .  .  .  .  . Tw ~~ ~~ ~~  .  .  .  . ,,  .  . To  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +::+::+::+::+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  . To  .  .  .  .  . ,,  .  .  .  .  .  .  .  .  . Wo| .  .  . Ht H< Ht H<  .  . H<|,, ,,  .|Sj  .  .  .  .  .  . P<| .  .  .  .  .  . Tw  .  .  . ~~ ~~ ~~  .  .  .  . ,,  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  . Tb  .  .  .  .  . Si  .  . ,,  .  .  .  .  .  .  .  .  . Wo|Ba  .  .  .  .  .  .  .  . Fb|,, ,,  .|Ba  . Sp Sp  .  .  .  .= .  .  .  .  .  .  .  .  .  . ~~ ~~ ~~ Nd Nd  .  . ,,  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+--+--+==+--+--+  +  +  +  
  . ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,,  .  .  .  .  .  .  .  .  .|Sn  . P^ P^  .  .  .  .  . Ca|,, ,,  .|Ba  .  .  .  .  .  .  .| .  . To  .  .  .  .  .  . Re ~~ ~~ ~~ Fi|Fa Ta  . Su Su Sb| .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+==+--+--+==+DD+--+==+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  . ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,,  .  .  .  .  .  .  .  .  .  .  .  .  .  . ,,  .  .  . ,, ,, ,,  .|Sj  . Wb  .  . P^ P^ Ca| .  .  .  .  .  .  .  .  . ~~ ~~ ~~ Ss  .=Ba Ta  .  .  .  .|Ba  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+==+--+DD+==+--+--+==+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  . To  .  .  .  .  . ,, Ss  . ,, ,,  . Si  .  .  .  .  .  .  .  . Sw  .  . Ss ,,  .  .  .  . ,, ,,  .  .  .  . ,,  .  .  .  .  .  .  .  .  . To  .  .  . ~~ ~~ ~~ ,, ,,D .  .  .  .  . Sd| .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  . ,,  . ,, ,, ,, ,, ,, ,, ,, ,, ,, ,,  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . ,, Ss  .  .  .  .  .  .  .  .  .  .  .  . ~~ ~~ ~~ ,,  .| .  .  .  .  . Sd| .  .  .  .
+  +  +  +  +--+==+--+DD+==+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .| .  .  .  .  . Wh  .|,, ,, ,, ,, ,, ,, ,, ,, Bt Tb  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . Te  .  .  .  .  .  .  .  .  .  . ~~ ~~ ~~ ,,  .=Fa  .  .  .  .  .| .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+--+--+--+--+--+  +  +  +  
  .  .  .  .| .  .  .  .  .  .  .|Sm  . ,, ,, ,,  . ,, ,,  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . G<  .  .  .  .  .  .  .  .  .  .  .  . ~~ ~~ ~~ ,,  .  .  .  . ,,  .  .  .  . To  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .| .  .  .  .  .  .  .| .  . ,, ,,  .  . ,, ,,  . Tb  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . G<  .  .  .  .  .  .  .  .  .  .  . ~~ ~~ ~~  . ,,  .  .  .  . ,,  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  . To  . Wo|Ip  .  .  .  .  .  .= . ,, ,, ,,  .  . ,, ,, ,, ,, ,, ,, ,,  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . ~~ ~~ ~~  . ,,  .  .  . Si ,,  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  . Wo| .  . Qt F^ F^ A^  .| .  . ,, ,,  .  . ,, ,, ,, ,, ,, ,, ,,  .  .  .  .  .  .  . We Wa  .  .  .  .  .  . ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,,  .  .  . ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,,  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  . To  .  .| .  . Fb F^ F^ Bo  .| .  . ,, ,,  .  .  . G^ G^  .  . ,,  .  .  .  .  .  .  .  . Ws  .  .  .  .  .  .  . ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,,  .  .  . ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,,  .
+  +  +  +  +--+--+--+--+--+--+--+  +  +  +  +--+--+--+==+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  . Wr  .  .  .  .  .  .  . ,, ,,| . Su Su  .  . W<| .  . Tc  .  .  .  .  .  .  .  .  .  .  .  .  .  . G<  .  .  .  .  .  .  . ,,  .  . ~~ ~~ ~~  .  .  .  .  .  . ,,  .  .  .  . ,,  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  . ,, ,,| . Tl C<  .  . C<= . Ss  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . G<  .  .  .  .  .  .  .  .  .  . ~~ ~~ ~~  .  .  .  .  .  . ,, Tb  .  .  . ,,  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  . ,, ,,| . Tl  .  .  .  .D,, ,, ,, ,, Bn  .  .  .  .  .  .  .  .  .  .  .  .  . ,, Ss  .  .  .  .  .  .  .  . ~~ ~~ ~~  .  .  .  .  .  . ,,  .  .  . To ,,  .  .
+  +  +  +  +--+--+--+--+==+--+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  . Lp| . Su Su  .  . Lp  . W<|Ss ,, ,,| .  .  .  .  . S<| .  .  .  .  . Np  .  . G^ G^  .  .  . M^ M^  . ,, ,, ,,  .  .  .  .  .  .  .  .  . ~~ ~~ ~~  .  .  .  .  .  . ,,  .  .  .  . ,,  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+==+DD+--+==+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  . Lp| . Uf  .  .  . Lp  . W<= . ,, ,,| . Sd Sd S^ S^  .= .  .  .  .  .  .  . ,,  .  . ,, ,,  .  .  .  .|Ga  .  .  .  . Yb  .| .  .  . Re ~~ ~~ ~~  .  .  .  .  .  . Ss ,,  .  .  .  . ,,  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+--+--+--+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .| .  .  .  .  .  .  .  .D,, ,, ,,  .  .  .  .  .  .  .  .  . ,, ,, ,, ,, ,, ,, ,, ,, ,,  .  .  .  .| .  .  .  .  .  . S<| .  .  .  . ~~ ~~ ~~  .  .  .  .  .  .  . ,,  .  .  .  . ,,  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +::+::+::+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+==+--+DD+==+--+--+  +  +  +  
  .  .  .  .| .  .  .  .  .  .  . C<| . ,, ,, Si  .  .  .  .  .  .  .  . ,,  .  .  .: . Cr Cr:,, ,,  .  .  .  .| .  .  . C<  .  . L<| .  .  .  . ~~ ~~ ~~  .  .  .  .|Hr  . Sr  .  .  . Sr| . ,,  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+DD+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .| .  .  .  . Tl Tl  .  .= . ,, ,, ,, ,,  .  .  .  .  .  .|Sw  . Bf  .| .:Cr Cr Cr:,, ,,  .  .  .  .|Su  .  . C< Sw  . L<= .  .  .  . ~~ ~~ ~~ Re  .  .  .|Bu  .  .  .  .  .  .| . ,,  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+DD+--+--+  +  +  +  +  +  +  +  +  +::+::+::+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .| .  .  .  . C^ C^ W^ W^| . ,, ,,|Sw  .  . Bf| .  .  .  .| .  . Bf  .= .  .  .  . ,, ,,  .  .  .  .|Su  .  .  .  .  .  .| .  .  . Re ~~ ~~ ~~  .  .  .  .| .  .  .  .  .  .  .| . ,,  .  .
+  +  +  +  +--+--+--+--+--+--+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  . Sa Sa  .  . Wo Wo  .  .  .  . ,, ,,| .  .  . Bf= .  .  .  .|Cp H^  .  .|Rb  .  .  . ,, ,,  . Si  .  .| .  . C^ C^  . C^ C^| . Sh  .  . ~~ ~~ ~~  .  .  .  .|Su  .  .  .  .  .  .= . ,,  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+--+--+--+  +  +  +  +  +  +  +  +  +  +--+--+--+--+--+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  . To  . Sa Sa  .  .  .  .  .  .  .  . ,, ,,|C^ Cp  . H<|Rb  .  .  .  .  . Cl  .  .  .  .  . ,, ,, ,, ,, ,,  .  . Cd Cd  . Ss  .  .  .  .  .  . ~~ ~~ ~~ Tw  .  .  .|Su  .  . Ta  .  . A<| . ,,  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+--+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  . ,, ,,  .  .  .  .  . Te  .  . Ab  . Cl  .  .  .  .  . ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,,  .  .  .  .  . ~~ ~~ ~~  .  .  .  .| .  .  . Ta  .  . A<| . ,,  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +::+::+::+::+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+==+DD+--+==+--+  +  +  +  +  +  +  +  +  +--+--+--+--+--+--+--+  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  . ,, ,,: . Ac Ac Ac: .  .  .  .  .  .  .  .  .  .  .  .  . ,,  . ,, ,,  .  .  . Pd|Pd Pd  .  . Cy Pd| .  . ~~ ~~ ~~  .  .  . Tn Tn Ba  .  .  . Tn Tn  . ,,  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  . Tb  .  . To  .  .  .  .  .  .  .  . ,, ,,:Ac  . Ac Ac: .  .  .  .  .  .  .  .  .  .  .  .  .  .  . ,, ,,  .  .  . Pd|Sj  .  . Sw  . Pd| . Re ~~ ~~ ~~  .  . Tw  .  .  .  .  .  .  .  .  . ,,  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +::+::+::+::+  +  +--+--+--+==+--+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  . ,, ,, ,, ,, ,, ,, ,, ,, ,,  .  .  .  .  .  .| .  . St  .  . Wh  .| .  .  .  .  .  . ,, ,,  .  .  .  .| .  .  .  .  .  .| .  . ~~ ~~ ~~  .  .  .  .  .  .  .  .  . ,,  . ,, ,,  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  . ,, ,,  . ,, ,,  .  .  . ,, ,,  .  .  .  .  .  .|St  .  .  .  .  .  .= . St  . ,,  . ,, ,, ,,  .  .  . Cy|Su  .  .  .  .  .= . ,, ~~ ~~ ~~  .  . Tw  .  .  .  . ,, ,, ,, ,, ,, ,,  .  .
+  +  +  +  +  +--+DD+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+DD+--+--+  +  +  +  +  
  .  .  .  .  .| .  . Sw  .| .  .  .  . ,, ,, ,,  .  .  .  .  .| .  .  .  .  .  .  .D,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,,|Su  .  .  .  .  .|,, ,, ,,  .  .  .  .  .  .  .  .|Sw  . Bs  .|,, ,,  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  . To  .  .  .|Bs  .  . C<= .  .  .  .  .  .  .  .  .  .  .  .| .  .  .  .  .  .  .| . Ss ,,  .  .  . ,, ,,  .  .  .  .| .  . P^  .  .  .| . Po ,,  .  .  . ,, ,, ,, ,, ,,| .  . Bs  .= .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+--+--+--+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +::+::+::+  
  .  .  .  .  .|Bs Cp H^  .|Wo  .  .  .  .  .  .  .  .  .  .  .| .  .  .  .  .  .  .= .  .  .  . To  . ,, ,,  .  .  .  .  .  .  .  .  .  . K> K>  . ~~ ~~ ~~ Re  .  .  .  .|Cp H^  .  .|Wo: . Cn Cn: .
+  +  +  +  +  +--+--+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+--+--+--+  +  +  +  +  
  .  .  .  .  .  .  . Cl Rb Wo  .  .  .  .  .  .  . Tb  .  .  .| .  . A^ A^  . W^ W^| .  .  .  .  .  . ,, ,,  .  .  .  .  .  .  .  .  . To K> K>  . ~~ ~~ ~~  .  .  .  .  .  .  .  .  . Wo:Cn Cn  .: .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +--+--+--+--+--+--+--+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +::+::+::+  
  .  .  .  .  .  .  . Cl Tc  .  . Tb  .  .  .  .  .  .  .  .  .  .  .  . St  . Lk Lk  .  .  .  .  .  . ,, ,, To  .  .  .  . Tb  .  .  .  .  .  .  . ~~ ~~ ~~  .  .  .  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  .  . Tb  .  .  .  .  .  .  .  .  .  .  . Lk Lk  .  . Tr  .  .  . ,, ,,  .  .  . Tb  .  .  .  .  .  .  .  .  .  . ~~ ~~ ~~  .  . Tb  .  .  . Tb  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . To  .  .  .  .  .  .  .  .  .  .  . ,, ,, ,,  .  .  .  .  .  .  .  .  .  .  .  .  . Re ~~ ~~ ~~  .  .  .  .  .  .  .  .  .  . Tb  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . ,, ,,  .  .  .  .  .  .  .  .  .  .  .  .  .  . ~~ ~~ ~~  .  .  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .
   Aa=Saraç tezgâhı, Ab=Elma ağacı, Ac=Havuç ekini, Ad=Taş yontma tezgâhı, An=Örs, Ba=Fıçı, Bb=Mayalama fıçısı, Be=Leğen ve ibrik, Bf=Tahta karyola, Bh=Arı kovanı, Bl=Ahır feneri, Bn=Bekçi ateşi, Bo=Körük, Br=Ekmek rafı, Bs=Saman yatak, Bt=Fıçı masa, Bu=Fıçı küvet, Ca=Şamdan, Cb=Kumaş topları rafı, Cc=Oymalı sandalye, Cd=Kurumaya asılmış boyalı kumaşlar, Ch=Elbise dolabı, Cl=Çamaşır ipi, Cn=Soğan ekini, Co=Bakır kazan seti, Cp=Pişirme kazanı, Cr=Pırasa ekini, Ct=Kesim masası, Cw=Sandık, Cy=Kil teknesi, Dr=Kurutma askısı, Fa=Balık sepeti, Fb=Odun sepeti, Fh=Yabani otlar, Fi=Balık sepeti (tuzak), Fo=Demir ocağı ve örs, Fs=Un çuvalı, Ft=Yemlik, Ga=Elbise askısı, Gb=Bahçe bankı, Gs=Tahıl çuvalı, Ha=Han sırası, Hb=Ot tınazı, Hp=At bağlama direği, Hr=Deri askısı, Hs=Taş ocak, Ht=Uzun han masası, Ip=Nal ve demir yığını, Ki=Tuğla fırını, Kt=Hamur teknesi, Lk=Kireç ocağı, Lo=Dokuma tezgâhı, Lp=Kereste yığını, Ma=Pazar tezgâhı, Mi=Değirmen taşı, Ms=Üç ayaklı tabure, Nd=Ağ kurutma askısı, Np=İlan tahtası, Pa=Kiler rafı, Pc=Hazırlık tezgâhı, Pd=Kuruyan çömlek rafı, Po=Çömlek sergisi, Ps=İksir rafı, Pw=Çömlekçi çarkı, Qt=Su teknesi (demirci), Qu=El değirmeni, Rb=Yağmur fıçısı, Re=Saz öbeği, Sa=Su hızarı, Sb=Tuzlama fıçısı, Sc=Tohum kasası, Sd=Bodrum merdiveni, Se=Raf, Sg=Çıkrık, Sh=Çalı, Si=Yol tabelası, Sj=Küp, Sl=Rulo rafı, Sm=Nal ve alet sergisi, Sn=Şarkıcı kürsüsü, So=Taş fırın, Sp=Satış tezgâhı, Sr=Eyer sehpası, Ss=Dükkân tabelası, St=Taş blokları, Su=Ahşap merdiven (üst kat), Sw=Tahta tabure, Ta=Tahta masa, Tb=Huş, Tc=Kiraz ağacı, Te=Erik ağacı, Tl=Boyalı masa, Tn=Tabak germesi, To=Meşe, Tp=Armut ağacı, Tr=Çam, Tw=Söğüt, Uf=Yarım kalmış dolap, Wa=Su kovası, Wb=Hasır sepet, We=Kuyu, Wh=Bileği taşı, Wk=Marangoz tezgâhı, Wl=Yazı kürsüsü (rahle), Wo=Odunluk, Wr=Su fıçısı, Ws=Çamaşır leğeni, Wt=Su yalağı, Yb=İplik ve yumak sepeti
```

### Köy yolu
Önce:
```
== Köy yolu (önce) ==
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  . ,,  .  .  .  .  .  . To  .  .  .  .  . ~~ ~~  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  . To ,,  .  .  . To  .  .  .  .  .  .  .  . ~~ ~~  . Tr  . To  .  .  .  .  .  . To  .  .  . Tb To  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  . ,, Tb  .  .  .  .  .  .  .  .  .  .  .  . ~~ ~~  .  .  .  .  .  .  . To  .  .  .  .  .  . Tb  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  . To  .  .  .  .  .  . ,,  . To  .  . To  .  .  .  . Tr  .  .  . ~~ ~~  .  .  . To  .  .  . To  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  . ,,  . Tb  .  .  .  .  .  . To  .  .  .  . ~~ ~~  .  . To To  .  .  .  .  .  .  . To  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  . Tb  .  .  . To  .  . ,,  .  .  . To To  .  .  .  .  .  .  .  . ~~ ~~  . Tr  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  . To  . To ,, ,,  .  .  .  . To  .  .  .  .  .  .  .  . ~~ ~~  . Tb  .  .  . To To Gb Gb  . Ba Sc  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  . ,,  .  .  . To  .  .  .  .  . To  .  .  . ~~ ~~  .  .  .  .  .  .  .  .  . Tb  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  . Tb ,, To  .  .  .  .  .  .  .  .  .  .  .  . ~~ ~~  .  .  .  . ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,,  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  . ,,  .  .  .  .  .  .  .  .  . Fd  .  .  . ~~ ~~  .  .  .  . ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,,  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  . Tb  . Fw  .  . ,, ,,  . Tb  .  .  .  .  .  .  .  .  .  .  . ~~ ~~  .  .  .  . ,, ,,  .  . ,,  . Tb  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  . ,, ,,  .  .  .  .  .  .  .  .  .  .  .  .  . ~~ ~~  .  .  .  . ,, ,,  .  .  .  .  .  .  . Tb  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  . ,,  .  .  .  . Tb  . ,, ,, ,, ,, ,,  .  .  .  . ,, ,, ,, ,, ,, ,,  .  .  .  . To  . To  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  . ,,  .  .  .  .  .  . ,, ,, ,, ,, ,,  .  .  .  . ,, ,, ,, ,, ,,  .  .  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  . Tb  .  .  .  . To ,,  .  . To  .  .  . ,, ,,  .  .  . ~~ ~~  .  .  .  . ,, To To  .  .  .  .  .  .  . Fm  .  .  . To  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  . ,, ,,  .  .  .  .  . ,, ,,  .  .  . ~~ ~~  .  .  . Fe  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  . To  . To  .  .  .  . ,, ,, ,, ,, ,, ,, ,, ,, ,,  .  .  . ~~ ~~  .  .  .  .  .  .  .  . To  .  .  . To  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  . Tb  . Tb  . To ,, ,, ,, ,, ,, ,, ,, ,,  .  .  .  . ~~ ~~  .  .  . To  .  .  .  .  .  .  .  . To  .  .  .  .  . To  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  . To  .  .  .  . Fr  . ,, ,,  . ,, Fs  .  .  . Tb  .  .  . ~~ ~~  .  .  .  .  . Tb  .  . To To Fn  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  . ,, ,,  .  .  .  .  .  .  . Tr  .  . ~~ ~~  . To  .  .  .  .  . To  . Tb  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  . ,, ,, ,, ,, ,, ,, ,, ,, ,,  .  .  .  .  .  .  . To  .  . ~~ ~~  .  .  .  .  .  .  .  .  . To  .  .  .  .  .  .  .  . To  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  . ,, ,, ,, ,, ,, ,, ,, ,,  .  .  .  .  .  .  .  .  .  .  . ~~ ~~  .  .  . Tr  .  .  .  .  .  . To  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  . To  . ,,  .  .  .  .  .  .  . To  .  .  .  .  .  .  . ~~ ~~  .  . Tb  . To Tb To To  .  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  . Tb To  .  .  .  . To  .  .  .  .  .  . To  .  .  . ~~ ~~  .  .  .  .  .  .  .  .  .  .  . Tb  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .
   Ba=Fıçı, Fd=Kuru dal yığını, Fe=Mürver çiçeği, Fm=Mantar öbeği, Fn=Isırgan, Fr=Böğürtlen çalısı, Fs=Yabani çilek, Fw=Yabani sarımsak, Gb=Bahçe bankı, Sc=Tohum kasası, Tb=Huş, To=Meşe, Tr=Söğüt
```
Sonra:
```
== Köy yolu (bölge) ==
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  . To  . To ,,  .  .  .  .  .  .  .  .  . Fw  .  . ~~ ~~  . Fm  .  .  . To  .  .  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  . ,,  .  . To  .  .  .  .  .  .  .  .  . ~~ ~~  .  . Tw  .  .  .  .  .  .  .  .  .  .  . Tr  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  . Fe  .  .  .  . ,,  .  .  .  .  .  .  .  .  .  .  .  .  . ~~ ~~  .  .  .  .  .  .  . Tr  .  .  .  .  .  .  .  . To  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  . Tb  .  .  .  .  . ,,  .  .  .  .  .  .  .  .  .  .  .  . Tw ~~ ~~  . Fn  .  .  .  .  .  .  .  . Tb  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  . To  . ,,  .  . To  .  . Tr  .  .  .  .  .  .  . ~~ ~~  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  . ,,  .  .  .  .  .  .  .  .  .  .  .  .  . ~~ ~~  .  .  . To  .  .  .  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  . ,, ,,  .  .  .  .  .  .  .  .  .  .  .  .  . ~~ ~~  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . Tb  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  . To  .  .  .  . ,,  .  .  .  .  .  .  .  .  .  .  . Tb  . ~~ ~~  .  .  .  .  .  .  .  .  .  .  .  .  . To  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  . To  . ,,  .  .  .  .  .  .  .  .  .  .  .  .  . ~~ ~~  .  .  .  . ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,,  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  . ,,  .  .  .  .  .  .  .  .  .  .  .  .  . ~~ ~~  .  .  .  . ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,,  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  . ,, ,,  .  .  .  .  .  .  .  .  .  .  . Tw  . ~~ ~~ Re  .  .  . ,, ,,  .  . ,,  . Sc G^ G^ Ba  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +::+::+  +  +  +  +  
  . To  .  .  .  .  . ,, ,,  .  .  .  .  .  .  .  .  .  .  .  .  . ~~ ~~  .  .  .  . ,, ,,  .  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  . ,,  .  .  . Fr  .  . ,, ,, ,, ,, ,,  .  .  .  . ,, ,, ,, ,, ,, ,,  .  .  .  .  .  . Tb  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  . Tb  .  .  . ,,  .  .  .  .  .  . ,, ,, ,, ,, ,,  .  .  .  . ,, ,, ,, ,, ,,  .  .  . To  .  .  .  .  .  . To  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  . ,,  .  .  .  .  .  . ,, ,,  .  .  . ~~ ~~  . Fg  .  . ,,  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  . ,, ,,  .  .  .  .  . ,, ,,  .  .  . ~~ ~~  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  . Si ,, ,, ,, ,, ,, ,, ,, ,, ,,  .  . Tw ~~ ~~  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . Fd  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  . ,, ,, ,, ,, ,, ,, ,, ,,  .  .  .  . ~~ ~~  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . To  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  . ,, ,,  . ,,  .  .  .  .  .  .  .  . ~~ ~~  .  .  .  . To  .  .  .  .  .  .  .  .  . To  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  . To  .  . Tb  . ,, ,,  .  .  .  .  .  . Tb  .  .  . ~~ ~~  .  .  .  .  .  .  .  .  . To  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  . ,, ,, ,, ,, ,, ,, ,, ,, ,,  . Tb  .  .  .  .  .  .  .  . ~~ ~~ Re  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  . ,, ,, ,, ,, ,, ,, ,, ,, Tb  .  .  .  .  .  .  .  .  .  . ~~ ~~  .  .  .  .  .  .  .  .  .  . To  .  .  .  .  .  .  . Tb  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  . Tr  .  . ,,  .  . Tr  .  .  .  .  .  .  .  .  .  .  .  . ~~ ~~  .  .  .  .  . To  .  .  .  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . To  .  .  . ~~ ~~  .  .  .  .  .  .  .  .  .  .  .  .  .  . To  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .
   Ba=Fıçı, Fd=Kuru dal yığını, Fe=Mürver çiçeği, Fg=Böğürtlen çalısı, Fm=Mantar öbeği, Fn=Isırgan, Fr=Yabani çilek, Fw=Yabani sarımsak, Gb=Bahçe bankı, Re=Saz öbeği, Sc=Tohum kasası, Si=Yol tabelası, Tb=Huş, To=Meşe, Tr=Çam, Tw=Söğüt
```

### Çiftlik girişi
Önce:
```
== Çiftlik girişi (önce) ==
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
 Te  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +--+--+==+--+--+--+==+--+--+  +  +  +  +  +  +  
  .  .  .  .  .  .  .| .  . Pa Pa Hs Co  .  . Wb| . Rb  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  . Ci  .  .  .| . Sd  .  .  .  .  . Su  .| .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .| . Sd  .  .  .  .  . Su  .| .  .  .  .  .  . Id
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .= .  . Cr Tw Tw Cr  .  .  .= .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  . We  .  .|Be  .  .  .  .  .  .  .  .| .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .| .  .  .  .  .  .  .  . Ac| .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .|Ca  .  .  .  .  . Sw  . Cw| .  .  .  .  .  .  .
+  +  +  +  +  +  +  +--+--+==+--+DD+--+==+--+--+  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  . ,,  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  . Hb  .  .  .  .  .  .  . ,,  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  . Gb Gb  . ,,  . Sw Sl  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  . ,,  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  . ,, ,,  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  . ,,  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  . ,, ,,  .  .  .  .  .  . ,,  .  . ,,  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  . ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,,
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . ,,  .  .  .  .
   Ac=Minderli koltuk, Be=Leğen ve ibrik, Ca=Şamdan, Ci=Yarma kütüğü, Co=Pişirme kazanı, Cr=Tahta sandalye, Cw=Sandık, Gb=Bahçe bankı, Hb=Ot tınazı, Hs=Taş ocak, Id=Sulama hendeği, Pa=Kiler rafı, Rb=Yağmur fıçısı, Sd=Bodrum merdiveni, Sl=Kütük sehpa, Su=Ahşap merdiven (üst kat), Sw=Tahta tabure, Te=Erik ağacı, Tw=Tahta masa, Wb=Su fıçısı, We=Kuyu
```
Sonra:
```
== Çiftlik evi kat 0 ==
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
 Tp  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . Hb  .  .  .  .  .  .
+  +  +  +  +  +  +  +--+--+==+--+--+--+==+--+--+  +  +  +  +  +  +  
  .  .  .  .  .  . Rb|Ca  .  .  .  .  .  .  . H<| .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .| . Sd  .  .  .  .  . Su Cp| .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .| . Sd  .  . Sw  .  . Su  .| .  .  .  .  .  . Id
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .= .  .  .  . Tw C<  .  .  .=Cb  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .|Wb  .  .  . Tw C<  .  .  .| .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .| .  .  .  .  .  .  .  . C<| .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .|P^ P^ B^  .  .  .  .  .  .| .  .  .  .  .  .  .
+  +  +  +  +  +  +  +--+--+==+--+DD+--+==+--+--+  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  . ,,  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  . ,,  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  . ,,  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  . ,, We  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  . ,, ,, ,, ,, ,, ,, ,,  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  . ,,  .  .  .  .  .  .  .  .  .  .  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  . ,, ,,  .  .  .  .  .  . ,,  .  . ,,  .
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  . Sw St  .  .  .  . ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,, ,,
+  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  +  
  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  .  . ,,  .  .  .  .
   Be=Leğen ve ibrik, Ca=Şamdan, Cb=Yarma kütüğü, Ch=Sandık, Cp=Pişirme kazanı, Cw=Tahta sandalye, Hb=Ot tınazı, Hs=Taş ocak, Id=Sulama hendeği, Ps=Kiler rafı, Rb=Yağmur fıçısı, Sd=Bodrum merdiveni, St=Kütük sehpa, Su=Ahşap merdiven (üst kat), Sw=Tahta tabure, Tp=Erik ağacı, Tw=Tahta masa, Wb=Su fıçısı, We=Kuyu
```

