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
        [InlineData("To Do", "User Story", "New")]
        [InlineData("New", "User Story", "New")]
        [InlineData("Committed", "User Story", "Active")]
        [InlineData("Done", "User Story", "Closed")]
        [InlineData("Removed", "User Story", "Removed")]
        [InlineData("Approved", "User Story", "Active")]
        [InlineData("Closed", "User Story", "Closed")]
        [InlineData("In Progress", "User Story", "Active")]
        [InlineData("Removed", "Bug", "Closed")]
        //From CMMI
        [InlineData("Proposed", "User Story", "New")]
        [InlineData("Active", "User Story", "Active")]
        [InlineData("Resolved", "User Story", "Resolved")]
        [InlineData("Closed", "User Story", "Closed")]
        [InlineData("Removed", "User Story", "Removed")]
        public void MapStateFieldValue_To_Agile(string sourceValue, string destinationWorkItemType, string expectedResult)
        {
            var actualResult = WorkItemWrite.MapStateFieldValue(new[] { "User Story", "Bug", "Epic", "Feature", "Task", "Issue", "Test Case" }, destinationWorkItemType, sourceValue);

            Assert.Equal(expectedResult, actualResult);
        }

        [Theory]
        //From Agile
        [InlineData("New", "Product Backlog Item", "New")]
        [InlineData("Active", "Product Backlog Item", "Approved")]
        [InlineData("Resolved", "Product Backlog Item", "Committed")]
        [InlineData("Closed", "Product Backlog Item", "Done")]
        [InlineData("Removed", "Product Backlog Item", "Removed")]
        //From CMMI
        [InlineData("Proposed", "Product Backlog Item", "New")]
        [InlineData("Active", "Product Backlog Item", "Approved")]
        [InlineData("Resolved", "Product Backlog Item", "Committed")]
        [InlineData("Closed", "Product Backlog Item", "Done")]
        [InlineData("Removed", "Product Backlog Item", "Removed")]
        public void MapStateFieldValue_To_Scrum(string sourceValue, string destinationWorkItemType, string expectedResult)
        {
            var actualResult = WorkItemWrite.MapStateFieldValue(new[] { "Product Backlog Item", "Bug", "Epic", "Feature", "Task", "Impediment", "Test Case" }, destinationWorkItemType, sourceValue);

            Assert.Equal(expectedResult, actualResult);
        }

        [Theory]
        //From Agile
        [InlineData("New", "Requirement", "Proposed")]
        [InlineData("Active", "Requirement", "Active")]
        [InlineData("Resolved", "Requirement", "Resolved")]
        [InlineData("Closed", "Requirement", "Closed")]
        [InlineData("Removed", "Requirement", "Removed")]
        //From Scrum
        [InlineData("To Do", "Requirement", "Proposed")]
        [InlineData("New", "Requirement", "Proposed")]
        [InlineData("Approved", "Requirement", "Active")]
        [InlineData("Committed", "Requirement", "Resolved")]
        [InlineData("Done", "Requirement", "Closed")]
        [InlineData("Removed", "Requirement", "Removed")]
        public void MapStateFieldValue_To_CMMI(string sourceValue, string destinationWorkItemType, string expectedResult)
        {
            var actualResult = WorkItemWrite.MapStateFieldValue(new[] { "Requirement", "Bug", "Epic", "Feature", "Task", "Issue", "Issue", "Risk", "Review", "Test Case" }, destinationWorkItemType, sourceValue);

            Assert.Equal(expectedResult, actualResult);
        }

        [Theory]
        [InlineData("http://google.com", true)]
        [InlineData("https://google.com", true)]
        [InlineData("mailto://test@test.com", true)]
        [InlineData("ftp://google.com", true)]
        [InlineData("speclog://aigrouptfs/AIProcurement/requirements/42555f3c-6415-47b7-82b2-586cbbfd8d3a", false)]
        public void IsValidTfsUri(string uri, bool expectedResult)
        {
            var actualResult = WorkItemWrite.IsValidTfsUri(uri);

            Assert.Equal(expectedResult, actualResult);
        }
    }
}
