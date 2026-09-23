#!/bin/bash
P=D:/idocNet/2019.4.ProductCenter/Dev/V20/.svn/pristine
echo "=== files with Mst_Product_CreateX ==="
grep -rln 'Mst_Product_CreateX' "$P" 2>/dev/null
echo "=== files with public static Mst_ ==="
grep -rln 'public static.*Mst_' "$P" 2>/dev/null | head -20
