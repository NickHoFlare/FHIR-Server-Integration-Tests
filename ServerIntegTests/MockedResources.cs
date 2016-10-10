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
        public static Patient Patient1 = new Patient()
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

        public static Patient Patient2 = new Patient()
        {
            Name = new List<HumanName>()
            {
                new HumanName()
                {
                    Given = new List<string>()
                    {
                        "Gabe"
                    },
                    Family = new List<string>()
                    {
                        "Newell"
                    },
                    Use = HumanName.NameUse.Official
                }
            },
            Telecom = new List<ContactPoint>()
            {
                new ContactPoint()
                {
                    System = ContactPoint.ContactPointSystem.Phone,
                    Value = "02929385",
                    Use = ContactPoint.ContactPointUse.Home,
                },
                new ContactPoint()
                {
                   System = ContactPoint.ContactPointSystem.Email,
                   Value = "gaben@valvesoftware.com",
                   Use = ContactPoint.ContactPointUse.Work
                }
            },
            Address = new List<Address>()
            {
                new Address()
                {
                    City = "Redmond",
                    Country = "America",
                    Use = Address.AddressUse.Home,
                    Line = new List<string>() {"123 Happy Street", "Test Building" },
                    State = "Washinton",
                    PostalCode = "2034"
                }
            },
            Gender = AdministrativeGender.Male,
        };

        public static Device Device1 = new Device()
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

        public static Device Device2 = new Device()
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

        public static Observation Observation1 = new Observation()
        {
            Status = Observation.ObservationStatus.Registered,
            Category = new CodeableConcept()
            {
                Coding = new List<Coding>()
                {
                    new Coding()
                    {
                        Code = "19283712",
                        System = "http://snomed.info/sct",
                        Display = "Spirometry"
                    }
                },
                Text = "Spirometry"
            },
            Code = new CodeableConcept()
            {
                Coding = new List<Coding>()
                {
                    new Coding()
                    {
                        Code = "28123-293",
                        System = "http://snomed.info/sct",
                        Display = "Lung capacity"
                    }
                }
            },
            Performer = new List<ResourceReference>()
            {
                new ResourceReference()
                {
                    Reference = "Patient/3"
                }
            },
            Subject = new ResourceReference()
            {
                Reference = "Patient/3"
            },
            Device = new ResourceReference()
            {
                Reference = "Device/3"
            },
        };

        public static Observation Observation2 = new Observation()
        {
            Status = Observation.ObservationStatus.Registered,
            Category = new CodeableConcept()
            {
                Coding = new List<Coding>()
                {
                    new Coding()
                    {
                        Code = "91827371",
                        System = "http://snomed.info/sct",
                        Display = "Body Mass"
                    }
                },
                Text = "Body Mass"
            },
            Code = new CodeableConcept()
            {
                Coding = new List<Coding>()
                {
                    new Coding()
                    {
                        Code = "292323-243",
                        System = "http://snomed.info/sct",
                        Display = "Body Mass"
                    }
                }
            },
            Performer = new List<ResourceReference>()
            {
                new ResourceReference()
                {
                    Reference = "Patient/2"
                }
            },
            Subject = new ResourceReference()
            {
                Reference = "Patient/2"
            },
            Device = new ResourceReference()
            {
                Reference = "Device/2"
            },
        };
    }
}
