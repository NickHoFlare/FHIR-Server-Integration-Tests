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
    public class PatientTests
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

            // Case where Patient does not exist
            var response4 = httpClient.GetAsync(baseUrl + "/Patient/4").Result;
            var response5 = httpClient.GetAsync(baseUrl + "/Patient/5").Result;

            Assert.AreEqual(HttpStatusCode.NotFound, response4.StatusCode);
            Assert.AreEqual(HttpStatusCode.NotFound, response5.StatusCode);
        }

        [TestMethod]
        public void TestFhirCreate()
        {
            Patient patient1 = MockedResources.Patient1;
            Patient patient2 = MockedResources.Patient2;

            var patient1Entry = fhirClient.Create(patient1);
            var patient2Entry = fhirClient.Create(patient2);

            Assert.AreEqual("4", patient1Entry.Id);
            Assert.AreEqual("5", patient2Entry.Id);
            Assert.AreEqual(MockedResources.Patient1.BirthDate, patient1Entry.BirthDate);
            Assert.AreEqual(MockedResources.Patient2.BirthDate, patient2Entry.BirthDate);
            Assert.AreEqual(MockedResources.Patient1.Address[0].City, patient1Entry.Address[0].City);
            Assert.AreEqual(MockedResources.Patient2.Address[0].City, patient2Entry.Address[0].City);
        }

        [TestMethod]
        public void TestHttpCreate()
        {
            // Successful creation
            string patient1Json = FhirSerializer.SerializeResourceToJson(MockedResources.Patient1);
            string patient2Json = FhirSerializer.SerializeResourceToJson(MockedResources.Patient2);
            string patient1Xml = FhirSerializer.SerializeResourceToXml(MockedResources.Patient1);
            string patient2Xml = FhirSerializer.SerializeResourceToXml(MockedResources.Patient2);

            HttpRequestMessage request1 = new HttpRequestMessage(HttpMethod.Post, baseUrl + "/Patient")
            {
                Content = new StringContent(patient1Json, Encoding.UTF8, "application/json+fhir")
            };
            var response1 = httpClient.SendAsync(request1).Result;
            HttpRequestMessage request2 = new HttpRequestMessage(HttpMethod.Post, baseUrl + "/Patient")
            {
                Content = new StringContent(patient2Json, Encoding.UTF8, "application/json+fhir")
            };
            var response2 = httpClient.SendAsync(request2).Result;
            HttpRequestMessage request3 = new HttpRequestMessage(HttpMethod.Post, baseUrl + "/Patient")
            {
                Content = new StringContent(patient1Xml, Encoding.UTF8, "application/xml+fhir")
            };
            var response3 = httpClient.SendAsync(request3).Result;
            HttpRequestMessage request4 = new HttpRequestMessage(HttpMethod.Post, baseUrl + "/Patient")
            {
                Content = new StringContent(patient2Xml, Encoding.UTF8, "application/xml+fhir")
            };
            var response4 = httpClient.SendAsync(request4).Result;

            Assert.AreEqual(HttpStatusCode.Created, response1.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, response2.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, response3.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, response4.StatusCode);

            // Check that record entries are created correctly
            var recordResponse1 = httpClient.GetAsync(baseUrl + "/Patientrecord/4").Result;
            var recordResponse2 = httpClient.GetAsync(baseUrl + "/Patientrecord/5").Result;
            var recordResponse3 = httpClient.GetAsync(baseUrl + "/Patientrecord/6").Result;
            var recordResponse4 = httpClient.GetAsync(baseUrl + "/Patientrecord/7").Result;

            string record1 = recordResponse1.Content.ReadAsStringAsync().Result;
            string record2 = recordResponse2.Content.ReadAsStringAsync().Result;
            string record3 = recordResponse3.Content.ReadAsStringAsync().Result;
            string record4 = recordResponse4.Content.ReadAsStringAsync().Result;

            Assert.IsTrue(record1.Contains("\"PatientId\":4"));
            Assert.IsTrue(record1.Contains("\"VersionId\":1"));
            Assert.IsTrue(record1.Contains("Action\":\"CREATE"));
            Assert.IsTrue(record2.Contains("\"PatientId\":5"));
            Assert.IsTrue(record2.Contains("\"VersionId\":1"));
            Assert.IsTrue(record2.Contains("Action\":\"CREATE"));
            Assert.IsTrue(record3.Contains("\"PatientId\":6"));
            Assert.IsTrue(record3.Contains("\"VersionId\":1"));
            Assert.IsTrue(record3.Contains("Action\":\"CREATE"));
            Assert.IsTrue(record4.Contains("\"PatientId\":7"));
            Assert.IsTrue(record4.Contains("\"VersionId\":1"));
            Assert.IsTrue(record4.Contains("Action\":\"CREATE"));

            // Case where ID is present
            Patient patientIdPresent = MockedResources.Patient1;
            patientIdPresent.Id = "10";
            string patientIdPresentString = FhirSerializer.SerializeResourceToXml(patientIdPresent);

            HttpRequestMessage request5 = new HttpRequestMessage(HttpMethod.Post, baseUrl + "/Patient")
            {
                Content = new StringContent(patientIdPresentString, Encoding.UTF8, "application/xml+fhir")
            };
            var response5 = httpClient.SendAsync(request5).Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, response5.StatusCode);

            // Case where wrong type of resource is provided
            string device = FhirSerializer.SerializeResourceToJson(MockedResources.Device1);
            HttpRequestMessage request6 = new HttpRequestMessage(HttpMethod.Post, baseUrl + "/Patient")
            {
                Content = new StringContent(device, Encoding.UTF8, "application/json+fhir")
            };
            var response6 = httpClient.SendAsync(request6).Result;

            Assert.AreEqual(HttpStatusCode.NotFound, response6.StatusCode);
        }

        [TestMethod]
        public void TestFhirUpdate()
        {
            ResourceIdentity identity1 = ResourceIdentity.Build("Patient", "2");
            ResourceIdentity identity2 = ResourceIdentity.Build("Patient", "3");

            Patient patient1 = fhirClient.Read<Patient>(identity1);
            patient1.Gender = AdministrativeGender.Other;
            Patient patient2 = fhirClient.Read<Patient>(identity2);
            patient2.Address = new List<Address>();
            patient2.Active = false;

            var patient1Entry = fhirClient.Update(patient1);
            var patient2Entry = fhirClient.Update(patient2);

            patient1 = fhirClient.Read<Patient>(identity1);
            patient2 = fhirClient.Read<Patient>(identity2);

            Assert.AreEqual("2", patient1Entry.Id);
            Assert.AreEqual("3", patient2Entry.Id);
            Assert.AreEqual(AdministrativeGender.Other, patient1Entry.Gender);
            Assert.AreEqual(false, patient2.Active);
            Assert.AreEqual(AdministrativeGender.Other, patient1.Gender);
            Assert.AreEqual(false, patient2Entry.Active);
            Assert.AreEqual("2", patient1Entry.Meta.VersionId);
            Assert.AreEqual("2", patient2Entry.Meta.VersionId);
        }

        [TestMethod]
        public void TestHttpUpdate()
        {
            ResourceIdentity identity1 = ResourceIdentity.Build("Patient", "2");
            ResourceIdentity identity2 = ResourceIdentity.Build("Patient", "3");

            Patient patient1 = fhirClient.Read<Patient>(identity1);
            patient1.Gender = AdministrativeGender.Other;
            Patient patient2 = fhirClient.Read<Patient>(identity2);
            patient2.BirthDate = "22/03/1991";


            // Successful update (JSON)
            string patient1Json = FhirSerializer.SerializeResourceToJson(patient1);
            string patient2Json = FhirSerializer.SerializeResourceToJson(patient2);

            HttpRequestMessage request1 = new HttpRequestMessage(HttpMethod.Put, baseUrl + "/Patient/2")
            {
                Content = new StringContent(patient1Json, Encoding.UTF8, "application/json+fhir")
            };
            var response1 = httpClient.SendAsync(request1).Result;
            HttpRequestMessage request2 = new HttpRequestMessage(HttpMethod.Put, baseUrl + "/Patient/3")
            {
                Content = new StringContent(patient2Json, Encoding.UTF8, "application/json+fhir")
            };
            var response2 = httpClient.SendAsync(request2).Result;

            // Successful update (XML)
            string patient1Xml = FhirSerializer.SerializeResourceToXml(patient1);
            string patient2Xml = FhirSerializer.SerializeResourceToXml(patient2);

            HttpRequestMessage request3 = new HttpRequestMessage(HttpMethod.Put, baseUrl + "/Patient/2")
            {
                Content = new StringContent(patient1Xml, Encoding.UTF8, "application/xml+fhir")
            };
            var response3 = httpClient.SendAsync(request3).Result;
            HttpRequestMessage request4 = new HttpRequestMessage(HttpMethod.Put, baseUrl + "/Patient/3")
            {
                Content = new StringContent(patient2Xml, Encoding.UTF8, "application/xml+fhir")
            };
            var response4 = httpClient.SendAsync(request4).Result;

            Assert.AreEqual(HttpStatusCode.OK, response1.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, response2.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, response3.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, response4.StatusCode);

            // Check that record entries are created correctly
            var recordResponse1 = httpClient.GetAsync(baseUrl + "/Patientrecord/4").Result;
            var recordResponse2 = httpClient.GetAsync(baseUrl + "/Patientrecord/5").Result;
            var recordResponse3 = httpClient.GetAsync(baseUrl + "/Patientrecord/6").Result;
            var recordResponse4 = httpClient.GetAsync(baseUrl + "/Patientrecord/7").Result;

            string record1 = recordResponse1.Content.ReadAsStringAsync().Result;
            string record2 = recordResponse2.Content.ReadAsStringAsync().Result;
            string record3 = recordResponse3.Content.ReadAsStringAsync().Result;
            string record4 = recordResponse4.Content.ReadAsStringAsync().Result;

            Assert.IsTrue(record1.Contains("\"PatientId\":2"));
            Assert.IsTrue(record1.Contains("\"VersionId\":2"));
            Assert.IsTrue(record1.Contains("Action\":\"UPDATE"));
            Assert.IsTrue(record2.Contains("\"PatientId\":3"));
            Assert.IsTrue(record2.Contains("\"VersionId\":2"));
            Assert.IsTrue(record2.Contains("Action\":\"UPDATE"));
            Assert.IsTrue(record3.Contains("\"PatientId\":2"));
            Assert.IsTrue(record3.Contains("\"VersionId\":3"));
            Assert.IsTrue(record3.Contains("Action\":\"UPDATE"));
            Assert.IsTrue(record4.Contains("\"PatientId\":3"));
            Assert.IsTrue(record4.Contains("\"VersionId\":3"));
            Assert.IsTrue(record4.Contains("Action\":\"UPDATE"));

            // Successful create(JSON)
            patient1.Id = "4";
            patient2.Id = "5";
            string patient1CreateJson = FhirSerializer.SerializeResourceToJson(patient1);
            string patient2CreateJson = FhirSerializer.SerializeResourceToJson(patient2);

            HttpRequestMessage request5 = new HttpRequestMessage(HttpMethod.Put, baseUrl + "/Patient/4")
            {
                Content = new StringContent(patient1CreateJson, Encoding.UTF8, "application/json+fhir")
            };
            var response5 = httpClient.SendAsync(request5).Result;
            HttpRequestMessage request6 = new HttpRequestMessage(HttpMethod.Put, baseUrl + "/Patient/5")
            {
                Content = new StringContent(patient2CreateJson, Encoding.UTF8, "application/json+fhir")
            };
            var response6 = httpClient.SendAsync(request6).Result;

            // Successful create(XML)
            patient1.Id = "6";
            patient2.Id = "7";
            string patient1CreateXml = FhirSerializer.SerializeResourceToXml(patient1);
            string patient2CreateXml = FhirSerializer.SerializeResourceToXml(patient2);

            HttpRequestMessage request7 = new HttpRequestMessage(HttpMethod.Put, baseUrl + "/Patient/6")
            {
                Content = new StringContent(patient1CreateXml, Encoding.UTF8, "application/xml+fhir")
            };
            var response7 = httpClient.SendAsync(request7).Result;
            HttpRequestMessage request8 = new HttpRequestMessage(HttpMethod.Put, baseUrl + "/Patient/7")
            {
                Content = new StringContent(patient2CreateXml, Encoding.UTF8, "application/xml+fhir")
            };
            var response8 = httpClient.SendAsync(request8).Result;

            Assert.AreEqual(HttpStatusCode.Created, response5.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, response6.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, response7.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, response8.StatusCode);

            // Check that record entries are created correctly
            var recordResponse5 = httpClient.GetAsync(baseUrl + "/Patientrecord/8").Result;
            var recordResponse6 = httpClient.GetAsync(baseUrl + "/Patientrecord/9").Result;
            var recordResponse7 = httpClient.GetAsync(baseUrl + "/Patientrecord/10").Result;
            var recordResponse8 = httpClient.GetAsync(baseUrl + "/Patientrecord/11").Result;

            string record5 = recordResponse5.Content.ReadAsStringAsync().Result;
            string record6 = recordResponse6.Content.ReadAsStringAsync().Result;
            string record7 = recordResponse7.Content.ReadAsStringAsync().Result;
            string record8 = recordResponse8.Content.ReadAsStringAsync().Result;

            Assert.IsTrue(record5.Contains("\"PatientId\":4"));
            Assert.IsTrue(record5.Contains("\"VersionId\":1"));
            Assert.IsTrue(record5.Contains("Action\":\"CREATE"));
            Assert.IsTrue(record6.Contains("\"PatientId\":5"));
            Assert.IsTrue(record6.Contains("\"VersionId\":1"));
            Assert.IsTrue(record6.Contains("Action\":\"CREATE"));
            Assert.IsTrue(record7.Contains("\"PatientId\":6"));
            Assert.IsTrue(record7.Contains("\"VersionId\":1"));
            Assert.IsTrue(record7.Contains("Action\":\"CREATE"));
            Assert.IsTrue(record8.Contains("\"PatientId\":7"));
            Assert.IsTrue(record8.Contains("\"VersionId\":1"));
            Assert.IsTrue(record8.Contains("Action\":\"CREATE"));

            // Case where wrong type of resource is provided
            string device = FhirSerializer.SerializeResourceToJson(MockedResources.Device1);
            HttpRequestMessage request9 = new HttpRequestMessage(HttpMethod.Put, baseUrl + "/Patient/8")
            {
                Content = new StringContent(device, Encoding.UTF8, "application/json+fhir")
            };
            var response9 = httpClient.SendAsync(request9).Result;

            Assert.AreEqual(HttpStatusCode.NotFound, response9.StatusCode);

            // Case where resource does not have logical ID
            patient1.Id = null;
            string patient1NoId = FhirSerializer.SerializeResourceToXml(patient1);
            HttpRequestMessage request10 = new HttpRequestMessage(HttpMethod.Put, baseUrl + "/Patient/8")
            {
                Content = new StringContent(patient1NoId, Encoding.UTF8, "application/xml+fhir")
            };
            var response10 = httpClient.SendAsync(request10).Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, response10.StatusCode);

            // Case where resource ID ! input resource logical ID
            patient1.Id = "2";
            string patient1NotEqualId = FhirSerializer.SerializeResourceToXml(patient1);
            HttpRequestMessage request11 = new HttpRequestMessage(HttpMethod.Put, baseUrl + "/Patient/11")
            {
                Content = new StringContent(patient1NotEqualId, Encoding.UTF8, "application/xml+fhir")
            };
            var response11 = httpClient.SendAsync(request11).Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, response11.StatusCode);
        }

        [TestMethod]
        public void TestFhirDelete()
        {
            ResourceIdentity identity1 = ResourceIdentity.Build("Patient", "1");
            ResourceIdentity identity2 = ResourceIdentity.Build("Patient", "2");

            fhirClient.Delete(identity1);
            fhirClient.Delete(identity2);

            try
            {
                fhirClient.Read<Patient>(baseUrl + "/Patient/1");
                Assert.Fail();
            }
            catch (FormatException ex)
            {
                Assert.IsTrue(ex.Message.Contains("Patient with id 1 has been deleted!"));
            }

            try
            {
                fhirClient.Read<Patient>(baseUrl + "/Patient/2");
                Assert.Fail();
            }
            catch (FormatException ex)
            {
                Assert.IsTrue(ex.Message.Contains("Patient with id 2 has been deleted!"));
            }
        }

        [TestMethod]
        public void TestHttpDelete()
        {
            // Case where delete successfully
            var response1 = httpClient.DeleteAsync(baseUrl + "/Patient/1").Result;
            var response2 = httpClient.DeleteAsync(baseUrl + "/Patient/2").Result;

            Assert.AreEqual(HttpStatusCode.NoContent, response1.StatusCode);
            Assert.AreEqual(HttpStatusCode.NoContent, response2.StatusCode);

            // Check that record entries are created correctly
            var recordResponse5 = httpClient.GetAsync(baseUrl + "/Patientrecord/4").Result;
            var recordResponse6 = httpClient.GetAsync(baseUrl + "/Patientrecord/5").Result;

            string record5 = recordResponse5.Content.ReadAsStringAsync().Result;
            string record6 = recordResponse6.Content.ReadAsStringAsync().Result;

            Assert.IsTrue(record5.Contains("\"PatientId\":1"));
            Assert.IsTrue(record5.Contains("\"VersionId\":1"));
            Assert.IsTrue(record5.Contains("Action\":\"DELETE"));
            Assert.IsTrue(record6.Contains("\"PatientId\":2"));
            Assert.IsTrue(record6.Contains("\"VersionId\":1"));
            Assert.IsTrue(record6.Contains("Action\":\"DELETE"));

            // Case where attempt delete on non-existent resource
            var response3 = httpClient.DeleteAsync(baseUrl + "/Patient/4").Result;

            Assert.AreEqual(HttpStatusCode.NoContent, response3.StatusCode);

            // Case where attempt delete on already deleted resource
            var response4 = httpClient.DeleteAsync(baseUrl + "/Patient/1").Result;
            var response5 = httpClient.DeleteAsync(baseUrl + "/Patient/2").Result;

            Assert.AreEqual(HttpStatusCode.OK, response4.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, response5.StatusCode);
        }

    }
}
