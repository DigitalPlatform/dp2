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

md palm_app
cd palm_app

del *.* /Q
xcopy ..\..\palmcenter\bin\%1\*.dll /Y

xcopy ..\..\palmcenter\bin\%1\*.exe /Y

del *.vshost.exe /Q

xcopy ..\..\palmcenter\bin\%1\palmcenter.exe.config /Y

md x86
xcopy ..\..\palmcenter\bin\%1\x86 x86 /s /Y
md x64
xcopy ..\..\palmcenter\bin\%1\x64 x64 /s /Y

cd ..

..\ziputil palm_app palm_app.zip -t

xcopy palm_app.zip .\bin\%1 /Y

xcopy palm_app.zip c:\publish\dp2installer\v3 /Y


:END