using CodeChallenge.Api.Logic;
using CodeChallenge.Api.Models;
using CodeChallenge.Api.Repositories;
using FluentAssertions;
using Moq;

namespace CodeChallenge.Tests
{
    public class MessageLogicTests
    {
        private readonly Mock<IMessageRepository> _repoMock;
        private readonly MessageLogic _logic;
        private readonly Guid _orgId = Guid.NewGuid();

        public MessageLogicTests()
        {
            _repoMock = new Mock<IMessageRepository>();
            _logic = new MessageLogic(_repoMock.Object);
        }

        [Fact]
        public async Task CreateMessage_ShouldReturnCreated_WhenValid()
        {
            var request = new CreateMessageRequest
            {
                Title = "Test Message",
                Content = "This is valid content with more than 10 chars."
            };

            _repoMock.Setup(r => r.GetByTitleAsync(_orgId, request.Title)).ReturnsAsync((Message?)null);

            var result = await _logic.CreateMessageAsync(_orgId, request);

            result.Should().BeOfType<Created<Message>>();
        }

        [Fact]
        public async Task CreateMessage_ShouldReturnConflict_WhenTitleExists()
        {
            var request = new CreateMessageRequest
            {
                Title = "Duplicate Title",
                Content = "Valid content text"
            };

            _repoMock.Setup(r => r.GetByTitleAsync(_orgId, request.Title)).ReturnsAsync(new Message());

            var result = await _logic.CreateMessageAsync(_orgId, request);

            result.Should().BeOfType<Conflict>();
        }

        [Fact]
        public async Task CreateMessage_ShouldReturnValidationError_WhenContentInvalid()
        {
            var request = new CreateMessageRequest
            {
                Title = "Valid Title",
                Content = "short"
            };

            var result = await _logic.CreateMessageAsync(_orgId, request);

            result.Should().BeOfType<ValidationError>();
        }

        [Fact]
        public async Task UpdateMessage_ShouldReturnNotFound_WhenMessageMissing()
        {
            _repoMock.Setup(r => r.GetByIdAsync(_orgId, It.IsAny<Guid>())).ReturnsAsync((Message?)null);

            var request = new UpdateMessageRequest
            {
                Title = "New Title",
                Content = "Valid updated content"
            };

            var result = await _logic.UpdateMessageAsync(_orgId, Guid.NewGuid(), request);

            result.Should().BeOfType<NotFound>();
        }

        [Fact]
        public async Task UpdateMessage_ShouldReturnConflict_WhenMessageInactive()
        {
            var inactiveMessage = new Message
            {
                Id = Guid.NewGuid(),
                OrganizationId = _orgId,
                IsActive = false,
                Title = "Old Title",
                Content = "Old content"
            };

            _repoMock.Setup(r => r.GetByIdAsync(_orgId, inactiveMessage.Id)).ReturnsAsync(inactiveMessage);

            var request = new UpdateMessageRequest
            {
                Title = "Updated",
                Content = "Updated content"
            };

            var result = await _logic.UpdateMessageAsync(_orgId, inactiveMessage.Id, request);

            result.Should().BeOfType<Conflict>();
        }

        [Fact]
        public async Task DeleteMessage_ShouldReturnNotFound_WhenMessageDoesNotExist()
        {
            _repoMock.Setup(r => r.GetByIdAsync(_orgId, It.IsAny<Guid>())).ReturnsAsync((Message?)null);

            var result = await _logic.DeleteMessageAsync(_orgId, Guid.NewGuid());

            result.Should().BeOfType<NotFound>();
        }

        [Fact]
        public async Task GetAllMessage_ShouldReturnData()
        {
            var messages = new List<Message>
            {
                new Message { Id = Guid.NewGuid(), Title = "A", Content = "Test", IsActive = true }
            };

            _repoMock.Setup(r => r.GetAllByOrganizationAsync(_orgId)).ReturnsAsync(messages);
            var result = await _logic.GetAllMessagesAsync(_orgId);
            result.Should().BeAssignableTo<IEnumerable<Message>>();
        }

        [Fact]
        public async Task GetAllMessage_ShouldReturnNoData()
        {
            var messages = new List<Message>();
            _repoMock.Setup(r => r.GetAllByOrganizationAsync(_orgId)).ReturnsAsync(messages);
            var result = await _logic.GetAllMessagesAsync(_orgId);
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetMessageById_ShouldReturnData()
        {
            var message = new Message
            {
                Id = Guid.NewGuid(), Title = "A", Content = "Test", IsActive = true 
            };

            _repoMock.Setup(r => r.GetByIdAsync(_orgId, _orgId)).ReturnsAsync(message);
            var result = await _logic.GetAllMessagesAsync(_orgId);
            result.Should().BeAssignableTo<IEnumerable<Message>>();
        }

        [Fact]
        public async Task GetMessageById_ShouldReturnNoData()
        {
            var message = new Message();
            _repoMock.Setup(r => r.GetByIdAsync(_orgId, _orgId)).ReturnsAsync(message);
            var result = await _logic.GetAllMessagesAsync(_orgId);
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }
    }
}