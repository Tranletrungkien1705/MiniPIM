#!/bin/bash
P=D:/idocNet/2019.4.ProductCenter/Dev/V20/.svn/pristine
echo "=== files with Mst_ProductUI ==="
grep -rln 'Mst_ProductUI' "$P" 2>/dev/null
echo "=== files with ProductCodeUser ==="
grep -rln 'ProductCodeUser' "$P" 2>/dev/null
echo "=== files with Mst_Product_Import_Update ==="
grep -rln 'Mst_Product_Import_Update' "$P" 2>/dev/null
