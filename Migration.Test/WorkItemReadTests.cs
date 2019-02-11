using TFSProjectMigration;
using Xunit;

namespace Migration.Test
{
    public class WorkItemReadTests
    {
        [Theory]
        [InlineData("D:\\TotalTfsMigrationTool\\TFSProjectMigration\\bin\\Debug\\Attachments\\2444\\",
            "test.txt",
            "D:\\TotalTfsMigrationTool\\TFSProjectMigration\\bin\\Debug\\Attachments\\2444\\test.txt")]
        [InlineData("D:\\TotalTfsMigrationTool\\TFSProjectMigration\\bin\\Debug\\Attachments\\2444\\",
            "Error (System_Web_HttpException)_ A public action method 'SupplierRatingsGrid' was not found on controller 'DN_ProContract_Web_UI_Controllers_SupplierManagement_SupplierDetailsController'_.msg",
            "D:\\TotalTfsMigrationTool\\TFSProjectMigration\\bin\\Debug\\Attachments\\2444\\Error (System_Web_HttpException)_ A public action method 'SupplierRatingsGrid' was not found on controller 'DN_ProContract_Web_UI_Controllers_SupplierManagement_SupplierDetailsControl.msg")]
        public void EnsureAllowedFilePathLength(string directoryPath, string fileName, string expectedResult)
        {
            var actualResult = WorkItemRead.EnsureAllowedFilePathLength(directoryPath, fileName);

            Assert.Equal(expectedResult, actualResult);
        }

        [Theory]
        [InlineData(1234, "D:\\TotalTfsMigrationTool\\TFSProjectMigration\\bin\\Debug\\Attachments\\2444\\testing.txt")]
        public void EnsureUniqueFileName(int attachmentId, string filePath)
        {
            var actualResult = WorkItemRead.EnsureUniqueFileName(attachmentId, filePath);

            Assert.NotEqual(filePath, actualResult);
            Assert.Equal("D:\\TotalTfsMigrationTool\\TFSProjectMigration\\bin\\Debug\\Attachments\\2444\\1234_testing.txt",
                actualResult);
        }
    }
}
