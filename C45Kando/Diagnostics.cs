

/* 
 * 
 * █▀▀ █░█ ░ █▀   █▀▄ █▀▀ █▀▀ █ █▀ █ █▀█ █▄░█   ▀█▀ █▀█ █▀▀ █▀▀   █▀▀ █▀▀ █▄░█ █▀▀ █▀█ ▄▀█ ▀█▀ █▀█ █▀█
 * █▄▄ ▀▀█ ▄ ▄█   █▄▀ ██▄ █▄▄ █ ▄█ █ █▄█ █░▀█   ░█░ █▀▄ ██▄ ██▄   █▄█ ██▄ █░▀█ ██▄ █▀▄ █▀█ ░█░ █▄█ █▀▄
 * 
 * C4.5 DECISION TREE GENERATOR
 * 
 * FLY / DON'T FLY
 * 
 * Made by Štefan Kando
 * Machine Learning course 2023
 * Technical University of Košice
 * 
 * File: Diagnostics.cs
 * File containing functions for calculating diagnostics of a decision tree
 * 
 */


namespace C45Kando
{
    public static partial class Program
    {
        /// <summary>
        /// Calculate the accuracy of the decision tree on the provided data.
        /// </summary>
        /// <param name="data">A list of TrainingData objects.</param>
        /// <param name="tree">The decision tree to test.</param>
        /// <returns>The accuracy of the decision tree as a double.</returns>
        public static double CalculateAccuracy(List<TrainingData> data, DecisionNode tree)
        {
            int correctPredictions = 0;

            foreach (TrainingData item in data)
            {
                bool predictedClass = bool.Parse(tree.Predict(item));

                if (predictedClass == item.Class)
                {
                    correctPredictions++;
                }
            }

            return (double)correctPredictions / data.Count;
        }


        /// <summary>
        /// Calculate the precision, recall, and F1 score of the decision tree on the provided data.
        /// </summary>
        /// <param name="data">A list of TrainingData objects.</param>
        /// <param name="tree">The decision tree to test.</param>
        /// <returns>A tuple containing the precision, recall, and F1 score as doubles.</returns>
        public static (double precision, double recall, double f1) CalculatePrecisionRecallF1(List<TrainingData> data, DecisionNode tree)
        {
            int truePositive = 0;
            int falsePositive = 0;
            int falseNegative = 0;

            //Iterating over dataset
            foreach (TrainingData item in data)
            {
                bool predictedClass = bool.Parse(tree.Predict(item));

                if (predictedClass == true && item.Class == true)
                {
                    truePositive++;
                }
                else if (predictedClass == true && item.Class == false)
                {
                    falsePositive++;
                }
                else if (predictedClass == false && item.Class == true)
                {
                    falseNegative++;
                }
            }

            //Calculating diagnostics
            double precision = (double)truePositive / (truePositive + falsePositive);
            double recall = (double)truePositive / (truePositive + falseNegative);
            double f1 = 2 * ((precision * recall) / (precision + recall));

            return (precision, recall, f1);
        }
    }
}
