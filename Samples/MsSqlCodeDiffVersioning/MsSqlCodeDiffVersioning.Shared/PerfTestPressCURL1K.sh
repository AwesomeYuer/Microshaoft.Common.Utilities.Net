#!/bin/bash
ps -a -e -o 'rss,vsz,comm' | grep dotnet

for i in {1..100}; do
  curl 'http://baidu.com' &
done

wait
