.nuget\NuGet.exe restore Metrics.sln

set MSBUILD="c:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe"
set XUNIT=".\packages\xunit.runners.1.9.2\\tools\xunit.console.clr4.exe"


%MSBUILD% Metrics.Sln /p:Configuration="Debug"
if %errorlevel% neq 0 exit /b %errorlevel%

%MSBUILD% Metrics.Sln /p:Configuration="Release"
if %errorlevel% neq 0 exit /b %errorlevel%

%XUNIT% .\bin\Debug\Tests\Metrics.Tests.dll
if %errorlevel% neq 0 exit /b %errorlevel%

%XUNIT% .\bin\Release\Tests\Metrics.Tests.dll
if %errorlevel% neq 0 exit /b %errorlevel%