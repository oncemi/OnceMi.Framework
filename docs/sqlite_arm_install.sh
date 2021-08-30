#!/bin/bash
echo "Start installing SQLite ARM dependencies..."
sudo rm -rf sqlite_arm.zip*
wget 'https://raw.githubusercontent.com/oncemi/OnceMi.Framework/main/docs/sqlite_arm.zip' --no-check-certificate
if [ ! -f "sqlite_arm.zip" ];then
  echo "Download file failed."
  exit 1
fi
#install unzip
if unzip -h > /dev/null 2>&1; then
    echo "unzip exist, skip install unzip."
else
    echo "unzip not exist, install it!"
    sudo apt update
    sudo apt install unzip -y
fi
#delete old
sudo rm -rf libSQLite.Interop.so SQLite.Interop.dll
unzip sqlite_arm.zip
sudo mv sqlite_arm/SQLite.Interop.dll sqlite_arm/libSQLite.Interop.so ./
sudo rm -rf sqlite_arm* sqlite_arm_install.sh
echo "Installation is complete."
