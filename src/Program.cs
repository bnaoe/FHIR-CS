﻿using System;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using  System.Collections.Generic;
namespace fhir_cs
{
    
    // See https://aka.ms/new-console-template for more information
    public static class Program 
    {
        private const string _fhirServer = "http://vonk.fire.ly"; //"http://hapi.fhir.org/baseR4"; 

        static void Main(string[] args)
        {
            FhirClient fhirClient = new FhirClient(_fhirServer)
            {
                PreferredFormat = ResourceFormat.Json,
                PreferredReturn = Prefer.ReturnRepresentation
            };

            Bundle patientBundle = fhirClient.Search<Patient>(new string[] {"name=test"});

            int patientNumber = 0;
            List<string> patientsWithEncounters = new List<string>();
            while (patientBundle !=null)
            {
                System.Console.WriteLine($"Patient Total: {patientBundle.Total} Entry count: {patientBundle.Entry.Count}");
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

                            if (encounterBundle.Total == 0)
                            {
                                continue;
                            }

                            patientsWithEncounters.Add(patient.Id);  

                        System.Console.WriteLine($"- Entry: {patientNumber,3}: {entry.FullUrl}");
                        System.Console.WriteLine($" - ID: {patient.Id}");

                        if (patient.Name.Count > 0)
                        {
                            System.Console.WriteLine($" - Name: {patient.Name[0].ToString()}");
                        }

                            System.Console.WriteLine($" Encounter Total: {encounterBundle.Total} Entry count: {encounterBundle.Entry.Count}");

                    }
                    patientNumber++;
                    if (patientsWithEncounters.Count >= 2)
                    {
                        break;
                    }                      
                }
                    if (patientsWithEncounters.Count >= 2)
                    {
                        break;
                    }  
                // get more results
                patientBundle = fhirClient.Continue(patientBundle);
            }
          
        }

    }

}