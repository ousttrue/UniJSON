using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UniJson;

public class SerializerTests
{

    [Test]
    public void SerializerTestsSimplePasses()
    {
        // Use the Assert class to test conditions.

        var s = JsonSerializer.Create();

        // number
        Assert.AreEqual("0", s.Serialize(0));
    }

}
