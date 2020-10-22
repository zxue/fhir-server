#!/bin/bash

mv /home/vscode/.vscode-server/data/Machine/settings.json settings.backup.json
cp /home/tmp/vscode-settings.json /home/vscode/.vscode-server/data/Machine/settings.json
cp /home/tmp/vscode-settings.json /home/vscode/.vscode-remote/data/Machine/settings.json