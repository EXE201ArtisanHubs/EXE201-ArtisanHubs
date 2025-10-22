using ArtisanHubs.API.DTOs.Common;
using ArtisanHubs.Bussiness.Services.ArtistProfiles.Interfaces;
using ArtisanHubs.Bussiness.Services.Carts.Interfaces;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Repositories.ArtistProfiles.Interfaces;
using ArtisanHubs.Data.Repositories.Carts.Interfaces;
using ArtisanHubs.Data.Repositories.Products.Interfaces;
using ArtisanHubs.DTOs.DTO.Reponse.Carts;
using ArtisanHubs.DTOs.DTO.Request.Carts;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.Bussiness.Services.Carts.Implements
{
    public class CartService : ICartService
    {

        private readonly ICartRepository _cartRepository;
        private readonly IMapper _mapper;
        private readonly IProductRepository _productRepository;
        private readonly IArtistProfileService _artistProfileService;
        public CartService(ICartRepository cartRepository, IMapper mapper, IProductRepository productRepository, IArtistProfileService artistProfileService)
        {
            _cartRepository = cartRepository;
            _mapper = mapper;
            _productRepository = productRepository;
            _artistProfileService = artistProfileService;
        }

        public async Task<ApiResponse<CartResponse?>> AddToCartAsync(int accountId, AddToCartRequest request)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(request.ProductId);
                if (product == null || product.Status != "Available")
                {
                    return ApiResponse<CartResponse?>.FailResponse("Product not found or not available.", 404);
                }

                var cart = await _cartRepository.GetCartByAccountIdAsync(accountId);

                if (cart == null)
                {
                    cart = new Cart { AccountId = accountId };
                    await _cartRepository.CreateCartAsync(cart);

                    cart = await _cartRepository.GetCartByAccountIdAsync(accountId);
                }

                var existingItem = cart.CartItems.FirstOrDefault(item => item.ProductId == request.ProductId);

                int newTotalQuantity = request.Quantity;
                if (existingItem != null)
                {
                    newTotalQuantity += existingItem.Quantity;
                }

                if (product.StockQuantity < newTotalQuantity)
                {
                    return ApiResponse<CartResponse?>.FailResponse($"Not enough stock. Only {product.StockQuantity} items available.", 400); 
                }

                if (existingItem != null)
                {
                    existingItem.Quantity += request.Quantity;
                }
                else
                {
                    var newItem = new CartItem
                    {
                        ProductId = request.ProductId,
                        Quantity = request.Quantity,
                        AddedAt = DateTime.UtcNow
                    };
                    cart.CartItems.Add(newItem);
                }

                cart.UpdatedAt = DateTime.UtcNow;
                await _cartRepository.UpdateCartAsync(cart);

                var updatedCart = await _cartRepository.GetCartByAccountIdAsync(accountId);
                var response = _mapper.Map<CartResponse>(updatedCart);

                response.TotalPrice = response.Items.Sum(i => i.Price * i.Quantity);

                return ApiResponse<CartResponse?>.SuccessResponse(response, "Product added to cart successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<CartResponse?>.FailResponse($"Error adding product to cart: {ex.Message}", 500);
            }
        }

        public async Task<Cart?> GetCartByIdAsync(int cartId)
        {
            return await _cartRepository.GetCartByIdAsync(cartId);
        }

        public async Task<ApiResponse<CartResponse?>> GetCartByAccountIdAsync(int accountId)
        {
            try
            {
                var cart = await _cartRepository.GetCartByAccountIdAsync(accountId);

                if (cart == null)
                {
                    var emptyCartResponse = new CartResponse
                    {
                        CartId = 0,
                        Items = new List<CartItemResponse>(),
                        TotalPrice = 0
                    };
                    return ApiResponse<CartResponse?>.SuccessResponse(emptyCartResponse, "User has an empty cart.");
                }

                var cartResponse = _mapper.Map<CartResponse>(cart);

                cartResponse.TotalPrice = cart.CartItems.Sum(item =>
                (item.Product.DiscountPrice ?? item.Product.Price) * item.Quantity
            );

                foreach (var itemResponse in cartResponse.Items)
                {
                    var productInCart = cart.CartItems
                                            .First(ci => ci.ProductId == itemResponse.ProductId)
                                            .Product;
                    itemResponse.Price = productInCart.DiscountPrice ?? productInCart.Price;
                }

                return ApiResponse<CartResponse?>.SuccessResponse(cartResponse);
            }
            catch (Exception ex)
            {
                return ApiResponse<CartResponse?>.FailResponse($"Error retrieving cart: {ex.Message}", 500);
            }
        }

    }
}
