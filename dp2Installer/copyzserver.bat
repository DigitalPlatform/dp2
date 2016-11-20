md zserver_app
cd zserver_app

del *.* /Q
xcopy ..\..\dp2zserver\bin\debug\*.dll /Y

xcopy ..\..\dp2zserver\bin\debug\*.exe /Y
rem xcopy ..\..\dp2zserver\bin\debug\*.pfx /Y

del *.vshost.exe /Q

xcopy ..\..\dp2zserver\bin\debug\dp2zserver.exe.config /Y

cd ..

..\ziputil zserver_app zserver_app.zip -t
..\ziputil zserver_data zserver_data.zip -t