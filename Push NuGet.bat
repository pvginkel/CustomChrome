@echo off

setlocal enableextensions enabledelayedexpansion

pushd "%~dp0"

cd CustomChrome

for %%f in (*.nupkg) do (
    if "!PACKAGE!" == "" (
        set PACKAGE=%%f
    ) else (
        echo Multiple NuGet packages found. Make sure only one exists.
        goto eof
    )
)

..\Libraries\NuGet\NuGet.exe push %PACKAGE%

:eof

pause

popd
