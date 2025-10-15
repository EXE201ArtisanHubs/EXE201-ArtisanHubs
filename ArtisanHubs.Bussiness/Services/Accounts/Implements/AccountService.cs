using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ArtisanHubs.API.DTOs.Common;
using ArtisanHubs.Bussiness.Services.Accounts.Interfaces;
using ArtisanHubs.Bussiness.Services.Shared;
using ArtisanHubs.Bussiness.Services.Tokens;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Repositories.Accounts.Implements;
using ArtisanHubs.Data.Repositories.Accounts.Interfaces;
using ArtisanHubs.DTOs.DTO.Reponse.Accounts;
using ArtisanHubs.DTOs.DTO.Request.Accounts;
using ArtisanHubs.DTOs.DTOs.Reponse;
using ArtisanHubs.DTOs.DTOs.Request.Accounts;
using AutoMapper;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace ArtisanHubs.Bussiness.Services.Accounts.Implements
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _repo;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher<Account> _passwordHasher;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public AccountService(IAccountRepository repo, IMapper mapper, ITokenService tokenService, IPasswordHasher<Account> passwordHasher, IEmailService emailService, IConfiguration configuration)
        {
            _repo = repo;
            _mapper = mapper;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task<ApiResponse<LoginResponse?>> LoginAsync(LoginRequest request)
        {
            try
            {
                var account = await _repo.GetByEmailAsync(request.Email);
                if (account == null)
                {
                    return ApiResponse<LoginResponse?>.FailResponse("Invalid email or password.", 401);
                }

                // Verify password
                var result = _passwordHasher.VerifyHashedPassword(account, account.PasswordHash, request.Password);
                if (result == PasswordVerificationResult.Failed)
                {
                    return ApiResponse<LoginResponse?>.FailResponse("Invalid email or password.", 401);
                }

                var token = _tokenService.CreateToken(account);

                var response = new LoginResponse
                {
                    AccountId = account.AccountId,
                    Email = account.Email,
                    Role = account.Role,
                    Token = token
                };

                return ApiResponse<LoginResponse?>.SuccessResponse(response, "Login successful.");
            }
            catch (Exception ex)
            {
                return ApiResponse<LoginResponse?>.FailResponse($"Error: {ex.Message}", 500);
            }
        }

        // Lấy tất cả Account
        public async Task<ApiResponse<IEnumerable<AccountResponse>>> GetAllAccountAsync()
        {
            try
            {
                var accounts = await _repo.GetAllAsync();
                var response = _mapper.Map<IEnumerable<AccountResponse>>(accounts);

                return ApiResponse<IEnumerable<AccountResponse>>.SuccessResponse(response, "Get all accounts successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<AccountResponse>>.FailResponse($"Error: {ex.Message}", 500);
            }
        }

        // Lấy Account theo Id
        public async Task<ApiResponse<AccountResponse?>> GetByIdAsync(int id)
        {
            try
            {
                var account = await _repo.GetByIdAsync(id);
                if (account == null)
                    return ApiResponse<AccountResponse?>.FailResponse("Account not found", 404);

                var response = _mapper.Map<AccountResponse>(account);
                return ApiResponse<AccountResponse?>.SuccessResponse(response, "Get account successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<AccountResponse?>.FailResponse($"Error: {ex.Message}", 500);
            }
        }

        // Tạo mới Account
        public async Task<ApiResponse<AccountResponse>> CreateAsync(AccountRequest request)
        {
            try
            {
                var existing = await _repo.GetByEmailAsync(request.Email);
                if (existing != null)
                {
                    return ApiResponse<AccountResponse>.FailResponse("Email already in use", 409); // 409 Conflict
                }

                var entity = _mapper.Map<Account>(request);
                entity.CreatedAt = DateTime.UtcNow;
                entity.Status = "Active";

                // THÊM DÒNG NÀY ĐỂ GÁN ROLE MẶC ĐỊNH
                entity.Role = "Customer";

                // Hash password trước khi lưu
                entity.PasswordHash = _passwordHasher.HashPassword(entity, request.Password);

                await _repo.CreateAsync(entity);

                var response = _mapper.Map<AccountResponse>(entity);
                return ApiResponse<AccountResponse>.SuccessResponse(response, "Create account successfully", 201);
            }
            catch (Exception ex)
            {
                return ApiResponse<AccountResponse>.FailResponse($"Error: {ex.Message}", 500);
            }
        }

        // Cập nhật Account
        public async Task<ApiResponse<AccountResponse?>> UpdateAsync(int id, AccountRequest request)
        {
            try
            {
                var existing = await _repo.GetByIdAsync(id);
                if (existing == null) return ApiResponse<AccountResponse?>.FailResponse("Account not found", 404);

                _mapper.Map(request, existing);
                existing.UpdatedAt = DateTime.UtcNow;

                await _repo.UpdateAsync(existing);

                var response = _mapper.Map<AccountResponse>(existing);
                return ApiResponse<AccountResponse?>.SuccessResponse(response, "Update account successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<AccountResponse?>.FailResponse($"Error: {ex.Message}", 500);
            }
        }

        // Xoá Account
        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            try
            {
                var account = await _repo.GetByIdAsync(id);
                if (account == null) return ApiResponse<bool>.FailResponse("Account not found", 404);

                await _repo.RemoveAsync(account);
                return ApiResponse<bool>.SuccessResponse(true, "Delete account successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.FailResponse($"Error: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponse<object>> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            var account = await _repo.GetByConditionAsync(a => a.Email == request.Email);

            if (account != null)
            {
                // 1. Tạo token ngẫu nhiên, an toàn
                var tokenBytes = RandomNumberGenerator.GetBytes(64);
                var resetToken = Convert.ToBase64String(tokenBytes)
                                        .Replace("/", "-")
                                        .Replace("+", "_")
                                        .Replace("=", ""); // URL safe token

                // 2. Đặt thời gian hết hạn (ví dụ: 15 phút)
                account.PasswordResetToken = resetToken;
                account.ResetTokenExpires = DateTime.UtcNow.AddMinutes(15);

                await _repo.UpdateAsync(account);

                // 3. Gửi email
                // Quan trọng: URL này phải trỏ đến trang Reset Password trên Frontend của bạn
                // Thay 3000 bằng cổng thực tế của frontend bạn
                var resetLink = $"http://localhost:3000/reset-password?token={resetToken}";
                await _emailService.SendPasswordResetEmailAsync(account.Email, resetLink);
            }

            return ApiResponse<object>.SuccessResponse(null, "If an account with that email exists, a password reset link has been sent.");
        }

        public async Task<ApiResponse<object>> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var account = await _repo.GetByConditionAsync(a => a.PasswordResetToken == request.Token);

            if (account == null || account.ResetTokenExpires < DateTime.UtcNow)
            {
                return ApiResponse<object>.FailResponse("Invalid or expired password reset token.", 400);
            }

            // --- SỬA LẠI PHẦN NÀY ---

            // Dùng _passwordHasher để hash mật khẩu mới, giống hệt như lúc đăng ký
            account.PasswordHash = _passwordHasher.HashPassword(account, request.NewPassword);

            // Vô hiệu hóa token
            account.PasswordResetToken = null;
            account.ResetTokenExpires = null;

            _repo.UpdateAsync(account);

            return ApiResponse<object>.SuccessResponse(null, "Password has been reset successfully.");
        }

        public async Task<ApiResponse<AccountResponse?>> GetMyAccountAsync(int accountId)
        {
            try
            {
                // Dùng accountId (lấy từ token) để tìm tài khoản
                var account = await _repo.GetByIdAsync(accountId);

                if (account == null)
                {
                    // Trường hợp này rất hiếm khi xảy ra nếu token hợp lệ
                    return ApiResponse<AccountResponse?>.FailResponse("Account not found.", 404);
                }

                // Map entity sang DTO để trả về cho client
                var response = _mapper.Map<AccountResponse>(account);
                return ApiResponse<AccountResponse?>.SuccessResponse(response, "Get current user info successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<AccountResponse?>.FailResponse($"An error occurred: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponse<LoginResponse>> LoginWithGoogleAsync(GoogleLoginRequest request)
        {
            try
            {
                var googleClientId = _configuration["Google:ClientId"];
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string> { googleClientId }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
                var user = await _repo.GetByEmailAsync(payload.Email);

                if (user == null)
                {
                    user = new Account
                    {
                        Email = payload.Email,
                        Username = payload.Name,
                        Avatar = payload.Picture,
                        Role = "Customer",
                        Status = "active",
                        PasswordHash = "" // Không cần mật khẩu
                    };
                    await _repo.CreateAsync(user);
                }

                var token = _tokenService.GenerateJwtToken(user);
                var loginResponse = new LoginResponse { Token = token, Username = user.Username, Role = user.Role };

                return ApiResponse<LoginResponse>.SuccessResponse(loginResponse, "Login successfully.");
            }
            catch (InvalidJwtException)
            {
                return ApiResponse<LoginResponse>.FailResponse("Invalid Google token.", 401);
            }
            catch (Exception ex)
            {
                return ApiResponse<LoginResponse>.FailResponse($"An unexpected error occurred: {ex.Message}", 500);
            }
        }
    }
}