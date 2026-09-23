#!/bin/bash
P=D:/idocNet/2019.4.ProductCenter/Dev/V20/.svn/pristine
F=$P/6a/6a79ae98efa0135b852007c8e58247c0ed1c14e4.svn-base
echo "=== line numbers of ProductType/Brand methods ==="
grep -n 'Mst_ProductType_Create\|Mst_ProductType_Update\|Mst_ProductType_Delete\|Mst_ProductType_CheckProductTypeName\|Mst_ProductType_CheckName\|Mst_Brand_Create\|Mst_Brand_Update\|Mst_Brand_Delete\|Mst_Brand_CheckDB\|Mst_Brand_CheckName\|Mst_Brand_CheckBrandName' $F
