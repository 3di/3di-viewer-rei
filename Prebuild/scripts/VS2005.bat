@rem Generates a solution (.sln) and a set of project files (.csproj, .vbproj, etc.)
@rem for Microsoft Visual Studio .NET 2005
cd ..
Prebuild.exe /target vs2005 /file prebuild.xml /build NET_2_0 /pause
