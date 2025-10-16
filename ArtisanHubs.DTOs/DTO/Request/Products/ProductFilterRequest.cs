using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.DTOs.DTO.Request.Products
{
    public class ProductFilterRequest
    {
        /// <summary>
        /// Filter by category ID
        /// </summary>
        public int? CategoryId { get; set; }

        /// <summary>
        /// Filter by artist ID
        /// </summary>
        public int? ArtistId { get; set; }

        /// <summary>
        /// Minimum price filter
        /// </summary>
        public decimal? MinPrice { get; set; }

        /// <summary>
        /// Maximum price filter
        /// </summary>
        public decimal? MaxPrice { get; set; }

        /// <summary>
        /// Filter by product status
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Search by product name (contains)
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Sort by field (Price, Name, CreatedAt)
        /// </summary>
        public string? SortBy { get; set; } = "CreatedAt";

        /// <summary>
        /// Sort order (asc, desc)
        /// </summary>
        public string? SortOrder { get; set; } = "desc";

        /// <summary>
        /// Page number for pagination
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// Page size for pagination
        /// </summary>
        public int Size { get; set; } = 10;
    }
}