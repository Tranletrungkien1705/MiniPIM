#!/bin/bash
P=D:/idocNet/2019.4.ProductCenter/Dev/V20/.svn/pristine
F="$P/3f/3f426e58637a07a3ea2cef05f42e560ac097a2c6.svn-base"
echo "=== head ==="
head -30 "$F"
echo "=== method-ish lines ==="
grep -nE '(public|private|internal|protected).*(Mst_|static)' "$F" | head -60
