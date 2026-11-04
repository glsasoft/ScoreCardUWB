using System;
using UnityEngine;

namespace LetsPiu.Main.Common.Scripts
{
    /// <summary>
    /// 从 GIF 里取出一帧可显示的画面。头像只需要静图，不播放动画。
    /// 第一帧如果只盖住画面的一小块，会按处置方式把后续帧叠上去，取不透明像素最多的那一版。
    /// </summary>
    public static class GifFrameDecoder
    {
        const int MaxSide = 2048;
        const int MaxFrames = 32;
        const float FullOpaqueRatio = 0.9f;

        public static bool IsGif(byte[] data)
        {
            return data != null
                && data.Length >= 6
                && data[0] == 'G'
                && data[1] == 'I'
                && data[2] == 'F';
        }

        public static Texture2D DecodeVisibleFrame(byte[] data)
        {
            if (!IsGif(data) || data.Length < 13)
            {
                return null;
            }

            int p = 6;
            int screenW = ReadU16(data, ref p);
            int screenH = ReadU16(data, ref p);
            if (screenW <= 0 || screenH <= 0 || screenW > MaxSide || screenH > MaxSide)
            {
                return null;
            }

            byte packed = data[p++];
            int background = data[p++];
            p++;

            Color32[] globalPalette = null;
            if ((packed & 0x80) != 0)
            {
                globalPalette = ReadPalette(data, ref p, 1 << ((packed & 7) + 1));
            }

            Color32 backgroundColor = default;
            if (globalPalette != null && background >= 0 && background < globalPalette.Length)
            {
                backgroundColor = globalPalette[background];
            }

            var canvas = new Color32[screenW * screenH];
            Color32[] best = null;
            int bestOpaque = 0;
            int frameCount = 0;
            int disposal = 1;
            int transparent = -1;
            int total = screenW * screenH;

            while (p < data.Length && frameCount < MaxFrames)
            {
                byte block = data[p++];
                if (block == 0x3B)
                {
                    break;
                }

                if (block == 0x21)
                {
                    if (p >= data.Length)
                    {
                        break;
                    }

                    byte label = data[p++];
                    if (label == 0xF9)
                    {
                        ReadGraphicControl(data, p, ref transparent, ref disposal);
                    }

                    if (!SkipSubBlocks(data, ref p))
                    {
                        break;
                    }

                    continue;
                }

                if (block != 0x2C)
                {
                    break;
                }

                int left = ReadU16(data, ref p);
                int top = ReadU16(data, ref p);
                int width = ReadU16(data, ref p);
                int height = ReadU16(data, ref p);
                if (p >= data.Length)
                {
                    break;
                }

                byte imagePacked = data[p++];
                Color32[] palette = globalPalette;
                if ((imagePacked & 0x80) != 0)
                {
                    palette = ReadPalette(data, ref p, 1 << ((imagePacked & 7) + 1));
                }

                if (palette == null || p >= data.Length || width <= 0 || height <= 0)
                {
                    break;
                }

                int minCodeSize = data[p++];
                byte[] indices = DecodeLzw(data, ref p, minCodeSize, width * height);
                int frameDisposal = disposal;
                int frameTransparent = transparent;
                disposal = 1;
                transparent = -1;
                if (indices == null)
                {
                    continue;
                }

                Color32[] restorePrevious = frameDisposal == 3 ? (Color32[])canvas.Clone() : null;
                BlitFrame(canvas, screenW, screenH, left, top, width, height, indices, palette, frameTransparent, (imagePacked & 0x40) != 0);
                frameCount++;

                int opaque = CountOpaque(canvas);
                if (opaque > bestOpaque)
                {
                    bestOpaque = opaque;
                    best = (Color32[])canvas.Clone();
                }

                if (opaque >= total * FullOpaqueRatio)
                {
                    break;
                }

                if (frameDisposal == 2)
                {
                    ClearRect(canvas, screenW, screenH, left, top, width, height, backgroundColor);
                }
                else if (frameDisposal == 3 && restorePrevious != null)
                {
                    canvas = restorePrevious;
                }
            }

            if (best == null || bestOpaque <= 0)
            {
                return null;
            }

            if (frameCount > 1)
            {
                Debug.Log("GifFrameDecoder 第一帧未铺满，已叠加 " + frameCount + " 帧，不透明像素 " + bestOpaque + "/" + total);
            }

            return ToTexture(best, screenW, screenH);
        }

        static void ReadGraphicControl(byte[] data, int p, ref int transparent, ref int disposal)
        {
            if (p >= data.Length)
            {
                return;
            }

            int size = data[p];
            if (size < 4 || p + size >= data.Length)
            {
                return;
            }

            byte flags = data[p + 1];
            disposal = (flags >> 2) & 7;
            transparent = (flags & 1) != 0 ? data[p + 4] : -1;
        }

        static void BlitFrame(
            Color32[] canvas, int screenW, int screenH, int left, int top, int width, int height,
            byte[] indices, Color32[] palette, int transparent, bool interlace)
        {
            int[] rowMap = interlace ? BuildInterlaceMap(height) : null;
            int count = Math.Min(indices.Length, width * height);
            for (int i = 0; i < count; i++)
            {
                int row = i / width;
                int col = i - row * width;
                if (rowMap != null)
                {
                    row = rowMap[row];
                }

                int x = left + col;
                int y = top + row;
                if ((uint)x >= (uint)screenW || (uint)y >= (uint)screenH)
                {
                    continue;
                }

                byte index = indices[i];
                if (index == transparent || index >= palette.Length)
                {
                    continue;
                }

                canvas[y * screenW + x] = palette[index];
            }
        }

        static void ClearRect(Color32[] canvas, int screenW, int screenH, int left, int top, int width, int height, Color32 color)
        {
            for (int row = 0; row < height; row++)
            {
                int y = top + row;
                if ((uint)y >= (uint)screenH)
                {
                    continue;
                }

                int rowStart = y * screenW;
                for (int col = 0; col < width; col++)
                {
                    int x = left + col;
                    if ((uint)x >= (uint)screenW)
                    {
                        continue;
                    }

                    canvas[rowStart + x] = color;
                }
            }
        }

        static int CountOpaque(Color32[] pixels)
        {
            int count = 0;
            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].a > 0)
                {
                    count++;
                }
            }

            return count;
        }

        static Texture2D ToTexture(Color32[] topDown, int width, int height)
        {
            var pixels = new Color32[topDown.Length];
            for (int y = 0; y < height; y++)
            {
                int src = y * width;
                int dst = (height - 1 - y) * width;
                Array.Copy(topDown, src, pixels, dst, width);
            }

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            return texture;
        }

        static int[] BuildInterlaceMap(int height)
        {
            var rows = new int[height];
            int i = 0;
            for (int y = 0; y < height; y += 8) rows[i++] = y;
            for (int y = 4; y < height; y += 8) rows[i++] = y;
            for (int y = 2; y < height; y += 4) rows[i++] = y;
            for (int y = 1; y < height; y += 2) rows[i++] = y;
            return rows;
        }

        static byte[] DecodeLzw(byte[] data, ref int p, int minCodeSize, int pixelCount)
        {
            if (minCodeSize < 2 || minCodeSize > 8 || pixelCount <= 0)
            {
                SkipSubBlocks(data, ref p);
                return null;
            }

            int clear = 1 << minCodeSize;
            int eoi = clear + 1;
            int codeSize = minCodeSize + 1;
            int next = eoi + 1;
            var dict = new byte[4096][];
            for (int i = 0; i < clear; i++)
            {
                dict[i] = new[] { (byte)i };
            }

            var output = new byte[pixelCount];
            int written = 0;
            int prev = -1;
            int bitBuffer = 0;
            int bitCount = 0;
            int subLeft = 0;
            bool consumedTerminator = false;

            while (written < pixelCount)
            {
                int code = ReadCode(data, ref p, ref subLeft, ref bitBuffer, ref bitCount, codeSize, ref consumedTerminator);
                if (code < 0 || code == eoi)
                {
                    break;
                }

                if (code == clear)
                {
                    codeSize = minCodeSize + 1;
                    next = eoi + 1;
                    prev = -1;
                    continue;
                }

                byte[] entry;
                if (code < next && dict[code] != null)
                {
                    entry = dict[code];
                }
                else if (code == next && prev >= 0 && dict[prev] != null)
                {
                    entry = Append(dict[prev], dict[prev][0]);
                }
                else
                {
                    break;
                }

                int copy = Math.Min(entry.Length, pixelCount - written);
                Buffer.BlockCopy(entry, 0, output, written, copy);
                written += copy;

                if (prev >= 0 && next < 4096 && dict[prev] != null)
                {
                    dict[next++] = Append(dict[prev], entry[0]);
                    if (codeSize < 12 && next == (1 << codeSize))
                    {
                        codeSize++;
                    }
                }

                prev = code;
            }

            if (!consumedTerminator)
            {
                if (subLeft > 0 && p + subLeft <= data.Length)
                {
                    p += subLeft;
                }

                SkipSubBlocks(data, ref p);
            }

            return written > 0 ? output : null;
        }

        static int ReadCode(
            byte[] data, ref int p, ref int subLeft, ref int bitBuffer, ref int bitCount, int codeSize, ref bool consumedTerminator)
        {
            while (bitCount < codeSize)
            {
                if (subLeft == 0)
                {
                    if (p >= data.Length)
                    {
                        return -1;
                    }

                    subLeft = data[p++];
                    if (subLeft == 0)
                    {
                        consumedTerminator = true;
                        return -1;
                    }
                }

                if (p >= data.Length)
                {
                    return -1;
                }

                bitBuffer |= data[p++] << bitCount;
                bitCount += 8;
                subLeft--;
            }

            int mask = (1 << codeSize) - 1;
            int code = bitBuffer & mask;
            bitBuffer >>= codeSize;
            bitCount -= codeSize;
            return code;
        }

        static byte[] Append(byte[] prefix, byte extra)
        {
            var next = new byte[prefix.Length + 1];
            Buffer.BlockCopy(prefix, 0, next, 0, prefix.Length);
            next[prefix.Length] = extra;
            return next;
        }

        static Color32[] ReadPalette(byte[] data, ref int p, int count)
        {
            if (count <= 0 || p + count * 3 > data.Length)
            {
                return null;
            }

            var palette = new Color32[count];
            for (int i = 0; i < count; i++)
            {
                palette[i] = new Color32(data[p], data[p + 1], data[p + 2], 255);
                p += 3;
            }

            return palette;
        }

        static bool SkipSubBlocks(byte[] data, ref int p)
        {
            while (p < data.Length)
            {
                int size = data[p++];
                if (size == 0)
                {
                    return true;
                }

                if (p + size > data.Length)
                {
                    return false;
                }

                p += size;
            }

            return false;
        }

        static int ReadU16(byte[] data, ref int p)
        {
            if (p + 1 >= data.Length)
            {
                return 0;
            }

            int value = data[p] | (data[p + 1] << 8);
            p += 2;
            return value;
        }
    }
}
