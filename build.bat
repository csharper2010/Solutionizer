@echo Off
SETLOCAL
set EnableNuGetPackageRestore=true 
%windir%\system32\windowspowershell\v1.0\powershell.exe .\psake.ps1 .\default.ps1 package
ENDLOCAL