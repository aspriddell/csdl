#!/usr/bin/env bash

apt install -y build-essential tar curl zip unzip cmake gcc g++ pkg-config
apt install -y gcc-aarch64-linux-gnu g++-aarch64-linux-gnu

# vcpkg setup and bootstrap
export VCPKG_HOME=$(pwd)/vcpkg
./vcpkg/bootstrap-vcpkg.sh

# setup/build (aarch64)
cmake -B csdl-aarch64-build-release -DCMAKE_TOOLCHAIN_FILE=build/aarch64-linux-gnu.cmake -DVCPKG_TARGET_TRIPLET=arm64-linux-release-dynamic
cmake --build csdl-aarch64-build-release --config Release

# setup/build (x64)
cmake -B csdl-x64-build-release -DCMAKE_TOOLCHAIN_FILE=vcpkg/scripts/buildsystems/vcpkg.cmake -DVCPKG_TARGET_TRIPLET=x64-linux-release-dynamic
cmake --build csdl-x64-build-release --config Release
