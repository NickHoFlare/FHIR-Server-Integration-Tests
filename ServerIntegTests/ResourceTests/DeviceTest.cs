using System.Net;
using System.Net.Http;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServerIntegTests.ResourceTests
{
    [TestClass]
    public class DeviceTest
    {
        private FhirClient fhirClient;
        private HttpClient httpClient;
        private const string baseUrl = "https://localhost:44384/fhir";

        [TestInitialize]
        public void TestInitialize()
        {
            fhirClient = new FhirClient(baseUrl);
            httpClient = new HttpClient();
        }

        [TestMethod]
        public void TestRead()
        {
            // Case where all OK
            ResourceIdentity identity1 = ResourceIdentity.Build("Patient", "1");
            ResourceIdentity identity2 = ResourceIdentity.Build("Patient", "2");
            ResourceIdentity identity3 = ResourceIdentity.Build("Patient", "3");

            var patient1 = fhirClient.Read<Patient>(identity1);
            var patient2 = fhirClient.Read<Patient>(identity2);
            var patient3 = fhirClient.Read<Patient>(identity3);
            var response1 = httpClient.GetAsync(baseUrl + "/Patient/1").Result;
            var response2 = httpClient.GetAsync(baseUrl + "/Patient/2").Result;
            var response3 = httpClient.GetAsync(baseUrl + "/Patient/3").Result;

            Assert.AreEqual("1", patient1.Id);
            Assert.AreEqual("2", patient2.Id);
            Assert.AreEqual("3", patient3.Id);
            Assert.AreEqual(HttpStatusCode.OK, response1.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, response2.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, response3.StatusCode);

            // Case where Device does not exist
            var response4 = httpClient.GetAsync(baseUrl + "/Patient/4").Result;
            var response5 = httpClient.GetAsync(baseUrl + "/Patient/5").Result;

            Assert.AreEqual(HttpStatusCode.NotFound, response4.StatusCode);
            Assert.AreEqual(HttpStatusCode.NotFound, response5.StatusCode);
        }

        [TestMethod]
        public void TestCreate()
        {
            fhirClient.Create()
        }
    }
