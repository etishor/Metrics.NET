mono .nuget/NuGet.exe restore Metrics.sln 

xbuild Metrics.Sln /p:Configuration="MonoDebug"
xbuild Metrics.Sln /p:Configuration="MonoRelease"

mono ./packages/xunit.runners.1.9.2/tools/xunit.console.clr4.exe ./bin/MonoDebug/Tests/Metrics.Tests.dll
mono ./packages/xunit.runners.1.9.2/tools/xunit.console.clr4.exe ./bin/MonoRelease/Tests/Metrics.Tests.dll

mono ./packages/xunit.runners.1.9.2/tools/xunit.console.clr4.exe ./bin/MonoDebug/Tests/Metrics.Log4Net.Tests.dll
mono ./packages/xunit.runners.1.9.2/tools/xunit.console.clr4.exe ./bin/MonoRelease/Tests/Metrics.Log4Net.Tests.dll