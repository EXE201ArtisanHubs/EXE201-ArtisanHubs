# H??ng d?n Upload ?nh cho Forum Thread

## T?ng quan
Tính n?ng này cho phép ng??i dùng upload ?nh khi t?o m?t bài ??ng (thread) m?i trong di?n ?àn. ?nh s? ???c l?u tr? trên Cloudinary và URL s? ???c l?u trong database.

## Thay ??i ?ã th?c hi?n

### 1. Entity Layer (ArtisanHubs.Data)
- **ForumThread.cs**: Thêm thu?c tính `ImageUrl` ?? l?u URL ?nh t? Cloudinary
```csharp
public string? ImageUrl { get; set; }
```

### 2. DTO Layer (ArtisanHubs.DTOs)
- **CreateForumThreadRequest.cs**: Thêm thu?c tính `ImageFile` ?? nh?n file upload
```csharp
public IFormFile? ImageFile { get; set; }
```

- **ForumThreadResponse.cs**: Thêm thu?c tính `ImageUrl` ?? tr? v? URL ?nh
```csharp
public string? ImageUrl { get; set; }
```

### 3. Business Layer (ArtisanHubs.Bussiness)
- **ForumThreadService.cs**: 
  - Inject `PhotoService` vào constructor
  - Thêm logic upload ?nh trong `CreateThreadAsync`
```csharp
if (request.ImageFile != null)
{
    var imageUrl = await _photoService.UploadImageAsync(request.ImageFile);
    if (!string.IsNullOrEmpty(imageUrl))
    {
        threadEntity.ImageUrl = imageUrl;
    }
}
```

### 4. API Layer (ArtisanHubs.API)
- **ForumThreadsController.cs**: Thay ??i `[FromBody]` thành `[FromForm]` cho endpoint `CreateThread`
```csharp
[HttpPost]
public async Task<IActionResult> CreateThread([FromForm] CreateForumThreadRequest request)
```

### 5. Database Migration
- T?o migration: `AddImageUrlToForumThread`
- Thêm c?t `ImageUrl` vào b?ng `ForumThreads`

## Cách s? d?ng API

### Endpoint: POST /api/v1/forum-threads

**Headers:**
```
Authorization: Bearer {your_jwt_token}
Content-Type: multipart/form-data
```

**Body (Form Data):**
- `Title` (required): Tiêu ?? bài ??ng
- `InitialPostContent` (required): N?i dung bài ??ng ??u tiên
- `ForumTopicId` (required): ID c?a ch? ?? di?n ?àn
- `ImageFile` (optional): File ?nh c?n upload

### Ví d? v?i cURL:
```bash
curl -X POST "https://your-api-url/api/v1/forum-threads" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -F "Title=Bài ??ng m?u" \
  -F "InitialPostContent=?ây là n?i dung bài ??ng" \
  -F "ForumTopicId=1" \
  -F "ImageFile=@/path/to/your/image.jpg"
```

### Ví d? v?i JavaScript (Fetch API):
```javascript
const formData = new FormData();
formData.append('Title', 'Bài ??ng m?u');
formData.append('InitialPostContent', '?ây là n?i dung bài ??ng');
formData.append('ForumTopicId', '1');
formData.append('ImageFile', fileInput.files[0]); // fileInput là input type="file"

fetch('https://your-api-url/api/v1/forum-threads', {
  method: 'POST',
  headers: {
    'Authorization': 'Bearer YOUR_JWT_TOKEN'
  },
  body: formData
})
.then(response => response.json())
.then(data => console.log(data));
```

## Response Format

### Success Response (201 Created):
```json
{
  "data": {
    "id": 1,
    "title": "Bài ??ng m?u",
    "createdAt": "2025-01-29T10:30:00Z",
    "imageUrl": "https://res.cloudinary.com/your-cloud/image/upload/v123456789/thread_image.jpg",
    "author": {
      "accountId": 5,
      "username": "john_doe"
    },
    "posts": [
      {
        "id": 1,
        "content": "?ây là n?i dung bài ??ng",
        "createdAt": "2025-01-29T10:30:00Z",
        "author": {
          "accountId": 5,
          "username": "john_doe"
        }
      }
    ]
  },
  "message": "Thread created successfully.",
  "statusCode": 201,
  "isSuccess": true
}
```

### Error Response (400 Bad Request):
```json
{
  "data": null,
  "message": "Forum topic not found.",
  "statusCode": 400,
  "isSuccess": false
}
```

## L?u ý quan tr?ng

1. **File Size**: Ki?m tra gi?i h?n kích th??c file trong c?u hình Cloudinary
2. **File Types**: Ch? ch?p nh?n các ??nh d?ng ?nh (jpg, png, gif, etc.)
3. **Authentication**: Ng??i dùng ph?i ??ng nh?p ?? t?o thread
4. **Image Optional**: ?nh là tùy ch?n, có th? t?o thread không có ?nh
5. **Cloud Storage**: ?nh ???c l?u trên Cloudinary, không l?u tr?c ti?p trên server

## C?u hình Cloudinary

??m b?o r?ng `appsettings.json` ?ã có c?u hình Cloudinary:
```json
{
  "CloudinarySettings": {
    "CloudName": "your-cloud-name",
    "ApiKey": "your-api-key",
    "ApiSecret": "your-api-secret"
  }
}
```

## Testing

?? test tính n?ng này:
1. ??m b?o b?n có JWT token h?p l?
2. Chu?n b? m?t file ?nh
3. S? d?ng Postman ho?c Swagger UI ?? g?i request
4. Ki?m tra response có ch?a `imageUrl`
5. Truy c?p URL ?? xác nh?n ?nh ?ã ???c upload thành công

## Troubleshooting

**L?i 401 Unauthorized:**
- Ki?m tra JWT token có h?p l? không
- Ki?m tra header Authorization có ?úng format không

**L?i 400 Bad Request:**
- Ki?m tra `ForumTopicId` có t?n t?i không
- Ki?m tra các tr??ng required ?ã ???c ?i?n ?? ch?a

**Upload ?nh th?t b?i:**
- Ki?m tra c?u hình Cloudinary
- Ki?m tra kích th??c và ??nh d?ng file
- Ki?m tra log ?? xem l?i chi ti?t

## M? r?ng t??ng lai

1. Thêm validation cho file type và size
2. Thêm tính n?ng resize ?nh t? ??ng
3. Thêm tính n?ng c?p nh?t ?nh cho thread ?ã t?o
4. Thêm tính n?ng xóa ?nh kh?i Cloudinary khi xóa thread
5. H? tr? nhi?u ?nh cho m?t thread
