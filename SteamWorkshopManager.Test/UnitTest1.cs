using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SteamWorkshopManager.Test;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void TestMethod1()
    {
        Console.WriteLine(Path.GetFullPath("."));
    }
}