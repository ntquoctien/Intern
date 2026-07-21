# Student Portal — Đăng nhập

**Route đề xuất:** `/student/login`  
**Mục tiêu:** xác thực sinh viên và đưa vào không gian tra cứu cá nhân.

## Bố cục

- Desktop: màn hình chia hai cột; bên trái là nhận diện trường và thông tin hỗ trợ, bên phải là login card rộng khoảng 400–440 px.
- Mobile: một cột, logo và login card chiếm toàn bộ chiều rộng có padding 20–24 px.
- Nền dùng màu thương hiệu nhẹ, không dùng ảnh làm giảm độ tương phản.

## Nội dung hiển thị

- Logo, tên trường và nhãn “Cổng thông tin sinh viên”.
- Tiêu đề “Đăng nhập”, mô tả ngắn về tài khoản được sử dụng.
- Trường mã sinh viên/tài khoản.
- Trường mật khẩu hoặc nút đăng nhập SSO sau khi cơ chế production được phê duyệt.
- Checkbox ghi nhớ phiên nếu chính sách cho phép.
- Nút “Đăng nhập”, link “Cần hỗ trợ đăng nhập”.
- Thông báo môi trường nội bộ nếu hệ thống chưa dùng authentication production.

## Trạng thái và tương tác

- Validate bắt buộc tại field; không báo tài khoản có tồn tại hay không.
- Nút có loading và bị khóa trong khi gửi.
- Lỗi dùng thông điệp chung: “Thông tin đăng nhập không hợp lệ”.
- Rate limit hiển thị thời gian thử lại; phiên hết hạn quay về trang này với banner phù hợp.
- Enter gửi form; focus đầu tiên ở mã sinh viên; hỗ trợ password manager.

## Dữ liệu và giới hạn

- Nguồn nhận diện: `identity.Users`, đối chiếu `academic.Students`.
- Phải chốt một mã chính thức giữa `Nickname`, `UserInternalId` và `UserName`.
- Không hiển thị hoặc gửi về client `PasswordHash`, `PasswordSalt`.
- Login bằng mã sinh viên đơn thuần chỉ phù hợp demo/nội bộ, không đủ cho production.

