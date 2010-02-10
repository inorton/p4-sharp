
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
		
		private ShellEnvironment env;
		
		public int Port {
			get { return port; }
			set { port = value; }
		}
		public string Server {
			get { return server; }
			set { server = value; }
		}
		public string Username {
			get { return username; }
			set { username = value; }
		}
		public string Password {
			get { return password; }
			set { password = value; }
		}

		public P4Shell ()
		{
			env = new ShellEnvironment();
		}
		
		public string GetWorkspaceRoot() {
			string wsr = String.Empty;
			if ( !loggedIn )
				Login(username, password);
			
			wsr = Info()["Client root"];		
					
			return wsr;				
			
		}
		
		public bool Login( string username, string password )
		{
			env.EnvironmentVariables.Add("P4USER",username);
			env.EnvironmentVariables.Add("P4PASSWD",password);
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
		
		public string[] Dirs( string wildcard ){
			string stdout = String.Empty;
			if ( !loggedIn )
				Login(username, password);
				
			env.ExecuteThrow( "p4", new string[] { "dirs",wildcard }, out stdout );
			return Regex.Split( stdout, "[\r\n]+" );
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
	}
}
