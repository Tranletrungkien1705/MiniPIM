#!/bin/bash
P=D:/idocNet/2019.4.ProductCenter/Dev/V20/.svn/pristine
F="$P/3f/3f426e58637a07a3ea2cef05f42e560ac097a2c6.svn-base"
echo "=== InvalidGTIN context ==="
grep -n 'InvalidGTIN\|IsNotNumberGTIN\|InvalidProductCodeUK\|CheckProductCodeUser' "$F"
echo "=== Mst_Product_CheckProductCodeUser (109-183) ==="
sed -n '109,183p' "$F"
