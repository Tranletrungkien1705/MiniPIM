#!/bin/bash
P=D:/idocNet/2019.4.ProductCenter/Dev/V20/.svn/pristine
echo "=== ErrProductCenter file ==="
grep -rln 'Mst_Product_CheckDB_ProductCodeUserExist' "$P" 2>/dev/null
echo "=== messages ==="
for f in $(grep -rln 'Mst_Product_CheckDB_ProductCodeUserExist' "$P" 2>/dev/null); do
  grep -n 'ProductCodeUser' "$f" 2>/dev/null | head -20
done
