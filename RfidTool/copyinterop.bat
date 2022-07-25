echo copy sqlite.interop.dll to x86 and x64 from nuget packages directory

MD x86
MD x64

copy .\bin\debug\x64\SQLite.Interop.dll .\x64 /Y
copy .\bin\debug\x86\SQLite.Interop.dll .\x86 /Y

