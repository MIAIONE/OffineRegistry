using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security;
using System.Text;

namespace OffineRegistry
{
    public class SubKeyContainer
    {
        public string Name { get; set; }
        public string Class { get; set; }
        public FILETIME LastWriteTime { get; set; }
    }

    public class ValueContainer
    {
        public string Name { get; set; }
        public object Data { get; set; }
        public RegValueType Type { get; set; }
        public bool InvalidData { get; set; }
    }
    [SuppressUnmanagedCodeSecurity]
    public class RegistryKey : RegistryBase
    {
        private readonly RegistryHive Hive;
        public string Name { get; protected set; }

        public string FullName { get; protected set; }

        private readonly RegistryKey _parent;

        public int SubkeyCount
        {
            get { return (int)_metadata.SubKeysCount; }
        }

        public int ValueCount
        {
            get { return (int)_metadata.ValuesCount; }
        }

        private readonly bool _ownsPointer = true;

        private readonly QueryInfoKeyData _metadata;

        public static class InitOffreg
        {
            internal static Native NativeApi;
            internal static Version OffregVer;
            public static void InitLibrary(string offregPath)
            {
                NativeApi = new Native(offregPath);
                var fileinfo = FileVersionInfo.GetVersionInfo(offregPath);
                
                OffregVer = new Version(fileinfo.ProductVersion);
                //Console.WriteLine(OffregVer.ToString());
            }
        }
        internal RegistryKey(RegistryKey parent, IntPtr ptr, string name, RegistryHive registryHive)
        {
            _intPtr = ptr;

            Name = name;
            FullName = (parent == null || parent.FullName == null ? "" : parent.FullName + "\\") + name;
            _parent = parent;

            _metadata = new QueryInfoKeyData();
            RefreshMetadata();

            Hive = registryHive;
        }

        internal RegistryKey(RegistryKey parentKey, string name, RegistryHive registryHive)
        {
            Win32Result result = InitOffreg.NativeApi.Syscall<Native.OROpenKey>()(parentKey._intPtr, name, out _intPtr);

            if (result != Win32Result.ERROR_SUCCESS)
                throw new Win32Exception((int)result);

            Name = name;
            FullName = (parentKey.FullName == null ? "" : parentKey.FullName + "\\") + name;
            _parent = parentKey;

            _metadata = new QueryInfoKeyData();
            RefreshMetadata();

            Hive = registryHive;
        }

        public static RegistryKey CreateHive(string hiveFile)
        {
            if (File.Exists(hiveFile))
            {
                return LoadHive(hiveFile);
            }
            else
            {
                return RegistryHive.Create(hiveFile).Root;
            }
        }

        public static RegistryKey LoadHive(string hiveFile)
        {
            return RegistryHive.Open(hiveFile).Root;
        }

        public void SaveHive(Version version)
        {
            File.Delete(Hive.HivePath);
            Hive.Save(version.Major, version.Minor);
        }
        public void SaveHive()
        {
            File.Delete(Hive.HivePath);
            Hive.Save(InitOffreg.OffregVer.Major, InitOffreg.OffregVer.Minor);
        }
        private void RefreshMetadata()
        {
            uint sizeClass = 0;
            uint countSubKeys = 0, maxSubKeyLen = 0, maxClassLen = 0;
            uint countValues = 0, maxValueNameLen = 0, maxValueLen = 0;
            uint securityDescriptorSize = 0;
            FILETIME lastWrite = new FILETIME();

            StringBuilder sbClass = new StringBuilder((int)sizeClass);

            Win32Result result = InitOffreg.NativeApi.Syscall<Native.ORQueryInfoKey>()(_intPtr, sbClass, ref sizeClass, ref countSubKeys,
                                                           ref maxSubKeyLen,
                                                           ref maxClassLen,
                                                           ref countValues, ref maxValueNameLen, ref maxValueLen,
                                                           ref securityDescriptorSize,
                                                           ref lastWrite);

            if (result == Win32Result.ERROR_MORE_DATA)
            {
                sizeClass++;

                sbClass = new StringBuilder((int)sizeClass);

                result = InitOffreg.NativeApi.Syscall<Native.ORQueryInfoKey>()(_intPtr, sbClass, ref sizeClass, ref countSubKeys, ref maxSubKeyLen,
                                                   ref maxClassLen,
                                                   ref countValues, ref maxValueNameLen, ref maxValueLen,
                                                   ref securityDescriptorSize,
                                                   ref lastWrite);

                if (result != Win32Result.ERROR_SUCCESS)
                    throw new Win32Exception((int)result);
            }
            else if (result != Win32Result.ERROR_SUCCESS)
                throw new Win32Exception((int)result);

            _metadata.Class = sbClass.ToString();
            _metadata.LastWriteTime = lastWrite;

            _metadata.SubKeysCount = countSubKeys;
            _metadata.MaxSubKeyLen = maxSubKeyLen;
            _metadata.MaxClassLen = maxClassLen;
            _metadata.ValuesCount = countValues;
            _metadata.MaxValueNameLen = maxValueNameLen;
            _metadata.MaxValueLen = maxValueLen; // Bytes
            _metadata.SizeSecurityDescriptor = securityDescriptorSize;
        }

        public SubKeyContainer[] EnumerateSubKeys()
        {
            SubKeyContainer[] results = new SubKeyContainer[_metadata.SubKeysCount];

            for (uint item = 0; item < _metadata.SubKeysCount; item++)
            {
                uint sizeName = _metadata.MaxSubKeyLen + 1;
                uint sizeClass = _metadata.MaxClassLen + 1;

                StringBuilder sbName = new StringBuilder((int)sizeName);
                StringBuilder sbClass = new StringBuilder((int)sizeClass);
                FILETIME fileTime = new FILETIME();

                Win32Result result = InitOffreg.NativeApi.Syscall<Native.OREnumKey>()(_intPtr, item, sbName, ref sizeName, sbClass, ref sizeClass,
                                                          ref fileTime);

                if (result != Win32Result.ERROR_SUCCESS)
                    throw new Win32Exception((int)result);

                SubKeyContainer container = new SubKeyContainer
                {
                    Name = sbName.ToString(),
                    Class = sbClass.ToString(),
                    LastWriteTime = fileTime
                };

                results[item] = container;
            }

            return results;
        }

        public string[] GetSubKeyNames()
        {
            string[] results = new string[_metadata.SubKeysCount];

            for (uint item = 0; item < _metadata.SubKeysCount; item++)
            {
                uint sizeName = _metadata.MaxSubKeyLen + 1;

                StringBuilder sbName = new StringBuilder((int)sizeName);
                Win32Result result = InitOffreg.NativeApi.Syscall<Native.PtrClass.OREnumValue>()(_intPtr, item, sbName, ref sizeName, (IntPtr)null, IntPtr.Zero,
                                                          IntPtr.Zero);

                if (result != Win32Result.ERROR_SUCCESS)
                    throw new Win32Exception((int)result);

                results[item] = sbName.ToString();
            }

            return results;
        }

        public RegistryKey OpenSubKey(string path)
        {
            RefreshMetadata();
            var isSuccess = TryOpenSubKey(path, out var result);
            if (isSuccess)
            {
                return result;
            }
            else
            {
                throw new Win32Exception($"path not found sub key '{path}'");
            }
        }

        public bool IsExistSubKey(string path)
        {
            return TryOpenSubKey(path, out _);
        }
        public bool TryOpenSubKey(string path, out RegistryKey key)
        {
            RefreshMetadata();
            var names = path.Replace('/', '\\').Split('\\', StringSplitOptions.RemoveEmptyEntries);
            var nextKey = this;
            foreach (var name in names)
            {
                var isSuccess = nextKey.TryOpenSubKeyPrivate(name, out RegistryKey newkey);
                if (isSuccess)
                {
                    nextKey = newkey;
                }
                else
                {
                    key = null;
                    return false;
                }
            }
            key = nextKey;

            return true;
        }

        private RegistryKey OpenSubKeyPrivate(string name)
        {
            return new RegistryKey(this, name, Hive);
        }

        private bool TryOpenSubKeyPrivate(string name, out RegistryKey key)
        {
            RefreshMetadata();
            Win32Result result = InitOffreg.NativeApi.Syscall<Native.OROpenKey>()(_intPtr, name, out IntPtr childPtr);

            if (result != Win32Result.ERROR_SUCCESS)
            {
                key = null;
                return false;
            }

            key = new RegistryKey(this, childPtr, name, Hive);
            return true;
        }

        public RegistryKey CreateSubKey(string path)
        {
            var names = path.Replace('/', '\\').Split('\\', StringSplitOptions.RemoveEmptyEntries);
            var nextKey = this;
            foreach (var name in names)
            {
                nextKey = nextKey.CreateSubKey(name, RegOption.REG_OPTION_NON_VOLATILE);
            }
            RefreshMetadata();
            return nextKey;
        }
        private RegistryKey CreateSubKey(string name, RegOption options = RegOption.REG_OPTION_NON_VOLATILE)
        {
            var isExist = TryOpenSubKey(name, out RegistryKey key);
            if (isExist)
            {
                return key;
            }
            else
            {
                Win32Result result = InitOffreg.NativeApi.Syscall<Native.ORCreateKey>()(_intPtr, name, null, options, IntPtr.Zero, out IntPtr newKeyPtr,
                                                            out KeyDisposition _);

                if (result != Win32Result.ERROR_SUCCESS)
                    throw new Win32Exception((int)result);

                RegistryKey newKey = new RegistryKey(this, newKeyPtr, name, Hive);

                RefreshMetadata();

                return newKey;
            }
        }

        private void DeleteCurrentSubKey()
        {
            if (_parent == null)
                throw new InvalidOperationException("Cannot delete the root key");

            Win32Result result = InitOffreg.NativeApi.Syscall<Native.ORDeleteKey>()(_intPtr, null);

            if (result != Win32Result.ERROR_SUCCESS)
                throw new Win32Exception((int)result);

            _parent.RefreshMetadata();
        }
        public void Delete()
        {
            DeleteSubKeyPrivate(this);
        }

        public void DeleteSubKey(string name)
        {
            using (RegistryKey subKey = OpenSubKeyPrivate(name))
            {
                DeleteSubKeyTreePrivate(subKey);
            }

            RefreshMetadata();
        }
        private void DeleteSubKeyPrivate(RegistryKey Key)
        {
            using (var subKey = Key)
            {
                DeleteSubKeyTreePrivate(subKey);
            }

            RefreshMetadata();
        }

        private static void DeleteSubKeyTreePrivate(RegistryKey key)
        {
            string[] childs = key.GetSubKeyNames();

            foreach (string child in childs)
            {
                try
                {
                    using RegistryKey childKey = key.OpenSubKeyPrivate(child);
                    DeleteSubKeyTreePrivate(childKey);
                }
                catch (Win32Exception ex)
                {
                    switch (ex.NativeErrorCode)
                    {
                        case (int)Win32Result.ERROR_FILE_NOT_FOUND:
                            break;

                        default:
                            throw;
                    }
                }
            }

            key.DeleteCurrentSubKey();
        }

        public string[] GetValueNames()
        {
            string[] results = new string[_metadata.ValuesCount];

            for (uint item = 0; item < _metadata.ValuesCount; item++)
            {
                uint sizeName = _metadata.MaxValueNameLen + 1;

                StringBuilder sbName = new StringBuilder((int)sizeName);

                Win32Result result = InitOffreg.NativeApi.Syscall<Native.PtrClass.OREnumValue>()(_intPtr, item, sbName, ref sizeName, IntPtr.Zero,
                                                            IntPtr.Zero,
                                                            IntPtr.Zero);

                if (result != Win32Result.ERROR_SUCCESS)
                    throw new Win32Exception((int)result);

                results[item] = sbName.ToString();
            }

            return results;
        }

        public ValueContainer[] EnumerateValues()
        {
            ValueContainer[] results = new ValueContainer[_metadata.ValuesCount];

            IntPtr dataPtr = IntPtr.Zero;
            try
            {
                dataPtr = Marshal.AllocHGlobal((int)_metadata.MaxValueLen);

                for (uint item = 0; item < _metadata.ValuesCount; item++)
                {
                    uint sizeName = _metadata.MaxValueNameLen + 1;
                    uint sizeData = _metadata.MaxValueLen;

                    StringBuilder sbName = new StringBuilder((int)sizeName);

                    Win32Result result = InitOffreg.NativeApi.Syscall<Native.OREnumValue>()(_intPtr, item, sbName, ref sizeName, out RegValueType type, dataPtr,
                                                                ref sizeData);

                    if (result != Win32Result.ERROR_SUCCESS)
                        throw new Win32Exception((int)result);

                    byte[] data = new byte[sizeData];
                    Marshal.Copy(dataPtr, data, 0, (int)sizeData);

                    ValueContainer container = new ValueContainer();

                    if (!Enum.IsDefined(typeof(RegValueType), type))
                    {
                        WarnDebugForValueType(sbName.ToString(), type);
                        type = RegValueType.REG_BINARY;
                    }

                    container.Name = sbName.ToString();
                    container.InvalidData = !Utils.TryConvertValueDataToObject(type, data, out object parsedData);
                    container.Data = parsedData;
                    container.Type = type;

                    results[item] = container;
                }
            }
            finally
            {
                if (dataPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(dataPtr);
            }

            return results;
        }

        public RegValueType GetValueKind(string name)
        {
            Win32Result result = InitOffreg.NativeApi.Syscall<Native.PtrClass.ORGetValue>()(_intPtr, null, name, out RegValueType type, IntPtr.Zero, IntPtr.Zero);

            if (result != Win32Result.ERROR_SUCCESS)
                throw new Win32Exception((int)result);

            return type;
        }

        public object GetValue(string name)
        {
            Tuple<RegValueType, byte[]> internalData = GetValueInternal(name);

            if (!Enum.IsDefined(typeof(RegValueType), internalData.Item1))
            {
                WarnDebugForValueType(name, internalData.Item1);
                internalData = new Tuple<RegValueType, byte[]>(RegValueType.REG_BINARY, internalData.Item2);
            }

            Utils.TryConvertValueDataToObject(internalData.Item1, internalData.Item2, out object data);

            return data;
        }

        public bool TryParseValue(string name, out object data)
        {
            Tuple<RegValueType, byte[]> internalData = GetValueInternal(name);

            if (!Enum.IsDefined(typeof(RegValueType), internalData.Item1))
            {
                WarnDebugForValueType(name, internalData.Item1);
                internalData = new Tuple<RegValueType, byte[]>(RegValueType.REG_BINARY, internalData.Item2);
            }

            return Utils.TryConvertValueDataToObject(internalData.Item1, internalData.Item2, out data);
        }

        public bool IsValueExist(string name)
        {
            uint size = 0;
            Win32Result result = InitOffreg.NativeApi.Syscall<Native.ORGetValue>()(_intPtr, null, name, out RegValueType _, IntPtr.Zero, ref size);

            return result == Win32Result.ERROR_SUCCESS;
        }

        public byte[] GetValueBytes(string name)
        {
            Tuple<RegValueType, byte[]> data = GetValueInternal(name);

            return data.Item2;
        }

        public void SetValue(string name, string value, RegValueType type = RegValueType.REG_SZ)
        {
            byte[] data = new byte[Utils.StringEncoding.GetByteCount(value) + Utils.SingleCharBytes];
            Utils.StringEncoding.GetBytes(value, 0, value.Length, data, 0);

            SetValue(name, type, data);
        }

        public void SetValue(string name, string[] values, RegValueType type = RegValueType.REG_MULTI_SZ)
        {
            foreach (string value in values)
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("No empty strings allowed");

            int bytes = 0;
            foreach (string value in values)
            {
                bytes += Utils.StringEncoding.GetByteCount(value) + Utils.SingleCharBytes;
            }
            bytes += Utils.SingleCharBytes;

            byte[] data = new byte[bytes];

            int position = 0;
            for (int i = 0; i < values.Length; i++)
            {
                position += Utils.StringEncoding.GetBytes(values[i], 0, values[i].Length, data, position) +
                            Utils.SingleCharBytes;
            }

            SetValue(name, type, data);
        }

        public void SetValue(string name, byte[] value, RegValueType type = RegValueType.REG_BINARY)
        {
            SetValue(name, type, value);
        }

        public void SetValue(string name, int value, RegValueType type = RegValueType.REG_DWORD)
        {
            byte[] data = BitConverter.GetBytes(value);

            if (type == RegValueType.REG_DWORD_BIG_ENDIAN)
                Array.Reverse(data);

            SetValue(name, type, data);
        }

        public void SetValue(string name, long value, RegValueType type = RegValueType.REG_QWORD)
        {
            byte[] data = BitConverter.GetBytes(value);

            SetValue(name, type, data);
        }

        public void SetValueNone(string name)
        {
            IntPtr dataPtr = IntPtr.Zero;
            try
            {
                dataPtr = Marshal.AllocHGlobal(0);

                Win32Result result = InitOffreg.NativeApi.Syscall<Native.ORSetValue>()(
                    _intPtr, name, RegValueType.REG_NONE, dataPtr, (uint)0);

                if (result != Win32Result.ERROR_SUCCESS)
                    throw new Win32Exception((int)result);
            }
            finally
            {
                if (dataPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(dataPtr);
            }

            RefreshMetadata();
        }

        private void SetValue(string name, RegValueType type, byte[] data)
        {
            IntPtr dataPtr = IntPtr.Zero;
            try
            {
                dataPtr = Marshal.AllocHGlobal(data.Length);
                Marshal.Copy(data, 0, dataPtr, data.Length);

                Win32Result result = InitOffreg.NativeApi.Syscall<Native.ORSetValue>()(_intPtr, name, type, dataPtr, (uint)data.Length);

                if (result != Win32Result.ERROR_SUCCESS)
                    throw new Win32Exception((int)result);
            }
            finally
            {
                if (dataPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(dataPtr);
            }

            RefreshMetadata();
        }

        public void DeleteValue(string name)
        {
            Win32Result result = InitOffreg.NativeApi.Syscall<Native.ORDeleteValue>()(_intPtr, name);

            if (result != Win32Result.ERROR_SUCCESS)
                throw new Win32Exception((int)result);

            RefreshMetadata();
        }

        internal Tuple<RegValueType, byte[]> GetValueInternal(string name)
        {
            uint size = 0;
            Win32Result result = InitOffreg.NativeApi.Syscall<Native.ORGetValue>()(_intPtr, null, name, out RegValueType type, IntPtr.Zero, ref size);

            if (result != Win32Result.ERROR_SUCCESS)
                throw new Win32Exception((int)result);

            byte[] res = new byte[size];
            IntPtr dataPtr = IntPtr.Zero;
            try
            {
                dataPtr = Marshal.AllocHGlobal((int)size);

                result = InitOffreg.NativeApi.Syscall<Native.ORGetValue>()(_intPtr, null, name, out type, dataPtr, ref size);

                if (result != Win32Result.ERROR_SUCCESS)
                    throw new Win32Exception((int)result);

                Marshal.Copy(dataPtr, res, 0, (int)size);
            }
            finally
            {
                if (dataPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(dataPtr);
            }

            return new Tuple<RegValueType, byte[]>(type, res);
        }

        public override void Close()
        {
            if (_intPtr != IntPtr.Zero && _ownsPointer && _parent != null)
            {
                Win32Result res = InitOffreg.NativeApi.Syscall<Native.ORCloseKey>()(_intPtr);

                if (res != Win32Result.ERROR_SUCCESS)
                    throw new Win32Exception((int)res);
            }
            if (_parent == null)
            {
                Hive.Close();
            }
        }

        public override void Dispose()
        {
            GC.SuppressFinalize(this);
            Close();
        }

        private void WarnDebugForValueType(string valueName, RegValueType parsedType)
        {
            Debug.WriteLine("WARNING-OFFREGLIB: unknown RegValueType " + parsedType + " converted to Binary in EnumerateValues() at key: " + FullName + ", value: " + valueName);
        }
    }
}