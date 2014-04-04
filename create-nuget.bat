rd /S /Q .\Publishing\lib

call build.bat

md .\Publishing\lib
md .\Publishing\lib\net451

copy .\bin\release\metrics.dll .\Publishing\lib\net451\
copy .\bin\release\metrics.xml .\Publishing\lib\net451\
copy .\bin\release\metrics.pdb .\Publishing\lib\net451\
copy .\bin\release\nancy.metrics.dll .\Publishing\lib\net451\
copy .\bin\release\nancy.metrics.xml .\Publishing\lib\net451\
copy .\bin\release\nancy.metrics.pdb .\Publishing\lib\net451\

.\.nuget\NuGet.exe pack .\Publishing\Metrics.Net.nuspec -OutputDirectory .\Publishing
.\.nuget\NuGet.exe pack .\Publishing\NancyFx.Metrics.nuspec -OutputDirectory .\Publishing