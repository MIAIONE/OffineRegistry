using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security;
using System.Text;

namespace OffineRegistry
{
    public enum RegValueType : uint
    {
        REG_NONE = 0,
        REG_SZ = 1,
        REG_EXPAND_SZ = 2,
        REG_BINARY = 3,
        REG_DWORD = 4,
        REG_DWORD_LITTLE_ENDIAN = 4,
        REG_DWORD_BIG_ENDIAN = 5,
        REG_LINK = 6,
        REG_MULTI_SZ = 7,
        REG_RESOURCE_LIST = 8,
        REG_FULL_RESOURCE_DESCRIPTOR = 9,
        REG_RESOURCE_REQUIREMENTS_LIST = 10,
        REG_QWORD = 11,
        REG_QWORD_LITTLE_ENDIAN = 11
    }

    public enum RegPredefinedKeys
    {
        HKEY_CLASSES_ROOT = unchecked((int)0x80000000),
        HKEY_CURRENT_USER = unchecked((int)0x80000001),
        HKEY_LOCAL_MACHINE = unchecked((int)0x80000002),
        HKEY_USERS = unchecked((int)0x80000003),
        HKEY_PERFORMANCE_DATA = unchecked((int)0x80000004),
        HKEY_CURRENT_CONFIG = unchecked((int)0x80000005),
        HKEY_DYN_DATA = unchecked((int)0x80000006),
        HKEY_CURRENT_USER_LOCAL_SETTINGS = unchecked((int)0x80000007)
    }

    public enum KeyDisposition : long
    {
        REG_CREATED_NEW_KEY = 0x00000001,
        REG_OPENED_EXISTING_KEY = 0x00000002
    }

    public enum KeySecurity
    {
        KEY_QUERY_VALUE = 0x0001,
        KEY_SET_VALUE = 0x0002,
        KEY_ENUMERATE_SUB_KEYS = 0x0008,
        KEY_NOTIFY = 0x0010,
        DELETE = 0x10000,
        STANDARD_RIGHTS_READ = 0x20000,
        KEY_READ = 0x20019,
        KEY_WRITE = 0x20006,
        KEY_ALL_ACCESS = 0xF003F,
        MAXIMUM_ALLOWED = 0x2000000
    }

    [Flags]
    public enum RegOption : uint
    {
        REG_OPTION_RESERVED = 0x00000000,
        REG_OPTION_NON_VOLATILE = 0x00000000,
        REG_OPTION_VOLATILE = 0x00000001,
        REG_OPTION_CREATE_LINK = 0x00000002,
        REG_OPTION_BACKUP_RESTORE = 0x00000004,
        REG_OPTION_OPEN_LINK = 0x00000008
    }

    public enum SECURITY_INFORMATION : uint
    {
        OWNER_SECURITY_INFORMATION = 0x00000001,
        GROUP_SECURITY_INFORMATION = 0x00000002,
        DACL_SECURITY_INFORMATION = 0x00000004,
        SACL_SECURITY_INFORMATION = 0x00000008,
        LABEL_SECURITY_INFORMATION = 0x00000010,
        PROTECTED_DACL_SECURITY_INFORMATION = 0x80000000,
        PROTECTED_SACL_SECURITY_INFORMATION = 0x40000000,
        UNPROTECTED_DACL_SECURITY_INFORMATION = 0x20000000,
        UNPROTECTED_SACL_SECURITY_INFORMATION = 0x10000000,
    }
    [SuppressUnmanagedCodeSecurity]
    internal class Native
    {
        //internal const string OffRegDllName = "offreg.dll";

        //[DllImport(OffRegDllName, EntryPoint = "ORCreateHive", CharSet = CharSet.Unicode)]
        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Unicode)]
        internal delegate Win32Result ORCreateHive(out IntPtr rootKeyHandle);

        //[DllImport(OffRegDllName, EntryPoint = "OROpenHive", CharSet = CharSet.Unicode)]
        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Unicode)]
        internal delegate Win32Result OROpenHive(string path, out IntPtr rootKeyHandle);

        //[DllImport(OffRegDllName, EntryPoint = "ORCloseHive", CharSet = CharSet.Unicode)]
        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Unicode)]
        internal delegate Win32Result ORCloseHive(IntPtr rootKeyHandle);

        //[DllImport(OffRegDllName, EntryPoint = "ORSaveHive", CharSet = CharSet.Unicode)]
        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Unicode)]
        internal delegate Win32Result ORSaveHive(
            IntPtr rootKeyHandle,
            string path,
            uint dwOsMajorVersion,
            uint dwOsMinorVersion);

        //[DllImport(OffRegDllName, EntryPoint = "ORCloseKey")]
        internal delegate Win32Result ORCloseKey(IntPtr hKey);

        //[DllImport(OffRegDllName, EntryPoint = "ORCreateKey", CharSet = CharSet.Unicode)]
        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Unicode)]
        internal delegate Win32Result ORCreateKey(
            IntPtr hKey,
            string lpSubKey,
            string lpClass,
            RegOption dwOptions,
            /*ref SECURITY_DESCRIPTOR*/ IntPtr lpSecurityDescriptor,
            /*ref IntPtr*/ out IntPtr phkResult,
            out KeyDisposition lpdwDisposition);

        //[DllImport(OffRegDllName, EntryPoint = "ORDeleteKey", CharSet = CharSet.Unicode)]
        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Unicode)]
        internal delegate Win32Result ORDeleteKey(
            IntPtr hKey,
            string lpSubKey);

        //[DllImport(OffRegDllName, EntryPoint = "ORDeleteValue", CharSet = CharSet.Unicode)]
        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Unicode)]
        internal delegate Win32Result ORDeleteValue(
            IntPtr hKey,
            string lpValueName);

        //[DllImport(OffRegDllName, EntryPoint = "OREnumKey", CharSet = CharSet.Unicode)]
        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Unicode)]
        internal delegate Win32Result OREnumKey(
            IntPtr hKey,
            uint dwIndex,
            StringBuilder lpName,
            ref uint lpcchName,
            StringBuilder lpClass,
            ref uint lpcchClass,
            ref FILETIME lpftLastWriteTime);

        //[DllImport(OffRegDllName, EntryPoint = "OREnumValue", CharSet = CharSet.Unicode)]
        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Unicode)]
        internal delegate Win32Result OREnumValue(
            IntPtr hKey,
            uint dwIndex,
            StringBuilder lpValueName,
            ref uint lpcchValueName,
            out RegValueType lpType,
            IntPtr lpData,
            ref uint lpcbData);



        //[DllImport(OffRegDllName, EntryPoint = "ORGetKeySecurity")]
        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Unicode)]
        internal delegate Win32Result ORGetKeySecurity(
            IntPtr hKey,
            SECURITY_INFORMATION securityInformation,
            IntPtr pSecurityDescriptor,
            ref uint lpcbSecurityDescriptor);

        //[DllImport(OffRegDllName, EntryPoint = "ORGetValue", CharSet = CharSet.Unicode)]
        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Unicode)]
        internal delegate Win32Result ORGetValue(
            IntPtr hKey,
            string lpSubKey,
            string lpValue,
            out RegValueType pdwType,
            IntPtr pvData,
            ref uint pcbData);



        //[DllImport(OffRegDllName, EntryPoint = "OROpenKey", CharSet = CharSet.Unicode)]
        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Unicode)]
        internal delegate Win32Result OROpenKey(
            IntPtr hKey,
            string lpSubKey,
            out IntPtr phkResult);

        //[DllImport(OffRegDllName, EntryPoint = "ORQueryInfoKey", CharSet = CharSet.Unicode)]
        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Unicode)]
        internal delegate Win32Result ORQueryInfoKey(
            IntPtr hKey,
            StringBuilder lpClass,
            ref uint lpcchClass,
            ref uint lpcSubKeys,
            ref uint lpcbMaxSubKeyLen,
            ref uint lpcbMaxClassLen,
            ref uint lpcValues,
            ref uint lpcbMaxValueNameLen,
            ref uint lpcbMaxValueLen,
            ref uint lpcbSecurityDescriptor,
            ref FILETIME lpftLastWriteTime);

        //[DllImport(OffRegDllName, EntryPoint = "ORSetValue", CharSet = CharSet.Unicode)]
        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Unicode)]
        internal delegate Win32Result ORSetValue(
            IntPtr hKey,
            string lpValueName,
            RegValueType dwType,
            IntPtr lpData,
            uint cbData);

        //[DllImport(OffRegDllName, EntryPoint = "ORSetKeySecurity")]
        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Unicode)]
        internal delegate Win32Result ORSetKeySecurity(
            IntPtr hKey,
            SECURITY_INFORMATION securityInformation,
            /*ref IntPtr*/ IntPtr pSecurityDescriptor);

        public class PtrClass
        {
            //[DllImport(OffRegDllName, EntryPoint = "ORGetValue", CharSet = CharSet.Unicode)]
            [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Unicode)]
            internal delegate Win32Result ORGetValue(
                IntPtr hKey,
                string lpSubKey,
                string lpValue,
                out RegValueType pdwType,
                IntPtr pvData,
                IntPtr pcbData);
            //[DllImport(OffRegDllName, EntryPoint = "OREnumValue", CharSet = CharSet.Unicode)]
            [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Unicode)]
            internal delegate Win32Result OREnumValue(
                IntPtr hKey,
                uint dwIndex,
                StringBuilder lpValueName,
                ref uint lpcchValueName,
                IntPtr lpType,
                IntPtr lpData,
                IntPtr lpcbData);

        }
        /*-------------------------------------------------------------------*/
        public readonly IntPtr OffregLibraryAddress;
        public Native(string offregPath)
        {
            OffregLibraryAddress = LoadLibrary(offregPath);
        }
        ~Native()
        {
            FreeLibrary(OffregLibraryAddress);
        }
        public T Syscall<T>() where T : Delegate
        {
            return Marshal.GetDelegateForFunctionPointer<T>(GetProcAddress(OffregLibraryAddress, typeof(T).Name));
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeLibrary(IntPtr hModule);
    }
}