echo copy sqlite.interop.dll to x86 and x64 from nuget packages directory ...

MD x86
MD x64

REM CD ..\packages\system.data.sqlite.core.1.0.110.*
CD ..\packages\system.data.sqlite.core.1.0.117.*

REM copy .\build\net46\x86\sqlite.interop.dll ..\..\dp2circulation\x86
REM copy .\build\net46\x64\sqlite.interop.dll ..\..\dp2circulation\x64

copy .\runtimes\win-x86\native\netstandard2.0\sqlite.interop.dll ..\..\dp2circulation\x86
copy .\runtimes\win-x64\native\netstandard2.0\sqlite.interop.dll ..\..\dp2circulation\x64

CD ..\..\dp2circulation