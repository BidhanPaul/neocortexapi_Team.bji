# ML 23/24-04 Implement the Spatial Pooler SDR Reconstruction
###### _Through out this project we contribute to  implement the Spatial Pooler SDR Reconstruction in NeoCortexAPI_

[![N|Logo](https://ddobric.github.io/neocortexapi/images/logo-NeoCortexAPI.svg )](https://ddobric.github.io/neocortexapi/)


In this Documentation we will describe our contribution in this project.

## Introduction
In our project to fully utilize the potential of HTM, we investigate the Reconstruct() function, the newest addition to the "NeoCortexAPI." The the core of our project is this technique, which is the inverse of SP. The project entitled "Visualization of Reconstructed Permanence Values." In order to demonstrate how HTM's Spatial Pooler applies the Reconstruct() method for input sequence rebuilding, we aim to clarify the terms between input and output.
# Methodology
Our methodology revolves around the precise reconstruction of the original input, initiated by providing numerical values ranging from 0 to 99. The encoder transforms these numerical values into int[] arrays, representing arrays of 0s and 1s, each consisting of 200 bits post-encoding. These encoded arrays become the sole input for our experiment.

**Fig: Methodology Flowchart**
![Methodology Flowchart](https://raw.githubusercontent.com/BidhanPaul/neocortexapi_team.bji/master/source/Docomentation%20neocortexapi_Team.bji/Flowchart.jpg)

## Hierarchical Temporal Memory (HTM) Spatial Pooler

The encoded int[] arrays undergo transformation using the HTM Spatial Pooler, generating Sparse Distributed Representations (SDRs). This pivotal step lays the groundwork for further exploration.

## Reconstruct() Method:

Utilizing the Neocortexapi's Reconstruct() method, we meticulously reverse the transformation of the encoded int[] arrays. The reconstructed representations are shaped by permanence values obtained from the Reconstruction method.
``` csharp
 public Dictionary<int, double> Reconstruct(int[] activeMiniColumns)
 {
     if (activeMiniColumns == null)
     {
         throw new ArgumentNullException(nameof(activeMiniColumns));
     }

     var cols = connections.GetColumnList(activeMiniColumns);

     Dictionary<int, double> permancences = new Dictionary<int, double>();

    
     foreach (var col in cols)
     {
         col.ProximalDendrite.Synapses.ForEach(s =>
         {
             double currPerm = 0.0;

             
             if (permancences.TryGetValue(s.InputIndex, out currPerm))
             {
               
                 permancences[s.InputIndex] = s.Permanence + currPerm;
             }
             else
             {
              
                 permancences[s.InputIndex] = s.Permanence;
             }
         });
     }

     return permancences;
 }
```
[Reconstruction in SP](https://github.com/BidhanPaul/neocortexapi_team.bji/blob/master/source/NeoCortexApi/SpatialPooler.cs#L1442) - Lines (1442 to 1482)

#### Reconstruct() Workflow:
- **Input Validation:** Thorough validation checks, throwing an `ArgumentNullException` if the input array of active mini-columns is null.
   
- **Column Retrieval:** Retrieve the list of columns associated with the active mini-columns from the connections.
   
- **Reconstruction Process:** Iterate through each column, accessing the synapses in its proximal dendrite.
   
- **Permanence Accumulation:** For each synapse, accumulate the permanence values for each input index in the reconstructed input dictionary.
   
- **Dictionary Update:** Update the reconstructed input dictionary, considering whether the input index already exists or needs to be added as a new key-value pair.
   
- **Result Return:** The method concludes by returning the reconstructed input as a dictionary, mapping input indices to their associated permanences.

# Running Reconstruct Method
```csharp
    private void RunRustructuringExperiment(SpatialPooler sp, EncoderBase encoder, List<double> inputValues)
{
    foreach (var input in inputValues)
    {
        var inpSdr = encoder.Encode(input);

        var actCols = sp.Compute(inpSdr, false);

        Dictionary<int, double> reconstructedPermanence = sp.Reconstruct(actCols);

        int maxInput = 200;

        Dictionary<int, double> allPermanenceDictionary = new Dictionary<int, double>();

        foreach (var kvp in reconstructedPermanence)
        {
            int inputIndex = kvp.Key;

            double probability = kvp.Value;

            allPermanenceDictionary [inputIndex] = probability;

        }
       
        for (int inputIndex = 0; inputIndex < maxInput; inputIndex++)
        {

            if (!reconstructedPermanence.ContainsKey(inputIndex))
            {

                allPermanenceDictionary[inputIndex] = 0.0;
            }
        }
        Debug.WriteLine($"Input: {input} SDR: {Helpers.StringifyVector(actCols)}");

        var ThresholdValue = 8.3;

        List<int> normalizePermanenceList = Helpers.ThresholdingProbabilities(permanenceValuesList, ThresholdValue);

        normalizedPermanence.Add(normalizePermanenceList.ToArray());
    }
}

```
[Running Reconstruct Method](https://github.com/BidhanPaul/neocortexapi_team.bji/blob/master/source/Samples/NeoCortexApiSample/SpatialPatternLearning.cs#L213) - Lines (213 to 308)
### Implementation Details:
###### Reconstruct permanence values from active columns using the Spatial Pooler
reconstructedPermanence = sp.Reconstruct(actCols)

###### Set the maximum input index
maxInput = 200
###### Note: According to the size of Encoded Inputs (200 bits)

###### Initialize a dictionary to store all input indices and their associated permanence probabilities
allPermanenceDictionary = new Dictionary<int, double>()

###### Storing Permanence in the dictionary with reconstructed permanence values
for each key-value pair (inputIndex, probability) in reconstructedPermanence
    allPermanenceDictionary[inputIndex] = probability

###### Handling Inactive Columns Permanence by assigning a default permanence value of 0.0
for inputIndex from 0 to maxInput
    if inputIndex not in reconstructedPermanence
        allPermanenceDictionary[inputIndex] = 0.0

###### Note: reconstructedPermanence is a subset contributing to the construction of allPermanenceDictionary

## Getting Data For Visualizing Permanence Values
```csharp
    //Getting The Heatmap data from Reconstructed Permanence as Double
     List<List<double>> heatmapData = new List<List<double>>();
    //Getting The Nomalize Permanence as int
     List<int[]> normalizedPermanence = new List<int[]>();
```
## Normalizing the Permanence Values
```csharp
   //We used the Threshold values 8.3 to normalize the permanence
   var ThresholdValue = 8.3;
   //calling the function ThresholdingProbabilities from Helpers.cs
List<int> normalizePermanenceList = Helpers.ThresholdingProbabilities(permanenceValuesList, ThresholdValue);
  //Converting normalizedPermanence into Array
normalizedPermanence.Add(normalizePermanenceList.ToArray());
```
###### Note: The Threshold Value 8.3 has the ability to Normalize The permanence with the most similiraty with Encoded Inputs. We tried multiple Threshold values and Debugged the output and compared with encoded inputs.
## Generate1DHeatmaps Function
```csharp
   private void Generate1DHeatmaps(List<List<double>> heatmapData, List<int[]> normalizedPermanence)
{
    int i = 1;

    foreach (var values in heatmapData)
    {
       
        string folderPath = Path.Combine(Environment.CurrentDirectory, "1DHeatMap");

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, $"heatmap_{i}.png");
        Debug.WriteLine($"FilePath: {filePath}");
      
        double[] array1D = values.ToArray();
       
        NeoCortexUtils.Draw1DHeatmap(new List<double[]>() { array1D }, new List<int[]>() { normalizedPermanence[i - 1] }, filePath, 200, 8, 9, 4, 0, 30);

        Debug.WriteLine("Heatmap generated and saved successfully.");
        i++;
    }
}
```
[GenarateHeatmap Function](https://github.com/BidhanPaul/neocortexapi_team.bji/blob/master/source/Samples/NeoCortexApiSample/SpatialPatternLearning.cs#L311) - Lines (311 to 341)
###### Parameters
- `heatmapData`: A list of lists containing probability data for heatmap generation.
- `normalizedPermanence`: A list of arrays containing normalized permanence values corresponding to the heatmap data.

###### Implementation
- The function iterates through each set of probabilities in `heatmapData`.

###### Folder and File Management:
- A folder path is defined based on the current environment, specifically within the "1DHeatMap" directory.
- If the folder does not exist, it is created to ensure proper organization.
- The file path for each heatmap is constructed dynamically using the folder path and an index (`i`).

###### 1D Array Conversion:
- The probabilities list is converted into a 1D array (`array1D`) using the `ToArray` method for compatibility with the subsequent heatmap generation process.

###### Heatmap Generation:
- The function calls a modified version of the `Draw1DHeatmapWithSeparatedValues` function from the `NeoCortexUtils` class.
- This function handles the visualization process, considering the 1D array of probabilities (`array1D`) and the corresponding normalized permanence values.
- Key parameters, such as file path, dimensions, and visualization settings, are dynamically adjusted for each iteration.
###### Note: Heatmap Generation Parameters

- **`filePath`**: File path where the heatmap image will be saved.
- **`width`**: 200 (pixels) - Width of the heatmap image.
- **`height`**: 8 (pixels) - Height of the heatmap image.
- **`mostHeatedColor`**: 9 - Value for the most heated color (Red represents 1).
- **`medianValue`**: 4 - Median value for color interpolation.
  - Example: Greater than 4 represents orange to red, less than 4 represents green to yellow.
- **`coldestColor`**: 0 - Coldest color representing 0 bits.
- **`enlargementFactor`**: 30 - Enlargement factor used to magnify the image for better visualization.


###### Debugging Information:
- Debugging information, including file paths and successful heatmap generation confirmation, is output using `Debug.WriteLine`.
## Calling HeatMap Function
```csharp
//Calling the HeatMap Function in RunRestructuringExperiment with two Perameters
Generate1DHeatmaps(heatmapData, normalizedPermanence);
```

## Dual Visualization: Heatmaps and int[] Sequences
We Applied this Function to Draw1DHeatmap
Click Below for More Details 
[Draw1dHeatmap](https://github.com/BidhanPaul/neocortexapi_team.bji/blob/master/source/NeoCortexUtils/NeoCortexUtils.cs) - Lines (220 to 306)
**Outcomes:**
- HeatMap Image for all inputs as Image Visualization.
- Reconstruced Input as int [] (Normalized Permanence)
- Combined Image.


**Results Example:**
**Fig: Final Outcome**
![Final Outcome](https://raw.githubusercontent.com/BidhanPaul/neocortexapi_team.bji/master/source/Docomentation%20neocortexapi_Team.bji/Final_Outcome_Example_heatmap_1.png)
## UnitTest of SdrReconstructionTests
We Tested the SdrReconstruction.cs with 9 Test cases and all Passed
This document provides an overview of the unit tests present in the project.
[SdrReconstructionTests](https://github.com/BidhanPaul/neocortexapi_team.bji/blob/master/source/UnitTestsProject/SdrReconstructionTests.cs)

## Spatial Pooler Reconstruction Tests

### Reconstruct_ValidInput_ReturnsResult
- **Test Category:** SpatialPoolerReconstruction
- **Description:** Verifies whether the `Reconstruct` method in the `SPSdrReconstructor` class behaves correctly under valid input conditions. It ensures that the method returns a dictionary containing keys for all provided active mini-columns, with corresponding permanence values. Additionally, it confirms that the method properly handles the case where a key is not present in the dictionary.

### Reconstruct_NullInput_ThrowsArgumentNullException
- **Test Category:** ReconstructionExceptionHandling
- **Description:** Verifies that the `Reconstruct` method in the `SPSdrReconstructor` class throws an `ArgumentNullException` when invoked with a null input parameter.

### Reconstruct_EmptyInput_ReturnsEmptyResult
- **Test Category:** ReconstructionEdgeCases
- **Description:** Tests whether the `Reconstruct` method returns an empty dictionary when provided with an empty input.

## Reconstruction Tests for Various Scenarios

### Reconstruct_AllPositivePermanences_ReturnsExpectedValues
- **Test Category:** ReconstructionAllPositiveValues
- **Description:** Checks if the `Reconstruct` method in the `SPSdrReconstructor` class handles a scenario where all mini-column indices provided as input are positive integers and returns permanence values that are non-negative.

### Reconstruct_AddsKeyIfNotExists
- **Test Category:** ReconstructionAddingKeyIfNotExist
- **Description:** Ensures that the `Reconstruct` method adds a key to the dictionary if it doesn't already exist.

### Reconstruct_ReturnsValidDictionary
- **Test Category:** ReconstructionReturnsKvP
- **Description:** Validates whether the `Reconstruct` method returns a valid dictionary containing integer keys and double values.

### Reconstruct_NegativePermanences_ReturnsFalse
- **Test Category:** ReconstructedNegativePermanenceRetunsFalse
- **Description:** Tests the behavior of the `Reconstruct` method when encountering negative permanences and asserts that no negative permanences should be present in the reconstructed values.

### Reconstruct_AtLeastOneNegativePermanence_ReturnsFalse
- **Test Category:** ReconstructedNegativePermanenceRetunsFalse
- **Description:** Validates the behavior of the `Reconstruct` method when at least one permanence value is negative.

### Reconstruct_InvalidDictionary_ReturnsFalse
- **Test Category:** DataIntegrityValidation
- **Description:** Verifies if the `Reconstruct` method returns a valid dictionary by checking specific criteria such as NaN values and keys less than 0.

### IsDictionaryInvalid with Not a Number
- **Test Category:** DictionaryValidityTests
- **Description:** Determines whether a dictionary is considered invalid based on specific criteria like null reference, NaN values, and keys less than 0.

