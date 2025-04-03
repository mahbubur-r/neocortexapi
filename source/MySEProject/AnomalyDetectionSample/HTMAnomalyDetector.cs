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
        private readonly string _trainingDataPath;  // Path to the folder containing training data
        private readonly string _testingDataPath;   // Path to the folder containing testing data
        private static double _cumulativeAccuracy = 0.0;  // Cumulative accuracy of the model over multiple sequences
        private static int _sequenceCount = 0;   // Count of processed sequences
        private readonly double _tolerance = 0.2;  // Tolerance level for anomaly detection (based on predicted vs actual value)
        List<double[]> allLearnedData = new List<double[]>();  // Stores all learned data sequences

        /// <summary>
        /// Initializes a new instance of HTMAnomalyDetector with default folder paths.
        /// </summary>
        /// <param name="trainingFolderPath">The folder path for the training data (default is "anomaly_training").</param>
        /// <param name="testingFolderPath">The folder path for the testing data (default is "anomaly_predicting").</param>
        public HTMAnomalyDetector(string trainingFolderPath = "anomaly_training", string testingFolderPath = "anomaly_predicting")
        {
            // Set the paths for the training and testing data based on the current working directory
            string baseDirectory = Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName;
            _trainingDataPath = Path.Combine(baseDirectory, trainingFolderPath);
            _testingDataPath = Path.Combine(baseDirectory, testingFolderPath);
        }

        /// <summary>
        /// Runs the HTM model training and anomaly detection experiment.
        /// This method trains the model, processes input sequences for learning,
        /// tests the model on unseen data, and saves the results.
        /// </summary>
        public void RunExperiment()
        {
            var htmTrainer = new HTMTrainingService();  // Instantiate the HTM trainer
            htmTrainer.TrainModelWithHTM(_trainingDataPath, _testingDataPath, out Predictor trainedPredictor);

            // Read and store input sequences from the training data folder
            CsvSequenceFolder trainingReader = new CsvSequenceFolder(_trainingDataPath);
            var inputSequences = trainingReader.ExtractSequencesFromFolder();

            // Add each learned sequence to the list
            foreach (var sequence in inputSequences)
            {
                allLearnedData.Add(sequence.ToArray());
            }

            Console.WriteLine("\nStarting anomaly detection experiment...\n");

            // Reset the trained predictor before starting the experiment
            trainedPredictor.Reset();

            // Load test sequences from the testing data folder
            var testSequences = LoadTestSequences(_testingDataPath);

            // Prepare the output file where results will be saved
            string outputFilePath = PrepareOutputFile();

            // Detect anomalies in test sequences and get results
            var (allTestingData, allAnomalyIndices, results) = DetectAnomalies(trainedPredictor, testSequences);

            // Save the results to the output file
            SaveResultsToFile(results, outputFilePath);

            // Calculate average accuracy
            StoredOutputValues.totalAvgAccuracy = _cumulativeAccuracy / _sequenceCount;

            // Generate anomaly comparison graphs
            AnomalyGraphs.CompareGraphForSequences(allLearnedData, allTestingData);
            AnomalyGraphs.CompareBothSequenceWithAnomalies(allLearnedData, allTestingData, allAnomalyIndices);

            Console.WriteLine("Anomaly detection experiment completed. Results saved to file.");
        }

        /// <summary>
        /// Loads numerical sequences from testing CSV files.
        /// </summary>
        /// <param name="folderPath">The folder path where the test sequences are located.</param>
        /// <returns>A list of lists, where each list contains a sequence of numerical values.</returns>
        private List<List<double>> LoadTestSequences(string folderPath)
        {
            var reader = new CsvSequenceFolder(folderPath);  // Instantiate CSV sequence reader
            return CsvSequenceFolder.TrimSequences(reader.ExtractSequencesFromFolder());  // Trim and return sequences from the folder
        }

        /// <summary>
        /// Generates a unique output file for experiment results.
        /// </summary>
        /// <returns>The path of the generated output file where results will be saved.</returns>
        private string PrepareOutputFile()
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");  // Generate a timestamp for the file name
            string outputFile = $"anomaly_output_{timestamp}.txt";  // File name with timestamp
            string outputFolder = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName, "output");
            Directory.CreateDirectory(outputFolder);  // Create output folder if it doesn't exist
            return Path.Combine(outputFolder, outputFile);  // Return full path for the output file
        }

        List<double[]> allTestingData = new List<double[]>();  // Stores test data sequences
        List<List<int>> allAnomalyIndices = new List<List<int>>();  // Stores the indices of detected anomalies in test sequences

        /// <summary>
        /// Detects anomalies in all test sequences and returns the results.
        /// </summary>
        /// <param name="predictor">The trained HTM predictor used to make predictions.</param>
        /// <param name="sequences">The list of sequences to process for anomaly detection.</param>
        /// <returns>A tuple containing the test data, anomaly indices, and the results of the anomaly detection.</returns>
        private Tuple<List<double[]>, List<List<int>>, List<string>> DetectAnomalies(Predictor predictor, List<List<double>> sequences)
        {
            var allResults = new List<string>();  // List to store the results for each sequence

            foreach (var sequence in sequences)
            {
                allTestingData.Add(sequence.ToArray());  // Store the test sequence

                try
                {
                    var (log, anomalies) = ProcessSequence(predictor, sequence.ToArray());  // Process each sequence and detect anomalies
                    allResults.AddRange(log);  // Add the results to the log
                    allAnomalyIndices.Add(anomalies);  // Store the indices of detected anomalies
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Sequence skipped due to error: {ex.Message}");  // Skip sequences with errors
                }
            }

            return Tuple.Create(allTestingData, allAnomalyIndices, allResults);  // Return results as a tuple
        }

        /// <summary>
        /// Processes a single sequence and detects anomalies based on predictions.
        /// </summary>
        /// <param name="predictor">The trained HTM predictor used to predict the next values in the sequence.</param>
        /// <param name="sequence">The sequence of values to process.</param>
        /// <returns>A tuple containing the result log and the indices of anomalies detected in the sequence.</returns>
        private Tuple<List<string>, List<int>> ProcessSequence(Predictor predictor, double[] sequence)
        {
            ValidateSequence(sequence);  // Validate the sequence before processing

            var resultLines = new List<string>
            {
                "------------------------------",
                $"Testing sequence: {string.Join(", ", sequence)}",
                "------------------------------"
            };

            var anomalousIndex = new List<int>();  // List to store the indices of detected anomalies
            double sequenceAccuracy = 0.0;  // Variable to calculate accuracy of the sequence

            for (int i = 0; i < sequence.Length - 1; i++)
            {
                var predictionResult = predictor.Predict(sequence[i]);  // Make a prediction based on the current input
                if (!predictionResult.Any())
                {
                    resultLines.Add("Prediction failed.");
                    continue;
                }

                double predictedValue = ExtractPredictedValue(predictionResult.First().PredictedInput);  // Extract the predicted value
                double similarity = predictionResult.First().Similarity;  // Get similarity of prediction
                bool isAnomalous = IsAnomaly(predictedValue, sequence[i + 1]);  // Check if the prediction is anomalous

                if (isAnomalous)
                {
                    anomalousIndex.Add((int)(i + 1));  // Store the index of the anomaly
                    resultLines.Add($"Anomaly detected: Predicted {predictedValue}, Actual {sequence[i + 1]}, Similarity {similarity}%");
                }
                else
                {
                    resultLines.Add($"No anomaly: Predicted {predictedValue}, Actual {sequence[i + 1]}, Similarity {similarity}%");
                }

                sequenceAccuracy += similarity;  // Update sequence accuracy
            }

            double avgAccuracy = sequenceAccuracy / (sequence.Length - 1);  // Calculate average accuracy for the sequence
            _cumulativeAccuracy += avgAccuracy;  // Update cumulative accuracy
            _sequenceCount++;  // Increment sequence count

            resultLines.Add($"Average accuracy: {avgAccuracy}%");
            resultLines.Add("------------------------------");

            return Tuple.Create(resultLines, anomalousIndex);  // Return the results and anomalies as a tuple
        }

        /// <summary>
        /// Validates input sequence before anomaly detection.
        /// </summary>
        /// <param name="sequence">The sequence to validate.</param>
        /// <exception cref="ArgumentException">Throws if the sequence is too short or contains non-numeric values.</exception>
        private void ValidateSequence(double[] sequence)
        {
            if (sequence.Length < 2) throw new ArgumentException("Sequence must have at least two values.");
            if (sequence.Any(double.IsNaN)) throw new ArgumentException("Sequence contains non-numeric values.");
        }

        /// <summary>
        /// Extracts the numerical predicted value from a string representation.
        /// </summary>
        /// <param name="prediction">The predicted value as a string.</param>
        /// <returns>The predicted numerical value.</returns>
        private double ExtractPredictedValue(string prediction)
        {
            return double.Parse(prediction.Split('-').Last());  // Parse the predicted value from the string
        }

        /// <summary>
        /// Determines if a value is an anomaly based on deviation from prediction.
        /// </summary>
        /// <param name="predicted">The predicted value.</param>
        /// <param name="actual">The actual value.</param>
        /// <returns>True if the value is an anomaly, false otherwise.</returns>
        private bool IsAnomaly(double predicted, double actual)
        {
            return Math.Abs(predicted - actual) / actual > _tolerance;  // Check if deviation exceeds the tolerance
        }

        /// <summary>
        /// Saves experiment results to a text file.
        /// </summary>
        /// <param name="results">The results to save.</param>
        /// <param name="filePath">The path to the file where the results should be saved.</param>
        private void SaveResultsToFile(List<string> results, string filePath)
        {
            File.WriteAllLines(filePath, results);  // Write the results to the specified file
        }
    }
}