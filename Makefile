.PHONY: all
all: win mac linux

.PHONY: win
win:
	dotnet publish TinyIV.App -c release -r win10-x64 -o publish/win

.PHONY: linux
linux:
	dotnet publish TinyIV.App -c release -r linux-x64 -o publish/linux

.PHONY: mac
mac:
	dotnet publish TinyIV.App -c release -r osx.10.14-x64 -o publish/mac

.PHONY: clean
clean:
	dotnet clean
	rm -rf **/bin **/obj
	rm -rf publish