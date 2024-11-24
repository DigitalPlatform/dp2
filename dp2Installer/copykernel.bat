@echo off
set argC=0
for %%x in (%*) do set /A argC+=1
if not "%argC%"=="1" (
echo USAGE: 
echo     %0 debug
echo     -or-
echo     %0 release
goto :END
)
@echo on

echo current configuration name: %1

md kernel_app
cd kernel_app

del *.* /Q

xcopy ..\..\dp2kernel\bin\%1\*.dll /Y
xcopy ..\..\dp2kernel\bin\%1\*.exe /Y
xcopy ..\..\dp2kernel\bin\%1\dp2kernel.exe.config /Y
xcopy ..\..\dp2kernel\bin\%1\*.pfx /Y

md x86
xcopy ..\..\dp2kernel\bin\%1\x86 x86 /Y
md x64
xcopy ..\..\dp2kernel\bin\%1\x64 x64 /Y

del *.vshost.exe /Q

cd ..

..\ziputil kernel_app kernel_app.zip -t
..\ziputil kernel_data kernel_data.zip -t

:END