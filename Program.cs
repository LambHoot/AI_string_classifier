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
        private static List<int[]> testFeatures = new List<int[]>();

        static void Main(string[] args)
        {

            #region Preprocessing
            buildStopList();
            buildVocabulary();

            Console.WriteLine("{0} removed", vocab.RemoveWhere(x => stops.Contains(x)));

            buildFeatures();
            #endregion

            //create testing feature list
            buildTestingFeatures();

            //test against training -> high accuracy
            bayes();

            //test against test -> print compiled accuracy

            //print compiled accuracy -> results .txt
        }

        #region Preprocessing Methods
        static public void buildStopList()
        {
            StreamReader r = new StreamReader("Resources/stoplist.txt");
            var fullFile = r.ReadToEnd();
            stops = new SortedSet<string>(fullFile.Split(new string[] { "\n", "\r\n"}, StringSplitOptions.RemoveEmptyEntries).ToList<String>());
        }

        static public void buildVocabulary()
        {
            StreamReader r = new StreamReader("Resources/traindata.txt");
            var fullFile = r.ReadToEnd();
            vocab = new SortedSet<string>(fullFile.Split(new string[] { "\n", "\r\n", " "}, StringSplitOptions.RemoveEmptyEntries).ToList<String>());
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

        static private void buildTestingFeatures()
        {
            string[] data = File.ReadAllLines("Resources/testdata.txt");

            for (int i = 0; i < data.Count(); i++)
            {
                int[] flags = new int[vocab.Count + 1];
                for (int j = 0; j < vocab.Count(); j++)
                {
                    flags[j] = data[i].Contains(vocab.ElementAt(j)) ? 1 : 0;
                }
                testFeatures.Add(flags);
            }
        }

        static private void bayes()
        {
            SortedDictionary<int, float[]> probD = new SortedDictionary<int, float[]>();
            probD.Add(0, new float[vocab.Count()]);
            probD.Add(1, new float[vocab.Count()]);
            int[] numWordsPerClass = new int[2];
            for (int k = 0; k < vocab.Count(); k++)
            {
                //frequency of word in classes
                for(int l = 0; l < features.Count(); l++)
                {
                    if(features.ElementAt(l)[k] == 1)
                    {//word has been found
                        probD[features.ElementAt(l)[vocab.Count()]][k] += 1;
                        numWordsPerClass[features.ElementAt(l)[vocab.Count()]] += 1;
                    }
                }
            }

            for (int k = 0; k < probD[0].Count(); k++)
            {
                probD[0][k] = (probD[0][k] + 1) / (numWordsPerClass[0] + vocab.Count());
                probD[1][k] = (probD[1][k] + 1) / (numWordsPerClass[1] + vocab.Count());
            }

        }

    }
}
