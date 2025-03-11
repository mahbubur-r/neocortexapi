using NeoCortexApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AnomalyDetectionSample
{
    /// <summary>
    /// Handles the training of an HTM model using numerical sequences from CSV files.
    /// </summary>
    public class HTMTrainingService
    {
        /// <summary>
        /// Trains an HTM model using data from the provided CSV file paths.
        /// </summary>
        /// <param name="trainingFolderPath">Path to the folder containing training CSV files.</param>
        /// <param name="predictionFolderPath">Path to the folder containing prediction CSV files.</param>
        /// <param name="trainedPredictor">The trained HTM predictor model.</param>
        public void TrainModelWithHTM(string trainingFolderPath, string predictionFolderPath, out Predictor trainedPredictor)
        {
            LogMessage("Starting anomaly detection experiment...");
            LogMessage("HTM training initiated...");

            // Measure training time
            Stopwatch stopwatch = Stopwatch.StartNew();

            trainedPredictor = TrainHTMModel(trainingFolderPath, predictionFolderPath);

            stopwatch.Stop();
            LogMessage($"HTM training completed! Total training time: {stopwatch.Elapsed.TotalSeconds} seconds.");
        }

        /// <summary>
        /// Reads sequences from CSV files, processes them into HTM format, and trains an HTM model.
        /// </summary>
        /// <param name="trainingFolderPath">Path to training data folder.</param>
        /// <param name="predictionFolderPath">Path to prediction data folder.</param>
        /// <returns>A trained HTM Predictor.</returns>
        private static Predictor TrainHTMModel(string trainingFolderPath, string predictionFolderPath)
        {
            // Load numerical sequences from CSV files
            var trainingSequences = LoadSequences(trainingFolderPath);
            var predictionSequences = LoadSequences(predictionFolderPath);

            // Combine training and prediction sequences
            List<List<double>> combinedSequences = new List<List<double>>(trainingSequences);
            combinedSequences.AddRange(predictionSequences);

            // Convert sequences into HTM-compatible input format
            var htmInput = ConvertToHTMInput(combinedSequences);

            // Train HTM model using multi-sequence learning
            return RunHTMTraining(htmInput);
        }

        /// <summary>
        /// Reads numerical sequences from CSV files in a specified folder.
        /// </summary>
        /// <param name="folderPath">Path to folder containing CSV files.</param>
        /// <returns>List of numerical sequences.</returns>
        private static List<List<double>> LoadSequences(string folderPath)
        {
            CsvSequenceFolder reader = new CsvSequenceFolder(folderPath);
            return reader.ExtractSequencesFromFolder();
        }

        /// <summary>
        /// Converts numerical sequences into the required HTM model input format.
        /// </summary>
        /// <param name="sequences">List of numerical sequences.</param>
        /// <returns>Formatted HTM input data.</returns>
        private static List<List<double>> ConvertToHTMInput(List<List<double>> sequences)
        {
            CSVToHTMInputConverter converter = new CSVToHTMInputConverter();
            return converter.ConvertToHTMInput(sequences);
        }

        /// <summary>
        /// Executes HTM model training using the provided input data.
        /// </summary>
        /// <param name="htmInput">Preprocessed input data for HTM model.</param>
        /// <returns>Trained Predictor model.</returns>
        private static Predictor RunHTMTraining(List<List<double>> htmInput)
        {
            MultiSequenceLearning learningAlgorithm = new MultiSequenceLearning();
            return learningAlgorithm.Run(htmInput);
        }

        /// <summary>
        /// Logs a formatted message to the console.
        /// </summary>
        /// <param name="message">Message to log.</param>
        private static void LogMessage(string message)
        {
            Console.WriteLine("\n------------------------------");
            Console.WriteLine(message);
            Console.WriteLine("------------------------------\n");
        }
    }
}
