#!/bin/bash
P=D:/idocNet/2019.4.ProductCenter/Dev/V20/.svn/pristine
F="$P/3f/3f426e58637a07a3ea2cef05f42e560ac097a2c6.svn-base"
echo "=== Mst_Product_CreateX error codes (1468-2743) ==="
sed -n '1468,2743p' "$F" | grep -oE 'Mst_Product_[A-Za-z_0-9]+' | sort -u
echo "=== Mst_Product_UpdateMasterX error codes (5508-6248) ==="
sed -n '5508,6248p' "$F" | grep -oE 'Mst_Product_[A-Za-z_0-9]+' | sort -u
