using System;

using cperforcesharp;

namespace cperforcetool
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var p4 = new PerforceAPI();

            p4.Connect("perforce.ncipher.com", 1666);

            if (args.Length > 0)
            {
                var stat = p4.DirectRun(args);
                foreach ( var k in stat.Keys )
                {
                    Console.WriteLine("{0}:{1}",k,stat[k]);
                }
            }
        }
    }
}
