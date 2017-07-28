using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace PowerBIExtractor.Tests
{
    [TestClass]
    public class JsonManipulationTests
    {
        [TestMethod]
        public void TestPropertyCounter()
        {
            JObject jsonObjects = JObject.Parse(JsonScripts.TestJson);

            string[] propertiesToCount = { "x", "y", "z" };
            int numberOfMatchingProperties = JsonUtil.CountProperties(jsonObjects, propertiesToCount);

            Assert.AreEqual(18, numberOfMatchingProperties);
        }

        [TestMethod]
        public void TestRemoveProperties()
        {
            JObject jsonObjects = JObject.Parse(JsonScripts.TestJson);

            string[] propertiesToRemove = { "x", "y", "z", "width", "height", "id" };
            JsonUtil.RemoveJsonProperties(jsonObjects, propertiesToRemove);

            int numberOfProperties = JsonUtil.CountProperties(jsonObjects);

            Assert.AreEqual(13, numberOfProperties);

        }

        [TestMethod]
        public void TestCollapseProperties()
        {
            JObject jsonObjects = JObject.Parse(JsonScripts.TestJson);

            string[] propertiesToCollapse = { "config" };
            JsonUtil.CollapseJsonProperties(jsonObjects, propertiesToCollapse);

            int numberOfProperties = JsonUtil.CountProperties(jsonObjects);

            Assert.AreEqual(19, numberOfProperties);
        }

        [TestMethod]
        public void TestSortArrayByProperties()
        {
            JObject jsonObjects = JObject.Parse(JsonScripts.TestJson);

            string[] propertiesToSortBy = { "x", "y", "z" };
            JsonUtil.SortJsonProperties(jsonObjects, propertiesToSortBy);

            var firstItem = jsonObjects["items"][0];
            Assert.AreEqual(0, (int)firstItem["x"]);
            Assert.AreEqual(0, (int)firstItem["y"]);
            Assert.AreEqual(900, (int)firstItem["z"]);

            var secondItem = jsonObjects["items"][1];
            Assert.AreEqual(0, (int)secondItem["x"]);
            Assert.AreEqual(900, (int)secondItem["y"]);
            Assert.AreEqual(0, (int)secondItem["z"]);


        }

    }
}
