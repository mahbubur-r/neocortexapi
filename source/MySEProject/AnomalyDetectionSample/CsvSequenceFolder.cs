using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AnomalyDetectionSample
{
    /// <summary>
    /// Utility class for reading and processing CSV files from a specified folder.
    /// </summary>
    public class CsvSequenceFolder
    {
        private readonly string _folderPath;

        /// <summary>
        /// Initializes a new instance of the CsvSequenceFolder class with the provided folder path.
        /// </summary>
        /// <param name="folderPath">The path to the folder containing CSV files.</param>
        public CsvSequenceFolder(string folderPath)
        {
            _folderPath = folderPath;
        }

        /// <summary>
        /// Reads all CSV files in the specified folder and extracts numerical sequences.
        /// </summary>
        /// <returns>A list of sequences extracted from the CSV files.</returns>
        /// <exception cref="ArgumentException">Thrown when a non-numeric value is found in a CSV file.</exception>
        public List<List<double>> ExtractSequencesFromFolder()
        {
            var folderSequences = new List<List<double>>();
            string[] fileEntries = Directory.GetFiles(_folderPath, "*.csv");

            foreach (string fileName in fileEntries)
            {
                try
                {
                    var sequencesInFile = ReadCsvFile(fileName);
                    folderSequences.AddRange(sequencesInFile);
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Error reading file {fileName}: {ex.Message}");
                }
            }
            return folderSequences;
        }

        /// <summary>
        /// Reads a CSV file and converts its contents into a list of numerical sequences.
        /// </summary>
        /// <param name="filePath">Path of the CSV file to read.</param>
        /// <returns>List of numerical sequences from the CSV file.</returns>
        /// <exception cref="ArgumentException">Thrown when a non-numeric value is found.</exception>
        private List<List<double>> ReadCsvFile(string filePath)
        {
            var sequences = new List<List<double>>();
            string[] csvLines = File.ReadAllLines(filePath);

            foreach (string line in csvLines)
            {
                var sequence = ParseCsvLine(line, filePath);
                sequences.Add(sequence);
            }
            return sequences;
        }

        /// <summary>
        /// Parses a single CSV line into a numerical sequence.
        /// </summary>
        /// <param name="line">The CSV line to parse.</param>
        /// <param name="filePath">The file path for error tracking.</param>
        /// <returns>A list of double values representing the sequence.</returns>
        /// <exception cref="ArgumentException">Thrown if a non-numeric value is encountered.</exception>
        private List<double> ParseCsvLine(string line, string filePath)
        {
            var sequence = new List<double>();
            string[] columns = line.Split(',');

            foreach (string column in columns)
            {
                if (double.TryParse(column, out double value))
                {
                    sequence.Add(value);
                }
                else
                {
                    throw new ArgumentException($"Non-numeric value found in {filePath}. Ensure all values are numbers.");
                }
            }
            return sequence;
        }

        /// <summary>
        /// Displays all extracted sequences in the console.
        /// </summary>
        public void DisplayCsvSequences()
        {
            var sequences = ExtractSequencesFromFolder();

            for (int i = 0; i < sequences.Count; i++)
            {
                Console.WriteLine($"Sequence {i + 1}: {string.Join(" ", sequences[i])}");
            }
        }

        /// <summary>
        /// Trims a random number of elements (between 1 and 4) from the beginning of each sequence.
        /// </summary>
        /// <param name="sequences">The list of sequences to trim.</param>
        /// <returns>A new list of sequences with elements removed.</returns>
        public static List<List<double>> TrimSequences(List<List<double>> sequences)
        {
            var random = new Random();
            return sequences.Select(sequence => sequence.Skip(random.Next(1, 5)).ToList()).ToList();
        }
    }
}
