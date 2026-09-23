#!/bin/bash
P=D:/idocNet/2019.4.ProductCenter/Dev/V20/.svn/pristine
F=$P/43/439a010d286c696b2957e144014af5795f2857a7.svn-base
echo "=== head ==="
head -30 $F
echo "=== method line numbers ==="
grep -n 'Mst_ProductType_Create\|Mst_ProductType_Update\|Mst_ProductType_Delete\|Mst_ProductType_CheckDB\|Mst_ProductType_CheckProductTypeName\|Mst_Brand_Create\|Mst_Brand_Update\|Mst_Brand_Delete\|Mst_Brand_CheckDB\|Mst_Brand_CheckBrandName' $F
