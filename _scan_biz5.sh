#!/bin/bash
P=D:/idocNet/2019.4.ProductCenter/Dev/V20/.svn/pristine
echo "=== Biz class names (public class XxxBiz / BizProductCenter) ==="
grep -rhn 'public class ' "$P" 2>/dev/null | grep -iE 'Biz' | sort -u | head -60
echo "=== files defining BizProductCenter ==="
grep -rln 'class BizProductCenter' "$P" 2>/dev/null
echo "=== files defining Biz* classes ==="
grep -rln 'public class Biz' "$P" 2>/dev/null
