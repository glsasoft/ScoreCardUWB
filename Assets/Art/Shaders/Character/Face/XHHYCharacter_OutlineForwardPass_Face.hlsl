#pragma once
#include "XHHYCharacter_LightUtil_Face.hlsl" //光照相关
#include "../XHHYUtilityHLSL/Common.hlsl" // 通用工具方法（原路径相对本目录找不到，改成显式相对路径）

// -----------------------------------------------------------------------------------------------------
// 实际逻辑已合并到 XHHYUtilityHLSL/XhhyPassCommon/XhhyOutlineForwardPass.hlsl（与 Character 共用）。
// 本文件的写法就是合并时采用的基准版本。
// -----------------------------------------------------------------------------------------------------
#include "../XHHYUtilityHLSL/XhhyPassCommon/XhhyOutlineForwardPass.hlsl"
