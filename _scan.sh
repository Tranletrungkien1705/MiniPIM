#!/bin/bash
P=D:/idocNet/2019.4.ProductCenter/Dev/V20/.svn/pristine
grep -rl 'class .*Controller : ApiControllerBase' "$P" | while read f; do
  cls=$(grep -m1 -o 'class [A-Za-z0-9_]*Controller' "$f")
  echo "$cls  <-  $f"
done