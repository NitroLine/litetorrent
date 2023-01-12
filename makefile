all: build copy

copy: build
	cp -i configure.py ./litetorrent-build/configure.py
	cp -i start-backend.py ./litetorrent-build/start-backend.py
	
build:
	dotnet build ./src/LiteTorrent.Backend/LiteTorrent.Backend.csproj -o ./litetorrent-build/backend
	dotnet build ./src/LiteTorrent.Client.Cli/LiteTorrent.Client.Cli.csproj -o ./litetorrent-build/cli
