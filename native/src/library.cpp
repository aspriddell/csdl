//
// library.cpp
// Created by Albie on 29/02/2024.
//

#include "library.h"

#include <libtorrent/fingerprint.hpp>
#include <libtorrent/torrent_handle.hpp>

extern "C" {
// given a config, create a session
lt::session* create_session(session_config* config)
{
    if (config == nullptr)
    {
        return create_session_from_pack(nullptr);
    }

    lt::settings_pack pack;

    // user-agent
    if (config->user_agent != nullptr)
    {
        auto useragent = std::string(config->user_agent);
        if (!useragent.empty())
        {
            pack.set_str(lt::settings_pack::user_agent, useragent);
        }
    }

    // fingerprint
    if (config->fingerprint != nullptr)
    {
        auto fingerprint = std::string(config->fingerprint);
        if (!fingerprint.empty())
        {
            pack.set_str(lt::settings_pack::peer_fingerprint, fingerprint);
        }
    }

    // events
    if (config->set_alert_flags)
    {
        pack.set_int(lt::settings_pack::alert_mask, config->alert_flags);
    }

    pack.set_bool(lt::settings_pack::anonymous_mode, config->private_mode);

    // disable seeding
    if (config->block_seeding)
    {
        pack.set_bool(lt::settings_pack::seeding_outgoing_connections, false);
    }

    // max connections
    if (config->max_connections > 0)
    {
        pack.set_int(lt::settings_pack::connections_limit, config->max_connections);
    }

    // encryption
    if (config->encrypted_peers_only)
    {
        pack.set_int(lt::settings_pack::out_enc_policy, lt::settings_pack::pe_forced);
        pack.set_int(lt::settings_pack::in_enc_policy, lt::settings_pack::pe_forced);
    }

    return create_session_from_pack(&pack);
}

lt::session* create_session_from_pack(lt::settings_pack* pack)
{
    lt::session_params params;

    if (pack != nullptr)
    {
        params.settings = *pack;
    }

    return new lt::session(params);
}

void destroy_session(lt::session* session)
{
    if (session == nullptr)
    {
        return;
    }

    session->abort();
    delete session;
}

void apply_settings(lt::session* session, lt::settings_pack* settings)
{
    if (session == nullptr || settings == nullptr)
    {
        return;
    }

    session->apply_settings(*settings);
}

void set_event_callback(lt::session* session, cs_alert_callback callback, bool include_unmapped_events)
{
    if (session == nullptr)
    {
        return;
    }
    else if (callback == nullptr)
    {
        clear_event_callback(session);
        return;
    }

    auto session_callback = [session, callback, include_unmapped_events]() -> void
    {
        std::thread(on_events_available, session, callback, include_unmapped_events).detach();
    };

    session->set_alert_notify(session_callback);
}

void clear_event_callback(lt::session* session)
{
    if (session == nullptr)
    {
        return;
    }

    session->set_alert_notify(nullptr);
}

lt::torrent_info* create_torrent_bytes(const char* data, long length)
{
    lt::span<char const> buffer(data, length);
    lt::load_torrent_limits cfg;

    return new lt::torrent_info(buffer, cfg, lt::from_span);
}

lt::torrent_info* create_torrent_file(const char* file_path)
{
    return new lt::torrent_info(std::string(file_path));
}

lt::torrent_info* create_torrent_magnet(const char* magnet)
{
    return new lt::parse_magnet_uri(std::string(magnet));
}

void destroy_torrent(lt::torrent_info* torrent)
{
    if (torrent != nullptr)
    {
        delete torrent;
    }
}

// attach a torrent to the session, returning a handle that can be used to control the download.
// the torrent info handle is copied, and can be freed after the call to attach_torrent with a call to destroy_torrent_info.
lt::torrent_handle* attach_torrent(lt::session* session, lt::torrent_info* torrent, const char* save_path)
{
    if (session == nullptr || torrent == nullptr)
    {
        return nullptr;
    }

    lt::add_torrent_params params;
    std::string save_path_copy(save_path);

    if (!save_path_copy.empty())
    {
        params.save_path = save_path_copy;
    }

    // enable paused-by-default, disable auto-management
    params.flags |= lt::torrent_flags::paused;
    params.flags &= ~lt::torrent_flags::auto_managed;

    // set torrent info - make_shared creates a copy
    params.ti = std::make_shared<lt::torrent_info>(*torrent);
    auto handle = new lt::torrent_handle(session->add_torrent(params));

    if (handle->is_valid())
    {
        return handle;
    }

    delete handle;
    return nullptr;
}

// after detaching the torrent, the torrent handle is no longer valid.
// additionally, a call to destroy_torrent is not needed.
void detach_torrent(lt::session* session, lt::torrent_handle* torrent)
{
    if (session == nullptr || torrent == nullptr)
    {
        return;
    }

    torrent->pause();
    session->remove_torrent(*torrent);

    delete torrent;
}

// get the info for a torrent.
// the torrent_info struct is allocated on the heap and must be freed with a call to destroy_torrent_info.
torrent_metadata* get_torrent_info(lt::torrent_info* torrent)
{
    if (torrent == nullptr)
    {
        return nullptr;
    }

    auto name = torrent->name();
    auto author = torrent->creator();
    auto comment = torrent->comment();

    auto torrent_name = new char[name.size() + 1]();
    auto torrent_author = new char[author.size() + 1]();
    auto torrent_comment = new char[comment.size() + 1]();

    std::copy(name.begin(), name.end(), torrent_name);
    std::copy(author.begin(), author.end(), torrent_author);
    std::copy(comment.begin(), comment.end(), torrent_comment);

    auto info = new torrent_metadata();

    info->name = torrent_name;
    info->creator = torrent_author;
    info->comment = torrent_comment;

    info->total_files = torrent->num_files();
    info->total_size = torrent->total_size();
    info->creation_date = torrent->creation_date();

    auto hash = torrent->info_hashes();

    // fill in the info hash
    if (hash.has_v1())
    {
        std::copy(hash.v1.begin(), hash.v1.end(), info->info_hash_v1);
    }
    else
    {
        std::fill(info->info_hash_v1, info->info_hash_v1 + 20, 0);
    }

    // fill in the info hash v2
    if (hash.has_v2())
    {
        std::copy(hash.v2.begin(), hash.v2.end(), info->info_hash_v2);
    }
    else
    {
        std::fill(info->info_hash_v2, info->info_hash_v2 + 32, 0);
    }

    return info;
}

void destroy_torrent_info(torrent_metadata* info)
{
    if (info == nullptr)
    {
        return;
    }

    delete[] info->name;
    delete[] info->creator;
    delete[] info->comment;

    delete info;
}

// given a torrent handle, get the list of files in the torrent.
void get_torrent_file_list(lt::torrent_info* torrent, torrent_file_list* file_list)
{
    if (torrent == nullptr || file_list == nullptr)
    {
        return;
    }

    const auto& files = torrent->files();

    auto num_files = files.num_files();
    auto list = new torrent_file_information[num_files];

    for (lt::file_index_t i(0); i != files.end_file(); i++)
    {
        auto name = files.file_name(i);
        auto path = files.file_path(i);

        char* file_name = new char[name.size() + 1]();
        char* file_path = new char[path.size() + 1]();

        list[(int)i] = {
            i,
            files.file_offset(i),
            files.file_size(i),
            files.mtime(i),
            file_name,
            file_path,
            files.file_absolute_path(i),
            files.pad_file_at(i)
        };

        std::copy(name.begin(), name.end(), file_name);
        std::copy(path.begin(), path.end(), file_path);
    }

    file_list->files = list;
    file_list->length = num_files;
}

void destroy_torrent_file_list(torrent_file_list* file_list)
{
    if (file_list == nullptr || file_list->files == nullptr)
    {
        return;
    }

    for (int i = 0; i < file_list->length; i++)
    {
        delete[] file_list->files[i].file_name;
        delete[] file_list->files[i].file_path;
    }

    delete[] file_list->files;
}

// set the download priority for a file in a torrent.
void set_file_dl_priority(lt::torrent_handle* torrent, const lt::file_index_t file_index, const uint8_t priority)
{
    if (torrent == nullptr)
    {
        return;
    }

    torrent->file_priority(file_index, (lt::download_priority_t)priority);
}

// get the download priority for a file in a torrent.
uint8_t get_file_dl_priority(lt::torrent_handle* torrent, const lt::file_index_t file_index)
{
    if (torrent == nullptr)
    {
        return 0;
    }

    return (uint8_t)torrent->file_priority(file_index);
}

// start and stop the download of a torrent.
void start_torrent(lt::torrent_handle* torrent)
{
    if (torrent == nullptr)
    {
        return;
    }

    torrent->resume();
}

// start and stop the download of a torrent.
void stop_torrent(lt::torrent_handle* torrent)
{
    if (torrent == nullptr)
    {
        return;
    }

    torrent->pause();
}

void reannounce_torrent(lt::torrent_handle* torrent, const int32_t seconds, const uint8_t ignore_min_interval)
{
    if (torrent == nullptr)
    {
        return;
    }

    lt::reannounce_flags_t flags = {};

    if (ignore_min_interval)
    {
        flags |= lt::torrent_handle::ignore_min_interval;
    }

    torrent->force_reannounce(seconds, -1, flags);
}

// get the progress of a torrent.
void get_torrent_status(lt::torrent_handle* torrent, torrent_status* torrent_status)
{
    if (torrent == nullptr || torrent_status == nullptr)
    {
        return;
    }

    auto s = torrent->status();

    if (s.errc != lt::error_code())
    {
        torrent_status->state = cs_torrent_state::torrent_error;
    }
    else
    {
        switch (s.state)
        {
        case lt::torrent_status::state_t::checking_files:
            torrent_status->state = cs_torrent_state::torrent_checking;
            break;

        case lt::torrent_status::state_t::checking_resume_data:
            torrent_status->state = cs_torrent_state::torrent_checking_resume;
            break;

        case lt::torrent_status::state_t::downloading_metadata:
            torrent_status->state = cs_torrent_state::torrent_metadata_downloading;
            break;

        case lt::torrent_status::state_t::downloading:
            torrent_status->state = cs_torrent_state::torrent_downloading;
            break;

        case lt::torrent_status::state_t::seeding:
            torrent_status->state = cs_torrent_state::torrent_seeding;
            break;

        case lt::torrent_status::state_t::finished:
            torrent_status->state = cs_torrent_state::torrent_finished;
            break;

        default:
            torrent_status->state = cs_torrent_state::torrent_state_unknown;
            break;
        }
    }

    torrent_status->progress = s.progress;

    torrent_status->count_peers = s.num_peers;
    torrent_status->count_seeds = s.num_seeds;

    torrent_status->bytes_uploaded = s.total_payload_upload;
    torrent_status->bytes_downloaded = s.total_payload_download;

    torrent_status->upload_rate = s.upload_payload_rate;
    torrent_status->download_rate = s.download_payload_rate;
}
}
