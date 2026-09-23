#!/bin/bash
P=D:/idocNet/2019.4.ProductCenter/Dev/V20/.svn/pristine
for f in $(grep -rln 'Mst_ProductGroup_UpdBU' $P 2>/dev/null); do
  echo "$f : $(wc -l < $f) lines"
done
echo "=== product master files ==="
for f in $(grep -rln 'Mst_Product_Create\|Mst_Product_Update\|Mst_Product_Delete' $P 2>/dev/null); do
  echo "$f : $(wc -l < $f) lines"
done
