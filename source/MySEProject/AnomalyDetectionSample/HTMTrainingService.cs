using NeoCortexApi;
using System.Diagnostics;
using System;
using System.Collections.Generic;

namespace AnomalyDetectionSample
{
    /// <summary>
    /// Executes the HTM model training experiment using sequence to return the trained model
    /// </summary>
    public class HTMTrainingService
    {
        /// <summary>
        /// Executes the HTM model training experiment using CSV files from specified folders and returns the trained predictor.
        /// </summary>
        /// <param name="trainingFolderPath">The path to the folder containing the CSV files used for training.</param>
        /// <param name="predictionFolderPath">The path to the folder containing the CSV files used for prediction.</param>
        /// <param name="trainedPredictor">The trained model that will be used for prediction.</param>
        public void TrainModelWithHTM(string trainingFolderPath, string predictionFolderPath, out Predictor trainedPredictor)
        {
            Console.WriteLine("------------------------------");
            Console.WriteLine();
            Console.WriteLine("Starting anomaly detection experiment!!");
            Console.WriteLine();
            Console.WriteLine("------------------------------");
            Console.WriteLine();
            Console.WriteLine("HTM training initiated...................");

            // Using Stopwatch to measure the total training time
            Stopwatch stopwatch = Stopwatch.StartNew();

            trainedPredictor = HTMTrainingSteps(trainingFolderPath, predictionFolderPath);

            stopwatch.Stop();

            Console.WriteLine();
            Console.WriteLine("------------------------------");
            Console.WriteLine();
            Console.WriteLine("HTM training completed! Total training time: " + stopwatch.Elapsed.TotalSeconds + " seconds.");
            Console.WriteLine();
            Console.WriteLine("------------------------------");
        }

        /// <summary>
        /// Trains an HTM (Hierarchical Temporal Memory) model using sequences from CSV files.
        /// It reads data from both training and prediction folders, converts them into HTM input format,
        /// and runs a multi-sequence learning algorithm to generate a trained predictor.
        /// </summary>
        /// <param name="trainingFolderPath">Path to the folder containing training data CSV files.</param>
        /// <param name="predictionFolderPath">Path to the folder containing prediction data CSV files.</param>
        /// <returns>Trained HTM predictor model.</returns>
        private static Predictor HTMTrainingSteps(string trainingFolderPath, string predictionFolderPath)
        {
            Predictor trainedPredictor;
            // Read numerical sequences from CSV files in the specified training folder
            CsvSequenceFolder trainingReader = new CsvSequenceFolder(trainingFolderPath);
            var trainingSequences = trainingReader.ExtractSequencesFromFolder();

            // Read numerical sequences from CSV files in the specified prediction folder
            CsvSequenceFolder predictionReader = new CsvSequenceFolder(predictionFolderPath);
            var predictionSequences = predictionReader.ExtractSequencesFromFolder();

            // Combine sequences from both training and prediction folders
            List<List<double>> combinedSequences = new List<List<double>>(trainingSequences);
            combinedSequences.AddRange(predictionSequences);

            // Convert sequences to HTM input format
            CSVToHTMInputConverter sequenceConverter = new CSVToHTMInputConverter();
            var htmInput = sequenceConverter.ConvertToHTMInput(combinedSequences);

            // Start multi-sequence learning experiment to generate predictor model
            MultiSequenceLearning learningAlgorithm = new MultiSequenceLearning();
            trainedPredictor = learningAlgorithm.Run(htmInput);

            // HTM model training completed
            return trainedPredictor;
        }
    }
}
