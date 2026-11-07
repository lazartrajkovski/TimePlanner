using TimePlanner.Services.DTOs;

namespace TimePlanner.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserResponse> Register(UserRegisterRequest request);
        Task<UserResponse> Login(UserLoginRequest request);
    }
}