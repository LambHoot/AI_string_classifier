using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_A1
{
    class Program
    {
        private static SortedSet<String> stops, vocab;
        private static List<int[]> features = new List<int[]>();

        static void Main(string[] args)
        {

            #region Preprocessing
            buildStopList();
            buildVocabulary();

            Console.WriteLine("{0} removed", vocab.RemoveWhere(x => stops.Contains(x)));

            buildFeatures();
            #endregion



            Console.WriteLine("fsaf");
        }

        #region Preprocessing Methods
        static public void buildStopList()
        {
            StreamReader r = new StreamReader("Resources/stoplist.txt");
            var fullFile = r.ReadToEnd();
            stops = new SortedSet<string>(fullFile.Split('\n').ToList<String>());
        }

        static public void buildVocabulary()
        {
            StreamReader r = new StreamReader("Resources/traindata.txt");
            var fullFile = r.ReadToEnd();
            vocab = new SortedSet<string>(fullFile.Split('\n', ' ').ToList<String>());
            vocab.Remove("");
        }

        static private void buildFeatures()
        {
            string[] data = File.ReadAllLines("Resources/traindata.txt");
            string[] labels = File.ReadAllLines("Resources/trainlabels.txt");
            
            StreamWriter writer = File.CreateText("Resources/preprocessed.txt");
            writer.WriteLine(string.Join(", ", vocab));

            for(int i=0; i<data.Count(); i++)
            {
                int[] flags = new int[vocab.Count + 1];
                for(int j = 0; j < vocab.Count(); j++)
                {
                    flags[j] = data[i].Contains(vocab.ElementAt(j)) ? 1 : 0;
                }
                flags[flags.Count() - 1] = Convert.ToInt32(labels[i]);

                features.Add(flags);
                writer.WriteLine(string.Join(", ", flags));
            }
        }
        #endregion
    }
}
