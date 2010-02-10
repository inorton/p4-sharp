using System;
using System.Diagnostics;
using ShellCapture;
using P4;

namespace p4test
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			
			P4Shell p = new P4Shell();
			
			if ( p.Login("inb","password") )
				Console.WriteLine("logged in");
		
			string [] dirs = p.Dirs("//nCipher/dev/home/inb/*");
			
			Console.WriteLine( "-\n{0}\n", String.Join("\n+",dirs)  );
		
			Console.WriteLine( p.WorkspaceName );
			Console.WriteLine( p.WorkspaceRoot );
		
		}
	}
}
