using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Goombratlas
{
    static class Yaz0
    {
        [DllImport("yaz0.dll")]
        static unsafe extern byte* decompress(byte* src, uint src_len, uint* dest_len);

        [DllImport("yaz0.dll")]
        static unsafe extern byte* compress(byte* src, uint src_len, uint* dest_len, byte opt_compr);

        [DllImport("yaz0.dll")]
        static unsafe extern void freePtr(void* ptr);

        public static unsafe byte[] Decompress(byte[] data)
        {
            uint src_len = (uint)data.Length;

            uint dest_len;
            fixed (byte* inputPtr = data)
            {
                byte* outputPtr = decompress(inputPtr, src_len, &dest_len);
                byte[] decomp = new byte[dest_len];
                Marshal.Copy((IntPtr)outputPtr, decomp, 0, (int)dest_len);
                freePtr(outputPtr);
                return decomp;
            }
        }

        public static void Compress(Stream stream, Tuple<int, byte[]> sarc)
        {
            //  return new MemoryStream(EveryFileExplorer.YAZ0.Compress(
            //      stream.ToArray(), Runtime.Yaz0CompressionLevel, (uint)Alignment));

            using (var writer = new BinaryDataWriter(stream, false))
            {
                writer.ByteOrder = ByteOrder.BigEndian;
                writer.Write("Yaz0", BinaryStringFormat.NoPrefixOrTermination);
                writer.Write((uint)sarc.Item2.Length);
                writer.Write((uint)sarc.Item1);
                writer.Write(0);
                writer.Write(Compress(sarc.Item2, 3));
            }
        }

        private static unsafe byte[] Compress(byte[] src, byte opt_compr)
        {
            uint src_len = (uint)src.Length;

            uint dest_len;
            fixed (byte* inputPtr = src)
            {
                byte* outputPtr = compress(inputPtr, src_len, &dest_len, opt_compr);
                byte[] comp = new byte[dest_len];
                Marshal.Copy((IntPtr)outputPtr, comp, 0, (int)dest_len);
                freePtr(outputPtr);
                return comp;
            }
        }
    }
}
