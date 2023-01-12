using System;
using System.Text;

namespace OffineRegistry
{
    internal static class Utils
    {
        public static Encoding StringEncoding
        {
            get { return Encoding.Unicode; }
        }

        public const int SingleCharBytes = 2;

        public static bool TryConvertValueDataToObject(RegValueType type, byte[] data, out object parsedData)
        {
            parsedData = data;

            switch (type)
            {
                case RegValueType.REG_NONE:
                    return false;

                case RegValueType.REG_LINK: // This is a unicode string
                case RegValueType.REG_EXPAND_SZ: // This is a unicode string
                case RegValueType.REG_SZ:
                    if (data.Length % 2 != 0)
                        return false;

                    int toIndex = 0;
                    while (data.Length > toIndex + 2 && (data[toIndex] != 0 || data[toIndex + 1] != 0))
                        toIndex += 2;

                    parsedData = StringEncoding.GetString(data, 0, toIndex);
                    return true;

                case RegValueType.REG_BINARY:
                    return true;

                case RegValueType.REG_DWORD:
                    if (data.Length != 4)
                        return false;

                    parsedData = BitConverter.ToInt32(data, 0);
                    return true;

                case RegValueType.REG_DWORD_BIG_ENDIAN:
                    if (data.Length != 4)
                        return false;

                    Array.Reverse(data);
                    parsedData = BitConverter.ToInt32(data, 0);
                    return true;

                case RegValueType.REG_MULTI_SZ:
                    if (data.Length % 2 != 0)
                        return false;

                    if (data.Length == 0)
                        return false;

                    if (data.Length == 2 && data[0] == 0 && data[1] == 0)
                    {
                        parsedData = new string[0];
                        return true;
                    }

                    if (data[data.Length - 4] != 0 || data[data.Length - 3] != 0 || data[data.Length - 2] != 0 ||
                        data[data.Length - 1] != 0)
                        return false;

                    string s2 = StringEncoding.GetString(data, 0, data.Length - 4);
                    parsedData = s2.Split(new[] { '\0' });
                    return true;

                case RegValueType.REG_RESOURCE_LIST:
                    return true;

                case RegValueType.REG_FULL_RESOURCE_DESCRIPTOR:
                    return true;

                case RegValueType.REG_RESOURCE_REQUIREMENTS_LIST:
                    return true;

                case RegValueType.REG_QWORD:
                    if (data.Length != 8)
                        return false;

                    parsedData = BitConverter.ToInt64(data, 0);
                    return true;

                default:
                    throw new ArgumentOutOfRangeException("TryConvertValueDataToObject was given an invalid RegValueType: " + type);
            }
        }
    }
}