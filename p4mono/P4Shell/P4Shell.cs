
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ShellCapture;
using System.Diagnostics;

namespace P4
{


	public class P4Shell
	{
		private string server = "perforce";
		private int port = 1666;
		private string username = Environment.GetEnvironmentVariable("USER");
		private string password = String.Empty;
		private bool loggedIn = false;
		
		internal ShellEnvironment env;
		
		private Dictionary<string,string[]> depotPathCache = new Dictionary<string, string[]>();
		private Dictionary<string,string[]> depotPathFileCache = new Dictionary<string, string[]>();
		
		/// <summary>
		/// Port number of the perforce server
		/// </summary>
		public int Port {
			get { return port; }
			set { port = value; }
		}
		
		/// <summary>
		/// Hostname of the perforce server
		/// </summary>
		public string Server {
			get { return server; }
			set { server = value; }
		}
		
		/// <summary>
		/// Perforce Username
		/// </summary>
		public string Username {
			get { return username; }
			set { username = value; }
		}
		
		/// <summary>
		/// Perforce Password
		/// </summary>
		public string Password {
			get { return password; }
			set { password = value; }
		}

		public P4Shell ()
		{
			env = new ShellEnvironment();
		}
		
		
		/// <summary>
		/// The full path to the root directory of the current workspace
		/// </summary>
		public string WorkspaceRoot {
			get {
				string wsr = String.Empty;
				if ( !loggedIn )
					Login(username, password);
				wsr = Info()["Client root"];			
				return wsr;					
			}
		}
		
		/// <summary>
		/// The name of the current workspace
		/// </summary>
		public string WorkspaceName {
			get {
			string wsn = String.Empty;
				if ( !loggedIn )
					Login(username, password);
				wsn = Info()["Client name"];			
				return wsn;					
			}
			
			set {
				env.EnvironmentVariables["P4CLIENT"] = value;
			}
		}
		
		public string[] GetWorkspaces(string username)
		{
			string stdout = String.Empty;
			string stderr = String.Empty;
			List<string> clients = new List<string>();
			if ( 0 == env.Execute( "p4", new string[]{ "clients","-u",username }, out stdout, out stderr ) ){
				
				string [] lines = stdout.Split( new string[]{"\n"}, StringSplitOptions.RemoveEmptyEntries );
				foreach ( string line in lines ){
					string[] ws = line.Split( new string[]{" "}, 3, StringSplitOptions.RemoveEmptyEntries );
					clients.Add( ws[1] );
				}
				
			}
			return clients.ToArray();
		}
		
		
		
		public bool Login( string username, string password )
		{
			env.EnvironmentVariables["P4USER"] = username;
			env.EnvironmentVariables["P4PASSWD"] = password;

			Process p = env.Expect( "p4", new string[]{ "login" } , "##", 
			                       new List<string[]>(){
				                     new string[] { "##","wait##20" },
				                     new string[] { "##", String.Format("write##{0}\n", password) },
			                       } );
			// p.StandardInput.WriteLine( password );
			p.WaitForExit();
			Console.WriteLine( p.StandardOutput.ReadToEnd() );
			
			string stderr = String.Empty;
			string stdout = String.Empty;
			
			int rv = env.Execute("p4",new string[]{ "depots" }, out stdout, out stderr );
			if ( rv == 0 ){
				loggedIn = true;
				return true;
			}
			
			throw new AccessViolationException(stderr);	
			
		}
		
		
		public Dictionary<string,string> Info(){
			string stdout = String.Empty;
			string [] info;
			Dictionary<string, string> ret = new Dictionary<string, string>();
			if ( !loggedIn )
				Login(username, password);
				
			env.ExecuteThrow( "p4", new string[] { "info" }, out stdout );
			info = Regex.Split( stdout, "[\r\n]+" );
			foreach ( string s in info ){
				string[] line = Regex.Split(s, ":\\s");
				try {
					ret.Add( line[0], line[1] );
				} catch ( Exception e ){ e.GetHashCode(); }
			}
			return ret;
		}
		
		public string[] Depots()
		{
			string stdout = String.Empty;
			if ( !loggedIn )
				Login( username, password );
				
			env.ExecuteThrow( "p4", new string[] { "depots" }, out stdout );
			
			string[] lines = Regex.Split( stdout, "[\r\n]+" );	
			List<string> depots = new List<string>();
			
			foreach ( string line in lines ){
				string x = Regex.Replace( line, "^Depot[\\s]+","" );
				string depot = Regex.Split( x, "\\s" )[0];
				depots.Add(depot);
			}
			
			return depots.ToArray();
			
		}
		
		public string[] Dirs( string wildcard ){
			string stdout = String.Empty;
			if ( !loggedIn )
				Login(username, password);
			
			if ( wildcard.EndsWith("...") ){
				wildcard = wildcard.Replace("...","*");
			}	
			
			List<string> dirs = new List<string>();
			string[] cached = null;

			if ( !depotPathCache.TryGetValue(wildcard, out cached) ){
				env.ExecuteThrow( "p4", new string[] { "dirs",wildcard }, out stdout );
				foreach ( string line in Regex.Split( stdout, "[\r\n]+" ) ){
					string[] path = Regex.Split(line,"/");
					dirs.Add( path[path.Length-1] );
				}
				
				depotPathCache[wildcard] = dirs.ToArray();
				
			} else {
				Console.WriteLine("cache hit");
				dirs = new List<string>( cached );
			}
			return dirs.ToArray();
		}
		
		public string[] Files( string wildcard ){
								
			string stdout = String.Empty;
			if ( !loggedIn )
				Login(username, password);
			
			if ( wildcard.EndsWith("...") ){
				wildcard = wildcard.Replace("...","*");
			}
			if ( wildcard.Length < 4 )
				return new string[]{};
			
			List<string> files = new List<string>();
			string[] cached = null;
			
			if ( !depotPathFileCache.TryGetValue(wildcard, out cached) ){
				try {			
					env.ExecuteThrow( "p4", new string[] { "files",wildcard }, out stdout );

					if ( !stdout.Contains("no such file") ){
						foreach ( string line in Regex.Split( stdout, "[\r\n]+" ) ){
							string[] path = Regex.Split(line,"/");
							string file = path[path.Length-1];
							string[] fname = Regex.Split(file, "#");
							files.Add( fname[0] );
						}
						depotPathFileCache[wildcard] = files.ToArray();
					}
				} catch ( ApplicationException e ){
					Console.Error.WriteLine( e.Message );
				}
			} else {
				files = new List<string>( cached );
			}
			
			
			return files.ToArray();
		}

        public bool CopyFile(string localPath, string depotPath)
        {
            var result = false;
            var stdout = String.Empty;
            if (!loggedIn)
                Login(username, password);
            try
            {
                env.ExecuteThrow("p4", new string[] { "print", "-o", localPath, depotPath }, out stdout);
                result = true;
            }
            catch (ApplicationException e)
            {
                Console.Error.WriteLine(e.Message);
            }
            return result;
        }

		public string[] Edit( string wildcard ){
			string stdout = String.Empty;
			if ( !loggedIn )
				Login(username, password);
				
			env.ExecuteThrow( "p4", new string[] { "edit",wildcard }, out stdout );
			return Regex.Split( stdout, "[\r\n]+" );
		}
		
		public string[] Add( string wildcard ){
			string stdout = String.Empty;
			if ( !loggedIn )
				Login(username, password);
				
			env.ExecuteThrow( "p4", new string[] { "add",wildcard }, out stdout );
			return Regex.Split( stdout, "[\r\n]+" );
		}
		
		public string[] Submit( string wildcard , string commitMessage){
			string stdout = String.Empty;
			if ( !loggedIn )
				Login(username, password);
			if ( commitMessage.Equals( String.Empty ) )
				throw new InvalidOperationException("Commit messages must contain text");
				
			env.ExecuteThrow( "p4", new string[] { "submit",wildcard,"-d",commitMessage }, out stdout );
			return Regex.Split( stdout, "[\r\n]+" );
		}
		
		public string[] Revert( string wildcard ){
			string stdout = String.Empty;
			if ( !loggedIn )
				Login(username, password);
				
			env.ExecuteThrow( "p4", new string[] { "revert",wildcard }, out stdout );
			return Regex.Split( stdout, "[\r\n]+" );
		}
		
		public string[] RevertUnchanged( string wildcard ){
			string stdout = String.Empty;
			if ( !loggedIn )
				Login(username, password);
				
			env.ExecuteThrow( "p4", new string[] { "revert","-a",wildcard }, out stdout );
			return Regex.Split( stdout, "[\r\n]+" );
		}
		
		public string[] ClientInfo(){
			string stdout = String.Empty;
			if ( !loggedIn )
				Login(username, password);
				
			env.ExecuteThrow( "p4", new string[] { "client","-o" }, out stdout );
			return Regex.Split( stdout, "[\r\n]+" );
		}
		
		public Dictionary<string, string> FStat( string file )
		{
			Dictionary<string,string> ret = new Dictionary<string, string>();
			string stdout = String.Empty;
			if ( !loggedIn )
				Login(username, password);
				
			env.ExecuteThrow( "p4", new string[] { "fstat",file }, out stdout );
			
			string[] lines = Regex.Split( stdout, "[\r\n]+" );
			foreach ( string line in lines ){
				string[] row = Regex.Split( line, " " );
				string key = row[1];
				string value = String.Empty;
				int x = 2;
				while ( x < row.Length ){
					value += row[x++];
				}
				ret.Add( key, value );
			}
			return ret;
		}
		
		public string LocalToDepot( string localpath )
		{			
			return String.Empty;
		}
	}
}
