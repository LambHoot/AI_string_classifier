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
        private static List<int[]> trainedFeatures = new List<int[]>();
        private static List<int[]> testFeatures = new List<int[]>();

        static void Main(string[] args)
        {
            string
                stopFile = args[0],
                trainingDataFile = args[1],
                trainingLabelFile = args[2],
                testingDataFile = args[3],
                testingLabelFile = args[4];

            Console.WriteLine("Creating preprocessed files...\n");
            stops = buildSetFromFile(stopFile);
            vocab = buildSetFromFile(trainingDataFile);
            vocab.Remove("");

            Console.WriteLine("{0} stop words removed.", vocab.RemoveWhere(x => stops.Contains(x)));

            //create feature list
            buildTrainFeatures(trainingDataFile, trainingLabelFile);

            buildTestingFeatures(testingDataFile, testingLabelFile);

            //test against training -> high accuracy
            float trainedAccuracy = bayes(trainedFeatures);
            float testAccuracy = bayes(testFeatures);

            StreamWriter writer = File.CreateText("Resources/results.txt");
            writer.WriteLine("Aassignment 1 q8 -  Training and Testing\nJacob Desrochers\tDenis Kefallinos\tPhilippe Miriello\n");
            writer.WriteLine("Trained Accuracy: {0}\nFiles used: {1}, {2}", trainedAccuracy, trainingDataFile, trainingLabelFile);
            writer.WriteLine("Test Accuracy: {0}\nFiles used: {1}, {2}", testAccuracy, testingDataFile, testingLabelFile);
            writer.WriteLine("Thank you!");
            writer.Close();
        }

        #region Preprocessing Methods
        static public SortedSet<string> buildSetFromFile(string file)
        {
            StreamReader r = new StreamReader(file);
            var fullFile = r.ReadToEnd();
            return new SortedSet<string>(fullFile.Split(new string[] { "\n", "\r\n", " " }, StringSplitOptions.RemoveEmptyEntries).ToList<String>());
        }

        static private void buildTrainFeatures(string dataFile, string labelsFile)
        {
            string[] data = File.ReadAllLines(dataFile);
            string[] labels = File.ReadAllLines(labelsFile);

            StreamWriter writer = File.CreateText("Resources/preprocessed.txt");
            writer.WriteLine(string.Join(", ", vocab));

            for (int i = 0; i < data.Count(); i++)
            {
                int[] flags = new int[vocab.Count + 1];
                for (int j = 0; j < vocab.Count(); j++)
                {
                    flags[j] = data[i].Contains(vocab.ElementAt(j)) ? 1 : 0;
                }
                flags[flags.Count() - 1] = Convert.ToInt32(labels[i]);

                trainedFeatures.Add(flags);
                writer.WriteLine(string.Join(", ", flags));
            }
        }
        #endregion

        static private void buildTestingFeatures(string dataFile, string labelsFile)
        {
            string[] data = File.ReadAllLines(dataFile);
            string[] labels = File.ReadAllLines(labelsFile);

            for (int i = 0; i < data.Count(); i++)
            {
                int[] flags = new int[vocab.Count + 1];
                for (int j = 0; j < vocab.Count(); j++)
                {
                    flags[j] = data[i].Contains(vocab.ElementAt(j)) ? 1 : 0;
                }
                flags[flags.Count() - 1] = Convert.ToInt32(labels[i]);

                testFeatures.Add(flags);
            }
        }

        //returns the accuracy of this
        static private float bayes(List<int[]> features)
        {
            SortedDictionary<int, float[]> probD = new SortedDictionary<int, float[]>();
            probD.Add(0, new float[vocab.Count()]);
            probD.Add(1, new float[vocab.Count()]);
            int[] numWordsPerClass = new int[2];
            for (int k = 0; k < vocab.Count(); k++)
            {
                //frequency of word in classes
                for (int l = 0; l < features.Count(); l++)
                {
                    if (features.ElementAt(l)[k] == 1)
                    {//word has been found
                        probD[features.ElementAt(l)[vocab.Count()]][k] += 1;
                    }
                }
            }

            List<int[]> sentences0 = features.Where(x => x.ElementAt(vocab.Count()) == 0).ToList();
            for (int u = 0; u < sentences0[0].Count() - 1; u++)
            {
                foreach (int[] sentence in sentences0)
                {
                    if (sentence[u] != 0)
                    {
                        numWordsPerClass[0]++;
                        break;
                    }
                }
            }

            List<int[]> sentences1 = features.Where(x => x.ElementAt(vocab.Count()) == 1).ToList();
            for (int u = 0; u < sentences1[0].Count() - 1; u++)
            {
                foreach (int[] sentence in sentences1)
                {
                    if (sentence[u] != 0)
                    {
                        numWordsPerClass[1]++;
                        break;
                    }
                }
            }

            for (int k = 0; k < probD[0].Count(); k++)
            {
                probD[0][k] = (probD[0][k] + 1) / (numWordsPerClass[0] + vocab.Count());
                probD[1][k] = (probD[1][k] + 1) / (numWordsPerClass[1] + vocab.Count());
            }

            float[] probOfClasses = new float[2];
            probOfClasses[0] = numWordsPerClass[0] / (float)vocab.Count();
            probOfClasses[1] = numWordsPerClass[1] / (float)vocab.Count();
            float[] totalScores = { 0f, 0f };
            int correct = 0;

            //get score for 0
            foreach (int[] wordsUsed in features)
            {
                totalScores[0] = 0f;
                totalScores[1] = 0f;
                for (int j = 0; j < wordsUsed.Count() - 1; j++)
                {
                    if (wordsUsed[j] == 1)
                    {
                        //find score for word in probD
                        totalScores[0] += probOfClasses[0] * probD[0][j];
                        totalScores[1] += probOfClasses[1] * probD[1][j];
                    }
                }
                //compare to actual
                if (Math.Max(totalScores[0], totalScores[1]) == totalScores[0])
                {
                    if (wordsUsed[wordsUsed.Count() - 1] == 0)
                        correct++;
                }
                if (Math.Max(totalScores[0], totalScores[1]) == totalScores[1])
                {
                    if (wordsUsed[wordsUsed.Count() - 1] == 1)
                        correct++;
                }
            }
            return correct / (float)features.Count();
        }

    }
}