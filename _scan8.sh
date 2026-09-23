#!/bin/bash
P=D:/idocNet/2019.4.ProductCenter/Dev/V20/.svn/pristine
F=$P/9b/9b81b409820ee23a9d674177b3d673ef1ca6e14c.svn-base
echo "=== head ==="
head -30 $F
echo "=== method line numbers ==="
grep -n 'public \|private \|Mst_ProductType\|Mst_Brand' $F | head -80
