/**
 * =============================================================================
 * CS2Fixes
 * Copyright (C) 2023 Source2ZE
 * =============================================================================
 *
 * This program is free software; you can redistribute it and/or modify it under
 * the terms of the GNU General Public License, version 3.0, as published by the
 * Free Software Foundation.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 * FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more
 * details.
 *
 * You should have received a copy of the GNU General Public License along with
 * this program.  If not, see <http://www.gnu.org/licenses/>.
 */

#pragma once

#include "tier0/dbg.h"
#include "const.h"
#include "utils/virtual.h"

#include <string>
#include <vector>
#include <stdint.h>
#include <type_traits>

#undef schema

struct SchemaKey
{
    int32_t offset;
    bool networked;
};

class Z_CBaseEntity;
void SetStateChanged(Z_CBaseEntity* pEntity, int offset);

inline uint32_t val_32_const = 0x811c9dc5;
inline uint32_t prime_32_const = 0x1000193;
inline uint64_t val_64_const = 0xcbf29ce484222325;
inline uint64_t prime_64_const = 0x100000001b3;

inline uint32_t hash_32_fnv1a_const(const char* str, const uint32_t value = val_32_const) noexcept
{
    return (str[0] == '\0') ? value : hash_32_fnv1a_const(&str[1], (value ^ uint32_t(str[0])) * prime_32_const);
}

inline uint64_t hash_64_fnv1a_const(const char* str, const uint64_t value = val_64_const) noexcept
{
    return (str[0] == '\0') ? value : hash_64_fnv1a_const(&str[1], (value ^ uint64_t(str[0])) * prime_64_const);
}

namespace schema {
static std::vector<std::string> CS2BadList = {
    "m_bIsValveDS",
    "m_bIsQuestEligible",
    "m_iItemDefinitionIndex", // in unmanaged this cannot be set.
    "m_iEntityLevel",
    "m_iItemIDHigh",
    "m_iItemIDLow",
    "m_iAccountID",
    "m_iEntityQuality",

    "m_bInitialized",
    "m_szCustomName",
    "m_iAttributeDefinitionIndex",
    "m_iRawValue32",
    "m_iRawInitialValue32",
    "m_flValue", // MNetworkAlias "m_iRawValue32"
    "m_flInitialValue", // MNetworkAlias "m_iRawInitialValue32"
    "m_bSetBonus",
    "m_nRefundableCurrency",

    "m_OriginalOwnerXuidLow",
    "m_OriginalOwnerXuidHigh",

    "m_nFallbackPaintKit",
    "m_nFallbackSeed",
    "m_flFallbackWear",
    "m_nFallbackStatTrak",

    "m_iCompetitiveWins",
    "m_iCompetitiveRanking",
    "m_iCompetitiveRankType",
    "m_iCompetitiveRankingPredicted_Win",
    "m_iCompetitiveRankingPredicted_Loss",
    "m_iCompetitiveRankingPredicted_Tie",

    "m_nActiveCoinRank",
    "m_nMusicID",
};

int16_t FindChainOffset(const char* className);
SchemaKey GetOffset(const char* className, uint32_t classKey, const char* memberName, uint32_t memberKey);
} // namespace schema
