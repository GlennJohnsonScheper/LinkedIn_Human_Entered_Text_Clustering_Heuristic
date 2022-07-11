/*
 * ClusterVocabulary.cs
 * 
 * 
 * To the tune of "I need a new drug." (algorithm)
 * 
 * 
 * I did TF-IDF = term-frequency/inverse-document-frequency,
 * which is appropriate for plain text, no part of which is
 * more important than any other part of the same text body.
 * 
 * Here, I need to value words more highly when they appear
 * in the most-valuable (DB most-frequent) vocabulary items.
 * 
 * 
 * E.g., Here is the top of a list of free-form entered skills:
 * 
 * There were 200969 unique Skill lines:
 * 
 119461 Management
 106087 Leadership
  75340 Project Management
  68583 Strategy
 ...
  18872 C#
 ...
   2462 C #
 ...
      1 - Aviation Anti-Submarine Warfare Operator A and C School
      1 - Application Servers: Apache Tomcat
      1 - Application Server: IBM WebSphere, MS SQL Server
      1 - Anvil, Solidworks, GD&T ASME Y14.5, ISO
      1 - Analysis & Design of Algorithms and Data Structures.
 ...
      1 - Able to interact professionally with individuals on all levels
      1 - 15 years MS Office experience; proficient in Excel and PowerPoint
      1 - - -
      1 ---
 * 
 * 
 * 
 * 
 * The Plan:
 * 
 * (and now illustrated with some actually results)
 *
 *
 * Create a topic key from each input line-item,
 * which will be used to group similar line-items.
 *
 * To make such a topic key:
 * 
 * 
 * 0. Convert UNICODE/LATIN1 to USASCII; Lowercase
 * 
 * 
 * 1. Convert specials to text: C#, .net, R&D, M & A, &nbsp;, &amp;, &,...
 * 
 * 
 * 2. Depunct, Dedup spaces, sorted-set to unique words, trim.
 * 
 * Items making empty keys, also non-alphabetics can be omitted;
 * 
 * In fact, drop any purely numeric words, not as /0/4/csharp/.
 * 
 * Note how the sorted-set creates unique unambiguous keys:
 * 
    1447 /and/development/research/
    1378 Research and Development (R&D)
      42 Research and Development
      21 Research & Development
       3 Research and development
       2 research and development
       1 Research And Development (r&d)
 *
 * 
 * 3. Keep a word freq count on all words in Token Keys.
 * 
 * As my profile corpus was gathered for certain fields,
 * the words pertaining those fields should occur often.
 * 
 * E.g.,
 * 835480, management
 * 100205, sql
 *  79784, and
 *  22682, csharp
 *    105, fishing
 *      2, pseudocode
 * 
 * 
 * Last time, I computed every word's value as a sum
 * of the counts of vocabulary items containing word.
 * 
 * I'll also adjust values by English word frequency.
 *
 * Awesome actually:
 * 
WordValue, WordQty, Word:
  256747,   835480, management
  213663,   213663, leadership
  207080,   237633, analysis
  201126,   201126, strategy
  158203,   158203, planning
  114316,   146175, sales
  112746,   112746, strategic
  110252,   212371, marketing
  101907,   101907, enterprise
  100205,   100205, sql
  ...
     973,    79784, and
  ...
       0,        1, actually
 * 
 * 
 * 4. Start discarding the least valuable words.
 *
 * E.g., (now with real data, (#) is WordQty sum of sublist)
...
removing word just
revising key /in/just/time/ to /in/time/ (4)
revising key /culture/just/ to /culture/ (2)
revising key /earned/him/hire/just/of/others/over/recommendation/the/ to /earned/him/hire/of/others/over/recommendation/the/ (1)
removing key /just/ (1)
...
 *
 *
 * 5. Decide some measure for when to stop discarding
 * 
 * - E.g., when 10% of the total number of items, i.e.,
 * the sum of all the item frequency counts, have been
 * newly moved from a non-empty key into the empty key.
 * 
 * Situation before the word (and key) discarding loop:
Removing 0, leaving 49456 words, yields 100% of value
 *
 * Situation after the word (and key) discarding loop:
Removing 48581, leaving 875 words, yields 90% of value
 *
 * Dropping unimportant words clustered similar topics:
 * 
 * And I just love the results, hence it is a new drug:
 * 
  175009 Topic Key: /management/
  119462 Management
   15485 IT Management
   13351 Time Management
    1753 Release Management
    1532 People Management
    1289 Knowledge Management
    1156 Document Management
     893 Store Management
     893 Facilities Management
     830 Stakeholder Management
     791 Utilization Management
     709 Pain Management
     ...
 *
 * 
 * Q.E.D.
 * 
 * 
 * The next step is to create DB tables of equivalent items,
 * to broaden the results of search for skills, titles, etc.
 *
 */


using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using GlennGlobals;
using GlennRegex;

namespace ClusterVocabulary
{
	/// <summary>
	/// ClusterVocabulary cracks one hard nut
	/// for its only caller, ProfileAnalyzer.
	/// 
	/// Receiving dict<str, int> of item counts,
	/// it decomposes those items and assigning
	/// value to words and items, discards many.
	/// 
	/// It produces a dict<str,list<str>> whose
	/// keys, as it were, stem all listed items.
	/// </summary>
	public static class Clustering
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

		
		// Input data of top English Word Frequencies.
		
		// I got this from a 2002 C++ Web Browser App.
		
		static string WordFreqEnglish =
			"9999to9999the9999that9999of9999is9999in9999for9999and9999a9041it8750you8364on7888i7240this6945with6461by6242as5995be5981are5821or4848have4830at4725from4670not4650was4222your4163we3839all3638an3545more3498but3414they3403can3198will3129if3038about" +
			"2948he2901one2809what2788my2626has2546new2475their2364which2354so2349there2310his2182our2166time2148who2113do2099other2096com2084when2038no2008out1933up1928would1883some1866people1853us1811how1766were1689also1678been1668like1632had1594just1591may" +
			"1571any1561only1538them1522use1510first1507than1506form1490these1481see1479now1474most1470page1453me1436get1402home1391very1353news1336here1336free1330world1328its1319into1279information1276her1259make1220many1196then1196search1195site1190work" +
			"1173over1169s1147said1105after1104two1088it's1087am1084web1083such1082because1081language1077should1067good1060well1040know1039back1024she1016help1007used1005don994even988system985could982years980year972think970find968www964those954way949go944don't" +
			"935where932life929much918re895day879read879name870posted853business851pm849last836great826same825through818windows815being808book807need803before799want794top783comments777right770post753each748part740made740click732article731him725does721take" +
			"719still713area712view698state692service683own683best680english676love674under673while669online668why668did667january665between656http651another649software640add637both636u635every633contact631today631history631c629video626since625support621say" +
			"620must620long617number615using612list609states596too595down592public591i'm588links584user584data580off579next578different570going567three566ve566really565without561god559services559old557things553change552internet545never545found544program543end" +
			"542university538file536united536blog530word530look530books527available524however523link523earth517little514ll514health513real510school508version508report507show505content501during499comment498set498email497war497example497church496languages" +
			"496american495something495please495around491children490place490full489come483times482level481text481order474based473called471few470high470edit469security469better468words467library463family458start458president457policy455man453rights453might451says" +
			"451e451case449company446point442group442articles441community441again440money439open436media433music432terms428give427research427government427against425days424national424general422story421person414code412rss407important406let405privacy404august" +
			"402power402always399related398often397management397copyright396users396second394microsoft394big393small392pages392law392ago391website391create390posts389line389house388sad388human387office385reply385city384review384including382p380design374mail" +
			"372white372google371technology371image371got370current368others367m366country365that's365less365date364local364live364d363shall359within359following359b357later357keep357account356though356once356learn356development355week355special355source355put" +
			"355game355ever355crying354thing354center353access352computer349yet349possible348students348question348international347call346problem345main345fact344sure344bush343sites343science342systems342sign342personal341html339press339john337org337education" +
			"336yes336december335left335jan334november334message332reviews332means332making332info331questions331project330until330several330having330done329least328lot327already326member324section324results323working323process323network323control323can't" +
			"323become322america321type321able320women320members320check319course318buy316product315recent314issues313whether313known313changes312resources311you're311job310try310space309run309files309features308october308july307iraq306enough305download304hard" +
			"304ask303price303given301thought301server301care300value300tell300non300far300away299provide299net298york298send298car296note295actually294future293programs292tools292t292problems292easy291water291share291feel291didn290someone290large290believe289tv" +
			"289either288looking287guide287didn't286travel286events286bad284social284products284four284early283r283print283issue283doing282february282century281nothing280uk280past279x278september278names278jobs277games277class276i've275single275inc275g274doesn" +
			"274common273true273reading273getting273art272men271talk269makes269doesn't269document269black269among269address268play268anything267works267include267child265write265south265came265author264side264party264experience264cannot263march263details262team" +
			"262simple261whole261n260map260kind259written258friends258blogs256writing256rather256body256above255visit255together255months255energy254hours254death253june252complete251subject251air250stories250etc248seen248mr248answer247mind247images247idea" +
			"246young246reserved246needs245popular245living245latest245forum244return244profile244modern244major243save243original243join243customer242store242reason242per241room241published241probably241photo240star240million239night239bit238upon237thanks" +
			"237sports237j237e-mail237almost237age236society236dr235size235marketing235anyone234stars233instead233april232added231photos230subscribe230hand230although229study229light229below229application228month228close227w227standard227quote227license227action" +
			"226vote226everything226co225understand225political225market224style224series224pay223wikipedia223third223kids223friend223five222term222result222learning222food221won221title221plan221matter221key221bill220similar220else220asked219went219took219simply" +
			"219hope219everyone218sex218north217started217shop217mean217daily217card216window216rate215taken214phone214according213short211table211l211department211amazon210court209present209legal209groups209global208what's207trying207told207seems207directory" +
			"206test206index206building206basic205specific205self205quality205especially204watch204provided204military203ways203various203training203register203property203field202request202points202edition202de202college201shows201low201discussion201created" +
			"201board200event200credit200companies200certain199themselves199front199act198private198particular198heart198head197sense197box196there's196rating196oil196middle196items196comes196along195wanted195usually195quite195further194update194thank194stop" +
			"194shopping194parts194itself194industry194half193total193tax193sometimes193lost193clear192release191running191knowledge191isn191applications190we're189usa189taking189culture189cost188washington188topic188record188politics187thus187soon187previous" +
			"187magazine187face186sun186prices186isn't186h186domain185period184yahoo184woman184needed184move184hotel184force184additional183sound183remember183options183mobile183george183f182wrong182o181interest181editor181coming181al180paper180offer180likely" +
			"180digital180countries179allow178v178nature178fun177red177radio177provides177maybe177login177david177across176staff176reports176reference176feed176federal176christmas176choose176behind175you'll175medical175french175forums175deal175continue175cause" +
			"174wrote174videos174q174material174leave174fast174faq173rules173percent173object173linux173inside172required172perhaps172nation172east171turn171operating171movie171longer171journal171beyond170tags170professional170database170copy170central170became" +
			"170average169near169model169location169due168yourself168takes168mother168hot168congress168cases167programming166natural166ideas166film166audio165west165official165late165effect165administration164sale164permalink164parents164format163st163saying" +
			"163response163log163finally163environment163bring163advertising162tips162student162minutes162interesting162individual162includes162china162apple161wish161via161step161performance161pc161offers161item161happy161foreign160useful160php160paul160mark" +
			"159receive159meet159included159function159father159character158memory158final158figure158europe158county157uses157six157picture157outside157entire157entertainment157dvd157build156sales156gets155views155topics155statement155opinion155follow155evidence" +
			"155blue155began154stuff154rest154node154display153position153lives152select152police152land152java152himself152hear152freedom152former152flash152consider152command152canada152archive151thinking151tech151pretty151menu151conditions151baby151amount" +
			"150updated150therefore150street150sent150seem150saw150goes150drive149won't149studies149song149cover148whose148limited148i'll148green148christian147schools147necessary147king147contents147button147bible146welcome146talking146london146laws146financial" +
			"146enter146choice146category146campaign146areas145truth145released145higher145heard145gift145color145categories145cart144trade144numbers144client144california143recently143foundation143engine143cd143archives143advertise142submit142sell142quick" +
			"142james142id142he's142difficult142cards141purpose141papers141nice141insurance141estate141directly140meaning140designed140collection140analysis139technical139speak139population139organization139ones139ms139michael139increase138they're138strong" +
			"138screen138risk138reader138pictures138movies138customers138currently138agree137navigation136weeks136stay136san136religious136oh136machine136k136england136considered136browser136british135weather135direct135ad135ability134religion134master134letter" +
			"134ii134held134gives134documents134browse133road133practice133changed133attention132win132standards132records132lead132i'd132german132focus132feedback132developed132built132advice132advanced131voice131types131success131rule131range131option" +
			"131elements131easily131dead131connection130safety130role130received130projects130myself130morning130interested130hit130gave130clinton130beginning129wide129values129tool129theory129forward129difference129decision129arts129americans128involved" +
			"128hardware128growth128feeds128economic128answers127wife127sources127ready127plus127peace127paid127feature127default127cars127appear126understanding126town126spanish126park126hands126featured126association125track125quickly125notice125hour125girl" +
			"125fire125authors124status124speech124resource124registered124multiple124entry124dictionary124description124attack124addition123western123solution123player123pdf123notes123hold123giving123favorite123apply122xml122wednesday122jesus122european122cut0";

		// To split numbers from words (capturing split pattern)
		
		static Regex reNumbers = new Regex("([0-9]+)", RegexOptions.Compiled);
		
		// 122 = Relative word frequencies range [9999...122].
		
		static Dictionary<string,int> freq122 = new Dictionary<string, int>(1200);

		
		/// <summary>
		/// just builds freq122 from words above it.
		/// </summary>
		
		public static void InitWordFrequencyTable()
		{
			// Create English word freq lookup Dict:
			// All unlisted words use the value 122.
			
			string [] sa = reNumbers.Split(WordFreqEnglish);
			
			// sa[0], sa[0]sa.Length-1 = empty strings.
			
			for(int i = 1; i < sa.Length-2; i += 2)
			{
				freq122.Add(sa[i+1], int.Parse(sa[i]));
			}
		}
		
		// for String.Split:
		
		static char [] caSpace = { ' ' };

		
		/// <summary>
		/// This is the workhorse of ProfileAnalyzer.
		/// </summary>
		/// <param name=origs>input vocabulary items</param>
		/// <param name=stems>output clustered items</param>

		public static void GroupSimilars(Dictionary<string,int> origs, out Dictionary<string,List<string>> stems, string legend)
		{
			stems = new Dictionary<string,List<string>>(); // to output
			
			// These origs are UNCLEANED, VERBATIM from profile data.
			
			// Will I need a line count, and/or a sum of item counts?

			int nOrigLines = origs.Count;
			int nOrigItems = 0;
			int nOrigEmpty = 0;
			
			// step 1 of solving word value:
			Dictionary<string,int> wordQty = new Dictionary<string,int>();
			
			// step 2 of solving word value:
			Dictionary<string,int> wordValue = new Dictionary<string,int>();
			
			// for a quick report of those:
			SortedSet<string> showWords = new SortedSet<string>();
			
			int nQtyOrigNonEmpty = 0;
			
			foreach(KeyValuePair<string,int> kvp in origs)
			{
				nOrigItems += kvp.Value; // count of this vocabulary item.

				string origText = kvp.Key;
				
				int origWeight = kvp.Value;

				string s = origText;
				
				// 0. rid non-USASCII
				
				s = MyRegex.nonUsAscii.Replace(s, MyRegex.replaceUnicodeEvaluator);
				
				// 1. Convert specials to text: C#, .net, R&D, M & A, &nbsp;, &amp;, &,...
				// Oh, I need Tolower first, as evaluator contains lowercase words.

				s = s.ToLower();
				s = s.Replace("&amp;", "&");
				s = MyRegex.reFavorites.Replace(s, MyRegex.replaceFavoritesEvaluator);
				s = s.Replace("&", " and ");

				// 2. Depunct, Dedup spaces, trim, lowercase, to make an indexing key.
				
				s = MyRegex.reSpacePunctRun.Replace(s, " ");
				s = s.Trim();
				
				// These word are not unique
				string[] wordsInLine = s.Split(caSpace);
				
				// Use a Sorted Set of words in line,
				// to de-duplicate; Also to regulate.
				SortedSet<string> setOfWords = new SortedSet<string>(wordsInLine);

				// concoct a key to hold line:
				// e.g., /development/software/
				string ky = "/" + String.Join("/", setOfWords) + "/";
				
				// Wait, rid numeric words from key: /5/vmware/
				ky = MyRegex.reSlashNumericSlash.Replace(ky, "");
				
				
				// rather than asking empty,
				// ask if any ALPHAs remain.
				// if(ky == "//") // empty from filtering orig
				if(MyRegex.reAlphas.IsMatch(ky) == false)
				{
					nOrigEmpty += origWeight; // need not store
				}
				else
				{
					nQtyOrigNonEmpty += origWeight; // to solve %
					
					// same key MAY already exist in output
					
					if(stems.ContainsKey(ky) == false)
					{
						stems.Add(ky, new List<string>());
					}
					
					// Forget messy obj tuples, arrays, instances.
					// Just prepend item's weight to value string.
					
					string qtyItem = origWeight.ToString().PadLeft(8) + " " + origText;
					stems[ky].Add(qtyItem);
					
					// Now also, for all the unique words in set,
					// add word with origWeight as a "word value",
					// summing to existing if it already existed.

					foreach(string word in setOfWords)
					{
						
						// step 1 of solving word value:
						// (Later to adjust by English word frequency.)
						
						if(wordQty.ContainsKey(word) == false)
						{
							wordQty.Add(word, origWeight);
						}
						else
						{
							wordQty[word] += origWeight;
						}
					}
				}
			}
			
			// All input values have been filtered into alike.
			
			say("Clustering: " + legend + ": Input of " +
			    nOrigLines + " Lines yielded " +
			    nOrigItems + " Total Items/Counts (" +
			    nOrigEmpty + " Empty + " +
			    nQtyOrigNonEmpty + " Non-empty Key Tokens).");
			
			
			// Also, WordQty has been tallied up.
			// Create WordValue from it, show me:
			
			int nQtyRemaining = nQtyOrigNonEmpty;
			
			foreach(KeyValuePair<string,int> kvp in wordQty)
			{
				string word = kvp.Key;
				int origQty = kvp.Value;
				int scaled = origQty;
				
				if(freq122.ContainsKey(word))
				{
					// This word has a freq factor from 122 to 1000.
					// The more frequent words are to be disparaged.
					scaled = origQty * 122 / freq122[word];
				}
				
				wordValue.Add(word, scaled);
				
				showWords.Add(
					scaled.ToString().PadLeft(8) + ", " +
					origQty.ToString().PadLeft(8) + ", " +
					word);
			}

			
			// Lovely 99 times. Now TMI
			
			// say("WordValue, WordQty, Word:\r\n" +
			//    string.Join("\r\n", showWords.Reverse()));
			
			
			// Now I must start looping on word list reduction.

			int wordsLeft = wordQty.Count;
			int wordsGone = 0;

			int PercentRemains = 100;
			
			foreach(string wv in showWords)
			{
				// Periodically as words are removed,
				// display the remaining clusters:
				// Moved atop loop, to dump at 100%:
				
				PercentRemains = 100 * nQtyRemaining / nQtyOrigNonEmpty;
				
				if(PercentRemains <= 90) // best % values yet TBD
				{
					
					say("Clustering: " + legend + ": Stopped at " +
					    PercentRemains + "% of item coverage remaining " +
					    "in " + stems.Count + " stemmed Key Tokens " +
					    "after deleting " + wordsGone + ", keeping " + wordsLeft +
					    " of " + wordQty.Count + " words.");

					return;
				}
				
				// This was used in my side app to develop code:
//				if(PercentRemains <= NextDumpPercent)
//				{
//					dumpAlike(fileItem, PercentRemains, wordsGone, wordsLeft);
//					NextDumpPercent -= dumpStepPercent;
//					if(NextDumpPercent < dumpStopPercent)
//						break;
//				}
				
				
				// from least valuable to most
				string word = wv.Substring(20);
				
				// say("removing word " + word);
				wordsGone++;
				wordsLeft--;

				string slashedWord = "/" + word + "/";
				
				List<string>keysToDo = new List<string>();
				foreach(string akey in stems.Keys)
				{
					if(akey.Contains(slashedWord))
						keysToDo.Add(akey);
				}
				
				foreach(string akey in keysToDo)
				{
					// what is the total held in akey?
					
					int sumQty = 0;
					foreach(string s in stems[akey])
					{
						// recover qty:
						sumQty += int.Parse(s.Substring(0, 8).Trim());
					}
					
					string newKey = akey.Replace(slashedWord, "/");
					// usually /x/y/z/ - /y/ becomes /x/z/,
					// but /x/ - /x/ becomes / when empty.
					
					// rather than asking empty,
					// ask if any ALPHAs remain.
					// if(newKey == "/")
					if(MyRegex.reAlphas.IsMatch(newKey) == false)
					{
						// remove akey from alike.
						// say("removing key " + akey + " (" + sumQty + ")");
						
						// deduct sumQty removed.
						stems.Remove(akey);
						nQtyRemaining -= sumQty;
					}
					else
					{
						// revise (and merge) akey.
						// say("revising key " + akey + " to " + newKey + " (" + sumQty + ")");
						
						if(stems.ContainsKey(newKey) == false)
						{
							// I can just move existing list:
							stems.Add(newKey, stems[akey]);
						}
						else
						{
							// I have to merge two lists
							stems[newKey].AddRange(stems[akey]);
						}
						stems.Remove(akey);
					}
				}
			}
		}
	}
}
