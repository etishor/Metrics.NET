rd /S /Q .\Publishing\lib

call build.bat
if %errorlevel% neq 0 exit /b %errorlevel%

md .\Publishing\lib
md .\Publishing\lib\net451
md .\Publishing\lib\net45

copy .\bin\release\metrics.dll .\Publishing\lib\net451\
copy .\bin\release\metrics.xml .\Publishing\lib\net451\
copy .\bin\release\metrics.pdb .\Publishing\lib\net451\

copy .\bin\MonoRelease\metrics.dll .\Publishing\lib\net45\
copy .\bin\MonoRelease\metrics.xml .\Publishing\lib\net45\
copy .\bin\MonoRelease\metrics.pdb .\Publishing\lib\net45\

copy .\bin\Net4.0Release\metrics.dll .\Publishing\lib\net40\
copy .\bin\Net4.0Release\metrics.xml .\Publishing\lib\net40\
copy .\bin\Net4.0Release\metrics.pdb .\Publishing\lib\net40\

copy .\bin\release\nancy.metrics.dll .\Publishing\lib\net451\
copy .\bin\release\nancy.metrics.xml .\Publishing\lib\net451\
copy .\bin\release\nancy.metrics.pdb .\Publishing\lib\net451\

copy .\bin\MonoRelease\nancy.metrics.dll .\Publishing\lib\net45\
copy .\bin\MonoRelease\nancy.metrics.xml .\Publishing\lib\net45\
copy .\bin\MonoRelease\nancy.metrics.pdb .\Publishing\lib\net45\

copy .\bin\release\owin.metrics.dll .\Publishing\lib\net451\
copy .\bin\release\owin.metrics.xml .\Publishing\lib\net451\
copy .\bin\release\owin.metrics.pdb .\Publishing\lib\net451\

copy .\bin\MonoRelease\owin.metrics.dll .\Publishing\lib\net45\
copy .\bin\MonoRelease\owin.metrics.xml .\Publishing\lib\net45\
copy .\bin\MonoRelease\owin.metrics.pdb .\Publishing\lib\net45\


.\.nuget\NuGet.exe pack .\Publishing\Metrics.Net.nuspec -OutputDirectory .\Publishing
if %errorlevel% neq 0 exit /b %errorlevel%

.\.nuget\NuGet.exe pack .\Publishing\Nancy.Metrics.nuspec -OutputDirectory .\Publishing
if %errorlevel% neq 0 exit /b %errorlevel%

.\.nuget\NuGet.exe pack .\Publishing\Owin.Metrics.nuspec -OutputDirectory .\Publishing
if %errorlevel% neq 0 exit /b %errorlevel%