using System;
using NUnit.Framework;

using cperforcesharp;

namespace cperforcesharptests
{
    [TestFixture()]
    public class Test
    {
        [Test()]
        public void Connect()
        {

            var repo = new PerforceAPI();

            Console.Error.WriteLine("Connecting..");

            repo.Connect("perforce.ncipher.com", 1666);

            //Console.Error.WriteLine("Login..");

            //repo.Login("inb", Environment.GetEnvironmentVariable("P4PASSWD"));


            Console.Error.WriteLine(">> info..");

            var info = repo.DirectRun("info");

            foreach (var k in info.Keys)
            {
                Console.Error.WriteLine("##{0}:{1}", k.ToString(), info[k] ); 
            }


            Console.Error.WriteLine(">> where..");

            var p4where = repo.DirectRun("where","//nCipher/dev/comp/cutils/trunk/SConscript");
            foreach (var k in p4where.Keys)
            {
                Console.Error.WriteLine("##{0}:{1}", k.ToString(), p4where[k] ); 
            }

        }
    }
}

