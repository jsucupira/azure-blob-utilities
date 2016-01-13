@echo off

:: This bat will create the nuget package and place it in the shared drive

:: setup package name
set "package=%~1"
goto :packageCheck
:packagePrompt
set /p "package=Enter package name: "
:packageCheck
if "%package%"=="" goto :packagePrompt

:: set version number
set "version=%~2"
goto :versionCheck
:versionPrompt
set /p "version=Enter version number: "
:versionCheck
if "%version%"=="" goto :versionPrompt

..\.nuget\nuget pack ..\%package%\%package%.csproj -Prop Configuration=Release -includereferencedprojects -Version %version%
pause