.nuget\NuGet.exe restore Metrics.sln

set MSBUILD="C:\Program Files (x86)\MSBuild\12.0\Bin\MSBuild.exe"
set XUNIT=".\packages\xunit.runners.2.0.0-beta5-build2785\tools\xunit.console.exe"

rd /S /Q .\bin\Debug
rd /S /Q .\bin\Release

%MSBUILD% Metrics.Sln /p:Configuration="Debug"
if %errorlevel% neq 0 exit /b %errorlevel%

%MSBUILD% Metrics.Sln /p:Configuration="Release"
if %errorlevel% neq 0 exit /b %errorlevel%

%XUNIT% .\bin\Debug\Tests\Metrics.Tests.dll
if %errorlevel% neq 0 exit /b %errorlevel%

%XUNIT% .\bin\Release\Tests\Metrics.Tests.dll
if %errorlevel% neq 0 exit /b %errorlevel%