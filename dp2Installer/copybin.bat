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

xcopy ..\..\dp2library\bin\debug\en-US en-US /s /Y
xcopy ..\..\dp2library\bin\debug\zh-CN zh-CN /s /Y

cd ..



cd kernel_app

del *.* /Q

xcopy ..\..\dp2kernel\dp2kernel\bin\debug\*.dll /Y
xcopy ..\..\dp2kernel\dp2kernel\bin\debug\*.exe /Y
xcopy ..\..\dp2kernel\dp2kernel\bin\debug\dp2kernel.exe.config /Y
xcopy ..\..\dp2kernel\dp2kernel\bin\debug\*.pfx /Y

del *.vshost.exe /Q

cd ..

