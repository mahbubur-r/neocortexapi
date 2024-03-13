using System;

namespace AnomalyDetectionSample
{
    public class CSVFileReader
    {
        private string _filePathToCSV;
        public CSVFileReader(string filePathToCSV)
        {
            _filePathToCSV = filePathToCSV;
        }

        public List<List<double>> ReadFile()
        {
            List<List<double>> sequences = new List<List<double>>();
            string[] csvLines = File.ReadAllLines(_filePathToCSV);
            for (int i = 0; i < csvLines.Length; i++)
            {
                string[] columns = csvLines[i].Split(new char[] { ',' });
                List<double> sequence = new List<double>();
                for (int j = 0; j < columns.Length; j++)
                {
                    if (double.TryParse(columns[j], out double value))
                    {
                        sequence.Add(value);
                    }
                    else
                    {
                        throw new ArgumentException($"Non-numeric value found! Please check the file.");
                    }
                }
                sequences.Add(sequence);
            }
            return sequences;
        }
        public void CSVSequencesConsoleOutput()
        {
            List<List<double>> sequences = ReadFile();
            for (int i = 0; i < sequences.Count; i++)
            {
                Console.Write("Sequence " + (i + 1) + ": ");
                foreach (double number in sequences[i])
                {
                    Console.Write(number + " ");
                }
                Console.WriteLine("");
            }
        }
        public static List<List<double>> TrimSequences(List<List<double>> sequences)
        {
            Random rnd = new Random();
            List<List<double>> trimmedSequences = new List<List<double>>();

            foreach (List<double> sequence in sequences)
            {
                int numElementsToRemove = rnd.Next(1, 5);
                List<double> trimmedSequence = sequence.Skip(numElementsToRemove).ToList();
                trimmedSequences.Add(trimmedSequence);
            }

            return trimmedSequences;
        }
    }
}
