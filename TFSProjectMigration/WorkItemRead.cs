using log4net;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.IO;
using System.Linq;
using System.Xml;

namespace TFSProjectMigration
{
    public class WorkItemRead
    {
        private TfsTeamProjectCollection tfs;
        public WorkItemStore store;
        public QueryHierarchy queryCol;
        private String projectName;
        public WorkItemTypeCollection workItemTypes;
        private static readonly ILog logger = LogManager.GetLogger(typeof(TFSWorkItemMigrationUI));

        public WorkItemRead(TfsTeamProjectCollection tfs, Project sourceProject)
        {
            this.tfs = tfs;
            projectName = sourceProject.Name;
            store = (WorkItemStore)tfs.GetService(typeof(WorkItemStore));
            queryCol = store.Projects[sourceProject.Name].QueryHierarchy;
            workItemTypes = store.Projects[sourceProject.Name].WorkItemTypes;
        }

        /* Get required work items from project and save existing attachments of workitems to local folder */
        public WorkItemCollection GetWorkItems(string project, System.Windows.Controls.ProgressBar ProgressBar)
        {
            WorkItemCollection workItemCollection = store.Query(" SELECT * " +
                                                                 " FROM WorkItems " +
                                                                 " WHERE [System.TeamProject] = '" + project +
                                                                 "' AND [System.State] <> 'Closed' ORDER BY [System.Id]");
            SaveAttachments(workItemCollection, ProgressBar);
            return workItemCollection;
        }


        public WorkItemCollection GetWorkItems(string project, bool IsNotIncludeClosed, bool IsNotIncludeRemoved, System.Windows.Controls.ProgressBar ProgressBar)
        {
            String query = "";
            if (IsNotIncludeClosed && IsNotIncludeRemoved)
            {
                query = String.Format(" SELECT * " +
                                                    " FROM WorkItems " +
                                                    " WHERE [System.TeamProject] = '" + project +
                                                    "' AND [System.State] <> 'Closed' AND [System.State] <> 'Removed' ORDER BY [System.Id]");
            }

            else if (IsNotIncludeRemoved)
            {
                query = String.Format(" SELECT * " +
                                                   " FROM WorkItems " +
                                                   " WHERE [System.TeamProject] = '" + project +
                                                   "' AND [System.State] <> 'Removed' ORDER BY [System.Id]");
            }
            else if (IsNotIncludeClosed)
            {
                query = String.Format(" SELECT * " +
                                                   " FROM WorkItems " +
                                                   " WHERE [System.TeamProject] = '" + project +
                                                   "' AND [System.State] <> 'Closed'  ORDER BY [System.Id]");
            }
            else
            {
                query = String.Format(" SELECT * " +
                                                   " FROM WorkItems " +
                                                   " WHERE [System.TeamProject] = '" + project +
                                                   "' ORDER BY [System.Id]");
            }
            System.Diagnostics.Debug.WriteLine(query);
            WorkItemCollection workItemCollection = store.Query(query);
            SaveAttachments(workItemCollection, ProgressBar);
            return workItemCollection;
        }
        /* Save existing attachments of workitems to local folders of workitem ID */
        private void SaveAttachments(WorkItemCollection workItemCollection, System.Windows.Controls.ProgressBar ProgressBar)
        {
            if (!Directory.Exists(@"Attachments"))
            {
                Directory.CreateDirectory(@"Attachments");
            }
            //else
            //{
            //    EmptyFolder(new DirectoryInfo(@"Attachments"));
            //}

            System.Net.WebClient webClient = new System.Net.WebClient();
            webClient.UseDefaultCredentials = true;

            int index = 0;
            foreach (WorkItem wi in workItemCollection)
            {
                if (wi.AttachedFileCount > 0)
                {
                    String path = @"Attachments\" + wi.Id;
                    bool folderExists = Directory.Exists(path);
                    if (!folderExists)
                    {
                        Directory.CreateDirectory(path);
                    }
                    else
                    {
                        //If directory exists, we're assuming we've had a previously successful run for this work item
                        continue;
                    }

                    foreach (Attachment att in wi.Attachments)
                    {
                        try
                        {
                            var filePath = EnsureAllowedFilePathLength(path, att.Name);

                            var isUniqueNme = true;
                            if (wi.AttachedFileCount > 1)
                                foreach (Attachment uniqueCheck in wi.Attachments)
                                {
                                    if (uniqueCheck.Id != att.Id && uniqueCheck.Name.Equals(att.Name, StringComparison.OrdinalIgnoreCase))
                                    {
                                        isUniqueNme = false;
                                        break;
                                    }
                                }

                            if (!isUniqueNme)
                            {
                                filePath = EnsureUniqueFileName(att.Id, filePath);
                                filePath = EnsureAllowedFilePathLength(path, Path.GetFileName(filePath));
                            }

                            webClient.DownloadFile(att.Uri, filePath);
                        }
                        catch (Exception)
                        {
                            logger.Info("Error downloading attachment for work item : " + wi.Id + " Type: " + wi.Type.Name);
                        }
                    }
                }
                index++;
                ProgressBar.Dispatcher.BeginInvoke(new Action(delegate ()
                {
                    float progress = index / (float)workItemCollection.Count;
                    ProgressBar.Value = (index / (float)workItemCollection.Count) * 100;
                }));
            }
        }

        internal static string EnsureAllowedFilePathLength(string directoryPath, string fileName)
        {
            //The specified path, file name, or both are too long. The fully qualified file name must be less than 260 characters, 
            //and the directory name must be less than 248 characters.

            const int maxPathLength = 259;

            //Ensure full directory path
            directoryPath = Path.GetFullPath(directoryPath);

            var fullPath = Path.Combine(directoryPath, fileName);

            if (fullPath.Length <= maxPathLength)
                return fullPath;

            var overflowAmount = fullPath.Length - maxPathLength;

            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            fileNameWithoutExtension = fileNameWithoutExtension.Substring(0, fileNameWithoutExtension.Length - overflowAmount);

            return Path.Combine(directoryPath, $"{fileNameWithoutExtension}{Path.GetExtension(fileName)}");
        }

        internal static string EnsureUniqueFileName(int attachmentId, string filePath)
        {
            return Path.Combine(Path.GetDirectoryName(filePath), $"{attachmentId}_{Path.GetFileName(filePath)}");
        }

        /*Delete all subfolders and files in given folder*/
        private void EmptyFolder(DirectoryInfo directoryInfo)
        {
            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo subfolder in directoryInfo.GetDirectories())
            {
                EmptyFolder(subfolder);
                subfolder.Delete();
            }
        }

        /* Return Areas and Iterations of the project */
        public XmlNode[] PopulateIterations()
        {
            ICommonStructureService css = (ICommonStructureService)tfs.GetService(typeof(ICommonStructureService));
            //Gets Area/Iteration base Project
            ProjectInfo projectInfo = css.GetProjectFromName(projectName);
            NodeInfo[] nodes = css.ListStructures(projectInfo.Uri);

            XmlElement areaTree = css.GetNodesXml(new[] { nodes.Single(n => n.StructureType == "ProjectModelHierarchy").Uri }, true);
            XmlElement iterationsTree = css.GetNodesXml(new[] { nodes.Single(n => n.StructureType == "ProjectLifecycle").Uri }, true);

            XmlNode areaNodes = areaTree.ChildNodes[0];
            XmlNode iterationsNodes = iterationsTree.ChildNodes[0];

            return new XmlNode[] { areaNodes, iterationsNodes };
        }
    }
}
