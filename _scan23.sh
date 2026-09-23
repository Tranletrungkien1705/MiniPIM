#!/bin/bash
grep -rln 'Mst_SpecCustomField_CheckDB' D:/idocNet/2019.4.ProductCenter/Dev/V20/.svn/pristine/ | while read f; do
  echo "$f $(wc -c < "$f")"
done