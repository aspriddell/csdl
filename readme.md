# csdl
A library providing libtorrent functionality through a C++ library for use in .NET

## Current Status
Currently in development with the following functionality:

- Start and stop a session
- Parse a torrent file (via file path or raw contents)
- Get information about the torrent (name, author, total file size)
- Get a list of files in the torrent (name, size, target download path)
- Attach and detach torrents from a session
- Start and stop torrents
- Get information about the session (download and upload rates, number of peers)

All functionality is then available through a C# library with the goal of providing an easy to use interface for .NET developers.
Functionality is currently added on an as-needed basis, and more can be added if requested.

## Requirements
### macOS
libtorrent can be installed via Homebrew. The development files are automatically included when using homebrew.

```bash
brew install libtorrent-rasterbar
```

### Linux
libtorrent can be installed via most package managers. Check [repology](https://repology.org/project/libtorrent-rasterbar/versions) for the correct package name and versions.
If building csdl from source, the development packages are also required.

```bash
# debian/ubuntu
sudo apt install libtorrent-rasterbar-dev
sudo apt install libtorrent-rasterbar

# fedora
sudo dnf install rb_libtorrent-devel
sudo dnf install rb_libtorrent
```

### Windows
Currently untested but will be supported soon.
