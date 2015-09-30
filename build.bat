@echo off
"%PROGRAMFILES(x86)%\MSBuild\14.0\Bin\MsBuild.exe" build.proj /t:%1 /p:Version=%2