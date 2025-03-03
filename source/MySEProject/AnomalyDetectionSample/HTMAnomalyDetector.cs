using NeoCortexApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AnomalyDetectionSample
{
    /// <summary>
    /// Responsible for executing the anomaly detection experiment using an HTM model.
    /// </summary>
    public class HTMAnomalyDetector
    {
        private readonly string _trainingDataPath;
        private readonly string _testingDataPath;
        private static double _cumulativeAccuracy = 0.0;
        private static int _sequenceCount = 0;
        private readonly double _tolerance = 0.1;

        /// <summary>
        /// Initializes a new instance of the HTMAnomalyDetector class with default folder paths.
        /// </summary>
        /// <param name="trainingFolderPath">Path to the folder containing training CSV files.</param>
        /// <param name="testingFolderPath">Path to the folder containing testing CSV files.</param>
        public HTMAnomalyDetector(string trainingFolderPath = "anomaly_training", string testingFolderPath = "anomaly_testing")
        {
            string projectBaseDirectory = Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName;
            _trainingDataPath = Path.Combine(projectBaseDirectory, trainingFolderPath);
            _testingDataPath = Path.Combine(projectBaseDirectory, testingFolderPath);
        }

        /// <summary>
        /// Executes the HTM model training and anomaly detection experiments.
        /// </summary>
        public void RunExperiment()
        {
            var htmTrainer = new HTMTrainingService();
            Predictor trainedPredictor;

            htmTrainer.TrainModelWithHTM(_trainingDataPath, _testingDataPath, out trainedPredictor);

            Console.WriteLine("\nStarting the anomaly detection experiment...\n");

            var testSequences = LoadTestSequences(_testingDataPath);
            trainedPredictor.Reset();

            string outputFilePath = PrepareOutputFile();

            List<string> experimentResults = DetectAnomaliesInSequences(trainedPredictor, testSequences);

            SaveResultsToFile(experimentResults, outputFilePath);

            StoredOutputValues.totalAvgAccuracy = _cumulativeAccuracy / _sequenceCount;

            Console.WriteLine("Experiment results have been written to the text file.");
            Console.WriteLine("Anomaly detection experiment completed.");
        }

        /// <summary>
        /// Loads test sequences from CSV files.
        /// </summary>
        private List<List<double>> LoadTestSequences(string folderPath)
        {
            var testSequenceReader = new CsvSequenceFolder(folderPath);
            var sequences = testSequenceReader.ExtractSequencesFromFolder();
            return CsvSequenceFolder.TrimSequences(sequences);
        }

        /// <summary>
        /// Creates and returns the file path for saving output results.
        /// </summary>
        private string PrepareOutputFile()
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string outputFile = $"anomaly_output_{timestamp}.txt";
            string projectBaseDirectory = Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName;
            string outputFolderPath = Path.Combine(projectBaseDirectory, "output");
            Directory.CreateDirectory(outputFolderPath);  // Ensure directory exists
            return Path.Combine(outputFolderPath, outputFile);
        }

        /// <summary>
        /// Detects anomalies for each sequence and returns the results.
        /// </summary>
        private List<string> DetectAnomaliesInSequences(Predictor predictor, List<List<double>> sequences)
        {
            var allResults = new List<string>();

            foreach (var sequence in sequences)
            {
                try
                {
                    var results = DetectAnomaliesInSequence(predictor, sequence.ToArray(), _tolerance);
                    allResults.AddRange(results);
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Exception caught: {ex.Message}");
                }
            }

            return allResults;
        }

        /// <summary>
        /// Saves the experiment results to a text file.
        /// </summary>
        private void SaveResultsToFile(List<string> results, string filePath)
        {
            File.WriteAllLines(filePath, results);
        }

        /// <summary>
        /// Detects anomalies in a sequence using the trained HTM model.
        /// </summary>
        private List<string> DetectAnomaliesInSequence(Predictor predictor, double[] sequence, double tolerance)
        {
            ValidateSequence(sequence);

            var resultLines = new List<string>
            {
                "------------------------------",
                $"Testing sequence: {string.Join(", ", sequence)}",
                "------------------------------"
            };

            double sequenceAccuracy = 0.0;

            for (int i = 0; i < sequence.Length; i++)
            {
                var currentValue = sequence[i];
                var predictionResult = predictor.Predict(currentValue);

                if (predictionResult.Count > 0)
                {
                    var predictedValue = ExtractPredictedValue(predictionResult.First().PredictedInput);
                    var similarity = predictionResult.First().Similarity;

                    if (i < sequence.Length - 1)
                    {
                        double nextValue = sequence[i + 1];
                        if (IsAnomaly(predictedValue, nextValue, tolerance))
                        {
                            resultLines.Add($"Anomaly detected: Predicted {predictedValue}, Actual {nextValue}, Similarity {similarity}%");
                            i++;
                        }
                        else
                        {
                            resultLines.Add($"No anomaly: Predicted {predictedValue}, Actual {nextValue}, Similarity {similarity}%");
                        }

                        sequenceAccuracy += similarity;
                    }
                    else
                    {
                        resultLines.Add("End of sequence reached.");
                    }
                }
                else
                {
                    resultLines.Add("Prediction failed.");
                }
            }

            double averageAccuracy = sequenceAccuracy / sequence.Length;
            _cumulativeAccuracy += averageAccuracy;
            _sequenceCount++;

            resultLines.Add($"Average accuracy for sequence: {averageAccuracy}%");
            resultLines.Add("------------------------------");

            return resultLines;
        }

        /// <summary>
        /// Validates the input sequence for length and numeric values.
        /// </summary>
        private void ValidateSequence(double[] sequence)
        {
            if (sequence.Length < 2)
                throw new ArgumentException("Sequence must contain at least two values.");

            if (sequence.Any(double.IsNaN))
                throw new ArgumentException("Sequence contains non-numeric values.");
        }

        /// <summary>
        /// Extracts the predicted value from the prediction string.
        /// </summary>
        private double ExtractPredictedValue(string prediction)
        {
            var tokens = prediction.Split('-');
            return double.Parse(tokens.Last());
        }

        /// <summary>
        /// Determines if the difference between predicted and actual value exceeds tolerance.
        /// </summary>
        private bool IsAnomaly(double predictedValue, double actualValue, double tolerance)
        {
            var deviation = Math.Abs(predictedValue - actualValue) / actualValue;
            return deviation > tolerance;
        }
    }
}
