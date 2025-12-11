using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.API.DTOs.Common;
using ArtisanHubs.Data.Paginate;
using ArtisanHubs.DTOs.DTO.Reponse.Transaction;
using ArtisanHubs.DTOs.DTO.Reponse.Admin;

public class AdminService
{
    private readonly ArtisanHubsDbContext _context;

    public AdminService(ArtisanHubsDbContext context)
    {
        _context = context;
    }

    public async Task<object> GetDashboardStatisticsAsync()
    {
        // 1. Total revenue (sum of all paid orders)
        var totalRevenue = await _context.Orders
            .Where(o => o.Status == "Paid")
            .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

        // 2. Total platform commission (sum of commission.admin_share)
        var totalPlatformCommission = await _context.Commissions
            .SumAsync(c => (decimal?)c.AdminShare) ?? 0;

        // 3. Total artist commission (sum of commission.amount)
        var totalArtistCommission = await _context.Commissions
            .SumAsync(c => (decimal?)c.Amount) ?? 0;

        // 4. Best-selling products (sum of orderdetail.quantity grouped by product_id)
        var bestSellingProducts = await _context.Orderdetails
            .GroupBy(od => od.ProductId)
            .Select(g => new
            {
                name = g.First().Product.Name,
                totalSold = g.Sum(od => od.Quantity)
            })
            .OrderByDescending(x => x.totalSold)
            .Take(10)
            .ToListAsync();

        // 5. Revenue trend over time (daily revenue based on paid orders)
        var revenueData = await _context.Orders
            .Where(o => o.Status == "Paid")
            .GroupBy(o => o.OrderDate.HasValue ? o.OrderDate.Value.Date : DateTime.MinValue.Date)
            .Select(g => new
            {
                Date = g.Key,
                Revenue = g.Sum(o => o.TotalAmount)
            })
            .OrderBy(x => x.Date)
            .ToListAsync();

        var revenueTrend = revenueData
            .Select(x => new
            {
                date = x.Date.ToString("yyyy-MM-dd"),
                revenue = x.Revenue
            })
            .ToList();

        return new
        {
            totalRevenue,
            totalPlatformCommission,
            totalArtistCommission,
            bestSellingProducts,
            revenueTrend
        };
    }

    public async Task<List<Withdrawrequest>> GetPendingWithdrawRequestsAsync()
    {
        return await _context.Withdrawrequests
            .Where(w => w.Status == "Pending")
            .Include(w => w.Artist)
            .ToListAsync();
    }
    public async Task<List<Commission>> GetUnpaidCommissionsAsync(int artistId)
    {
        return await _context.Commissions
            .Where(c => c.ArtistId == artistId && !c.IsPaid)
            .ToListAsync();
    }

    public async Task<bool> ApproveWithdrawRequestAsync(int withdrawRequestId)
    {
        var withdrawRequest = await _context.Withdrawrequests
            .Include(w => w.Artist)
            .FirstOrDefaultAsync(w => w.WithdrawId == withdrawRequestId);

        if (withdrawRequest == null || withdrawRequest.Status != "Pending")
            return false;

        // Mark as approved
        withdrawRequest.Status = "Approved";
        withdrawRequest.ApprovedAt = DateTime.UtcNow;

        // Find unpaid commissions for the artist
        var unpaidCommissions = await _context.Commissions
            .Where(c => c.ArtistId == withdrawRequest.ArtistId && !c.IsPaid)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        decimal toPay = withdrawRequest.Amount;
        foreach (var commission in unpaidCommissions)
        {
            if (toPay <= 0) break;
            commission.IsPaid = true;
            toPay -= commission.Amount;

            var wallet = await _context.Artistwallets.FirstOrDefaultAsync(w => w.ArtistId == commission.ArtistId);
            if (wallet != null)
            {
                wallet.PendingBalance -= commission.Amount;
                wallet.Balance += commission.Amount;

                var walletTransaction = new Wallettransaction
                {
                    WalletId = wallet.WalletId,
                    Amount = commission.Amount,
                    TransactionType = "commission_paid",
                    CommissionId = commission.CommissionId,
                    Status = "Completed",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Wallettransactions.Add(walletTransaction);
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CreateCommissionForPaidOrderAsync(int orderId, decimal platformRate)
    {
        var order = await _context.Orders
            .Include(o => o.Orderdetails)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

        if (order == null || order.Status != "PAID")
            return false;

        // ✅ Check xem đã tạo commission cho order này chưa (tránh trùng)
        var existingCommissions = await _context.Commissions
            .AnyAsync(c => c.OrderId == orderId);
        if (existingCommissions)
            return true; // Đã có commission rồi, không tạo nữa

        foreach (var detail in order.Orderdetails)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == detail.ProductId);

            if (product == null) continue;

            decimal totalAmount = detail.TotalPrice;
            decimal adminShare = totalAmount * platformRate;
            decimal artistShare = totalAmount - adminShare;

            var commission = new Commission
            {
                ProductId = product.ProductId,
                ArtistId = product.ArtistId,
                OrderId = order.OrderId,
                Amount = totalAmount, // Tổng hoa hồng
                AdminShare = adminShare, // Phần sàn (10%)
                Rate = platformRate,
                IsPaid = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Commissions.Add(commission);
            await _context.SaveChangesAsync(); // Để lấy CommissionId

            var wallet = await _context.Artistwallets.FirstOrDefaultAsync(w => w.ArtistId == product.ArtistId);
            if (wallet == null)
            {
                // Tự động tạo wallet nếu chưa có
                wallet = new Artistwallet
                {
                    ArtistId = product.ArtistId,
                    Balance = 0,
                    PendingBalance = 0,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Artistwallets.Add(wallet);
                await _context.SaveChangesAsync();
            }

            wallet.PendingBalance += artistShare;

            var walletTransaction = new Wallettransaction
            {
                WalletId = wallet.WalletId,
                Amount = artistShare,
                TransactionType = "commission_pending",
                CommissionId = commission.CommissionId,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            _context.Wallettransactions.Add(walletTransaction);
            await _context.SaveChangesAsync();
        }

        return true;
    }

    public async Task<object> GetArtistWalletBalanceAsync(int artistId)
    {
        var wallet = await _context.Artistwallets
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.ArtistId == artistId);

        if (wallet == null)
            return null;

        return new
        {
            artistId = artistId,
            balance = wallet.Balance,
            pendingBalance = wallet.PendingBalance,
            createdAt = wallet.CreatedAt
        };
    }

    public async Task<ApiResponse<IPaginate<TransactionResponse>>> GetAllTransactionsAsync(
        int page = 1, 
        int size = 10, 
        string searchTerm = null, 
        string transactionType = null, 
        string status = null)
    {
        try
        {
            IQueryable<Wallettransaction> query = _context.Wallettransactions
                .Include(t => t.Wallet)
                    .ThenInclude(w => w.Artist)
                .Include(t => t.Commission)
                    .ThenInclude(c => c.Order)
                .Include(t => t.Withdraw)
                .AsNoTracking();

            // Search by artist name or transaction ID
            if (!string.IsNullOrEmpty(searchTerm))
            {
                string keyword = searchTerm.ToLower();
                query = query.Where(t =>
                    t.Wallet.Artist.ArtistName.ToLower().Contains(keyword) ||
                    t.TransactionId.ToString().Contains(keyword)
                );
            }

            // Filter by transaction type
            if (!string.IsNullOrEmpty(transactionType))
            {
                query = query.Where(t => t.TransactionType.ToLower() == transactionType.ToLower());
            }

            // Filter by status
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(t => t.Status.ToLower() == status.ToLower());
            }

            // Get total count
            var total = await query.CountAsync();

            // Apply pagination
            var transactions = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            // Map to response DTO
            var transactionResponses = transactions.Select(t => new TransactionResponse
            {
                TransactionId = t.TransactionId,
                WalletId = t.WalletId,
                ArtistId = t.Wallet.ArtistId,
                ArtistName = t.Wallet.Artist?.ArtistName ?? "Unknown",
                Amount = t.Amount,
                TransactionType = t.TransactionType,
                CommissionId = t.CommissionId,
                WithdrawId = t.WithdrawId,
                PaymentId = t.PaymentId,
                Status = t.Status,
                CreatedAt = t.CreatedAt,
                OrderCode = t.Commission?.Order?.OrderCode.ToString(),
                BankName = t.Withdraw?.BankName
            }).ToList();

            var totalPages = (int)Math.Ceiling(total / (double)size);

            var paginatedResult = new Paginate<TransactionResponse>
            {
                Items = transactionResponses,
                Page = page,
                Size = size,
                Total = total,
                TotalPages = totalPages
            };

            return ApiResponse<IPaginate<TransactionResponse>>.SuccessResponse(
                paginatedResult,
                "Get paginated transactions successfully"
            );
        }
        catch (Exception ex)
        {
            return ApiResponse<IPaginate<TransactionResponse>>.FailResponse($"Error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<TransactionResponse>> GetTransactionByIdAsync(int transactionId)
    {
        try
        {
            var transaction = await _context.Wallettransactions
                .Include(t => t.Wallet)
                    .ThenInclude(w => w.Artist)
                .Include(t => t.Commission)
                    .ThenInclude(c => c.Order)
                .Include(t => t.Withdraw)
                .Include(t => t.Payment)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId);

            if (transaction == null)
            {
                return ApiResponse<TransactionResponse>.FailResponse("Transaction not found", 404);
            }

            var response = new TransactionResponse
            {
                TransactionId = transaction.TransactionId,
                WalletId = transaction.WalletId,
                ArtistId = transaction.Wallet.ArtistId,
                ArtistName = transaction.Wallet.Artist?.ArtistName ?? "Unknown",
                Amount = transaction.Amount,
                TransactionType = transaction.TransactionType,
                CommissionId = transaction.CommissionId,
                WithdrawId = transaction.WithdrawId,
                PaymentId = transaction.PaymentId,
                Status = transaction.Status,
                CreatedAt = transaction.CreatedAt,
                OrderCode = transaction.Commission?.Order?.OrderCode.ToString(),
                BankName = transaction.Withdraw?.BankName
            };

            return ApiResponse<TransactionResponse>.SuccessResponse(
                response,
                "Get transaction successfully"
            );
        }
        catch (Exception ex)
        {
            return ApiResponse<TransactionResponse>.FailResponse($"Error: {ex.Message}");
        }
    }

    // Get order detail with commission breakdown for Admin
    public async Task<ApiResponse<AdminOrderDetailResponse>> GetOrderDetailWithCommissionAsync(int orderId)
    {
        try
        {
            var order = await _context.Orders
                .Include(o => o.Account)
                .Include(o => o.Orderdetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.Artist)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return ApiResponse<AdminOrderDetailResponse>.FailResponse("Order not found", 404);
            }

            // Get all commissions for this order
            var commissions = await _context.Commissions
                .Where(c => c.OrderId == orderId)
                .ToListAsync();

            var orderItems = new List<AdminOrderItemResponse>();
            decimal subTotal = 0;
            int totalItems = 0;
            decimal totalPlatformCommission = 0;
            decimal totalArtistEarnings = 0;

            foreach (var detail in order.Orderdetails)
            {
                var commission = commissions.FirstOrDefault(c => 
                    c.ProductId == detail.ProductId && 
                    c.OrderId == orderId);

                var itemResponse = new AdminOrderItemResponse
                {
                    OrderDetailId = detail.OrderDetailId,
                    ProductId = detail.ProductId,
                    ProductName = detail.Product?.Name ?? "Unknown",
                    ProductImage = detail.Product?.Images,
                    Quantity = detail.Quantity,
                    UnitPrice = detail.UnitPrice,
                    TotalPrice = detail.TotalPrice,
                    ArtistId = detail.Product?.ArtistId ?? 0,
                    ArtistName = detail.Product?.Artist?.ArtistName ?? "Unknown",
                    CommissionId = commission?.CommissionId,
                    CommissionAmount = commission?.Amount ?? 0,
                    CommissionRate = commission?.Rate ?? 0,
                    PlatformCommission = commission?.AdminShare ?? 0,
                    ArtistEarning = commission?.Amount ?? 0,
                    IsPaid = commission?.IsPaid ?? false,
                    PaidAt = commission?.CreatedAt
                };

                orderItems.Add(itemResponse);
                subTotal += detail.TotalPrice;
                totalItems += detail.Quantity;
                totalPlatformCommission += commission?.AdminShare ?? 0;
                totalArtistEarnings += commission?.Amount ?? 0;
            }

            var orderResponse = new AdminOrderDetailResponse
            {
                OrderId = order.OrderId,
                OrderCode = order.OrderCode,
                OrderDate = order.OrderDate ?? DateTime.UtcNow,
                Status = order.Status,
                PaymentMethod = order.PaymentMethod ?? "Unknown",
                ShippingFee = order.ShippingFee,
                TotalAmount = order.TotalAmount,
                ShippingAddress = order.ShippingAddress ?? "",
                CustomerId = order.AccountId,
                CustomerName = order.Account?.Username ?? "Unknown",
                CustomerEmail = order.Account?.Email ?? "Unknown",
                CustomerPhone = order.Account?.Phone,
                OrderItems = orderItems,
                TotalItems = totalItems,
                SubTotal = subTotal,
                TotalPlatformCommission = totalPlatformCommission,
                TotalArtistEarnings = totalArtistEarnings,
                CreatedAt = order.CreatedAt ?? DateTime.UtcNow,
                UpdatedAt = order.UpdatedAt ?? DateTime.UtcNow
            };

            return ApiResponse<AdminOrderDetailResponse>.SuccessResponse(
                orderResponse,
                "Get order detail successfully"
            );
        }
        catch (Exception ex)
        {
            return ApiResponse<AdminOrderDetailResponse>.FailResponse($"Error: {ex.Message}");
        }
    }

    // Get order statistics with total platform commissions
    public async Task<ApiResponse<OrderStatisticsResponse>> GetOrderStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var query = _context.Orders.AsQueryable();

            // Apply date filters if provided
            if (fromDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(o => o.OrderDate <= toDate.Value);
            }

            var orders = await query.ToListAsync();

            // Count orders by status
            var totalOrders = orders.Count;
            var pendingOrders = orders.Count(o => o.Status == "Pending");
            var paidOrders = orders.Count(o => o.Status == "Paid");
            var processingOrders = orders.Count(o => o.Status == "Processing");
            var shippingOrders = orders.Count(o => o.Status == "Shipping");
            var deliveredOrders = orders.Count(o => o.Status == "Delivered");
            var cancelledOrders = orders.Count(o => o.Status == "Cancelled");

            // Calculate revenue
            var totalRevenue = orders.Where(o => o.Status != "Cancelled").Sum(o => o.TotalAmount);
            var totalShippingFees = orders.Where(o => o.Status != "Cancelled").Sum(o => o.ShippingFee);

            // Get commissions for orders in the date range
            var orderIds = orders.Select(o => o.OrderId).ToList();
            var commissions = await _context.Commissions
                .Where(c => orderIds.Contains(c.OrderId))
                .ToListAsync();

            var totalPlatformCommission = commissions.Sum(c => c.AdminShare);
            var totalArtistEarnings = commissions.Sum(c => c.Amount);
            var paidCommissions = commissions.Where(c => c.IsPaid).Sum(c => c.AdminShare);
            var unpaidCommissions = commissions.Where(c => !c.IsPaid).Sum(c => c.AdminShare);

            var statistics = new OrderStatisticsResponse
            {
                TotalOrders = totalOrders,
                PendingOrders = pendingOrders,
                PaidOrders = paidOrders,
                ProcessingOrders = processingOrders,
                ShippingOrders = shippingOrders,
                DeliveredOrders = deliveredOrders,
                CancelledOrders = cancelledOrders,
                TotalRevenue = totalRevenue,
                TotalPlatformCommission = totalPlatformCommission,
                TotalArtistEarnings = totalArtistEarnings,
                TotalShippingFees = totalShippingFees,
                PaidCommissions = paidCommissions,
                UnpaidCommissions = unpaidCommissions,
                TotalCommissionRecords = commissions.Count,
                FromDate = fromDate,
                ToDate = toDate
            };

            return ApiResponse<OrderStatisticsResponse>.SuccessResponse(
                statistics,
                "Get order statistics successfully"
            );
        }
        catch (Exception ex)
        {
            return ApiResponse<OrderStatisticsResponse>.FailResponse($"Error: {ex.Message}");
        }
    }

    // Get all artists with product count and statistics for Admin management
    public async Task<ApiResponse<IPaginate<ArtistManagementResponse>>> GetAllArtistsForAdminAsync(
        int page = 1, 
        int size = 10, 
        string searchTerm = null, 
        string status = null)
    {
        try
        {
            IQueryable<Artistprofile> query = _context.Artistprofiles
                .Include(a => a.Account)
                .Include(a => a.Products)
                .AsNoTracking();

            // Search by artist name, bio, or email
            if (!string.IsNullOrEmpty(searchTerm))
            {
                string keyword = searchTerm.ToLower();
                query = query.Where(a =>
                    a.ArtistName.ToLower().Contains(keyword) ||
                    (a.Bio != null && a.Bio.ToLower().Contains(keyword)) ||
                    (a.Account != null && a.Account.Email.ToLower().Contains(keyword))
                );
            }

            // Filter by account status
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(a => a.Account != null && a.Account.Status.ToLower() == status.ToLower());
            }

            // Get total count
            var total = await query.CountAsync();

            // Apply pagination
            var artists = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            // Map to response DTO with statistics
            var artistResponses = new List<ArtistManagementResponse>();

            foreach (var artist in artists)
            {
                // Get wallet info
                var wallet = await _context.Artistwallets
                    .AsNoTracking()
                    .FirstOrDefaultAsync(w => w.ArtistId == artist.ArtistId);

                // Get commission stats
                var commissions = await _context.Commissions
                    .Where(c => c.ArtistId == artist.ArtistId)
                    .ToListAsync();

                // Get sold products count (from order details)
                var soldProductsCount = await _context.Orderdetails
                    .Where(od => od.Product.ArtistId == artist.ArtistId)
                    .Select(od => od.ProductId)
                    .Distinct()
                    .CountAsync();

                // Calculate total revenue from completed orders
                var totalRevenue = await _context.Orderdetails
                    .Where(od => od.Product.ArtistId == artist.ArtistId && 
                                od.Order.Status == "Delivered")
                    .SumAsync(od => od.TotalPrice);

                var artistResponse = new ArtistManagementResponse
                {
                    ArtistId = artist.ArtistId,
                    ArtistName = artist.ArtistName,
                    Bio = artist.Bio ?? "",
                    ProfilePicture = artist.ProfileImage,
                    ContactEmail = artist.Account?.Email,
                    ContactPhone = artist.Account?.Phone,
                    AccountId = artist.AccountId,
                    Username = artist.Account?.Username ?? "Unknown",
                    Email = artist.Account?.Email ?? "Unknown",
                    AccountStatus = artist.Account?.Status ?? "Unknown",
                    TotalProducts = artist.Products.Count,
                    ActiveProducts = artist.Products.Count(p => p.Status == "Active"),
                    SoldProducts = soldProductsCount,
                    TotalRevenue = totalRevenue,
                    TotalCommissionEarned = commissions.Where(c => c.IsPaid).Sum(c => c.Amount),
                    WalletBalance = wallet?.Balance ?? 0,
                    PendingBalance = wallet?.PendingBalance ?? 0,
                    CreatedAt = artist.CreatedAt ?? DateTime.UtcNow,
                    UpdatedAt = artist.CreatedAt
                };

                artistResponses.Add(artistResponse);
            }

            var totalPages = (int)Math.Ceiling(total / (double)size);

            var paginatedResult = new Paginate<ArtistManagementResponse>
            {
                Items = artistResponses,
                Page = page,
                Size = size,
                Total = total,
                TotalPages = totalPages
            };

            return ApiResponse<IPaginate<ArtistManagementResponse>>.SuccessResponse(
                paginatedResult,
                "Get artists successfully"
            );
        }
        catch (Exception ex)
        {
            return ApiResponse<IPaginate<ArtistManagementResponse>>.FailResponse($"Error: {ex.Message}");
        }
    }

    // Get all products of a specific artist
    public async Task<ApiResponse<ArtistProductListResponse>> GetArtistProductsAsync(int artistId)
    {
        try
        {
            var artist = await _context.Artistprofiles
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.ArtistId == artistId);

            if (artist == null)
            {
                return ApiResponse<ArtistProductListResponse>.FailResponse("Artist not found", 404);
            }

            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.ArtistId == artistId)
                .OrderByDescending(p => p.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            var productItems = new List<ArtistProductItemResponse>();
            decimal totalInventoryValue = 0;

            foreach (var product in products)
            {
                // Get sales statistics for this product
                var totalSold = await _context.Orderdetails
                    .Where(od => od.ProductId == product.ProductId && 
                                od.Order.Status == "Delivered")
                    .SumAsync(od => od.Quantity);

                var totalRevenue = await _context.Orderdetails
                    .Where(od => od.ProductId == product.ProductId && 
                                od.Order.Status == "Delivered")
                    .SumAsync(od => od.TotalPrice);

                var productItem = new ArtistProductItemResponse
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    Description = product.Description,
                    Images = product.Images,
                    Price = product.Price,
                    DiscountPrice = product.DiscountPrice,
                    Quantity = product.StockQuantity,
                    Status = product.Status,
                    CategoryId = product.CategoryId ?? 0,
                    CategoryName = product.Category?.Description ?? "Unknown",
                    TotalSold = totalSold,
                    TotalRevenue = totalRevenue,
                    CreatedAt = product.CreatedAt ?? DateTime.UtcNow,
                    UpdatedAt = product.UpdatedAt
                };

                productItems.Add(productItem);

                // Calculate inventory value
                var currentPrice = product.DiscountPrice ?? product.Price;
                totalInventoryValue += currentPrice * product.StockQuantity;
            }

            var response = new ArtistProductListResponse
            {
                ArtistId = artist.ArtistId,
                ArtistName = artist.ArtistName,
                ProfilePicture = artist.ProfileImage,
                Products = productItems,
                TotalProducts = products.Count,
                ActiveProducts = products.Count(p => p.Status == "Active"),
                InactiveProducts = products.Count(p => p.Status != "Active"),
                TotalInventoryValue = totalInventoryValue
            };

            return ApiResponse<ArtistProductListResponse>.SuccessResponse(
                response,
                "Get artist products successfully"
            );
        }
        catch (Exception ex)
        {
            return ApiResponse<ArtistProductListResponse>.FailResponse($"Error: {ex.Message}");
        }
    }

    // Get revenue statistics by period (day, week, month)
    public async Task<ApiResponse<RevenueStatisticsResponse>> GetRevenueStatisticsAsync(
        string period = "month", 
        int count = 6, 
        DateTime? fromDate = null, 
        DateTime? toDate = null)
    {
        try
        {
            var labels = new List<string>();
            var revenueData = new List<decimal>();
            var platformCommissionData = new List<decimal>();
            var artistEarningsData = new List<decimal>();

            DateTime startDate = fromDate ?? DateTime.UtcNow.AddMonths(-count);
            DateTime endDate = toDate ?? DateTime.UtcNow;

            if (period.ToLower() == "day")
            {
                // Daily statistics
                for (int i = 0; i < count; i++)
                {
                    var date = endDate.AddDays(-i);
                    labels.Insert(0, date.ToString("dd/MM"));

                    var orders = await _context.Orders
                        .Where(o => o.OrderDate.HasValue && 
                                   o.OrderDate.Value.Date == date.Date &&
                                   o.Status != "Cancelled")
                        .ToListAsync();

                    var orderIds = orders.Select(o => o.OrderId).ToList();
                    var commissions = await _context.Commissions
                        .Where(c => orderIds.Contains(c.OrderId))
                        .ToListAsync();

                    revenueData.Insert(0, orders.Sum(o => o.TotalAmount));
                    platformCommissionData.Insert(0, commissions.Sum(c => c.AdminShare));
                    artistEarningsData.Insert(0, commissions.Sum(c => c.Amount));
                }
            }
            else if (period.ToLower() == "week")
            {
                // Weekly statistics
                for (int i = 0; i < count; i++)
                {
                    var weekEnd = endDate.AddDays(-7 * i);
                    var weekStart = weekEnd.AddDays(-6);
                    labels.Insert(0, $"{weekStart:dd/MM}-{weekEnd:dd/MM}");

                    var orders = await _context.Orders
                        .Where(o => o.OrderDate.HasValue &&
                                   o.OrderDate.Value.Date >= weekStart.Date &&
                                   o.OrderDate.Value.Date <= weekEnd.Date &&
                                   o.Status != "Cancelled")
                        .ToListAsync();

                    var orderIds = orders.Select(o => o.OrderId).ToList();
                    var commissions = await _context.Commissions
                        .Where(c => orderIds.Contains(c.OrderId))
                        .ToListAsync();

                    revenueData.Insert(0, orders.Sum(o => o.TotalAmount));
                    platformCommissionData.Insert(0, commissions.Sum(c => c.AdminShare));
                    artistEarningsData.Insert(0, commissions.Sum(c => c.Amount));
                }
            }
            else // month
            {
                // Monthly statistics
                for (int i = 0; i < count; i++)
                {
                    var monthDate = endDate.AddMonths(-i);
                    var monthStart = new DateTime(monthDate.Year, monthDate.Month, 1);
                    var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                    labels.Insert(0, monthDate.ToString("MM/yyyy"));

                    var orders = await _context.Orders
                        .Where(o => o.OrderDate.HasValue &&
                                   o.OrderDate.Value >= monthStart &&
                                   o.OrderDate.Value <= monthEnd &&
                                   o.Status != "Cancelled")
                        .ToListAsync();

                    var orderIds = orders.Select(o => o.OrderId).ToList();
                    var commissions = await _context.Commissions
                        .Where(c => orderIds.Contains(c.OrderId))
                        .ToListAsync();

                    revenueData.Insert(0, orders.Sum(o => o.TotalAmount));
                    platformCommissionData.Insert(0, commissions.Sum(c => c.AdminShare));
                    artistEarningsData.Insert(0, commissions.Sum(c => c.Amount));
                }
            }

            // Calculate summary
            var totalRevenue = revenueData.Sum();
            var totalPlatformCommission = platformCommissionData.Sum();
            var totalArtistEarnings = artistEarningsData.Sum();
            var totalOrders = await _context.Orders
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate && o.Status != "Cancelled")
                .CountAsync();
            var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

            var response = new RevenueStatisticsResponse
            {
                Labels = labels,
                Datasets = new List<RevenueDataset>
                {
                    new RevenueDataset
                    {
                        Label = "Doanh thu",
                        Data = revenueData,
                        BorderColor = "#2196F3",
                        BackgroundColor = "rgba(33, 150, 243, 0.1)"
                    },
                    new RevenueDataset
                    {
                        Label = "Hoa hồng nền tảng",
                        Data = platformCommissionData,
                        BorderColor = "#4CAF50",
                        BackgroundColor = "rgba(76, 175, 80, 0.1)"
                    },
                    new RevenueDataset
                    {
                        Label = "Thu nhập nghệ nhân",
                        Data = artistEarningsData,
                        BorderColor = "#FF9800",
                        BackgroundColor = "rgba(255, 152, 0, 0.1)"
                    }
                },
                Summary = new RevenueSummary
                {
                    TotalRevenue = totalRevenue,
                    TotalPlatformCommission = totalPlatformCommission,
                    TotalArtistEarnings = totalArtistEarnings,
                    AverageOrderValue = averageOrderValue,
                    TotalOrders = totalOrders,
                    Period = period
                }
            };

            return ApiResponse<RevenueStatisticsResponse>.SuccessResponse(
                response,
                "Get revenue statistics successfully"
            );
        }
        catch (Exception ex)
        {
            return ApiResponse<RevenueStatisticsResponse>.FailResponse($"Error: {ex.Message}");
        }
    }

    // Get order status distribution
    public async Task<ApiResponse<OrderStatusDistributionResponse>> GetOrderStatusDistributionAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        try
        {
            var query = _context.Orders.AsQueryable();

            // Apply date filters
            if (fromDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(o => o.OrderDate <= toDate.Value);
            }

            var orders = await query.ToListAsync();

            // Count by status
            var totalOrders = orders.Count;
            var pendingOrders = orders.Count(o => o.Status == "Pending");
            var paidOrders = orders.Count(o => o.Status == "Paid");
            var processingOrders = orders.Count(o => o.Status == "Processing");
            var shippingOrders = orders.Count(o => o.Status == "Shipping");
            var deliveredOrders = orders.Count(o => o.Status == "Delivered");
            var cancelledOrders = orders.Count(o => o.Status == "Cancelled");

            // Calculate rates
            var deliveryRate = totalOrders > 0 ? (decimal)deliveredOrders / totalOrders * 100 : 0;
            var cancellationRate = totalOrders > 0 ? (decimal)cancelledOrders / totalOrders * 100 : 0;

            var response = new OrderStatusDistributionResponse
            {
                Labels = new List<string> 
                { 
                    "Pending", "Paid", "Processing", "Shipping", "Delivered", "Cancelled" 
                },
                Data = new List<int> 
                { 
                    pendingOrders, paidOrders, processingOrders, 
                    shippingOrders, deliveredOrders, cancelledOrders 
                },
                Colors = new List<string> 
                { 
                    "#FFA500", "#4CAF50", "#2196F3", 
                    "#9C27B0", "#8BC34A", "#F44336" 
                },
                Summary = new OrderStatusSummary
                {
                    TotalOrders = totalOrders,
                    PendingOrders = pendingOrders,
                    PaidOrders = paidOrders,
                    ProcessingOrders = processingOrders,
                    ShippingOrders = shippingOrders,
                    DeliveredOrders = deliveredOrders,
                    CancelledOrders = cancelledOrders,
                    DeliveryRate = Math.Round(deliveryRate, 2),
                    CancellationRate = Math.Round(cancellationRate, 2)
                }
            };

            return ApiResponse<OrderStatusDistributionResponse>.SuccessResponse(
                response,
                "Get order status distribution successfully"
            );
        }
        catch (Exception ex)
        {
            return ApiResponse<OrderStatusDistributionResponse>.FailResponse($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Chuyển tiền từ PendingBalance sang Balance cho Artist khi customer xác nhận đã nhận hàng
    /// </summary>
    public async Task<bool> ReleaseCommissionToArtistAsync(int orderId)
    {
        try
        {
            // Lấy tất cả commission chưa trả của đơn hàng này
            var commissions = await _context.Commissions
                .Where(c => c.OrderId == orderId && !c.IsPaid)
                .ToListAsync();

            if (!commissions.Any())
                return false; // Không có commission nào hoặc đã trả hết rồi

            foreach (var commission in commissions)
            {
                // Tìm ví của artist
                var wallet = await _context.Artistwallets
                    .FirstOrDefaultAsync(w => w.ArtistId == commission.ArtistId);

                if (wallet == null)
                {
                    // Tự động tạo wallet nếu chưa có
                    wallet = new Artistwallet
                    {
                        ArtistId = commission.ArtistId,
                        Balance = 0,
                        PendingBalance = 0,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Artistwallets.Add(wallet);
                    await _context.SaveChangesAsync();
                }

                // Tính tiền artist nhận được (Amount - AdminShare)
                decimal artistEarning = commission.Amount - commission.AdminShare;

                // Chuyển từ PendingBalance sang Balance
                wallet.PendingBalance -= artistEarning;
                wallet.Balance += artistEarning;

                // Đánh dấu commission đã trả
                commission.IsPaid = true;

                // Tạo transaction log
                var transaction = new Wallettransaction
                {
                    WalletId = wallet.WalletId,
                    Amount = artistEarning,
                    TransactionType = "commission_released",
                    CommissionId = commission.CommissionId,
                    Status = "Completed",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Wallettransactions.Add(transaction);
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            // Log error if needed
            return false;
        }
    }
}
