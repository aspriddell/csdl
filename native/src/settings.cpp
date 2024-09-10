//
// settings.cpp
// Created by Albie on 09/09/24.
//

#include "settings.h"

#include <magic_enum.hpp>

#pragma region "enum mapping"

// magic_enum customization for libtorrent settings_pack enums
// required for some platforms to work as expected
namespace magic_enum::customize {
    template <>
    struct enum_range<libtorrent::settings_pack::int_types> {
        static constexpr int min = libtorrent::settings_pack::int_type_base;
        static constexpr int max = libtorrent::settings_pack::max_int_setting_internal;
    };

    template <>
    struct enum_range<libtorrent::settings_pack::string_types> {
        static constexpr int min = libtorrent::settings_pack::string_type_base;
        static constexpr int max = libtorrent::settings_pack::max_string_setting_internal;
    };

    template <>
    struct enum_range<libtorrent::settings_pack::bool_types> {
        static constexpr int min = libtorrent::settings_pack::bool_type_base;
        static constexpr int max = libtorrent::settings_pack::max_bool_setting_internal;
    };    
}

template <typename T>
typename std::enable_if<std::is_enum<T>::value, bool>::type
set_value(const char* key, std::function<bool(T)> setter)
{
    if (key == nullptr)
    {
        return false;
    }

    auto enum_key = magic_enum::enum_cast<T>(key, magic_enum::case_insensitive);
    if (!enum_key.has_value())
    {
        return false;
    }

    return setter(enum_key.value());
}

#pragma endregion

lt::settings_pack* create_settings_pack()
{
    return new lt::settings_pack;
}

void destroy_settings_pack(lt::settings_pack* pack)
{
    if (pack != nullptr)
    {
        delete pack;
    }
}

uint8_t settings_pack_set_str(lt::settings_pack* pack, const char* key, const char* value)
{
    if (pack == nullptr)
    {
        return false;
    }

    return set_value<lt::settings_pack::string_types>(key, [pack, value](lt::settings_pack::string_types key)
    {
        if (value == nullptr)
        {
            return false;
        }

        pack->set_str(key, std::string(value));
        return true;
    });
}

uint8_t settings_pack_set_bool(lt::settings_pack* pack, const char* key, bool value)
{
    if (pack == nullptr)
    {
        return false;
    }

    return set_value<lt::settings_pack::bool_types>(key, [pack, value](lt::settings_pack::bool_types key)
    {
        pack->set_bool(key, value);
        return true;
    });
}

uint8_t settings_pack_set_int(lt::settings_pack* pack, const char* key, int value)
{
    if (pack == nullptr)
    {
        return false;
    }

    return set_value<lt::settings_pack::int_types>(key, [pack, value](lt::settings_pack::int_types key)
    {
        pack->set_int(key, value);
        return true;
    });
}
