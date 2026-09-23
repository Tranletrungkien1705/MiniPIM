#!/bin/bash
P=D:/idocNet/2019.4.ProductCenter/Dev/V20/.svn/pristine
F=$P/20/20372fd17bd161cb63d2490d4ca852e0e3629e94.svn-base
echo "=== lines: $(wc -l < $F) ==="
grep -n 'class Mst_Brand' $F
sed -n "$(grep -n 'class Mst_Brand' $F | head -1 | cut -d: -f1),+60p" $F