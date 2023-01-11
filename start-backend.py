import subprocess
import json
import os
import signal

def pass_func(s, f):
    pass

signal.signal(signal.SIGINT, pass_func)

current_file_dir = os.path.dirname(__file__)

cli_appsettings_full_name = os.path.join(current_file_dir, "cli", "appsettings.json")
appsettings = None
with open(cli_appsettings_full_name, "r") as file:
    appsettings = json.load(file)

backend_executable_full_name = os.path.join(current_file_dir, "backend", "LiteTorrent.Backend")
try:
    subprocess.call([backend_executable_full_name + f" {appsettings['BackendAddress']}"], shell=True)
except KeyboardInterrupt:
    pass
