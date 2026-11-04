using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using UnityEngine;

namespace Common.Utils
{
    public static class GZipUtil
    { 
        /// <summary>
        /// 解壓縮字串
        /// </summary>
        /// <param name="zippedText"></param>
        /// <returns></returns>
        public static string UnzipZippedText(string zippedText)
        {
            if (string.IsNullOrEmpty(zippedText))
            {
                return string.Empty;
            }
            string unzippedText = null;
            var buffer = Convert.FromBase64String(zippedText);
            var ms = new MemoryStream(buffer);
            try
            {
                var zipStream = new GZipStream(ms, CompressionMode.Decompress);

                using var streamReader = new StreamReader(zipStream);
                unzippedText = streamReader.ReadToEnd();
            }
            catch (Exception e)
            {
                unzippedText = string.Empty;
            }
            return unzippedText;
        }

        public static byte[] Decompress1(byte[] compressedData)
        {
            using var memoryStream = new MemoryStream(compressedData);
            using var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
            using var decompressedStream = new MemoryStream();
            var buffer = new byte[4096];
            int bytesRead;

            while ((bytesRead = gzipStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                decompressedStream.Write(buffer, 0, bytesRead);
            }

            return decompressedStream.ToArray();
        }

        public static byte[] Decompress2(byte[] zippedData)
        {
            var ms = new MemoryStream(zippedData);
            var compressedStream = new GZipStream(ms, CompressionMode.Decompress);
            var outBuffer = new MemoryStream();
            var block = new byte[10240];
            while (true)
            {
                int bytesRead = compressedStream.Read(block, 0, block.Length);
                if (bytesRead <= 0)
                    break;
                else
                    outBuffer.Write(block, 0, bytesRead);
            }
            compressedStream.Close();
            return outBuffer.ToArray();
        }

        public static byte[] Decompress3(byte[] compressedData)
        {
            using var inputStream = new MemoryStream(compressedData);
            using var deflateStream = new DeflateStream(inputStream, CompressionMode.Decompress);
            using var outputStream = new MemoryStream();
            try
            {
                var buffer = new byte[10240];
                int bytesRead;

                while ((bytesRead = deflateStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    outputStream.Write(buffer, 0, bytesRead);
                }

                return outputStream.ToArray();
            }
            catch (Exception ex)
            {
                Debug.LogError("GZipUtil 解压缩时出错：" + ex.Message.ToString());
                return null;
            }
        }

        public static string Decompress(byte[] compressedData)
        {
            using var inputStream = new MemoryStream(compressedData);
            using var deflateStream = new DeflateStream(inputStream, CompressionMode.Decompress);
            using var reader = new StreamReader(deflateStream, Encoding.UTF8);
            try
            {
                return reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                Debug.LogError("GZipUtil 解压缩时出错：" + ex.Message);
                return null;
            }
        }

        public static byte[] GzipInflate(byte[] data)
        {
            if (data.Length == 0) return data;
            try
            {
                using var compressedStream = new MemoryStream(data);
                using var decompressedStream = new MemoryStream();
                using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                {
                    var buffer = new byte[1024];
                    int bytesRead;

                    while ((bytesRead = gzipStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        decompressedStream.Write(buffer, 0, bytesRead);
                    }
                }

                return decompressedStream.ToArray();
            }
            catch (Exception ex)
            {
                Debug.LogError("GZipUtil 解压缩时出错：" + ex.Message.ToString());
                return null;
            }
        }

        public static byte[] ZlibInflate(byte[] data)
        {
            if (data == null || data.Length == 0)
                return Array.Empty<byte>();
            // 比分牌不需要解压
            // try
            // {
            //     using var compressedStream = new MemoryStream(data);
            //     using var inflaterStream = new ICSharpCode.SharpZipLib.Zip.Compression.Streams.InflaterInputStream(compressedStream);
            //     using var decompressedStream = new MemoryStream(data.Length * 2);
            //
            //     byte[] buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(8192); // 减少GC
            //     try
            //     {
            //         int read;
            //         while ((read = inflaterStream.Read(buffer, 0, buffer.Length)) > 0)
            //             decompressedStream.Write(buffer, 0, read);
            //     }
            //     finally
            //     {
            //         System.Buffers.ArrayPool<byte>.Shared.Return(buffer);
            //     }
            //
            //     return decompressedStream.ToArray();
            // }
            // catch (Exception ex)
            // {
            //     Debug.LogError($"[GZipUtil] 解压失败: {ex.Message}");
            //     return data;
            // }
            return data;
        }

        private const string Message = "{\"type\":\"type\"}";
        private static readonly byte[] SMessageBytes = Encoding.ASCII.GetBytes(Message);

        private static void CompressBytesToStream(Stream stream)
        {
            using var compressor = new DeflateStream(stream, CompressionMode.Compress, leaveOpen: true);
            compressor.Write(SMessageBytes, 0, SMessageBytes.Length);
        }

        private static int DecompressStreamToBytes(Stream stream)
        {
            stream.Position = 0;
            const int bufferSize = 512;
            var decompressedBytes = new byte[bufferSize];
            using var decompressor = new DeflateStream(stream, CompressionMode.Decompress);
            var length = decompressor.Read(decompressedBytes, 0, bufferSize);

            return length;
        }
    }
}