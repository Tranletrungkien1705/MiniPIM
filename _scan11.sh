#!/bin/bash
P=D:/idocNet/2019.4.ProductCenter/Dev/V20/.svn/pristine
F=$P/80/80ff70976aeaf9a1e877fb16ed02054209d4280d.svn-base
echo "=== lines: $(wc -l < $F) ==="
head -30 $F
echo "=== method line numbers ==="
grep -n 'Mst_ProductType_Create\|Mst_ProductType_Update\|Mst_ProductType_Delete\|Mst_ProductType_CheckDB\|CheckProductTypeName\|Mst_Brand_Create\|Mst_Brand_Update\|Mst_Brand_Delete\|Mst_Brand_CheckDB\|CheckBrandName' $F