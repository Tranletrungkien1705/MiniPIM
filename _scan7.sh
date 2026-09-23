#!/bin/bash
P=D:/idocNet/2019.4.ProductCenter/Dev/V20/.svn/pristine
echo "=== files with Mst_ProductType_Create ==="
grep -rln 'Mst_ProductType_Create\|Mst_ProductType_Update\|Mst_ProductType_Delete' $P 2>/dev/null
echo "=== files with Mst_Brand_Create ==="
grep -rln 'Mst_Brand_Create\|Mst_Brand_Update\|Mst_Brand_Delete' $P 2>/dev/null
echo "=== files with Mst_ProductType_CheckProductTypeName ==="
grep -rln 'Mst_ProductType_CheckProductTypeName\|Mst_ProductType_CheckName' $P 2>/dev/null
