#/bin/bash

cd "$(dirname "$0")"
if [ -e kquity.zip ]
then
tar -xf kquity.zip
rm kquity.zip
fi
cd kquity
python WSServer.py