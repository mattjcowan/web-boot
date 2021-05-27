
SET CDIR=%~dp0
SET CDIR=%CDIR:~0,-1%
cd $CDIR

call rimraf ../data/test-extensions/Sample.Extension

rem see: https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-publish
dotnet publish -f net5.0 -r win10-x64 -c Development --no-dependencies --no-self-contained -o ../data/test-extensions/Sample.Extension