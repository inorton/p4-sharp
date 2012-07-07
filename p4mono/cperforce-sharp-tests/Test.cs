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

            //Console.Error.WriteLine("Connecting..");

            repo.Connect("perforce.ncipher.com",1666);

            //Console.Error.WriteLine("Login..");

            repo.Login("inb", Environment.GetEnvironmentVariable("P4PASSWD"));


            //Console.Error.WriteLine("info..");

            repo.DirectRun("info");

            //Console.Error.WriteLine( repo.GetInfoMessages() );
        }
    }
}

