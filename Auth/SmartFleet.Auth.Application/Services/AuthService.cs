using SmartFleet.Auth.Application.DTOs;
using SmartFleet.Auth.Application.Interfaces;
using SmartFleet.Auth.Domain.Entities;

namespace SmartFleet.Auth.Application.Services
{
    // Application/Services/AuthService.cs
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher; // We'll need this for security
        private readonly ITokenService _tokenService;     // We'll need this for JWTs

        public AuthService(
            IUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher,
            ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
        }

        public async Task<AuthResponse> SignupAsync(SignupRequest request)
        {
            // 1. Validation
            var existingUser = await _unitOfWork.Users.GetByEmailAsync(request.Email);
            if (existingUser != null)
                throw new Exception("User already exists"); // Or a custom DomainException

            // 2. Create User
            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = _passwordHasher.HashPassword(request.Password),
                Role = "User"
            };

            await _unitOfWork.Users.AddAsync(user);

            // 3. Auto-Login Logic
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshTokenString = _tokenService.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = refreshTokenString,
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(7)
            };

            await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);

            // 4. Atomic Save
            await _unitOfWork.SaveChangesAsync();

            // Return the record using the positional constructor
            return new AuthResponse(
                accessToken,
                refreshTokenString,
                DateTime.UtcNow.AddMinutes(15)
            );
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);

            if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            // Generate Access Token
            var accessToken = _tokenService.GenerateAccessToken(user);
            // Generate Refresh Token string
            var refreshTokenString = _tokenService.GenerateRefreshToken();

            // 1. Create the RefreshToken Entity for the DB
            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = refreshTokenString,
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(7), // Long-lived
                IsRevoked = false
            };

            // 2. Persist
            await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
            await _unitOfWork.SaveChangesAsync();

            // 3. Return your record with the Access Token Expiry (e.g., 15 mins from now)
            return new AuthResponse(
                accessToken,
                refreshTokenString,
                DateTime.UtcNow.AddMinutes(15)
            );
        }

        public async Task<AuthResponse> RefreshAsync(RefreshTokenRequest request)
        {
            // 1. Fetch Token
            var storedToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(request.RefreshToken);

            // 2. Validate using your Domain Entity logic
            if (storedToken == null || !storedToken.IsActive)
                throw new Exception("Invalid or expired refresh token");

            // 3. Get User
            var user = await _unitOfWork.Users.GetByIdAsync(storedToken.UserId);
            if (user == null) throw new Exception("User not found");

            // 4. Token Rotation (Security Best Practice)
            storedToken.IsRevoked = true;
            _unitOfWork.RefreshTokens.Update(storedToken);

            var newAccessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshTokenString = _tokenService.GenerateRefreshToken();

            var newRefreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = newRefreshTokenString,
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(7)
            };

            await _unitOfWork.RefreshTokens.AddAsync(newRefreshTokenEntity);
            await _unitOfWork.SaveChangesAsync();

            return new AuthResponse(
                newAccessToken,
                newRefreshTokenString,
                DateTime.UtcNow.AddMinutes(15)
            );
        }
    }
}
