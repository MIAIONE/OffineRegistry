using System;

namespace OffineRegistry
{
    public abstract class RegistryBase : IDisposable
    {
        protected IntPtr _intPtr;

        public abstract void Close();

        public abstract void Dispose();
    }
}