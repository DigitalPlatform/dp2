echo copy sqlite.interop.dll to x86 and x64 from nuget packages directory

MD x86
MD x64

CD ..\packages\system.data.sqlite.core.*

copy .\build\net40\x86\sqlite.interop.dll ..\..\dp2libraryxe\x86
copy .\build\net40\x64\sqlite.interop.dll ..\..\dp2libraryxe\x64

CD ..\..\dp2libraryxe