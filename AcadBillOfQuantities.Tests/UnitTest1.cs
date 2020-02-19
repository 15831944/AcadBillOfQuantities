using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using AcadBillOfQuantities.Windows.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AcadBillOfQuantities.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string assemblyDir = Path.GetDirectoryName(assemblyPath);
            string thisDir = Directory.GetCurrentDirectory();
            string configFilePath = Path.Combine(thisDir, "Categories.xml");

            var serializer = new XmlSerializer(type: typeof(Settings));
            Settings settings;
            using (var stream = XmlReader.Create(configFilePath))
            {
                settings = (Settings)serializer.Deserialize(stream);
            }
        }
    }
}
