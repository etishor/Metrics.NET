mono .nuget/NuGet.exe restore Metrics.sln 

xbuild Metrics.Sln /p:Configuration="Debug"
xbuild Metrics.Sln /p:Configuration="Release"

mono ./xunit.runners.2.0.0-beta5-build2785/tools/xunit.console.exe ./bin/Debug/Tests/Metrics.Tests.dll -parallel none
mono ./xunit.runners.2.0.0-beta5-build2785/tools/xunit.console.exe ./bin/Release/Tests/Metrics.Tests.dll -parallel none