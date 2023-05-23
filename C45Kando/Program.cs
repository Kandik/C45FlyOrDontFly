using System.Diagnostics;


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
 * File: Program.cs
 * File containing main function
 * 
 */


namespace C45Kando
{
    public static partial class Program
    {
        //Main
        public static void Main(string[] args)
        {
            // Display the header
            Header();
            LineBreak();

            // Load weather data
            Console.WriteLine("Nacitam data o pocasi...");
            List<WeatherData>? weatherDataList = LoadWeatherData();
            if(weatherDataList is null)
            {
                // Handle failed weather data loading
                Console.Error.WriteLine("\nNacitanie data o pocasi zlyhalo.\n" +
                    "Skontrolujte, ci \"data\" priecinok obsahuje \"weatherdata.csv\".\n" +
                    "Program sa vypne.");
                WaitKey();
                return;
            }

            DateTime minDate = weatherDataList.Min(x => x.Date);
            DateTime maxDate = weatherDataList.Max(x => x.Date);

            // Display the weather data statistics
            Console.WriteLine($"\nData o pocasi uspesne nacitane.\nNacitanych {weatherDataList.Count} zaznamov od {minDate} do {maxDate}.");
            LineBreak();

            Console.WriteLine();
            LineBreak();

            // Load calendar data
            Console.WriteLine("Nacitam data z kalendara...");
            List<IcsData>? icsDataList = LoadIcsData();
            if(icsDataList is null)
            {
                // Handle failed calendar data loading
                Console.Error.WriteLine("\nNacitanie dat z kalendara zlyhalo.\n" +
                    "Skontrolujte, ci \"data\" priecinok obsahuje \"rezervacie letov Future Fly_rezervacieletov@gmail.com.ics\".\n" +
                    "Poznamka: Tento subor obsahuje citlive informacie a nie je sucastou odovzdania zadania.\n" +
                    "Program sa vypne.");
                WaitKey();
                return;
            }

            // Compose and filter the data
            List<TrainingData> dataList = ComposeData(weatherDataList, icsDataList);

            int trueCount = dataList.Count(data => data.Class == true);
            int falseCount = dataList.Count(data => data.Class == false);
            minDate = icsDataList.Min(x => x.Date);
            maxDate = icsDataList.Max(x => x.Date);

            // Display the calendar data statistics
            Console.WriteLine($"\nData z kalendara uspesne nacitane.\nNacitanych {icsDataList.Count} zaznamov od {minDate} do {maxDate}.");
            LineBreak();

            Console.WriteLine($"\nVyfiltrovany prienik obsahuje {dataList.Count} zaznamov,\n" +
                $"z toho {trueCount} pozitivnych a {falseCount} negativnych.");


            // Get user input for the number of positive and negative training examples
            int trueItemCount = 0;
            while (trueItemCount <= 0 || trueItemCount > trueCount)
            {
                Console.Write($"\nZadajte pocet pozitivnych prikladov do trenovacej mnoziny (1-{trueCount}): ");
                try
                {
                    trueItemCount = int.Parse(Console.ReadLine());
                }
                catch {}
            }

            int falseItemCount = 0;
            while (falseItemCount <= 0 || falseItemCount > falseCount)
            {
                Console.Write($"\nZadajte pocet negativnych prikladov do trenovacej mnoziny (1-{falseCount}): ");
                try
                {
                    falseItemCount = int.Parse(Console.ReadLine());
                }
                catch { }
            }

            // Create training and test sets
            List<TrainingData> positiveInstances = dataList.Where(x => x.Class == true).ToList();
            List<TrainingData> negativeInstances = dataList.Where(x => x.Class == false).ToList();

            Shuffle(positiveInstances);
            Shuffle(negativeInstances);

            List<TrainingData> trainingSet = positiveInstances.Take(trueItemCount).Concat(negativeInstances.Take(falseItemCount)).ToList();
            List<TrainingData> testSet = positiveInstances.Skip(trueItemCount).Concat(negativeInstances.Skip(falseItemCount)).ToList();

            Shuffle(trainingSet);
            Shuffle(testSet);


            Console.WriteLine();
            LineBreak();


            // Build the decision tree
            Console.WriteLine("Generujem strom...");

            C45Algorithm algorithm = new C45Algorithm();
            Stopwatch stopwatch = Stopwatch.StartNew();
            DecisionNode root = algorithm.BuildTree(trainingSet);
            stopwatch.Stop();

            Console.WriteLine($"Strom uspesne vygenerovany za {stopwatch.ElapsedMilliseconds} ms.");


            LineBreak();


            // Prepare and display diagnostics
            string foldername = DateTime.Now.ToString("yyMMdd-HHmmss");
            string diagnosticString = CreateDiagnosticString(foldername, trueItemCount, falseItemCount);
            

            Console.WriteLine("\nDIAGNOSTIKA");
            PrintDiagnostics("Trenovacie data", trainingSet, root, ref diagnosticString);
            PrintDiagnostics("Testovacie data", testSet, root, ref diagnosticString);
            PrintDiagnostics("Vsetky data", dataList, root, ref diagnosticString);

            LineBreak();

            // Generate output files
            Console.WriteLine("\nGenerujem subory...");

            Directory.CreateDirectory($"..\\..\\..\\data\\{foldername}");

            GenerateTXT(foldername, "Diagnostics", diagnosticString);

            string mermaidSyntax = GenerateMermaidSyntax(root);
            GenerateTXT(foldername, "Mermaid", mermaidSyntax);

            ExportDataToExcel(trainingSet, root, foldername, "TrainData");
            ExportDataToExcel(testSet, root, foldername, "TestData");
            ExportDataToExcel(dataList, root, foldername, "AllData");

            Console.WriteLine($"Vytvorenych 5 suborov v \"data/{foldername}\".\n");

            LineBreak();


            // Display instructions for tree visualization
            Console.WriteLine("\nGenerovanie dokoncene.\n\n" +
                "Pre vizualizaciu stromu skopirujte obsah \"Mermaid.txt\" na stranku \"https://mermaid.live\".\n\n");

            // Wait for user input before closing the console
            WaitKey();
        }


        /// <summary>
        /// Display the header information and a line break.
        /// </summary>
        public static void Header()
        {
            LineBreak();
            Console.WriteLine("Generator rozhodovacich stromov C4.5\n" +
                "Lietat/nelietat\n" +
                "Naprogramoval Stefan Kando");
            LineBreak();
            Console.WriteLine();
        }

        /// <summary>
        /// Create a diagnostic string containing folder name, true item count, and false item count.
        /// </summary>
        /// <param name="foldername">Name of the folder.</param>
        /// <param name="trueItemCount">Count of true items.</param>
        /// <param name="falseItemCount">Count of false items.</param>
        /// <returns>A string meant to contain diagnostic information.</returns>
        public static string CreateDiagnosticString(string foldername, int trueItemCount, int falseItemCount)
        {
            return $"Generovanie stromu z {foldername}\n\n" +
                $"Pouzite nastavenia:\n" +
                $"Pocet pozitivnych trenovacich prikladov: {trueItemCount}\n" +
                $"Pocet negativnych trenovacich prikladov: {falseItemCount}\n\n" +
                $"Diagnostika:\n";
        }

        /// <summary>
        /// Print diagnostics for a given dataset and update the diagnostic string.
        /// </summary>
        /// <param name="dataName">Name of the dataset.</param>
        /// <param name="dataset">List of training data.</param>
        /// <param name="tree">Decision tree.</param>
        /// <param name="DiagnosticString">Reference to the diagnostic string to be updated.</param>
        public static void PrintDiagnostics(string dataName, List<TrainingData> dataset, DecisionNode tree, ref string DiagnosticString)
        {
            double accuracy = CalculateAccuracy(dataset, tree);
            (double precision, double recall, double f1) = CalculatePrecisionRecallF1(dataset, tree);

            string output = $"\n{dataName}:\n" +
                $"Presnost: {Math.Round(accuracy * 100, 2)}%\n" +
                $"Spravnost: {Math.Round(precision * 100, 2)}%\n" +
                $"Navratnost: {Math.Round(recall * 100, 2)}%\n" +
                $"F1: {Math.Round(f1 * 100, 2)}%\n";
            
            Console.Write(output);
            DiagnosticString += output;
        }

        /// <summary>
        /// Wait for the user to press any key to end the program.
        /// </summary>
        public static void WaitKey()
        {
            Console.WriteLine("Stlacte akukolvek klavesu pre ukoncenie...");
            Console.ReadKey(true);
        }

        public static void LineBreak()
        {
            Console.WriteLine(new string('-', 30));
        }
    }
}
