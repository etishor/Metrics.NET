c:\WINDOWS\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe Metrics.Sln /p:Configuration="Release"

md .\release\lib
md .\release\lib\net451

copy .\bin\release\metrics.dll .\release\lib\net451\
copy .\bin\release\metrics.xml .\release\lib\net451\
copy .\bin\release\metrics.pdb .\release\lib\net451\
copy .\bin\release\nancy.metrics.dll .\release\lib\net451\
copy .\bin\release\nancy.metrics.xml .\release\lib\net451\
copy .\bin\release\nancy.metrics.pdb .\release\lib\net451\
