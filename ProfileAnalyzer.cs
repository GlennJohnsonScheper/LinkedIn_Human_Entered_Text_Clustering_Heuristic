/*
 * ProfileAnalyzer.cs
 *
 *
 * Metrics after the first phase of Original text analysis:
 * 
 * totalRowsRead = 333566 in 563426 ms
 * There were 332157 counts in 151 lines for Original Industry
 * There were 333566 counts in 6897 lines for Original Location
 * There were 9404790 counts in 199779 lines for Original Skill
 * There were 2199547 counts in 895023 lines for Original Title
 * There were 2179023 counts in 727883 lines for Original Company
 * There were 712612 counts in 107630 lines for Original School
 * There were 783497 counts in 398439 lines for Original Degree
 * There were 384752 counts in 141678 lines for Original Certificate
 * There were 372066 counts in 135132 lines for Original License
 * 
 * 
 * TODO: next phase will be to cluster similar items together.
 * E.g., like this item from my prior MergeAndMatchVocabulary:
   2793 ===== /founder/and/ceo/ =====
   1493 Founder & CEO
   1260 Founder and CEO
     11 Founder And Ceo
     10 Founder & Ceo
      4 FOUNDER AND CEO
      4 FOUNDER & CEO
      3 Founder&CEO
      2 Founder and CEO.
      2 Founder And CEO
      1 Founder, and CEO
      1 FOUNDER and CEO
      1 Founder and Ceo
      1 founder & ceo * * 
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

using MySql.Data.MySqlClient; // use .Net 4.5.2, ref MySql.Data

using GlennGlobals;
using GlennTeller;
using GlennRegex;
using ClusterVocabulary;

namespace ProfileAnalyzer
{
	
	/// <summary>
	/// To index fields of profile table row passed to analyzer
	/// </summary>
	enum iFields {
		id = 0,
		topBlock,
		summary,
		skills,
		experience,
		education,
		certifications,
		nFields
	}
	
	/// <summary>
	/// Create one Analyer above the loop doing all profile rows.
	/// </summary>
	public class Analyzer : IDisposable
	{
		
		// Many dict<str,int> to count the frequencies of text fields.
		
		Dictionary<string,int> OriginalIndustryCounts = new Dictionary<string,int>();
		
		Dictionary<string,int> OriginalLocationCounts = new Dictionary<string,int>();
		
		Dictionary<string,int> OriginalSkillCounts = new Dictionary<string,int>();
		
		Dictionary<string,int> OriginalTitleCounts = new Dictionary<string,int>();
		
		Dictionary<string,int> OriginalCompanyCounts = new Dictionary<string,int>();
		
		Dictionary<string,int> OriginalDateEtcCounts = new Dictionary<string,int>();
		
		Dictionary<string,int> OriginalJobDescCounts = new Dictionary<string,int>();
		
		Dictionary<string,int> OriginalSchoolCounts = new Dictionary<string,int>();
		
		Dictionary<string,int> OriginalDegreeCounts = new Dictionary<string,int>();

		Dictionary<string,int> OriginalEduYearsCounts = new Dictionary<string,int>();
		
		Dictionary<string,int> OriginalCertificateCounts = new Dictionary<string,int>();
		
		Dictionary<string,int> OriginalCertLicenseCounts = new Dictionary<string,int>();

		Dictionary<string,int> OriginalCertYearsCounts = new Dictionary<string,int>();

		
		static char[] caSpace = { ' ' }; // for string.Split

		
		public Analyzer()
		{
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing) {
			}
		}


		// cross-cutting logging concerns

		void say(string s)
		{
			Globals.teller.say(s);
		}
		
		void err(string s)
		{
			Globals.teller.err(s);
		}
		

		
		/// <summary>
		/// Operate on the passed fields of one profile row.
		/// </summary>
		/// 
		/// <param name="fields">Array of objects in a Database Row:
		/// id
		/// top_block
		/// summary
		/// skills
		/// experience
		/// education
		/// certifications
		/// </param>
		public void ProcessProfileTableRow(object [] fields)
		{
			
			// ==== top block ====
			
			// The person name is in top block.
			// It's of no interest in this App.
			
			// show me:
			//Match m = MyRegex.reIngestPrefix.Match((string)fields[(int)iFields.topBlock]);
			//if(m.Success)
			//{
			//	say(m.Groups["nameEtc"].Value);
			//}
			
			
			// There is 0 or 1 location line per profile:
			//location: Greater San Diego Area
			
			// There is 0 or 1 industry line per profile:
			//industry: Information Technology and Services
			
			// Both industry & location lines are in top block.

			if(! (fields[(int)iFields.topBlock] is System.DBNull))
			{
				string topBlock = (string)fields[(int)iFields.topBlock];

				// 1. industry
				{
					Match m = MyRegex.reIndustryLine.Match(topBlock);
					if(m.Success)
					{
						string industry = m.Groups["industry"].Value.Trim();
						
						// DIY
						if(OriginalIndustryCounts.ContainsKey(industry))
							OriginalIndustryCounts[industry]++;
						else
							OriginalIndustryCounts.Add(industry, 1);
					}
				}

				// 2. location
				{
					Match m = MyRegex.reLocationLine.Match(topBlock);
					if(m.Success)
					{
						string location = m.Groups["location"].Value.Trim();
						
						// DIY
						if(OriginalLocationCounts.ContainsKey(location))
							OriginalLocationCounts[location]++;
						else
							OriginalLocationCounts.Add(location, 1);
					}
				}
			}

			
			// ==== skills block ====
			
			// There might be 1 to 40 skills.
			
			// skills come from skills block.
			
			if(! (fields[(int)iFields.skills] is System.DBNull))
			{
				string skills = (string)fields[(int)iFields.skills];

				//Skills & Expertise
				//
				//+ Drupal
				//+ Visual Basic
				
				MatchCollection mc = MyRegex.reSkillLines.Matches(skills);

				foreach(Match m in mc)
				{
					string Skill = m.Groups["skill"].Value.Trim();
					
					// DIY
					if(OriginalSkillCounts.ContainsKey(Skill))
						OriginalSkillCounts[Skill]++;
					else
						OriginalSkillCounts.Add(Skill, 1);
				}
			}

			
			// ==== experience block ====
			
			// 0-N job titles come from experience block
			
			if(! (fields[(int)iFields.experience] is System.DBNull))
			{
				string experience = (string)fields[(int)iFields.experience];

				//Experience
				//
				//+
				//
				//Desktop Support Spec.
				//
				// IBM Global Services
				//
				//Present
				//
				//+
				//
				//Desktop Support Tech
				//
				//TEKsystems
				//
				//2005 2007
				//
				
				string [] items = MyRegex.rePlusLineTriplet.Split(experience);

				// skip items[0] == header text.
				
				for(int i = 1; i < items.Length; i++)
				{
					string item = items[i];

					// I have found highly variable format.
					// Yuck counting topics by blank lines.
					// Trim() rids \r \n, avoids a line "".

					string [] lines = MyRegex.reOneOrMoreNewlines.Split(item.Trim());
					
					// say("\r\nJOB["+i+"] = " + String.Join("\r\n", lines));
					// Yes, sweet:
					//JOB[1] = Professional Services Manager/Director & Solution Architect - APAC
					//Unicorn Software Solutions Pty Ltd
					//September 2005 Present(11 years 9 months)Asia Pacific
					//Project management Corporate Performance Management and Business Intell...
					//Recommendations
					//count: (2)

					// lines[0] = Title is ALWAYS the first line
					if(lines.Length > 0)
					{
						string Title = lines[0].Trim();
						// DIY
						if(OriginalTitleCounts.ContainsKey(Title))
							OriginalTitleCounts[Title]++;
						else
							OriginalTitleCounts.Add(Title, 1);
					}
					
					// lines[1] Company line may be absent.
					// lines[1..2] Date line may be absent.
					// lines[1..3] Desc line may be absent.
					bool haveCy = false;
					bool haveDt = false;
					for(int j = 1; j < lines.Length; j++)
					{
						string assess = lines[j].Trim();
						
						if(assess == "Recommendations")
							break; // rest of lines = don't care
						
						if(haveDt == false)
						{
							Match m = MyRegex.reFromToLocn.Match(assess);
							if(m.Success)
							{
								// surely the dateEtc line
								haveDt = true;
								string DateEtc = assess;
								
								// Not of interest in this App.
								// Just prove if I got it okay:
								
								// DIY
								//if(OriginalDateEtcCounts.ContainsKey(DateEtc))
								//	OriginalDateEtcCounts[DateEtc]++;
								//else
								//	OriginalDateEtcCounts.Add(DateEtc, 1);
							}
							else
							{
								// not the date line
								if(haveCy == false)
								{
									// likely a prior company line
									haveCy = true;
									
									// However, it might be a job descr.
									// Establish metrics to distinguish.
									
									int nWordsInCompany = assess.Split(caSpace).Length;
									// noteHistogramCompanyWords(nWordsInCompany);

									// Done. Metric crossed over at [1-8]/[9..]:
									if(nWordsInCompany > 8)
									{
										// IF based on the metric;
										// SO do not collect here.

										// likely a later job description line
										string JobDesc = assess;

										// Not of interest in this App.
										// Just prove if I got it okay:

										// DIY
										// if(OriginalJobDescCounts.ContainsKey(JobDesc))
										// 	OriginalJobDescCounts[JobDesc]++;
										// else
										// 	OriginalJobDescCounts.Add(JobDesc, 1);
									}
									else
									{
										
										string Company = assess;
										// DIY
										if(OriginalCompanyCounts.ContainsKey(Company))
											OriginalCompanyCounts[Company]++;
										else
											OriginalCompanyCounts.Add(Company, 1);
									}
								}
								else
								{
									// likely a later job description line

									// Establish metrics to distinguish.
									// noteHistogramJobDescWords(assess.Split(caSpace).Length);

									string JobDesc = assess;

									// Not of interest in this App.
									// Just prove if I got it okay:

									// DIY
									// if(OriginalJobDescCounts.ContainsKey(JobDesc))
									// 	OriginalJobDescCounts[JobDesc]++;
									// else
									// 	OriginalJobDescCounts.Add(JobDesc, 1);
									
									break; // rest of lines = don't care
								}
							}
						}
						else
						{
							// surely a later job description line
							
							// Establish metrics to distinguish.
							// noteHistogramJobDescWords(assess.Split(caSpace).Length);
							
							string JobDesc = assess;

							// Not of interest in this App.
							// Just prove if I got it okay:

							// DIY
							// if(OriginalJobDescCounts.ContainsKey(JobDesc))
							// 	OriginalJobDescCounts[JobDesc]++;
							// else
							// 	OriginalJobDescCounts.Add(JobDesc, 1);
							
							break; // rest of lines = don't care
						}
					}
				}
			}

			
			// ==== education block ====

			
			// 0-N diverse items come from education block
			
			if(! (fields[(int)iFields.education] is System.DBNull))
			{
				string education = (string)fields[(int)iFields.education];

				//Education
				//
				//+
				//
				//National University
				//
				//Master of Computer Science (MCS), Computer Software Engineering
				//
				//2013 2015
				//
				//Modern Operating Systems, Database Design and Implementation, Security in Computing, Web and Cloud Computing, Software Architecture Principles, Software Engineering Fundamentals, User Interface Engineering, Engineering Software Quality, Software Testing, Software Architecure Applications, Computer Science Project I & II, Programming in Java, Programming in C++, Intro to Programming Concepts
				//
				//Activities and Societies
				//
				//Society of Hispanic Professional Engineers www.shpe.org
				//
				//+
				//
				//National University
				//
				//Bachelor's of Science, Information Technology Management
				//
				//2011 2014
				//
				//Network and application security including intrusion prevention/detection systems, LAN/WAN, wireless LAN administration, network protocols, desktop application, software development and the software development life cycle, Java, C++, C/C#, Visual Basic, MS SQL, NO SQL, My SQL, Netbeans and Netbeans integrated development environment, Microsoft Visual Studio, J Creator, Dreamweaver, Photoshop, Quark Express, X-Code, Game Maker, Game Salad, Xamarin Studio, Blender, iClone, Unity, Construct, Matlab, operating systems, IT servers using Linux/Novell/MS Server/Active Directory, desktop application support, web and cloud computing, advanced programming, user interface programming
				//
				//Activities and Societies
				//
				//SHPE - Society of Hispanic Professional Engineers San Diego Vice President - City College 2008, Current member, no office
				//
				
				
				string [] items = MyRegex.rePlusLineTriplet.Split(education);

				// skip items[0] == header text.
				
				for(int i = 1; i < items.Length; i++)
				{
					string item = items[i];

					// I have found highly variable format.
					// Yuck counting topics by blank lines.
					// Trim() rids \r \n, avoids a line "".

					string [] lines = MyRegex.reOneOrMoreNewlines.Split(item.Trim());
					
					// say("\r\n---EDU["+i+"]---\r\n" + String.Join("\r\n", lines));
					

					// lines[0] = School is ALWAYS the first line
					if(lines.Length > 0)
					{
						string School = lines[0].Trim();
						// DIY
						if(OriginalSchoolCounts.ContainsKey(School))
							OriginalSchoolCounts[School]++;
						else
							OriginalSchoolCounts.Add(School, 1);
					}
					
					// lines[1] Degree line may be absent.
					// lines[1..2] Date line may be absent.
					
					for(int j = 1; j < lines.Length; j++)
					{
						string assess = lines[j].Trim();
						
						if(assess == "Activities and Societies")
							break; // rest of lines = don't care
						
						MatchCollection mc = MyRegex.reAnyYears.Matches(assess);

						string minYear = "9999";
						string maxYear = "0000";
						
						foreach(Match m in mc)
						{
							string year = m.Groups["year"].Value.Trim();
							if(string.Compare(minYear, year) > 0)
								minYear = year;
							if(string.Compare(maxYear, year) < 0)
								maxYear = year;
						}
						if(minYear != "9999")
						{
							string EduYears = minYear + "-" + maxYear;
							
							// Not of interest in this App.
							// Just prove if I got it okay:
							
							// DIY
							if(Globals.outputDates)
							{
								if(OriginalEduYearsCounts.ContainsKey(EduYears))
									OriginalEduYearsCounts[EduYears]++;
								else
									OriginalEduYearsCounts.Add(EduYears, 1);
							}
						}
						else
						{
							// Not a date line
							// Likely a Degree line
							string Degree = assess;
							
							// DIY
							if(OriginalDegreeCounts.ContainsKey(Degree))
								OriginalDegreeCounts[Degree]++;
							else
								OriginalDegreeCounts.Add(Degree, 1);
						}
					}
				}
				
			}

			
			// ==== certifications block ====
			
			// 0-N diverse items come from certifications block
			
			if(! (fields[(int)iFields.certifications] is System.DBNull))
			{
				string certifications = (string)fields[(int)iFields.certifications];

				//Certifications
				//
				//+
				//
				//Apple Developer Certificate since 2013
				//
				//Apple, License
				//
				//November 2013 November 2016
				//
				
				string [] items = MyRegex.rePlusLineTriplet.Split(certifications);

				// skip items[0] == header text.
				
				for(int i = 1; i < items.Length; i++)
				{
					string item = items[i];

					// I have found highly variable format.
					// Yuck counting topics by blank lines.
					// Trim() rids \r \n, avoids a line "".

					string [] lines = MyRegex.reOneOrMoreNewlines.Split(item.Trim());
					
					//say("\r\n---CERT["+i+"]---\r\n" + String.Join("\r\n", lines));
					

					// lines[0] = Certificate is ALWAYS the first line
					if(lines.Length > 0)
					{
						string Certificate = lines[0].Trim();
						// DIY
						if(OriginalCertificateCounts.ContainsKey(Certificate))
							OriginalCertificateCounts[Certificate]++;
						else
							OriginalCertificateCounts.Add(Certificate, 1);
					}
					
					// lines[1] CertLicense line may be absent.
					// lines[1..2] Date line may be absent.

					for(int j = 1; j < lines.Length; j++)
					{
						string assess = lines[j].Trim();
						
//						if(assess == "Activities and Societies")
//							break; // rest of lines = don't care
						
						MatchCollection mc = MyRegex.reAnyYears.Matches(assess);

						string minYear = "9999";
						string maxYear = "0000";
						
						foreach(Match m in mc)
						{
							string year = m.Groups["year"].Value.Trim();
							if(string.Compare(minYear, year) > 0)
								minYear = year;
							if(string.Compare(maxYear, year) < 0)
								maxYear = year;
						}
						if(minYear != "9999")
						{
							string CertYears = minYear + "-" + maxYear;
							
							// Not of interest in this App.
							// Just prove if I got it okay:
							
							// DIY
							if(Globals.outputDates)
							{
								if(OriginalCertYearsCounts.ContainsKey(CertYears))
									OriginalCertYearsCounts[CertYears]++;
								else
									OriginalCertYearsCounts.Add(CertYears, 1);
							}
						}
						else
						{
							// Not a date line
							// Likely a License line
							string CertLicense = assess;
							
							// DIY
							if(OriginalCertLicenseCounts.ContainsKey(CertLicense))
								OriginalCertLicenseCounts[CertLicense]++;
							else
								OriginalCertLicenseCounts.Add(CertLicense, 1);
						}
					}
				}

				
			}
			
			
		}


		
		public void PostInputPhaseAnalysis()
		{
			// Two Histograms just used to eyeball and choose a code threshold:
			// OutputHistogram(HistogramCompanyWords, "HistogramCompanyWords");
			// OutputHistogram(HistogramJobDescWords, "HistogramJobDescWords");

			// Show me! Show me! Show me!
			
			// I've seen it all now. TMI.

//			OutputDictQtyNamesByDescendingQty(OriginalIndustryCounts, "Original Industry", 0);
//			OutputDictQtyNamesByDescendingQty(OriginalLocationCounts, "Original Location", 0);
//
//			OutputDictQtyNamesByDescendingQty(OriginalSkillCounts, "Original Skill", 0);
//
//			OutputDictQtyNamesByDescendingQty(OriginalTitleCounts, "Original Title", 0);
//			OutputDictQtyNamesByDescendingQty(OriginalCompanyCounts, "Original Company", 0);
//			OutputDictQtyNamesByDescendingQty(OriginalDateEtcCounts, "Original DateEtc", 0);
//			OutputDictQtyNamesByDescendingQty(OriginalJobDescCounts, "Original JobDesc", 0);
//
//			OutputDictQtyNamesByDescendingQty(OriginalSchoolCounts, "Original School", 0);
//			OutputDictQtyNamesByDescendingQty(OriginalDegreeCounts, "Original Degree", 0);
//			if(Globals.outputDates)
//				OutputDictQtyNamesByDescendingQty(OriginalEduYearsCounts, "Original EduYears", 0);
//
//			OutputDictQtyNamesByDescendingQty(OriginalCertificateCounts, "Original Certificate", 0);
//			OutputDictQtyNamesByDescendingQty(OriginalCertLicenseCounts, "Original License", 0);
//			if(Globals.outputDates)
//				OutputDictQtyNamesByDescendingQty(OriginalCertYearsCounts, "Original CertYears", 0);
			
			// OutputDictQtyNamesByDescendingQty(Original Counts, "Original ", 0);
			

			
			// That was all linear.
			// Now I'll make magic.
			

			Clustering.InitWordFrequencyTable();

			// Not all of these are appropriate texts to cluster:
			
			// OriginalIndustryCounts
			// OriginalLocationCounts
			// OriginalSkillCounts
			// OriginalTitleCounts
			// OriginalCompanyCounts
			// OriginalDateEtcCounts
			// OriginalJobDescCounts
			// OriginalSchoolCounts
			// OriginalDegreeCounts
			// OriginalEduYearsCounts
			// OriginalCertificateCounts
			// OriginalCertLicenseCounts
			// OriginalCertYearsCounts

			
			// Groups similar items under the same 'stem' Token Key.
			Dictionary<string,List<string>> stems;
			
			// Take Crap, Give back Gold!
			
			Clustering.GroupSimilars(OriginalIndustryCounts, out stems, "OriginalIndustryCounts");
			OutputStemmedItems(stems, "OriginalIndustryCounts");
			
			Clustering.GroupSimilars(OriginalSkillCounts, out stems, "OriginalSkillCounts");
			OutputStemmedItems(stems, "OriginalSkillCounts");
			
			Clustering.GroupSimilars(OriginalTitleCounts, out stems, "OriginalTitleCounts");
			OutputStemmedItems(stems, "OriginalTitleCounts");
			
			Clustering.GroupSimilars(OriginalCompanyCounts, out stems, "OriginalCompanyCounts");
			OutputStemmedItems(stems, "OriginalCompanyCounts");
			
			Clustering.GroupSimilars(OriginalSchoolCounts, out stems, "OriginalSchoolCounts");
			OutputStemmedItems(stems, "OriginalSchoolCounts");
			
			Clustering.GroupSimilars(OriginalDegreeCounts, out stems, "OriginalDegreeCounts");
			OutputStemmedItems(stems, "OriginalDegreeCounts");
			
			Clustering.GroupSimilars(OriginalCertificateCounts, out stems, "OriginalCertificateCounts");
			OutputStemmedItems(stems, "OriginalCertificateCounts");
			
			Clustering.GroupSimilars(OriginalCertLicenseCounts, out stems, "OriginalCertLicenseCounts");
			OutputStemmedItems(stems, "OriginalCertLicenseCounts");
			
		}

		
		// Bazillion times I have done like unto this:

		void OutputDictQtyNamesByDescendingQty(Dictionary<string,int> dict, string legend, int threshold)
		{
			SortedSet<string> itemsByDescFreq = new SortedSet<string>();
			int sumItemQtys = 0;

			foreach(KeyValuePair<string, int> kvp in dict)
			{
				sumItemQtys += kvp.Value;
				if(kvp.Value < threshold) // zero to say all
					continue;
				itemsByDescFreq.Add(kvp.Value.ToString().PadLeft(7) + " " + kvp.Key);
			}

			// Prefix (due to sort) this title:
			itemsByDescFreq.Add("There were " + sumItemQtys + " counts in " + itemsByDescFreq.Count + " lines for " + legend);

			// one huge say:
			say("\r\n=====*****=====\r\n" +
			    string.Join("\r\n", new List<string>(itemsByDescFreq).ToArray().Reverse()) +
			    "\r\n-----*****-----\r\n");
		}
		
		
		// develop a metric to distinguish
		// company names (should be short)
		// from descriptions (often long).

		// Done. Fini. Metric crossed over at [1-8]/[9..]:
		
		/*
		const int nWordsRange = 20;
		
		static int [] HistogramCompanyWords = new int[nWordsRange];
		static int [] HistogramJobDescWords = new int[nWordsRange];
		
		void noteHistogramCompanyWords(int nWords)
		{
			if(nWords > nWordsRange - 1)
				nWords = nWordsRange - 1;
			HistogramCompanyWords[nWords]++;
		}
		
		void noteHistogramJobDescWords(int nWords)
		{
			if(nWords > nWordsRange - 1)
				nWords = nWordsRange - 1;
			HistogramJobDescWords[nWords]++;
			
		}
		
		void OutputHistogram(int[] histogram, string legend)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(legend);
			for(int i = 0; i < nWordsRange; i++)
			{
				sb.Append("[" + i.ToString().PadLeft(3) + "]: " + histogram[i].ToString().PadLeft(8));
			}
			say(sb.ToString());
		}
		 */
		
		void OutputStemmedItems(Dictionary<string,List<string>> alike, string legend)
		{
			SortedSet<string> BlocksByFrequency = new SortedSet<string>();
			
			foreach(KeyValuePair<string,List<string>> kvp in alike)
			{
				// kvp.Value are not sorted. do so:
				SortedSet<string> similars = new SortedSet<string>();
				int sumQty = 0;
				foreach(string s in kvp.Value)
				{
					// recover qty:
					sumQty += int.Parse(s.Substring(0, 8).Trim());
					similars.Add(s);
				}
				
				BlocksByFrequency.Add(sumQty.ToString().PadLeft(8) +
				                      " Topic Key: " + kvp.Key +
				                      "\r\n" + String.Join("\r\n", similars.Reverse()));
			}
			
			say("\r\n\r\n" + legend + "\r\n\r\n" +
			    String.Join("\r\n\r\n",BlocksByFrequency.Reverse()) +
			    "\r\n\r\n");
		}


	}
}