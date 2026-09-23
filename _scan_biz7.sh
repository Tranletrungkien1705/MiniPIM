#!/bin/bash
P=D:/idocNet/2019.4.ProductCenter/Dev/V20/.svn/pristine
echo "=== all Mst_*_Create/Update/Delete/CheckDB method names ==="
grep -rhoE 'Mst_[A-Za-z0-9_]+_(Create|Update|Delete|CheckDB|CreateX|UpdateX|DeleteX)[A-Za-z0-9_]*' "$P" 2>/dev/null | sort -u
