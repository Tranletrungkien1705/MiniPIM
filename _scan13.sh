#!/bin/bash
P=D:/idocNet/2019.4.ProductCenter/Dev/V20/.svn/pristine
F=$P/80/80ff70976aeaf9a1e877fb16ed02054209d4280d.svn-base
echo "########## Mst_Brand_Update (2040-2245) ##########"
sed -n '2040,2245p' $F
echo "########## Mst_Brand_Delete (2245-2479) ##########"
sed -n '2245,2479p' $F