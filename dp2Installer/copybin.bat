md library_app
cd library_app

del *.* /Q
xcopy ..\..\dp2library\bin\debug\*.dll /Y

del chchdx*.dll /Q
del dongshifang*.dll /Q
del testmessageinterface.dll /Q

xcopy ..\..\dp2library\bin\debug\*.exe /Y
xcopy ..\..\dp2library\bin\debug\*.pfx /Y

del *.vshost.exe /Q

xcopy ..\..\dp2library\bin\debug\dp2library.exe.config /Y

md en-US
xcopy ..\..\dp2library\bin\debug\en-US en-US /s /Y
md zh-CN
xcopy ..\..\dp2library\bin\debug\zh-CN zh-CN /s /Y

cd ..


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

