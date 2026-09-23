#!/bin/bash
P=D:/idocNet/2019.4.ProductCenter/Dev/V20/.svn/pristine
echo "=== search for GTIN message text ==="
grep -rhn 'GTIN' "$P" 2>/dev/null | grep -iE 'không|hợp lệ|trùng|tồn tại|Invalid' | head -20
echo "=== search ProductCodeUser message ==="
grep -rhn 'ProductCodeUser' "$P" 2>/dev/null | grep -iE 'không|hợp lệ|trùng|tồn tại' | head -20
