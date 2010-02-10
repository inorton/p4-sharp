
using System;
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
	
		public int Execute( string command, string[] args, out string stdout, out string stderr )
		{
			Process p = new Process();
			p.StartInfo.FileName = command;
			if ( args != null )
				if ( args.Length > 0 )
					p.StartInfo.Arguments = String.Join(" ", args );
					
			p.StartInfo.EnvironmentVariables.Clear();
			foreach ( string k in EnvironmentVariables.Keys ){
				p.StartInfo.EnvironmentVariables.Add( k.ToUpper(), EnvironmentVariables[k] );
			}
			
			
			p.StartInfo.WorkingDirectory = WorkingDirectory;
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardError = true;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			
			p.Start();
			p.WaitForExit();

			stdout = p.StandardOutput.ReadToEnd();

			stderr = p.StandardError.ReadToEnd();
			
			return p.ExitCode;
			
		}

	}
}
