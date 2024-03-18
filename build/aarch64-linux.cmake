set(CMAKE_SYSTEM_NAME Linux)
set(CMAKE_SYSTEM_PROCESSOR aarch64)

find_program(CMAKE_C_COMPILER NAMES aarch64-linux-gnu-gcc)
find_program(CMAKE_CXX_COMPILER NAMES aarch64-linux-gnu-g++)

set(CMAKE_FIND_ROOT_PATH /usr/aarch64-linux-gnu)

set(CMAKE_FIND_ROOT_PATH_MODE_PROGRAM NEVER)
set(CMAKE_FIND_ROOT_PATH_MODE_LIBRARY ONLY)
set(CMAKE_FIND_ROOT_PATH_MODE_INCLUDE ONLY)

set(VCPKG_TARGET_TRIPLET arm64-linux-release-dynamic)
include($ENV{VCPKG_HOME}/scripts/buildsystems/vcpkg.cmake)
