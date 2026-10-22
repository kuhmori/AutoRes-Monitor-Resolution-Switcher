@echo off
setlocal
cd /d "%~dp0"
echo Building AutoRes...
echo (Closing any running AutoRes so the exe can be replaced)
taskkill /im AutoRes.exe /f >nul 2>&1

set "OUT=%~dp0..\Release"

where dotnet >nul 2>&1
if errorlevel 1 goto msbuild
dotnet build AutoRes.csproj -c Release -o "%OUT%" -nologo > build.log 2>&1
goto done

:msbuild
set "VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
if not exist "%VSWHERE%" goto notools
for /f "usebackq delims=" %%i in (`"%VSWHERE%" -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe`) do set "MSBUILD=%%i"
if not defined MSBUILD goto notools
"%MSBUILD%" AutoRes.csproj -restore -p:Configuration=Release "-p:OutDir=%OUT%\\" -nologo > build.log 2>&1
goto done

:notools
echo Neither the .NET SDK nor Visual Studio MSBuild was found. > build.log

:done
type build.log
echo.
findstr /c:"Build succeeded" build.log >nul && (echo Done: the new AutoRes.exe is in the Release folder) || (echo Build failed - see build.log)
pause
