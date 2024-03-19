# csdl.Native
Native libraries for csdl.

[![Latest Nuget](https://img.shields.io/nuget/v/csdl.Native?label=csdl.Native&logo=nuget)](https://nuget.org/packages/csdl.Native)
[![DragonFruit Discord](https://img.shields.io/discord/482528405292843018?label=Discord&style=popout)](https://discord.gg/VA26u5Z)

## Overview
csdl requires two native libraries to function as-expected, `libcsdl` and `libtorrent`. `libtorrent` provides the main functionality, while `libcsdl` re-exposes the required functions in a C-style interface, which is required to be able to interact with the library.

For 99% of cases, this library should not be referenced directly and csdl should be used instead.

## Requirements
libcsdl and libtorrent are built to work on as many systems as possible. All builds produce an `x86_64` and `arm64` edition.

- macOS: Big Sur 11 or later
- Linux: Ubuntu 20.04 or later
- Windows: Windows 10.0.14393.6796 or later
