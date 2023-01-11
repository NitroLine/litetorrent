import json
import os

def dump(path, obj):
    with open(path, "w") as file:
        json.dump(obj, file)

current_file_dir = os.path.dirname(__file__)
backend_appsettings_full_name = os.path.join(current_file_dir, "backend", "appsettings.json")
cli_appsettings_full_name = os.path.join(current_file_dir, "cli", "appsettings.json")

port = input("CliPort: ")
piece_dir = input("PieceDirectory: ")
sharedFile_dir = input("SharedFileDirectory: ")
hashTree_dir = input("HashTreeDirectory: ")
distributing_address = input("Distributing Address (Example: 90.0.0.1:8000): ")

backend_appsettings = {
    "LocalStorage": {
        "ShardDirectoryPath": piece_dir,
        "HashTreeDirectoryPath": hashTree_dir,
        "SharedFileDirectoryPath": sharedFile_dir
    },
    "Transport": {
        "TorrentEndpoint": distributing_address,
        "PeerId": "vostok"
    }
}

cli_appsettings = {
    "BackendAddress": port
}

dump(backend_appsettings_full_name, backend_appsettings)
dump(cli_appsettings_full_name, cli_appsettings)
