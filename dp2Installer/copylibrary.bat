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

rd library_data /S /Q
md library_data
xcopy ..\dp2libraryxe\library_data library_data /s /Y

..\ziputil library_app library_app.zip -t
..\ziputil library_data library_data.zip -t