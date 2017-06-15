using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GeoProject.Library;

namespace GeoProject.Tester
{
    [TestClass]
    public class GeoHashUnitTest
    {
        [TestMethod]
        public void TestGeoHash()
        {
            var ss = new GeoHash().Encode(170.7896, -78.11868);

            var loc = new GeoHash().Decode(ss);
        }
    }
}
