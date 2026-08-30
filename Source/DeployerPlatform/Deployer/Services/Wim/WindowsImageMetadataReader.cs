using System;
using System.IO;
using System.Linq;
using System.Text;
using Serilog;

namespace Deployer.Services.Wim
{
    public class WindowsImageMetadataReader : WindowsImageMetadataReaderBase
    {
        private static long ToInt64LittleEndian(byte[] buffer, int offset)
        {
            return (long)ToUInt64LittleEndian(buffer, offset);
        }

        private static uint ToUInt32LittleEndian(byte[] buffer, int offset)
        {
            var a = (buffer[offset + 3] << 24) & 0xFF000000U;
            var b = (buffer[offset + 2] << 16) & 0x00FF0000U;
            var c = (buffer[offset + 1] << 8) & 0x0000FF00U;
            var d = (buffer[offset + 0] << 0) & 0x000000FFU;

            return (uint)(a | b | c | d);
        }


        private static ulong ToUInt64LittleEndian(byte[] buffer, int offset)
        {
            return ((ulong)ToUInt32LittleEndian(buffer, offset + 4) << 32) | ToUInt32LittleEndian(buffer, offset + 0);
        }
        
        //
        // https://stackoverflow.com/questions/1471975/best-way-to-find-position-in-the-stream-where-given-byte-sequence-starts
        //
        public static long FindPosition(Stream stream, byte[] byteSequence)
        {
            if (byteSequence.Length > stream.Length)
            {
                return -1;
            }

            var buffer = new byte[byteSequence.Length];

            var bufStream = new BufferedStream(stream, byteSequence.Length);
            int i;

            while ((i = bufStream.Read(buffer, 0, byteSequence.Length)) == byteSequence.Length)
            {
                if (byteSequence.SequenceEqual(buffer))
                {
                    return bufStream.Position - byteSequence.Length;
                }

                bufStream.Position -= byteSequence.Length - PadLeftSequence(buffer, byteSequence);
            }

            return -1;
        }

        private static int PadLeftSequence(byte[] bytes, byte[] seqBytes)
        {
            var i = 1;
            while (i < bytes.Length)
            {
                var n = bytes.Length - i;
                var aux1 = new byte[n];
                var aux2 = new byte[n];
                Array.Copy(bytes, i, aux1, 0, n);
                Array.Copy(seqBytes, aux2, n);
                if (aux1.SequenceEqual(aux2))
                {
                    return i;
                }

                i++;
            }

            return i;
        }

        protected override Stream GetXmlMetadataStream(Stream wim)
        {
            wim.Seek(0, SeekOrigin.Begin);
            var header = new byte[208];
            if (wim.Read(header, 0, header.Length) < header.Length)
            {
                throw new InvalidOperationException("The WIM header is truncated.");
            }

            if (header[0] != 0x4D || header[1] != 0x53 || header[2] != 0x57 || header[3] != 0x49 || header[4] != 0x4D)
            {
                throw new InvalidOperationException("The file does not start with a WIM header.");
            }

            var xmlSizeField = ToUInt64LittleEndian(header, 72);
            var xmlOffset = ToInt64LittleEndian(header, 80);
            var xmlOriginalSize = ToInt64LittleEndian(header, 88);
            var xmlStoredSize = (long)(xmlSizeField & 0x00FFFFFFFFFFFFFFUL);
            var xmlFlags = (byte)(xmlSizeField >> 56);

            Log.Verbose("(WIM) XML resource offset={Offset} stored={Stored} original={Original} flags=0x{Flags:X2}",
                xmlOffset, xmlStoredSize, xmlOriginalSize, xmlFlags);

            if (xmlOffset < 0 || xmlStoredSize <= 0 || xmlOffset + xmlStoredSize > wim.Length)
            {
                throw new InvalidOperationException("The WIM XML resource location is not valid.");
            }

            if ((xmlFlags & 0x04) != 0)
            {
                throw new InvalidOperationException("Compressed WIM XML is not supported. Export the image with DISM.");
            }

            wim.Seek(xmlOffset, SeekOrigin.Begin);
            var stored = new byte[xmlStoredSize];
            var read = 0;
            while (read < stored.Length)
            {
                var n = wim.Read(stored, read, stored.Length - read);
                if (n <= 0)
                {
                    break;
                }

                read += n;
            }

            if (read < stored.Length)
            {
                throw new InvalidOperationException("The WIM XML resource is truncated.");
            }

            var text = DecodeWimXml(stored);
            var utf8 = Encoding.UTF8.GetBytes(text);
            return new MemoryStream(utf8);
        }

        private static string DecodeWimXml(byte[] stored)
        {
            if (stored.Length >= 2 && stored[0] == 0xFF && stored[1] == 0xFE)
            {
                return Encoding.Unicode.GetString(stored, 2, stored.Length - 2);
            }

            if (stored.Length >= 2 && stored[0] == 0xFE && stored[1] == 0xFF)
            {
                return Encoding.BigEndianUnicode.GetString(stored, 2, stored.Length - 2);
            }

            if (stored.Length >= 3 && stored[0] == 0xEF && stored[1] == 0xBB && stored[2] == 0xBF)
            {
                return Encoding.UTF8.GetString(stored, 3, stored.Length - 3);
            }

            if (stored.Length >= 2 && stored[1] == 0)
            {
                return Encoding.Unicode.GetString(stored);
            }

            return Encoding.UTF8.GetString(stored);
        }
    }    
}