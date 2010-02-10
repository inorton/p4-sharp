
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
				env.EnvironmentVariables.Add("P4CLIENT",value);
			}
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
