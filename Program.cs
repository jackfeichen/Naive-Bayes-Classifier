using System;
using System.Collections.Generic;
using System.IO;

namespace classifier 
{
	class Program 
    {
		private static readonly string[] LABELS = {"atheism","auto","baseball","christian","crypt","electronics","graphics","guns","hockey","ibm","mac","medical","mideast","motorcycles","politics","religion","sale","space","windows","winx"};

		public static void Main (string[] args) 
        {
			if(args.Length != 2) 
            {
				Console.WriteLine("Please use 2 arguments: splits/train# splits/test#");
				return;
			}
			string train = args[0];
            string test = args[1];
			
			string dir = Path.GetDirectoryName(Path.GetDirectoryName(train));
			string dataPath = Path.Combine(dir, "data");
			
			string[] files = File.ReadAllLines(train);

            //DateTime start = DateTime.Now;

            Dictionary<string, string> examples = new Dictionary<string, string>(files.Length);
            foreach (string file in files)
                examples.Add(file, File.ReadAllText(Path.Combine(dataPath, file)));

            files = File.ReadAllLines(test);
            Dictionary<string, string> tests = new Dictionary<string, string>(files.Length);
            foreach (string file in files)
                tests.Add(file, File.ReadAllText(Path.Combine(dataPath, file)));

            Algorithm algo = new Algorithm();

            algo.LearnNaiveBayesText(examples, LABELS);
            algo.Classify(tests, LABELS);

            //Console.WriteLine("This took: {0:s}", (DateTime.Now - start));

            //Test.TestPath();
            //Test.TestSet();
            //Test.TestRegex();
            //Test.TestDictionarySort();

            //Console.WriteLine("Done, press enter to exit.");
            //Console.ReadLine();
            foreach (KeyValuePair<string, string> entry in algo.classification)
                Console.WriteLine(entry.Key + " " + entry.Value);
        }
	}
}

