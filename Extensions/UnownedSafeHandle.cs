using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

public class UnownedSafeHandle : SafeHandle
{
    public UnownedSafeHandle(IntPtr _existingHandle) : base(IntPtr.Zero, false)
    {
        SetHandle(_existingHandle);
    }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle() => true;
}
