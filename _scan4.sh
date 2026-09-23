#!/bin/bash
P=D:/idocNet/2019.4.ProductCenter/Dev/V20/.svn/pristine
for f in 3f/3f426e58637a07a3ea2cef05f42e560ac097a2c6 5f/5ff63b5eef98c59404a22dbcdc78cd865e86a5b8 93/9341dd6ff167576ee32681d739af4f9bf4499955 96/96806605a05f41c4b2e47d9a66e7795103a4bf84 57/57b0b7a8b5162f748849f4c0911c3a6ff2f9712f b5/b58bb8ae59c19cde4237bb7738cc89de45c0506b 19/19b2e4d039294df97f2fb74a9a06285a1fa727ec; do
  echo "===== $f ====="
  grep -n 'namespace \|class ' $P/$f.svn-base | head -5
  grep -no 'Mst_[A-Za-z_]*' $P/$f.svn-base | sed 's/^[0-9]*://' | sort -u | head -60
done
