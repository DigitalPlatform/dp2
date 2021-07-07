md palm_app
cd palm_app

del *.* /Q
xcopy ..\..\palmcenter\bin\debug\*.dll /Y

xcopy ..\..\palmcenter\bin\debug\*.exe /Y

del *.vshost.exe /Q

xcopy ..\..\palmcenter\bin\debug\palmcenter.exe.config /Y

md x86
xcopy ..\..\palmcenter\bin\debug\x86 x86 /s /Y
md x64
xcopy ..\..\palmcenter\bin\debug\x64 x64 /s /Y

cd ..

..\ziputil palm_app palm_app.zip -t

xcopy palm_app.zip c:\publish\dp2installer\v3 /Y