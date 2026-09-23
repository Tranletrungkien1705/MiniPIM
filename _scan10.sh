#!/bin/bash
P=D:/idocNet/2019.4.ProductCenter/Dev/V20/.svn/pristine
echo "=== files with 'public DataSet Mst_ProductType_Create' ==="
grep -rln 'DataSet Mst_ProductType_Create\|DataSet Mst_ProductType_Update\|DataSet Mst_ProductType_Delete' $P 2>/dev/null
echo "=== files with 'DataSet Mst_Brand_Create' ==="
grep -rln 'DataSet Mst_Brand_Create\|DataSet Mst_Brand_Update\|DataSet Mst_Brand_Delete' $P 2>/dev/null
echo "=== files with 'Mst_ProductType_CheckProductTypeName' or 'CheckProductTypeName' ==="
grep -rln 'CheckProductTypeName\|CheckBrandName' $P 2>/dev/null