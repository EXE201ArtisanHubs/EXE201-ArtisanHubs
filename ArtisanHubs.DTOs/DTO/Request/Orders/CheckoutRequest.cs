namespace ArtisanHubs.DTOs.DTO.Request.Orders
{
    public class CheckoutRequest
    {
        public int AccountId { get; set; }

        public List<int> CartItemIds { get; set; }
        public AddressDto ShippingAddress { get; set; }
    }

    public class AddressDto
    {
        public string RecipientName { get; set; }
        public string Phone { get; set; }
        public string Street { get; set; }
        public string Ward { get; set; }
        public string District { get; set; }
        public string Province { get; set; }
    }
}