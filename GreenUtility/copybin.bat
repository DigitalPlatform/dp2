md temp

xcopy bin\debug\greenutility.exe temp /Y
xcopy bin\debug\*.dll temp /Y

..\ziputil temp c:\publish\dp2circulation\v3\greenutility.zip -t -b


