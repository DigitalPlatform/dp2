md kernel_app
cd kernel_app

del *.* /Q

xcopy ..\..\dp2kernel\bin\debug\*.dll /Y
xcopy ..\..\dp2kernel\bin\debug\*.exe /Y
xcopy ..\..\dp2kernel\bin\debug\dp2kernel.exe.config /Y
xcopy ..\..\dp2kernel\bin\debug\*.pfx /Y

md x86
xcopy ..\..\dp2kernel\bin\debug\x86 x86 /Y
md x64
xcopy ..\..\dp2kernel\bin\debug\x64 x64 /Y

del *.vshost.exe /Q

cd ..

..\ziputil kernel_app kernel_app.zip -t
..\ziputil kernel_data kernel_data.zip -t