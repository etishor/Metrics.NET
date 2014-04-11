c:\WINDOWS\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe Metrics.Sln /p:Configuration="Debug"
c:\WINDOWS\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe Metrics.Sln /p:Configuration="Release"
c:\WINDOWS\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe Metrics.Sln /p:Configuration="MonoDebug"
c:\WINDOWS\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe Metrics.Sln /p:Configuration="MonoRelease"
c:\WINDOWS\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe Metrics.Sln /p:Configuration="Net4.0Debug"
c:\WINDOWS\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe Metrics.Sln /p:Configuration="Net4.0Release"

.\packages\xunit.runners.1.9.2\tools\xunit.console.clr4.exe .\bin\Debug\Tests\Metrics.Tests.dll
.\packages\xunit.runners.1.9.2\tools\xunit.console.clr4.exe .\bin\Release\Tests\Metrics.Tests.dll
.\packages\xunit.runners.1.9.2\tools\xunit.console.clr4.exe .\bin\MonoDebug\Tests\Metrics.Tests.dll
.\packages\xunit.runners.1.9.2\tools\xunit.console.clr4.exe .\bin\MonoRelease\Tests\Metrics.Tests.dll
.\packages\xunit.runners.1.9.2\tools\xunit.console.clr4.exe .\bin\Net4.0Debug\Tests\Metrics.Tests.dll
.\packages\xunit.runners.1.9.2\tools\xunit.console.clr4.exe .\bin\Net4.0Release\Tests\Metrics.Tests.dll