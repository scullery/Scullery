@echo off
setlocal

if not exist version.cmd goto nover
call version.cmd

for %%I in (
    Scullery.Core
    Scullery.EntityFrameworkCore
    Scullery
) do (
    set _ASSEMBLY=%%I
    call :pub
)

goto end

:pub

rem *
rem * Publish
rem *

pushd build\%_ASSEMBLY%
nuget push %_ASSEMBLY%.%_VERSION%.nupkg -ApiKey %_NUGETKEY% -Source https://api.nuget.org/v3/index.json
popd

exit /b

:nover
echo No version file specified
goto end

:end
