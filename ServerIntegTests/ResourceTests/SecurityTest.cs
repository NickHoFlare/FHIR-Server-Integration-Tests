using System;
using System.Net;
using System.Net.Http;
using System.Text;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServerIntegTests.ResourceTests
{
    [TestClass]
    public class SecurityTest
    {
        private FhirClient fhirClient;
        private HttpClient httpClient;
        private const string baseUrl = "http://localhost:49333/fhir";

        [TestInitialize]
        public void TestInitialize()
        {
            fhirClient = new FhirClient(baseUrl);
            httpClient = new HttpClient();
        }

        [TestMethod]
        public void TestRead()
        {
            var response1 = httpClient.GetAsync(baseUrl + "/Patient/1").Result;
            var response2 = httpClient.GetAsync(baseUrl + "/Patient/2").Result;
            var response3 = httpClient.GetAsync(baseUrl + "/Device/1").Result;
            var response4 = httpClient.GetAsync(baseUrl + "/Device/2").Result;
            var response5 = httpClient.GetAsync(baseUrl + "/Observation/1").Result;
            var response6 = httpClient.GetAsync(baseUrl + "/Observation/2").Result;

            Assert.AreEqual(HttpStatusCode.Forbidden, response1.StatusCode);
            Assert.AreEqual(HttpStatusCode.Forbidden, response2.StatusCode);
            Assert.AreEqual(HttpStatusCode.Forbidden, response3.StatusCode);
            Assert.AreEqual(HttpStatusCode.Forbidden, response4.StatusCode);
            Assert.AreEqual(HttpStatusCode.Forbidden, response5.StatusCode);
            Assert.AreEqual(HttpStatusCode.Forbidden, response6.StatusCode);
        }

        [TestMethod]
        public void TestCreate()
        {
            string patient1Json = FhirSerializer.SerializeResourceToJson(MockedResources.Patient1);
            string patient1Xml = FhirSerializer.SerializeResourceToXml(MockedResources.Patient1);
            string device1Json = FhirSerializer.SerializeResourceToJson(MockedResources.Device1);
            string device1Xml = FhirSerializer.SerializeResourceToXml(MockedResources.Device1);
            string observation1Json = FhirSerializer.SerializeResourceToJson(MockedResources.Observation1);
            string observation1Xml = FhirSerializer.SerializeResourceToXml(MockedResources.Observation1);

            HttpRequestMessage request1 = new HttpRequestMessage(HttpMethod.Post, baseUrl + "/Patient")
            {
                Content = new StringContent(patient1Json, Encoding.UTF8, "application/json+fhir")
            };
            var response1 = httpClient.SendAsync(request1).Result;
            HttpRequestMessage request2 = new HttpRequestMessage(HttpMethod.Post, baseUrl + "/Device")
            {
                Content = new StringContent(device1Json, Encoding.UTF8, "application/json+fhir")
            };
            var response2 = httpClient.SendAsync(request2).Result;
            HttpRequestMessage request3 = new HttpRequestMessage(HttpMethod.Post, baseUrl + "/Observation")
            {
                Content = new StringContent(observation1Json, Encoding.UTF8, "application/json+fhir")
            };
            var response3 = httpClient.SendAsync(request3).Result;
            HttpRequestMessage request4 = new HttpRequestMessage(HttpMethod.Post, baseUrl + "/Patient")
            {
                Content = new StringContent(patient1Xml, Encoding.UTF8, "application/xml+fhir")
            };
            var response4 = httpClient.SendAsync(request4).Result;
            HttpRequestMessage request5 = new HttpRequestMessage(HttpMethod.Post, baseUrl + "/Device")
            {
                Content = new StringContent(device1Xml, Encoding.UTF8, "application/xml+fhir")
            };
            var response5 = httpClient.SendAsync(request5).Result;
            HttpRequestMessage request6 = new HttpRequestMessage(HttpMethod.Post, baseUrl + "/Observation")
            {
                Content = new StringContent(observation1Xml, Encoding.UTF8, "application/xml+fhir")
            };
            var response6 = httpClient.SendAsync(request6).Result;

            Assert.AreEqual(HttpStatusCode.Forbidden, response1.StatusCode);
            Assert.AreEqual(HttpStatusCode.Forbidden, response2.StatusCode);
            Assert.AreEqual(HttpStatusCode.Forbidden, response3.StatusCode);
            Assert.AreEqual(HttpStatusCode.Forbidden, response4.StatusCode);
            Assert.AreEqual(HttpStatusCode.Forbidden, response5.StatusCode);
            Assert.AreEqual(HttpStatusCode.Forbidden, response6.StatusCode);
        }

        [TestMethod]
        public void TestUpdate()
        {
            ResourceIdentity identity1 = ResourceIdentity.Build("Patient", "2");
            ResourceIdentity identity2 = ResourceIdentity.Build("Device", "2");
            ResourceIdentity identity3 = ResourceIdentity.Build("Observation", "2");

            try
            {
                Patient patient = fhirClient.Read<Patient>(identity1);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("Operation was unsuccessful, and returned status 403"));
            }
            try
            {
                Device device = fhirClient.Read<Device>(identity2);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("Operation was unsuccessful, and returned status 403"));
            }
            try
            {
                Observation observation = fhirClient.Read<Observation>(identity3);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("Operation was unsuccessful, and returned status 403"));
            }
        }

        [TestMethod]
        public void TestDelete()
        {
            var response1 = httpClient.DeleteAsync(baseUrl + "/Patient/1").Result;
            var response2 = httpClient.DeleteAsync(baseUrl + "/Device/2").Result;
            var response3 = httpClient.DeleteAsync(baseUrl + "/Observation/1").Result;

            Assert.AreEqual(HttpStatusCode.Forbidden, response1.StatusCode);
            Assert.AreEqual(HttpStatusCode.Forbidden, response2.StatusCode);
            Assert.AreEqual(HttpStatusCode.Forbidden, response3.StatusCode);
        }
    }
}
