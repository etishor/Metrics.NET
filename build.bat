.nuget\NuGet.exe restore Metrics.sln

set MSBUILD="C:\Program Files (x86)\MSBuild\12.0\Bin\MSBuild.exe"
set XUNIT=".\packages\xunit.runners.1.9.2\tools\xunit.console.clr4.exe"

%MSBUILD% Metrics.Sln /p:Configuration="Debug"
if %errorlevel% neq 0 exit /b %errorlevel%

%MSBUILD% Metrics.Sln /p:Configuration="Release"
if %errorlevel% neq 0 exit /b %errorlevel%

%MSBUILD% Metrics.Sln /p:Configuration="MonoDebug"
if %errorlevel% neq 0 exit /b %errorlevel%

%MSBUILD% Metrics.Sln /p:Configuration="MonoRelease"
if %errorlevel% neq 0 exit /b %errorlevel%


%XUNIT% .\bin\Debug\Tests\Metrics.Tests.dll
if %errorlevel% neq 0 exit /b %errorlevel%

%XUNIT% .\bin\Release\Tests\Metrics.Tests.dll
if %errorlevel% neq 0 exit /b %errorlevel%

%XUNIT% .\bin\MonoDebug\Tests\Metrics.Tests.dll
if %errorlevel% neq 0 exit /b %errorlevel%

%XUNIT% .\bin\MonoRelease\Tests\Metrics.Tests.dll
if %errorlevel% neq 0 exit /b %errorlevel%



%XUNIT% .\bin\Debug\Tests\Metrics.Log4Net.Tests.dll
if %errorlevel% neq 0 exit /b %errorlevel%

%XUNIT% .\bin\Release\Tests\Metrics.Log4Net.Tests.dll
if %errorlevel% neq 0 exit /b %errorlevel%

%XUNIT% .\bin\MonoDebug\Tests\Metrics.Log4Net.Tests.dll
if %errorlevel% neq 0 exit /b %errorlevel%

%XUNIT% .\bin\MonoRelease\Tests\Metrics.Log4Net.Tests.dll
if %errorlevel% neq 0 exit /b %errorlevel%