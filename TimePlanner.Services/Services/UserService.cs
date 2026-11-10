using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using TimePlanner.Domain.Entities;
using TimePlanner.Repositories.Interfaces;
using TimePlanner.Services.DTOs;
using TimePlanner.Services.Interfaces;
using TimePlanner.Services.Validators;

namespace TimePlanner.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ITokenService _tokenService;


        public UserService(IUserRepository userRepository, IMapper mapper, IPasswordHasher<User> passwordHasher,
            ITokenService tokenService)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
        }

        public async Task<UserResponse> Register(UserRegisterRequest request)
        {
            UserValidator.ValidateAndNormalize(request);

            if (await _userRepository.GetByUsername(request.Username) is not null)
            {
                throw new InvalidOperationException("Username is already taken");
            }

            if (await _userRepository.GetByEmail(request.Email) is not null)
            {
                throw new InvalidOperationException("Email is already taken");
            }

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                TimeZoneId = request.TimeZoneId,
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            var createdUser = await _userRepository.Add(user);

            var response = _mapper.Map<UserResponse>(createdUser);
            response.Token = _tokenService.GenerateToken(createdUser);

            return response;
        }

        public async Task<UserResponse> Login(UserLoginRequest request)
        {
            UserValidator.ValidateAndNormalize(request);
            var user = await _userRepository.GetByUsername(request.Username);

            if (user is null)
            {
                throw new UnauthorizedAccessException("Invalid username or password");
            }

            var passwordResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

            if (passwordResult == PasswordVerificationResult.Failed)
            {
                throw new UnauthorizedAccessException("Invalid username or password");
            }

            var response = _mapper.Map<UserResponse>(user);
            response.Token = _tokenService.GenerateToken(user);

            return response;
        }
    }
}