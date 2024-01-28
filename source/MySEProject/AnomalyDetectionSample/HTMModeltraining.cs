using NeoCortexApi;
using System.Diagnostics;


namespace AnomalyDetectionSample
{
    public class HTMModeltraining
    {
        public void RunHTMModelLearning(string trainingfolderPath, string predictingfolderPath, out Predictor predictor)
        {
            Console.WriteLine("------------------------------");
            Console.WriteLine();
            Console.WriteLine("Starting our anomaly detection experiment!!");
            Console.WriteLine();
            Console.WriteLine("------------------------------");
            Console.WriteLine();
            Console.WriteLine("HTM Model training initiated...................");
            // Using stopwatch to calculate the total training time
            Stopwatch swh = Stopwatch.StartNew();

            // Read numerical sequences from CSV files in the specified folder containing files having training(learning) sequences
            // CSVFileReader class can also be used for single files
            CSVFolderReader reader = new CSVFolderReader(trainingfolderPath);
            var sequences1 = reader.ReadFolder();

            // Read numerical sequences from CSV files in the specified folder containing files having prediction sequences
            // CSVFileReader class can also be used for single files
            CSVFolderReader reader1 = new CSVFolderReader(predictingfolderPath);
            var sequences2 = reader1.ReadFolder();
        }

        }
}
