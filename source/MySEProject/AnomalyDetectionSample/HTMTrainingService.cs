using NeoCortexApi;
using System.Diagnostics;
using System;
using System.Collections.Generic;

namespace AnomalyDetectionSample
{
    /// <summary>
    /// Handles the training of an HTM model using sequence data from CSV files.
    /// </summary>
    public class HTMTrainingService
    {
        /// <summary>
        /// Trains an HTM model using CSV files and returns the trained predictor.
        /// </summary>
        /// <param name="trainingFolderPath">The folder path containing training CSV files.</param>
        /// <param name="predictionFolderPath">The folder path containing prediction CSV files.</param>
        /// <param name="trainedPredictor">The output trained model used for prediction.</param>
        public void TrainModelWithHTM(string trainingFolderPath, string predictionFolderPath, out Predictor trainedPredictor)
        {
            Console.WriteLine("\n------------------------------");
            Console.WriteLine("Starting HTM model training...");
            Console.WriteLine("------------------------------\n");

            // Start stopwatch to measure training time
            Stopwatch stopwatch = Stopwatch.StartNew();

            trainedPredictor = ExecuteTrainingSteps(trainingFolderPath, predictionFolderPath);

            stopwatch.Stop();

            Console.WriteLine("\n------------------------------");
            Console.WriteLine($"HTM training completed! Total training time: {stopwatch.Elapsed.TotalSeconds:F2} seconds.");
            Console.WriteLine("------------------------------\n");
        }

        /// <summary>
        /// Executes the necessary steps for training an HTM model.
        /// </summary>
        /// <param name="trainingFolderPath">Path to the folder containing training sequences.</param>
        /// <param name="predictionFolderPath">Path to the folder containing prediction sequences.</param>
        /// <returns>The trained predictor model.</returns>
        private static Predictor ExecuteTrainingSteps(string trainingFolderPath, string predictionFolderPath)
        {
            // Load training sequences from CSV files
            var trainingSequences = LoadSequencesFromFolder(trainingFolderPath);

            // Load prediction sequences from CSV files
            var predictionSequences = LoadSequencesFromFolder(predictionFolderPath);

            // Combine both training and prediction sequences
            List<List<double>> combinedSequences = new List<List<double>>(trainingSequences);
            combinedSequences.AddRange(predictionSequences);

            // Convert sequences to HTM-compatible input format
            var htmInput = ConvertToHTMInput(combinedSequences);

            // Train HTM model using multi-sequence learning algorithm
            return TrainHTMModel(htmInput);
        }

        /// <summary>
        /// Loads numerical sequences from CSV files in a given folder.
        /// </summary>
        /// <param name="folderPath">Path to the folder containing CSV files.</param>
        /// <returns>A list of numerical sequences.</returns>
        private static List<List<double>> LoadSequencesFromFolder(string folderPath)
        {
            CsvSequenceFolder sequenceReader = new CsvSequenceFolder(folderPath);
            return sequenceReader.ExtractSequencesFromFolder();
        }

        /// <summary>
        /// Converts a list of numerical sequences into an HTM-compatible format.
        /// </summary>
        /// <param name="sequences">The combined numerical sequences.</param>
        /// <returns>The formatted HTM input.</returns>
        private static HTMInput ConvertToHTMInput(List<List<double>> sequences)
        {
            CSVToHTMInputConverter sequenceConverter = new CSVToHTMInputConverter();
            return sequenceConverter.ConvertToHTMInput(sequences);
        }

        /// <summary>
        /// Trains an HTM model using the formatted input data.
        /// </summary>
        /// <param name="htmInput">HTM-compatible input data.</param>
        /// <returns>A trained predictor model.</returns>
        private static Predictor TrainHTMModel(HTMInput htmInput)
        {
            MultiSequenceLearning learningAlgorithm = new MultiSequenceLearning();
            return learningAlgorithm.Run(htmInput);
        }
    }
}
