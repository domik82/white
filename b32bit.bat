call tools\nant\NAnt.exe -nologo -buildfile:build\custom-nant.build compile.custom.nant -logfile:a.log -D:processor-type=32bit
call w %*