#!/bin/bash
P=D:/idocNet/2019.4.ProductCenter/Dev/V20/.svn/pristine
echo "=== find message resource file ==="
grep -rln 'Mst_Product_CheckDB_ProductCodeUserNotFound","Không' "$P" 2>/dev/null
echo "=== GTIN messages ==="
grep -rhn 'InvalidGTIN\|IsNotNumberGTIN\|InvalidProductCodeUK' "$P" 2>/dev/null | grep -E '\{"Err' | head
