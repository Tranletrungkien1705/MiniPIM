#!/bin/bash
P=D:/idocNet/2019.4.ProductCenter/Dev/V20/.svn/pristine
F=$P/80/80ff70976aeaf9a1e877fb16ed02054209d4280d.svn-base
echo "########## Mst_Brand_DeleteX (32-127) ##########"
sed -n '32,127p' $F
echo "=== ProductType method lines ==="
grep -n 'Mst_ProductType_Create\|Mst_ProductType_Update\|Mst_ProductType_Delete\|Mst_ProductType_CheckDB\|CheckProductTypeName' $F