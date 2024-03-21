cmake_minimum_required(VERSION 3.21)
set(CMAKE_CXX_STANDARD 20)

# target versions
if(${CMAKE_SYSTEM_NAME} MATCHES "Windows")
  set(CMAKE_SYSTEM_VERSION 10.0 CACHE TYPE INTERNAL FORCE)
elseif(${CMAKE_SYSTEM_NAME} MATCHES "Darwin")
  set(CMAKE_OSX_DEPLOYMENT_TARGET 11)
endif()

include(GenerateExportHeader)
include_directories(${CMAKE_BINARY_DIR}/include ${CMAKE_SOURCE_DIR}/native/include)

project(cs_native VERSION 0.1 LANGUAGES CXX)
find_package(LibtorrentRasterbar CONFIG REQUIRED)

# rpath fixes (add homebrew on macOS)
if(${CMAKE_SYSTEM_NAME} MATCHES "Darwin")
  set(CMAKE_BUILD_WITH_INSTALL_RPATH TRUE)
  set(CMAKE_INSTALL_RPATH "@loader_path")
elseif(${CMAKE_SYSTEM_NAME} MATCHES "Linux")
  set(CMAKE_BUILD_WITH_INSTALL_RPATH TRUE)
  set(CMAKE_INSTALL_RPATH "$ORIGIN")
endif()

add_library(csdl SHARED native/src/library.cpp native/src/events.cpp)
generate_export_header(csdl EXPORT_FILE_NAME ${CMAKE_BINARY_DIR}/include/lib_export.h)

target_link_libraries(csdl PUBLIC LibtorrentRasterbar::torrent-rasterbar)
