#!/bin/bash
P=D:/idocNet/2019.4.ProductCenter/Dev/V20/.svn/pristine
echo "=== ProductUI method context (57/576fa...) ==="
grep -n 'Mst_ProductUI' "$P/57/576fa7bf51e5a5a478f3b13cc8bd5e2d6976fe4b.svn-base" 2>/dev/null | head -30
echo "=== ProductCodeUser check context (3f/3f426...) ==="
grep -n 'ProductCodeUser' "$P/3f/3f426e58637a07a3ea2cef05f42e560ac097a2c6.svn-base" 2>/dev/null | head -30
