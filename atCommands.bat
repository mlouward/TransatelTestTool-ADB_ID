REM AT+CGMI : Manufacturer name
@echo off
cd platform-tools/

adb shell "su -c 'cat /dev/at_usb0 & echo -e %1\\r > /dev/at_usb0'"
cd..