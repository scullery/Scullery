@echo off
setlocal

if not exist version.cmd goto nover
call version.cmd

set _CONFIGURATION=Release

for %%I in (
    Scullery.Core
    Scullery.EntityFrameworkCore
) do (
    set _ASSEMBLY=%%I
    call :package
)

:meta

rem *
rem * Build meta package
rem *

set _ASSEMBLY=Scullery
mkdir build\%_ASSEMBLY%
copy /y src\%_ASSEMBLY%.nuspec build\%_ASSEMBLY%

call :create

goto end

:package

rem *
rem * Build
rem *

pushd src\%_ASSEMBLY%
dotnet build --configuration %_CONFIGURATION%
popd

rem *
rem * Prepare directory
rem *

mkdir build
mkdir build\%_ASSEMBLY%
mkdir build\%_ASSEMBLY%\lib
mkdir build\%_ASSEMBLY%\lib\net6.0

copy /y src\%_ASSEMBLY%\bin\%_CONFIGURATION%\net6.0\%_ASSEMBLY%.dll build\%_ASSEMBLY%\lib\net6.0
copy /y src\%_ASSEMBLY%\%_ASSEMBLY%.nuspec build\%_ASSEMBLY%

call :create

exit /b

:create
rem *
rem * Create package
rem *

pushd build\%_ASSEMBLY%
nuget pack %_ASSEMBLY%.nuspec

rem *
rem * Remove existing local package
rem *

if not exist "%Source%\packages\%_ASSEMBLY%\%_VERSION%" goto skiprmdir
rmdir /q /s %Source%\packages\%_ASSEMBLY%\%_VERSION%
:skiprmdir

rem *
rem * Add local package
rem *

nuget add %_ASSEMBLY%.%_VERSION%.nupkg -source %Source%\packages
popd

rem *
rem * Remove cached pacakge
rem *

if not exist "%USERPROFILE%\.nuget\packages\%_ASSEMBLY%" goto skiprmdir2
rmdir /q /s %USERPROFILE%\.nuget\packages\%_ASSEMBLY%
:skiprmdir2

exit /b

:nover
echo No version file specified
goto end

:end
