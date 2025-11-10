# ? Ch?c n?ng Remove Item from Cart - ?ã hoàn thành

## ?? T?ng quan

?ã tri?n khai ??y ?? ch?c n?ng **xóa item kh?i gi? hàng** theo ?úng c?u trúc c?a project ArtisanHubs.

---

## ?? Files ?ã t?o/ch?nh s?a

### 1. **Repository Layer**

#### `ArtisanHubs.Data\Repositories\Carts\Interfaces\ICartRepository.cs`
```csharp
// Thêm 2 methods m?i:
Task<CartItem?> GetCartItemByIdAsync(int cartItemId);
Task RemoveCartItemAsync(CartItem cartItem);
```

#### `ArtisanHubs.Data\Repositories\Carts\Implements\CartRepository.cs`
```csharp
// Implement 2 methods:
public async Task<CartItem?> GetCartItemByIdAsync(int cartItemId)
{
    return await _context.CartItems
        .Include(ci => ci.Cart)
        .FirstOrDefaultAsync(ci => ci.Id == cartItemId);
}

public async Task RemoveCartItemAsync(CartItem cartItem)
{
    _context.CartItems.Remove(cartItem);
    await _context.SaveChangesAsync();
}
```

---

### 2. **Service Layer**

#### `ArtisanHubs.Bussiness\Services\Carts\Interfaces\ICartService.cs`
```csharp
// Thêm method:
Task<ApiResponse<CartResponse>> RemoveFromCartAsync(int accountId, int cartItemId);
```

#### `ArtisanHubs.Bussiness\Services\Carts\Implements\CartService.cs`
```csharp
public async Task<ApiResponse<CartResponse?>> RemoveFromCartAsync(int accountId, int cartItemId)
{
    // Logic:
    // 1. Get cart item by ID
    // 2. Verify ownership (item belongs to user's cart)
    // 3. Remove cart item
    // 4. Update cart timestamp
    // 5. Return updated cart with recalculated total price
}
```

**Features:**
- ? Ki?m tra cart item t?n t?i
- ? Xác th?c quy?n s? h?u (user ch? xóa ???c item trong cart c?a mình)
- ? Xóa item kh?i database
- ? C?p nh?t th?i gian update c?a cart
- ? Tính l?i t?ng giá tr? cart
- ? X? lý tr??ng h?p cart r?ng sau khi xóa
- ? Error handling ??y ??

---

### 3. **Controller Layer**

#### `ArtisanHubs.API\Controllers\CartsController.cs`
```csharp
/// <summary>
/// Remove item from cart
/// </summary>
/// <param name="cartItemId">ID c?a cart item c?n xóa</param>
/// <returns>Cart sau khi xóa item</returns>
[HttpDelete("{cartItemId}")]
[Authorize(Roles = "Customer")]
public async Task<IActionResult> RemoveFromCart(int cartItemId)
{
    var accountId = GetCurrentAccountId();
    var result = await _cartService.RemoveFromCartAsync(accountId, cartItemId);
    return StatusCode(result.StatusCode, result);
}
```

---

## ?? API Endpoint

### **DELETE /api/carts/{cartItemId}**

**Method**: `DELETE`  
**Authorization**: Bearer Token (Customer role)  
**Route**: `/api/carts/{cartItemId}`

#### **Request**

**Path Parameter:**
- `cartItemId` (int, required): ID c?a cart item c?n xóa

**Headers:**
```
Authorization: Bearer {JWT_TOKEN}
```

#### **Response**

**Success (200 OK):**
```json
{
  "isSuccess": true,
  "statusCode": 200,
  "message": "Item removed from cart successfully.",
  "data": {
    "cartId": 1,
    "items": [
      {
        "cartItemId": 2,
        "productId": 10,
        "productName": "Handmade Vase",
        "price": 50.00,
        "quantity": 2,
        "imageUrl": "https://cloudinary.com/image.jpg"
      }
    ],
    "totalPrice": 100.00
  }
}
```

**Cart Empty (200 OK):**
```json
{
  "isSuccess": true,
  "statusCode": 200,
  "message": "Item removed successfully. Cart is now empty.",
  "data": {
    "cartId": 1,
    "items": [],
    "totalPrice": 0
  }
}
```

**Not Found (404):**
```json
{
  "isSuccess": false,
  "statusCode": 404,
  "message": "Cart item not found.",
  "data": null
}
```

**Unauthorized (403):**
```json
{
  "isSuccess": false,
  "statusCode": 403,
  "message": "Unauthorized to remove this item.",
  "data": null
}
```

**Error (500):**
```json
{
  "isSuccess": false,
  "statusCode": 500,
  "message": "Error removing item from cart: {error details}",
  "data": null
}
```

---

## ?? Testing

### **1. Test v?i Postman/cURL**

#### Get Cart tr??c khi xóa
```bash
GET https://localhost:7000/api/carts
Authorization: Bearer {TOKEN}
```

**Response:**
```json
{
  "data": {
    "cartId": 1,
    "items": [
      {
        "cartItemId": 5,  // ? L?y ID này
        "productId": 10,
        "productName": "Product A",
        "quantity": 2,
        "price": 25.00
      },
      {
        "cartItemId": 6,
        "productId": 11,
        "productName": "Product B",
        "quantity": 1,
        "price": 50.00
      }
    ],
    "totalPrice": 100.00
  }
}
```

#### Remove Item
```bash
DELETE https://localhost:7000/api/carts/5
Authorization: Bearer {TOKEN}
```

#### Verify
```bash
GET https://localhost:7000/api/carts
Authorization: Bearer {TOKEN}
```

**Response (item ?ã b? xóa):**
```json
{
  "data": {
    "cartId": 1,
    "items": [
      {
        "cartItemId": 6,
        "productId": 11,
        "productName": "Product B",
        "quantity": 1,
        "price": 50.00
      }
    ],
    "totalPrice": 50.00  // ? T?ng giá ?ã gi?m
  }
}
```

---

### **2. Test Cases**

#### ? Test Case 1: Remove item thành công
```
GIVEN user có cart v?i 2 items
WHEN user xóa 1 item
THEN item b? xóa kh?i cart
AND total price ???c tính l?i
AND response tr? v? cart updated
```

#### ? Test Case 2: Remove item cu?i cùng (cart empty)
```
GIVEN user có cart v?i 1 item
WHEN user xóa item ?ó
THEN cart tr? thành empty
AND items = []
AND totalPrice = 0
```

#### ? Test Case 3: Remove item không t?n t?i
```
GIVEN cartItemId không h?p l?
WHEN user g?i DELETE /api/carts/{invalidId}
THEN response 404 Not Found
```

#### ? Test Case 4: Remove item c?a user khác
```
GIVEN user A có cart v?i item X
WHEN user B c? xóa item X
THEN response 403 Forbidden
```

#### ? Test Case 5: Không có JWT token
```
GIVEN user ch?a ??ng nh?p
WHEN user g?i DELETE /api/carts/{id}
THEN response 401 Unauthorized
```

---

## ?? Security Features

1. **Authorization**: Ch? Customer role ???c phép
2. **Ownership Verification**: User ch? xóa ???c items trong cart c?a mình
3. **JWT Authentication**: Yêu c?u Bearer token h?p l?
4. **Input Validation**: Ki?m tra cartItemId h?p l?

---

## ?? Business Logic Flow

```
Client Request
    ?
[DELETE /api/carts/{cartItemId}]
    ?
CartsController.RemoveFromCart()
    ?
GetCurrentAccountId() ? Extract accountId from JWT
    ?
CartService.RemoveFromCartAsync(accountId, cartItemId)
    ?
????????????????????????????????????????
? 1. Get cart item by ID               ?
? 2. Verify cart item exists           ?
? 3. Get user's cart                   ?
? 4. Verify ownership                  ?
? 5. Remove cart item from DB          ?
? 6. Update cart.UpdatedAt             ?
? 7. Get updated cart                  ?
? 8. Calculate new total price         ?
? 9. Map to CartResponse                ?
????????????????????????????????????????
    ?
Return ApiResponse<CartResponse>
    ?
Client receives updated cart
```

---

## ?? Related Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/carts` | GET | L?y cart c?a user hi?n t?i |
| `/api/carts` | POST | Thêm product vào cart |
| `/api/carts/{cartItemId}` | DELETE | **Xóa item kh?i cart** ? NEW |

---

## ?? Database Impact

### **Before Delete:**
```sql
CartItems table:
| Id | CartId | ProductId | Quantity |
|----|--------|-----------|----------|
| 5  | 1      | 10        | 2        |
| 6  | 1      | 11        | 1        |
```

### **After DELETE /api/carts/5:**
```sql
CartItems table:
| Id | CartId | ProductId | Quantity |
|----|--------|-----------|----------|
| 6  | 1      | 11        | 1        |  ? Item 5 ?ã b? xóa

Carts table:
| Id | AccountId | UpdatedAt           |
|----|-----------|---------------------|
| 1  | 10        | 2024-01-20 10:30:00 | ? UpdatedAt ???c c?p nh?t
```

---

## ?? Usage Example (Frontend)

### **React/TypeScript Example**

```typescript
// services/cartService.ts
export const removeFromCart = async (cartItemId: number) => {
  const token = localStorage.getItem('jwt_token');
  
  const response = await fetch(`https://localhost:7000/api/carts/${cartItemId}`, {
    method: 'DELETE',
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });
  
  return await response.json();
};

// components/CartItem.tsx
const CartItem = ({ item }) => {
  const handleRemove = async () => {
    try {
      const result = await removeFromCart(item.cartItemId);
      
      if (result.isSuccess) {
        toast.success('Item removed from cart!');
        // Refresh cart data
        refreshCart();
      }
    } catch (error) {
      toast.error('Failed to remove item');
    }
  };

  return (
    <div className="cart-item">
      <img src={item.imageUrl} alt={item.productName} />
      <h3>{item.productName}</h3>
      <p>Quantity: {item.quantity}</p>
      <p>Price: ${item.price}</p>
      <button onClick={handleRemove}>
        ??? Remove
      </button>
    </div>
  );
};
```

---

## ? Verification Checklist

- [x] Build successful
- [x] Repository methods implemented
- [x] Service logic implemented
- [x] Controller endpoint added
- [x] Authorization configured
- [x] Error handling complete
- [x] Ownership verification
- [x] Total price recalculation
- [x] Empty cart handling
- [x] Documentation created

---

## ?? Summary

**Ch?c n?ng Remove Item from Cart ?ã ???c tri?n khai hoàn ch?nh!**

? **Features:**
- Xóa item kh?i cart an toàn
- Ki?m tra quy?n s? h?u
- T? ??ng tính l?i t?ng giá
- X? lý cart r?ng
- Error handling ??y ??

? **Security:**
- JWT Authentication
- Role-based authorization
- Ownership verification

? **Code Quality:**
- Theo ?úng c?u trúc project
- Clean code patterns
- Proper error messages
- Comprehensive validation

**Ready to use in production!** ??

