AutoRes

This WinForms app watches for a selected process executable and switches the primary monitor resolution to the saved "game" resolution while the process runs, restoring the saved "normal" resolution when it exits.

Usage

- Open the solution in Visual Studio and build.
- The app uses an icon from: C:\Users\lazik\Downloads\icons8-monitor-32.ico at runtime (Program.IconPath).
- Use the dropdown to select an executable (Refresh to rescan running processes).
- Click "Set Normal" while at your normal desktop resolution to save it.
- Launch the game and click "Set Game" while the game is running at the preferred resolution to save it.
- Enable "Autostart with Windows" to add/remove the app from HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Run.
- Toggle Dark Mode to change the UI colors.

Notes

- Settings are stored in %APPDATA%\\AutoRes\\settings.xml.
- The app polls for the chosen process every 1 second.
- Changing display resolution requires privileges and may fail depending on the GPU driver and available modes.

