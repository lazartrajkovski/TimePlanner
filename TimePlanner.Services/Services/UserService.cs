using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using TimePlanner.Domain.Entities;
using TimePlanner.Repositories.Interfaces;
using TimePlanner.Services.DTOs;
using TimePlanner.Services.Interfaces;

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
            var username = request.Username.Trim().ToLowerInvariant();
            var email = request.Email.Trim().ToLowerInvariant();
            var timeZoneId = request.TimeZoneId.Trim();

            if (!TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out _))
            {
                throw new ArgumentException("Invalid time zone");
            }

            if (await _userRepository.GetByUsername(username) is not null)
            {
                throw new InvalidOperationException("Username is already taken");
            }

            if (await _userRepository.GetByEmail(email) is not null)
            {
                throw new InvalidOperationException("Email is already taken");
            }

            var user = new User
            {
                Username = username,
                Email = email,
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                TimeZoneId = timeZoneId,
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            var createdUser = await _userRepository.Add(user);

            var response = _mapper.Map<UserResponse>(createdUser);
            response.Token = _tokenService.GenerateToken(createdUser);

            return response;
        }

        public async Task<UserResponse> Login(UserLoginRequest request)
        {
            var username = request.Username.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByUsername(username);

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