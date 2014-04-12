c:\WINDOWS\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe Metrics.Sln /p:Configuration="Debug"
if %errorlevel% neq 0 exit /b %errorlevel%

c:\WINDOWS\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe Metrics.Sln /p:Configuration="Release"
if %errorlevel% neq 0 exit /b %errorlevel%

c:\WINDOWS\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe Metrics.Sln /p:Configuration="MonoDebug"
if %errorlevel% neq 0 exit /b %errorlevel%

c:\WINDOWS\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe Metrics.Sln /p:Configuration="MonoRelease"
if %errorlevel% neq 0 exit /b %errorlevel%

c:\WINDOWS\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe Metrics.Sln /p:Configuration="Net4.0Debug"
if %errorlevel% neq 0 exit /b %errorlevel%

c:\WINDOWS\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe Metrics.Sln /p:Configuration="Net4.0Release"
if %errorlevel% neq 0 exit /b %errorlevel%

.\packages\xunit.runners.1.9.2\tools\xunit.console.clr4.exe .\bin\Debug\Tests\Metrics.Tests.dll
if %errorlevel% neq 0 exit /b %errorlevel%

.\packages\xunit.runners.1.9.2\tools\xunit.console.clr4.exe .\bin\Release\Tests\Metrics.Tests.dll
if %errorlevel% neq 0 exit /b %errorlevel%

.\packages\xunit.runners.1.9.2\tools\xunit.console.clr4.exe .\bin\MonoDebug\Tests\Metrics.Tests.dll
if %errorlevel% neq 0 exit /b %errorlevel%

.\packages\xunit.runners.1.9.2\tools\xunit.console.clr4.exe .\bin\MonoRelease\Tests\Metrics.Tests.dll
if %errorlevel% neq 0 exit /b %errorlevel%

.\packages\xunit.runners.1.9.2\tools\xunit.console.clr4.exe .\bin\Net4.0Debug\Tests\Metrics.Tests.dll
if %errorlevel% neq 0 exit /b %errorlevel%

.\packages\xunit.runners.1.9.2\tools\xunit.console.clr4.exe .\bin\Net4.0Release\Tests\Metrics.Tests.dll
if %errorlevel% neq 0 exit /b %errorlevel%