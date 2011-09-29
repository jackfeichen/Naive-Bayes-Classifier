using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace classifier
{
    internal class Test
    {
        public static void TestPath(string file)
        {
            string dir = Path.GetDirectoryName(Path.GetDirectoryName(file));
            string dataPath = Path.Combine(dir, "data");

            Console.WriteLine(dir);
            Console.WriteLine(dataPath);
            Console.WriteLine(Directory.Exists(dataPath));
        }

        public static void TestSet()
        {
            Set<string> names = new Set<string>();
            names.Add("Jim");
            names.Add("Jones");
            Console.WriteLine("contains Jim? {0}", names.Contains("Jim"));
            Console.WriteLine("contains jim? {0}", names.Contains("jim"));
            names.Add("Jim");
            Console.WriteLine(names.Count);
            names.AddRange(new string[] { "Jones", "Jonny", "Michael" });
            Console.WriteLine(names.Count);
        }

        public static void TestRegex()
        {
            Regex re = new Regex(@"\btest\b", RegexOptions.IgnoreCase);
            String sample = "testing test TESTING TEST Testing Test tesTING tesT TESt";

            MatchCollection mc = re.Matches(sample);
            Console.WriteLine("Total count: {0}", mc.Count);
            foreach (Match m in mc)
                Console.WriteLine(m.Value);
        }

        public static void TestDictionarySort()
        {
            Dictionary<string, double> dict = new Dictionary<string, double>();
            dict.Add("john", 1.535d);
            dict.Add("joe", 1.5356d);
            dict.Add("amy", 1.4325d);

            List<double> sorter = new List<double>(dict.Values);
            sorter.Sort();
            sorter.Reverse();

            foreach (KeyValuePair<string, double> entry in dict)
            {
                if(entry.Value.Equals(sorter[0]))
                    Console.WriteLine("{0} - {1}", entry.Key, entry.Value);
            }
        }
    }
}
