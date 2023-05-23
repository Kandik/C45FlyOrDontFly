using System.Text;
using OfficeOpenXml;
using OfficeOpenXml.Style;


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
 * File: DataExport.cs
 * File containing functions for data exporting
 * 
 */


namespace C45Kando
{
    public static partial class Program
    {
        /// <summary>
        /// Generates a text file with the given data and saves it in the specified folder and filename.
        /// </summary>
        /// <param name="foldername">The name of the folder to save the file in</param>
        /// <param name="filename">The name of the file to be saved</param>
        /// <param name="data">The data to be written to the file</param>
        public static void GenerateTXT(string foldername, string filename, string data)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter($"..\\..\\..\\data\\{foldername}\\{filename}.txt"))
                {
                    // Write the string to the file.
                    writer.Write(data);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
        }


        /// <summary>
        /// Generates a Mermaid syntax string representation of the given decision tree.
        /// </summary>
        /// <param name="node">The root node of the decision tree</param>
        /// <returns>The Mermaid syntax string representation of the decision tree</returns>
        public static string GenerateMermaidSyntax(DecisionNode node)
        {
            StringBuilder mermaidSyntax = new StringBuilder();
            mermaidSyntax.AppendLine("graph TD");
            GenerateNodeMermaidSyntax(node, mermaidSyntax, 1);
            return mermaidSyntax.ToString();
        }


        /// <summary>
        /// Recursively generates the Mermaid syntax for the given node and its descendants.
        /// </summary>
        /// <param name="node">The current node</param>
        /// <param name="mermaidSyntax">The StringBuilder to append the generated syntax</param>
        /// <param name="nodeId">The current node's ID</param>
        /// <returns>The ID of the last processed node</returns>
        private static int GenerateNodeMermaidSyntax(DecisionNode node, StringBuilder mermaidSyntax, int nodeId)
        {
            int currentNodeId = nodeId;

            if (node is DecisionLeaf leaf)
            {
                mermaidSyntax.AppendLine($"{currentNodeId}[{GetClassValue(leaf.Class)}]");
            }
            else if (node is DecisionSplit split)
            {
                int leftNodeId = currentNodeId + 1;
                int rightNodeId = GenerateNodeMermaidSyntax(split.LeftChild, mermaidSyntax, leftNodeId) + 1;

                // Round the split values to two decimal places in the Mermaid syntax.
                double roundedSplitValue = Math.Round(split.SplitValue, 2);
                mermaidSyntax.AppendLine($"{currentNodeId}{{\"{GetAttributeNameByIndex(split.AttributeIndex)} <= {roundedSplitValue}\" ?}}");
                mermaidSyntax.AppendLine($"{currentNodeId}-- Áno -->{leftNodeId}");
                mermaidSyntax.AppendLine($"{currentNodeId}-- Nie -->{rightNodeId}");

                currentNodeId = GenerateNodeMermaidSyntax(split.RightChild, mermaidSyntax, rightNodeId);
            }

            return currentNodeId;
        }


        /// <summary>
        /// Gets a string interpretation of an attribute given its index.
        /// </summary>
        /// <param name="index">Index of an attribute</param>
        /// <returns>String name of an attribute</returns>
        public static string GetAttributeNameByIndex(int index)
        {
            switch (index)
            {
                case 0: return "Maximálna teplota (°C)";
                case 1: return "Oblačnosť (%)";
                case 2: return "Zrážky za 24 hodín (mm)";
                case 3: return "Viditeľnosť (m)";
                case 4: return "Smer vetra (°)";
                case 5: return "Rýchlosť vetra (kt)";
                default: return "";
            }
        }


        /// <summary>
        /// Converts binary class value to a string
        /// </summary>
        /// <param name="Class">Class value</param>
        /// <returns>"Lietať" if true, "Nelietať" if false</returns>
        public static string GetClassValue(bool Class)
        {
            if (Class)
            {
                return "Lietať";
            }
            return "Nelietať";
        }


        /// <summary>
        /// Exports the given training data and decision tree predictions to an Excel file.
        /// </summary>
        /// <param name="data">The list of training data instances</param>
        /// <param name="tree">The decision tree used for making predictions</param>
        /// <param name="foldername">The name of the folder to save the Excel file in</param>
        /// <param name="filename">The name of the Excel file to be saved</param>
        public static void ExportDataToExcel(List<TrainingData> data, DecisionNode tree, string foldername, string filename)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("TrainingData");

                // Write the header row.
                for (int i = 0; i < 6; i++)
                {
                    worksheet.Cells[1, i + 1].Value = GetAttributeNameByIndex(i);
                }

                worksheet.Cells[1, 7].Value = "Trieda";
                worksheet.Cells[1, 8].Value = "Vypočítaná trieda";

                // Write the data rows.
                for (int i = 0; i < data.Count; i++)
                {
                    TrainingData rowData = data[i];
                    worksheet.Cells[i + 2, 1].Value = rowData.MaxTemp + "°C";
                    worksheet.Cells[i + 2, 2].Value = rowData.EffectiveCloudCover + "%";
                    worksheet.Cells[i + 2, 3].Value = rowData.Precip24h + " mm";
                    worksheet.Cells[i + 2, 4].Value = rowData.Visibility + " m";
                    worksheet.Cells[i + 2, 5].Value = rowData.WindDir10m + "°";
                    worksheet.Cells[i + 2, 6].Value = rowData.WindSpeed10m + " kt";
                    worksheet.Cells[i + 2, 7].Value = GetClassValue(rowData.Class);

                    // Calculate the class using the decision tree.
                    bool calculatedClass = bool.Parse(tree.Predict(rowData));
                    worksheet.Cells[i + 2, 8].Value = GetClassValue(calculatedClass);

                    // Highlight the row if the prediction is incorrect.
                    if (rowData.Class != calculatedClass)
                    {
                        worksheet.Cells[i + 2, 1, i + 2, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[i + 2, 1, i + 2, 8].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                    }
                }

                try
                {
                    // Save the Excel file.
                    using (FileStream fileStream = new FileStream($"..\\..\\..\\data\\{foldername}\\{filename}.xlsx", FileMode.Create))
                    {
                        package.SaveAs(fileStream);
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.ToString());
                }

            }
        }
    }
}
