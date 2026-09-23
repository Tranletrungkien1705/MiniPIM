#!/bin/bash
P=D:/idocNet/2019.4.ProductCenter/Dev/V20/.svn/pristine
echo "=== files with 'Mst_ProductType_Create(' ==="
grep -rln 'Mst_ProductType_Create(' $P 2>/dev/null
echo "=== files with 'Mst_ProductType_CheckDB(' ==="
grep -rln 'Mst_ProductType_CheckDB(' $P 2>/dev/null
echo "=== files with 'ProductTypeName' ==="
grep -rln 'ProductTypeName' $P 2>/dev/null