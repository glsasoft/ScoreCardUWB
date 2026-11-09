#pragma once
#include "XHHYCharacter_LightUtil.hlsl" //光照相关
#include "../XHHYUtilityHLSL/Common.hlsl" // 通用工具方法（原路径相对本目录找不到，改成显式相对路径）

// -----------------------------------------------------------------------------------------------------
// 实际逻辑已合并到 Face / Eye 共用的同一份文件里。
// 旧版本通道里缺 NiloDoPerspectiveRemoval 调用，现已统一为 Face / Eye 版。
// -----------------------------------------------------------------------------------------------------
#include "../XHHYUtilityHLSL/XhhyPassCommon/XhhyDepthOnlyOrDepthNormalPass.hlsl"
