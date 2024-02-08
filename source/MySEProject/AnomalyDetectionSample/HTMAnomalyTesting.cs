using NeoCortexApi;
using System;

namespace AnomalyDetectionSample
{
    public class HTMAnomalyTesting
    {
        private readonly string _trainingFolderPath;
        private readonly string _predictingFolderPath;
        /// <summary>
        /// Runs the anomaly detection experiment.
        /// </summary>
        public HTMAnomalyTesting(string trainingFolderPath = "training", string predictingFolderPath = "predicting")
        {
            // Folder directory set to location of C# files. This is the relative path.
            string projectbaseDirectory = Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName;
            _trainingFolderPath = Path.Combine(projectbaseDirectory, trainingFolderPath);
            _predictingFolderPath = Path.Combine(projectbaseDirectory, predictingFolderPath);

        }
        public void Run()
        {
            // HTM model training initiated
            HTMModeltraining myModel = new HTMModeltraining();
            Predictor myPredictor;


            myModel.RunHTMModelLearning(_trainingFolderPath, _predictingFolderPath, out myPredictor);

            Console.WriteLine("------------------------------");
            Console.WriteLine();
            Console.WriteLine("Started testing our trained HTM Engine...................");
            Console.WriteLine();

            // Starting to test our trained HTM model

            // CSVFileReader can also be used in place of CSVFolderReader to read a single file
            // We will take sequences from predicting folder
            // After that, we will then trim those sequences: sequences where first few elements are removed, for anomaly detection
            CSVFolderReader testseq = new CSVFolderReader(_predictingFolderPath);
            var inputtestseq = testseq.ReadFolder();
            var triminputtestseq = CSVFolderReader.TrimSequences(inputtestseq);
            myPredictor.Reset();


            foreach (List<double> list in triminputtestseq)
            {
                double[] lst = list.ToArray();
                try
                {
                    DetectAnomaly(myPredictor, lst);
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Exception caught: {ex.Message}");
                }
            }
        }

        Console.WriteLine();
            Console.WriteLine("------------------------------");
            Console.WriteLine();
            Console.WriteLine("Anomaly detection experiment complete!!.");
            Console.WriteLine();
            Console.WriteLine("------------------------------");
}
    private static void DetectAnomaly(Predictor predictor, double[] list)
    {
        if (list.Length < 2)
        {
            throw new ArgumentException($"List must contain at least two values. Actual count: {list.Length}. List: [{string.Join(",", list)}]");

        }
        foreach (double value in list)
        {
            if (double.IsNaN(value))
            {
                throw new ArgumentException($"List contains non-numeric values. List: [{string.Join(",", list)}]");
            }
        }
}
