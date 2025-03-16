using NeoCortexApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AnomalyDetectionSample
{
    /// <summary>
    /// Executes an anomaly detection experiment using an HTM model.
    /// </summary>
    public class HTMAnomalyDetector
    {
        private readonly string _trainingDataPath;
        private readonly string _testingDataPath;
        private static double _cumulativeAccuracy = 0.0;
        private static int _sequenceCount = 0;
        private readonly double _tolerance = 0.1;

        /// <summary>
        /// Initializes a new instance of HTMAnomalyDetector with default folder paths.
        /// </summary>
        public HTMAnomalyDetector(string trainingFolderPath = "anomaly_training", string testingFolderPath = "anomaly_predicting")
        {
            string baseDirectory = Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName;
            _trainingDataPath = Path.Combine(baseDirectory, trainingFolderPath);
            _testingDataPath = Path.Combine(baseDirectory, testingFolderPath);
        }

        /// <summary>
        /// Runs the HTM model training and anomaly detection experiment.
        /// </summary>
        public void RunExperiment()
        {
            var htmTrainer = new HTMTrainingService();
            htmTrainer.TrainModelWithHTM(_trainingDataPath, _testingDataPath, out Predictor trainedPredictor);

            Console.WriteLine("\nStarting anomaly detection experiment...\n");
            trainedPredictor.Reset();

            var testSequences = LoadTestSequences(_testingDataPath);
            string outputFilePath = PrepareOutputFile();

            var (alldata, anomalydices, results) = DetectAnomalies(trainedPredictor, testSequences);
            SaveResultsToFile(results, outputFilePath);

            StoredOutputValues.totalAvgAccuracy = _cumulativeAccuracy / _sequenceCount;
            AnomalyVisualizer.CreateGraphForAnomalies(allData, allAnomalyIndices);
            Console.WriteLine("Anomaly detection experiment completed. Results saved to file.");
        }

        /// <summary>
        /// Loads numerical sequences from testing CSV files.
        /// </summary>
        private List<List<double>> LoadTestSequences(string folderPath)
        {
            var reader = new CsvSequenceFolder(folderPath);
            return CsvSequenceFolder.TrimSequences(reader.ExtractSequencesFromFolder());
        }

        /// <summary>
        /// Generates a unique output file for experiment results.
        /// </summary>
        private string PrepareOutputFile()
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string outputFile = $"anomaly_output_{timestamp}.txt";
            string outputFolder = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName, "output");
            Directory.CreateDirectory(outputFolder);
            return Path.Combine(outputFolder, outputFile);
        }

        List<double[]> allData = new List<double[]>();
        List<List<int>> allAnomalyIndices = new List<List<int>>();

        /// <summary>
        /// Detects anomalies in all test sequences and returns the results.
        /// </summary>
        private Tuple<List<double[]>, List<List<int>>, List<string>>  DetectAnomalies(Predictor predictor, List<List<double>> sequences)
        {
            var allResults = new List<string>();

            foreach (var sequence in sequences)
            {
                allData.Add(sequence.ToArray());

                try
                {
                    var (log, anomalies) = ProcessSequence(predictor, sequence.ToArray());
                    allResults.AddRange(log);
                    allAnomalyIndices.Add(anomalies);
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Sequence skipped due to error: {ex.Message}");
                }
            }

            return Tuple.Create(allData, allAnomalyIndices, allResults);
        }

        /// <summary>
        /// Processes a single sequence and detects anomalies based on predictions.
        /// </summary>
        private Tuple<List<string>, List<int>> ProcessSequence(Predictor predictor, double[] sequence)
        {
            ValidateSequence(sequence);
            var resultLines = new List<string>
            {
                "------------------------------",
                $"Testing sequence: {string.Join(", ", sequence)}",
                "------------------------------"
            };

            var anomalousIndex = new List<int>();
            double sequenceAccuracy = 0.0;

            for (int i = 0; i < sequence.Length - 1; i++)
            {
                var predictionResult = predictor.Predict(sequence[i]);
                if (!predictionResult.Any())
                {
                    resultLines.Add("Prediction failed.");
                    continue;
                }

                double predictedValue = ExtractPredictedValue(predictionResult.First().PredictedInput);
                double similarity = predictionResult.First().Similarity;
                bool isAnomalous = IsAnomaly(predictedValue, sequence[i + 1]);

                if (isAnomalous)
                {
                    anomalousIndex.Add((int)(i + 1)); // Store the actual value as an int
                    resultLines.Add($"Anomaly detected: Predicted {predictedValue}, Actual {sequence[i + 1]}, Similarity {similarity}%");
                }
                else
                {
                    resultLines.Add($"No anomaly: Predicted {predictedValue}, Actual {sequence[i + 1]}, Similarity {similarity}%");
                }

                sequenceAccuracy += similarity;
            }

            double avgAccuracy = sequenceAccuracy / (sequence.Length - 1);
            _cumulativeAccuracy += avgAccuracy;
            _sequenceCount++;

            resultLines.Add($"Average accuracy: {avgAccuracy}%");
            resultLines.Add("------------------------------");

            return Tuple.Create(resultLines, anomalousIndex);
        }

        /// <summary>
        /// Validates input sequence before anomaly detection.
        /// </summary>
        private void ValidateSequence(double[] sequence)
        {
            if (sequence.Length < 2) throw new ArgumentException("Sequence must have at least two values.");
            if (sequence.Any(double.IsNaN)) throw new ArgumentException("Sequence contains non-numeric values.");
        }

        /// <summary>
        /// Extracts the numerical predicted value from a string representation.
        /// </summary>
        private double ExtractPredictedValue(string prediction)
        {
            return double.Parse(prediction.Split('-').Last());
        }

        /// <summary>
        /// Determines if a value is an anomaly based on deviation from prediction.
        /// </summary>
        private bool IsAnomaly(double predicted, double actual)
        {
            return Math.Abs(predicted - actual) / actual > _tolerance;
        }

        /// <summary>
        /// Saves experiment results to a text file.
        /// </summary>
        private void SaveResultsToFile(List<string> results, string filePath)
        {
            File.WriteAllLines(filePath, results);
        }
    }
}
