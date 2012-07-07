using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace cperforcesharp
{
    public class UnsafeNativeMethods
    {
        [DllImport("cperforce",CallingConvention = CallingConvention.Cdecl )]
        public static extern IntPtr p4_connect(string p4port);

        [DllImport("cperforce",CallingConvention = CallingConvention.Cdecl )]
        public static extern void p4_close(IntPtr ctx);

        [DllImport("cperforce",CallingConvention = CallingConvention.Cdecl )]
        public static extern int p4_dropped(IntPtr ctx);

        [DllImport("cperforce",CallingConvention = CallingConvention.Cdecl )]
        public static extern int p4_get_infobuf( IntPtr ctx, StringBuilder buf, int buf_len );

        [DllImport("cperforce",CallingConvention = CallingConvention.Cdecl )]
        public static extern int p4_get_binarybuf( IntPtr ctx, byte[] buf, int buf_len );

        [DllImport("cperforce",CallingConvention = CallingConvention.Cdecl )]
        public static extern int p4_login( IntPtr ctx, string username, string password );

        [DllImport("cperforce",CallingConvention = CallingConvention.Cdecl )]
        public static extern int p4_run( IntPtr ctx, int argc, string[] argv );
    }
}

