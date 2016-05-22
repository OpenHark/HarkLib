
all:
	@make -B make clean

COMMON_SRC_FILES = lib\Settings.cs

MAKE_SRC_FILES = Make.cs \
	lib\Compilation.cs \
	lib\CompilationCs.cs \
	lib\CompilationFs.cs \
	lib\Cache.cs
	
CLEAN_SRC_FILES = Clean.cs

make:
	@csc -out:../.make.exe $(MAKE_SRC_FILES) $(COMMON_SRC_FILES) -nologo

clean:
	@csc -out:../.clean.exe $(CLEAN_SRC_FILES) $(COMMON_SRC_FILES) -nologo
