all: build copy

copy: build
	sudo cp -i configure.py ~/Desktop/litetorrent-build/configure.py
	sudo cp -i start-backend.py ~/Desktop/litetorrent-build/start-backend.py
	

build:
	sudo dotnet build ./src/LiteTorrent.Backend/LiteTorrent.Backend.csproj -o ~/Desktop/litetorrent-build/backend
	sudo dotnet build ./src/LiteTorrent.Client.Cli/LiteTorrent.Client.Cli.csproj -o ~/Desktop/litetorrent-build/cli
