using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace cperforcesharp
{

    public class UnsafeNativeMethods
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void p4_get_stat_string_cb( string x );

        [DllImport("cperforce",CallingConvention = CallingConvention.Cdecl )]
        public static extern IntPtr p4_connect(string p4port);

        [DllImport("cperforce",CallingConvention = CallingConvention.Cdecl )]
        public static extern void p4_close(IntPtr ctx);

        [DllImport("cperforce",CallingConvention = CallingConvention.Cdecl )]
        public static extern int p4_dropped(IntPtr ctx);

        [DllImport("cperforce",CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, SetLastError = true )]
        public static extern int p4_get_infobuf( IntPtr ctx, [Out] StringBuilder buf, uint buf_len );

        [DllImport("cperforce",CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.None )]
        public static extern int p4_get_textbuf( IntPtr ctx, StringBuilder buf, int buf_len );

        [DllImport("cperforce",CallingConvention = CallingConvention.Cdecl )]
        public static extern int p4_get_binarybuf( IntPtr ctx, byte[] buf, int buf_len );

        [DllImport("cperforce",CallingConvention = CallingConvention.Cdecl )]
        public static extern int p4_login( IntPtr ctx, string username, string password );

        [DllImport("cperforce",CallingConvention = CallingConvention.Cdecl )]
        public static extern int p4_run( IntPtr ctx, int argc, string[] argv, string input );

        [DllImport("cperforce",CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, SetLastError = true )]
        static extern int p4_get_stat_keys( IntPtr ctx, p4_get_stat_string_cb cb );



        public static List<string> p4_stat_Keys(IntPtr ctx)
        {
            List<string> keys = new List<string>();

            p4_get_stat_string_cb cb = delegate (string x) {
                keys.Add(x);
            };

            int count = p4_get_stat_keys(ctx, cb);
            if ( count == 0 ) keys = null;

            return keys;
        }

        [DllImport("cperforce",CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, SetLastError = true )]
        static extern int p4_get_stat_value( IntPtr ctx, string key, p4_get_stat_string_cb cb );

        public static string p4_stat_Value(IntPtr ctx, string key )
        {
            string rv = null;
            p4_get_stat_string_cb cb = delegate (string x) {
                rv = x;
            };
            p4_get_stat_value( ctx, key, cb );
            return rv;
        }
    }
}