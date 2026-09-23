#!/bin/bash
cd D:/idocNet/2019.4.ProductCenter/Dev/V20/.svn/pristine
for f in "$@"; do
  echo "=== $f ==="
  head -3 "$f.svn-base"
  grep -nE 'class |namespace ' "$f.svn-base" | head -4
done
