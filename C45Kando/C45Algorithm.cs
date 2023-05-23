

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
 * File: C45Algorithm.cs
 * File containing C4.5 implementation
 * 
 */


namespace C45Kando
{
    public class C45Algorithm
    {
        // Define a threshold for stopping the recursion.
        private const int MinSamplesSplit = 5;

        /// <summary>
        /// Builds a decision tree from the provided training data using the C4.5 algorithm.
        /// </summary>
        /// <param name="data">A list of TrainingData objects</param>
        /// <returns>The root DecisionNode of the built decision tree</returns>
        public DecisionNode BuildTree(List<TrainingData> data)
        {
            // Base case: If the data has less than the minimum number of samples or is pure (all samples have the same class),
            // create a leaf node with the majority class.
            if (data.Count < MinSamplesSplit || IsPure(data))
            {
                return new DecisionLeaf(MajorityClass(data));
            }

            // Find the best attribute and split value to split the data on.
            (int attributeIndex, double splitValue) = FindBestSplit(data);

            // Split the data into two groups based on the split value.
            var leftData = new List<TrainingData>();
            var rightData = new List<TrainingData>();

            foreach (var item in data)
            {
                double value = GetAttributeValueByIndex(item, attributeIndex);
                if (value <= splitValue)
                {
                    leftData.Add(item);
                }
                else
                {
                    rightData.Add(item);
                }
            }

            // Recursively build the left and right subtrees.
            DecisionNode leftChild = BuildTree(leftData);
            DecisionNode rightChild = BuildTree(rightData);

            // Delete the node if both child nodes are same.
            if (leftChild is DecisionLeaf leftLeaf && rightChild is DecisionLeaf rightLeaf && leftLeaf.Class == rightLeaf.Class)
            {
                return new DecisionLeaf(leftLeaf.Class);
            }

            // Create a new decision split node with the best attribute index, split value, and child nodes.
            return new DecisionSplit(attributeIndex, Math.Round(splitValue, 2), leftChild, rightChild);
        }


        /// <summary>
        /// Checks if the given data is pure (all samples have the same class).
        /// </summary>
        /// <param name="data">A list of TrainingData objects</param>
        /// <returns>True if the data is pure, false otherwise</returns>
        private bool IsPure(List<TrainingData> data)
        {
            bool firstClass = data[0].Class;
            return data.All(d => d.Class == firstClass);
        }

        /// <summary>
        /// Determines the majority class for the given data.
        /// </summary>
        /// <param name="data">A list of TrainingData objects</param>
        /// <returns>True if the majority class is positive, false otherwise</returns>
        private bool MajorityClass(List<TrainingData> data)
        {
            int positiveCount = data.Count(d => d.Class);
            int negativeCount = data.Count - positiveCount;
            return positiveCount >= negativeCount;
        }


        /// <summary>
        /// Finds the best attribute index and split value for the given data.
        /// </summary>
        /// <param name="data">A list of TrainingData objects</param>
        /// <returns>A tuple containing the best attribute index and split value</returns>
        private (int attributeIndex, double splitValue) FindBestSplit(List<TrainingData> data)
        {
            int attributeCount = 6; // The number of attributes in the TrainingData class.
            double bestInfoGain = double.MinValue;
            int bestAttributeIndex = -1;
            double bestSplitValue = double.NaN;

            // Iterate through each attribute index to find the one that provides the best split.
            for (int attributeIndex = 0; attributeIndex < attributeCount; attributeIndex++)
            {
                double currentSplitValue = GetBestSplitValueForAttribute(data, attributeIndex);
                double currentInfoGain = CalculateInfoGain(data, attributeIndex, currentSplitValue);

                // Update the best values if the current attribute provides a better split.
                if (currentInfoGain > bestInfoGain)
                {
                    bestInfoGain = currentInfoGain;
                    bestAttributeIndex = attributeIndex;
                    bestSplitValue = currentSplitValue;
                }
            }

            return (bestAttributeIndex, bestSplitValue);
        }


        /// <summary>
        /// Finds the best split value for the given attribute index in the data.
        /// </summary>
        /// <param name="data">A list of TrainingData objects</param>
        /// <param name="attributeIndex">The index of the attribute to find the best split value for</param>
        /// <returns>The best split value for the attribute</returns>
        private double GetBestSplitValueForAttribute(List<TrainingData> data, int attributeIndex)
        {
            // Sort the data based on the attribute value.
            var sortedData = data.OrderBy(d => GetAttributeValueByIndex(d, attributeIndex)).ToList();
            double bestSplitValue = double.MinValue;
            double bestInfoGain = double.MinValue;

            // Iterate through the sorted data to find the best split value.
            for (int i = 0; i < sortedData.Count - 1; i++)
            {
                double currentValue = GetAttributeValueByIndex(sortedData[i], attributeIndex);
                double nextValue = GetAttributeValueByIndex(sortedData[i + 1], attributeIndex);

                // If the current value is equal to the next value, continue to the next iteration.
                if (currentValue == nextValue)
                {
                    continue;
                }

                // Calculate the current split value and information gain.
                double currentSplitValue = (currentValue + nextValue) / 2;
                double currentInfoGain = CalculateInfoGain(sortedData, attributeIndex, currentSplitValue);

                // Update the best values if the current split provides a better information gain.
                if (currentInfoGain > bestInfoGain)
                {
                    bestInfoGain = currentInfoGain;
                    bestSplitValue = currentSplitValue;
                }
            }

            return bestSplitValue;
        }

        /// <summary>
        /// Calculates the information gain for the given attribute index and split value in the data.
        /// </summary>
        /// <param name="data">A list of TrainingData objects</param>
        /// <param name="attributeIndex">The index of the attribute to calculate the information gain for</param>
        /// <param name="splitValue">The value to split the data on</param>
        /// <returns>The calculated information gain</returns>
        private double CalculateInfoGain(List<TrainingData> data, int attributeIndex, double splitValue)
        {
            double initialEntropy = CalculateEntropy(data);
            int totalCount = data.Count;

            var leftData = new List<TrainingData>();
            var rightData = new List<TrainingData>();

            // Split the data based on the split value.
            foreach (var item in data)
            {
                double value = GetAttributeValueByIndex(item, attributeIndex);
                if (value <= splitValue)
                {
                    leftData.Add(item);
                }
                else
                {
                    rightData.Add(item);
                }
            }

            // Calculate the entropy for both the left and right data groups.
            double leftEntropy = CalculateEntropy(leftData);
            double rightEntropy = CalculateEntropy(rightData);

            // Calculate the weighted entropy using the left and right data group entropies.
            double weightedEntropy = (leftData.Count / (double)totalCount) * leftEntropy + (rightData.Count / (double)totalCount) * rightEntropy;

            // Calculate and return the information gain by subtracting the weighted entropy from the initial entropy.
            return initialEntropy - weightedEntropy;
        }

        /// <summary>
        /// Calculates the entropy of the given data.
        /// </summary>
        /// <param name="data">A list of TrainingData objects</param>
        /// <returns>The calculated entropy</returns>
        private double CalculateEntropy(List<TrainingData> data)
        {
            int totalCount = data.Count;
            int positiveCount = data.Count(d => d.Class);
            int negativeCount = totalCount - positiveCount;

            // Calculate the probability of positive and negative classes.
            double positiveProb = positiveCount / (double)totalCount;
            double negativeProb = negativeCount / (double)totalCount;

            // Calculate and return the entropy using the probabilities.
            return -positiveProb * Log2(positiveProb) - negativeProb * Log2(negativeProb);
        }

        /// <summary>
        /// Calculates the base-2 logarithm of a given number.
        /// </summary>
        /// <param name="x">The number to calculate the base-2 logarithm for</param>
        /// <returns>The calculated base-2 logarithm</returns>
        private double Log2(double x)
        {
            if (x == 0)
            {
                return 0;
            }
            return Math.Log(x) / Math.Log(2);
        }

        /// <summary>
        /// Retrieves the attribute value from the TrainingData object based on the given index.
        /// </summary>
        /// <param name="data">A TrainingData object</param>
        /// <param name="index">The index of the attribute to retrieve</param>
        /// <returns>The attribute value</returns>
        private double GetAttributeValueByIndex(TrainingData data, int index)
        {
            switch (index)
            {
                case 0: return data.MaxTemp;
                case 1: return data.EffectiveCloudCover;
                case 2: return data.Precip24h;
                case 3: return data.Visibility;
                case 4: return data.WindDir10m;
                case 5: return data.WindSpeed10m;
                default: throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

    }
}
