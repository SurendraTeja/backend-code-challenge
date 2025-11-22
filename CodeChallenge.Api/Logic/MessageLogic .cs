using CodeChallenge.Api.Models;
using CodeChallenge.Api.Repositories;

namespace CodeChallenge.Api.Logic
{
    public class MessageLogic : IMessageLogic
    {
        private readonly IMessageRepository _repository;

        public MessageLogic(IMessageRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Message>> GetAllMessagesAsync(Guid organizationId)
        {
            return await this._repository.GetAllByOrganizationAsync(organizationId);
        }

        public async Task<Message?> GetMessageAsync(Guid organizationId, Guid id)
        {
            return await this._repository.GetByIdAsync(organizationId, id);
        }

        public async Task<Result> CreateMessageAsync(Guid organizationId, CreateMessageRequest request)
        {
            // Basic validations
            var errors = Validate(true, request, null);
            if (errors.Count > 0)
            {
                return new ValidationError(errors);
            }

            // Rule: Unique title
            var existing = await _repository.GetByTitleAsync(organizationId, request.Title);
            if (existing != null)
            {
                return new Conflict("A message with the same title already exists.");
            }

            var message = new Message
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                Title = request.Title,
                Content = request.Content,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.CreateAsync(message);

            return new Created<Message>(message);
        }

        public async Task<Result> UpdateMessageAsync(Guid organizationId, Guid id, UpdateMessageRequest request)
        {
            var existing = await _repository.GetByIdAsync(organizationId, id);
            if (existing == null)
            {
                return new NotFound("Message not found.");
            }

            if (!existing.IsActive)
            {
                return new Conflict("Cannot update an inactive message.");
            }

            // Validate data
            var errors = Validate(false, null, request);
            if (errors.Count > 0)
            {
                return new ValidationError(errors);
            }

            // Rule: Unique title
            if (existing.Title != request.Title)
            {
                var other = await _repository.GetByTitleAsync(organizationId, request.Title);
                if (other != null)
                {
                    return new Conflict("A message with this title already exists.");
                }
            }

            // Apply updates
            existing.Title = request.Title;
            existing.Content = request.Content;
            existing.IsActive = request.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(existing);

            return new Updated();
        }

        public async Task<Result> DeleteMessageAsync(Guid organizationId, Guid id)
        {
            var existing = await _repository.GetByIdAsync(organizationId, id);
            if (existing == null)
            {
                return new NotFound("Message not found.");
            }

            if (!existing.IsActive)
            {
                return new Conflict("Cannot delete an inactive message.");
            }

            var deleted = await _repository.DeleteAsync(organizationId, id);
            if (!deleted)
            {
                return new NotFound("Message not found.");
            }

            return new Deleted();
        }

        private Dictionary<string, string[]> Validate(bool isCreate, CreateMessageRequest? request, UpdateMessageRequest? updateMessageRequest)
        {
            var errors = new Dictionary<string, string[]>();
            if (isCreate && request != null)
            {
                if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length < 3 || request.Title.Length > 200)
                {
                    errors.Add("Title", new[]
                    {
                        "Title is required and must be between 3 and 200 characters."
                    });
                }

                if (string.IsNullOrWhiteSpace(request.Content) || request.Content.Length < 10 || request.Content.Length > 1000)
                {
                    errors.Add("Content", new[]
                    {
                        "Content must be between 10 and 1000 characters."
                    });
                }
            }
            else if(!isCreate && updateMessageRequest != null)
            {
                if (string.IsNullOrWhiteSpace(updateMessageRequest.Title) || updateMessageRequest.Title.Length < 3 || updateMessageRequest.Title.Length > 200)
                {
                    errors.Add("Title", new[]
                    {
                        "Title is required and must be between 3 and 200 characters."
                    });
                }

                if (string.IsNullOrWhiteSpace(updateMessageRequest.Content) || updateMessageRequest.Content.Length < 10 || updateMessageRequest.Content.Length > 1000)
                {
                    errors.Add("Content", new[]
                    {
                        "Content must be between 10 and 1000 characters."
                    });
                }
            }

            return errors;
        }
    }
}
