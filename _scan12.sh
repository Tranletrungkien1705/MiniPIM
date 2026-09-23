#!/bin/bash
P=D:/idocNet/2019.4.ProductCenter/Dev/V20/.svn/pristine
F=$P/80/80ff70976aeaf9a1e877fb16ed02054209d4280d.svn-base
echo "########## Mst_Brand_CheckDB + CheckBrandName (1377-1523) ##########"
sed -n '1377,1523p' $F
echo "########## Mst_Brand_Create (1832-2040) ##########"
sed -n '1832,2040p' $F