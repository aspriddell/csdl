//
// events.cpp - handles alert callbacks
// Created by Albie on 04/03/2024.
//

#include "events.h"

#include <libtorrent/session.hpp>
#include <libtorrent/alert_types.hpp>

void fill_event_info(cs_alert* alert, lt::alert* lt_alert, cs_alert_type alert_type) {
    alert->type = alert_type;
    alert->category = (unsigned int)lt_alert->category();

    alert->message = lt_alert->message().c_str();
    alert->epoch = lt_alert->timestamp().time_since_epoch().count();
}

void populate_peer_alert(cs_peer_alert* peer_alert, lt::peer_alert* alert, cs_peer_alert_type alert_type) {
    fill_event_info(&peer_alert->alert, alert, cs_alert_type::peer_notification);

    peer_alert->type = alert_type;
    peer_alert->handle = &alert->handle;

    auto v6_mapped_addr = alert->endpoint.address().to_v6().to_bytes();
    std::copy(v6_mapped_addr.begin(), v6_mapped_addr.end(), peer_alert->ipv6_address);
}

void on_events_available(lt::session* session, cs_alert_callback callback) {
    static std::mutex mutex;

    // try to take lock, return if failed
    // shouldn't be needed but just in case
    if (!mutex.try_lock()) {
        return;
    }

    std::vector<lt::alert*> events;
    session->pop_alerts(&events);

    handle_events:
    for (auto &alert: events) {
        switch (alert->type()) {

            // torrent state changed
            case lt::state_changed_alert::alert_type:
            {
                auto state_alert = lt::alert_cast<lt::state_changed_alert>(alert);
                cs_torrent_status_alert status_alert{};

                status_alert.handle = &state_alert->handle;
                status_alert.new_state = state_alert->state;
                status_alert.old_state = state_alert->prev_state;

                fill_event_info(&status_alert.alert, alert, cs_alert_type::torrent_status);
                callback(&status_alert);
                break;
            }

            // performance warning
            case lt::performance_alert::alert_type:
            {
                auto perf_alert = lt::alert_cast<lt::performance_alert>(alert);
                cs_client_performance_alert perf_warning{};

                perf_warning.warning_type = perf_alert->warning_code;

                fill_event_info(&perf_warning.alert, alert, cs_alert_type::client_performance);
                callback(&perf_warning);
                break;
            }

            // peer connected
            case lt::peer_connect_alert::alert_type:
            {
                auto peer_alert = lt::alert_cast<lt::peer_connect_alert>(alert);
                auto direction = (peer_alert->direction == lt::peer_connect_alert::direction_t::in) ? cs_peer_alert_type::connected_in : cs_peer_alert_type::connected_out;

                cs_peer_alert peer_connected{};

                populate_peer_alert(&peer_connected, peer_alert, direction);
                callback(&peer_connected);
                break;
            }

            // peer disconnected
            case lt::peer_disconnected_alert::alert_type:
            {
                auto peer_alert = lt::alert_cast<lt::peer_disconnected_alert>(alert);
                cs_peer_alert peer_disconnected{};

                populate_peer_alert(&peer_disconnected, peer_alert, cs_peer_alert_type::disconnected);
                callback(&peer_disconnected);
                break;
            }

            // peer banned
            case lt::peer_ban_alert::alert_type:
            {
                auto peer_alert = lt::alert_cast<lt::peer_ban_alert>(alert);
                cs_peer_alert peer_banned{};

                populate_peer_alert(&peer_banned, peer_alert, cs_peer_alert_type::banned);
                callback(&peer_banned);
                break;
            }

            // peer snubbed
            case lt::peer_snubbed_alert::alert_type:
            {
                auto peer_alert = lt::alert_cast<lt::peer_snubbed_alert>(alert);
                cs_peer_alert peer_snubbed{};

                populate_peer_alert(&peer_snubbed, peer_alert, cs_peer_alert_type::snubbed);
                callback(&peer_snubbed);
                break;
            }

            // peer unsnubbed
            case lt::peer_unsnubbed_alert::alert_type:
            {
                auto peer_alert = lt::alert_cast<lt::peer_unsnubbed_alert>(alert);
                cs_peer_alert peer_unsnubbed{};

                populate_peer_alert(&peer_unsnubbed, peer_alert, cs_peer_alert_type::unsnubbed);
                callback(&peer_unsnubbed);
                break;
            }

            // peer errored
            case lt::peer_error_alert::alert_type:
            {
                auto peer_alert = lt::alert_cast<lt::peer_error_alert>(alert);
                cs_peer_alert peer_errored{};

                populate_peer_alert(&peer_errored, peer_alert, cs_peer_alert_type::errored);
                callback(&peer_errored);
                break;
            }

            default:
            {
                cs_alert generic_alert{};

                fill_event_info(&generic_alert, alert, cs_alert_type::generic);
                callback(&generic_alert);
                break;
            }
        }
    }

    events.clear();
    session->pop_alerts(&events);

    if (!events.empty()) {
        goto handle_events;
    }

    mutex.unlock();
}

