#ClipSnap



A lightweight screenshot tool for Windows 10/11 that automatically saves screenshots to a folder.



##Why ClipSnap?



Windows 10's built-in screenshot tool (Win+Shift+S) doesn't automatically save screenshots to a folder - you have to manually paste and save them. Windows 11 added this feature, but Windows 10 users are left out. ClipSnap fixes this!


##Features



- 📸 \*\*Region Selection\*\* - Click and drag to capture any area of your screen

- 💾 \*\*Auto-Save\*\* - Screenshots are automatically saved to your configured folder

- 📋 \*\*Clipboard Copy\*\* - Optionally copy screenshots to clipboard (toggleable)

- ⌨️ \*\*Global Hotkeys\*\* - Win+Shift+S and Print Screen (configurable)

- 🖥️ \*\*Multi-Monitor Support\*\* - Works across all your displays

- 🚀 \*\*Start with Windows\*\* - Optional auto-start on login

- 🎨 \*\*Modern UI\*\* - Clean, dark-themed settings interface



## Installation



1. Download the latest release from the \[Releases](https://github.com/7Zeb/ClipSnap/releases) page

2. Extract and run `ClipSnap.exe`

3. The app will minimize to your system tray



## Usage



1. Press \*\*Win+Shift+S\*\* or \*\*Print Screen\*\* to start a capture

2. Click and drag to select the area you want to capture

3. Release to capture - the screenshot is automatically saved and copied to clipboard



### Settings



Right-click the tray icon and select \*\*Settings\*\* to configure:

- Screenshot save folder

- Clipboard behavior

- Hotkeys

- Auto-start with Windows



## Building from Source



### Requirements

- .NET 8.0 SDK

- Visual Studio 2022 (recommended) or VS Code



### Build Steps

```bash

cd src/ClipSnap

dotnet restore

dotnet build

```



### Run

```bash

dotnet run --project ClipSnap

```



## License



This project is licensed under the GNU General Public License v2.0 - see the \[LICENSE](LICENSE) file for details.



\## Contributing



Contributions are welcome! Please feel free to submit a Pull Request.

