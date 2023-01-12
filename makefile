all: build copy

copy: build
	cp -i configure.py $(OUTPUT_DIR)/configure.py
	cp -i start-backend.py $(OUTPUT_DIR)/start-backend.py
	
build:
	dotnet build ./src/LiteTorrent.Backend/LiteTorrent.Backend.csproj -o $(OUTPUT_DIR)/backend
	dotnet build ./src/LiteTorrent.Client.Cli/LiteTorrent.Client.Cli.csproj -o $(OUTPUT_DIR)/cli
