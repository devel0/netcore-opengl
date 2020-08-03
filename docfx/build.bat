cd %~dp0

rd ..\docs /q /s

docfx

mkdir ..\docs\test

xcopy /s ..\test ..\docs\test /exclude:exclude.txt
