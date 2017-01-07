rd /S /Q .\Publishing\lib

rem call build.net40.bat
rem if %errorlevel% neq 0 exit /b %errorlevel%

md .\Publishing\lib
md .\Publishing\lib\net40

copy .\bin\release\metrics.dll .\Publishing\lib\net40\
copy .\bin\release\metrics.xml .\Publishing\lib\net40\
copy .\bin\release\metrics.pdb .\Publishing\lib\net40\

.\.nuget\NuGet.exe pack .\Publishing\Metrics.Net.net40.nuspec -OutputDirectory .\Publishing


if %errorlevel% neq 0 exit /b %errorlevel%
