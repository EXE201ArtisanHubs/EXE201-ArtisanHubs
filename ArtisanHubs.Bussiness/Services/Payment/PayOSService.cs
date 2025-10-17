using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ArtisanHubs.DTOs.DTO.Reponse;
using ArtisanHubs.DTOs.DTO.Request;
using Microsoft.Extensions.Configuration;
using Net.payOS;
using Net.payOS.Types;

namespace ArtisanHubs.Bussiness.Services.Payment
{
    public class PayOSService
    {
        private readonly string _clientId;
        private readonly string _apiKey;
        private readonly string _checksumKey;
        private readonly HttpClient _httpClient;
        private readonly PayOS _payOS;

        public PayOSService(IConfiguration configuration)
        {
            var clientId = configuration["PayOS:ClientId"];
            var apiKey = configuration["PayOS:ApiKey"];
            var checksumKey = configuration["PayOS:ChecksumKey"];

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api-merchant.payos.vn/")
            };

            _payOS = new PayOS(_clientId, _apiKey, _checksumKey);
        }

        public async Task<CreatePaymentResult> CreatePaymentLinkAsync(
            string orderCode, decimal amount, string description, string returnUrl, string cancelUrl)
        {
            // Fix: Provide a value for the 'items' parameter, which is required by the PaymentData constructor.
            var paymentData = new PaymentData(
                orderCode: long.Parse(orderCode), // Assuming orderCode is convertible to long
                amount: (int)amount,
                description: description,
                items: new List<ItemData>(), // Providing an empty list for 'items'
                cancelUrl: cancelUrl,
                returnUrl: returnUrl,
                signature: null,
                buyerName: null,
                buyerEmail: null,
                buyerPhone: null,
                buyerAddress: null,
                expiredAt: null
            );

            var result = await _payOS.createPaymentLink(paymentData);
            return result;
        }
    }
}
