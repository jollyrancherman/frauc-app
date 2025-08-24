using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using User.API.Application.Commands;
using User.API.Application.Queries;

namespace User.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<CreateUserResponse>> CreateUser(
        [FromBody] CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetUserById), new { id = response.Id }, response);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<UserResponse>> GetUserById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetUserByIdQuery(id);
        var response = await _mediator.Send(query, cancellationToken);

        if (response == null)
        {
            return NotFound();
        }

        return Ok(response);
    }

    [HttpGet("email/{email}")]
    [Authorize]
    public async Task<ActionResult<UserResponse>> GetUserByEmail(
        string email,
        CancellationToken cancellationToken)
    {
        var query = new GetUserByEmailQuery(email);
        var response = await _mediator.Send(query, cancellationToken);

        if (response == null)
        {
            return NotFound();
        }

        return Ok(response);
    }

    [HttpGet("username/{username}")]
    [Authorize]
    public async Task<ActionResult<UserResponse>> GetUserByUsername(
        string username,
        CancellationToken cancellationToken)
    {
        var query = new GetUserByUsernameQuery(username);
        var response = await _mediator.Send(query, cancellationToken);

        if (response == null)
        {
            return NotFound();
        }

        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<UpdateUserResponse>> UpdateUser(
        Guid id,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateUserCommand(id, request.FirstName, request.LastName);

        try
        {
            var response = await _mediator.Send(command, cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<ActionResult> DeactivateUser(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeactivateUserCommand(id);
        var success = await _mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }
}

public sealed record UpdateUserRequest(
    string FirstName,
    string LastName
);