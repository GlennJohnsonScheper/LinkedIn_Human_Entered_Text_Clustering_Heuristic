/*
 * LinkedIn_Human_Entered_Text_Clustering_Heuristic.cs
 * 
 * Process the existing, deduplicated profiles table
 * in the ptr_work database, to create new tables of
 * the several kinds of personal profile data fields
 * that I will soon MergeAndMatch, and use to search.
 *
 * E.g., As the prior App tabulated these few fields:
 * 
  543680: State: CA
  200035: State: NY
  124364: State: ZZ
  123144: CoLocn: 8175","1106","3367","NY","New York","
  121021: CoLocn: ","","","ZZ","","
  112467: CoLocn: 0805","0909","0170","CA","San Francisco","
  110983: skills: Management
   98474: skills: Leadership
   91983: CoLocn: San Francisco Bay Area
   73239: State:
   73195: State: TX
   72827: CoLocn: Greater New York City Area
   71549: CoLocn: ","","","","","
   70015: skills: Project Management
   64876: skills: Strategy
   63614: skills: Cloud Computing
   60547: Industry: Information Technology and Services
   60192: CoLocn: 1307","0575","0517","CA","San Diego","
   56912: skills: Microsoft Office
   54768: company-following: Google
   54651: location: 0805","0909","0170","CA","San Francisco","
   52711: skills: Program Management
   52007: State: AZ
   50208: Industry: Computer Software
   ...
    7242: groups: Cloud Computing, SaaS & Virtualization
   ...
    3063: company: IBM
   ...
     910: title: Software Engineer
 *
 * I am absolutely impressed with MySQL reading speed:
 * Reading all fields of all 333566 rows was 71377 ms.
 * However, the time to create 333K rows was 36 hours.
 *
 * Trying to count strings using MySQL SET qty=qty+1
 * ran 11.5 hours to only do the first 250K profiles.
 * 
 * Counting in dict<str,int> did 333566 in 117050 ms.
 *
 * 
 * first results:
 * 
There were 332153 counts in 150 lines for Original Industry
  64155 Information Technology and Services
  56694 Computer Software
  18508 Financial Services
  12620 Internet
   9460 Hospital & Health Care
   8764 Marketing and Advertising
   8369 Pharmaceuticals
   ...
 * 
There were 333566 counts in 6897 lines for Original Location
  52258 San Francisco Bay Area
  31801 Greater San Diego Area
  29946 Greater New York City Area
  15518 San Diego, California
  12543 Greater Los Angeles Area
  11455 New York, New York
   7476 San Francisco, California
   ...
 *
 * etc.
 * 
 * Next step is to cluster all these vocabularies....
 * 
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using MySql.Data.MySqlClient; // tools, pkg mgr, browse, install... MySql.Data

using GlennGlobals;
using GlennTeller;
using ProfileAnalyzer;
using GlennRegex;

namespace LinkedIn_Human_Entered_Text_Clustering_Heuristic
{
	class Program
	{

		// cross-cutting logging concerns

		static void say(string s)
		{
			Globals.teller.say(s);
		}
		
		static void err(string s)
		{
			Globals.teller.err(s);
		}

		
		// one thread; one connection; one static.
		
		static MySqlConnection sqlConn;

		// Any reusable global SQL cmds
		
		// ...
		
		
		// Profile parsing is an extensive topic.
		
		static Analyzer ProfileAnalyzer;
		
		
		public static void Main(string[] args)
		{
			if(Globals.interactive)
			{
				Console.WriteLine(Globals.appNamePart + " starting...");
			}
			
			using(Globals.teller = new Teller(Globals.outputFolder, Globals.appNamePart))
			{
				// modified before sharing to get user, pw from environment:
				string dbUsername = Environment.GetEnvironmentVariable("MYSQL_DB_USER");
				string dbPassword = Environment.GetEnvironmentVariable("MYSQL_DB_PASSWORD");
				{
					try
					{
						string connectionstring = string.Format(
							// Add ; database = {4} as this app only accesses one db:
							"Server = {0}; Port ={1}; Uid = {2}; Pwd = {3}; database = {4};",
							Globals.dbServer,
							Globals.dbPort,
							dbUsername,
							dbPassword,
							Globals.dbDatabaseWork
						);

						using(ProfileAnalyzer = new Analyzer())
							using(sqlConn = new MySqlConnection(connectionstring))
						{
							if(ReadyMySqlDatabase())
							{
								BlockProcessProfileTable();
							}
							ProfileAnalyzer.PostInputPhaseAnalysis();
						}
					}
					catch(Exception ex)
					{
						err("Exception122: " + ex.ToString());
					}
				}
			}
			
			if(Globals.interactive)
			{
				Console.WriteLine("Press any key to exit...");
				Console.ReadKey();
			}
		}

		
		static bool ReadyMySqlDatabase()
		{
			try
			{
				sqlConn.Open();
			}
			catch(MySqlException ex)
			{
				err("Exception143: " + ex.Message);
				return false;
			}
			
			if(sqlConn.State != ConnectionState.Open)
			{
				err("MySql database connection is not open.");
				return false;
			}

			
			if(Globals.dropTables)
			{
				// Drop & Re-create any tables this app will produce.
				
				// Industry
				
				
				// These tables for SQL SET qty = qty + 1 are obsolete.
				// But I will need them again to output completed data.
				
				
//				try
//				{
//					using(MySqlCommand cmd = new MySqlCommand(Globals.sqlDropWorkIndustry, sqlConn))
//					{
//						say("cmd [" + Globals.sqlDropWorkIndustry + "] returned " + cmd.ExecuteNonQuery());
//					}
//				}
//				catch(MySqlException ex)
//				{
//					err("Exception167: " + ex.Message);
//					return false;
//				}
//
//				try
//				{
//					using(MySqlCommand cmd = new MySqlCommand(Globals.sqlCreateWorkIndustry, sqlConn))
//					{
//						say("cmd [" + Globals.sqlCreateWorkIndustry + "] returned " + cmd.ExecuteNonQuery());
//					}
//				}
//				catch(MySqlException ex)
//				{
//					err("Exception180: " + ex.Message);
//					return false;
//				}

				// Skill
				// Singular table name skill, despite input says "skills".
				
//				try
//				{
//					using(MySqlCommand cmd = new MySqlCommand(Globals.sqlDropWorkSkill, sqlConn))
//					{
//						say("cmd [" + Globals.sqlDropWorkSkill + "] returned " + cmd.ExecuteNonQuery());
//					}
//				}
//				catch(MySqlException ex)
//				{
//					err("Exception196: " + ex.Message);
//					return false;
//				}
//
//				try
//				{
//					using(MySqlCommand cmd = new MySqlCommand(Globals.sqlCreateWorkSkill, sqlConn))
//					{
//						say("cmd [" + Globals.sqlCreateWorkSkill + "] returned " + cmd.ExecuteNonQuery());
//					}
//				}
//				catch(MySqlException ex)
//				{
//					err("Exception209: " + ex.Message);
//					return false;
//				}

				// Title
				
//				try
//				{
//					using(MySqlCommand cmd = new MySqlCommand(Globals.sqlDropWorkTitle, sqlConn))
//					{
//						say("cmd [" + Globals.sqlDropWorkTitle + "] returned " + cmd.ExecuteNonQuery());
//					}
//				}
//				catch(MySqlException ex)
//				{
//					err("Exception224: " + ex.Message);
//					return false;
//				}
//
//				try
//				{
//					using(MySqlCommand cmd = new MySqlCommand(Globals.sqlCreateWorkTitle, sqlConn))
//					{
//						say("cmd [" + Globals.sqlCreateWorkTitle + "] returned " + cmd.ExecuteNonQuery());
//					}
//				}
//				catch(MySqlException ex)
//				{
//					err("Exception237: " + ex.Message);
//					return false;
//				}

			}
			
			// instantiate any other reusable MySqlCommand:
			// I will add *.Parameters["@..."].Value later.

			
			return true;
		}

		
		static void BlockProcessProfileTable()
		{
			int limit = Globals.selectBatchBlockLimit;
			int lastId = 0; // last id that was read

			int totalRowsRead = 0;
			Stopwatch sw = new Stopwatch();
			sw.Start();
			
			using(MySqlCommand readCmd = new MySqlCommand(Globals.sqlReadProfile, sqlConn))
			{
				readCmd.Parameters.Add(new MySqlParameter("@limit", MySqlDbType.Int24));
				readCmd.Parameters["@limit"].Value = limit;
				readCmd.Parameters.Add(new MySqlParameter("@lastid", MySqlDbType.Int24));

				for(int i = 0; i < Globals.stopAfterBlockNumber; i++)
				{
					int rowsRead = 0;
					readCmd.Parameters["@lastid"].Value = lastId;

					// Exception: There is already an open DataReader associated with this Connection which must be closed first.
					// So just save the avg 10Kb rows to a list, and do other database work below the loop iterating reader rows.

					
					// Using a Queue: totalRowsRead = 333566 in 79,254 ms
					// Queue<object[]> resultRows = new Queue<object[]>(limit);
					//
					// Using a List: totalRowsRead = 333566 in 76,458 ms
					// List<object[]> resultRows = new List<object[]>(limit);
					//
					// Using an Array: totalRowsRead = 333566 in 75,170 ms
					// object[][] resultRows = new object[limit][];
					//
					// Using Read only: totalRowsRead = 333566 in 71,377 ms
					//
					// Adding All Analysis & Array did 333566 in 563,426 ms.
					
					
					object[][] resultRows = new object[limit][];
					
					using(MySqlDataReader reader = readCmd.ExecuteReader())
					{
						while(reader.Read())
						{
							object [] fields = new object[reader.FieldCount];
							reader.GetValues(fields);
							lastId = (int)fields[(int)iFields.id]; // last ACTUAL record id processed

							// show me:
							//Match m = MyRegex.reIngestPrefix.Match((string)fields[(int)iFields.topBlock]);
							//if(m.Success)
							//{
							//	say(m.Groups["nameEtc"].Value);
							//}
							
							// Queue: resultRows.Enqueue(fields);
							// List: resultRows.Add(fields);
							// Array: resultRows[rowsRead] = fields;
							resultRows[rowsRead] = fields;
							rowsRead++;
						}
					}

					
					// Queue:
					// while(resultRows.Count > 0)
					// .... Queue:object [] fields = resultRows.Dequeue();
					
					// List:
					// foreach(object [] fields in resultRows)
					
					// Array:
					// for(int iRow = 0; iRow < rowsRead; iRow++)
					// ... Array: object [] fields = resultRows[iRow];

					
					for(int iRow = 0; iRow < rowsRead; iRow++)
					{
						object [] fields = resultRows[iRow];

						// show me again:
						//Match m = MyRegex.reIngestPrefix.Match((string)fields[(int)iFields.topBlock]);
						//if(m.Success)
						//{
						//	say(m.Groups["nameEtc"].Value);
						//}
						
						// LinkedIn Profile parsing is an extensive topic.
						// Parse and count into many Global dict<str,int>:
						ProfileAnalyzer.ProcessProfileTableRow(fields);
					}
					
					// show progress
					// say("Block[" + i + "] processed " + rowsRead + ", lastId = " + lastId);

					totalRowsRead += rowsRead;
					
					if(rowsRead < limit)
						break;
				}
				// show speed
				say("totalRowsRead = " + totalRowsRead + " in " + sw.ElapsedMilliseconds + " ms");
			}
		}
	}
}