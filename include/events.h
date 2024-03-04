//
// events.hpp - ported event structures
// Created by Albie on 04/03/2024.
//

#ifndef CS_NATIVE_EVENTS_H
#define CS_NATIVE_EVENTS_H

#include "lib_export.h"

#include <ctime>
#include <libtorrent/alert.hpp>
#include <libtorrent/error_code.hpp>
#include <libtorrent/torrent_status.hpp>
#include <libtorrent/torrent_handle.hpp>


// used internally in main library, not intended for public use
typedef void (__cdecl *alert_callback)(void* alert);
CSDL_NO_EXPORT void on_events_available(lt::session* session, alert_callback callback);

#ifdef __cplusplus
extern "C" {
#endif

    // base format for all alerts
    struct cs_alert {
        int type;
        lt::alert_category_t category;

        time_t epoch;

        const char* message;
    };

    struct cs_torrent_status_alert {
        cs_alert alert;

        lt::torrent_handle* handle;

        lt::torrent_status::state_t old_state;
        lt::torrent_status::state_t new_state;
    };

    struct cs_client_performance_alert {
        cs_alert alert;

        uint8_t warning_type;
    };

    enum cs_peer_alert_type : uint8_t {
        connected_in = 0,
        connected_out = 1,
        disconnected = 2,
        banned = 3,
        snubbed = 4,
        unsnubbed = 5,
        errored = 6
    };

    struct cs_peer_alert {
        cs_alert alert;

        lt::torrent_handle* handle;
        cs_peer_alert_type type;

        uint8_t ipv6_address[16];
    };

#ifdef __cplusplus
}
#endif
#endif //CS_NATIVE_EVENTS_H
