SET OutputDir=%1
Set DataDir=%2

REM s - Copies directories and subdirectories except empty ones.
REM I - If the destination does not exist and copying more than one file, assumes that destination must be a directory.
REM Y - Suppresses prompting to confirm you want to overwrite an existing destination file.
REM D - copy only files newer than the destination
xcopy /s /I /Y /D %DataDir% %OutputDir%
del %OutputDir%\.gitignore
