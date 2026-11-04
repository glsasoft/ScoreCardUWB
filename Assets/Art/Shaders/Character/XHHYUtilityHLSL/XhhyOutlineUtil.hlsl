// SPDX-License-Identifier: (Not available for this version, you are only allowed to use this software if you have express permission from the copyright holder and agreed to the latest NiloToonURP EULA)
// Copyright (c) 2021 Kuroneko ShaderLab Limited

// For more information, visit -> https://github.com/ColinLeung-NiloCat/UnityURPToonLitShaderExample

// #pragma once is a safe guard best practice in almost every .hlsl, 
// doing this can make sure your .hlsl's user can include this .hlsl anywhere anytime without producing any multi include conflict
#pragma once

float ApplyOutlineFadeOutPerspectiveCamera(float inputMulFix, float cameraFov)
{
    // 如果角色在相机视野中太小/太远，就使轮廓 "淡出"。
    // 想象一下，这一行是做色调贴图的最简单的方法，在60/cameraFov的情况下夹紧。
    // 但这里我们不是重新映射HDR的颜色，而是重新映射轮廓的宽度。
    return min(60/cameraFov, inputMulFix); //保持与min(2,inputMulFix)相似，在fov 30相机中。
}
float ApplyOutlineFadeOutOrthoCamera(float inputMulFix)
{
    // make outline "fadeout" if character is too small in camera's view
    // imagine this line is the most simple way to do tone mapping that clamp at 2,
    // but here we are not remapping HDR color, we are remapping outline width
    return min(2,inputMulFix);
}
float GetOutlineCameraFovAndDistanceFixMultiplier(float positionVS_Z, float cameraFOV, float applyPercentage)
{
    float outlineWidthMulFix;
    if(unity_OrthoParams.w == 0)
    {
        ////////////////////////////////
        // Perspective camera case
        ////////////////////////////////

        //保持屏幕上所有摄像机距离的轮廓宽度相似
        outlineWidthMulFix = abs(positionVS_Z);

        //如果需要一个平滑的停止，可以替换成一个更好的色调映射函数
        outlineWidthMulFix = ApplyOutlineFadeOutPerspectiveCamera(outlineWidthMulFix,cameraFOV);

        //在屏幕上保持相似的宽度，并考虑到所有相机的视野。
        outlineWidthMulFix *= cameraFOV;     
    }
    else
    {
        ////////////////////////////////
        // Orthographic camera case
        ////////////////////////////////

        // 不需要关心摄像机的距离和视野，因为正视摄像机不需要考虑这些。
        float orthoSize = abs(unity_OrthoParams.y);
        orthoSize = ApplyOutlineFadeOutOrthoCamera(orthoSize);
        outlineWidthMulFix = orthoSize * 50; // 100/2 is a magic number to match perspective camera's outline width
    }

    // allow user to select applying how much of this auto outline width adjustment(this function)
    outlineWidthMulFix = lerp(60,outlineWidthMulFix, saturate(applyPercentage));

    return outlineWidthMulFix * 0.00005; // mul a const to make return result = default normal expand amount WS
}

// [currently not being used in NiloToonURP]
// If your project has a faster way to get camera fov in shader, you don't need to use this method.
// For example, you write cmd.SetGlobalFloat("_CurrentCameraFOV",cameraFOV) using a new RendererFeature in C# 
// (NiloToonURP's renderer feature did that already by providing _CurrentCameraFOV).
float GetCameraFOV()
{
    // https://answers.unity.com/questions/770838/how-can-i-extract-the-fov-information-from-the-pro.html
    float t = unity_CameraProjection._m11;
    float Rad2Deg = 180 / 3.1415;
    float fov = atan(1.0f / t) * 2.0 * Rad2Deg;
    return fov;
}
// [currently not being used in NiloToonURP]
// slower due to GetCameraFOV(), but don't need to provide cameraFOV
float GetOutlineCameraFovAndDistanceFixMultiplier(float positionVS_Z, float applyPercentage = 1)
{
    return GetOutlineCameraFovAndDistanceFixMultiplier(positionVS_Z, GetCameraFOV(), applyPercentage);
}
