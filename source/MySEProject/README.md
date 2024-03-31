***Topic: ML23/24-08 Implement Anomaly Detection Sample***

***Getting Started***
To run this program, you need to have the following software installed on your machine:

Visual Studio 2019 or later
.NET Framework 4.7.2 or later

***Installing***
-Clone this repository or download the code as a zip file.
-Extract the zip file to a directory of your choice.
-Open the solution file NeoCortexApiSample.sln in Visual Studio.
-Build the solution by selecting Build Solution from the Build menu.

This project is based on NeoCortex API. More details [here](https://github.com/ddobric/neocortexapi/blob/master/source/Documentation/gettingStarted.md).

## Summary of the Project:

HTM (Hierarchical Temporal Memory) is a machine learning algorithm that processes time-series data in a distributed manner using a hierarchical network of nodes. Each nodes, or columns, can be trained to learn, and recognize patterns in input data. This can be used in identifying anomalies/deviations from normal patterns. It is a promising method for predicting and detecting anomalies in a range of applications. In this project, we will train our HTM Engine using the multisequencelearning class in the NeoCortex API, and then use the trained engine to learn patterns and identify anomalies. Specifically, numerical sequences will be read from various CSV files inside a folder in order to create an anomaly detection system.  


## Project Description

To train our HTM Engine, we used the [MultiSequenceLearning](https://github.com/ddobric/neocortexapi/blob/master/source/Samples/NeoCortexApiSample/MultisequenceLearning.cs) class in the NeoCortex API. Firstly, we will read and train the HTM Engine using the data from both our training (learning) and predicting (predictive) folders, which are present as numerical sequences in CSV files in the 'training' and 'predicting' folders inside the project directory. We will read numerical sequence data from the prediction folder for testing purposes, remove the first few elements (thus effectively turning the data into a subsequence of the original sequence; we have already inserted anomalies at random indexes into this data), and then use it to detect anomalies.

Please take note that all files inside the folders are read with the.csv extension, and exception handlers are set up in case the file format is incorrect.

We are employing artificial integer sequence data of network load for this project, which is saved inside of CSV files and is rounded off to the nearest integer, in percentage. Example of a csv file within training folder.

```
69,72,75,68,72,67,66,70,72,67
69,72,75,68,72,67,66,70,69,67
68,74,75,68,72,67,66,70,69,65
71,74,75,68,72,67,66,70,69,65
```
Normally, the values stay within the range of 65 to 75. All values outside of this range are considered anomalies for testing purposes. However, in order to identify anomalies, we have a csv file in the predicting folder. Typically, some of the data in this file does not fall within 65 and 75. 

```
71,74,98,68,92,65,66,70,69,65
71,74,75,68,72,65,66,30,69,35
71,74,75,71,72,65,36,70,69,65
71,75,75,71,72,65,66,70,98,95
```
We have uploaded the anomaly results of our data in this repository for reference.

1. output result of combined numerical sequence data from training folder (without anomalies) and predicting folder (with anomalies) can be found [here](https://github.com/mahbubur-r/neocortexapi/tree/Team_Anomaly_Detection/source/MySEProject/AnomalyDetectionSample/output).

## Execution of the project

The following is how our project is carried out.
 

* In the beginning, we have ExtractSequencesFromFolder method of [CsvSequenceFolder](https://github.com/mahbubur-r/neocortexapi/blob/Team_Anomaly_Detection/source/MySEProject/AnomalyDetectionSample/CsvSequenceFolder.cs) class to read all the files placed inside a folder. These classes keep track of the read sequences in a list of numerical sequences that will be used repeatedly in the future. To handle non-numeric data, some classes have incorporated exception handling inside. With the Trimsequences technique, data can be trimmed. It returns a numeric sequence after trimming one to four components (numbers 1 through 4) from the start.

```csharp
 public List<List<double>> ExtractSequencesFromFolder()
        {
         ....  
          return folderSequences;
        }

public static List<List<double>> TrimSequences(List<List<double>> sequences)
        {
        ....
          return trimmedSequences;
        }
```

* After that, the method ConvertToHTMInput of [CSVToHTMInputConverter](https://github.com/mahbubur-r/neocortexapi/blob/Team_Anomaly_Detection/source/MySEProject/AnomalyDetectionSample/CSVToHTMInputConverter.cs) class is there which converts all the read sequences to a format suitable for HTM training.

```csharp
Dictionary<string, List<double>> dictionary = new Dictionary<string, List<double>>();
for (int i = 0; i < sequences.Count; i++)
    {
     // Unique key created and added to dictionary for HTM Input                
     string key = "S" + (i + 1);
     List<double> value = sequences[i];
     dictionary.Add(key, value);
    }
     return dictionary;
```
* After that, we have ExecuteHTMModelTraining method of [HTMTrainingManager](https://github.com/mahbubur-r/neocortexapi/blob/Team_Anomaly_Detection/source/MySEProject/AnomalyDetectionSample/HTMTrainingManager.cs) class to train our model using the converted sequences. The numerical data sequences from training (for learning) and predicting folders are combined before training the HTM engine. This class returns our trained model object predictor.
```csharp
.....
MultiSequenceLearning learning = new MultiSequenceLearning();
predictor = learning.Run(htmInput);
.....
.....
List<List<double>> combinedSequences = new List<List<double>>(sequences1);
combinedSequences.AddRange(sequences2);
.....
```
* In the end, we use [HTMAnomalyExperiment](https://github.com/mahbubur-r/neocortexapi/blob/Team_Anomaly_Detection/source/MySEProject/AnomalyDetectionSample/HTMAnomalyExperiment.cs) to detected anomalies in sequences read from files inside predicting folder. All the classes explained earlier- CSV files reading (CsvSequenceFolder), combining and converting them for HTM training (CSVToHTMInputConverter) and training the HTM engine (using HTMTrainingManager) will be used here. We use the same class (CsvSequenceFolder) to read files for our predicting sequences. TrimSequences method is then used to trim sequences for anomaly testing. Method for trimming is already explained earlier.

```csharp
.....
CsvSequenceFolder testSequencesReader = new CsvSequenceFolder(_predictingCSVFolderPath);
var inputSequences = testSequencesReader.ExtractSequencesFromFolder();
var trimmedInputSequences = CsvSequenceFolder.TrimSequences(inputSequences);
.....
```
Path to training and predicting folder is set as default and passed on the constructor, or can be set inside the class manually.

```csharp
.....
_trainingCSVFolderPath = Path.Combine(projectBaseDirectory, trainingFolderPath);
_predictingCSVFolderPath = Path.Combine(projectBaseDirectory, predictingFolderPath);
.....
```
In the end, DetectAnomaly method is used to detect anomalies in our trimmed sequences one by one, using our trained HTM Model predictor. 
```csharp
foreach (List<double> list in triminputtestseq)
       {
         .....
         double[] lst = list.ToArray();
         DetectAnomaly(myPredictor, lst);
       }
```
Exception handling is present, such that errors thrown from DetectAnomaly method can be handled (like passing of non-numeric values, or number of elements in list less than two).

[DetectAnomaly](https://github.com/mahbubur-r/neocortexapi/blob/98ae630c79221e9d7a792282c5faabc08a2b794f/source/MySEProject/AnomalyDetectionSample/HTMAnomalyExperiment.cs#L105) is the main method from ExtractSequencesFromFolder class which detects anomalies in our data. It traverses each value of a list one by one in a sliding window manner, and uses trained model predictor to predict the next element for comparison. We use an anomalyscore to quantify the comparison and detect anomalies; if the prediction crosses a certain tolerance level, it is declared as an anomaly.

In our sliding window approach, naturally the first element is skipped, so we ensure that the first element is checked for anomaly in the beginning.

We can get our prediction in a list of results in format of "NeoCortexApi.Classifiers.ClassifierResult`1[System.String]" from our trained model Predictor using the following:

```csharp
var res = predictor.Predict(item);
```
Here, assume that item passed to the model is of int type with value 8. We can use this to analyze how prediction works. When this is executed,
```csharp
foreach (var pred in res)
 {
   Console.WriteLine($"{pred.PredictedInput} - {pred.Similarity}");
    }
```
We get the following output.
```
S2_2-9-10-7-11-8-1 - 100
S1_1-2-3-4-2-5-0 - 5
S1_-1.0-0-1-2-3-4 - 0
S1_-1.0-0-1-2-3-4-2 - 0
```
We know that the item we passed here is 8. The first line gives us the best prediction with similarity accuracy. We can easily get the predicted value which will come after 8 (here, it is 1), and previous value (11, in this case). We use basic string operations to get our required values.

We will then use this to detect anomalies.

* When we iteratively pass values to DetectAnomaly method using our sliding window approach, we will not be able to detect anomaly in the first element. So, in the beginning, we use the second element of the list to predict and compare the previous element (which is the first element). A flag is set to control the command execution; if the first element has anomaly, then we will not use it to detect our second element. We will directly start from second element. Otherwise, we will start from first element as usual.

* Now, when we traverse the list one by one to the right, we pass the value to the predictor to get the next value and compare the prediction with the actual value. If there's anomaly, then it is outputted to the user, and the anomalous element is skipped. Upon reaching to the last element, we can end our traversal and move on to next list.

We use anomalyscore (difference ratio) for comparison with our already preset threshold. When it exceeds, probable anomalies are found.

To run this project, use the following class/methods given in [Program.cs].

```csharp
HTMAnomalyExperiment tester = new HTMAnomalyExperiment();
tester.ExecuteExperiment();
```

### HTM Engine Settings:

It is crucial that our input data be encoded so that our HTM Engine can process it. More on [this](https://github.com/ddobric/neocortexapi/blob/master/source/Documentation/Encoders.md). 

We are utilizing the following settings since we will be training and testing data that falls between the range of integer values between 0-100 without any periodicity. Since we only expect values to fall inside this range, the minimum and maximum values are set to 0 and 100, respectively. These numbers must be adjusted for other usage scenarios. More on [this](https://github.com/mahbubur-r/neocortexapi/blob/0da3d6b9ac2e654e80b4bab9a84ad2e26f887028/source/MySEProject/AnomalyDetectionSample/multisequencelearning.cs#L22-L64)
 
Complete settings is [here](https://github.com/mahbubur-r/neocortexapi/blob/0da3d6b9ac2e654e80b4bab9a84ad2e26f887028/source/MySEProject/AnomalyDetectionSample/multisequencelearning.cs#L54-L64)

The configuration that we have used is as follows. More on [this](https://github.com/ddobric/neocortexapi/blob/master/source/Documentation/SpatialPooler.md#parameter-desription)
HTM Configuration is [here](https://github.com/mahbubur-r/neocortexapi/blob/0da3d6b9ac2e654e80b4bab9a84ad2e26f887028/source/MySEProject/AnomalyDetectionSample/multisequencelearning.cs#L26-L50)

### Multisequence learning

The [multisequencelearning](https://github.com/mahbubur-r/neocortexapi/blob/Team_Anomaly_Detection/source/MySEProject/AnomalyDetectionSample/multisequencelearning.cs) class file's [RunExperiment](https://github.com/mahbubur-r/neocortexapi/blob/34299872fcd5cdb30e6ab5fa41f8d46a19e6331e/source/MySEProject/AnomalyDetectionSample/multisequencelearning.cs#L74) method provides an example of how multisequence learning functions. As a summary,

* Initialization of connection memory and [HTM configuration](https://github.com/mahbubur-r/neocortexapi/blob/0da3d6b9ac2e654e80b4bab9a84ad2e26f887028/source/MySEProject/AnomalyDetectionSample/multisequencelearning.cs#L81) are performed. The [HTM Classifier](https://github.com/mahbubur-r/neocortexapi/blob/0da3d6b9ac2e654e80b4bab9a84ad2e26f887028/source/MySEProject/AnomalyDetectionSample/multisequencelearning.cs#L85), [Cortex layer](https://github.com/mahbubur-r/neocortexapi/blob/0da3d6b9ac2e654e80b4bab9a84ad2e26f887028/source/MySEProject/AnomalyDetectionSample/multisequencelearning.cs#L91), and [Homeostatic Plasticity Controller](https://github.com/mahbubur-r/neocortexapi/blob/0da3d6b9ac2e654e80b4bab9a84ad2e26f887028/source/MySEProject/AnomalyDetectionSample/multisequencelearning.cs#L96) are then initialized.

* Following that, Temporal Memory and Spatial Pooler are initialized.

```csharp
.....
TemporalMemory tm = new TemporalMemory();
SpatialPoolerMT sp = new SpatialPoolerMT(hpc);
.....
```
* The cortical layer is then added with spatial pooler memory, which is trained for the maximum number of cycles.

```csharp
.....
layer1.HtmModules.Add("sp", sp);
int maxCycles = 3500;
for (int i = 0; i < maxCycles && isInStableState == false; i++)
.....
`````
* In order to learn every input sequence, temporal memory is then introduced to the cortical layer.

```csharp
.....
layer1.HtmModules.Add("tm", tm);
foreach (var sequenceKeyPair in sequences){
.....
}
.....
```
* The HTM classifier and trained cortical layer are finally returned. More [here](https://github.com/mahbubur-r/neocortexapi/blob/0da3d6b9ac2e654e80b4bab9a84ad2e26f887028/source/MySEProject/AnomalyDetectionSample/multisequencelearning.cs#L298)

 
## Results

Following the project's execution, we obtained the following [Outputs](https://github.com/mahbubur-r/neocortexapi/tree/Team_Anomaly_Detection/source/MySEProject/AnomalyDetectionSample/output)

One testing sequence for anomaly detection and average accuracy for this sequence given below:

```
Testing the sequence for anomaly detection: 72, 67, 66, 90, 69, 97.

Current element in the testing sequence: 72
No anomaly detected in the next element. HTM Engine found similarity: 95.83%.
Current element in the testing sequence: 67
No anomaly detected in the next element. HTM Engine found similarity: 83.33%.
Current element in the testing sequence: 66
****Anomaly detected**** in the next element. HTM Engine predicted: 70 with similarity: 100%, actual value: 90.
Skipping to the next element in the testing sequence due to detected anomaly.
Current element in the testing sequence: 69
****Anomaly detected**** in the next element. HTM Engine predicted: 72 with similarity: 100%, actual value: 97.
Skipping to the next element in the testing sequence due to detected anomaly.

Average accuracy for this sequence: 63.19333333333333%.
```

```
Testing the sequence for anomaly detection: 75, 71, 72, 65, 66, 70, 98, 95.

Current element in the testing sequence: 75
No anomaly detected in the next element. HTM Engine found similarity: 100%.
Current element in the testing sequence: 71
Nothing predicted from HTM Engine. Anomaly detection failed.
Current element in the testing sequence: 72
No anomaly detected in the next element. HTM Engine found similarity: 61.67%.
Current element in the testing sequence: 65
Nothing predicted from HTM Engine. Anomaly detection failed.
Current element in the testing sequence: 66
No anomaly detected in the next element. HTM Engine found similarity: 92.59%.
Current element in the testing sequence: 70
****Anomaly detected**** in the next element. HTM Engine predicted: 72 with similarity: 87.5%, actual value: 98.
Skipping to the next element in the testing sequence due to detected anomaly.
Current element in the testing sequence: 95
Nothing predicted from HTM Engine. Anomaly detection failed.

Average accuracy for this sequence: 42.72%.

------------------------------
------------------------------

Testing the sequence for anomaly detection: 75, 68, 72, 67, 66, 99, 72, 67.

Current element in the testing sequence: 75
No anomaly detected in the next element. HTM Engine found similarity: 100%.
Current element in the testing sequence: 68
No anomaly detected in the next element. HTM Engine found similarity: 100%.
Current element in the testing sequence: 72
No anomaly detected in the next element. HTM Engine found similarity: 70.83%.
Current element in the testing sequence: 67
No anomaly detected in the next element. HTM Engine found similarity: 91.67%.
Current element in the testing sequence: 66
****Anomaly detected**** in the next element. HTM Engine predicted: 70 with similarity: 100%, actual value: 99.
Skipping to the next element in the testing sequence due to detected anomaly.
Current element in the testing sequence: 72
Nothing predicted from HTM Engine. Anomaly detection failed.
Current element in the testing sequence: 67
End of sequence. Further anomaly testing cannot be continued.

Average accuracy for this sequence: 57.8125%.
```

As we can see, the accuracy rate ranges from 50% to 70%. In an anomaly detection algorithm, a high degree of accuracy on the sequence is desired. Our machine's hardware specifications prevent us from running programs with a lot of cycles and sequences. Nevertheless, by executing more data sequence and cycle, accuracy can be increased.

On the other hand, a variety of factors come into play, such as the quantity and quality of the data, as well as the hyperparameters that are used to tune and train the model. To achieve the best results, more data should be used for training, and the hyperparameters should be further adjusted to find the most ideal setting for training. Due to scheduling and processing limitations, we used fewer numerical sequences as data to illustrate our sample project, but this might be changed if we made greater use of resources, such as the cloud.
