using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtisanHubs.API.DTOs.Common;
using ArtisanHubs.Bussiness.Services.ArtistProfiles.Interfaces;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Paginate;
using ArtisanHubs.Data.Repositories.Accounts.Interfaces;
using ArtisanHubs.Data.Repositories.ArtistProfiles.Interfaces;
using ArtisanHubs.DTOs.DTO.Reponse.ArtistProfile;
using ArtisanHubs.DTOs.DTO.Reponse.Order;
using ArtisanHubs.DTOs.DTO.Request.ArtistProfile;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ArtisanHubs.Bussiness.Services.ArtistProfiles.Implements
{
    public class ArtistProfileService : IArtistProfileService
    {
        private readonly IArtistProfileRepository _repo;
        private readonly IMapper _mapper;
        private readonly IAccountRepository _accountRepo;
        private readonly ArtisanHubsDbContext _context;
        private readonly PhotoService _photoService;

        public ArtistProfileService(IArtistProfileRepository repo, IMapper mapper, PhotoService photoService, IAccountRepository accountRepository, ArtisanHubsDbContext context)
        {
            _repo = repo;
            _mapper = mapper;
            _photoService = photoService;
            _accountRepo = accountRepository;
            _context = context;
        }

        public async Task<int?> GetArtistIdByAccountIdAsync(int accountId)
        {
            var profile = await _repo.GetProfileByAccountIdAsync(accountId);
            return profile?.ArtistId;
        }

        // Lấy profile của nghệ nhân đang đăng nhập
        public async Task<ApiResponse<ArtistProfileResponse?>> GetMyProfileAsync(int accountId)
        {
            try
            {
                var profile = await _repo.GetProfileByAccountIdAsync(accountId);
                if (profile == null)
                    return ApiResponse<ArtistProfileResponse?>.FailResponse("Artist profile not found", 404);

                var response = _mapper.Map<ArtistProfileResponse>(profile);
                return ApiResponse<ArtistProfileResponse?>.SuccessResponse(response, "Get profile successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<ArtistProfileResponse?>.FailResponse($"Error: {ex.Message}", 500);
            }
        }

        //// Tạo mới profile cho nghệ nhân
        //public async Task<ApiResponse<ArtistProfileResponse>> CreateMyProfileAsync(int accountId, ArtistProfileRequest request)
        //{
        //    await using var transaction = await _context.Database.BeginTransactionAsync();           
        //    try
        //    {
        //        // Kiểm tra các điều kiện như cũ
        //        var existingProfile = await _repo.GetProfileByAccountIdAsync(accountId);
        //        if (existingProfile != null)
        //        {
        //            return ApiResponse<ArtistProfileResponse>.FailResponse("Artist profile already exists for this account.", 409);
        //        }

        //        var accountToUpdate = await _accountRepo.GetByIdAsync(accountId);
        //        if (accountToUpdate == null || accountToUpdate.Role != "Customer")
        //        {
        //            return ApiResponse<ArtistProfileResponse>.FailResponse("Only accounts with 'Customer' role can create an artist profile.", 403);
        //        }

        //        // 1. Dùng Repository để tạo profile.
        //        // Repository này sẽ gọi SaveChanges() lần 1.
        //        var entity = _mapper.Map<Artistprofile>(request);
        //        entity.AccountId = accountId;
        //        entity.CreatedAt = DateTime.UtcNow;
        //        await _repo.CreateAsync(entity);

        //        // 2. Dùng Repository để cập nhật role.
        //        // Repository này sẽ gọi SaveChanges() lần 2.
        //        accountToUpdate.Role = "Artist";
        //        await _accountRepo.UpdateAsync(accountToUpdate);

        //        // Handle image upload
        //        if (request.ProfileImage != null)
        //        {
        //            var imageUrl = await _photoService.UploadImageAsync(request.ProfileImage);
        //            if (!string.IsNullOrEmpty(imageUrl))
        //            {
        //                entity.ProfileImage = imageUrl; // Adjust property name as needed
        //            }
        //        }

        //        await _repo.CreateAsync(entity);

        //        var response = _mapper.Map<ArtistProfileResponse>(entity);
        //        return ApiResponse<ArtistProfileResponse>.SuccessResponse(response, "Create profile and upgrade role to Artist successfully", 201);
        //    }
        //    catch (Exception ex)
        //    {
        //        // 4. Nếu có bất kỳ lỗi nào, rollback tất cả thay đổi
        //        await transaction.RollbackAsync();
        //        return ApiResponse<ArtistProfileResponse>.FailResponse($"Error: {ex.Message}", 500);
        //    }
        //}
        public async Task<ApiResponse<ArtistProfileResponse>> CreateMyProfileAsync(int accountId, ArtistProfileRequest request)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existingProfile = await _repo.GetProfileByAccountIdAsync(accountId);
                if (existingProfile != null)
                    return ApiResponse<ArtistProfileResponse>.FailResponse("Artist profile already exists for this account.", 409);

                var accountToUpdate = await _accountRepo.GetByIdAsync(accountId);
                if (accountToUpdate == null || accountToUpdate.Role != "Customer")
                    return ApiResponse<ArtistProfileResponse>.FailResponse("Only accounts with 'Customer' role can create an artist profile.", 403);

                // 1️⃣ Tạo profile mới
                var entity = _mapper.Map<Artistprofile>(request);
                entity.AccountId = accountId;
                entity.CreatedAt = DateTime.UtcNow;

                await _repo.CreateAsync(entity); // ✅ chỉ gọi 1 lần

                // 2️⃣ Cập nhật role
                accountToUpdate.Role = "Artist";
                await _accountRepo.UpdateAsync(accountToUpdate);

                // 3️⃣ Upload ảnh (cập nhật sau khi có URL)
                if (request.ProfileImage != null)
                {
                    var imageUrl = await _photoService.UploadImageAsync(request.ProfileImage);
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        entity.ProfileImage = imageUrl;
                        await _repo.UpdateAsync(entity); // chỉ update, không create lại
                    }
                }

                var response = _mapper.Map<ArtistProfileResponse>(entity);
                await transaction.CommitAsync();
                return ApiResponse<ArtistProfileResponse>.SuccessResponse(response, "Create profile and upgrade role to Artist successfully", 201);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ApiResponse<ArtistProfileResponse>.FailResponse($"Error: {ex.Message}", 500);
            }
        }

        // Cập nhật profile cho nghệ nhân
        // Cập nhật profile cho nghệ nhân
        public async Task<ApiResponse<ArtistProfileResponse?>> UpdateMyProfileAsync(int accountId, ArtistProfileRequest request)
        {
            try
            {
                var existing = await _repo.GetQueryable()
                                          .Include(p => p.Achievements)
                                          .FirstOrDefaultAsync(p => p.AccountId == accountId);

                if (existing == null)
                    return ApiResponse<ArtistProfileResponse?>.FailResponse("Artist profile not found to update", 404);

                // Mapper sẽ cập nhật các trường cơ bản và xử lý danh sách Achievements
                _mapper.Map(request, existing);

                    if (request.ProfileImage != null)
                    {
                        // Upload the image and get the URL
                        var imageUrl = await _photoService.UploadImageAsync(request.ProfileImage);
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            existing.ProfileImage = imageUrl; // Assign the uploaded image URL to the profile
                        }
                    }

                await _repo.UpdateAsync(existing);

                var response = _mapper.Map<ArtistProfileResponse>(existing);
                return ApiResponse<ArtistProfileResponse?>.SuccessResponse(response, "Update profile successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<ArtistProfileResponse?>.FailResponse($"Error: {ex.Message}", 500);
            }
        }

        // Lấy profile tất cả nghệ nhân
        public async Task<ApiResponse<IPaginate<Artistprofile>>> GetAllProfilesAsync(int page, int size, string? searchTerm = null)
        {
            try
            {
                // Lấy danh sách có phân trang
                var result = await _repo.GetPagedAsync(null, page, size, searchTerm);

                // ✅ Trả về gói trong ApiResponse mà KHÔNG ép kiểu
                return ApiResponse<IPaginate<Artistprofile>>.SuccessResponse(
                    result,
                    "Get paginated accounts successfully"
                );
            }
            catch (Exception ex)
            {
                // ✅ Bắt lỗi và trả về fail response
                return ApiResponse<IPaginate<Artistprofile>>.FailResponse($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteProfileAsync(int id)
        {
            try
            {
                var existing = await _repo.GetByIdAsync(id);
                if (existing == null)
                    return ApiResponse<bool>.FailResponse("Artist profile not found to delete", 404);

               var response = await _repo.RemoveAsync(existing);
                return ApiResponse<bool>.SuccessResponse(true, "Delete profile successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.FailResponse($"Error: {ex.Message}", 500);
            }
        }

        //public async Task<ApiResponse<IEnumerable<ArtistProfileResponse>>> GetAllArtistsAsync()
        //{
        //    try
        //    {
        //        var artists = await _repo.GetAllArtistsAsync();
        //        var response = _mapper.Map<IEnumerable<ArtistProfileResponse>>(artists);

        //        return ApiResponse<IEnumerable<ArtistProfileResponse>>.SuccessResponse(
        //            response, "Get all artists successfully.");
        //    }
        //    catch (Exception ex)
        //    {
        //        return ApiResponse<IEnumerable<ArtistProfileResponse>>.FailResponse(
        //            $"An error occurred: {ex.Message}", 500);
        //    }
        //}

        public async Task<bool> CreateWithdrawRequestAsync(int artistId, decimal amount, string bankName, string accountHolder, string accountNumber)
        {
            var wallet = await _context.Artistwallets.FirstOrDefaultAsync(w => w.ArtistId == artistId);
            if (wallet == null || wallet.Balance < amount)
                return false;

            var withdrawRequest = new Withdrawrequest
            {
                ArtistId = artistId,
                Amount = amount,
                BankName = bankName,
                AccountHolder = accountHolder,
                AccountNumber = accountNumber,
                Status = "Pending",
                RequestedAt = DateTime.UtcNow
            };
            _context.Withdrawrequests.Add(withdrawRequest);
            await _context.SaveChangesAsync(); // Để lấy WithdrawId

            wallet.PendingBalance += amount;
            wallet.CreatedAt = DateTime.UtcNow;

            var walletTransaction = new Wallettransaction
            {
                WalletId = wallet.WalletId,
                Amount = -amount,
                TransactionType = "withdraw_request",
                WithdrawId = withdrawRequest.WithdrawId,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            _context.Wallettransactions.Add(walletTransaction);

            await _context.SaveChangesAsync();
            return true;
        }

        // Lấy số dư ví của nghệ nhân
        public async Task<ApiResponse<decimal>> GetWalletBalanceAsync(int artistId)
        {
            var wallet = await _context.Artistwallets
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.ArtistId == artistId);

            if (wallet == null)
                return ApiResponse<decimal>.FailResponse("Wallet not found", 404);

            return ApiResponse<decimal>.SuccessResponse(wallet.Balance, "Get wallet balance successfully");
        }

        public async Task<ApiResponse<List<Commission>>> GetMyCommissionsAsync(int artistId)
        {
            var commissions = await _context.Commissions
                .AsNoTracking()
                .Where(c => c.ArtistId == artistId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return ApiResponse<List<Commission>>.SuccessResponse(commissions, "Get commissions successfully");
        }

        public async Task<ApiResponse<List<Withdrawrequest>>> GetMyWithdrawRequestsAsync(int artistId)
        {
            var withdraws = await _context.Withdrawrequests
                .AsNoTracking()
                .Where(w => w.ArtistId == artistId)
                .OrderByDescending(w => w.RequestedAt)
                .ToListAsync();

            return ApiResponse<List<Withdrawrequest>>.SuccessResponse(withdraws, "Get withdraw requests successfully");
        }

        public async Task<ApiResponse<IPaginate<ArtistOrderResponse>>> GetMyOrdersAsync(
            int artistId, 
            int page = 1, 
            int size = 10, 
            string searchTerm = null, 
            string status = null)
        {
            try
            {
                // Lấy tất cả OrderDetail của các sản phẩm thuộc Artist này
                IQueryable<Orderdetail> query = _context.Orderdetails
                    .Include(od => od.Order)
                        .ThenInclude(o => o.Account)
                    .Include(od => od.Product)
                    .Where(od => od.Product.ArtistId == artistId)
                    .AsNoTracking();

                // Filter by order status
                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(od => od.Order.Status.ToLower() == status.ToLower());
                }

                // Search by customer name or order code
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    string keyword = searchTerm.ToLower();
                    query = query.Where(od =>
                        od.Order.OrderCode.ToString().Contains(keyword) ||
                        od.Order.Account.Username.ToLower().Contains(keyword)
                    );
                }

                // Group by OrderId để lấy danh sách các OrderId unique
                var orderIds = await query
                    .Select(od => od.OrderId)
                    .Distinct()
                    .OrderByDescending(id => id)
                    .Skip((page - 1) * size)
                    .Take(size)
                    .ToListAsync();

                var total = await query.Select(od => od.OrderId).Distinct().CountAsync();

                // Lấy thông tin chi tiết của các orders
                var orders = await _context.Orders
                    .Include(o => o.Account)
                    .Include(o => o.Orderdetails)
                        .ThenInclude(od => od.Product)
                    .Where(o => orderIds.Contains(o.OrderId))
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                // Lấy commissions cho các orders này
                var commissions = await _context.Commissions
                    .Where(c => orderIds.Contains(c.OrderId) && c.ArtistId == artistId)
                    .ToListAsync();

                // Map sang response DTO
                var orderResponses = new List<ArtistOrderResponse>();

                foreach (var order in orders)
                {
                    // Chỉ lấy những items thuộc về artist này
                    var artistOrderDetails = order.Orderdetails
                        .Where(od => od.Product.ArtistId == artistId)
                        .ToList();

                    var orderItems = new List<ArtistOrderItemResponse>();
                    decimal totalCommission = 0;
                    decimal platformFee = 0;

                    foreach (var detail in artistOrderDetails)
                    {
                        // Tìm commission tương ứng
                        var commission = commissions.FirstOrDefault(c => 
                            c.OrderId == order.OrderId && 
                            c.ProductId == detail.ProductId);

                        var itemResponse = new ArtistOrderItemResponse
                        {
                            OrderDetailId = detail.OrderDetailId,
                            ProductId = detail.ProductId,
                            ProductName = detail.Product?.Name ?? "Unknown",
                            ProductImage = detail.Product?.Images,
                            Quantity = detail.Quantity,
                            UnitPrice = detail.UnitPrice,
                            TotalPrice = detail.TotalPrice,
                            CommissionAmount = commission?.Amount ?? 0,
                            CommissionRate = commission?.Rate ?? 0,
                            PlatformShare = commission?.AdminShare ?? 0,
                            IsPaid = commission?.IsPaid ?? false
                        };

                        orderItems.Add(itemResponse);
                        totalCommission += itemResponse.CommissionAmount;
                        platformFee += itemResponse.PlatformShare;
                    }

                    var orderResponse = new ArtistOrderResponse
                    {
                        OrderId = order.OrderId,
                        OrderCode = order.OrderCode,
                        OrderDate = order.OrderDate,
                        Status = order.Status,
                        PaymentMethod = order.PaymentMethod ?? "Unknown",
                        ShippingFee = order.ShippingFee,
                        TotalAmount = order.TotalAmount,
                        ShippingAddress = order.ShippingAddress ?? "",
                        CreatedAt = order.CreatedAt,
                        CustomerId = order.AccountId,
                        CustomerName = order.Account?.Username ?? "Unknown",
                        CustomerEmail = order.Account?.Email ?? "",
                        CustomerPhone = order.Account?.Phone ?? "",
                        OrderItems = orderItems,
                        TotalCommission = totalCommission,
                        PlatformFee = platformFee,
                        ArtistEarnings = totalCommission
                    };

                    orderResponses.Add(orderResponse);
                }

                var totalPages = (int)Math.Ceiling(total / (double)size);

                var paginatedResult = new Paginate<ArtistOrderResponse>
                {
                    Items = orderResponses,
                    Page = page,
                    Size = size,
                    Total = total,
                    TotalPages = totalPages
                };

                return ApiResponse<IPaginate<ArtistOrderResponse>>.SuccessResponse(
                    paginatedResult,
                    "Get artist orders successfully"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<IPaginate<ArtistOrderResponse>>.FailResponse($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ArtistOrderResponse>> GetOrderDetailAsync(int artistId, int orderId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Account)
                    .Include(o => o.Orderdetails)
                        .ThenInclude(od => od.Product)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                {
                    return ApiResponse<ArtistOrderResponse>.FailResponse("Order not found", 404);
                }

                // Kiểm tra xem order có sản phẩm của artist này không
                var hasArtistProduct = order.Orderdetails.Any(od => od.Product.ArtistId == artistId);
                if (!hasArtistProduct)
                {
                    return ApiResponse<ArtistOrderResponse>.FailResponse("You don't have any products in this order", 403);
                }

                // Lấy commissions
                var commissions = await _context.Commissions
                    .Where(c => c.OrderId == orderId && c.ArtistId == artistId)
                    .ToListAsync();

                // Chỉ lấy những items thuộc về artist này
                var artistOrderDetails = order.Orderdetails
                    .Where(od => od.Product.ArtistId == artistId)
                    .ToList();

                var orderItems = new List<ArtistOrderItemResponse>();
                decimal totalCommission = 0;
                decimal platformFee = 0;

                foreach (var detail in artistOrderDetails)
                {
                    var commission = commissions.FirstOrDefault(c => 
                        c.OrderId == order.OrderId && 
                        c.ProductId == detail.ProductId);

                    var itemResponse = new ArtistOrderItemResponse
                    {
                        OrderDetailId = detail.OrderDetailId,
                        ProductId = detail.ProductId,
                        ProductName = detail.Product?.Name ?? "Unknown",
                        ProductImage = detail.Product?.Images,
                        Quantity = detail.Quantity,
                        UnitPrice = detail.UnitPrice,
                        TotalPrice = detail.TotalPrice,
                        CommissionAmount = commission?.Amount ?? 0,
                        CommissionRate = commission?.Rate ?? 0,
                        PlatformShare = commission?.AdminShare ?? 0,
                        IsPaid = commission?.IsPaid ?? false
                    };

                    orderItems.Add(itemResponse);
                    totalCommission += itemResponse.CommissionAmount;
                    platformFee += itemResponse.PlatformShare;
                }

                var orderResponse = new ArtistOrderResponse
                {
                    OrderId = order.OrderId,
                    OrderCode = order.OrderCode,
                    OrderDate = order.OrderDate,
                    Status = order.Status,
                    PaymentMethod = order.PaymentMethod ?? "Unknown",
                    ShippingFee = order.ShippingFee,
                    TotalAmount = order.TotalAmount,
                    ShippingAddress = order.ShippingAddress ?? "",
                    CreatedAt = order.CreatedAt,
                    CustomerId = order.AccountId,
                    CustomerName = order.Account?.Username ?? "Unknown",
                    CustomerEmail = order.Account?.Email ?? "",
                    CustomerPhone = order.Account?.Phone ?? "",
                    OrderItems = orderItems,
                    TotalCommission = totalCommission,
                    PlatformFee = platformFee,
                    ArtistEarnings = totalCommission
                };

                return ApiResponse<ArtistOrderResponse>.SuccessResponse(
                    orderResponse,
                    "Get order detail successfully"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<ArtistOrderResponse>.FailResponse($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Artist cập nhật trạng thái đơn hàng của mình
        /// Chỉ cho phép: PAID → Processing → Shipping
        /// </summary>
        public async Task<ApiResponse<object>> UpdateOrderStatusAsync(int artistId, int orderId, string newStatus)
        {
            try
            {
                // Validate status
                var allowedStatuses = new[] { "Processing", "Shipping" };
                if (!allowedStatuses.Contains(newStatus))
                    return ApiResponse<object>.FailResponse("Invalid status. Allowed: Processing, Shipping", 400);

                // Lấy order
                var order = await _context.Orders
                    .Include(o => o.Orderdetails)
                    .ThenInclude(od => od.Product)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                    return ApiResponse<object>.FailResponse("Order not found", 404);

                // Kiểm tra order có sản phẩm của artist này không
                var hasArtistProduct = order.Orderdetails.Any(od => od.Product.ArtistId == artistId);
                if (!hasArtistProduct)
                    return ApiResponse<object>.FailResponse("This order does not contain your products", 403);

                // Validate state transition
                var validTransitions = new Dictionary<string, string[]>
                {
                    { "PAID", new[] { "Processing" } },
                    { "Processing", new[] { "Shipping" } },
                    { "Shipping", new string[] { } } // Không được chuyển từ Shipping
                };

                if (!validTransitions.ContainsKey(order.Status))
                    return ApiResponse<object>.FailResponse($"Cannot update from current status: {order.Status}", 400);

                if (!validTransitions[order.Status].Contains(newStatus))
                    return ApiResponse<object>.FailResponse($"Cannot change from {order.Status} to {newStatus}", 400);

                // Cập nhật status
                order.Status = newStatus;
                order.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return ApiResponse<object>.SuccessResponse(
                    new { OrderId = orderId, NewStatus = newStatus },
                    "Order status updated successfully"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.FailResponse($"Error: {ex.Message}");
            }
        }
    }
}
