using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;

namespace Cjayride.SkyheimEitr
{
    /// <summary>
    /// The Ukrainian translation contribution replaced four 64x64 skill icons in Skyheim's
    /// rune atlas (fire, frost, holy, nature) with clearer artwork. Those pixels are Unity's
    /// import output, so they ship as data (assets\rune-atlas.txt, embedded here, deflate
    /// compressed) and are handed to the loaded Texture2D at runtime.
    ///
    /// The atlas is imported non-readable, so the game cannot hand us its pixels to edit a few
    /// tiles in place; we supply the whole 512x512 DXT5 image instead. Editing the bundle bytes
    /// directly is not reproducible outside Unity - this is.
    /// </summary>
    internal static class AltarIconPatch
    {
        const string ResourceName = "rune-atlas.txt";

        static bool _applied;

        internal static bool Apply()
        {
            SkillIconSprites.Apply();

            if (_applied)
            {
                return true;
            }

            Payload payload;
            if (!ReadPayload(out payload))
            {
                Plugin.Log("Rune atlas icon payload was not found. Skill sprites still use the PNG replacements.");
                _applied = true;
                return true;
            }

            Texture2D atlas = SkyheimAssetPatches.FindTexture(payload.TextureName);
            if (atlas == null)
            {
                Plugin.Log("Rune atlas texture is not loaded yet; will retry skill-icon paint.");
                return SkillIconSprites.Ready;
            }

            if (atlas.width != payload.Width || atlas.height != payload.Height)
            {
                Plugin.Log($"Rune atlas is {atlas.width}x{atlas.height}, patch expects {payload.Width}x{payload.Height}. PNG skill icons still apply.");
                _applied = true;
                return true;
            }

            if (atlas.format != payload.Format)
            {
                Plugin.Log($"Rune atlas is {atlas.format}, patch expects {payload.Format}. PNG skill icons still apply.");
                _applied = true;
                return true;
            }

            if (payload.Data.Length != ExpectedSize(atlas))
            {
                Plugin.Log("Rune atlas icon patch size does not match the texture. PNG skill icons still apply.");
                _applied = true;
                return true;
            }

            try
            {
                atlas.LoadRawTextureData(payload.Data);
                atlas.Apply(false, false);
                Plugin.Log($"Rune atlas repainted with the new skill icons ({atlas.width}x{atlas.height} {atlas.format}).");
            }
            catch (Exception ex)
            {
                Plugin.Log("Rune atlas paint failed (" + ex.Message + "). PNG skill icons still apply.");
            }

            _applied = true;
            return true;
        }

        private static int ExpectedSize(Texture2D atlas)
        {
            int blocksWide = (atlas.width + 3) / 4;
            int blocksHigh = (atlas.height + 3) / 4;

            // DXT5/BC3 packs each 4x4 block into 16 bytes; the atlas has no mip chain.
            return blocksWide * blocksHigh * 16;
        }

        private static bool ReadPayload(out Payload payload)
        {
            payload = null;

            try
            {
                using (Stream stream = typeof(AltarIconPatch).Assembly.GetManifestResourceStream(ResourceName))
                {
                    if (stream == null)
                    {
                        return false;
                    }

                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string header = reader.ReadLine();
                        if (header == null)
                        {
                            return false;
                        }

                        string[] head = header.Split(' ');
                        if (head.Length < 7 || head[0] != "icontex1")
                        {
                            return false;
                        }

                        StringBuilder base64 = new StringBuilder();
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.Length == 0 || line[0] == '#')
                            {
                                continue;
                            }

                            base64.Append(line);
                        }

                        byte[] compressed = Convert.FromBase64String(base64.ToString());
                        int size = int.Parse(head[5]);
                        byte[] data = Inflate(compressed, size);
                        if (data == null)
                        {
                            Plugin.Log("Rune atlas icon patch did not decompress. Icons stay as shipped.");
                            return false;
                        }

                        payload = new Payload
                        {
                            TextureName = head[1],
                            Width = int.Parse(head[2]),
                            Height = int.Parse(head[3]),
                            Format = (TextureFormat)int.Parse(head[4]),
                            Data = data,
                        };
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Plugin.Log("Rune atlas icon patch was not readable: " + ex.Message);
                return false;
            }
        }

        private static byte[] Inflate(byte[] compressed, int size)
        {
            byte[] data = new byte[size];
            using (MemoryStream source = new MemoryStream(compressed))
            using (DeflateStream def = new DeflateStream(source, CompressionMode.Decompress))
            {
                int got = 0;
                while (got < size)
                {
                    int read = def.Read(data, got, size - got);
                    if (read <= 0)
                    {
                        return null;
                    }

                    got += read;
                }
            }

            return data;
        }

        private class Payload
        {
            internal string TextureName;
            internal int Width;
            internal int Height;
            internal TextureFormat Format;
            internal byte[] Data;
        }
    }
}
