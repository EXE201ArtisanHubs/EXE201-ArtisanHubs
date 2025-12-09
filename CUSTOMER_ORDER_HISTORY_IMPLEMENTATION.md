# Customer Order History Implementation

## Overview
Implemented customer order history feature allowing customers to view their purchase history and order status.

## Implementation Date
January 2025

## Features Implemented

### 1. Customer Order DTO (CustomerOrderResponse.cs)
**Location:** `ArtisanHubs.DTOs/DTO/Reponse/Order/CustomerOrderResponse.cs`

**Classes:**
- `CustomerOrderResponse` - Main order information for customer view
- `CustomerOrderItemResponse` - Individual product line items with artist details

**Key Properties:**
```csharp
CustomerOrderResponse:
- OrderId, OrderCode, OrderDate
- Status, PaymentMethod
- ShippingFee, TotalAmount, ShippingAddress
- List<CustomerOrderItemResponse> OrderItems
- TotalItems, SubTotal
- CreatedAt, UpdatedAt

CustomerOrderItemResponse:
- OrderDetailId, ProductId, ProductName, ProductImage
- Quantity, UnitPrice, TotalPrice
- ArtistId, ArtistName
```

### 2. Service Layer (OrderService.cs)
**Location:** `ArtisanHubs.Bussiness/Services/OrderService.cs`

**New Methods:**

#### GetMyOrdersAsync
```csharp
public async Task<ApiResponse<IPaginate<CustomerOrderResponse>>> GetMyOrdersAsync(
    int accountId, 
    int page = 1, 
    int size = 10, 
    string searchTerm = null, 
    string status = null)
```

**Features:**
- Pagination support (page, size)
- Filter by order status
- Search by order code or product name
- Returns orders with all product details and artist information
- Calculates SubTotal and TotalItems for each order
- Orders sorted by CreatedAt descending (newest first)

**Query Logic:**
```csharp
- WHERE AccountId = accountId
- Include Orderdetails → Product → Artist
- Optional: Status filter (case-insensitive)
- Optional: Search by OrderCode or Product.Name (case-insensitive)
```

#### GetMyOrderDetailAsync
```csharp
public async Task<ApiResponse<CustomerOrderResponse>> GetMyOrderDetailAsync(
    int accountId, 
    int orderId)
```

**Features:**
- View single order detail
- Permission validation (order must belong to customer)
- Returns 404 if order not found or doesn't belong to customer
- Full product and artist information included

### 3. API Endpoints (OrderController.cs)
**Location:** `ArtisanHubs.API/Controllers/OrderController.cs`

#### GET /api/order/my-orders
**Authorization:** `[Authorize(Roles = "Customer")]`

**Query Parameters:**
- `page` (int, default: 1) - Page number
- `size` (int, default: 10) - Items per page
- `searchTerm` (string, optional) - Search by order code or product name
- `status` (string, optional) - Filter by order status

**Response:**
```json
{
  "data": {
    "items": [
      {
        "orderId": 1,
        "orderCode": 123456,
        "orderDate": "2025-01-15T10:30:00",
        "status": "Delivered",
        "paymentMethod": "PayOS",
        "shippingFee": 30000,
        "totalAmount": 530000,
        "shippingAddress": "123 Main St, Ho Chi Minh",
        "orderItems": [
          {
            "orderDetailId": 1,
            "productId": 5,
            "productName": "Handmade Ceramic Vase",
            "productImage": "https://...",
            "quantity": 2,
            "unitPrice": 250000,
            "totalPrice": 500000,
            "artistId": 3,
            "artistName": "Artist Name"
          }
        ],
        "totalItems": 2,
        "subTotal": 500000,
        "createdAt": "2025-01-15T10:30:00",
        "updatedAt": "2025-01-16T14:20:00"
      }
    ],
    "page": 1,
    "size": 10,
    "total": 15,
    "totalPages": 2
  },
  "isSuccess": true,
  "message": "Get customer orders successfully",
  "statusCode": 200
}
```

#### GET /api/order/my-orders/{orderId}
**Authorization:** `[Authorize(Roles = "Customer")]`

**Path Parameters:**
- `orderId` (int) - The order ID to retrieve

**Response:**
Same structure as single order in the list above, but without pagination wrapper.

**Error Responses:**
- `404 Not Found` - Order doesn't exist or doesn't belong to customer
- `401 Unauthorized` - Invalid or missing authentication token

## Database Queries

### Order List Query
```sql
SELECT o.*, od.*, p.*, a.*
FROM Orders o
INNER JOIN Orderdetails od ON o.OrderId = od.OrderId
INNER JOIN Products p ON od.ProductId = p.ProductId
INNER JOIN Artistprofiles a ON p.ArtistId = a.ArtistId
WHERE o.AccountId = @accountId
  AND (@status IS NULL OR LOWER(o.Status) = LOWER(@status))
  AND (@searchTerm IS NULL OR 
       o.OrderCode LIKE '%' + @searchTerm + '%' OR
       LOWER(p.Name) LIKE '%' + LOWER(@searchTerm) + '%')
ORDER BY o.CreatedAt DESC
OFFSET (@page - 1) * @size ROWS
FETCH NEXT @size ROWS ONLY
```

### Order Detail Query
```sql
SELECT o.*, od.*, p.*, a.*
FROM Orders o
INNER JOIN Orderdetails od ON o.OrderId = od.OrderId
INNER JOIN Products p ON od.ProductId = p.ProductId
INNER JOIN Artistprofiles a ON p.ArtistId = a.ArtistId
WHERE o.OrderId = @orderId
  AND o.AccountId = @accountId
```

## Security Features

1. **Role-Based Authorization**: Only customers can access their orders
2. **Account Validation**: JWT token provides AccountId through ClaimTypes.NameIdentifier
3. **Ownership Verification**: Orders filtered by AccountId, preventing cross-customer access
4. **Token Validation**: Invalid or expired tokens return 401 Unauthorized

## Usage Examples

### Customer Views All Orders
```bash
curl -X GET "https://api.example.com/api/order/my-orders?page=1&size=10" \
  -H "Authorization: Bearer {jwt_token}"
```

### Customer Searches Orders
```bash
curl -X GET "https://api.example.com/api/order/my-orders?searchTerm=vase&page=1&size=10" \
  -H "Authorization: Bearer {jwt_token}"
```

### Customer Filters by Status
```bash
curl -X GET "https://api.example.com/api/order/my-orders?status=Delivered&page=1&size=10" \
  -H "Authorization: Bearer {jwt_token}"
```

### Customer Views Order Detail
```bash
curl -X GET "https://api.example.com/api/order/my-orders/123" \
  -H "Authorization: Bearer {jwt_token}"
```

## Order Status Values
Common status values in the system:
- `Pending` - Order placed, awaiting payment
- `Paid` - Payment confirmed
- `Processing` - Order being prepared
- `Shipping` - Out for delivery
- `Delivered` - Successfully delivered
- `Cancelled` - Order cancelled
- `Returned` - Order returned

## Differences from Artist Order View

| Feature | Customer View | Artist View |
|---------|--------------|-------------|
| **Filter By** | Own orders (AccountId) | Products sold (Product.ArtistId) |
| **Commission Details** | ❌ Not shown | ✅ Full commission breakdown |
| **Customer Info** | ❌ Not needed (own info) | ✅ Customer name, email, phone |
| **Artist Info** | ✅ Artist name per product | ❌ Not shown (own info) |
| **Payment Status** | ✅ Order status | ✅ Commission payment status |
| **Calculations** | SubTotal + ShippingFee | Commission amounts, ArtistEarnings |

## Testing Checklist

- [x] Create CustomerOrderResponse DTO
- [x] Implement GetMyOrdersAsync with pagination
- [x] Implement GetMyOrderDetailAsync with validation
- [x] Add API endpoints to OrderController
- [x] Apply Customer role authorization
- [x] Extract AccountId from JWT claims
- [x] Handle invalid order access (404)
- [x] Support search by order code and product name
- [x] Support filter by status
- [x] Calculate SubTotal and TotalItems correctly

## Integration Points

### Frontend Integration
```typescript
// Customer Order List
interface CustomerOrderListRequest {
  page: number;
  size: number;
  searchTerm?: string;
  status?: string;
}

// Customer Order Response
interface CustomerOrder {
  orderId: number;
  orderCode: number;
  orderDate: string;
  status: string;
  paymentMethod: string;
  shippingFee: number;
  totalAmount: number;
  shippingAddress: string;
  orderItems: CustomerOrderItem[];
  totalItems: number;
  subTotal: number;
  createdAt: string;
  updatedAt: string;
}

interface CustomerOrderItem {
  orderDetailId: number;
  productId: number;
  productName: string;
  productImage: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  artistId: number;
  artistName: string;
}
```

## Notes
- No interface file (IOrderService.cs) found - OrderService used directly in controller
- Pre-existing nullable reference type warnings in other files (not related to this feature)
- Feature follows same pattern as Artist order history for consistency
- Uses Entity Framework Core with AsNoTracking for read-only queries
- AutoMapper not needed - manual mapping for better control over nested structures

## Related Files
- `CustomerOrderResponse.cs` - Response DTOs
- `OrderService.cs` - Service implementation
- `OrderController.cs` - API endpoints
- `ArtistOrderResponse.cs` - Parallel feature for artists

## Future Enhancements
- Export order history to PDF/Excel
- Order tracking integration with shipping provider
- Reorder functionality
- Product review after delivery
- Order status notifications
