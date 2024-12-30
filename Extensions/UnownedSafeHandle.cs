using System;
using System.Runtime.InteropServices;

public class UnownedSafeHandle : SafeHandle
{
    public UnownedSafeHandle(IntPtr _existingHandle) : base(IntPtr.Zero, false)
    {
        SetHandle(_existingHandle);
    }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle() => true;
}
