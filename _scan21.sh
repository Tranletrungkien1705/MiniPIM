#!/bin/bash
for f in 3f/3f426e58637a07a3ea2cef05f42e560ac097a2c6 43/439a010d286c696b2957e144014af5795f2857a7 5f/5ff63b5eef98c59404a22dbcdc78cd865e86a5b8 80/80ff70976aeaf9a1e877fb16ed02054209d4280d 93/9341dd6ff167576ee32681d739af4f9bf4499955 96/96806605a05f41c4b2e47d9a66e7795103a4bf84; do
  echo "=== $f ==="
  grep -c 'Mst_SSCCType_CheckDB' "D:/idocNet/2019.4.ProductCenter/Dev/V20/.svn/pristine/$f.svn-base"
done