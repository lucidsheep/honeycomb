timeout /t 5
if exist update.zip tar -xf update.zip
if exist update.zip del update.zip
start Honeycomb.exe