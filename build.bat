.nuget\NuGet.exe restore Metrics.sln

set MSBUILD="C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe"
set XUNIT=".\packages\xunit.runner.console.2.0.0\tools\xunit.console.exe"

rd /S /Q .\bin\Debug
rd /S /Q .\bin\Release

%MSBUILD% Metrics.Sln /p:Configuration="Debug"
if %errorlevel% neq 0 exit /b %errorlevel%

%MSBUILD% Metrics.Sln /p:Configuration="Release"
if %errorlevel% neq 0 exit /b %errorlevel%

%XUNIT% .\bin\Debug\Tests\Metrics.Tests.dll -maxthreads 1
if %errorlevel% neq 0 exit /b %errorlevel%

%XUNIT% .\bin\Release\Tests\Metrics.Tests.dll -maxthreads 1
if %errorlevel% neq 0 exit /b %errorlevel%