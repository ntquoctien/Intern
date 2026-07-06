# API Convention

## Response format chuẩn

Tất cả endpoint nên trả về format chung:

```json
{
  "success": true,
  "message": "Success",
  "data": {}
}
```

Contract nằm trong `SharedKernel.ApiResponse<T>`:

- `success`: kết quả xử lý.
- `message`: thông báo ngắn.
- `data`: dữ liệu trả về, có thể là object, collection hoặc `null`.

## Quy ước route

Mỗi service dùng base route riêng:

- IdentityService: `/api/identity`
- AcademicService: `/api/academic`
- ExamService: `/api/exam`
- CommunicationService: `/api/communication`

Endpoint hiện tại:

- `GET /api/{service}/health`
- `GET /api/{service}/info`

## Quy ước đặt tên endpoint

- Dùng danh từ số nhiều cho resource CRUD trong tương lai, ví dụ `/api/academic/students`.
- Dùng động từ hoặc trạng thái rõ nghĩa cho endpoint hành động đặc biệt nếu cần, ví dụ `/api/identity/password-resets`.
- Dùng kebab-case cho route nhiều từ.
- Dùng HTTP method đúng ý nghĩa:
  - `GET`: đọc dữ liệu.
  - `POST`: tạo mới hoặc thực hiện action.
  - `PUT`: cập nhật toàn bộ resource.
  - `PATCH`: cập nhật một phần resource.
  - `DELETE`: xóa resource.

## Quy ước pagination tương lai

Các endpoint trả danh sách lớn nên hỗ trợ query:

```http
GET /api/academic/students?pageNumber=1&pageSize=20
```

Response data có thể dùng `PagedResult<T>`:

```json
{
  "success": true,
  "message": "Students retrieved successfully",
  "data": {
    "items": [],
    "pageNumber": 1,
    "pageSize": 20,
    "totalItems": 0,
    "totalPages": 0
  }
}
```

## Quy ước error response tương lai

Khi có lỗi, API nên vẫn dùng `ApiResponse<T>`:

```json
{
  "success": false,
  "message": "Validation failed",
  "data": null
}
```

Gợi ý HTTP status:

- `400 Bad Request`: request không hợp lệ.
- `401 Unauthorized`: chưa đăng nhập hoặc token không hợp lệ.
- `403 Forbidden`: không có quyền.
- `404 Not Found`: không tìm thấy dữ liệu.
- `409 Conflict`: xung đột dữ liệu.
- `500 Internal Server Error`: lỗi hệ thống.

Trong phase hiện tại chưa có validation, authentication hoặc CRUD nên các quy ước lỗi chỉ là định hướng cho phase sau.
