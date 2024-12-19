echo copy sqlite.interop.dll to x86 and x64 from nuget packages directory ...

MD x86
MD x64

REM CD ..\packages\system.data.sqlite.core.1.0.118.*
CD ..\packages\Stub.System.Data.SQLite.Core.NetFramework.1.0.118.*


copy .\build\net46\x86\sqlite.interop.dll ..\..\dp2circulation\x86
copy .\build\net46\x64\sqlite.interop.dll ..\..\dp2circulation\x64

REM copy .\runtimes\win-x86\native\netstandard2.0\sqlite.interop.dll ..\..\dp2circulation\x86
REM copy .\runtimes\win-x64\native\netstandard2.0\sqlite.interop.dll ..\..\dp2circulation\x64

CD ..\..\dp2circulation