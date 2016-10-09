using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hl7.Fhir.Model;

namespace ServerIntegTests
{
    public class MockedResources
    {
        public static Patient patient1 = new Patient()
        {
            Name = new List<HumanName>()
            {
                new HumanName()
                {
                    Given = new List<string>()
                    {
                        "Roger"
                    },
                    Family = new List<string>()
                    {
                        "Newman"
                    },
                    Use = HumanName.NameUse.Official
                }
            },
            Telecom = new List<ContactPoint>()
            {
                new ContactPoint()
                {
                    System = ContactPoint.ContactPointSystem.Phone,
                    Value = "022381827",
                    Use = ContactPoint.ContactPointUse.Home,
                },
                new ContactPoint()
                {
                   System = ContactPoint.ContactPointSystem.Email,
                   Value = "myEmail@emailaddress.com",
                   Use = ContactPoint.ContactPointUse.Home
                }
            },
            Address = new List<Address>()
            {
                new Address()
                {
                    City = "Sydney",
                    Country = "Australia",
                    Use = Address.AddressUse.Home,
                    Line = new List<string>() {"123 Happy Street", "Test Building" },
                    State = "NSW",
                    PostalCode = "2034"
                }
            },
            Gender = AdministrativeGender.Male,
        };

        public static  Device device1 = new Device()
        {
            Type = new CodeableConcept()
            {
                Coding = new List<Coding>()
                {
                    new Coding()
                    {
                        System = "http://snomed.info/sct",
                        Code = "86184003",
                        Display = "Electrocardiographic monitor and recorder"
                    }
                },
                Text = "ECG"
            },
            Status = Device.DeviceStatus.Available,
            Manufacturer = "Acme Devices, Inc",
            Model = "AB 45-J",
            LotNumber = "43453424"
        };

        public static Device device2 = new Device()
        {
            Type = new CodeableConcept()
            {
                Coding = new List<Coding>()
                {
                    new Coding()
                    {
                        System = "http://snomed.info/sct",
                        Code = "27113001",
                        Display = "Mass Measurement Apparatus"
                    }
                },
                Text = "Weighing Machine"
            },
            Status = Device.DeviceStatus.Available,
            Manufacturer = "Acme Devices, Inc",
            Model = "OD 9-0",
            LotNumber = "12345678"
        };
    }
}
