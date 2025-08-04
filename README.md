# SyncTool
SyncTool is a simple cross-platform tool for one-way synchronization of files. It copies or updates files from the source folder to the replica folder, ensuring the replica matches the source.

## Running SyncTool: Arguments or Settings

You can launch SyncTool and configure it either by passing command-line arguments or by using the program's built-in settings interface.

### 1. Using Command-Line Arguments
SyncTool accepts command-line arguments to specify synchronization parameters directly when launching the program.

Common arguments:

- --logs — Path to save log files.
- --source — Path to the source folder.
- --replica — Path to the replica folder.
- --interval — Sync interval (e.g., 10s, 5m, 2h). Supported units: s = seconds, m = minutes, h = hours, d = days, M = months, y = years.

Example:
```
SyncTool.exe --logs "C:\User\Logs\" --source "C:\Users\Me\Documents" --replica "E:\Backup\Documents" --interval 1s
```
This runs SyncTool syncing the source to the replica folder with 1 second interval in one-way mode and outputs logs.

### 2. Using In-Program Settings
You can also start SyncTool without arguments and configure all options via the program’s graphical settings interface:

- Set the source, replica and logs folders.
- Choose the synchronization interval.
- Save these settings to a configuration file.

Once saved, the program will remember these settings on the next launch, so you don’t have to enter them again.

# License
This project is licensed under the [MIT License](https://github.com/ITeMbI4/SyncTool/blob/master/LICENSE).

# Credits
Developed by ITeMbI4.