using CodeChallenge.Api.Models;
using CodeChallenge.Api.Repositories;
using GlobalShared.CommonUtils.Library;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CodeChallenge.Api.Controllers;

[ApiController]
[Route("api/v1/organizations/{organizationId}/messages")]
public class MessagesController : ControllerBase
{
    private readonly IMessageRepository _repository;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(IMessageRepository repository, ILogger<MessagesController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Message>>> GetAll(Guid organizationId)
    {
        // TODO: Implement
        var response = new ResponseViewModel<IEnumerable<Message>>();
        IEnumerable<Message> result = null;
        try
        {
            _logger.LogInformation("MessagesController : GetAll : Start.");
            result = await _repository.GetAllByOrganizationAsync(organizationId);
            if (result != null && result.Count() > 0)
            {
                _logger.LogInformation("MessagesController : GetAll : All Organization Message Fetched Successfully.");
                response = Utils.ResponseMapping((int)HttpStatusCode.OK, $"All Organizations Message Fetched Successfully.", result, null);
                return this.Ok(response);
            }

            response = Utils.ResponseMapping((int)HttpStatusCode.NotFound, $"Organizations Message Not Found.", result ?? null, null);
            return this.StatusCode((int)HttpStatusCode.NotFound, response);
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"MessagesController : GetAll : Exception.{ex.ToString()}");
            return this.ExceptionResult(ex, response, "GetAll");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Message>> GetById(Guid organizationId, Guid id)
    {
        // TODO: Implement
        var response = new ResponseViewModel<Message>();
        Message result = null;
        try
        {
            _logger.LogInformation("MessagesController : GetById : Start.");
            result = await _repository.GetByIdAsync(organizationId, id);
            if (result != null)
            {
                _logger.LogInformation("MessagesController : GetById : Message Fetched Successfully.");
                response = Utils.ResponseMapping((int)HttpStatusCode.OK, $"Message Fetched Successfully.", result, null);
                return this.Ok(response);
            }

            response = Utils.ResponseMapping((int)HttpStatusCode.NotFound, $"Organizations Not Found.", result ?? null, null);
            return this.StatusCode((int)HttpStatusCode.NotFound, response);
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"MessagesController : GetById : Exception.{ex.ToString()}");
            return this.ExceptionResult(ex, response, "GetById");
        }
    }

    [HttpPost]
    public async Task<ActionResult<Message>> Create(Guid organizationId, [FromBody] CreateMessageRequest request)
    {
        // TODO: Implement
        var response = new ResponseViewModel<Message>();
        Message result = null;
        try
        {
            _logger.LogInformation("MessagesController : Create : Start.");
            var newMessage = new Message
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                Title = request.Title,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow
            };
            result = await _repository.CreateAsync(newMessage);
            if (result != null)
            {
                _logger.LogInformation($"MessagesController : Create : Message Created Successfully {newMessage.Id}.");
                response = Utils.ResponseMapping((int)HttpStatusCode.Created, $"Message Created Successfully.", result, null);
                return this.Ok(response);
            }

            response = Utils.ResponseMapping((int)HttpStatusCode.BadRequest, $"Message Creation Failed.", result ?? null, null);
            return this.StatusCode((int)HttpStatusCode.BadRequest, response);
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"MessagesController : Create : Exception.{ex.ToString()}");
            return this.ExceptionResult(ex, response, "Create");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid organizationId, Guid id, [FromBody] UpdateMessageRequest request)
    {
        // TODO: Implement
        var response = new ResponseViewModel<Message>();
        Message result = null;
        try
        {
            _logger.LogInformation("MessagesController : Update : Start.");
            var existing = await _repository.GetByIdAsync(organizationId, id);
            if (existing == null)
            {
                _logger.LogInformation("MessagesController : Update : Existing Message Not Found.");
                response = Utils.ResponseMapping((int)HttpStatusCode.NotFound, $"Organizations Not Found.", result ?? null, null);
                return this.StatusCode((int)HttpStatusCode.NotFound, response);
            }

            existing.Content = request.Content;
            existing.Title = request.Title;
            existing.IsActive = request.IsActive;

            result = await _repository.UpdateAsync(existing);
            if (result != null)
            {
                _logger.LogInformation($"MessagesController : Update : Updated {existing.Id}.");
                response = Utils.ResponseMapping((int)HttpStatusCode.OK, $"Message Updated Successfully.", result, null);
                return this.Ok(response);
            }

            response = Utils.ResponseMapping((int)HttpStatusCode.BadRequest, $"Message Update Failed.", result ?? null, null);
            return this.StatusCode((int)HttpStatusCode.BadRequest, response);
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"MessagesController : Update : Exception.{ex.ToString()}");
            return this.ExceptionResult(ex, response, "Update");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid organizationId, Guid id)
    {
        // TODO: Implement
        var response = new ResponseViewModel<string>();
        string result = string.Empty;
        try
        {
            _logger.LogInformation("MessagesController : Delete : Start.");
            var existing = await _repository.GetByIdAsync(organizationId, id);
            if (existing == null)
            {
                _logger.LogInformation("MessagesController : Delete : Existing Message Not Found.");
                response = Utils.ResponseMapping((int)HttpStatusCode.NotFound, $"Organizations Not Found.", result, null);
                return this.StatusCode((int)HttpStatusCode.NotFound, response);
            }

            bool isDeleted = await _repository.DeleteAsync(organizationId, id);
            if (isDeleted)
            {
                _logger.LogInformation($"MessagesController : Delete : Deleted {existing.Id}.");
                response = Utils.ResponseMapping((int)HttpStatusCode.OK, $"Message Deleted Successfully.", $"Message Deleted Successfully.", null);
                return this.Ok(response);
            }

            response = Utils.ResponseMapping((int)HttpStatusCode.BadRequest, $"Message Deletion Failed.", $"Message Deletion Failed.", null);
            return this.StatusCode((int)HttpStatusCode.BadRequest, response);
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"MessagesController : Delete : Exception.{ex.ToString()}");
            return this.ExceptionResult(ex, response, "Delete");
        }
    }

    private ObjectResult ExceptionResult<TType>(Exception ex, ResponseViewModel<TType> response, string methodName)
    where TType : class
    {
        response.Error.Add(new ResponseViewModel<TType>.Errors
        {
            Code = (int)HttpStatusCode.InternalServerError,
            Description = ex.Message == null || ex.Message == string.Empty ? "Something went wrong" : ex.Message,
            Message = ex.Message,
            More_Info = ex.StackTrace,
        });
        response = Utils.ResponseMapping((int)HttpStatusCode.InternalServerError, $"Exception occurs while processing request for {methodName}.", null, response.Error);
        return this.StatusCode((int)HttpStatusCode.InternalServerError, response);
    }
}
