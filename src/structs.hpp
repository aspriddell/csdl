//
// structs.hpp
// Created by Albie on 29/02/2024.
//

#ifndef CSDL_STRUCTS_HPP
#define CSDL_STRUCTS_HPP

#include <libtorrent/session.hpp>

extern "C" {
    typedef struct cs_session_config {
        // see https://www.libtorrent.org/reference-Settings.html#settings_pack
        char* user_agent;
        char* fingerprint;

        bool private_mode;
        bool block_seeding;
        bool encrypted_peers_only;

        int max_connections;
    } session_config;

    typedef struct cs_file_information {
        lt::file_index_t index;

        char* file_name;
        char* file_path;

        long file_size;
    } file_info;

    typedef struct cs_torrent_info {
        char* name;
        char* author;
        char* comment;

        int total_files;
        long total_size;

        time_t creation_date;
    } torrent_file_info;

    typedef struct cs_download_status {
        int state;

        float progress;

        int count_peers;
        int count_seeds;

        long bytes_uploaded;
        long bytes_downloaded;

        long upload_rate;
        long download_rate;
    } download_status;

    typedef struct cs_torrent_file_list {
        int length;
        file_info* files;
    } torrent_file_list;
}

#endif //CSDL_STRUCTS_HPP
