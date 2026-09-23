#!/bin/bash
P=D:/idocNet/2019.4.ProductCenter/Dev/V20/.svn/pristine
F="$P/3f/3f426e58637a07a3ea2cef05f42e560ac097a2c6.svn-base"
echo "=== lines: $(wc -l < "$F") ==="
echo "=== public static methods ==="
grep -n 'public static' "$F" | head -200
