@echo off

set szip="C:\Program Files\7-Zip\7z.exe"
if not exist %szip% (
	echo Error, 7-Zip is not installed.
	pause
	exit
)

set release_dir=%cd%\..\NCFDDClient\bin\Release
set compile_dir=%release_dir%\net8.0\
if not exist "%compile_dir%" (	
	echo Error, compiled release directory not found.
	pause
	exit
)

set pack_dir=%release_dir%\NCFDDClient\

rmdir "%pack_dir%" /s /q
xcopy "%compile_dir%" "%pack_dir%" /q
del "%pack_dir%\.env"
del "%pack_dir%\NCFDDClient.exe"
xcopy "%cd%\Package Files\.env.example" "%pack_dir%"
xcopy "%cd%\Package Files\install.sh" "%pack_dir%"
del NCFDDClient.tar.gz
cls

%szip% a -ttar "%release_dir%\NCFDDClient.tar" "%pack_dir%"
%szip% a -tgzip NCFDDClient.tar.gz "%release_dir%\NCFDDClient.tar"

rmdir "%pack_dir%" /s /q
del "%release_dir%\NCFDDClient.tar"

pause