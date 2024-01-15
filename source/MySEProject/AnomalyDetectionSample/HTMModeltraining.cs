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

        }
}
