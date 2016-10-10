using System;
using System.Collections.Generic;
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
    public class ObservationTests
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
            ResourceIdentity identity1 = ResourceIdentity.Build("Observation", "1");
            ResourceIdentity identity2 = ResourceIdentity.Build("Observation", "2");
            ResourceIdentity identity3 = ResourceIdentity.Build("Observation", "3");

            var observation1 = fhirClient.Read<Observation>(identity1);
            var observation2 = fhirClient.Read<Observation>(identity2);
            var observation3 = fhirClient.Read<Observation>(identity3);
            var response1 = httpClient.GetAsync(baseUrl + "/Observation/1").Result;
            var response2 = httpClient.GetAsync(baseUrl + "/Observation/2").Result;
            var response3 = httpClient.GetAsync(baseUrl + "/Observation/3").Result;

            Assert.AreEqual("1", observation1.Id);
            Assert.AreEqual("2", observation2.Id);
            Assert.AreEqual("3", observation3.Id);
            Assert.AreEqual(HttpStatusCode.OK, response1.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, response2.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, response3.StatusCode);

            // Case where Observation does not exist
            var response4 = httpClient.GetAsync(baseUrl + "/Observation/4").Result;
            var response5 = httpClient.GetAsync(baseUrl + "/Observation/5").Result;

            Assert.AreEqual(HttpStatusCode.NotFound, response4.StatusCode);
            Assert.AreEqual(HttpStatusCode.NotFound, response5.StatusCode);
        }

        [TestMethod]
        public void TestFhirCreate()
        {
            Observation observation1 = MockedResources.Observation1;
            Observation observation2 = MockedResources.Observation2;

            var observation1Entry = fhirClient.Create(observation1);
            var observation2Entry = fhirClient.Create(observation2);

            Assert.AreEqual("4", observation1Entry.Id);
            Assert.AreEqual("5", observation2Entry.Id);
            Assert.AreEqual(MockedResources.Observation1.Device.Reference, observation1Entry.Device.Reference);
            Assert.AreEqual(MockedResources.Observation2.Device.Reference, observation2Entry.Device.Reference);
            Assert.AreEqual(MockedResources.Observation1.Code.Coding[0].System, observation1Entry.Code.Coding[0].System);
            Assert.AreEqual(MockedResources.Observation2.Code.Coding[0].System, observation2Entry.Code.Coding[0].System);
        }

        [TestMethod]
        public void TestHttpCreate()
        {
            // Successful creation
            string observation1Json = FhirSerializer.SerializeResourceToJson(MockedResources.Observation1);
            string observation2Json = FhirSerializer.SerializeResourceToJson(MockedResources.Observation2);
            string observation1Xml = FhirSerializer.SerializeResourceToXml(MockedResources.Observation1);
            string observation2Xml = FhirSerializer.SerializeResourceToXml(MockedResources.Observation2);

            HttpRequestMessage request1 = new HttpRequestMessage(HttpMethod.Post, baseUrl + "/Observation")
            {
                Content = new StringContent(observation1Json, Encoding.UTF8, "application/json+fhir")
            };
            var response1 = httpClient.SendAsync(request1).Result;
            HttpRequestMessage request2 = new HttpRequestMessage(HttpMethod.Post, baseUrl + "/Observation")
            {
                Content = new StringContent(observation2Json, Encoding.UTF8, "application/json+fhir")
            };
            var response2 = httpClient.SendAsync(request2).Result;
            HttpRequestMessage request3 = new HttpRequestMessage(HttpMethod.Post, baseUrl + "/Observation")
            {
                Content = new StringContent(observation1Xml, Encoding.UTF8, "application/xml+fhir")
            };
            var response3 = httpClient.SendAsync(request3).Result;
            HttpRequestMessage request4 = new HttpRequestMessage(HttpMethod.Post, baseUrl + "/Observation")
            {
                Content = new StringContent(observation2Xml, Encoding.UTF8, "application/xml+fhir")
            };
            var response4 = httpClient.SendAsync(request4).Result;

            Assert.AreEqual(HttpStatusCode.Created, response1.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, response2.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, response3.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, response4.StatusCode);

            // Check that record entries are created correctly
            var recordResponse1 = httpClient.GetAsync(baseUrl + "/ObservationRecord/4").Result;
            var recordResponse2 = httpClient.GetAsync(baseUrl + "/ObservationRecord/5").Result;
            var recordResponse3 = httpClient.GetAsync(baseUrl + "/ObservationRecord/6").Result;
            var recordResponse4 = httpClient.GetAsync(baseUrl + "/ObservationRecord/7").Result;

            string record1 = recordResponse1.Content.ReadAsStringAsync().Result;
            string record2 = recordResponse2.Content.ReadAsStringAsync().Result;
            string record3 = recordResponse3.Content.ReadAsStringAsync().Result;
            string record4 = recordResponse4.Content.ReadAsStringAsync().Result;

            Assert.IsTrue(record1.Contains("\"ObservationId\":4"));
            Assert.IsTrue(record1.Contains("\"VersionId\":1"));
            Assert.IsTrue(record1.Contains("Action\":\"CREATE"));
            Assert.IsTrue(record2.Contains("\"ObservationId\":5"));
            Assert.IsTrue(record2.Contains("\"VersionId\":1"));
            Assert.IsTrue(record2.Contains("Action\":\"CREATE"));
            Assert.IsTrue(record3.Contains("\"ObservationId\":6"));
            Assert.IsTrue(record3.Contains("\"VersionId\":1"));
            Assert.IsTrue(record3.Contains("Action\":\"CREATE"));
            Assert.IsTrue(record4.Contains("\"ObservationId\":7"));
            Assert.IsTrue(record4.Contains("\"VersionId\":1"));
            Assert.IsTrue(record4.Contains("Action\":\"CREATE"));

            // Case where ID is present
            Observation ObservationIdPresent = MockedResources.Observation1;
            ObservationIdPresent.Id = "10";
            string observationIdPresentString = FhirSerializer.SerializeResourceToXml(ObservationIdPresent);

            HttpRequestMessage request5 = new HttpRequestMessage(HttpMethod.Post, baseUrl + "/Observation")
            {
                Content = new StringContent(observationIdPresentString, Encoding.UTF8, "application/xml+fhir")
            };
            var response5 = httpClient.SendAsync(request5).Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, response5.StatusCode);

            // Case where wrong type of resource is provided
            string device = FhirSerializer.SerializeResourceToJson(MockedResources.Device1);
            HttpRequestMessage request6 = new HttpRequestMessage(HttpMethod.Post, baseUrl + "/Observation")
            {
                Content = new StringContent(device, Encoding.UTF8, "application/json+fhir")
            };
            var response6 = httpClient.SendAsync(request6).Result;

            Assert.AreEqual(HttpStatusCode.NotFound, response6.StatusCode);
        }

        [TestMethod]
        public void TestFhirUpdate()
        {
            ResourceIdentity identity1 = ResourceIdentity.Build("Observation", "2");
            ResourceIdentity identity2 = ResourceIdentity.Build("Observation", "3");

            Observation observation1 = fhirClient.Read<Observation>(identity1);
            observation1.Status = Observation.ObservationStatus.Amended;
            Observation observation2 = fhirClient.Read<Observation>(identity2);
            observation2.Device = new ResourceReference() {Reference = "Device/4"};

            var observation1Entry = fhirClient.Update(observation1);
            var observation2Entry = fhirClient.Update(observation2);

            observation1 = fhirClient.Read<Observation>(identity1);
            observation2 = fhirClient.Read<Observation>(identity2);

            Assert.AreEqual("2", observation1Entry.Id);
            Assert.AreEqual("3", observation2Entry.Id);
            Assert.AreEqual(Observation.ObservationStatus.Amended, observation1Entry.Status);
            Assert.AreEqual("Device/4", observation2Entry.Device.Reference);
            Assert.AreEqual(Observation.ObservationStatus.Amended, observation1Entry.Status);
            Assert.AreEqual("Device/4", observation2Entry.Device.Reference);
            Assert.AreEqual("2", observation1Entry.Meta.VersionId);
            Assert.AreEqual("2", observation2Entry.Meta.VersionId);
        }

        [TestMethod]
        public void TestHttpUpdate()
        {
            ResourceIdentity identity1 = ResourceIdentity.Build("Observation", "2");
            ResourceIdentity identity2 = ResourceIdentity.Build("Observation", "3");

            Observation observation1 = fhirClient.Read<Observation>(identity1);
            observation1.Status = Observation.ObservationStatus.Amended;
            Observation observation2 = fhirClient.Read<Observation>(identity2);
            observation2.Device = new ResourceReference() { Reference = "Device/4" };


            // Successful update (JSON)
            string observation1Json = FhirSerializer.SerializeResourceToJson(observation1);
            string observation2Json = FhirSerializer.SerializeResourceToJson(observation2);

            HttpRequestMessage request1 = new HttpRequestMessage(HttpMethod.Put, baseUrl + "/Observation/2")
            {
                Content = new StringContent(observation1Json, Encoding.UTF8, "application/json+fhir")
            };
            var response1 = httpClient.SendAsync(request1).Result;
            HttpRequestMessage request2 = new HttpRequestMessage(HttpMethod.Put, baseUrl + "/Observation/3")
            {
                Content = new StringContent(observation2Json, Encoding.UTF8, "application/json+fhir")
            };
            var response2 = httpClient.SendAsync(request2).Result;

            // Successful update (XML)
            string observation1Xml = FhirSerializer.SerializeResourceToXml(observation1);
            string observation2Xml = FhirSerializer.SerializeResourceToXml(observation2);

            HttpRequestMessage request3 = new HttpRequestMessage(HttpMethod.Put, baseUrl + "/Observation/2")
            {
                Content = new StringContent(observation1Xml, Encoding.UTF8, "application/xml+fhir")
            };
            var response3 = httpClient.SendAsync(request3).Result;
            HttpRequestMessage request4 = new HttpRequestMessage(HttpMethod.Put, baseUrl + "/Observation/3")
            {
                Content = new StringContent(observation2Xml, Encoding.UTF8, "application/xml+fhir")
            };
            var response4 = httpClient.SendAsync(request4).Result;

            Assert.AreEqual(HttpStatusCode.OK, response1.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, response2.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, response3.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, response4.StatusCode);

            // Check that record entries are created correctly
            var recordResponse1 = httpClient.GetAsync(baseUrl + "/ObservationRecord/4").Result;
            var recordResponse2 = httpClient.GetAsync(baseUrl + "/ObservationRecord/5").Result;
            var recordResponse3 = httpClient.GetAsync(baseUrl + "/ObservationRecord/6").Result;
            var recordResponse4 = httpClient.GetAsync(baseUrl + "/ObservationRecord/7").Result;

            string record1 = recordResponse1.Content.ReadAsStringAsync().Result;
            string record2 = recordResponse2.Content.ReadAsStringAsync().Result;
            string record3 = recordResponse3.Content.ReadAsStringAsync().Result;
            string record4 = recordResponse4.Content.ReadAsStringAsync().Result;

            Assert.IsTrue(record1.Contains("\"ObservationId\":2"));
            Assert.IsTrue(record1.Contains("\"VersionId\":2"));
            Assert.IsTrue(record1.Contains("Action\":\"UPDATE"));
            Assert.IsTrue(record2.Contains("\"ObservationId\":3"));
            Assert.IsTrue(record2.Contains("\"VersionId\":2"));
            Assert.IsTrue(record2.Contains("Action\":\"UPDATE"));
            Assert.IsTrue(record3.Contains("\"ObservationId\":2"));
            Assert.IsTrue(record3.Contains("\"VersionId\":3"));
            Assert.IsTrue(record3.Contains("Action\":\"UPDATE"));
            Assert.IsTrue(record4.Contains("\"ObservationId\":3"));
            Assert.IsTrue(record4.Contains("\"VersionId\":3"));
            Assert.IsTrue(record4.Contains("Action\":\"UPDATE"));

            // Successful create(JSON)
            observation1.Id = "4";
            observation2.Id = "5";
            string observation1CreateJson = FhirSerializer.SerializeResourceToJson(observation1);
            string observation2CreateJson = FhirSerializer.SerializeResourceToJson(observation2);

            HttpRequestMessage request5 = new HttpRequestMessage(HttpMethod.Put, baseUrl + "/Observation/4")
            {
                Content = new StringContent(observation1CreateJson, Encoding.UTF8, "application/json+fhir")
            };
            var response5 = httpClient.SendAsync(request5).Result;
            HttpRequestMessage request6 = new HttpRequestMessage(HttpMethod.Put, baseUrl + "/Observation/5")
            {
                Content = new StringContent(observation2CreateJson, Encoding.UTF8, "application/json+fhir")
            };
            var response6 = httpClient.SendAsync(request6).Result;

            // Successful create(XML)
            observation1.Id = "6";
            observation2.Id = "7";
            string observation1CreateXml = FhirSerializer.SerializeResourceToXml(observation1);
            string observation2CreateXml = FhirSerializer.SerializeResourceToXml(observation2);

            HttpRequestMessage request7 = new HttpRequestMessage(HttpMethod.Put, baseUrl + "/Observation/6")
            {
                Content = new StringContent(observation1CreateXml, Encoding.UTF8, "application/xml+fhir")
            };
            var response7 = httpClient.SendAsync(request7).Result;
            HttpRequestMessage request8 = new HttpRequestMessage(HttpMethod.Put, baseUrl + "/Observation/7")
            {
                Content = new StringContent(observation2CreateXml, Encoding.UTF8, "application/xml+fhir")
            };
            var response8 = httpClient.SendAsync(request8).Result;

            Assert.AreEqual(HttpStatusCode.Created, response5.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, response6.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, response7.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, response8.StatusCode);

            // Check that record entries are created correctly
            var recordResponse5 = httpClient.GetAsync(baseUrl + "/ObservationRecord/8").Result;
            var recordResponse6 = httpClient.GetAsync(baseUrl + "/ObservationRecord/9").Result;
            var recordResponse7 = httpClient.GetAsync(baseUrl + "/ObservationRecord/10").Result;
            var recordResponse8 = httpClient.GetAsync(baseUrl + "/ObservationRecord/11").Result;

            string record5 = recordResponse5.Content.ReadAsStringAsync().Result;
            string record6 = recordResponse6.Content.ReadAsStringAsync().Result;
            string record7 = recordResponse7.Content.ReadAsStringAsync().Result;
            string record8 = recordResponse8.Content.ReadAsStringAsync().Result;

            Assert.IsTrue(record5.Contains("\"ObservationId\":4"));
            Assert.IsTrue(record5.Contains("\"VersionId\":1"));
            Assert.IsTrue(record5.Contains("Action\":\"CREATE"));
            Assert.IsTrue(record6.Contains("\"ObservationId\":5"));
            Assert.IsTrue(record6.Contains("\"VersionId\":1"));
            Assert.IsTrue(record6.Contains("Action\":\"CREATE"));
            Assert.IsTrue(record7.Contains("\"ObservationId\":6"));
            Assert.IsTrue(record7.Contains("\"VersionId\":1"));
            Assert.IsTrue(record7.Contains("Action\":\"CREATE"));
            Assert.IsTrue(record8.Contains("\"ObservationId\":7"));
            Assert.IsTrue(record8.Contains("\"VersionId\":1"));
            Assert.IsTrue(record8.Contains("Action\":\"CREATE"));

            // Case where wrong type of resource is provided
            string device = FhirSerializer.SerializeResourceToJson(MockedResources.Device1);
            HttpRequestMessage request9 = new HttpRequestMessage(HttpMethod.Put, baseUrl + "/Observation/8")
            {
                Content = new StringContent(device, Encoding.UTF8, "application/json+fhir")
            };
            var response9 = httpClient.SendAsync(request9).Result;

            Assert.AreEqual(HttpStatusCode.NotFound, response9.StatusCode);

            // Case where resource does not have logical ID
            observation1.Id = null;
            string observation1NoId = FhirSerializer.SerializeResourceToXml(observation1);
            HttpRequestMessage request10 = new HttpRequestMessage(HttpMethod.Put, baseUrl + "/Observation/8")
            {
                Content = new StringContent(observation1NoId, Encoding.UTF8, "application/xml+fhir")
            };
            var response10 = httpClient.SendAsync(request10).Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, response10.StatusCode);

            // Case where resource ID ! input resource logical ID
            observation1.Id = "2";
            string observation1NotEqualId = FhirSerializer.SerializeResourceToXml(observation1);
            HttpRequestMessage request11 = new HttpRequestMessage(HttpMethod.Put, baseUrl + "/Observation/11")
            {
                Content = new StringContent(observation1NotEqualId, Encoding.UTF8, "application/xml+fhir")
            };
            var response11 = httpClient.SendAsync(request11).Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, response11.StatusCode);
        }

        [TestMethod]
        public void TestFhirDelete()
        {
            ResourceIdentity identity1 = ResourceIdentity.Build("Observation", "1");
            ResourceIdentity identity2 = ResourceIdentity.Build("Observation", "2");

            fhirClient.Delete(identity1);
            fhirClient.Delete(identity2);

            try
            {
                fhirClient.Read<Observation>(baseUrl + "/Observation/1");
                Assert.Fail();
            }
            catch (FormatException ex)
            {
                Assert.IsTrue(ex.Message.Contains("Observation with id 1 has been deleted!"));
            }

            try
            {
                fhirClient.Read<Observation>(baseUrl + "/Observation/2");
                Assert.Fail();
            }
            catch (FormatException ex)
            {
                Assert.IsTrue(ex.Message.Contains("Observation with id 2 has been deleted!"));
            }
        }

        [TestMethod]
        public void TestHttpDelete()
        {
            // Case where delete successfully
            var response1 = httpClient.DeleteAsync(baseUrl + "/Observation/1").Result;
            var response2 = httpClient.DeleteAsync(baseUrl + "/Observation/2").Result;

            Assert.AreEqual(HttpStatusCode.NoContent, response1.StatusCode);
            Assert.AreEqual(HttpStatusCode.NoContent, response2.StatusCode);

            // Check that record entries are created correctly
            var recordResponse5 = httpClient.GetAsync(baseUrl + "/ObservationRecord/4").Result;
            var recordResponse6 = httpClient.GetAsync(baseUrl + "/ObservationRecord/5").Result;

            string record5 = recordResponse5.Content.ReadAsStringAsync().Result;
            string record6 = recordResponse6.Content.ReadAsStringAsync().Result;

            Assert.IsTrue(record5.Contains("\"ObservationId\":1"));
            Assert.IsTrue(record5.Contains("\"VersionId\":1"));
            Assert.IsTrue(record5.Contains("Action\":\"DELETE"));
            Assert.IsTrue(record6.Contains("\"ObservationId\":2"));
            Assert.IsTrue(record6.Contains("\"VersionId\":1"));
            Assert.IsTrue(record6.Contains("Action\":\"DELETE"));

            // Case where attempt delete on non-existent resource
            var response3 = httpClient.DeleteAsync(baseUrl + "/Observation/4").Result;

            Assert.AreEqual(HttpStatusCode.NoContent, response3.StatusCode);

            // Case where attempt delete on already deleted resource
            var response4 = httpClient.DeleteAsync(baseUrl + "/Observation/1").Result;
            var response5 = httpClient.DeleteAsync(baseUrl + "/Observation/2").Result;

            Assert.AreEqual(HttpStatusCode.OK, response4.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, response5.StatusCode);
        }
    }
}
