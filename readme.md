# ASF Raspberry Pi Setup Notes

## Overview
FreeGamesMonitor is a .NET service that runs alongside ArchiSteamFarm (ASF) to detect and automatically claim free Steam games via ASF IPC.

You can keep ASF running for AFK hours/card farming while the monitor handles free game claiming in the background.

## Requirements
- ASF installed and configured
- .NET runtime available (example: `$HOME/.dotnet/dotnet`)
- Linux system with `systemd`

## Architecture
- ASF process handles Steam bot sessions
- FreeGamesMonitor process checks free app offers and submits claim actions
- Both communicate through ASF IPC

## Runtime Model
- ASF runs as a background service
- FreeGamesMonitor runs as an independent `systemd` service
- Communication occurs via ASF IPC (`http://localhost:1242`)

## Installation
### Source paths
- ASF binary: `~/ASF/Core/ArchiSteamFarm`
- Bot config: `~/ASF/Core/config/myaccount.json`
- Global config: `~/ASF/Core/config/ASF.json`
- Free games monitor project: `~/ASF/FreeGamesMonitor`

### Runtime paths (post-publish)
- Publish output: `~/ASF/FreeGamesMonitor/bin/Release/net10.0/publish/`
- .NET runtime: `$HOME/.dotnet/dotnet`

### Build and run (manual test)
```bash
cd ~/ASF/FreeGamesMonitor
dotnet publish -c Release
$HOME/.dotnet/dotnet bin/Release/net10.0/publish/FreeGamesMonitor.dll
```

### Add another bot
1. Go to [ASF WebConfigGenerator](https://justarchinet.github.io/ASF-WebConfigGenerator)
2. Generate bot config
3. Save to `~/ASF/Core/config/botname.json`
4. ASF auto-loads configs

## Service Management
### ASF service
- Start ASF: `sudo systemctl start asf`
- Stop ASF: `sudo systemctl stop asf`
- ASF logs: `sudo journalctl -u asf -f`

### FreeGamesMonitor service
1. Create service file:
   `sudo nano /etc/systemd/system/freegames.service`

2. Paste:
```ini
[Unit]
Description=ASF Free Games Monitor (C#)
After=network.target

[Service]
WorkingDirectory=/home/your-user/ASF/FreeGamesMonitor
ExecStart=/home/your-user/.dotnet/dotnet /home/your-user/ASF/FreeGamesMonitor/bin/Release/net10.0/publish/FreeGamesMonitor.dll
Restart=always
User=your-user

[Install]
WantedBy=multi-user.target
```

3. Enable service:
```bash
sudo systemctl daemon-reload
sudo systemctl enable freegames
```

4. Start service:
```bash
sudo systemctl start freegames
```

5. Check status:
```bash
sudo systemctl status freegames
```

6. View logs:
```bash
sudo journalctl -u freegames -f
```

## Build / Development
### Update or rebuild workflow
```bash
cd ~/ASF/FreeGamesMonitor
dotnet publish -c Release
sudo systemctl restart freegames
```

## Configuration
### IPC
- URL: `http://localhost:1242`
- Password: set in `ASF.json`

## Notes
- Runs continuously under `systemd`
- Timer is handled inside the application (no cron required)
- Built for ASF integration via IPC/API

