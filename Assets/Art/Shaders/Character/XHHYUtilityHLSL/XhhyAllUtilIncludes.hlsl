// SPDX-License-Identifier: (Not available for this version, you are only allowed to use this software if you have express permission from the copyright holder and agreed to the latest NiloToonURP EULA)
// Copyright (c) 2021 Kuroneko ShaderLab Limited

// For more information, visit -> https://github.com/ColinLeung-NiloCat/UnityURPToonLitShaderExample

// #pragma once is a safe guard best practice in almost every .hlsl, 
// doing this can make sure your .hlsl's user can include this .hlsl anywhere anytime without producing any multi include conflict
#pragma once

#include "XhhyDefineURPGlobalTextures.hlsl"

#include "XhhyHSVRGBConvert.hlsl"
#include "XhhyNormalVectorUtil.hlsl"
#include "XhhyOutlineUtil.hlsl"
#include "XhhyZOffsetUtil.hlsl"
#include "XhhyInvLerpRemapUtil.hlsl"
#include "XhhyDepthTextureUtil.hlsl"
#include "GGXDirectSpecular\XhhyGGXSpecular.hlsl"
#include "XhhyStrandSpecular.hlsl"
#include "XhhyPerspectiveRemovalUtil.hlsl"
#include "XhhyDitherFadeoutClipUtil.hlsl"

