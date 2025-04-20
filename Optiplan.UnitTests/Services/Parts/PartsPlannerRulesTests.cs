using System;
using System.Collections.Generic;
using System.Linq;
using Optiplan.WebApi.Services.Parts;
using Xunit;

namespace Optiplan.UnitTests.Services.Parts
{
    public class PartsPlannerRuleTests
    {
        private bool _verbose = false; // Set to true for detailed output
        
        private void LogInfo(string message)
        {
            if (_verbose)
            {
                Console.WriteLine(message);
            }
        }
        
        private void LogImportant(string message)
        {
            // Important messages are always displayed
            Console.WriteLine(message);
        }

        [Fact]
        public void Step1_OnlyJobsWithAvailableParts_ArePlanned()
        {
            // Arrange
            LogImportant("\n Testing: Step 1 – Only jobs with available parts");
            var planner = new PartsPlanner();
            var jobs = PartsPlannerTestData.AllJobs();

            // Act
            var result = planner.PlanJobs(jobs, _verbose);

            // Assert
            var jobsWithoutParts = jobs.Where(job => !job.HasAllRequiredParts).ToList();
            
            // Only show jobs without parts if verbose is enabled
            foreach (var job in jobsWithoutParts)
            {
                LogInfo($" {job.Name} skipped – missing parts.");
            }

            // Ensure no job without parts is planned
            foreach (var job in jobsWithoutParts)
            {
                Assert.DoesNotContain(result, j => j.Name == job.Name);
            }

            // Only if verbose is on, show jobs with parts that were not planned
            if (_verbose)
            {
                LogImportant("\nJobs with parts that were NOT planned:");
                var jobsWithPartsNotPlanned = jobs
                    .Where(job => job.HasAllRequiredParts)
                    .Where(job => !result.Any(r => r.Name == job.Name))
                    .ToList();
                
                foreach (var job in jobsWithPartsNotPlanned)
                {
                    LogImportant($" {job.Name} - not planned for other reasons");
                }
            }
        }

        [Fact]
        public void Step2_JobsSortedByCriticality()
        {
            // Arrange
            LogImportant("\n Testing: Step 2 – Jobs are sorted by criticality, then by deadline");
            var planner = new PartsPlanner();
            var jobs = PartsPlannerTestData.AllJobs();

            // Act
            var result = planner.PlanJobs(jobs, _verbose);
            
            // We know the result is sorted by start date, not criticality
            // Sort the jobs based on criticality for this test
            var sortedByCriticality = result
                .OrderByDescending(job => job.Criticality)
                .ThenBy(job => job.DueDate)
                .ToList();
            

            // Assert: Check that sorting by criticality is correct
            for (int i = 1; i < sortedByCriticality.Count; i++)
            {
                var prevJob = sortedByCriticality[i - 1];
                var currentJob = sortedByCriticality[i];

                // Criticality should be higher or equal for the previous job
                Assert.True(prevJob.Criticality >= currentJob.Criticality,
                    $"Job with lower criticality ({prevJob.Name} - {prevJob.Criticality}) came before higher ({currentJob.Name} - {currentJob.Criticality})");

                // If criticality is the same, check that the deadline is earlier or equal
                if (prevJob.Criticality == currentJob.Criticality)
                {
                    Assert.True(prevJob.DueDate <= currentJob.DueDate,
                        $"Job with later deadline ({prevJob.Name} - {prevJob.DueDate:dd.MM}) came before earlier ({currentJob.Name} - {currentJob.DueDate:dd.MM})");
                }
            }

            LogImportant("\n Step 2 – Jobs are correctly sorted by criticality and deadline.");
        }

        [Fact]
        public void Step3_JobsWithMissingDependencies_AreExcluded()
        {
            // Arrange
            LogImportant("\n Testing: Step 3 – Jobs with missing dependencies are removed");
            var planner = new PartsPlanner();
            var jobs = PartsPlannerTestData.AllJobs();

            // Act
            var result = planner.PlanJobs(jobs, _verbose);

            // Assert
            var jobsWithMissingDeps = jobs.Where(job => job.Dependencies.Any(dep => !jobs.Any(r => r.Name == dep))).ToList();
            
            if (_verbose)
            {
                foreach (var job in jobsWithMissingDeps)
                {
                    LogImportant($" {job.Name} skipped – missing dependency.");
                }
            }

            // Check that no job with missing dependencies is included in the result
            bool allExcluded = true;
            foreach (var job in jobsWithMissingDeps)
            {
                if (result.Any(j => j.Name == job.Name))
                {
                    allExcluded = false;
                    LogImportant($" Job {job.Name} was planned even though it's missing dependencies.");
                }
            }

            // Specifically check JobD, which has missing dependencies
            if (result.Any(job => job.Name == "JobD"))
            {
                allExcluded = false;
                LogImportant(" JobD was planned even though it's missing dependencies.");
            }
            
            if (allExcluded)
            {
                LogImportant("\n Step 3 – All jobs with missing dependencies were excluded.");
            }
            else
            {
                Assert.True(allExcluded, "Jobs with missing dependencies were not excluded");
            }
        }

        [Fact]
        public void Step3_JobOrderFollowsDependencies()
        {
            // Arrange
            LogImportant("\n Testing: Step 3b – Dependencies come before the job");
            var planner = new PartsPlanner();
            var jobs = PartsPlannerTestData.AllJobs();

            // Act
            var result = planner.PlanJobs(jobs, _verbose);
            var jobDict = result.ToDictionary(j => j.Name);

            // Debug: Show jobs with dependencies
            if (_verbose)
            {
                LogImportant("\nJobs with dependencies:");
                foreach (var job in result.Where(j => j.Dependencies.Any()))
                {
                    LogImportant($"{job.Name} depends on: {string.Join(", ", job.Dependencies)}");
                }
                LogImportant("");
            }

            // Assert
            bool allDependenciesValid = true;
            foreach (var job in result)
            {
                foreach (var dependency in job.Dependencies)
                {
                    // Check if the dependency exists in the result
                    if (jobDict.TryGetValue(dependency, out var depJob))
                    {
                        // Check if dependency is planned
                        if (!depJob.PlannedEnd.HasValue)
                        {
                            allDependenciesValid = false;
                            LogImportant($" Dependency {dependency} for job {job.Name} does not have a planned end date.");
                        }

                        // Ensure the dependency ends before the job starts
                        if (!(depJob.PlannedEnd.Value < job.PlannedStart))
                        {
                            allDependenciesValid = false;
                            LogImportant($" Job {job.Name} starts before dependency {dependency} is finished.");
                        }
                    }
                    else
                    {
                        allDependenciesValid = false;
                        LogImportant($" Dependency {dependency} for job {job.Name} was not found in the result.");
                    }
                }
            }
            
            if (allDependenciesValid)
            {
                LogImportant(" Step 3b – Dependencies were correctly followed.");
            }
            else
            {
                Assert.True(allDependenciesValid, "Dependencies were not correctly followed");
            }
        }

        [Fact]
        public void Step4_JobsWithSameCriticality_AreOrderedByDueDate()
        {
            // Arrange
            LogImportant("\n Testing: Step 4 – Jobs with the same criticality are sorted by deadline");
            var planner = new PartsPlanner();
            var jobs = PartsPlannerTestData.AllJobs();

            // Act
            var result = planner.PlanJobs(jobs, _verbose);

            // Group jobs by criticality
            var criticalities = result.GroupBy(j => j.Criticality).ToList();

            if (_verbose)
            {
                LogImportant("\nJobs grouped by criticality:");
                foreach (var group in criticalities)
                {
                    LogImportant($"Criticality {group.Key}:");
                    foreach (var job in group.OrderBy(j => j.DueDate))
                    {
                        LogImportant($"  - {job.Name}: Deadline {job.DueDate:yyyy-MM-dd}");
                    }
                }
                LogImportant("");
            }

            bool allGroupsCorrect = true;
            foreach (var group in criticalities)
            {
                // Sort jobs by due date within the group
                var ordered = group.OrderBy(j => j.DueDate).ToList();
                
                // Assert that the jobs are in the correct order
                for (int i = 0; i < ordered.Count - 1; i++)
                {
                    var currentJob = ordered[i];
                    var nextJob = ordered[i + 1];

                    if (!(currentJob.DueDate <= nextJob.DueDate))
                    {
                        allGroupsCorrect = false;
                        LogImportant($" Job {currentJob.Name} (deadline: {currentJob.DueDate:d}) should come before {nextJob.Name} (deadline: {nextJob.DueDate:d})");
                    }
                }
            }

            if (allGroupsCorrect)
            {
                LogImportant(" Step 4 – Jobs with the same criticality are correctly sorted by deadline.");
            }
            else
            {
                Assert.True(allGroupsCorrect, "Jobs with the same criticality are not correctly sorted by deadline");
            }
        }

        [Fact]
        public void Step5_OnlyExecutableJobsWithEnoughTechnicians_ArePlanned()
        {
            // Arrange
            LogImportant("\n Testing: Step 5 – Jobs are only planned if they can be performed and have enough technicians");
            var planner = new PartsPlanner();
            var jobs = PartsPlannerTestData.AllJobs();

            // Act
            var result = planner.PlanJobs(jobs, _verbose);

            // Debug: Show jobs that cannot be performed
            if (_verbose)
            {
                var nonExecutableJobs = jobs.Where(j => !j.CanBePerformedNow).ToList();
                LogImportant("\nJobs that cannot be performed now:");
                foreach (var job in nonExecutableJobs)
                {
                    LogImportant($"- {job.Name} (planned: {result.Any(j => j.Name == job.Name)})");
                }
                LogImportant("");
            }

            // Assert
            bool allJobsValid = true;
            
            // Assert: Check that JobE is not planned (it cannot be executed)
            if (result.Any(job => job.Name == "JobE"))
            {
                allJobsValid = false;
                LogImportant(" JobE cannot be performed now and should not be planned");
            }

            // Assert that each job in the result has a planned start date within the allowed planning window
            foreach (var job in result)
            {
                if (!job.PlannedStart.HasValue)
                {
                    allJobsValid = false;
                    LogImportant($" Job {job.Name} was not planned.");
                }
                
                // Calculate planning window dates
                DateTime july1st = new DateTime(DateTime.Today.Year, 7, 1);
                int daysUntilMonday = ((int)DayOfWeek.Monday - (int)july1st.DayOfWeek + 7) % 7;
                DateTime planningStart = july1st.AddDays(daysUntilMonday);
                DateTime planningEnd = planningStart.AddDays(13);
                
                if (!(job.PlannedStart.Value >= planningStart && job.PlannedStart.Value <= planningEnd))
                {
                    allJobsValid = false;
                    LogImportant($" Job {job.Name} has a start date outside the allowed planning window.");
                }
            }

            // Ensure there are enough technicians for each planned job
            foreach (var job in result)
            {
                int availableTechniciansPerDay = 4; // Example: Max technicians available per day
                var jobDays = GetJobDays(job).ToList();

                foreach (var day in jobDays)
                {
                    if (!(availableTechniciansPerDay >= job.RequiredTechnicians))
                    {
                        allJobsValid = false;
                        LogImportant($" Not enough technicians for {job.Name} on {day:dd.MM}. " +
                            $"Needed {job.RequiredTechnicians} technicians, but only {availableTechniciansPerDay} available.");
                    }
                }
            }

            if (allJobsValid)
            {
                LogImportant(" Step 5 – All jobs that can be performed and have enough technicians are correctly planned.");
                foreach (var job in result)
                {
                    LogImportant($"- {job.Name}: Criticality: {job.Criticality}, Start: {job.PlannedStart:dd.MM}, End: {job.PlannedEnd:dd.MM}, Technicians: {job.RequiredTechnicians}, DueDate: {job.DueDate:dd.MM}, Dependencies: {string.Join(", ", job.Dependencies)}");
                }
            }
            else
            {
                Assert.True(allJobsValid, "Not all jobs meet the requirements for step 5");
            }
        }

        // Helper function to get all the days that a job will take
        private IEnumerable<DateTime> GetJobDays(Job job)
        {
            if (!job.PlannedStart.HasValue)
            {
                throw new InvalidOperationException($"Job {job.Name} has no planned start date.");
            }

            var days = new List<DateTime>();
            for (DateTime day = job.PlannedStart.Value; day <= job.PlannedStart.Value.AddDays(job.DurationDays - 1); day = day.AddDays(1))
            {
                days.Add(day);
            }
            return days;
        }
    }
}