using CodeChallenge.Api.Logic;
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
    private readonly IMessageLogic _logic;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(IMessageLogic logic, ILogger<MessagesController> logger)
    {
        _logic = logic;
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
            result = await _logic.GetAllMessagesAsync(organizationId);
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
            result = await _logic.GetMessageAsync(organizationId, id);
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
    public async Task<ActionResult<Result>> Create(Guid organizationId, [FromBody] CreateMessageRequest request)
    {
        // TODO: Implement
        var response = new ResponseViewModel<Result>();
        Result result = null;
        try
        {
            _logger.LogInformation("MessagesController : Create : Start.");
            result = await _logic.CreateMessageAsync(organizationId, request);
            switch (result)
            {
                case Created<Message>:
                    _logger.LogInformation("MessagesController : Create : Message Created successfully.");
                    return Ok(Utils.ResponseMapping((int)HttpStatusCode.OK, "Message Created successfully.", result, null));

                case Conflict cf:
                    _logger.LogWarning($"MessagesController : Create : Conflict. {cf.Message}");
                    return Conflict(Utils.ResponseMapping((int)HttpStatusCode.Conflict, cf.Message, result, null));

                case ValidationError validationError:
                    _logger.LogWarning($"MessagesController : Create : ValidationError");
                    return BadRequest(Utils.ResponseMapping((int)HttpStatusCode.BadRequest, "Create Failed", result, null));

                default:
                    return BadRequest(Utils.ResponseMapping((int)HttpStatusCode.BadRequest, "Create failed.", result, null));
            }
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
        var response = new ResponseViewModel<Result>();
        Result result = null;
        try
        {
            _logger.LogInformation("MessagesController : Update : Start.");
            result = await _logic.UpdateMessageAsync(organizationId, id, request);
            switch (result)
            {
                case Updated:
                    _logger.LogInformation("MessagesController : Update : Message Updated successfully.");
                    return Ok(Utils.ResponseMapping((int)HttpStatusCode.OK, "Message Updated successfully.", result, null));

                case NotFound nf:
                    _logger.LogWarning($"MessagesController : Update : Not found. {nf.Message}");
                    return NotFound(Utils.ResponseMapping((int)HttpStatusCode.NotFound, nf.Message, result, null));

                case Conflict cf:
                    _logger.LogWarning($"MessagesController : Update : Conflict. {cf.Message}");
                    return Conflict(Utils.ResponseMapping((int)HttpStatusCode.Conflict, cf.Message, result, null));

                case ValidationError validationError:
                    _logger.LogWarning($"MessagesController : Update : ValidationError");
                    return BadRequest(Utils.ResponseMapping((int)HttpStatusCode.BadRequest, "Update Failed", result, null));

                default:
                    return BadRequest(Utils.ResponseMapping((int)HttpStatusCode.BadRequest, "Update failed.", result, null));
            }
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
        var response = new ResponseViewModel<Result>();
        Result result = null;
        try
        {
            _logger.LogInformation("MessagesController : Delete : Start.");
            result = await _logic.DeleteMessageAsync(organizationId, id);
            switch (result)
            {
                case Deleted:
                    _logger.LogInformation("MessagesController : Delete : Deleted successfully.");
                    return Ok(Utils.ResponseMapping((int)HttpStatusCode.OK, "Message deleted successfully.", result, null));

                case NotFound nf:
                    _logger.LogWarning($"MessagesController : Delete : Not found. {nf.Message}");
                    return NotFound(Utils.ResponseMapping((int)HttpStatusCode.NotFound, nf.Message, result, null ));

                case Conflict cf:
                    _logger.LogWarning($"MessagesController : Delete : Conflict. {cf.Message}");
                    return Conflict(Utils.ResponseMapping((int)HttpStatusCode.Conflict, cf.Message, result, null));

                default:
                    return BadRequest(Utils.ResponseMapping((int)HttpStatusCode.BadRequest, "Delete failed.", result, null));
            }
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
