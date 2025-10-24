using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ArtisanHubs.Data.Entities;

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
}
