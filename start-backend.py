import subprocess
import json
import os

cli_appsettings_full_name = os.path.join(__file__, "cli", "appsettings.json")
appsettings = None
with open(cli_appsettings_full_name, "r") as file:
    appsettings = json.load(file)

backend_executable_full_name = os.path.join(__file__, "backend", "LiteTorrent.Backend")

subprocess.Popen([backend_executable_full_name, f"{appsettings['BackendAddress']}"], shell=True)
