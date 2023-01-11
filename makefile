all: build copy

copy: build
	cp -i configure.py ~/Desktop/litetorrent-build/configure.py
	cp -i start-backend.py ~/Desktop/litetorrent-build/start-backend.py
	
build:
	dotnet build ./src/LiteTorrent.Backend/LiteTorrent.Backend.csproj -o ~/Desktop/litetorrent-build/backend
	dotnet build ./src/LiteTorrent.Client.Cli/LiteTorrent.Client.Cli.csproj -o ~/Desktop/litetorrent-build/cli
