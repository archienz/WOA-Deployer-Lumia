using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Deployer.Lumia.NetFx.PhoneMetadata.Streams
{
    public class DeviceStream : Stream
    {
        private const uint GENERIC_READ = 0x80000000;
        private const uint OPEN_EXISTING = 3;
        private const uint DEVICE = 0x00000040;
        private const uint NOBUFFERING = 0x20000000;
        private const uint FILE_SHARE_READ = 0x00000001;
        private const uint FILE_SHARE_WRITE = 0x00000002;

        private string PhysicalDiskId;
        private long _Position = 0;

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess,
          uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition,
          uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadFile(
            IntPtr hFile,
            byte[] lpBuffer,
            int nNumberOfBytesToRead,
            ref int lpNumberOfBytesRead,
            IntPtr lpOverlapped
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint DeviceIoControl(
            SafeFileHandle hDevice,
            uint dwIoControlCode,
            IntPtr lpInBuffer,
            uint nInBufferSize,
            IntPtr lpOutBuffer,
            int nOutBufferSize,
            ref uint lpBytesReturned,
            IntPtr lpOverlapped
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetFilePointerEx(SafeFileHandle hFile, long liDistanceToMove, out long lpNewFilePointer, uint dwMoveMethod);

        private SafeFileHandle handleValue = null;
        private long _length;

        private static uint FILE_DEVICE_FILE_SYSTEM = 9;
        private static uint METHOD_BUFFERED = 0;
        private static uint FILE_ANY_ACCESS = 0x00000000;

        private static uint FSCTL_LOCK_VOLUME = CTL_CODE(FILE_DEVICE_FILE_SYSTEM, 6, METHOD_BUFFERED, FILE_ANY_ACCESS);
        private static uint FSCTL_UNLOCK_VOLUME = CTL_CODE(FILE_DEVICE_FILE_SYSTEM, 7, METHOD_BUFFERED, FILE_ANY_ACCESS);

        private static uint CTL_CODE(uint DeviceType, uint Function, uint Method, uint Access)
        {
            return (((DeviceType) << 16) | ((Access) << 14) | ((Function) << 2) | (Method));
        }

        public DeviceStream(string device)
        {
            if (string.IsNullOrEmpty(device))
            {
                throw new ArgumentNullException("Path");
            }

            PhysicalDiskId = device;
            Load();
        }

        private void Load()
        {
            _length = GetDiskSize.GetDiskLength(@"\\.\PhysicalDrive" + PhysicalDiskId.ToLower().Replace(@"\\.\physicaldrive", ""));
            IntPtr ptr = CreateFile(@"\\.\PhysicalDrive" + PhysicalDiskId.ToLower().Replace(@"\\.\physicaldrive", ""), GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, DEVICE, IntPtr.Zero);
            handleValue = new SafeFileHandle(ptr, true);

            if (handleValue.IsInvalid)
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }

            uint lpBytesReturned = 0;
            var result = DeviceIoControl(handleValue, FSCTL_LOCK_VOLUME, IntPtr.Zero, 0, IntPtr.Zero, 0, ref lpBytesReturned, IntPtr.Zero);

            if (0 == result)
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            return;
        }

        public override long Length
        {
            get
            {
                return _length;
            }
        }

        public override long Position
        {
            get
            {
                return _Position;
            }
            set
            {
                Seek(value, SeekOrigin.Begin);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between offset and 
        /// (offset + count - 1) replaced by the bytes read from the current source. </param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream. </param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns></returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (offset < 0 || count < 0 || offset + count > buffer.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (count == 0)
            {
                return 0;
            }

            if (!SetFilePointerEx(handleValue, _Position, out _, 0))
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }

            var dest = offset == 0 ? buffer : new byte[count];
            int bytesRead = 0;
            if (!ReadFile(handleValue.DangerousGetHandle(), dest, count, ref bytesRead, IntPtr.Zero))
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }

            if (offset != 0 && bytesRead > 0)
            {
                Buffer.BlockCopy(dest, 0, buffer, offset, bytesRead);
            }

            _Position += bytesRead;
            return bytesRead;
        }

        public override int ReadByte()
        {
            var lpBuffer = new byte[1];
            var read = Read(lpBuffer, 0, 1);
            if (read == 0)
            {
                return -1;
            }

            return lpBuffer[0];
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long off = offset;

            switch (origin)
            {
                case SeekOrigin.Current:
                    off += _Position;
                    break;
                case SeekOrigin.End:
                    off += _length;
                    break;
            }

            long ret;
            if (!SetFilePointerEx(handleValue, off, out ret, 0))
                return _Position;
            _Position = ret;

            return ret;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override void Close()
        {
            Dispose(true);
            base.Close();
        }

        private bool disposed;

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing && handleValue != null && !handleValue.IsInvalid)
                {
                    uint lpBytesReturned = 0;
                    DeviceIoControl(handleValue, FSCTL_UNLOCK_VOLUME, IntPtr.Zero, 0, IntPtr.Zero, 0, ref lpBytesReturned, IntPtr.Zero);
                    handleValue.Dispose();
                    handleValue = null;
                }

                disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}