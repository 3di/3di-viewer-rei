@rem Generates a solution (.sln) and a set of project files (.csproj) 
@rem for Microsoft Visual Studio .NET 2002
cd ..
Prebuild.exe /target vs2002 /file prebuild.xml /build NET_1_1 /pause
