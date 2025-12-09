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

        if (order == null || order.Status != "Paid")
            return false;

        foreach (var detail in order.Orderdetails)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == detail.ProductId);

            if (product == null) continue;

            decimal totalAmount = detail.TotalPrice;
            decimal artistShare = totalAmount * (1 - platformRate);
            decimal adminShare = totalAmount * platformRate;

            var commission = new Commission
            {
                ProductId = product.ProductId,
                ArtistId = product.ArtistId,
                OrderId = order.OrderId,
                Amount = artistShare,
                AdminShare = adminShare,
                Rate = platformRate,
                IsPaid = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Commissions.Add(commission);
            await _context.SaveChangesAsync(); // Để lấy CommissionId

            var wallet = await _context.Artistwallets.FirstOrDefaultAsync(w => w.ArtistId == product.ArtistId);
            if (wallet != null)
            {
                wallet.PendingBalance += artistShare;
                wallet.CreatedAt = DateTime.UtcNow;

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
                var commission = commissions.FirstOrDefault(c => c.OrderDetailId == detail.OrderDetailId);

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
                    CommissionRate = commission?.CommissionRate ?? 0,
                    PlatformCommission = commission?.AdminShare ?? 0,
                    ArtistEarning = commission?.Amount ?? 0,
                    IsPaid = commission?.IsPaid ?? false,
                    PaidAt = commission?.PaidAt
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
                OrderDate = order.OrderDate,
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
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt
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
}
