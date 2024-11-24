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

md gate_app
cd gate_app

del *.* /Q
xcopy ..\..\dp2gate\bin\%1\*.dll /Y

xcopy ..\..\dp2gate\bin\%1\*.exe /Y

del *.vshost.exe /Q

xcopy ..\..\dp2gate\bin\%1\dp2gate.exe.config /Y

md x86
xcopy ..\..\dp2gate\bin\%1\x86 x86 /s /Y
md x64
xcopy ..\..\dp2gate\bin\%1\x64 x64 /s /Y

cd ..

..\ziputil gate_app gate_app.zip -t

xcopy gate_app.zip c:\publish\dp2installer\v3 /Y

:END