#!/bin/bash
P=D:/idocNet/2019.4.ProductCenter/Dev/V20/.svn/pristine
echo "=== all Biz files (namespace ProductCenter.Biz) ==="
grep -rln 'namespace ProductCenter.Biz' "$P" 2>/dev/null
echo "=== class names in Biz ==="
grep -rhn 'public class ' "$P" 2>/dev/null | grep -iE 'Biz|Product|Spec|Group|Unit|Brand|Model|Price|Attr' | head -40
