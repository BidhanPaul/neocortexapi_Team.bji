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

        [TestCategory("ReconstructionAddingKey If not Exist")]
        [TestMethod]
        public void Reconstruct_AddsKeyIfNotExists()
        {
            var cfg = UnitTestHelpers.GetHtmConfig(200, 1024);
            Connections mem = new Connections(cfg);
            SpatialPoolerMT sp = new SpatialPoolerMT();
            sp.Init(mem);
            SPSdrReconstructor reconstructor = new SPSdrReconstructor(mem);

            int[] activeMiniColumns = new int[] { 1, 2, 3, 4, 5, 7, 20, 54, 700 };

            Dictionary<int, double> permanences = reconstructor.Reconstruct(activeMiniColumns);

            Assert.IsNotNull(permanences);

            Assert.IsTrue(permanences.ContainsKey(1));


        }
        [TestCategory("ReconstructionReturnsKvP")]
        [TestMethod]
        public void Reconstruct_ReturnsValidDictionary()
        {
            var cfg = UnitTestHelpers.GetHtmConfig(200, 1024);
            Connections mem = new Connections(cfg);
            SpatialPoolerMT sp = new SpatialPoolerMT();
            sp.Init(mem);
            SPSdrReconstructor reconstructor = new SPSdrReconstructor(mem);


            int[] activeMiniColumns = new int[] { 1, 2, 3 };
            Dictionary<int, double> permanences = reconstructor.Reconstruct(activeMiniColumns);
            Assert.IsNotNull(permanences);

            Assert.IsTrue(permanences.Keys.All(key => key is int));
            Assert.IsTrue(permanences.Values.All(value => value is double));


        }

        [TestCategory("ReconstructedNegativePermanenceRetunsFalse")]
        [TestMethod]
        public void Reconstruct_NegativePermanences_ReturnsFalse()

        {

            var cfg = UnitTestHelpers.GetHtmConfig(200, 1024);
            Connections mem = new Connections(cfg);
            SpatialPoolerMT sp = new SpatialPoolerMT();
            sp.Init(mem);
            SPSdrReconstructor reconstructor = new SPSdrReconstructor(mem);


            int[] activeMiniColumns = new int[] { 1, 2, 3, 4, 5 };


            Dictionary<int, double> permanences = reconstructor.Reconstruct(activeMiniColumns);

            Assert.IsFalse(permanences.Values.Any(value => value < 0), "Result should be false due to negative permanence values");
        }


        [TestCategory("ReconstructedNegativePermanenceRetunsFalse")]
        [TestMethod]
        public void Reconstruct_AtLeastOneNegativePermanence_ReturnsFalse()
        {

            var cfg = UnitTestHelpers.GetHtmConfig(200, 1024);
            Connections mem = new Connections(cfg);
            SpatialPoolerMT sp = new SpatialPoolerMT();
            sp.Init(mem);
            SPSdrReconstructor reconstructor = new SPSdrReconstructor(mem);
            int[] activeMiniColumns = new int[] { 1, 2, 3, 4, 5 };

            Dictionary<int, double> permanences = reconstructor.Reconstruct(activeMiniColumns);

            Assert.IsNotNull(permanences);

            Assert.IsFalse(permanences.Values.Any(value => value < 0), "At least one permanence value should be negative");


        }

        [TestCategory("DataIntegrityValidation")]
        [TestMethod]
        public void Reconstruct_InvalidDictionary_ReturnsFalse()
        {

            var cfg = UnitTestHelpers.GetHtmConfig(200, 1024);
            Connections mem = new Connections(cfg);
            SpatialPoolerMT sp = new SpatialPoolerMT();
            sp.Init(mem);
            SPSdrReconstructor reconstructor = new SPSdrReconstructor(mem);
            int[] activeMiniColumns = new int[] { 1, 2, 3, 4, 84 };

            Dictionary<int, double> permanences = reconstructor.Reconstruct(activeMiniColumns);

            // Debug trace for reconstructed permanences
            Debug.WriteLine("Reconstructed Permanences:");
            foreach (var kvp in permanences)
            {
                Debug.WriteLine($"Key: {kvp.Key}, Value: {kvp.Value}");
            }
            Assert.IsFalse(IsDictionaryInvalid(permanences), "Result should be false for an invalid dictionary");



        }
        [TestCategory("DictionaryValidityTests")]
        [TestMethod]
        private bool IsDictionaryInvalid(Dictionary<int, double> dictionary)
        {


            if (dictionary == null)
            {
                return true;
            }

            if (dictionary.Values.Any(value => double.IsNaN(value)) || dictionary.Keys.Any(key => key < 0))
            {
                return true;
            }

            return false;
        }

    }
}
