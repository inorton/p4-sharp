using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace cperforcesharp
{
    public class UnsafeNativeMethods
    {
        [DllImport("cperforce",CallingConvention = CallingConvention.Cdecl )]
        public static extern IntPtr p4_connect(string host, string port);

        [DllImport("cperforce",CallingConvention = CallingConvention.Cdecl )]
        public static extern void p4_close(IntPtr ctx);

        [DllImport("cperforce",CallingConvention = CallingConvention.Cdecl )]
        public static extern int p4_dropped(IntPtr ctx);




    }
}

