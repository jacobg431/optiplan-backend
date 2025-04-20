using System;
using System.Collections.Generic;
using System.Linq;

namespace Optiplan.WebApi.Services.Parts
{
    public class PartsPlanner
    {
        private bool _verbose = false;

        public List<Job> PlanJobs(List<Job> allJobs, bool verbose = false)
        {
            _verbose = verbose;
            
            // Calculate the first Monday in July
            DateTime july1st = new DateTime(DateTime.Today.Year, 7, 1);
            int daysUntilMonday = ((int)DayOfWeek.Monday - (int)july1st.DayOfWeek + 7) % 7;
            DateTime planningStart = july1st.AddDays(daysUntilMonday);
            DateTime planningEnd = planningStart.AddDays(14);
            
            LogInfo($"Planning from {planningStart:yyyy-MM-dd} (Monday) to {planningEnd:yyyy-MM-dd}");
            
            var plannedJobs = new List<Job>();
            var technicianAvailability = new Dictionary<DateTime, int>();

            // Initialize technician availability
            for (DateTime day = planningStart; day <= planningEnd; day = day.AddDays(1))
            {
                technicianAvailability[day] = 5; // Number of available technicians per day
            }
            
            // Get all valid jobs (with parts and executable)
            var validJobs = allJobs
                .Where(job => job.HasAllRequiredParts && job.CanBePerformedNow)
                .ToList();
            
            // Keep planning until all jobs are processed or no more can be planned
            bool madeProgress = true;
            while (madeProgress)
            {
                madeProgress = false;
                
                // Sort remaining jobs by criticality (highest first)
                var remainingJobs = validJobs
                    .Where(job => !plannedJobs.Any(p => p.Name == job.Name))
                    .OrderByDescending(job => job.Criticality)
                    .ToList();
                
                LogInfo($"Planning iteration with {remainingJobs.Count} jobs remaining");
                
                foreach (var job in remainingJobs)
                {
                    // Check if all dependencies are finished planning
                    bool allDependenciesFinished = true;
                    DateTime earliestStart = planningStart;

                    foreach (var depName in job.Dependencies)
                    {
                        var depJob = plannedJobs.FirstOrDefault(j => j.Name == depName);
                        if (depJob == null || !depJob.PlannedEnd.HasValue)
                        {
                            allDependenciesFinished = false;
                            LogInfo($"Skipped: {job.Name} – dependency {depName} not planned or not completed");
                            break;
                        }

                        // Update earliest start date based on the end date of the dependency
                        var depEnd = depJob.PlannedEnd.Value;
                        if (depEnd.AddDays(1) > earliestStart)
                        {
                            earliestStart = depEnd.AddDays(1);
                        }
                    }

                    if (!allDependenciesFinished)
                    {
                        continue;
                    }

                    // Check if the job can still be planned within the planning window
                    if (earliestStart > planningEnd)
                    {
                        LogInfo($"Skipped: {job.Name} – cannot be started within the planning window");
                        continue;
                    }
                    
                    // Check if planning the job would violate its due date
                    if (earliestStart.AddDays(job.DurationDays - 1) > job.DueDate)
                    {
                        LogInfo($"Warning: {job.Name} might be completed after its due date ({job.DueDate:dd.MM})");
                        // Still continue planning if possible, as high criticality jobs should be planned
                    }

                    // Find the first available start date where all necessary technicians are available
                    DateTime? possibleStartDate = null;
                    for (DateTime start = earliestStart; start <= planningEnd.AddDays(1 - job.DurationDays); start = start.AddDays(1))
                    {
                        bool allDaysAvailable = true;
                        for (int i = 0; i < job.DurationDays; i++)
                        {
                            var day = start.AddDays(i);
                            if (technicianAvailability[day] < job.RequiredTechnicians)
                            {
                                allDaysAvailable = false;
                                break;
                            }
                        }

                        if (allDaysAvailable)
                        {
                            possibleStartDate = start;
                            break;
                        }
                    }

                    if (possibleStartDate.HasValue)
                    {
                        // Update technician availability
                        for (int i = 0; i < job.DurationDays; i++)
                        {
                            var day = possibleStartDate.Value.AddDays(i);
                            technicianAvailability[day] -= job.RequiredTechnicians;
                        }

                        job.PlannedStart = possibleStartDate.Value;
                        plannedJobs.Add(job);
                        madeProgress = true;
                        
                        // Check if the job will be completed on time
                        bool onTime = job.PlannedEnd <= job.DueDate;
                        string timeStatus = onTime ? "On time" : "Past due date";
                        
                        LogInfo($"Planned: {job.Name} (Criticality {job.Criticality}) from {possibleStartDate.Value:dd.MM} to {job.PlannedEnd:dd.MM} - {timeStatus}");
                        
                        // Break out of the loop to restart with the sorted remaining jobs
                        // This ensures highest criticality jobs always get scheduled first
                        break;
                    }
                    else
                    {
                        LogInfo($"Skipped: {job.Name} – not enough technicians available");
                    }
                }
            }

            // Sort the final list by planned start date for the output
            return plannedJobs.OrderBy(job => job.PlannedStart).ToList();
        }
        
        private void LogInfo(string message)
        {
            if (_verbose)
            {
                Console.WriteLine(message);
            }
        }

        public void LogPlannedJobs(List<Job> plannedJobs)
        {
            Console.WriteLine("\nPlanned jobs in order by start date:");
            foreach (var job in plannedJobs.OrderBy(job => job.PlannedStart))
            {
                Console.WriteLine($"- {job.Name}: Start {job.PlannedStart:dd.MM}, End {job.PlannedEnd:dd.MM}");
            }
        }
    }

    public class Job
    {
        public required string Name { get; set; }
        public bool HasAllRequiredParts { get; set; }
        public int Criticality { get; set; }
        public List<string> Dependencies { get; set; } = new();
        public DateTime DueDate { get; set; }
        public int RequiredTechnicians { get; set; } = 1;
        public bool CanBePerformedNow { get; set; }
        public int DurationDays { get; set; } = 1;
        public DateTime? PlannedStart { get; set; }
        public DateTime? PlannedEnd => PlannedStart?.AddDays(DurationDays - 1);
    }
}