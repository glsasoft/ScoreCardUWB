#pragma once
#include "XHHYCharacter_LightUtil.hlsl" //光照相关
#include "../XHHYUtilityHLSL/Common.hlsl" // 通用工具方法（原路径相对本目录找不到，改成显式相对路径）
#include "Assets/Graphics/RenderFeature/AerialFog/AerialFogCommon.hlsl" //描边在屏幕空间雾之后画，自己算雾

// -----------------------------------------------------------------------------------------------------
// 实际逻辑已合并到 Face 共用的同一份文件里。
// 旧版本这段的 "禁用渲染" 判断被整段注释掉了（_EnableRendering / _RenderOutline 不生效），
// 末尾还多一句 color.a = _Alpha，现已统一为 Face 版。
// -----------------------------------------------------------------------------------------------------
#include "../XHHYUtilityHLSL/XhhyPassCommon/XhhyOutlineForwardPass.hlsl"
