using System;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using  System.Collections.Generic;
namespace fhir_cs
{
    // See https://aka.ms/new-console-template for more information
    public static class Program 
    {
        //private const string _fhirServer = "http://vonk.fire.ly";  
        //private const string _fhirServer = "http://hapi.fhir.org/baseR4"; 

        private static readonly Dictionary<string,string> _fhirServers = new Dictionary<string, string>()
        {
            {"PublicVonk", "http://vonk.fire.ly"},
            {"PublicHAPI", "http://hapi.fhir.org/baseR4/"},
            {"Local", "http://localhost:8080/fhir"},
        };

        private static readonly string _fhirServer = _fhirServers["PublicHAPI"];
        /// <summary>
        /// Main entry point for the program
        /// </summary>
        /// <param name="args"></param>
        static int Main(string[] args)
        {
            FhirClient fhirClient = new FhirClient(_fhirServer)
            {
                PreferredFormat = ResourceFormat.Json,
                PreferredReturn = Prefer.ReturnRepresentation
            };

            //CreatePatient(fhirClient,"zzTest", "FirstFhir");
            
            DeletePatient(fhirClient,"2807846");

            List<Patient> patients = GetPatients(
                fhirClient,
                new string[] {"name=zzTest"});
            System.Console.WriteLine($"Found {patients.Count} patients.");

            string myPatientId = null;

            return 0;
        }

        

        /// <summary>
        /// Delete patient using specified id.
        /// </summary>
        /// <param name="fhirClient"></param>
        /// <param name="id"></param>
        static void DeletePatient(
            FhirClient fhirClient,
            string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }
            System.Console.WriteLine($"Deleting patient {id}.");
            fhirClient.Delete($"Patient/{id}");
        }

        /// <summary>
        /// Create Patient with specified name
        /// </summary>
        /// <param name="fhirClient"></param>
        /// <param name="familyName"></param>
        /// <param name="givenName"></param>
        static void CreatePatient(
            FhirClient fhirClient, 
            string familyName,
            string givenName)
        {
            Patient toCreate = new Patient()
            {
                Name = new List<HumanName>()
                {
                    new HumanName()
                    {
                       Family = familyName,
                       Given = new List<string>()
                       {
                           givenName,
                       },  
                    }
                },
                BirthDateElement = new Date(1970, 01, 01),
            };

             Patient created = fhirClient.Create<Patient>(toCreate);
             System.Console.WriteLine($"Created Patient/{created.Id}");
        }
            
        /// <summary>
        /// Get a list of patients matching the specified criteria
        /// </summary>
        /// <param name="fhirClient"></param>
        /// <param name="patientCriteria"></param>
        /// <param name="maxPatients">The max no. of patients returning default:20</param>
        /// <param name="onlyWithEncounters">Flag to only return patients with encounters default:false</param>
        /// <returns></returns>
        


        static List<Patient> GetPatients(
            FhirClient fhirClient, 
            string[] patientCriteria = null,
            int maxPatients = 20,
            bool onlyWithEncounters = false
            )
        {
            List<Patient> patients = new List<Patient>();
            Bundle patientBundle;
            if (patientCriteria == null || patientCriteria.Length == 0)
            {
                patientBundle = fhirClient.Search<Patient>();
            }
             else 
             {
                patientBundle = fhirClient.Search<Patient>(patientCriteria);    
             }

            int patientNumber = 0;
            List<string> patientsWithEncounters = new List<string>();
            while (patientBundle !=null)
            {
                System.Console.WriteLine($"Patient Bundle Total: {patientBundle.Total} Entry count: {patientBundle.Entry.Count}");
                
                // list each patient in the bundle
                foreach (Bundle.EntryComponent entry in patientBundle.Entry)
                {
                    if (entry.Resource != null)
                    {
                        Patient patient = (Patient)entry.Resource;

                          Bundle encounterBundle = fhirClient.Search<Encounter>(
                            new string[]
                            {
                                $"patient=Patient/{patient.Id}",

                            });

                            if (onlyWithEncounters && encounterBundle.Total == 0)
                            {
                                continue;
                            }

                            patients.Add(patient);  

                        System.Console.WriteLine($"- Entry: {patients.Count,3}: {entry.FullUrl}");
                        System.Console.WriteLine($" - ID: {patient.Id}");

                        if (patient.Name.Count > 0)
                        {
                            System.Console.WriteLine($" - Name: {patient.Name[0].ToString()}");
                        }

                        if (encounterBundle.Total > 0) 
                        {
                            System.Console.WriteLine($" Encounter Total: {encounterBundle.Total} Entry count: {encounterBundle.Entry.Count}");
                        }

                    }
                    if (patients.Count >= maxPatients)
                    {
                        break;
                    }                      
                }
                    if (patients.Count >= maxPatients)
                    {
                        break;
                    }  
                // get more results
                patientBundle = fhirClient.Continue(patientBundle);
            }
          return patients;
        }

    }

}