# [ArchiSteamFarm](https://github.com/JustArchiNET/ArchiSteamFarm) - FreeGamesMonitor

Automatically detects and claims free Steam games via [ArchiSteamFarm](https://github.com/JustArchiNET/ArchiSteamFarm) (ASF) IPC.

Runs alongside ASF whenever a free game appears on Steam, it gets added to your account automatically.

---

## Installation

### Step 1 - Install .NET 10 Runtime

#### Linux (Ubuntu, Debian, Raspberry Pi)
```bash
sudo apt update && sudo apt install -y dotnet-runtime-10.0
```

Verify:
```bash
dotnet --version
```

#### Windows
Download and install from: https://dotnet.microsoft.com/en-us/download/dotnet/10.0
Under **Run apps - Runtime**, click **Download x64** and run the installer.

#### macOS
```bash
brew install dotnet-runtime
```

---

### Step 2 - Enable IPC in ASF

Create or edit `ASF/Core/config/ASF.json`:
```json
{
  "IPC": true,
  "IPCPassword": "yourpassword"
}
```
> ASF auto-detects this change, no restart needed.

---

### Step 3 - Download FreeGamesMonitor

Go to the [Releases page](../../releases/latest) and download the zip for your system:

| Platform | File |
|---|---|
| Linux x64 (Ubuntu, Debian) | `FreeGamesMonitor-linux-x64.zip` |
| Linux arm64 (Raspberry Pi 4/5) | `FreeGamesMonitor-linux-arm64.zip` |
| Linux arm (Raspberry Pi 2/3) | `FreeGamesMonitor-linux-arm.zip` |
| Windows x64 | `FreeGamesMonitor-win-x64.zip` |
| Windows arm64 | `FreeGamesMonitor-win-arm64.zip` |
| macOS Intel | `FreeGamesMonitor-osx-x64.zip` |
| macOS Apple Silicon | `FreeGamesMonitor-osx-arm64.zip` |

---

### Step 4 - Extract

#### Linux / macOS
```bash
unzip FreeGamesMonitor-linux-x64.zip -d ~/FreeGamesMonitor
cd ~/FreeGamesMonitor
```

#### Windows
Right-click the zip → Extract All → choose a folder like `C:\FreeGamesMonitor`

---

### Step 5 - Configure

#### Linux / macOS
```bash
cp config.example.json config.json
nano config.json
```

#### Windows
Copy `config.example.json`, rename the copy to `config.json`, open with Notepad.

Fill in your values:
```json
{
  "AsfUrl": "http://localhost:1242",
  "AsfPassword": "yourpassword",
  "BotName": "yourbotname",
  "SeenFile": "seen_games.json",
  "CheckIntervalHours": 24
}
```

| Field | What to put |
|---|---|
| `AsfUrl` | Leave as-is if ASF is on the same machine |
| `AsfPassword` | The password you set in ASF.json |
| `BotName` | Your bot name from ASF config |
| `CheckIntervalHours` | How often to check in hours (default 24) |

---

### Step 6 - Test Run

```bash
cd ~/FreeGamesMonitor
dotnet FreeGamesMonitor.dll
```

You should see:
```
FreeGamesMonitor started.
Checking for free games...
```

If it works, stop it with `Ctrl+C` and set it up as a service below.

---

## Recommended - Run as a Service

Running as a service means FreeGamesMonitor starts automatically on boot and runs in the background without keeping a terminal open.

### Linux / Raspberry Pi

```bash
sudo nano /etc/systemd/system/freegames.service
```

Paste this - replace `your-user` with your Linux username:
```ini
[Unit]
Description=ASF Free Games Monitor
After=network.target

[Service]
Type=simple
User=your-user
WorkingDirectory=/home/your-user/FreeGamesMonitor
ExecStart=/usr/bin/dotnet /home/your-user/FreeGamesMonitor/FreeGamesMonitor.dll
Restart=on-failure

[Install]
WantedBy=multi-user.target
```

Enable and start:
```bash
sudo systemctl daemon-reload
sudo systemctl enable freegames
sudo systemctl start freegames
sudo systemctl status freegames
```

Check live logs:
```bash
sudo journalctl -u freegames -f
```

### Windows - run on startup
1. Press `Win+R` → type `shell:startup` → Enter
2. Create a `.bat` file in that folder with:
```bat
dotnet C:\FreeGamesMonitor\FreeGamesMonitor.dll
```
It will start automatically when Windows boots.

---

## Building from Source

Use this if you want to build it yourself instead of using the release zip.

### Step 1 - Install .NET 10 SDK
```bash
sudo apt update && sudo apt install -y dotnet-sdk-10.0
```

### Step 2 - Get the source files
Clone the repo or download and extract the source zip into a folder called `FreeGamesMonitor`.

### Step 3 - Build
```bash
cd ~/FreeGamesMonitor
dotnet publish -c Release
```

### Step 4 - Configure and run
```bash
cp config.example.json bin/Release/net10.0/publish/config.json
nano bin/Release/net10.0/publish/config.json
dotnet bin/Release/net10.0/publish/FreeGamesMonitor.dll
```

---

## Service Management

| Action | Command |
|---|---|
| Start | `sudo systemctl start freegames` |
| Stop | `sudo systemctl stop freegames` |
| Restart | `sudo systemctl restart freegames` |
| Status | `sudo systemctl status freegames` |
| Logs | `sudo journalctl -u freegames -f` |

---

## Updating

1. Stop the service: `sudo systemctl stop freegames`
2. Download the new zip from [Releases](../../releases/latest)
3. Extract and overwrite old files - **keep your `config.json`**
4. Start again: `sudo systemctl start freegames`

---

## Troubleshooting

**`dotnet: command not found`**
→ .NET is not installed. Redo Step 1.

**`config.json not found` on first run**
→ The program creates a default one and exits. Fill it in and run again.

**`Unauthorized` or `403` in logs**
→ `AsfPassword` in `config.json` doesn't match the password in ASF's `ASF.json`.

**`No new free games found` every time**
→ Working correctly - it only redeems games it hasn't seen before.

**Bot not found error**
→ Check `BotName` in `config.json` matches your bot name in ASF exactly.

---

## Architecture

- ASF handles your Steam bot session and card farming
- FreeGamesMonitor runs independently and communicates with ASF via IPC
- Both run as separate services
- IPC endpoint: `http://localhost:1242`
