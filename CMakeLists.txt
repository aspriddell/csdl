cmake_minimum_required(VERSION 3.21)
set(CMAKE_CXX_STANDARD 20)

include(GenerateExportHeader)
include_directories(${CMAKE_BINARY_DIR}/include ${CMAKE_SOURCE_DIR}/include)

project(cs_native VERSION 0.1 LANGUAGES CXX)
find_package(LibtorrentRasterbar CONFIG REQUIRED)

add_library(csdl SHARED src/library.cpp src/events.cpp)
generate_export_header(csdl EXPORT_FILE_NAME ${CMAKE_BINARY_DIR}/include/lib_export.h)

target_link_libraries(csdl PUBLIC LibtorrentRasterbar::torrent-rasterbar)
