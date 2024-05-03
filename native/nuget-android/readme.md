# csdl.Native.android
Android-specific native libraries for csdl.

[![Latest Nuget](https://img.shields.io/nuget/v/csdl.Native?label=csdl.Native&logo=nuget)](https://nuget.org/packages/csdl.Native)
[![DragonFruit Discord](https://img.shields.io/discord/482528405292843018?label=Discord&style=popout)](https://discord.gg/VA26u5Z)

## Overview
This library provides Android-specific native libraries used by csdl, staticly linking against `libtorrent` and built for `x86_64`, `armeabi-v7a` and `arm64-v8a`. This package should only be installed by users requiring support for Android-based platforms targeting Android 5.0+.

When updating `csdl`, `csdl.Native` or `csdl.Native.android`, all of these packages should be upgraded to the latest version to ensure against API mismatches.

### License
`libcsdl` is provided under the Apache 2.0 license, while `libtorrent` used BSD 3-Clause. Please refer to [license.md](license.md) for more information.
