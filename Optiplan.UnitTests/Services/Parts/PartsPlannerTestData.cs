using System;
using System.Collections.Generic;
using System.Linq;
using Optiplan.WebApi.Services.Parts;
using Optiplan.UnitTests.Services.Parts;
using Xunit;

namespace Optiplan.UnitTests.Services.Parts
{
    public static class PartsPlannerTestData
    {
        public static List<Job> AllJobs()
        {
            // Calculate the first Monday in July of the current year
            DateTime july1st = new DateTime(DateTime.Today.Year, 7, 1);
            int daysUntilMonday = ((int)DayOfWeek.Monday - (int)july1st.DayOfWeek + 7) % 7;
            var startOfWeek = july1st.AddDays(daysUntilMonday);
            
            // Planning window is 14 days from that Monday
            DateTime planningEnd = startOfWeek.AddDays(14);
            
            LogInfo($"Planning period: {startOfWeek:yyyy-MM-dd} (Monday) to {planningEnd:yyyy-MM-dd}");

            var jobs = new List<Job>
            {
                new Job
                {
                    Name = "JobA",
                    HasAllRequiredParts = true,             
                    Criticality = 10,
                    DueDate = startOfWeek.AddDays(2),      
                    DurationDays = 2,
                    RequiredTechnicians = 2,
                    CanBePerformedNow = true
                },
                new Job
                {
                    Name = "JobB",
                    HasAllRequiredParts = true,         
                    Criticality = 8,
                    Dependencies = new List<string> { "JobA" },
                    DueDate = startOfWeek.AddDays(4),      
                    DurationDays = 3,
                    RequiredTechnicians = 2,
                    CanBePerformedNow = true
                },
                new Job
                {
                    Name = "JobC",
                    HasAllRequiredParts = false,           
                    Criticality = 5,
                    DueDate = startOfWeek.AddDays(1),      
                    DurationDays = 1,
                    RequiredTechnicians = 4,
                    CanBePerformedNow = false             
                },
                new Job
                {
                    Name = "JobD",
                    HasAllRequiredParts = true,
                    Criticality = 6,
                    Dependencies = new List<string> { "JobX" }, 
                    DueDate = startOfWeek.AddDays(3),      
                    DurationDays = 1,
                    RequiredTechnicians = 5,
                    CanBePerformedNow = false              
                },
                new Job
                {
                    Name = "JobE",
                    HasAllRequiredParts = true,
                    Criticality = 7,
                    DueDate = startOfWeek,                 
                    DurationDays = 1,
                    RequiredTechnicians = 2,
                    CanBePerformedNow = false             
                },
        
               
                new Job
                {
                    Name = "JobF",
                    HasAllRequiredParts = true,            
                    Criticality = 3,
                    DueDate = startOfWeek.AddDays(9),
                    DurationDays = 1,
                    RequiredTechnicians = 3,
                    CanBePerformedNow = true
                },
                
                new Job
                {
                    Name = "JobG",
                    HasAllRequiredParts = true,         
                    Criticality = 5,
                    DueDate = startOfWeek.AddDays(6),
                    DurationDays = 1,
                    RequiredTechnicians = 4,
                    CanBePerformedNow = true
                },
                new Job
                {
                    Name = "JobI",
                    HasAllRequiredParts = true,        
                    Criticality = 6,
                    DueDate = startOfWeek.AddDays(6),
                    DurationDays = 1,
                    RequiredTechnicians = 2,
                    CanBePerformedNow = true
                },
                
                new Job
                {
                    Name = "JobJ",
                    HasAllRequiredParts = false,
                    Criticality = 8,
                    DueDate = startOfWeek.AddDays(1),    
                    DurationDays = 1,
                    RequiredTechnicians = 1,
                    CanBePerformedNow = true             
                },
                
                new Job
                {
                    Name = "JobK",
                    HasAllRequiredParts = true,         
                    Criticality = 10,
                    DueDate = startOfWeek.AddDays(7),
                    DurationDays = 1,
                    RequiredTechnicians = 2,
                    CanBePerformedNow = true
                },

                new Job
                {
                    Name = "JobL",
                    HasAllRequiredParts = true,         
                    Criticality = 9,
                    DueDate = startOfWeek.AddDays(8),
                    DurationDays = 1,
                    RequiredTechnicians = 3,
                    CanBePerformedNow = true
                },

                new Job
                {
                    Name = "JobM",
                    HasAllRequiredParts = true,        
                    Criticality = 4,
                    Dependencies = new List<string> { "JobK" }, 
                    DueDate = startOfWeek.AddDays(9),
                    DurationDays = 1,
                    RequiredTechnicians = 2,
                    CanBePerformedNow = true
                },

                new Job
                {
                    Name = "JobN",
                    HasAllRequiredParts = true,         
                    Criticality = 2,
                    DueDate = startOfWeek.AddDays(10),
                    DurationDays = 1,
                    RequiredTechnicians = 1,
                    CanBePerformedNow = true
                },

                new Job
                {
                    Name = "JobO",
                    HasAllRequiredParts = true,         
                    Criticality = 10,
                    DueDate = startOfWeek.AddDays(11),
                    DurationDays = 4,
                    RequiredTechnicians = 2,
                    CanBePerformedNow = true
                },

                new Job
                {
                    Name = "JobP",
                    HasAllRequiredParts = false,        
                    Criticality = 1,
                    DueDate = startOfWeek.AddDays(12),
                    DurationDays = 1,
                    RequiredTechnicians = 2,
                    CanBePerformedNow = true
                },

            };

            return jobs;
        }

        private static void LogInfo(string message)
        {
            Console.WriteLine(message);
        }
    }
}
