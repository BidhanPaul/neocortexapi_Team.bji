using NeoCortexApi;
using NeoCortexApi.Entities;
using NUnit.Framework;
using System;

[TestFixture]
public class HomeostaticPlasticityControllerTests
{
    [Test]
    public void Stability_Event_Fired_Correctly()
    {
        // Arrange
        var inputValues = new List<int>() { 1, 2, 3 }; // Example input values
        var mem = new Connections(); // Assuming Memory class is defined
        bool isInStableState = false;

        // Create an instance of HomeostaticPlasticityController and subscribe to the stability event
        var hpa = new HomeostaticPlasticityController(mem, inputValues.Count * 40,
            (isStable, numPatterns, actColAvg, seenInputs) =>
            {
                if (isStable == false)
                {
                    // Log message for unstable state
                    System.Diagnostics.Debug.WriteLine($"INSTABLE STATE");
                    isInStableState = false;
                }
                else
                {
                    // Log message for stable state
                    System.Diagnostics.Debug.WriteLine($"STABLE STATE");
                    isInStableState = true;
                }
            });

        // Act: Simulate entering stable state by invoking the event handler with isStable = true
        hpa.HandleStabilityEvent(true, 3, 4.5, new List<int>() { 1, 2, 3 });

        // Assert
        Assert.IsTrue(isInStableState, "System should be in stable state after handling stability event.");
    }


}
