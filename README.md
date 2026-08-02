# AutoRes - Monitor Resolution Switcher

> Automatically change your monitor resolution based on which application is running.

![AutoRes Interface](docs/autores-preview.png)

## Features

✨ **Smart Resolution Switching**
- Automatically detects when a game/application launches
- Changes your monitor resolution to your saved game profile
- Restores normal resolution when the game closes

🎮 **Game Profiles**
- Save multiple game profiles with custom resolutions
- Each game can have its own resolution setting
- Easily manage and delete profiles
- Profiles persist between sessions

🌙 **Dark Mode**
- Built-in dark mode for comfortable use
- Toggle with a single click
- Settings saved automatically

⏫ **Auto-Start**
- Option to start with Windows automatically
- Runs in system tray (minimize to tray)
- Restore by clicking tray icon

🔇 **Start Minimized**
- Option to start the app minimized to system tray
- No window pops up on startup
- App still monitors games in the background

💾 **Persistent Settings**
- All settings automatically saved to `%APPDATA%\AutoRes\settings.xml`
- Profiles, normal resolution, preferences all preserved
- No manual configuration needed—everything works out of the box

## Installation

### Option 1: Download Release
1. Go to [Releases](https://github.com/kuhmori/AutoRes/releases)
2. Download the latest `AutoRes.zip` release
3. Extract the zip file
4. Run `AutoRes.exe`

### Option 2: Build from Source
**Requirements:**
- Visual Studio 2019+ or Visual Studio Build Tools
- .NET Framework 4.8 Developer Pack

```bash
git clone https://github.com/kuhmori/AutoRes.git
cd AutoRes
dotnet build -c Release
cd AutoRes\bin\Release\net48
AutoRes.exe
```

## Requirements

- **Windows 7 or later** (tested on Windows 10/11)
- **.NET Framework 4.8** or later
- Administrator privileges (for changing display settings)

## Usage

### Setting Up Your First Profile

1. **Set Normal Resolution:**
   - Click **"Set Normal"** button
   - This saves your default monitor resolution

2. **Add a Game Profile:**
   - Select a game/application from the dropdown (e.g., `notepad`)
   - Enter desired resolution (width x height) in the text fields
   - Or leave blank to use current resolution
   - Click **"Save Game"** button
   - Profile appears in the list below

3. **Monitor Your Games:**
   - Status shows "Monitoring X game(s)"
   - Launch your game
   - Resolution automatically changes to your saved profile
   - When game closes, resolution restores to normal

### Managing Profiles

- **View Profiles:** List shows all saved game profiles with their resolutions
- **Delete Profile:** Select a profile and click **"Delete"**
- **Dark Mode:** Click **"Dark Mode"** to toggle
- **Auto-Start:** Check **"Auto-Start"** to run at Windows startup

## How It Works

1. **Process Detection:** App monitors running processes every second
2. **Profile Matching:** Checks if any running process matches saved game profiles
3. **Resolution Change:** Uses Windows Display Settings API (`ChangeDisplaySettings`)
4. **Automatic Restore:** Returns to normal resolution when game closes

## Settings Location

Settings are saved to:
```
%APPDATA%\AutoRes\settings.xml
```

Example path:
```
C:\Users\{YourUsername}\AppData\Roaming\AutoRes\settings.xml
```

## Troubleshooting

### "Failed to change to {width}x{height}. Check if resolution is supported."
- The resolution you entered isn't supported by your monitor
- Check Windows Display Settings for supported resolutions
- Try a standard resolution like 1920x1080, 1280x720, etc.

### Resolution doesn't change when game launches
- Make sure the **process name** is correct (check Task Manager)
- If game uses a launcher, you may need to profile the launcher name
- Try adding the game profile again with correct process name

### App won't start
- Make sure **Windows' explorer.exe is running** (it's required)
- Try running as Administrator
- Check that **.NET Framework 4.8** is installed

### Settings not saving
- Ensure you have write permissions to `%APPDATA%\AutoRes\`
- Try running as Administrator

## Configuration

No manual configuration needed! Everything is managed through the GUI:

- **Normal Resolution:** Set via "Set Normal" button
- **Game Profiles:** Add/delete via "Save Game" and "Delete" buttons  
- **Preferences:** Dark Mode and Auto-Start settings are saved automatically

Settings are automatically persisted to `%APPDATA%\AutoRes\settings.xml` (you never need to manually edit this file).

## Common Processes Filtered Out

The app automatically filters these commonly-used processes to keep the list clean:

```
svchost, explorer, dwm, lsass, services, winlogon, csrss, wininit,
conhost, taskhostw, SearchIndexer, System, Registry, smss,
SearchProtocolHost, SettingSyncHost, WmiPrvSE, OneDrive, TeamViewer,
CCleaner, avast, avgnt, Avira, BitDefender, McAfee, Norton, kaspersky,
nod32, WindowsUpdates, TiWorker, ntoskrnl, Idle, chrome, edge, firefox, iexplore
```

**Note:** This is a heuristic filter that may be updated in future releases. If your game is being filtered out, check Task Manager for the actual process name and report it as an issue.

This keeps the process list focused on actual games and applications.

## Architecture

```
AutoRes/
├── Program.cs                 # Application entry point
├── MainForm.cs               # Main UI and monitoring logic
├── MainForm.Designer.cs      # UI controls layout
├── AppSettings.cs            # Settings persistence
├── ResolutionHelper.cs       # Windows Display API wrapper
└── AutostartHelper.cs        # Windows Registry autostart
```

**Key Technologies:**
- **Windows Forms** - UI framework
- **XML Serialization** - Settings persistence
- **Windows API (user32.dll)** - Display mode changes
- **Process Monitoring** - .NET Process class

## Contributing

Contributions welcome! To contribute:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## Issues & Feedback

Found a bug or have a feature request? [Open an issue](https://github.com/kuhmori/AutoRes/issues)

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Disclaimer

- This tool modifies your display settings. Use at your own risk.
- Some games may override resolution changes—autostart the app before launching games in those cases.
- Always test with a game before relying on AutoRes for important work.

## Changelog

### v1.0.0 (2024)
- Initial release
- Game profile management
- Auto-detection and resolution switching
- Dark mode support
- System tray minimization
- Auto-start with Windows
- Settings persistence

---

**Made with ❤️ for gamers and resolution switchers everywhere**

Need help? Check the [Issues](https://github.com/kuhmori/AutoRes/issues) page or create a new issue!
