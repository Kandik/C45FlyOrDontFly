

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
 * File: DataClasses.cs
 * File containing data framework classes
 * 
 */


namespace C45Kando
{
    /// <summary>
    /// Weather data framework for import from "weatherdata.csv"
    /// </summary>
    public class WeatherData
    {
        public DateTime Date { get; set; }
        public double MaxTemp { get; set; }
        public double EffectiveCloudCover { get; set; }
        public double Precip24h { get; set; }
        public double Visibility { get; set; }
        public double WindDir10m { get; set; }
        public double WindSpeed10m { get; set; }
    }

    /// <summary>
    /// ICS data framework for import from "rezervacie letov Future Fly_rezervacieletov@gmail.com.ics"
    /// </summary>
    public class IcsData
    {
        public string Summary { get; set; }
        public DateTime Date { get; set; }
    }

    /// <summary>
    /// Combined training data framework
    /// </summary>
    public class TrainingData
    {
        public double MaxTemp { get; set; }
        public double EffectiveCloudCover { get; set; }
        public double Precip24h { get; set; }
        public double Visibility { get; set; }
        public double WindDir10m { get; set; }
        public double WindSpeed10m { get; set; }
        public bool Class { get; set; }
    }
}
