#pragma once
#include "XHHYCharacter_LightUtil.hlsl" //光照相关

// -----------------------------------------------------------------------------------------------------
// 实际逻辑已合并到 Face / Eye 共用的同一份文件里，这里只保留壳，方便 .shader 的 include 路径不动。
// 合并前的旧版是本文件内容（未乘 _GlobalToonShaderNormalBiasMultiplier），现已统一为 Face / Eye 版。
// -----------------------------------------------------------------------------------------------------
#include "../XHHYUtilityHLSL/XhhyPassCommon/XhhyShadowCasterPass.hlsl"
