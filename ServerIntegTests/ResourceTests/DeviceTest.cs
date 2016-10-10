using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
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
            ResourceIdentity identity1 = ResourceIdentity.Build("Device", "1");
            ResourceIdentity identity2 = ResourceIdentity.Build("Device", "2");
            ResourceIdentity identity3 = ResourceIdentity.Build("Device", "3");

            var device1 = fhirClient.Read<Device>(identity1);
            var device2 = fhirClient.Read<Device>(identity2);
            var device3 = fhirClient.Read<Device>(identity3);
            var response1 = httpClient.GetAsync(baseUrl + "/Device/1").Result;
            var response2 = httpClient.GetAsync(baseUrl + "/Device/2").Result;
            var response3 = httpClient.GetAsync(baseUrl + "/Device/3").Result;

            Assert.AreEqual("1", device1.Id);
            Assert.AreEqual("2", device2.Id);
            Assert.AreEqual("3", device3.Id);
            Assert.AreEqual(HttpStatusCode.OK, response1.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, response2.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, response3.StatusCode);

            // Case where Device does not exist
            var response4 = httpClient.GetAsync(baseUrl + "/Device/4").Result;
            var response5 = httpClient.GetAsync(baseUrl + "/Device/5").Result;

            Assert.AreEqual(HttpStatusCode.NotFound, response4.StatusCode);
            Assert.AreEqual(HttpStatusCode.NotFound, response5.StatusCode);
        }

        [TestMethod]
        public void TestFhirCreate()
        {
            Device device1 = MockedResources.Device1;
            Device device2 = MockedResources.Device2;

            var device1Entry = fhirClient.Create(device1);
            var device2Entry = fhirClient.Create(device2);

            Assert.AreEqual("4", device1Entry.Id);
            Assert.AreEqual("5", device2Entry.Id);
            Assert.AreEqual(MockedResources.Device1.Model, device1Entry.Model);
            Assert.AreEqual(MockedResources.Device2.Model, device2Entry.Model);
            Assert.AreEqual(MockedResources.Device1.Type.Text, device1Entry.Type.Text);
            Assert.AreEqual(MockedResources.Device2.Type.Text, device2Entry.Type.Text);
        }

        [TestMethod]
        public void TestHttpCreate()
        {
            // Successful creation
            string device1Json = FhirSerializer.SerializeResourceToJson(MockedResources.Device1);
            string device2Json = FhirSerializer.SerializeResourceToJson(MockedResources.Device2);
            string device1Xml = FhirSerializer.SerializeResourceToXml(MockedResources.Device1);
            string device2Xml = FhirSerializer.SerializeResourceToXml(MockedResources.Device2);

            HttpRequestMessage request1 = new HttpRequestMessage(HttpMethod.Post, baseUrl + "/Device")
            {
                Content = new StringContent(device1Json, Encoding.UTF8, "application/json+fhir")
            };
            var response1 = httpClient.SendAsync(request1).Result;
            HttpRequestMessage request2 = new HttpRequestMessage(HttpMethod.Post, baseUrl + "/Device")
            {
                Content = new StringContent(device2Json, Encoding.UTF8, "application/json+fhir")
            };
            var response2 = httpClient.SendAsync(request2).Result;
            HttpRequestMessage request3 = new HttpRequestMessage(HttpMethod.Post, baseUrl + "/Device")
            {
                Content = new StringContent(device1Xml, Encoding.UTF8, "application/xml+fhir")
            };
            var response3 = httpClient.SendAsync(request3).Result;
            HttpRequestMessage request4 = new HttpRequestMessage(HttpMethod.Post, baseUrl + "/Device")
            {
                Content = new StringContent(device2Xml, Encoding.UTF8, "application/xml+fhir")
            };
            var response4 = httpClient.SendAsync(request4).Result;

            Assert.AreEqual(HttpStatusCode.Created, response1.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, response2.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, response3.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, response4.StatusCode);

            // Check that record entries are created correctly
            var recordResponse1 = httpClient.GetAsync(baseUrl + "/Devicerecord/4").Result;
            var recordResponse2 = httpClient.GetAsync(baseUrl + "/Devicerecord/5").Result;
            var recordResponse3 = httpClient.GetAsync(baseUrl + "/Devicerecord/6").Result;
            var recordResponse4 = httpClient.GetAsync(baseUrl + "/Devicerecord/7").Result;

            string record1 = recordResponse1.Content.ReadAsStringAsync().Result;
            string record2 = recordResponse2.Content.ReadAsStringAsync().Result;
            string record3 = recordResponse3.Content.ReadAsStringAsync().Result;
            string record4 = recordResponse4.Content.ReadAsStringAsync().Result;

            Assert.IsTrue(record1.Contains("\"DeviceId\":4"));
            Assert.IsTrue(record1.Contains("\"VersionId\":1"));
            Assert.IsTrue(record1.Contains("Action\":\"CREATE"));
            Assert.IsTrue(record2.Contains("\"DeviceId\":5"));
            Assert.IsTrue(record2.Contains("\"VersionId\":1"));
            Assert.IsTrue(record2.Contains("Action\":\"CREATE"));
            Assert.IsTrue(record3.Contains("\"DeviceId\":6"));
            Assert.IsTrue(record3.Contains("\"VersionId\":1"));
            Assert.IsTrue(record3.Contains("Action\":\"CREATE"));
            Assert.IsTrue(record4.Contains("\"DeviceId\":7"));
            Assert.IsTrue(record4.Contains("\"VersionId\":1"));
            Assert.IsTrue(record4.Contains("Action\":\"CREATE"));

            // Case where ID is present
            Device deviceIdPresent = MockedResources.Device1;
            deviceIdPresent.Id = "10";
            string deviceIdPresentString = FhirSerializer.SerializeResourceToXml(deviceIdPresent);

            HttpRequestMessage request5 = new HttpRequestMessage(HttpMethod.Post, baseUrl + "/Device")
            {
                Content = new StringContent(deviceIdPresentString, Encoding.UTF8, "application/xml+fhir")
            };
            var response5 = httpClient.SendAsync(request5).Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, response5.StatusCode);

            // Case where wrong type of resource is provided
            string patient = FhirSerializer.SerializeResourceToJson(MockedResources.Patient1);
            HttpRequestMessage request6 = new HttpRequestMessage(HttpMethod.Post, baseUrl + "/Device")
            {
                Content = new StringContent(patient, Encoding.UTF8, "application/json+fhir")
            };
            var response6 = httpClient.SendAsync(request6).Result;

            Assert.AreEqual(HttpStatusCode.NotFound, response6.StatusCode);
        }

        [TestMethod]
        public void TestFhirUpdate()
        {
            ResourceIdentity identity1 = ResourceIdentity.Build("Device", "2");
            ResourceIdentity identity2 = ResourceIdentity.Build("Device", "3");

            Device device1 = fhirClient.Read<Device>(identity1);
            device1.Manufacturer = "Test Industries";
            Device device2 = fhirClient.Read<Device>(identity2);
            device2.Manufacturer = "Test Corp";

            var device1Entry = fhirClient.Update(device1);
            var device2Entry = fhirClient.Update(device2);

            device1 = fhirClient.Read<Device>(identity1);
            device2 = fhirClient.Read<Device>(identity2);

            Assert.AreEqual("2", device1Entry.Id);
            Assert.AreEqual("3", device2Entry.Id);
            Assert.AreEqual("Test Industries", device1Entry.Manufacturer);
            Assert.AreEqual("Test Corp", device2Entry.Manufacturer);
            Assert.AreEqual("Test Industries", device1.Manufacturer);
            Assert.AreEqual("Test Corp", device2.Manufacturer);
            Assert.AreEqual("2", device1Entry.Meta.VersionId);
            Assert.AreEqual("2", device2Entry.Meta.VersionId);
        }

        [TestMethod]
        public void TestHttpUpdate()
        {
            ResourceIdentity identity1 = ResourceIdentity.Build("Device", "2");
            ResourceIdentity identity2 = ResourceIdentity.Build("Device", "3");

            Device device1 = fhirClient.Read<Device>(identity1);
            device1.Manufacturer = "Test Industries";
            Device device2 = fhirClient.Read<Device>(identity2);
            device2.Manufacturer = "Test Corp";


            // Successful update (JSON)
            string device1Json = FhirSerializer.SerializeResourceToJson(device1);
            string device2Json = FhirSerializer.SerializeResourceToJson(device2);

            HttpRequestMessage request1 = new HttpRequestMessage(HttpMethod.Put, baseUrl + "/Device/2")
            {
                Content = new StringContent(device1Json, Encoding.UTF8, "application/json+fhir")
            };
            var response1 = httpClient.SendAsync(request1).Result;
            HttpRequestMessage request2 = new HttpRequestMessage(HttpMethod.Put, baseUrl + "/Device/3")
            {
                Content = new StringContent(device2Json, Encoding.UTF8, "application/json+fhir")
            };
            var response2 = httpClient.SendAsync(request2).Result;

            // Successful update (XML)
            string device1Xml = FhirSerializer.SerializeResourceToXml(device1);
            string device2Xml = FhirSerializer.SerializeResourceToXml(device2);

            HttpRequestMessage request3 = new HttpRequestMessage(HttpMethod.Put, baseUrl + "/Device/2")
            {
                Content = new StringContent(device1Xml, Encoding.UTF8, "application/xml+fhir")
            };
            var response3 = httpClient.SendAsync(request3).Result;
            HttpRequestMessage request4 = new HttpRequestMessage(HttpMethod.Put, baseUrl + "/Device/3")
            {
                Content = new StringContent(device2Xml, Encoding.UTF8, "application/xml+fhir")
            };
            var response4 = httpClient.SendAsync(request4).Result;

            Assert.AreEqual(HttpStatusCode.OK, response1.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, response2.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, response3.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, response4.StatusCode);

            // Check that record entries are created correctly
            var recordResponse1 = httpClient.GetAsync(baseUrl + "/Devicerecord/4").Result;
            var recordResponse2 = httpClient.GetAsync(baseUrl + "/Devicerecord/5").Result;
            var recordResponse3 = httpClient.GetAsync(baseUrl + "/Devicerecord/6").Result;
            var recordResponse4 = httpClient.GetAsync(baseUrl + "/Devicerecord/7").Result;

            string record1 = recordResponse1.Content.ReadAsStringAsync().Result;
            string record2 = recordResponse2.Content.ReadAsStringAsync().Result;
            string record3 = recordResponse3.Content.ReadAsStringAsync().Result;
            string record4 = recordResponse4.Content.ReadAsStringAsync().Result;

            Assert.IsTrue(record1.Contains("\"DeviceId\":2"));
            Assert.IsTrue(record1.Contains("\"VersionId\":2"));
            Assert.IsTrue(record1.Contains("Action\":\"UPDATE"));
            Assert.IsTrue(record2.Contains("\"DeviceId\":3"));
            Assert.IsTrue(record2.Contains("\"VersionId\":2"));
            Assert.IsTrue(record2.Contains("Action\":\"UPDATE"));
            Assert.IsTrue(record3.Contains("\"DeviceId\":2"));
            Assert.IsTrue(record3.Contains("\"VersionId\":3"));
            Assert.IsTrue(record3.Contains("Action\":\"UPDATE"));
            Assert.IsTrue(record4.Contains("\"DeviceId\":3"));
            Assert.IsTrue(record4.Contains("\"VersionId\":3"));
            Assert.IsTrue(record4.Contains("Action\":\"UPDATE"));

            // Successful create(JSON)
            device1.Id = "4";
            device2.Id = "5";
            string device1CreateJson = FhirSerializer.SerializeResourceToJson(device1);
            string device2CreateJson = FhirSerializer.SerializeResourceToJson(device2);

            HttpRequestMessage request5 = new HttpRequestMessage(HttpMethod.Put, baseUrl + "/Device/4")
            {
                Content = new StringContent(device1CreateJson, Encoding.UTF8, "application/json+fhir")
            };
            var response5 = httpClient.SendAsync(request5).Result;
            HttpRequestMessage request6 = new HttpRequestMessage(HttpMethod.Put, baseUrl + "/Device/5")
            {
                Content = new StringContent(device2CreateJson, Encoding.UTF8, "application/json+fhir")
            };
            var response6 = httpClient.SendAsync(request6).Result;

            // Successful create(XML)
            device1.Id = "6";
            device2.Id = "7";
            string device1CreateXml = FhirSerializer.SerializeResourceToXml(device1);
            string device2CreateXml = FhirSerializer.SerializeResourceToXml(device2);

            HttpRequestMessage request7 = new HttpRequestMessage(HttpMethod.Put, baseUrl + "/Device/6")
            {
                Content = new StringContent(device1CreateXml, Encoding.UTF8, "application/xml+fhir")
            };
            var response7 = httpClient.SendAsync(request7).Result;
            HttpRequestMessage request8 = new HttpRequestMessage(HttpMethod.Put, baseUrl + "/Device/7")
            {
                Content = new StringContent(device2CreateXml, Encoding.UTF8, "application/xml+fhir")
            };
            var response8 = httpClient.SendAsync(request8).Result;

            Assert.AreEqual(HttpStatusCode.Created, response5.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, response6.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, response7.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, response8.StatusCode);

            // Check that record entries are created correctly
            var recordResponse5 = httpClient.GetAsync(baseUrl + "/Devicerecord/8").Result;
            var recordResponse6 = httpClient.GetAsync(baseUrl + "/Devicerecord/9").Result;
            var recordResponse7 = httpClient.GetAsync(baseUrl + "/Devicerecord/10").Result;
            var recordResponse8 = httpClient.GetAsync(baseUrl + "/Devicerecord/11").Result;

            string record5 = recordResponse5.Content.ReadAsStringAsync().Result;
            string record6 = recordResponse6.Content.ReadAsStringAsync().Result;
            string record7 = recordResponse7.Content.ReadAsStringAsync().Result;
            string record8 = recordResponse8.Content.ReadAsStringAsync().Result;

            Assert.IsTrue(record5.Contains("\"DeviceId\":4"));
            Assert.IsTrue(record5.Contains("\"VersionId\":1"));
            Assert.IsTrue(record5.Contains("Action\":\"CREATE"));
            Assert.IsTrue(record6.Contains("\"DeviceId\":5"));
            Assert.IsTrue(record6.Contains("\"VersionId\":1"));
            Assert.IsTrue(record6.Contains("Action\":\"CREATE"));
            Assert.IsTrue(record7.Contains("\"DeviceId\":6"));
            Assert.IsTrue(record7.Contains("\"VersionId\":1"));
            Assert.IsTrue(record7.Contains("Action\":\"CREATE"));
            Assert.IsTrue(record8.Contains("\"DeviceId\":7"));
            Assert.IsTrue(record8.Contains("\"VersionId\":1"));
            Assert.IsTrue(record8.Contains("Action\":\"CREATE"));

            // Case where wrong type of resource is provided
            string patient = FhirSerializer.SerializeResourceToJson(MockedResources.Patient1);
            HttpRequestMessage request9 = new HttpRequestMessage(HttpMethod.Put, baseUrl + "/Device/8")
            {
                Content = new StringContent(patient, Encoding.UTF8, "application/json+fhir")
            };
            var response9 = httpClient.SendAsync(request9).Result;

            Assert.AreEqual(HttpStatusCode.NotFound, response9.StatusCode);

            // Case where resource does not have logical ID
            device1.Id = null;
            string device1NoId = FhirSerializer.SerializeResourceToXml(device1);
            HttpRequestMessage request10 = new HttpRequestMessage(HttpMethod.Put, baseUrl + "/Device/8")
            {
                Content = new StringContent(device1NoId, Encoding.UTF8, "application/xml+fhir")
            };
            var response10 = httpClient.SendAsync(request10).Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, response10.StatusCode);

            // Case where resource ID ! input resource logical ID
            device1.Id = "2";
            string device1NotEqualId = FhirSerializer.SerializeResourceToXml(device1);
            HttpRequestMessage request11 = new HttpRequestMessage(HttpMethod.Put, baseUrl + "/Device/11")
            {
                Content = new StringContent(device1NotEqualId, Encoding.UTF8, "application/xml+fhir")
            };
            var response11 = httpClient.SendAsync(request11).Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, response11.StatusCode);
        }

        [TestMethod]
        public void TestFhirDelete()
        {
            ResourceIdentity identity1 = ResourceIdentity.Build("Device", "1");
            ResourceIdentity identity2 = ResourceIdentity.Build("Device", "2");

            fhirClient.Delete(identity1);
            fhirClient.Delete(identity2);

            try
            {
                fhirClient.Read<Device>(baseUrl + "/Device/1");
                Assert.Fail();
            }
            catch (FormatException ex)
            {
                Assert.IsTrue(ex.Message.Contains("Device with id 1 has been deleted!"));
            }

            try
            {
                fhirClient.Read<Device>(baseUrl + "/Device/2");
                Assert.Fail();
            }
            catch (FormatException ex)
            {
                Assert.IsTrue(ex.Message.Contains("Device with id 2 has been deleted!"));
            }
        }

        [TestMethod]
        public void TestHttpDelete()
        {
            // Case where delete successfully
            var response1 = httpClient.DeleteAsync(baseUrl + "/Device/1").Result;
            var response2 = httpClient.DeleteAsync(baseUrl + "/Device/2").Result;

            Assert.AreEqual(HttpStatusCode.NoContent, response1.StatusCode);
            Assert.AreEqual(HttpStatusCode.NoContent, response2.StatusCode);

            // Check that record entries are created correctly
            var recordResponse5 = httpClient.GetAsync(baseUrl + "/Devicerecord/4").Result;
            var recordResponse6 = httpClient.GetAsync(baseUrl + "/Devicerecord/5").Result;

            string record5 = recordResponse5.Content.ReadAsStringAsync().Result;
            string record6 = recordResponse6.Content.ReadAsStringAsync().Result;

            Assert.IsTrue(record5.Contains("\"DeviceId\":1"));
            Assert.IsTrue(record5.Contains("\"VersionId\":1"));
            Assert.IsTrue(record5.Contains("Action\":\"DELETE"));
            Assert.IsTrue(record6.Contains("\"DeviceId\":2"));
            Assert.IsTrue(record6.Contains("\"VersionId\":1"));
            Assert.IsTrue(record6.Contains("Action\":\"DELETE"));

            // Case where attempt delete on non-existent resource
            var response3 = httpClient.DeleteAsync(baseUrl + "/Device/4").Result;

            Assert.AreEqual(HttpStatusCode.NoContent, response3.StatusCode);

            // Case where attempt delete on already deleted resource
            var response4 = httpClient.DeleteAsync(baseUrl + "/Device/1").Result;
            var response5 = httpClient.DeleteAsync(baseUrl + "/Device/2").Result;

            Assert.AreEqual(HttpStatusCode.OK, response4.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, response5.StatusCode);
        }
    }
}
