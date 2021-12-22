md gate_app
cd gate_app

del *.* /Q
xcopy ..\..\dp2gate\bin\debug\*.dll /Y

xcopy ..\..\dp2gate\bin\debug\*.exe /Y

del *.vshost.exe /Q

xcopy ..\..\dp2gate\bin\debug\dp2gate.exe.config /Y

md x86
xcopy ..\..\dp2gate\bin\debug\x86 x86 /s /Y
md x64
xcopy ..\..\dp2gate\bin\debug\x64 x64 /s /Y

cd ..

..\ziputil gate_app gate_app.zip -t

xcopy gate_app.zip c:\publish\dp2installer\v3 /Y