using System.Collections.Generic;
using TFSProjectMigration;
using Xunit;

namespace Migration.Test
{
    public class WorkItemWriteTests
    {
        [Theory]
        [InlineData("", null)]
        [InlineData(null, null)]
        [InlineData("Bug", "Bug")]
        [InlineData("Product Backlog Item", "User Story")]
        [InlineData("Impediment", "Issue")]
        public void GetWorkItemTypeMapping_To_Agile(string sourceWorkItemType, string expectedResult)
        {
            var agileTypes = new[] {
                "Bug",
                "Code Review Request",
                "Code Review Response",
                "Epic",
                "Feature",
                "Feedback Request",
                "Feedback Response",
                "Shared Steps",
                "Task",
                "Test Case",
                "Test Plan",
                "Test Suite",
                "User Story",
                "Issue",
                "Shared Parameter"
            };

            var actualResult = WorkItemWrite.GetMappedWorkItemTypeName(sourceWorkItemType, agileTypes);

            Assert.Equal(expectedResult, actualResult);
        }

        [Theory]
        [InlineData("", null)]
        [InlineData(null, null)]
        [InlineData("Bug", "Bug")]
        [InlineData("User Story", "Product Backlog Item")]
        [InlineData("Issue", "Impediment")]
        public void GetWorkItemTypeMapping_To_Scrum(string sourceWorkItemType, string expectedResult)
        {
            var agileTypes = new[] {
                "Bug",
                "Product Backlog Item",
                "Impediment",
                "Epic",
                "Feature"
            };

            var actualResult = WorkItemWrite.GetMappedWorkItemTypeName(sourceWorkItemType, agileTypes);

            Assert.Equal(expectedResult, actualResult);
        }
    }
}
