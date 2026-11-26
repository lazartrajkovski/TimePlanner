using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Moq;
using TimePlanner.Domain.Entities;
using TimePlanner.Repositories.Interfaces;
using TimePlanner.Services.DTOs.Users;
using TimePlanner.Services.Exceptions;
using TimePlanner.Services.Interfaces;
using TimePlanner.Services.Services;

namespace TimePlanner.Tests.Services.Users;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IPasswordHasher<User>> _passwordHasherMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _mapperMock = new Mock<IMapper>();
        _passwordHasherMock = new Mock<IPasswordHasher<User>>();
        _tokenServiceMock = new Mock<ITokenService>();

        _userService = new UserService(
            _userRepositoryMock.Object,
            _mapperMock.Object,
            _passwordHasherMock.Object,
            _tokenServiceMock.Object);
    }

    [Fact]
    public async Task Register_ValidRequest_ReturnsUserWithToken()
    {
        var request = new UserRegisterRequest
        {
            Username = " TestUser ",
            Email = " TEST@EXAMPLE.COM ",
            Password = "VerySecurePassword123!",
            FirstName = " Test ",
            LastName = " User ",
            TimeZoneId = "Europe/Skopje",
        };

        var createdUser = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashed-password",
            FirstName = "Test",
            LastName = "User",
            TimeZoneId = "Europe/Skopje",
        };

        var mappedResponse = new UserResponse
        {
            Username = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
        };

        _userRepositoryMock
            .Setup(repository => repository.GetByUsername("testuser"))
            .ReturnsAsync((User?)null);

        _userRepositoryMock
            .Setup(repository => repository.GetByEmail("test@example.com"))
            .ReturnsAsync((User?)null);

        _passwordHasherMock
            .Setup(hasher => hasher.HashPassword(
                It.IsAny<User>(),
                request.Password))
            .Returns("hashed-password");

        _userRepositoryMock
            .Setup(repository => repository.Add(It.IsAny<User>()))
            .ReturnsAsync(createdUser);

        _mapperMock
            .Setup(mapper => mapper.Map<UserResponse>(createdUser))
            .Returns(mappedResponse);

        _tokenServiceMock
            .Setup(tokenService => tokenService.GenerateToken(createdUser))
            .Returns("test-token");

        var result = await _userService.Register(request);

        Assert.Equal("testuser", request.Username);
        Assert.Equal("test@example.com", request.Email);
        Assert.Equal("Test", request.FirstName);
        Assert.Equal("User", request.LastName);

        Assert.Equal("testuser", result.Username);
        Assert.Equal("test@example.com", result.Email);
        Assert.Equal("Test", result.FirstName);
        Assert.Equal("User", result.LastName);
        Assert.Equal("test-token", result.Token);

        _passwordHasherMock.Verify(
            hasher => hasher.HashPassword(
                It.Is<User>(user =>
                    user.Username == "testuser" &&
                    user.Email == "test@example.com" &&
                    user.FirstName == "Test" &&
                    user.LastName == "User" &&
                    user.TimeZoneId == "Europe/Skopje"),
                request.Password),
            Times.Once);

        _userRepositoryMock.Verify(
            repository => repository.Add(
                It.Is<User>(user =>
                    user.Username == "testuser" &&
                    user.Email == "test@example.com" &&
                    user.PasswordHash == "hashed-password" &&
                    user.FirstName == "Test" &&
                    user.LastName == "User" &&
                    user.TimeZoneId == "Europe/Skopje")),
            Times.Once);

        _tokenServiceMock.Verify(
            tokenService => tokenService.GenerateToken(createdUser),
            Times.Once);
    }

    [Fact]
    public async Task Register_UsernameAlreadyTaken_ThrowsBeforeCheckingEmail()
    {
        var request = new UserRegisterRequest
        {
            Username = " TestUser ",
            Email = "test@example.com",
            Password = "VerySecurePassword123!",
            FirstName = "Test",
            LastName = "User",
            TimeZoneId = "Europe/Skopje",
        };

        _userRepositoryMock
            .Setup(repository => repository.GetByUsername("testuser"))
            .ReturnsAsync(new User
            {
                Id = 1,
                Username = "testuser",
            });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _userService.Register(request));

        Assert.Equal("Username is already taken", exception.Message);

        _userRepositoryMock.Verify(
            repository => repository.GetByUsername("testuser"),
            Times.Once);

        _userRepositoryMock.Verify(
            repository => repository.GetByEmail(It.IsAny<string>()),
            Times.Never);

        _passwordHasherMock.Verify(
            hasher => hasher.HashPassword(
                It.IsAny<User>(),
                It.IsAny<string>()),
            Times.Never);

        _userRepositoryMock.Verify(
            repository => repository.Add(It.IsAny<User>()),
            Times.Never);

        _tokenServiceMock.Verify(
            tokenService => tokenService.GenerateToken(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task Register_EmailAlreadyTaken_ThrowsBeforeHashingPassword()
    {
        var request = new UserRegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "VerySecurePassword123!",
            FirstName = "Test",
            LastName = "User",
            TimeZoneId = "Europe/Skopje",
        };

        _userRepositoryMock
            .Setup(repository => repository.GetByUsername("testuser"))
            .ReturnsAsync((User?)null);

        _userRepositoryMock
            .Setup(repository => repository.GetByEmail("test@example.com"))
            .ReturnsAsync(new User
            {
                Id = 1,
                Email = "test@example.com",
            });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _userService.Register(request));

        Assert.Equal("Email is already taken", exception.Message);

        _userRepositoryMock.Verify(
            repository => repository.GetByUsername("testuser"),
            Times.Once);

        _userRepositoryMock.Verify(
            repository => repository.GetByEmail("test@example.com"),
            Times.Once);

        _passwordHasherMock.Verify(
            hasher => hasher.HashPassword(
                It.IsAny<User>(),
                It.IsAny<string>()),
            Times.Never);

        _userRepositoryMock.Verify(
            repository => repository.Add(It.IsAny<User>()),
            Times.Never);

        _tokenServiceMock.Verify(
            tokenService => tokenService.GenerateToken(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsUserWithToken()
    {
        var request = new UserLoginRequest
        {
            Username = "TestUser ",
            Password = "VerySecurePassword123!",
        };

        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashed-password",
            FirstName = "Test",
            LastName = "User",
            TimeZoneId = "Europe/Skopje",
        };

        var mappedResponse = new UserResponse
        {
            Username = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
        };

        _userRepositoryMock
            .Setup(repository => repository.GetByUsername("testuser"))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(hasher => hasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password))
            .Returns(PasswordVerificationResult.Success);

        _mapperMock
            .Setup(mapper => mapper.Map<UserResponse>(user))
            .Returns(mappedResponse);

        _tokenServiceMock
            .Setup(tokenService => tokenService.GenerateToken(user))
            .Returns("test-token");

        var result = await _userService.Login(request);

        Assert.Equal("test-token", result.Token);

        _userRepositoryMock.Verify(
            repository => repository.GetByUsername("testuser"),
            Times.Once);

        _passwordHasherMock.Verify(
            hasher => hasher.VerifyHashedPassword(
                user,
                "hashed-password",
                request.Password),
            Times.Once);

        _mapperMock.Verify(
            mapper => mapper.Map<UserResponse>(user),
            Times.Once);

        _tokenServiceMock.Verify(
            tokenService => tokenService.GenerateToken(user),
            Times.Once);
    }

    [Fact]
    public async Task Login_PasswordNeedsRehash_ReturnsUserWithToken()
    {
        var request = new UserLoginRequest
        {
            Username = "testuser",
            Password = "VerySecurePassword123!",
        };

        var user = new User
        {
            Id = 1,
            Username = "testuser",
            PasswordHash = "old-format-hash",
        };

        var mappedResponse = new UserResponse
        {
            Username = "testuser"
        };

        _userRepositoryMock
            .Setup(repository => repository.GetByUsername("testuser"))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(hasher => hasher.VerifyHashedPassword(
                user,
                "old-format-hash",
                request.Password))
            .Returns(PasswordVerificationResult.SuccessRehashNeeded);

        _mapperMock
            .Setup(mapper => mapper.Map<UserResponse>(user))
            .Returns(mappedResponse);

        _tokenServiceMock
            .Setup(tokenService => tokenService.GenerateToken(user))
            .Returns("test-token");

        var result = await _userService.Login(request);

        Assert.Equal("test-token", result.Token);

        _mapperMock.Verify(
            mapper => mapper.Map<UserResponse>(user),
            Times.Once);

        _tokenServiceMock.Verify(
            tokenService => tokenService.GenerateToken(user),
            Times.Once);
    }

    [Fact]
    public async Task Login_UserNotFound_ThrowsBeforeVerifyingPassword()
    {
        var request = new UserLoginRequest
        {
            Username = "testuser",
            Password = "VerySecurePassword123!",
        };

        _userRepositoryMock
            .Setup(repository => repository.GetByUsername("testuser"))
            .ReturnsAsync((User?)null);

        var exception = await Assert.ThrowsAsync<UnauthorizedException>(
            () => _userService.Login(request));

        Assert.Equal("Invalid username or password", exception.Message);

        _userRepositoryMock.Verify(
            repository => repository.GetByUsername("testuser"),
            Times.Once);

        _passwordHasherMock.Verify(
            hasher => hasher.VerifyHashedPassword(
                It.IsAny<User>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);

        _mapperMock.Verify(
            mapper => mapper.Map<UserResponse>(It.IsAny<User>()),
            Times.Never);

        _tokenServiceMock.Verify(
            tokenService => tokenService.GenerateToken(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task Login_InvalidPassword_ThrowsBeforeGeneratingToken()
    {
        var request = new UserLoginRequest
        {
            Username = "testuser",
            Password = "WrongPassword123!",
        };

        var user = new User
        {
            Id = 1,
            Username = "testuser",
            PasswordHash = "hashed-password",
        };

        _userRepositoryMock
            .Setup(repository => repository.GetByUsername("testuser"))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(hasher => hasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password))
            .Returns(PasswordVerificationResult.Failed);

        var exception = await Assert.ThrowsAsync<UnauthorizedException>(
            () => _userService.Login(request));

        Assert.Equal("Invalid username or password", exception.Message);

        _passwordHasherMock.Verify(
            hasher => hasher.VerifyHashedPassword(
                user,
                "hashed-password",
                request.Password),
            Times.Once);

        _mapperMock.Verify(
            mapper => mapper.Map<UserResponse>(It.IsAny<User>()),
            Times.Never);

        _tokenServiceMock.Verify(
            tokenService => tokenService.GenerateToken(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task Register_InvalidRequest_ThrowsBeforeAccessingRepository()
    {
        var request = new UserRegisterRequest
        {
            Username = " ",
            Email = "test@example.com",
            Password = "VerySecurePassword123!",
            FirstName = "Test",
            LastName = "User",
            TimeZoneId = "Europe/Skopje",
        };

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _userService.Register(request));

        Assert.Equal("Username is required", exception.Message);

        _userRepositoryMock.Verify(
            repository => repository.GetByUsername(It.IsAny<string>()),
            Times.Never);

        _userRepositoryMock.Verify(
            repository => repository.GetByEmail(It.IsAny<string>()),
            Times.Never);

        _passwordHasherMock.Verify(
            hasher => hasher.HashPassword(
                It.IsAny<User>(),
                It.IsAny<string>()),
            Times.Never);

        _userRepositoryMock.Verify(
            repository => repository.Add(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task Login_InvalidRequest_ThrowsBeforeAccessingRepository()
    {
        var request = new UserLoginRequest
        {
            Username = " ",
            Password = "VerySecurePassword123!",
        };

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _userService.Login(request));

        Assert.Equal("Username is required", exception.Message);

        _userRepositoryMock.Verify(
            repository => repository.GetByUsername(It.IsAny<string>()),
            Times.Never);

        _passwordHasherMock.Verify(
            hasher => hasher.VerifyHashedPassword(
                It.IsAny<User>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);

        _mapperMock.Verify(
            mapper => mapper.Map<UserResponse>(It.IsAny<User>()),
            Times.Never);

        _tokenServiceMock.Verify(
            tokenService => tokenService.GenerateToken(It.IsAny<User>()),
            Times.Never);
    }
}