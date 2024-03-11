﻿using NeoCortex;
using NeoCortexApi;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NeoCortexApiSample
{
    /// <summary>
    /// Implements an experiment that demonstrates how to learn spatial patterns.
    /// SP will learn every presented input in multiple iterations.
    /// </summary>
    public class SpatialPatternLearning
    {
        public void Run()
        {
            Console.WriteLine($"Hello NeocortexApi! Experiment {nameof(SpatialPatternLearning)}");

            // Used as a boosting parameters
            // that ensure homeostatic plasticity effect.
            double minOctOverlapCycles = 1.0;
            double maxBoost = 5.0;

            // We will use 200 bits to represent an input vector (pattern).
            int inputBits = 200;

            // We will build a slice of the cortex with the given number of mini-columns
            int numColumns = 1024;

            //
            // This is a set of configuration parameters used in the experiment.
            HtmConfig cfg = new HtmConfig(new int[] { inputBits }, new int[] { numColumns })
            {
                CellsPerColumn = 10,
                MaxBoost = maxBoost,
                DutyCyclePeriod = 100,
                MinPctOverlapDutyCycles = minOctOverlapCycles,

                GlobalInhibition = false,
                NumActiveColumnsPerInhArea = 0.02 * numColumns,
                PotentialRadius = (int)(0.15 * inputBits),
                LocalAreaDensity = -1,
                ActivationThreshold = 10,
                
                MaxSynapsesPerSegment = (int)(0.01 * numColumns),
                Random = new ThreadSafeRandom(42),
                StimulusThreshold=10,
            };

            double max = 100;

            //
            // This dictionary defines a set of typical encoder parameters.
            Dictionary<string, object> settings = new Dictionary<string, object>()
            {
                { "W", 15},
                { "N", inputBits},
                { "Radius", -1.0},
                { "MinVal", 0.0},
                { "Periodic", false},
                { "Name", "scalar"},
                { "ClipInput", false},
                { "MaxVal", max}
            };


            EncoderBase encoder = new ScalarEncoder(settings);

            //
            // We create here 100 random input values.
            List<double> inputValues = new List<double>();

            for (int i = 0; i < (int)max; i++)
            {
                inputValues.Add((double)i);
            }

            var sp = RunExperiment(cfg, encoder, inputValues);

            RunRustructuringExperiment(sp, encoder, inputValues);
        }

       

        /// <summary>
        /// Implements the experiment.
        /// </summary>
        /// <param name="cfg"></param>
        /// <param name="encoder"></param>
        /// <param name="inputValues"></param>
        /// <returns>The trained bersion of the SP.</returns>
        private static SpatialPooler RunExperiment(HtmConfig cfg, EncoderBase encoder, List<double> inputValues)
        {
            // Creates the htm memory.
            var mem = new Connections(cfg);

            bool isInStableState = false;

            //
            // HPC extends the default Spatial Pooler algorithm.
            // The purpose of HPC is to set the SP in the new-born stage at the begining of the learning process.
            // In this stage the boosting is very active, but the SP behaves instable. After this stage is over
            // (defined by the second argument) the HPC is controlling the learning process of the SP.
            // Once the SDR generated for every input gets stable, the HPC will fire event that notifies your code
            // that SP is stable now.
            HomeostaticPlasticityController hpa = new HomeostaticPlasticityController(mem, inputValues.Count * 40,
                (isStable, numPatterns, actColAvg, seenInputs) =>
                {
                    // Event should only be fired when entering the stable state.
                    // Ideal SP should never enter unstable state after stable state.
                    if (isStable == false)
                    {
                        Debug.WriteLine($"INSTABLE STATE");
                        // This should usually not happen.
                        isInStableState = false;
                    }
                    else
                    {
                        Debug.WriteLine($"STABLE STATE");
                        // Here you can perform any action if required.
                        isInStableState = true;
                    }
                });

            // It creates the instance of Spatial Pooler Multithreaded version.
            SpatialPooler sp = new SpatialPooler(hpa);
            //sp = new SpatialPoolerMT(hpa);

            // Initializes the 
            sp.Init(mem, new DistributedMemory() { ColumnDictionary = new InMemoryDistributedDictionary<int, NeoCortexApi.Entities.Column>(1) });

            // mem.TraceProximalDendritePotential(true);

            // It creates the instance of the neo-cortex layer.
            // Algorithm will be performed inside of that layer.
            CortexLayer<object, object> cortexLayer = new CortexLayer<object, object>("L1");

            // Add encoder as the very first module. This model is connected to the sensory input cells
            // that receive the input. Encoder will receive the input and forward the encoded signal
            // to the next module.
            cortexLayer.HtmModules.Add("encoder", encoder);

            // The next module in the layer is Spatial Pooler. This module will receive the output of the
            // encoder.
            cortexLayer.HtmModules.Add("sp", sp);

            double[] inputs = inputValues.ToArray();

            // Will hold the SDR of every inputs.
            Dictionary<double, int[]> prevActiveCols = new Dictionary<double, int[]>();

            // Will hold the similarity of SDKk and SDRk-1 fro every input.
            Dictionary<double, double> prevSimilarity = new Dictionary<double, double>();

            //
            // Initiaize start similarity to zero.
            foreach (var input in inputs)
            {
                prevSimilarity.Add(input, 0.0);
                prevActiveCols.Add(input, new int[0]);
            }

            // Learning process will take 1000 iterations (cycles)
            int maxSPLearningCycles = 1000;

            int numStableCycles = 0;

            for (int cycle = 0; cycle < maxSPLearningCycles; cycle++)
            {
                Debug.WriteLine($"Cycle  ** {cycle} ** Stability: {isInStableState}");

                //
                // This trains the layer on input pattern.
                foreach (var input in inputs)
                {
                    double similarity;

                    // Learn the input pattern.
                    // Output lyrOut is the output of the last module in the layer.
                    // 
                    var lyrOut = cortexLayer.Compute((object)input, true) as int[];

                    // This is a general way to get the SpatialPooler result from the layer.
                    var activeColumns = cortexLayer.GetResult("sp") as int[];

                    var actCols = activeColumns.OrderBy(c => c).ToArray();

                    similarity = MathHelpers.CalcArraySimilarity(activeColumns, prevActiveCols[input]);

                    Debug.WriteLine($"[cycle={cycle.ToString("D4")}, i={input}, cols=:{actCols.Length} s={similarity}] SDR: {Helpers.StringifyVector(actCols)}");

                    prevActiveCols[input] = activeColumns;
                    prevSimilarity[input] = similarity;
                }

                if (isInStableState)
                {
                    numStableCycles++;
                }

                if (numStableCycles > 5)
                    break;
            }

            return sp;
        }

        /// <summary>
        /// Executes an experiment to analyze and visualize the behavior of a Spatial Pooler (SP) in response to a sequence of encoded input values. 
        /// This method systematically encodes each input value into a Sparse Distributed Representation (SDR) using the specified encoder, 
        /// then processes these SDRs through the SP to identify active columns. It reconstructs permanence values for these active columns, 
        /// normalizes them against a predefined threshold, and aggregates this data to generate visual heatmaps. These heatmaps illustrate 
        /// how the SP's internal representations of inputs evolve over time, enabling a deeper understanding of its learning and memory processes.
        /// Additionally, the method assesses the SP's ability to adapt its synaptic connections (permanences) in response to the inputs, 
        /// thereby effectively 'training' the SP through exposure to the dataset. The experiment aims to shed light on the dynamics of synaptic 
        /// plasticity within the SP framework, offering insights that could guide the tuning of its parameters for improved performance in specific tasks.
        /// </summary>
        /// <param name="sp">The Spatial Pooler instance to be used for the experiment. It processes input SDRs to simulate neural activity and synaptic plasticity.</param>
        /// <param name="encoder">The encoder used for converting raw input values into SDRs. The quality of encoding directly influences the SP's performance and the experiment's outcomes.</param>
        /// <param name="inputValues">A list of input values to be encoded and processed through the SP. These values serve as the experimental dataset, exposing the SP to various patterns and contexts.</param>
        /// <returns>The trained version of the SP after it has been exposed to all input values and adjusted its synaptic connections accordingly. This trained SP is expected to have refined its internal representations and synaptic efficiencies, making it better suited for processing similar inputs in the future.</returns>
        /// <remarks>
        /// The method assumes the SP and encoder are properly initialized and configured before being passed as arguments. The 'cfg' parameter, 
        /// mentioned in the context but not present in the provided code, is presumed to contain configuration settings for the SP and encoder, 
        /// possibly including parameters such as learning rates, permanence thresholds, and encoder-specific settings. Adjusting these configurations 
        /// could significantly impact the experiment's outcomes by altering the SP's learning dynamics and the quality of input representations.
        /// </remarks>



        // Define a method to run the restructuring experiment, which takes a spatial pooler, an encoder, and a list of input values as arguments.
        private void RunRustructuringExperiment(SpatialPooler sp, EncoderBase encoder, List<double> inputValues)
        {
            // Initialize a list to get heatmap data for all input values.
            List<List<double>> heatmapData = new List<List<double>>();

            // Initialize a list to get normalized permanence values.
            List<int[]> normalizedPermanence = new List<int[]>();

           // Loop through each input value in the list of input values.
            foreach (var input in inputValues)
            {
                // Encode the current input value using the provided encoder, resulting in an SDR
                var inpSdr = encoder.Encode(input);

                // Compute the active columns in the spatial pooler for the given input SDR, without learning.
                var actCols = sp.Compute(inpSdr, false);

                // Reconstruct the permanence values for the active columns.
                Dictionary<int, double> reconstructedPermanence = sp.Reconstruct(actCols);

                // Define the maximum number of inputs to consider.
                int maxInput = 200;

                // Initialize a dictionary to hold all permanence values, including those not directly reconstructed.
                Dictionary<int, double> allPermanenceDictionary = new Dictionary<int, double>();

                // Populate the all permanence dictionary with reconstructed permanence values.
                foreach (var kvp in reconstructedPermanence)
                {
                    int inputIndex = kvp.Key;

                    double probability = kvp.Value;

                    allPermanenceDictionary [inputIndex] = probability;

                }
                // Ensure that all input indices up to the maximum are represented in the dictionary, even if their permanence is 0.
                for (int inputIndex = 0; inputIndex < maxInput; inputIndex++)
                {

                    if (!reconstructedPermanence.ContainsKey(inputIndex))
                    {

                        allPermanenceDictionary[inputIndex] = 0.0;
                    }
                }
                // Convert the dictionary of all permanences to a list and add it to the heatmap data.
                List<double> permanenceValuesList = allPermanenceDictionary.Values.ToList();

                heatmapData.Add(permanenceValuesList);

                // Output debug information showing the input value and its corresponding SDR as a string.
                Debug.WriteLine($"Input: {input} SDR: {Helpers.StringifyVector(actCols)}");

                // Define a threshold value for normalizing permanences.
                var ThresholdValue = 8.3;

                // Normalize permanences based on the threshold value and convert them to a list of integers.
                List<int> normalizePermanenceList = Helpers.ThresholdingProbabilities(permanenceValuesList, ThresholdValue);
               
                // Add the normalized permanences to the list of all normalized permanences.
                normalizedPermanence.Add(normalizePermanenceList.ToArray());

            }
            // Generate 1D heatmaps using the heatmap data and the normalized permanences.
            Generate1DHeatmaps(heatmapData, normalizedPermanence);
        }

        private void Generate1DHeatmaps(List<List<double>> heatmapData, List<int[]> normalizedPermanence)
        {
            int i = 1;

            foreach (var values in heatmapData)
            {
                string filePath = $"heatmap_{i}.png";

                Debug.WriteLine($"FilePath: {filePath}");

                // Add the normalized permanences to the list of all normalized permanences.
                double[] heatmapValuesArray = values.ToArray();
                //Have to pass the perameteres for heatmaps
                NeoCortexUtils.Draw1dHeatmap(new List<double[]>() { heatmapValuesArray }, new List<int[]>() { normalizedPermanence[i - 1] });

                Debug.WriteLine($"HeatMap Genarated Successfully");

                i++;

                // Generating heatmap
            }


        }

    }
}
