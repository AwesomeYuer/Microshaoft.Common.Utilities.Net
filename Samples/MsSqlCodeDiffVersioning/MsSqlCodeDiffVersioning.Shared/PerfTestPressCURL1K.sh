#!/bin/bash
for i in {1..100}; do
  curl 'http://baidu.com' &
done

wait
