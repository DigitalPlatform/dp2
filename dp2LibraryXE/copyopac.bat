md opac_app
cd opac_app

xcopy \cs4.0\newopac\*.asax /Y
xcopy \cs4.0\newopac\*.aspx /Y
xcopy \cs4.0\newopac\*.aspx.cs /Y

del about.* /Q
del search2.* /Q
del sample.* /Q
del simple.* /Q
del site.* /Q
del testlogin.* /Q
del *.txt /Q
del start.xml /Q


md bin
cd bin
xcopy \cs4.0\newopac\bin\*.dll /Y
del nanchangsso.dll /Q
xcopy \cs4.0\newopac\bin\en-US en-US /Y
xcopy \cs4.0\newopac\bin\zh-CN zh-CN /Y
cd ..

md app_code
cd app_code
xcopy \cs4.0\newopac\app_code\*.* /Y
cd ..

cd ..

