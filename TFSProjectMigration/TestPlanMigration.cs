using log4net;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.TestManagement.Client;
using System;
using System.Collections;
using System.Windows.Controls;

namespace TFSProjectMigration
{
    public class TestPlanMigration
    {
        private ITestManagementTeamProject sourceproj;
        private ITestManagementTeamProject destinationproj;
        public Hashtable workItemMap;
        private ProgressBar progressBar;
        private String projectName;
        private static readonly ILog logger = LogManager.GetLogger(typeof(TFSWorkItemMigrationUI));

        public TestPlanMigration(TfsTeamProjectCollection sourceTfs, TfsTeamProjectCollection destinationTfs, string sourceProject, string destinationProject, Hashtable workItemMap, ProgressBar progressBar)
        {
            sourceproj = GetProject(sourceTfs, sourceProject);
            destinationproj = GetProject(destinationTfs, destinationProject);
            this.workItemMap = workItemMap;
            this.progressBar = progressBar;
            projectName = sourceProject;
        }

        private ITestManagementTeamProject GetProject(TfsTeamProjectCollection tfs, string project)
        {
            ITestManagementService tms = tfs.GetService<ITestManagementService>();

            return tms.GetTeamProject(project);
        }

        public void CopyTestPlans()
        {
            destinationproj.WitProject.Store.SyncToCache();

            int i = 1;
            int planCount = sourceproj.TestPlans.Query("Select * From TestPlan").Count;

            foreach (ITestPlan sourceplan in sourceproj.TestPlans.Query("Select * From TestPlan"))
            {
                System.Diagnostics.Debug.WriteLine("Plan - {0} : {1}", sourceplan.Id, sourceplan.Name);

                ITestPlan destinationplan = destinationproj.TestPlans.Create();

                destinationplan.Name = sourceplan.Name;
                destinationplan.Description = sourceplan.Description;
                destinationplan.StartDate = sourceplan.StartDate;
                destinationplan.EndDate = sourceplan.EndDate;
                destinationplan.State = sourceplan.State;
                destinationplan.Save();

                //drill down to root test suites.
                if (sourceplan.RootSuite != null && sourceplan.RootSuite.Entries.Count > 0)
                {
                    CopyTestSuites(sourceplan, destinationplan);
                }

                destinationplan.Save();

                progressBar.Dispatcher.BeginInvoke(new Action(delegate ()
                {
                    float progress = i / (float)planCount;

                    progressBar.Value = (i / (float)planCount) * 100;
                }));
                i++;
            }
        }

        //Copy all Test suites from source plan to destination plan.
        private void CopyTestSuites(ITestPlan sourceplan, ITestPlan destinationplan)
        {
            ITestSuiteEntryCollection suites = sourceplan.RootSuite.Entries;
            CopyTestCases(sourceplan.RootSuite, destinationplan.RootSuite);

            foreach (ITestSuiteEntry suite_entry in suites)
            {
                var suite = suite_entry.TestSuite;
                if (suite != null)
                {
                    IStaticTestSuite newSuite = destinationproj.TestSuites.CreateStatic();
                    newSuite.Title = suite.Title;
                    destinationplan.RootSuite.Entries.Add(newSuite);
                    destinationplan.Save();

                    var staticTestSuite = suite_entry.TestSuite as IStaticTestSuite;
                    if (staticTestSuite != null)
                    {
                        CopyTestCases(staticTestSuite, newSuite);
                        if (staticTestSuite.Entries.Count > 0)
                            CopySubTestSuites(staticTestSuite, newSuite);
                    }
                }
            }
        }
        //Drill down and Copy all subTest suites from source root test suite to destination plan's root test suites.
        private void CopySubTestSuites(IStaticTestSuite parentsourceSuite, IStaticTestSuite parentdestinationSuite)
        {
            ITestSuiteEntryCollection suitcollection = parentsourceSuite.Entries;
            foreach (ITestSuiteEntry suite_entry in suitcollection)
            {
                IStaticTestSuite suite = suite_entry.TestSuite as IStaticTestSuite;
                if (suite != null)
                {
                    IStaticTestSuite subSuite = destinationproj.TestSuites.CreateStatic();
                    subSuite.Title = suite.Title;
                    parentdestinationSuite.Entries.Add(subSuite);

                    CopyTestCases(suite, subSuite);

                    if (suite.Entries.Count > 0)
                        CopySubTestSuites(suite, subSuite);
                }
            }
        }

        //Copy all subTest suites from source root test suite to destination plan's root test suites.
        private void CopyTestCases(IStaticTestSuite sourcesuite, IStaticTestSuite destinationsuite)
        {
            ITestSuiteEntryCollection suiteentrys = sourcesuite.TestCases;

            foreach (ITestSuiteEntry testcase in suiteentrys)
            {
                try
                {   //check whether testcase exists in new work items(closed work items may not be created again).
                    if (!workItemMap.ContainsKey(testcase.TestCase.WorkItem.Id))
                    {
                        continue;
                    }

                    int newWorkItemID = (int)workItemMap[testcase.TestCase.WorkItem.Id];
                    ITestCase tc = destinationproj.TestCases.Find(newWorkItemID);
                    destinationsuite.Entries.Add(tc);

                    bool updateTestCase = false;
                    TestActionCollection testActionCollection = tc.Actions;
                    foreach (var item in testActionCollection)
                    {
                        updateTestCase = true;
                        item.CopyToNewOwner(tc);
                    }
                    if (updateTestCase)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Test case with Id: {0} updated", tc.Id);
                        tc.Save();
                    }
                }
                catch (Exception)
                {
                    logger.Info("Error retrieving Test case  " + testcase.TestCase.WorkItem.Id + ": " + testcase.Title);
                }
            }
        }
    }
}
