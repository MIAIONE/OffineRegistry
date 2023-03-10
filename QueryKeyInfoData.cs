using System.Runtime.InteropServices.ComTypes;

namespace OffineRegistry
{
    internal class QueryInfoKeyData
    {
        public string Class { get; set; }
        public uint SubKeysCount { get; set; }

        public uint MaxSubKeyLen { get; set; }

        public uint MaxClassLen { get; set; }

        public uint ValuesCount { get; set; }

        public uint MaxValueNameLen { get; set; }

        public uint MaxValueLen { get; set; }
        public uint SizeSecurityDescriptor { get; set; }
        public FILETIME LastWriteTime { get; set; }
    }
}