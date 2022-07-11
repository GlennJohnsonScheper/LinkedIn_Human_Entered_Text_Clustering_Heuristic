/*
 * GlennTeller.cs
 */

using System;
using System.Diagnostics;
using System.IO;

using GlennGlobals;

namespace GlennTeller
{
	/// <summary>
	/// Supplies cross-cutting logging concerns: say(), err();
	/// </summary>
	
	public class Teller : IDisposable
	{
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing) {
				if(swOut != null)
				{
					swOut.Close();
				}
				if(swErr != null)
				{
					swErr.Close();
				}
			}
		}

		static string outputFolder = "C:\a";
		static string appNamePart = "Teller";
		static string outFilePath = "";
		static string errFilePath = "";
		
		static StreamWriter swOut;
		static StreamWriter swErr;
		
		static object ioLock = new object();
		
		static Stopwatch sw;
		
		public Teller(string outputFolder, string appNamePart)
		{
			Teller.outputFolder = outputFolder;
			Teller.appNamePart = appNamePart;
			
			if(Directory.Exists(outputFolder) == false)
				Directory.CreateDirectory(outputFolder);

			sw = new Stopwatch();
			DateTime dtWas = DateTime.Now;
			DateTime dt;
			do
			{
				dt = DateTime.Now;
			} while(dt == dtWas);
			sw.Start(); // at top of new second
			
			string dateTimePart = dt.ToString("yyyy-MM-dd_HH.mm.ss");
			outFilePath = Path.Combine(outputFolder, appNamePart + "_" + dateTimePart + "_out.txt");
			errFilePath = Path.Combine(outputFolder, appNamePart + "_" + dateTimePart + "_error.txt");
			
			if(Globals.writeOutput)
			{
				lock(ioLock)
				{
					swOut = File.CreateText(outFilePath);
					swOut.WriteLine(outFilePath);
				}
			}
		}
		
		public void say(string msg)
		{
			string prefix = "";
			if(Globals.prefixMsWidth > 0)
			 	prefix = sw.ElapsedMilliseconds.ToString().PadLeft(Globals.prefixMsWidth) + " ";
			if(Globals.writeOutput)
			{
				lock(ioLock)
				{
					swOut.WriteLine(prefix + msg);
				}
			}
			if(Globals.interactive)
			{
				Console.WriteLine(prefix + msg);
			}
		}
		
		public void err(string msg)
		{
			string prefix = "Error: ";
			if(Globals.prefixMsWidth > 0)
			 	prefix = sw.ElapsedMilliseconds.ToString().PadLeft(Globals.prefixMsWidth) + " Error: ";
			lock(ioLock)
			{
				if(swErr == null)
				{
					swErr = File.CreateText(errFilePath);
					swErr.WriteLine(errFilePath);
				}
				swErr.WriteLine(prefix + msg);
				if(Globals.writeOutput)
				{
					swOut.WriteLine(prefix + msg);
				}
			}
			if(Globals.interactive)
			{
				Console.WriteLine(prefix + msg);
			}
		}
	}
}
