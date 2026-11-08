using Microsoft.AspNetCore.Mvc;
using TimePlanner.Services.DTOs;
using TimePlanner.Services.Interfaces;

namespace TimePlanner.API.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserResponse>> Register(UserRegisterRequest request)
    {
        var response = await _userService.Register(request);
        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserResponse>> Login(UserLoginRequest request)
    {
        var response = await _userService.Login(request);
        return Ok(response);
    }
}