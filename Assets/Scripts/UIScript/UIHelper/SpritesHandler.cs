using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.U2D;

namespace LetsPiu
{
    public class SpritesHandler
    {
        public static bool SetSprite(SpriteAtlas[] spritesData, Image image, string spriteName, bool isNative = false)
        {
            if (image == null)
            {
                Debug.LogWarning("Image is null");
                return false;
            }
            
            if (string.IsNullOrEmpty(spriteName))
            {
                return false;
            }

            if (spritesData == null || spritesData.Length == 0)
            {
                return false;
            }

            Sprite sprite = null;
            SpriteAtlas atlas = null;
            var length = spritesData.Length;


            for (int i = length - 1; i >= 0; --i)
            {
                atlas = spritesData[i];
                if (atlas == null)
                {
                    continue;
                }

                sprite = atlas.GetSprite(spriteName);
                if (sprite != null)
                {
                    image.sprite = sprite;
                    if (isNative)
                    {
                        image.SetNativeSize();
                    }
                    return true;
                }
            }

            image.sprite = sprite;
            return false;
        }
    }
}