#!/bin/bash

cd /Users/kevin/Documents/git/kqstyle/Assets/..
sleep 5s
if test -f "update.zip"; then
 tar -xf update.zip
 rm update.zip 
 fi 
 ./Honeycomb