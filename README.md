# AutoRes - Monitor Resolution Switcher

> Automatically change your monitor resolution based on which application is running.

![AutoRes Interface](autores-preview.png)

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

⏫ **Auto-Start & Tray**
- Option to start with Windows automatically
- Runs in the system tray (minimize or close to tray)
- Left-click the tray icon to show/hide, right-click for **Open** / **Exit**
- Only one copy runs at a time: launching it again just brings the window up

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
git clone https://github.com/kuhmori/AutoRes-Monitor-Resolution-Switcher.git
cd AutoRes-Monitor-Resolution-Switcher\AutoRes
dotnet build -c Release
bin\Release\AutoRes.exe
```

## Requirements

- **Windows 7 or later** (tested on Windows 10/11)
- **.NET Framework 4.8** or later
- Administrator privileges (for changing display settings)

## Usage

### Setting Up Your First Profile

1. **Desktop resolution:**
   - Pick your normal resolution (or click **Use current**) and click **Save**
   - AutoRes switches back to this when a game closes

2. **Add a game:**
   - Pick the game from the **Program** list (start it first, then click ↻), type its process name, or use **Browse…** to select its `.exe`
   - Pick a **Resolution** from the list of modes your monitor supports (or type one, e.g. `1728x1080`)
   - Click **Save game** — saving again for the same program updates it

3. **Monitor your games:**
   - The status pill shows "Watching X games"
   - Launch your game
   - Resolution automatically changes to your saved profile
   - When game closes, resolution restores to normal

### Managing Profiles

- **View Profiles:** List shows all saved game profiles with their resolutions
- **Edit Profile:** Click a profile to load it into the editor, change it, then **Save game**
- **Delete Profile:** Select a profile and click **Remove** (or press Delete)
- **Settings:** Start with Windows, Start minimized, Close to tray, Dark mode

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

- **Normal Resolution:** Set in the "Desktop resolution" card
- **Game Profiles:** Add/update with "Save game", delete with "Remove"
- **Preferences:** The toggles in the "Settings" card are saved automatically

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
├── Program.cs            # Entry point, single-instance handling
├── MainForm.cs           # Window layout, tray menu and game monitoring
├── Controls.cs           # Themed controls (cards, toggles, status pill, profile list)
├── Theme.cs              # Light/dark palettes and tray menu renderer
├── AppSettings.cs        # Settings persistence + Windows autostart
├── ResolutionHelper.cs   # Windows display API wrapper
├── NativeMethods.cs      # Dark title bar, native control themes
└── build.bat             # One-click build into ..\Release
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

### v1.1.0
- Redesigned interface (cards, toggle switches, status indicator, better dark mode incl. title bar)
- Tray right-click menu with Open / Exit, optional close-to-tray
- Resolution dropdown lists the modes your monitor supports
- Browse for a game's .exe, click a profile to edit it
- Single-instance, icon embedded in the exe, sharp on high-DPI screens
- Game resolution is applied temporarily, so a crash or reboot can't leave you stuck on it

### v1.0.0 (2024)
- Initial release
- Game profile management
- Auto-detection and resolution switching
- Dark mode support
- System tray minimization
- Auto-start with Windows
- Settings persistence

---

**Created with the help of Gemini and Claude**

Need help? Check the [Issues](https://github.com/kuhmori/AutoRes/issues) page or create a new issue!
