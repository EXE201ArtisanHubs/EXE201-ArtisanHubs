using System.Net.Http.Headers;
using System.Text.Json;
using ArtisanHubs.DTOs.DTO.Reponse;
using Microsoft.Extensions.Configuration;

public class GHTKService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _apiKey;

    public GHTKService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _baseUrl = config["GHTK:BaseUrl"]!;
        _apiKey = config["GHTK:ApiKey"]!;
    }

    public async Task<GHTKFeeResponse?> GetShippingFeeAsync(
        string pickProvince, string pickDistrict,
        string province, string district,
        string address, int weight, int value,
        string transport = "road")
    {
        // Tạo URL
        var url = $"{_baseUrl}/shipment/fee?" +
                  $"pick_province={Uri.EscapeDataString(pickProvince)}&" +
                  $"pick_district={Uri.EscapeDataString(pickDistrict)}&" +
                  $"province={Uri.EscapeDataString(province)}&" +
                  $"district={Uri.EscapeDataString(district)}&" +
                  $"address={Uri.EscapeDataString(address)}&" +
                  $"weight={weight}&" +
                  $"value={value}&" +
                  $"transport={transport}";

        // Thiết lập header chính xác cho GHTK
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Token", _apiKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        // Gửi request
        var response = await _httpClient.GetAsync(url);

        // Nếu thất bại, ném ngoại lệ
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"GHTK API Error {(int)response.StatusCode}: {errorContent}");
        }

        // Đọc nội dung JSON
        var content = await response.Content.ReadAsStringAsync();

        // Deserialize về đối tượng C#
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var result = JsonSerializer.Deserialize<GHTKFeeResponse>(content, options);

        return result;
    }
}
