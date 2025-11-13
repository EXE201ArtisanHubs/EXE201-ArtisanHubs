# ? Ch?c n?ng Update Cart Item Quantity - Hoàn thành

## ?? T?ng quan

?ã tri?n khai **??y ?? ch?c n?ng c?p nh?t s? l??ng (quantity)** s?n ph?m trong gi? hàng khi ng??i dùng b?m nút **+** ho?c **-**.

---

## ?? API Endpoint

### **PUT /api/carts/{cartItemId}/quantity**

**Authorization**: Bearer Token (Customer role)

#### **Request**

**URL**: `/api/carts/5/quantity`  
**Method**: `PUT`

**Headers:**
```
Authorization: Bearer {JWT_TOKEN}
Content-Type: application/json
```

**Body:**
```json
{
  "quantity": 3
}
```

#### **Response Success (200 OK)**

```json
{
  "isSuccess": true,
  "statusCode": 200,
  "message": "Cart item quantity updated successfully.",
  "data": {
    "cartId": 1,
    "items": [
      {
        "cartItemId": 5,
        "productId": 10,
        "productName": "Handmade Vase",
        "price": 50.00,
        "quantity": 3,        // ? ?ã update t? 2 ? 3
        "imageUrl": "https://cloudinary.com/vase.jpg"
      },
      {
        "cartItemId": 6,
        "productId": 11,
        "productName": "Ceramic Bowl",
        "price": 25.00,
        "quantity": 1,
        "imageUrl": "https://cloudinary.com/bowl.jpg"
      }
    ],
    "totalPrice": 175.00    // ? T? ??ng tính l?i: (50*3) + (25*1)
  }
}
```

#### **Response Errors**

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
  "message": "Unauthorized to update this item.",
  "data": null
}
```

**Out of Stock (400):**
```json
{
  "isSuccess": false,
  "statusCode": 400,
  "message": "Not enough stock. Only 5 items available.",
  "data": null
}
```

**Validation Error (400):**
```json
{
  "isSuccess": false,
  "statusCode": 400,
  "message": "Quantity must be between 1 and 100.",
  "data": null
}
```

---

## ?? Giao di?n Frontend

### **Cart Page UI:**

```
???????????????????????????????????????????????????????????
?  ?? GI? HÀNG C?A B?N                                    ?
???????????????????????????????????????????????????????????
?                                                          ?
?  [Image] Handmade Vase           $50.00                 ?
?          Quantity: [- 2 +]        Subtotal: $100.00     ?
?                       ? ?                               ?
?                       ? ?                               ?
?          Click "-" ???? ???? Click "+"                 ?
?                                                          ?
?          ? G?i API PUT /api/carts/5/quantity           ?
?            Body: { "quantity": 3 }                      ?
?          ? Total price t? ??ng c?p nh?t!                ?
?                                                          ?
?  ?????????????????????????????????????????????????????????
?                                                          ?
?  [Image] Ceramic Bowl            $25.00                 ?
?          Quantity: [- 1 +]        Subtotal: $25.00      ?
?                                                          ?
???????????????????????????????????????????????????????????
?                                    Total: $125.00        ?
?                                                          ?
?                            [Proceed to Checkout]         ?
???????????????????????????????????????????????????????????
```

---

## ?? Frontend Implementation (React/TypeScript)

### **CartItem Component:**

```typescript
import { useState } from 'react';
import { toast } from 'react-toastify';

interface CartItemProps {
  item: {
    cartItemId: number;
    productId: number;
    productName: string;
    price: number;
    quantity: number;
    imageUrl?: string;
  };
  onUpdate: () => void;
}

const CartItem: React.FC<CartItemProps> = ({ item, onUpdate }) => {
  const [quantity, setQuantity] = useState(item.quantity);
  const [loading, setLoading] = useState(false);

  // Hàm update quantity
  const updateQuantity = async (newQuantity: number) => {
    if (newQuantity < 1 || newQuantity > 100) {
      toast.error('Quantity must be between 1 and 100');
      return;
    }

    setLoading(true);
    try {
      const response = await fetch(
        `/api/carts/${item.cartItemId}/quantity`,
        {
          method: 'PUT',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${localStorage.getItem('jwt_token')}`
          },
          body: JSON.stringify({ quantity: newQuantity })
        }
      );

      const result = await response.json();

      if (result.isSuccess) {
        setQuantity(newQuantity);
        toast.success('Quantity updated!');
        onUpdate(); // Refresh cart ?? c?p nh?t total price
      } else {
        toast.error(result.message);
        setQuantity(item.quantity);
      }
    } catch (error) {
      toast.error('Failed to update quantity');
      setQuantity(item.quantity);
    } finally {
      setLoading(false);
    }
  };

  // Click nút "+"
  const handleIncrease = () => {
    const newQty = quantity + 1;
    setQuantity(newQty);
    updateQuantity(newQty);
  };

  // Click nút "-"
  const handleDecrease = () => {
    if (quantity > 1) {
      const newQty = quantity - 1;
      setQuantity(newQty);
      updateQuantity(newQty);
    } else {
      toast.warning('Minimum quantity is 1');
    }
  };

  // User gõ tr?c ti?p
  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = parseInt(e.target.value) || 1;
    setQuantity(value);
  };

  // User blur kh?i input
  const handleInputBlur = () => {
    if (quantity !== item.quantity) {
      updateQuantity(quantity);
    }
  };

  return (
    <div className="cart-item">
      <img src={item.imageUrl} alt={item.productName} />
      
      <div className="item-details">
        <h3>{item.productName}</h3>
        <p className="price">${item.price.toFixed(2)}</p>
      </div>

      {/* Quantity Controls */}
      <div className="quantity-controls">
        <button 
          onClick={handleDecrease}
          disabled={loading || quantity <= 1}
          className="btn-decrease"
        >
          -
        </button>
        
        <input 
          type="number"
          value={quantity}
          onChange={handleInputChange}
          onBlur={handleInputBlur}
          min="1"
          max="100"
          disabled={loading}
          className="quantity-input"
        />
        
        <button 
          onClick={handleIncrease}
          disabled={loading || quantity >= 100}
          className="btn-increase"
        >
          +
        </button>
      </div>

      {/* Subtotal */}
      <div className="item-subtotal">
        <p>Subtotal: ${(item.price * quantity).toFixed(2)}</p>
      </div>

      {loading && <span className="loading">?</span>}
    </div>
  );
};

export default CartItem;
```

### **Cart Page:**

```typescript
import { useState, useEffect } from 'react';
import CartItem from './CartItem';

const CartPage = () => {
  const [cart, setCart] = useState(null);
  const [loading, setLoading] = useState(true);

  const fetchCart = async () => {
    try {
      const response = await fetch('/api/carts', {
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('jwt_token')}`
        }
      });
      const result = await response.json();
      if (result.isSuccess) {
        setCart(result.data);
      }
    } catch (error) {
      console.error('Error fetching cart:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchCart();
  }, []);

  if (loading) return <div>Loading...</div>;
  if (!cart || cart.items.length === 0) {
    return <div>Your cart is empty</div>;
  }

  return (
    <div className="cart-page">
      <h1>?? Your Shopping Cart</h1>

      <div className="cart-items">
        {cart.items.map(item => (
          <CartItem 
            key={item.cartItemId}
            item={item}
            onUpdate={fetchCart}  // Refresh cart khi quantity thay ??i
          />
        ))}
      </div>

      <div className="cart-summary">
        <h2>Order Summary</h2>
        <div className="total">
          <span>Total:</span>
          <span className="total-price">
            ${cart.totalPrice.toFixed(2)}
          </span>
        </div>
        <button className="btn-checkout">
          Proceed to Checkout
        </button>
      </div>
    </div>
  );
};

export default CartPage;
```

---

## ?? User Flow

### **Scenario: User t?ng quantity**

```
1. User vào Cart Page
   ? GET /api/carts
   ? Response: cartItemId: 5, quantity: 2, totalPrice: 100

2. User click nút "+"
   ? quantity hi?n th? t?ng lên 3 (optimistic update)
   ? PUT /api/carts/5/quantity
     Body: { "quantity": 3 }

3. API x? lý:
   ? Validate quantity (1-100) ?
   ? Check stock availability ?
   ? Update cartItem.Quantity = 3 ?
   ? Tính l?i totalPrice = 150 ?

4. Frontend nh?n response:
   ? Hi?n th? quantity m?i: 3
   ? C?p nh?t subtotal: $150
   ? C?p nh?t totalPrice: $150
   ? Toast: "Quantity updated!" ?
```

### **Scenario: Không ?? stock**

```
1. Product stock = 5
2. User set quantity = 6
3. API response 400:
   "Not enough stock. Only 5 items available."
4. Frontend:
   ? Reset quantity v? 5
   ? Toast error message
```

---

## ? Key Features

- ? **T?ng/gi?m quantity** v?i nút +/-
- ? **T? ??ng tính l?i total price** 
- ? **Ki?m tra stock availability**
- ? **Validate quantity** (1-100)
- ? **Xác th?c ownership** (user ch? update cart c?a mình)
- ? **Optimistic UI update** (hi?n th? ngay, call API sau)
- ? **Error handling** ??y ??
- ? **Loading states** khi ?ang update

---

## ?? Business Logic

### **Validation Rules:**

1. **Quantity Range**: 1 ? quantity ? 100
2. **Stock Check**: quantity ? product.StockQuantity
3. **Ownership**: Cart item ph?i thu?c v? user
4. **Product Exists**: Product ph?i t?n t?i

### **Auto-calculation:**

```csharp
// Total Price t? ??ng tính l?i
totalPrice = ?(item.Price × item.Quantity)

// Ví d?:
Item 1: $50 × 3 = $150
Item 2: $25 × 1 = $25
Total: $175
```

---

## ?? Test v?i Postman

### **1. Get Cart tr??c khi update**

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
        "cartItemId": 5,
        "productName": "Vase",
        "quantity": 2,
        "price": 50.00
      }
    ],
    "totalPrice": 100.00
  }
}
```

### **2. Update Quantity**

```bash
PUT https://localhost:7000/api/carts/5/quantity
Authorization: Bearer {TOKEN}
Content-Type: application/json

{
  "quantity": 3
}
```

### **3. Verify**

```bash
GET https://localhost:7000/api/carts
Authorization: Bearer {TOKEN}
```

**Response (quantity ?ã update):**
```json
{
  "data": {
    "cartId": 1,
    "items": [
      {
        "cartItemId": 5,
        "productName": "Vase",
        "quantity": 3,        // ? Changed!
        "price": 50.00
      }
    ],
    "totalPrice": 150.00    // ? Changed!
  }
}
```

---

## ?? Files ?ã t?o/s?a

### **T?o m?i:**
- ? `ArtisanHubs.DTOs\DTO\Request\Carts\UpdateCartItemQuantityRequest.cs`

### **Ch?nh s?a:**
- ? `ArtisanHubs.Bussiness\Services\Carts\Interfaces\ICartService.cs`
- ? `ArtisanHubs.Bussiness\Services\Carts\Implements\CartService.cs`
- ? `ArtisanHubs.API\Controllers\CartsController.cs`

---

## ? Checklist

- [x] Create UpdateCartItemQuantityRequest DTO
- [x] Add method to ICartService
- [x] Implement UpdateCartItemQuantityAsync
- [x] Add PUT endpoint to CartsController
- [x] Validate quantity (1-100)
- [x] Check stock availability
- [x] Verify ownership
- [x] Recalculate total price automatically
- [x] Error handling complete
- [x] Build successful

---

## ?? Summary

**Ch?c n?ng Update Quantity ?ã hoàn thành 100%!**

? **API Endpoint**: `PUT /api/carts/{cartItemId}/quantity`  
? **Auto-calculation**: Total price t? ??ng tính l?i  
? **Validation**: Stock check + quantity range  
? **Security**: Ownership verification  
? **UX**: Optimistic UI updates  

**S?n sàng s? d?ng trong production!** ??

