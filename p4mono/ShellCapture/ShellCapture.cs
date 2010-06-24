
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;

namespace ShellCapture
{


	public class ShellEnvironment
	{
		public string WorkingDirectory;
		public StringDictionary EnvironmentVariables;
				 
		public ShellEnvironment() : this( Path.GetFullPath( System.IO.Directory.GetCurrentDirectory() ), true ) 
		{
		}
		
		public ShellEnvironment( string workingDirectory, bool useEnvironmentVariables ){
			WorkingDirectory = workingDirectory;
			EnvironmentVariables = new StringDictionary();
			if ( useEnvironmentVariables ){
				foreach ( string k in System.Environment.GetEnvironmentVariables().Keys ){
					EnvironmentVariables[k] = System.Environment.GetEnvironmentVariable( k );
				}
			}
		}
		
		public void Chdir( string cdto ){
			if ( Directory.Exists( cdto ) )
			    WorkingDirectory = Path.GetFullPath( cdto );
			throw new DirectoryNotFoundException( cdto );
		}
		
		public void ExecuteThrow( string command, string[] args, out string stdout )
		{
			string stderr = String.Empty;
			int rv = Execute( command, args, out stdout, out stderr );
			if ( rv != 0 ){
				if ( stderr.Equals( String.Empty ) ){
					stderr = String.Format("exited with non-zero status ({0})",rv );
				}
				throw new ApplicationException( stderr );
			}
		}

		public int Execute( string command, string[] args )
		{
			string ignore;
			return Execute( command, args, out ignore, out ignore );
		}
		
		public Process Expect( string command, string[] args, string commandSeq, List<string[]> script )
		{
			Process p = Start( command, args );
			
			foreach ( string[] expect_send in script ){
				string readstdout = String.Empty;
				
				if ( expect_send[0].StartsWith(commandSeq) ){
					string[] xcommand = Regex.Split( expect_send[1], commandSeq );
					int sleep = 10;
					switch( xcommand[0] ){
						case "wait":
						if ( xcommand.Length == 2 )
							sleep = int.Parse( xcommand[1] );
						goto default;
						
						case "write":
						if ( xcommand.Length == 2 )
							p.StandardInput.Write( xcommand[1] );
						break;
						
						default:
						System.Threading.Thread.Sleep( sleep );
						break;
					}
				} else {
					Console.Error.WriteLine("Waiting for {0}", expect_send[0] );	
					while ( Regex.Match( readstdout, expect_send[0] ).Success == false ){
						Console.WriteLine("waiting...");
						System.Threading.Thread.Sleep( 10 );
						readstdout += p.StandardOutput.ReadLine();
					}
					/* got match */
					readstdout = string.Empty;
					p.StandardInput.Write( expect_send[1] );
				}
			}
			
			return p;
			
			
			
		}
	
		public int Execute( string command, string[] args, out string stdout, out string stderr )
		{
			Process p = Start( command, args );
			p.WaitForExit();

			stdout = p.StandardOutput.ReadToEnd();
			stderr = p.StandardError.ReadToEnd();
			
			return p.ExitCode;
			
		}
		
		private Process Start( string command, string[] args )
		{
			Process p = new Process();
			p.StartInfo.FileName = command;
			if ( args != null )
				if ( args.Length > 0 )
					p.StartInfo.Arguments = String.Join(" ", args );
					
			p.StartInfo.EnvironmentVariables.Clear();
			foreach ( string k in EnvironmentVariables.Keys ){
				//Console.Error.WriteLine("{0}={1}", k, EnvironmentVariables[k] );
				p.StartInfo.EnvironmentVariables.Add( k.ToUpper(), EnvironmentVariables[k] );
			}
			
			
			p.StartInfo.WorkingDirectory = WorkingDirectory;
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardError = true;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.RedirectStandardInput = true;
			p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			
			p.Start();
			return p;
		}

	}
}
