cd content\shaders
set mgfxPath="C:\Program Files (x86)\MSBuild\MonoGame\v3.0\Tools\2MGFX.exe"

echo.
echo Deleting previously compiled shaders...

for /r %%x in (*.fx.2MGFX) do (
	call del "%%x"
)

echo Old shader files deleted!
echo.

echo.
echo Compiling shaders!

for /r %%x in (*.fx) do (
	call ""%mgfxPath%"" "%%x" "%%x.2MGFX"
	::echo Deleting %%x...
	call del "%%x"
)

echo Shaders compiled and source .fx files removed!
echo.