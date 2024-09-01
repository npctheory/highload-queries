using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Application.Users.Queries.GetUser;
using Application.Users.Queries.SearchUsers;
using MediatR;
using Domain.Entities;
using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Users.DTO;
using System.Text.Json;
using Application.Users.Queries.Login;

namespace Api.Controllers
{
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ISender _mediator;
        private readonly IMapper _mapper;

        public UserController(ISender mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        [AllowAnonymous]
        [HttpGet("user/get/{id}")]
        public async Task<IActionResult> GetUserByIdAsync([FromRoute] string id)
        {
            UserDTO user = await _mediator.Send(new GetUserQuery(id));
            return Ok(user);
        }

        [AllowAnonymous]
        [HttpGet("user/search")]
        public async Task<IActionResult> SearchUsersAsync([FromQuery] string first_name, [FromQuery] string second_name)
        {
            List<UserDTO> users = await _mediator.Send(new SearchUsersQuery(first_name, second_name));
            return Ok(users);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] JsonElement jsonElement)
        {
            string id = jsonElement.GetProperty("id").GetString();
            string password = jsonElement.GetProperty("password").GetString();

            TokenDTO token = await _mediator.Send(new LoginQuery(id,password)); 
            return Ok(token);
        }
    }
}