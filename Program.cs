using System;
using System.Collections.Generic;
using System.IO;

namespace classifier 
{
	class Program 
    {
		/// <summary>
		/// This is just the set of pre-defined labels. 
		/// It can be provided as a command-line argument or stored in a database.
		/// </summary>	
		private static readonly string[] LABELS = {
			"atheism","auto","baseball","christian","crypt","electronics","graphics","guns","hockey","ibm",
			"mac","medical","mideast","motorcycles","politics","religion","sale","space","windows","winx"};

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
			
            //DateTime start = DateTime.Now;

			// First, pull in all of the training files and store it in memory
			string[] files = File.ReadAllLines(train);

            Dictionary<string, string> examples = new Dictionary<string, string>(files.Length);
            foreach (string file in files)
                examples.Add(file, File.ReadAllText(Path.Combine(dataPath, file)));

			// Next, grab all of the test files and reall it all
            files = File.ReadAllLines(test);
            Dictionary<string, string> tests = new Dictionary<string, string>(files.Length);
            foreach (string file in files)
                tests.Add(file, File.ReadAllText(Path.Combine(dataPath, file)));

            Algorithm algo = new Algorithm();
			
			/* 
			 * Train the naive bayes algorithm to place weights on words 
			 * and classify these words for various labels based on 
			 * the training data.
			 */
            algo.LearnNaiveBayesText(examples, LABELS);
			
			/*
			 * Classify the test data based on the trained algorithm.
			 */
            algo.Classify(tests, LABELS);

            //Console.WriteLine("This took: {0:s}", (DateTime.Now - start));

			// Output the results (can be pipe-directed to a file)
            foreach (KeyValuePair<string, string> entry in algo.classification)
                Console.WriteLine(entry.Key + " " + entry.Value);

            //Test.TestPath();
            //Test.TestSet();
            //Test.TestRegex();
            //Test.TestDictionarySort();

            //Console.WriteLine("Done, press enter to exit.");
            //Console.ReadLine();
        }
	}
}

