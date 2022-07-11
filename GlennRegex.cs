/*
 * GlennRegex.cs
 * 
 * Favorite Regex for several PTRMySql Apps.
 */

using System;
using System.Text;
using System.Text.RegularExpressions;

namespace GlennRegex
{
	/// <summary>
	/// Glenn's Regular Expressions
	/// </summary>
	static class MyRegex
	{
		
		// First line of every profile is ingest prefix, thus:
		// Ingest Filename: Carlos Fuchen, CISSP, CISM, CISA, CCSK 2017-07-02.txt
		
		public static Regex reIngestPrefix = new Regex(
			@"^Ingest Filename: (?<nameEtc>.+) (?<dateStr>\d\d\d\d-\d\d-\d\d).txt", RegexOptions.Compiled);

		
		// Find and parse industry line:
		
		public static Regex reIndustryLine = new Regex(
			"^industry:(?<industry>.+)$", RegexOptions.Multiline | RegexOptions.Compiled);


		// Find and parse location line:
		
		public static Regex reLocationLine = new Regex(
			"^location:(?<location>.+)$", RegexOptions.Multiline | RegexOptions.Compiled);


		// Find and parse skills on multiple lines:
		// Starts with a literal '+', match has \r.
		
		public static Regex reSkillLines = new Regex(
			"^\\+ (?<skill>.+)$", RegexOptions.Multiline | RegexOptions.Compiled);

		
		// Convert all Unicode, and Latin1 favorably, to USASCII.
		
		// Usage: string s = nonUsAscii.Replace(s, replaceUnicodeEvaluator);
		
		public static Regex nonUsAscii = new Regex(@"[^ -~]", RegexOptions.Compiled);
		
		public static string replaceUnicodeEvaluator(Match match)
		{
			int n = (int)(match.Groups[0].Value[0]);
			if (n < 192 || n > 383)
				return " "; // or "X"; // or string.Empty;
			char c = "AAAAAAACEEEEIIIIENOOOOO?OUUUUYPsaaaaaaaceeeeiiiienooooo?ouuuuypyAaAaAaCcCcCcCcDdDdEeEeEeEeEeGgGgGgGgHhHhIiIiIiIiIiIiJjKkkLlLlLlLlLlNnNnNnnNnOoOoOoOoRrRrRrSsSsSsSsTtTtTtUuUuUuUuUuUuWwYyYZzZzZzs?"[n - 192];
			if (c == '?')
				return " "; // or "X"; // or string.Empty;
			return c.ToString();
		}

		
		// Split various profile text blocks at "", "+", "" line triplets.
		
		public static Regex rePlusLineTriplet = new Regex("\r?\n\r?\n\\+\r?\n\r?\n", RegexOptions.Compiled);


		// Split text at single newlines
		// Non-capture ?: group, lest delimiters also appear in Split array.
		
		public static Regex reSingleNewline = new Regex("(?:\r\n|\r|\n)", RegexOptions.Compiled);


		// Split text at multiple newlines
		// Non-capture ?: group, lest delimiters also appear in Split array.
		
		public static Regex reOneOrMoreNewlines = new Regex("(?:\r\n|\r|\n)+", RegexOptions.Compiled);

		
		// to grab non-empty top line of a text block
		
		public static Regex reTopLine = new Regex("^(?<atop>.+)$", RegexOptions.Multiline | RegexOptions.Compiled);

		
		// Incorporate hard-won wisdom from BBCandidateProfileProcessor.cs
		
		// Date Lines on each job item have varied formats. E.g.:
		//Present
		//August 2015 Present(2 years 6 months)
		//February 2000 February 2005(5 years)Scotts Valley, CA

		const string aMonth = "January|February|March|April|May|June|July|August|September|October|November|December";

		public static Regex reFromToLocn = new Regex(
			@"^" +
			@"(?<fromMonth>" + aMonth + @")? ?" +
			@"(?<fromYear>\d{4})? ?" +
			@"(?<toMonth>" + aMonth + @")? ?" +
			@"(?<toYear>\d{4}|Present)" + // require it - only absent in 60 of 120,000.
			@"(\(" +
			@"((?<nYears>\d+) years?)? ?" +
			@"((?<nMonths>\d+) months?)?" +
			@"\))?" +
			@"(?<location>.*)?" +
			@"$", RegexOptions.Compiled);

		
		// Simpler, just to know which edu/cert line is the date
		
		//May 2014
		//2013 2015
		//November 2013 November 2016

		public static Regex reAnyYears = new Regex(
			@"(^|\D)(?<year>(19|20|21)\d{2})($|\D)", RegexOptions.Compiled);

		
		// Now, as I start the task to cluster similar names...
		
		
		// Match to dedup/rid all remaining white-space or punctuation.
		// Hey, "[\s\p{P}]+" let a couple puncts remain: | and another.
		// With less subtlety, [^a-z0-9]+ can leave only alphanumerics.
		
		public static Regex reSpacePunctRun = new Regex(@"[^a-z0-9]+", RegexOptions.Compiled);

		
		// certain terms like C# must be kid-gloved first before I depunct:
		
		// usage: string s = reFavorites.Replace(s, replaceFavoritesEvaluator);
		// Oh, another prior concern: &amp -> &;
		// And another post concern: & -> and
		
		static string faves4re =
			@"(\bat&t\b"
			+ @"|\br&d\b|\br & d\b"
			+ @"|\bc#|\bf#|\bc\+\+"
			+ @"|\bc #|\bf #|\bc \+\+"
			+ @"|\bm&a\b|\bm & a\b"
			+ @"|\bp&l\b|\bp & l\b"
			+ @"|\bv&v\b|\bfp&a\b"
			+ @"|\.net\b|\btcp/ip\b"
			+ @"|\bos x\b|\bu\.s\.a\.|\bu\.s\."
			+ @"|\bbig 4\b|\bsr\."
			+ @")";

		
		// N.B. Only works on lowercase; Due to hard coded switch cases:
		
		public static Regex reFavorites = new Regex(faves4re, RegexOptions.IgnoreCase | RegexOptions.Compiled);

		// N.B. Only works on lowercase; Due to hard coded switch cases:

		public static string replaceFavoritesEvaluator(Match match)
		{
			switch(match.Groups[0].Value)
			{
				case "at&t":
					return "american telephone and telegraph";
				case "r&d":
				case "r & d":
					return "research and development";
					
				case "c#":
				case "c #":
					return "csharp";
				case "f#":
				case "f #":
					return "fsharp";
				case "c++":
				case "c ++":
					return "cplusplus";
					
				case "m&a":
				case "m & a":
					return "mergers and acquisitions";
				case "p&l":
				case "p & l":
					return "profit and loss";
				case "v&v":
					return "verification and validation";
				case "fp&a":
					return "financial planning and analysis";
					
				case ".net":
					return " dotnet"; // insert a space
				case "tcp/ip":
					return "tcpip";
				case "os x":
					return "osx";

				case "u.s.":
					return "united states ";
				case "u.s.a.":
					return "united states ";
				case "big 4":
					return "big4";
				case "sr.":
					return "senior";
			}
			return "";
		}
		
		// To ask if (USASCII) ALPHAs remain, not if empty:
		
		public static Regex reAlphas = new Regex("[a-z]", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		
		// To replace any purely numeric words in a key:
		// And also, all 1-letter words, save C and R.
		// And also, to finally lay aside "and" in keys!

		//Problem if matches overlap. Trick is:
		//(?=...) matches if ... matches next,
		//but doesn’t consume any of the string.
		//This is called a lookahead assertion.
		
		
		public static Regex reSlashNumericSlash = new Regex("/([0-9]+|and|[a-bd-qs-z])(?=/)", RegexOptions.Compiled);

		
	}
}
