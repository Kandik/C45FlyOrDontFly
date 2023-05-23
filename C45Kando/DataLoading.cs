using System.Globalization;
using System.Text.RegularExpressions;


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
 * File: DataLoading.cs
 * File containing functions for data loading
 * 
 */


namespace C45Kando
{
    public static partial class Program
    {
        /// <summary>
        /// Load weather data from CSV file into a list of WeatherData objects.
        /// </summary>
        /// <returns>A list of WeatherData objects or null if an exception occurs.</returns>
        public static List<WeatherData>? LoadWeatherData()
        {
            List<WeatherData>? weatherDataList = new List<WeatherData>();

            string filePath = @"..\..\..\data\weatherdata.csv";

            try
            {
                using (var reader = new StreamReader(filePath))
                {
                    string[] headers = reader.ReadLine().Split(';');

                    // Read each line of the CSV and create WeatherData objects
                    while (!reader.EndOfStream)
                    {
                        string[] values = reader.ReadLine().Split(';');

                        WeatherData weatherData = new WeatherData();
                        weatherData.Date = DateTimeOffset.ParseExact(values[0].Substring(0, values[0].Length - 6),
                            "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture).DateTime;
                        weatherData.MaxTemp = double.Parse(values[1], CultureInfo.InvariantCulture);
                        weatherData.EffectiveCloudCover = double.Parse(values[2], CultureInfo.InvariantCulture);
                        weatherData.Precip24h = double.Parse(values[3], CultureInfo.InvariantCulture);

                        // Handle visibility data
                        if (double.Parse(values[4], CultureInfo.InvariantCulture) >= 9999)
                        {
                            weatherData.Visibility = 9999;
                        }
                        else
                        {
                            weatherData.Visibility = double.Parse(values[4], CultureInfo.InvariantCulture);
                        }

                        weatherData.WindDir10m = double.Parse(values[5], CultureInfo.InvariantCulture);
                        weatherData.WindSpeed10m = double.Parse(values[6], CultureInfo.InvariantCulture);

                        weatherDataList.Add(weatherData);
                    }
                }

                return weatherDataList;
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return null;
            }
        }


        /// <summary>
        /// Load ICS data from the .ics file into a list of IcsData objects.
        /// </summary>
        /// <returns>A list of IcsData objects or null if an exception occurs.</returns>
        public static List<IcsData>? LoadIcsData()
        {
            List<IcsData>? icsDataList = new List<IcsData>();

            string filePath = @"..\..\..\data\rezervacie letov Future Fly_rezervacieletov@gmail.com.ics";

            string[]? lines;
            try
            {
                lines = File.ReadAllLines(filePath);
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return null;
            }

            // Read each line of the ICS file and create IcsData objects
            foreach (string line in lines)
            {
                if (line.StartsWith("BEGIN:VEVENT"))
                {
                    IcsData icsData = new IcsData();
                    icsDataList.Add(icsData);
                }
                else if (line.StartsWith("SUMMARY:"))
                {
                    icsDataList.Last().Summary = line.Substring(8);
                }
                else if (line.StartsWith("DTSTART:"))
                {
                    DateTime startDate;
                    if (DateTime.TryParseExact(line.Substring(8), "yyyyMMddTHHmmssZ", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out startDate))
                    {
                        icsDataList.Last().Date = startDate;
                    }
                }
            }

            return icsDataList;
        }


        /// <summary>
        /// Find IcsData objects with summaries matching the specified pattern.
        /// </summary>
        /// <param name="icsDataList">A list of IcsData objects.</param>
        /// <param name="pattern">The regex pattern to match.</param>
        /// <returns>A list of IcsData objects with summaries matching the pattern.</returns>
        public static List<IcsData> FindPatternInSummaries(List<IcsData> icsDataList, string pattern)
        {
            List<IcsData> matchingIcsDataList = new List<IcsData>();

            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            foreach (IcsData icsData in icsDataList)
            {
                if (regex.IsMatch(icsData.Summary))
                {
                    matchingIcsDataList.Add(icsData);
                }
            }

            return matchingIcsDataList;
        }


        /// <summary>
        /// Compose a list of TrainingData objects using the provided weather and ICS data.
        /// </summary>
        /// <param name="weatherDataList">A list of WeatherData objects.</param>
        /// <param name="icsDataList">A list of IcsData objects.</param>
        /// <returns>A list of TrainingData objects.</returns>
        public static List<TrainingData> ComposeData(List<WeatherData> weatherDataList, List<IcsData> icsDataList)
        {
            List<TrainingData> dataList = new List<TrainingData>();

            //Filter the data by date
            DateTime minDate = weatherDataList.Min(x => x.Date);
            DateTime maxDate = weatherDataList.Max(x => x.Date);
            List<IcsData> filteredIcsDataList = icsDataList.Where(icsData => icsData.Date >= minDate && icsData.Date <= maxDate).ToList();

            //Divide the data to positive and negative classes using Regex
            List<IcsData> flown = FindPatternInSummaries(filteredIcsDataList, "^FF");
            List<IcsData> cancelled = FindPatternInSummaries(filteredIcsDataList, "zrušené");

            //Iterating over weather data and finding intersection
            foreach (WeatherData weatherData in weatherDataList)
            {
                // Check if any IcsData objects correspond to this WeatherData object
                List<IcsData> flownForWeatherData = flown.Where(x => x.Date.Date == weatherData.Date.Date).ToList();
                List<IcsData> cancelledForWeatherData = cancelled.Where(x => x.Date.Date == weatherData.Date.Date).ToList();

                // Create a new TrainingData object only if there are matching IcsData objects
                if (flownForWeatherData.Count > 0 || cancelledForWeatherData.Count > 0)
                {
                    TrainingData trainingData = new TrainingData()
                    {
                        MaxTemp = weatherData.MaxTemp,
                        EffectiveCloudCover = weatherData.EffectiveCloudCover,
                        Precip24h = weatherData.Precip24h,
                        Visibility = weatherData.Visibility,
                        WindDir10m = weatherData.WindDir10m,
                        WindSpeed10m = weatherData.WindSpeed10m,
                        Class = cancelledForWeatherData.Count == 0 ? true : false
                    };

                    dataList.Add(trainingData);
                }
            }

            return dataList;
        }


        /// <summary>
        /// Shuffle the elements of a list using the Fisher-Yates algorithm.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the list.</typeparam>
        /// <param name="list">The list to shuffle.</param>
        private static void Shuffle<T>(List<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
