#/bin/bash

cd "$(dirname "$0")"
sleep 5
if [ -e update.zip ]
then
tar -xf update.zip
rm update.zip
fi
./MacOS/Honeycomb &
disown