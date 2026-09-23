#!/bin/bash
P=D:/idocNet/2019.4.ProductCenter/Dev/V20/.svn/pristine
echo "=== Mst_Brand model class ==="
grep -rln 'class Mst_Brand' $P 2>/dev/null
echo "=== Mst_ProductGroup BrandCode usage ==="
grep -rn 'BrandCode' $P/6a/6a79ae98efa0135b852007c8e58247c0ed1c14e4.svn-base | head -20