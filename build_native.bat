dotnet clean -c Release

dotnet publish -c Release -r win-x64 -o Build

upx.exe Build/DEB.exe
