

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
 * File: C45Classes.cs
 * File containing classes used for C4.5 decision tree
 * 
 */


namespace C45Kando
{
    /// <summary>
    /// The abstract DecisionNode class serves as a base for both leaf and split nodes in the decision tree.
    /// </summary>
    public abstract class DecisionNode
    {
        public abstract string Predict(TrainingData data);
    }

    /// <summary>
    /// The DecisionLeaf class represents a leaf node in the decision tree.
    /// It contains a boolean property that represents the classification decision.
    /// </summary>
    public class DecisionLeaf : DecisionNode
    {
        public bool Class { get; set; }

        public DecisionLeaf(bool cls)
        {
            Class = cls;
        }

        /// <summary>
        /// Returns the classification decision as a string.
        /// </summary>
        /// <param name="data">A TrainingData object</param>
        /// <returns>The classification decision as a string</returns>
        public override string Predict(TrainingData data)
        {
            return Class.ToString();
        }
    }

    /// <summary>
    /// The DecisionSplit class represents an internal node in the decision tree.
    /// It contains the attribute index, split value, and left and right children to traverse the tree.
    /// </summary>
    public class DecisionSplit : DecisionNode
    {
        public int AttributeIndex { get; set; }
        public double SplitValue { get; set; }
        public DecisionNode LeftChild { get; set; }
        public DecisionNode RightChild { get; set; }

        public DecisionSplit(int attributeIndex, double splitValue, DecisionNode leftChild, DecisionNode rightChild)
        {
            AttributeIndex = attributeIndex;
            SplitValue = splitValue;
            LeftChild = leftChild;
            RightChild = rightChild;
        }

        /// <summary>
        /// Predicts the class of the given data by traversing the tree.
        /// </summary>
        /// <param name="data">A TrainingData object</param>
        /// <returns>The predicted class as a string</returns>
        public override string Predict(TrainingData data)
        {
            double value = GetAttributeValueByIndex(data, AttributeIndex);
            return value <= SplitValue ? LeftChild.Predict(data) : RightChild.Predict(data);
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
