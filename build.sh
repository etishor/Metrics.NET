mono .nuget/NuGet.exe restore Metrics.sln 

xbuild Metrics.Sln /p:Configuration="Debug"
xbuild Metrics.Sln /p:Configuration="Release"

mono ./packages/xunit.runner.console.2.0.0/tools/xunit.console.exe ./bin/Debug/Tests/Metrics.Tests.dll -parallel none
mono ./packages/xunit.runner.console.2.0.0/tools/xunit.console.exe ./bin/Release/Tests/Metrics.Tests.dll -parallel none