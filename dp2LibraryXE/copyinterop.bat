echo copy sqlite.interop.dll to x86 and x64 from nuget packages directory

MD x86
MD x64

CD ..\packages\system.data.sqlite.core.1.0.110.*

copy .\runtimes\win-x86\native\netstandard2.0\sqlite.interop.dll ..\..\dp2libraryxe\x86
copy .\runtimes\win-x64\native\netstandard2.0\sqlite.interop.dll ..\..\dp2libraryxe\x64

CD ..\..\dp2libraryxe