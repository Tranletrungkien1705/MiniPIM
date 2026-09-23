#!/bin/bash
P=D:/idocNet/2019.4.ProductCenter/Dev/V20/.svn/pristine
F="$P/3f/3f426e58637a07a3ea2cef05f42e560ac097a2c6.svn-base"
echo "=== GTIN check 1680-1710 ==="
sed -n '1680,1710p' "$F"
echo "=== GTIN check 2690,2720 ==="
sed -n '2690,2720p' "$F"
echo "=== ProductCodeUser check 1620,1660 ==="
sed -n '1620,1660p' "$F"
