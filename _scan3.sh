#!/bin/bash
P=D:/idocNet/2019.4.ProductCenter/Dev/V20/.svn/pristine
F=$P/6a/6a79ae98efa0135b852007c8e58247c0ed1c14e4.svn-base
echo "=== class/namespace decls ==="
grep -n 'class \|namespace ' $F | head -40
echo "=== public method names (Mst_*) ==="
grep -no 'Mst_[A-Za-z_]*' $F | sort -u -t: -k2 | head -120
