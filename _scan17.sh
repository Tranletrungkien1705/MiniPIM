#!/bin/bash
P=D:/idocNet/2019.4.ProductCenter/Dev/V20/.svn/pristine
echo "=== files mentioning Mst_ProductType_Update ==="
grep -rln 'Mst_ProductType_Update' $P 2>/dev/null
echo "=== files mentioning Mst_ProductType_Delete ==="
grep -rln 'Mst_ProductType_Delete' $P 2>/dev/null
echo "=== files mentioning Mst_ProductType_Create ==="
grep -rln 'Mst_ProductType_Create' $P 2>/dev/null