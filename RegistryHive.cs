using System;
using System.ComponentModel;
using System.Security;
using static OffineRegistry.RegistryKey;

namespace OffineRegistry
{
    [SuppressUnmanagedCodeSecurity]
    internal class RegistryHive : RegistryBase
    {
        public RegistryKey Root { get; private set; }
        public string HivePath { get; private set; }
        internal RegistryHive(IntPtr hivePtr, string hivePath)
        {
            _intPtr = hivePtr;
            HivePath = hivePath;
            Root = new RegistryKey(null, _intPtr, null, this);
        }

        public void Save(int majorVersionTarget, int minorVersionTarget)
        {
            Win32Result res = InitOffreg.NativeApi.Syscall<Native.ORSaveHive>()(_intPtr, HivePath, (uint)majorVersionTarget, (uint)minorVersionTarget);

            if (res != Win32Result.ERROR_SUCCESS)
                throw new Win32Exception((int)res);
        }

        public static RegistryHive Create(string hiveFile)
        {
            Win32Result res = InitOffreg.NativeApi.Syscall<Native.ORCreateHive>()(out IntPtr newHive);

            if (res != Win32Result.ERROR_SUCCESS)
                throw new Win32Exception((int)res);

            return new RegistryHive(newHive, hiveFile);
        }

        public static RegistryHive Open(string hiveFile)
        {
            Win32Result res = InitOffreg.NativeApi.Syscall<Native.OROpenHive>()(hiveFile, out IntPtr existingHive);

            if (res != Win32Result.ERROR_SUCCESS)
                throw new Win32Exception((int)res);

            return new RegistryHive(existingHive, hiveFile);
        }

        public override void Close()
        {
            if (_intPtr != IntPtr.Zero)
            {
                Win32Result res = InitOffreg.NativeApi.Syscall<Native.ORCloseHive>()(_intPtr);

                if (res != Win32Result.ERROR_SUCCESS)
                    throw new Win32Exception((int)res);
            }
        }

        public override void Dispose()
        {
            GC.SuppressFinalize(this);
            Close();
        }
    }
}