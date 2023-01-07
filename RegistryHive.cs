using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace OffineRegistry
{
     public class OffregHive : RegistryBase
{
    /// <summary>
    ///     The Root key of this hive.
    /// </summary>
    public RegistryKey Root { get; private set; }

    /// <summary>
    ///     Internal constructor to form an Offline Registry Hive from an open handle.
    /// </summary>
    /// <param name="hivePtr"></param>
    internal OffregHive(IntPtr hivePtr)
    {
        _intPtr = hivePtr;

        // Represent this as a key also
        Root = new RegistryKey(null, _intPtr, null);
    }

    /// <summary>
    ///     Saves a hive to Disk.
    ///     See http://msdn.microsoft.com/en-us/library/ee210773(v=vs.85).aspx for more details.
    /// </summary>
    /// <remarks>The target file must not exist.</remarks>
    /// <param name="targetFile">The target file to write to.</param>
    /// <param name="majorVersionTarget">The compatibility version to save for, see the link in summary.</param>
    /// <param name="minorVersionTarget">The compatibility version to save for, see the link in summary.</param>
    public void SaveHive(string targetFile, uint majorVersionTarget, uint minorVersionTarget)
    {
        Win32Result res = Native.SaveHive(_intPtr, targetFile, majorVersionTarget, minorVersionTarget);

        if (res != Win32Result.ERROR_SUCCESS)
            throw new Win32Exception((int)res);
    }

    /// <summary>
    ///     Creates a new hive in memory.
    /// </summary>
    /// <returns>The newly created hive.</returns>
    public static OffregHive Create()
    {
            Win32Result res = Native.CreateHive(out IntPtr newHive);

            if (res != Win32Result.ERROR_SUCCESS)
            throw new Win32Exception((int)res);

        return new OffregHive(newHive);
    }

    /// <summary>
    ///     Opens an existing hive from the disk.
    /// </summary>
    /// <param name="hiveFile">The file to open.</param>
    /// <returns>The newly opened hive.</returns>
    public static OffregHive Open(string hiveFile)
    {
            Win32Result res = Native.OpenHive(hiveFile, out IntPtr existingHive);

            if (res != Win32Result.ERROR_SUCCESS)
            throw new Win32Exception((int)res);

        return new OffregHive(existingHive);
    }

    /// <summary>
    ///     Closes the hive and releases ressources used by it.
    /// </summary>
    public override void Close()
    {
        if (_intPtr != IntPtr.Zero)
        {
            Win32Result res = Native.CloseHive(_intPtr);

            if (res != Win32Result.ERROR_SUCCESS)
                throw new Win32Exception((int)res);
        }
    }

    /// <summary>
    ///     Disposes the hive object and releases ressources used by it.
    /// </summary>
    public override void Dispose()
    {
        GC.SuppressFinalize(this);
        Close();
    }
}
}
