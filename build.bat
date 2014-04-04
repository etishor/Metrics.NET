c:\WINDOWS\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe Metrics.Sln /p:Configuration="Release"

md .\Publishing\lib
md .\Publishing\lib\net451

copy .\bin\release\metrics.dll .\Publishing\lib\net451\
copy .\bin\release\metrics.xml .\Publishing\lib\net451\
copy .\bin\release\metrics.pdb .\Publishing\lib\net451\
copy .\bin\release\nancy.metrics.dll .\Publishing\lib\net451\
copy .\bin\release\nancy.metrics.xml .\Publishing\lib\net451\
copy .\bin\release\nancy.metrics.pdb .\Publishing\lib\net451\
