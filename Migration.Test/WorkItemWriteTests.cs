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

        [Theory]
        //From Scrum
        [InlineData("To Do", "New")]
        [InlineData("New", "New")]
        [InlineData("Committed", "Active")]
        [InlineData("Done", "Closed")]
        [InlineData("Removed", "Removed")]
        [InlineData("Approved", "Active")]
        [InlineData("Closed", "Closed")]
        [InlineData("In Progress", "Active")]
        //From CMMI
        [InlineData("Proposed", "New")]
        [InlineData("Active", "Active")]
        [InlineData("Resolved", "Resolved")]
        [InlineData("Closed", "Closed")]
        [InlineData("Removed", "Removed")]
        public void MapStateFieldValue_To_Agile(string sourceValue, string expectedResult)
        {
            var actualResult = WorkItemWrite.MapStateFieldValue(new[] { "User Story", "Bug", "Epic", "Feature", "Task", "Issue", "Test Case" }, sourceValue);

            Assert.Equal(expectedResult, actualResult);
        }

        [Theory]
        //From Agile
        [InlineData("New", "New")]
        [InlineData("Active", "Approved")]
        [InlineData("Resolved", "Committed")]
        [InlineData("Closed", "Done")]
        [InlineData("Removed", "Removed")]
        //From CMMI
        [InlineData("Proposed", "New")]
        [InlineData("Active", "Approved")]
        [InlineData("Resolved", "Committed")]
        [InlineData("Closed", "Done")]
        [InlineData("Removed", "Removed")]
        public void MapStateFieldValue_To_Scrum(string sourceValue, string expectedResult)
        {
            var actualResult = WorkItemWrite.MapStateFieldValue(new[] { "Product Backlog Item", "Bug", "Epic", "Feature", "Task", "Impediment", "Test Case" }, sourceValue);

            Assert.Equal(expectedResult, actualResult);
        }

        [Theory]
        //From Agile
        [InlineData("New", "Proposed")]
        [InlineData("Active", "Active")]
        [InlineData("Resolved", "Resolved")]
        [InlineData("Closed", "Closed")]
        [InlineData("Removed", "Removed")]
        //From Scrum
        [InlineData("To Do", "Proposed")]
        [InlineData("New", "Proposed")]
        [InlineData("Approved", "Active")]
        [InlineData("Committed", "Resolved")]
        [InlineData("Done", "Closed")]
        [InlineData("Removed", "Removed")]
        public void MapStateFieldValue_To_CMMI(string sourceValue, string expectedResult)
        {
            var actualResult = WorkItemWrite.MapStateFieldValue(new[] { "Requirement", "Bug", "Epic", "Feature", "Task", "Issue", "Issue", "Risk", "Review", "Test Case" }, sourceValue);

            Assert.Equal(expectedResult, actualResult);
        }
    }
}
