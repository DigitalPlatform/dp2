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

md library_app
cd library_app

del *.* /Q
xcopy ..\..\dp2library\bin\%1\*.dll /Y

del chchdx*.dll /Q
del dongshifang*.dll /Q
del testmessageinterface.dll /Q

xcopy ..\..\dp2library\bin\%1\*.exe /Y
xcopy ..\..\dp2library\bin\%1\*.pfx /Y

del *.vshost.exe /Q

xcopy ..\..\dp2library\bin\%1\dp2library.exe.config /Y

md en-US
xcopy ..\..\dp2library\bin\%1\en-US en-US /s /Y
md zh-CN
xcopy ..\..\dp2library\bin\%1\zh-CN zh-CN /s /Y

cd ..

rd library_data /S /Q
md library_data
xcopy ..\dp2libraryxe\library_data library_data /s /Y

..\ziputil library_app library_app.zip -t
..\ziputil library_data library_data.zip -t

:END