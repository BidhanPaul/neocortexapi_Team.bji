using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeoCortexApi;
using NeoCortexApi.Entities;
using System.Net.Http.Headers;
using Naa = NeoCortexApi.NeuralAssociationAlgorithm;
using System.Diagnostics;
using NeoCortexEntities.NeuroVisualizer;
using Newtonsoft.Json.Linq;


namespace UnitTestsProject
{
    [TestClass]
    public class SdrReconstructionTests
    {
        [TestCategory("SpatialPoolerReconstruction")]
        [TestMethod]
        public void Reconstruct_ValidInput_ReturnsResult()
        {
            var cfg = UnitTestHelpers.GetHtmConfig(200, 1024);

            Connections mem = new Connections(cfg);

            SpatialPoolerMT sp = new SpatialPoolerMT();
            sp.Init(mem);
            SPSdrReconstructor reconstructor = new SPSdrReconstructor(mem);

            int[] activeMiniColumns = new int[] { 0, 7, 2, 12, 24, 29, 37, 39, 46 };

            Dictionary<int, double> permanences = reconstructor.Reconstruct(activeMiniColumns);


            Assert.IsNotNull(permanences);
            Assert.IsTrue(permanences.ContainsKey(0));
            Assert.AreEqual(2.8, permanences[0], 5.0);
            Assert.IsTrue(permanences.ContainsKey(1));
            Assert.AreEqual(4.1, permanences[1], 5.5);
            Assert.IsTrue(permanences.ContainsKey(2));
            Assert.AreEqual(3.5, permanences[2], 4.0);
            Assert.IsTrue(permanences.ContainsKey(3));
            Assert.AreEqual(3.4, permanences[3], 7.0);
            Assert.IsTrue(permanences.ContainsKey(4));
            Assert.AreEqual(5.5, permanences[4], 4.5);
            Assert.IsFalse(permanences.ContainsKey(101));
        }

        [TestCategory("ReconstructionExceptionHandling")]
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Reconstruct_NullInput_ThrowsArgumentNullException()
        {
            var connections = new Connections();
            var reconstructor = new SPSdrReconstructor(connections);

            reconstructor.Reconstruct(null);

        }

        [TestCategory("ReconstructionEdgeCases")]
        [TestMethod]
        public void Reconstruct_EmptyInput_ReturnsEmptyResult()
        {

            var cfg = UnitTestHelpers.GetHtmConfig(200, 1024);
            Connections mem = new Connections(cfg);
            SpatialPoolerMT sp = new SpatialPoolerMT();
            sp.Init(mem);
            SPSdrReconstructor reconstructor = new SPSdrReconstructor(mem);


            Dictionary<int, double> permanences = reconstructor.Reconstruct(new int[0]);


            Assert.IsNotNull(permanences);
            Assert.AreEqual(0, permanences.Count);
        }

        [TestCategory("ReconstructionAllPositiveValues")]
        [TestMethod]
        public void Reconstruct_AllPositivePermanences_ReturnsExpectedValues()
        {
            var cfg = UnitTestHelpers.GetHtmConfig(200, 1024);
            Connections mem = new Connections(cfg);
            SpatialPoolerMT sp = new SpatialPoolerMT();
            sp.Init(mem);
            SPSdrReconstructor reconstructor = new SPSdrReconstructor(mem);

            int[] activeMiniColumns = new int[] { 1, 2, 3, 4, 5 };

            Dictionary<int, double> permanences = reconstructor.Reconstruct(activeMiniColumns);

            Assert.IsNotNull(permanences);

            foreach (var value in permanences.Values)
            {
                Assert.IsTrue(value >= 0, $"Expected positive value, but got {value}");
            }
        }

        /// <summary>
        /// Tests whether SPSdrReconstructor's Reconstruct method adds a key to the dictionary if it doesn't already exist.
        /// </summary>
        /// 
        [TestCategory("ReconstructionAddingKey If not Exist")]
        [TestMethod]
        public void Reconstruct_AddsKeyIfNotExists()
        {
            // Get HTM configuration
            var cfg = UnitTestHelpers.GetHtmConfig(200, 1024);


            // Initialize Connections object
            Connections mem = new Connections(cfg);


            // Initialize SpatialPoolerMT object
            SpatialPoolerMT sp = new SpatialPoolerMT();
            sp.Init(mem);


            // Initialize SPSdrReconstructor object
            SPSdrReconstructor reconstructor = new SPSdrReconstructor(mem);


            // Define active mini-columns array
            int[] activeMiniColumns = new int[] { 1, 2, 3, 4, 5, 7, 20, 54, 700 };


            // Reconstruct permanences for active mini-columns
            Dictionary<int, double> permanences = reconstructor.Reconstruct(activeMiniColumns);


            // Assert that the returned dictionary is not null
            Assert.IsNotNull(permanences);


            // Assert that the dictionary contains the key 1
            Assert.IsTrue(permanences.ContainsKey(1));


        }

        /// <summary>
        /// Tests the behavior of SPSdrReconstructor's Reconstruct method to ensure it returns a valid dictionary.
        /// </summary>


        [TestCategory("ReconstructionReturnsKvP")]
        [TestMethod]
        public void Reconstruct_ReturnsValidDictionary()
        {
            // Get HTM configuration
            var cfg = UnitTestHelpers.GetHtmConfig(200, 1024);

            // Initialize Connections object
            Connections mem = new Connections(cfg);

            
            // Initialize SpatialPoolerMT object
            SpatialPoolerMT sp = new SpatialPoolerMT();
            sp.Init(mem);

            // Initialize SPSdrReconstructor object
            SPSdrReconstructor reconstructor = new SPSdrReconstructor(mem);

           
            // Define active mini-columns array
            int[] activeMiniColumns = new int[] { 1, 2, 3 };


            // Reconstruct permanences for active mini-columns
            Dictionary<int, double> permanences = reconstructor.Reconstruct(activeMiniColumns);


            // Assert that the returned dictionary is not null
            Assert.IsNotNull(permanences);


            // Assert that all keys in the dictionary are of type int
            Assert.IsTrue(permanences.Keys.All(key => key is int));


            / Assert that all values in the dictionary are of type double
            Assert.IsTrue(permanences.Values.All(value => value is double));


        }

        /// <summary>
        /// Tests the behavior of SPSdrReconstructor's Reconstruct method when negative permanences are encountered.
        /// </summary>
        /// 

        [TestCategory("ReconstructedNegativePermanenceRetunsFalse")]
        [TestMethod]
        public void Reconstruct_NegativePermanences_ReturnsFalse()

        {
            // Get HTM configuration
            var cfg = UnitTestHelpers.GetHtmConfig(200, 1024);

            // Initialize Connections object
            Connections mem = new Connections(cfg);

            // Initialize SpatialPoolerMT object
            SpatialPoolerMT sp = new SpatialPoolerMT();
            sp.Init(mem);

            // Initialize SPSdrReconstructor object
            SPSdrReconstructor reconstructor = new SPSdrReconstructor(mem);

            // Define active mini-columns array
            int[] activeMiniColumns = new int[] { 1, 2, 3, 4, 5 };

            // Reconstruct permanences for active mini-columns
            Dictionary<int, double> permanences = reconstructor.Reconstruct(activeMiniColumns);

            // Assert that no negative permanences are present in the reconstructed values
            Assert.IsFalse(permanences.Values.Any(value => value < 0), "Result should be false due to negative permanence values");
        }
        /// <summary>
        /// Tests the behavior of reconstructing permanences when at least one permanence value is negative.
        /// The test initializes a spatial pooler configuration and connections, then reconstructs permanences using a set of active mini-columns.
        /// It checks if the resulting dictionary of permanences contains any negative values, and asserts that at least one permanence value should be negative.
        /// </summary>
        [TestCategory("ReconstructedNegativePermanenceRetunsFalse")]
        [TestMethod]
        public void Reconstruct_AtLeastOneNegativePermanence_ReturnsFalse()
        {
            // Initialize spatial pooler configuration and connections
            var cfg = UnitTestHelpers.GetHtmConfig(200, 1024);
            Connections mem = new Connections(cfg);
            SpatialPoolerMT sp = new SpatialPoolerMT();
            sp.Init(mem);
            // Initialize SPSdrReconstructor for reconstructing permanences
            SPSdrReconstructor reconstructor = new SPSdrReconstructor(mem);
            // Define a set of active mini-columns
            int[] activeMiniColumns = new int[] { 1, 2, 3, 4, 5 };

            // Reconstruct permanences based on the active mini-columns
            Dictionary<int, double> permanences = reconstructor.Reconstruct(activeMiniColumns);
            // Assert that the reconstructed dictionary is not null
            Assert.IsNotNull(permanences);
            // Assert that at least one permanence value is negative
            Assert.IsFalse(permanences.Values.Any(value => value < 0), "At least one permanence value should be negative");


        }
        /// <summary>
        /// Tests the behavior of reconstructing permanences when provided with an invalid dictionary.
        /// The test initializes a spatial pooler configuration and connections, then reconstructs permanences using a set of active mini-columns.
        /// It checks if the resulting dictionary of permanences is considered invalid, and asserts that the result should be false for an invalid dictionary.
        /// </summary>
        [TestCategory("DataIntegrityValidation")]
        [TestMethod]
        public void Reconstruct_InvalidDictionary_ReturnsFalse()
        {

            // Initialize spatial pooler configuration and connections
            var cfg = UnitTestHelpers.GetHtmConfig(100, 1024);

            Connections mem = new Connections(cfg);
            SpatialPoolerMT sp = new SpatialPoolerMT();
            sp.Init(mem);
            // Initialize SPSdrReconstructor for reconstructing permanences
            SPSdrReconstructor reconstructor = new SPSdrReconstructor(mem);

            // Define a set of active mini-columns
            int[] activeMiniColumns = new int[] { 1, 2, 3 };


            // Reconstruct permanences based on the active mini-columns
            Dictionary<int, double> permanences = reconstructor.Reconstruct(activeMiniColumns);

            // Debug trace for reconstructed permanences
            Debug.WriteLine("Reconstructed Permanences:");
            foreach (var kvp in permanences)
            {
                Debug.WriteLine($"Key: {kvp.Key}, Value: {kvp.Value}");
            }
            // Assert that the reconstructed dictionary is not considered invalid
            Assert.IsFalse(IsDictionaryInvalid(permanences), "Result should be false for an invalid dictionary");



        }
        /// <summary>
        /// Determines whether a dictionary is considered invalid based on specific criteria.
        /// </summary>
        /// <param name="dictionary">The dictionary to be checked.</param>
        /// <returns>True if the dictionary is invalid, otherwise false.</returns>
        
        [TestCategory("DictionaryValidityTests")]
        [TestMethod]
        private bool IsDictionaryInvalid(Dictionary<int, double> dictionary)
        {
            // Check if the dictionary reference is null, indicating invalidity.
            if (dictionary == null)
            {
                return true;
            }
            // Check for invalid values or keys in the dictionary.
            // Values containing NaN (Not-a-Number) are considered invalid.
            // Additionally, any keys less than 0 are also considered invalid.


            if (dictionary.Values.Any(value => double.IsNaN(value)) || dictionary.Keys.Any(key => key < 0))
            {
                return true;
            }

            return false;
        }

    }
}
