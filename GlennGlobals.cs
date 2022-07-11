/*
 * GlennGlobals.cs
 */

using System;
using System.Collections.Generic;

using GlennTeller;

namespace GlennGlobals
{
	/// <summary>
	/// teller!, config settings, SQL strings
	/// </summary>
	public static class Globals
	{
		
		// cross-cutting logging concerns
		
		public static Teller teller; // supplies say(), err().
		public static bool interactive = true; // tees console
		public static bool writeOutput = true; // else just err
		public static int prefixMsWidth = 0; // zero for don't	
		public const string outputFolder = @"C:\a";
		public const string appNamePart = "LinkedIn_Human_Entered_Text_Clustering_Heuristic";

		public static bool dropTables = true; // or false for quick debug

		public static bool outputDates = false; // not topic of this app.
		
		// Several apps loop over database row blocks.

		public const int selectBatchBlockLimit = 1000;
		public const int stopAfterBlockNumber = 10; // int.MaxValue; // or 1 for quick debug

		
		// Database was created in MySQL Workbench:
		
		public const string dbServer = "localhost";
		public const string dbPort = "3306";
		// before sharing, making user, pw non-const, main gets from environment...

			
		// A prior app (not in sight here)
		// once created dbDatabasePrecious.
		
		// Precious ingested candidate profile database:
		// NIS -- Not in service, to protect data:
		// public const string dbDatabasePrecious = "ingest_lip_to_mysql";

		
		// This is the everyday working database:
		
		public const string dbDatabaseWork = "ptr_work";


		// =======================

		
		// App PTRMySQLCreateWorkFromIngest:
		
		// creates, copies profile to Work db:

		// Work db gets same id as in ingest db.
		
		// NIS -- Not in service, to protect data:
		//public const string sqlDropWorkProfile =
		//	"DROP TABLE IF EXISTS " + dbDatabaseWork + ".profile;";
		
		// NIS -- Not in service, to protect data:
		//public const string sqlCreateWorkProfile =
		//	"CREATE TABLE IF NOT EXISTS " + dbDatabaseWork + ".profile (" +
		//	// Precious db id was AUTO_INCREMENT ASC
		//	// Work db id copies from precious db id.
		//	"id MEDIUMINT PRIMARY KEY, " +
		//	"top_block TEXT NOT NULL, " +
		//	"summary TEXT, " +
		//	"skills TEXT, " +
		//	"experience TEXT, " +
		//	"education TEXT, " +
		//	"certifications TEXT);";

		// NIS -- Not in service, to protect data:
		//public const string sqlFillWorkProfile =
		//	"INSERT INTO " + dbDatabaseWork + ".profile " +
		//	"SELECT " +
		//	"id, " +
		//	"top_block, " +
		//	"summary, " +
		//	"skills, " +
		//	"experience, " +
		//	"education, " +
		//	"certifications " +
		//	"FROM " + dbDatabasePrecious + ".profile " +
		//	"WHERE id > @lastid ORDER BY id ASC LIMIT @limit;";

		
		// =======================

		
		// Used in the copy app and the de-duplicate app:
		
		public const string sqlLastIdInWorkProfile =
			"SELECT MAX(id) FROM " + dbDatabaseWork + ".profile;";

		
		// =======================

		
		// App PTRMySQLDeduplicateWork:
		
		// deletes oldest work.profile
		// row if it is redundant in a
		// hash of three parts of row:
		// name + 1st job + 1st school.

		// This Block Read is used in more apps:
		
		public const string sqlReadProfile =
			"SELECT " +
			"id, " +
			"top_block, " +
			"summary, " +
			"skills, " +
			"experience, " +
			"education, " +
			"certifications " +
			"FROM " + dbDatabaseWork + ".profile " +
			"WHERE id > @lastid ORDER BY id ASC LIMIT @limit;";

		public const string sqlCreateTempUniquely =
			"CREATE TEMPORARY TABLE " + dbDatabaseWork + ".uniquely (" +
			"uniquehash BINARY(16) PRIMARY KEY, " +
			"profile_id MEDIUMINT UNIQUE NOT NULL, " +
			"ingestdate CHAR(10) NOT NULL);"; // like "2017-07-28"

		public const string sqlInsertIntoTempUniquely =
			"INSERT INTO " + dbDatabaseWork + ".uniquely (" +
			"uniquehash, " +
			"profile_id, " +
			"ingestdate " +
			") values (" +
			"@uniquehash, " +
			"@profile_id, " +
			"@ingestdate);";

		public const string sqlSelectByHashFromTempUniquely =
			"SELECT " +
			"profile_id, " +
			"ingestdate " +
			"FROM " + Globals.dbDatabaseWork + ".uniquely " +
			"WHERE uniquehash = @uniquehash;";


		public const string sqlUpdateByPidInTempUniquely =
			"UPDATE " + Globals.dbDatabaseWork + ".uniquely " +
			"SET " +
			"profile_id = @profile_id_keep, " +
			"ingestdate = @ingestdate_keep " +
			"WHERE profile_id = @profile_id_discard;";


		public const string sqlDeleteByPidInProfile =
			"DELETE " +
			"FROM " + Globals.dbDatabaseWork + ".profile " +
			"WHERE id = @profile_id_discard;";

		
		// =======================
		
		
		// App LinkedIn_Human_Entered_Text_Clustering_Heuristic:
		
		// parses all the rows from work.profile to
		// create new work.tables counting original
		// texts for industry, skills, titles, etc.

		// 1. reuses const sqlReadProfile from above.

		
		// Abandoned approach: counting strings in SQL, e.g.,
		// SET qty = qty + 1, ran hours vs seconds in memory.
		
		
		// TO DO: create vocabulary mapping tables
		// soon to be produced by this current app.
		

		
		// =======================

		
	}
}
