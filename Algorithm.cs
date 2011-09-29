using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace classifier
{
    internal class Set<T> : ICollection<T>
    {
        private Dictionary<T, int> d;
        public Set() { d = new Dictionary<T, int>(); }
        public Set(int capacity) { d = new Dictionary<T, int>(capacity); }
        public Set(IEnumerable<T> col) : this() { this.AddRange(col); }

        internal IDictionary<T, int> D { get { return d; } }
        public int Count { get { return d.Count; } }
        public int this[T key] { get { return d[key]; } }

        private int totalWordCount = 0;
        public int TotalWordCount 
        { 
            get { return totalWordCount; }
            set { totalWordCount = value; }
        }
        public virtual void Add(T item) 
        {
            if (!d.ContainsKey(item)) { d.Add(item, 1); }
            else { d[item]++; }
        }
        public void AddRange(IEnumerable<T> col) { foreach (T item in col) { this.Add(item); } }
        public bool Contains(T item) { return d.ContainsKey(item); }
        public bool Remove(T item) { return d.Remove(item); }
        public void Clear() { d.Clear(); }

        public void CopyTo(T[] array, int arrayIndex) { d.Keys.CopyTo(array, arrayIndex); }
        public bool IsReadOnly { get { throw new NotImplementedException(); } }
        public IEnumerator<T> GetEnumerator() { return d.Keys.GetEnumerator(); ; }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return d.Keys.GetEnumerator(); }

        public T[] ToArray()
        {
            List<T> result = new List<T>(d.Keys);
            return result.ToArray();
        }
    }

    internal class Algorithm
    {
        private const double BASE = 2.0d;
        private const string REGEX_WORDS = @"([a-zA-Z]+)";
        private static readonly Regex re = new Regex(REGEX_WORDS, RegexOptions.Compiled);

        internal Set<string> vocabulary = new Set<string>();
        internal Dictionary<string, Set<string>> docs= new Dictionary<string, Set<string>>();
        internal Dictionary<string, double> prior = new Dictionary<string, double>();
        internal Dictionary<string, Dictionary<string, double>> likelihood = new Dictionary<string, Dictionary<string, double>>();
        internal Dictionary<string, string> classification = new Dictionary<string, string>();

		public void LearnNaiveBayesText(Dictionary<string, string> examples, string[] labels) 
        {
			Dictionary<string, int> match = new Dictionary<string, int>(labels.Length);

            foreach (string label in labels) 
            {
                match.Add(label, 0);
                docs.Add(label, new Set<string>());
                likelihood.Add(label, new Dictionary<string, double>());
            }

            // have the examples split, got vocab, |doc_j|, and text for each v_j
            double exampleCount = (double)examples.Count;

            ICollection<string> words;
			foreach(KeyValuePair<string, string> entry in examples) 
            {
                words = GetWords(entry.Value);
                vocabulary.AddRange(words);
                foreach (string key in labels)
                {
                    if (entry.Key.Contains(key)) 
                    { 
                        match[key]++;
                        docs[key].AddRange(words);
                        docs[key].TotalWordCount += words.Count;
                    }
                    //if (j++ == 0) File.WriteAllLines(key + ".output.txt", Convert<string, int>(docs[key].D));
                }
			}

            // remove words
            Remove(0, labels);

            double pr;
            foreach (string key in labels)
            {
                // get my priors
                prior[key] = Math.Abs(Math.Log(match[key] / exampleCount, BASE));
                foreach (string word in docs[key])
                {
                    pr = CalcLikelihoodWithQ(docs[key][word], docs[key].TotalWordCount, word);
                    likelihood[key].Add(word, pr);
                }
            }

            //Console.WriteLine(vocabulary.Count);
            //File.WriteAllLines("politics.txt", ToStringArray<string, int>(docs["politics"].D));
            //File.WriteAllLines("medical.txt", ToStringArray<string, int>(docs["medical"].D));
            //File.WriteAllLines("priors.txt", ToStringArray<string, double>(prior));
            //File.WriteAllLines("vocab.txt", ToStringArray<string, int>(vocabulary.D));
        }

        private void Remove(int p, string[] labels)
        {
            List<KeyValuePair<string, int>> list = new List<KeyValuePair<string, int>>(vocabulary.D);
            list.Sort(delegate(KeyValuePair<string, int> e1, KeyValuePair<string, int> e2)
                { return e1.Value.CompareTo(e2.Value); });
            list.Reverse();
            for (int i = 0; i < p; i++)
            {
                vocabulary.Remove(list[i].Key);
                foreach (string key in labels)
                {
                    docs[key].Remove(list[i].Key);
                }
            }
        }

        public void Classify(Dictionary<string, string> tests, string[] labels)
        {
            Dictionary<string, double> heap;
            ICollection<string> positions;
            List<double> sorter;

            foreach (KeyValuePair<string, string> doc in tests)
            {
                positions = GetWords(doc.Value, true);
                heap = new Dictionary<string, double>();
                foreach (string key in labels)
                {
                    double v = prior[key];
                    foreach (string ai in positions) 
                        v += GetLogLikelihood(key, ai);
                    // add into heap
                    heap.Add(key, v);
                }
                //// todo, find a better sort
                sorter = new List<double>(heap.Values);
                sorter.Sort();
                foreach (KeyValuePair<string, double> entry in heap)
                {
                    if (entry.Value == sorter[0])
                        classification.Add(doc.Key, entry.Key);
                }
                //File.WriteAllLines("positions.txt", ToStringArray<string, int>(positions.D));
            }
            //File.WriteAllLines("classification.txt", ToStringArray<string, string>(classification));
        }

        private double GetLogLikelihood(string key, string ai)
        {
            // get my n, count duplicate words multiple times
            double result;
            if (likelihood[key].TryGetValue(ai, out result))
                return result;

            // should only be here if I don't have the word in my docs
            result = CalcLikelihoodWithQ(0, docs[key].TotalWordCount, ai);
            likelihood[key][ai] = result;
            return result;
        }

        private double CalcLikelihood(int nk, int n)
        {
            //return Math.Abs(Math.Log(((double)(nk + 1d) / (n + vocabulary.Count)), BASE));
            int m = 10;
            double result = Math.Abs(Math.Log(
                                (nk + ((double)m / vocabulary.Count))
                                / (n + m)
                            , BASE));
            return result;
        }
        private double CalcLikelihoodWithQ(int nk, int n, string w)
        {
            int m = vocabulary.Count;
            //double q = 1d/vocabulary.Count;
            double q = 0d;
            int num = 0, denom = 0;

            foreach (Set<string> doc in docs.Values)
            {
                num += doc.Contains(w) ? doc[w] : 0;
                denom += doc.TotalWordCount;
            }
            q = ((double)(num + 1) / (denom + vocabulary.Count));
            double result = Math.Abs(Math.Log(
                                ((double)n / (n + m)) * ((double)nk / n) 
                                + ((double)m / (n + m)) * q
                            , BASE));
            return result;
        }

        private ICollection<string> GetWords(string text) { return GetWords(text, false); }
        private ICollection<string> GetWords(string text, bool check)
        {
            string filter = text.ToLower();
            List<string> words = new List<string>();
            foreach(Match m in re.Matches(filter)) 
            {
                // additional filters
                if (m.Value.Length > 2 
                    &&(!check || (check && vocabulary.Contains(m.Value))))
                {
                    words.Add(m.Value);
                }
            }
            return words;
        }

        private static string[] ToStringArray<K, V>(IDictionary<K, V> dict)
        {
            string[] result = new string[dict.Count];
            int i = 0;
            foreach (KeyValuePair<K, V> entry in dict)
                result[i++] = String.Format("{0}:{1}", entry.Key, entry.Value);
            return result;
        }
    }
}

