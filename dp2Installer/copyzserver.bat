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

md zserver_app
cd zserver_app

del *.* /Q
xcopy ..\..\dp2zserver\bin\%1\*.dll /Y

xcopy ..\..\dp2zserver\bin\%1\*.exe /Y
rem xcopy ..\..\dp2zserver\bin\%1\*.pfx /Y

del *.vshost.exe /Q

xcopy ..\..\dp2zserver\bin\%1\dp2zserver.exe.config /Y

cd ..

..\ziputil zserver_app zserver_app.zip -t
..\ziputil zserver_data zserver_data.zip -t


:END