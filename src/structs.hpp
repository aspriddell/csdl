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
    } torrent_file_entry;

    typedef struct cs_torrent_meta {
        char* name;
        char* author;
        char* comment;

        int total_files;
        long total_size;

        time_t creation_date;
    } torrent_metadata;

    typedef struct cs_torrent_state {
        int state;

        float progress;

        int count_peers;
        int count_seeds;

        long bytes_uploaded;
        long bytes_downloaded;

        long upload_rate;
        long download_rate;
    } torrent_state;

    typedef struct cs_torrent_file_list {
        int length;
        torrent_file_entry* files;
    } torrent_file_list;
}

#endif //CSDL_STRUCTS_HPP
