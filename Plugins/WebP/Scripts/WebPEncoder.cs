using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace CubemapMaker.WebP
{
    /// <summary>
    /// Provides WebP encoding functionality using Google's libwebp.
    /// Based on libwebp by Google (https://developers.google.com/speed/webp/docs/api)
    /// </summary>
    public static class WebPEncoder
    {
        #region Native Plugin Imports
        [DllImport("WebPPlugin", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr EncodeWebP(byte[] rgba, int width, int height, int stride, float quality, out int outputSize);

        [DllImport("WebPPlugin", CallingConvention = CallingConvention.Cdecl)]
        private static extern void FreeWebP(IntPtr ptr);
        #endregion

        /// <summary>
        /// Encodes a Texture2D to WebP format
        /// </summary>
        /// <param name="texture">The texture to encode</param>
        /// <param name="quality">Quality factor (0-100)</param>
        /// <returns>WebP encoded data as byte array</returns>
        public static byte[] EncodeToWebP(Texture2D texture, float quality)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));

            if (quality < 0 || quality > 100)
                throw new ArgumentOutOfRangeException(nameof(quality), "Quality must be between 0 and 100");

            // Ensure the texture is readable
            if (!texture.isReadable)
            {
                Debug.LogWarning("[WebPEncoder] Texture is not readable. Creating a readable copy...");
                texture = CreateReadableCopy(texture);
            }

            try
            {
                // Get the raw texture data
                Color32[] colors = texture.GetPixels32();
                byte[] rgba = new byte[colors.Length * 4];
                
                // Convert Color32 array to RGBA byte array
                for (int i = 0; i < colors.Length; i++)
                {
                    int index = i * 4;
                    rgba[index] = colors[i].r;
                    rgba[index + 1] = colors[i].g;
                    rgba[index + 2] = colors[i].b;
                    rgba[index + 3] = colors[i].a;
                }

                // Encode to WebP
                int outputSize;
                IntPtr encodedData = EncodeWebP(rgba, texture.width, texture.height, texture.width * 4, quality, out outputSize);

                if (encodedData == IntPtr.Zero || outputSize <= 0)
                    throw new Exception("WebP encoding failed. The encoder returned null or invalid data.");

                // Copy the encoded data to a managed array
                byte[] result = new byte[outputSize];
                Marshal.Copy(encodedData, result, 0, outputSize);

                // Free the unmanaged memory
                FreeWebP(encodedData);

                return result;
            }
            catch (Exception e)
            {
                throw new Exception($"WebP encoding failed: {e.Message}", e);
            }
            finally
            {
                // Clean up the readable copy if we created one
                if (!texture.isReadable)
                    UnityEngine.Object.Destroy(texture);
            }
        }

        /// <summary>
        /// Creates a readable copy of a texture
        /// </summary>
        private static Texture2D CreateReadableCopy(Texture2D source)
        {
            // Create a temporary RenderTexture
            RenderTexture rt = RenderTexture.GetTemporary(
                source.width,
                source.height,
                0,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.Linear
            );

            try
            {
                // Copy the source texture to the RenderTexture
                Graphics.Blit(source, rt);

                // Create a new readable texture
                Texture2D readable = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
                
                // Store the current render texture
                RenderTexture prev = RenderTexture.active;
                RenderTexture.active = rt;

                try
                {
                    // Read the pixels from the RenderTexture
                    readable.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                    readable.Apply();
                    return readable;
                }
                finally
                {
                    // Restore the previous render texture
                    RenderTexture.active = prev;
                }
            }
            finally
            {
                // Clean up
                RenderTexture.ReleaseTemporary(rt);
            }
        }
    }
} 