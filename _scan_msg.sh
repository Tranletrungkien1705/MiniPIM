#!/bin/bash
P=D:/idocNet/2019.4.ProductCenter/Dev/V20/.svn/pristine
echo "=== find ErrProductCenter file ==="
grep -rln 'Mst_Product_Create_InvalidGTIN' "$P" 2>/dev/null
echo "=== find error message text ==="
grep -rhn 'Mst_Product_Create_InvalidGTIN\|Mst_Product_CheckDB_ProductCodeUserExist\|Mst_Product_CheckDB_ProductCodeUserNotFound\|Mst_Product_Create_InvalidProductCodeUK' "$P" 2>/dev/null | grep -iE 'const|=' | head -20
