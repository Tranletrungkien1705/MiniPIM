#!/bin/bash
DB="D:/idocNet/2019.4.ProductCenter/Dev/V20/.svn/wc.db"
DOTNET="C:/Program Files/dotnet/dotnet.exe"
cd D:/idocNet/_labs/_svntool
for f in "$@"; do
  echo "=== $f ==="
  "$DOTNET" run --project . -- "$DB" "$f" 2>&1 | grep -vE 'RQ_|RT_|_RQ|_RT' | head -30
done
