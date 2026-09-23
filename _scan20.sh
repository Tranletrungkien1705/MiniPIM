#!/bin/bash
for f in 13/139aab074d4d4f8e63af3b67801d7284ee8f4223 3f/3f426e58637a07a3ea2cef05f42e560ac097a2c6 50/50e6a93613efe143101c79e44756ad15cabf422b 57/57b0b7a8b5162f748849f4c0911c3a6ff2f9712f 5f/5ff63b5eef98c59404a22dbcdc78cd865e86a5b8 65/65506e78e64e73c3dbf069bc4436b42f44a6f4a3 70/70bc7104fa9036f8f5b6ddfe7324951561136435 71/71c34204d282623e460af4a19f60e9d29dcd2fdf 73/73087cc29d271ccceb751dca0db6441f2f4d151e; do
  echo "=== $f ==="
  grep -c 'public DataSet' "D:/idocNet/2019.4.ProductCenter/Dev/V20/.svn/pristine/$f.svn-base"
  grep -o 'Mst_[A-Za-z]*_\|Prd_[A-Za-z]*_\|Sys_[A-Za-z]*_' "D:/idocNet/2019.4.ProductCenter/Dev/V20/.svn/pristine/$f.svn-base" | sort -u | tr '\n' ' '
  echo
done
