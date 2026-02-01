Nhỏ gọn — cửa hàng điện tử mẫu trên .NET 6  với chức năng giỏ hàng, đặt hàng, email xác nhận và tích hợp thanh toán Momo / VNPay.

## Tính năng chính
- Giỏ hàng, tăng/giảm/xóa sản phẩm
- Trang thanh toán (shipping, coupon, phí ship)
- Tạo đơn hàng, lưu Order + OrderDetails
- Gửi email xác nhận đơn hàng
- Thanh toán: Tiền mặt, Momo (redirect + IPN với xác thực), VNPay (redirect + IPN)
- Quản lý người dùng bằng Identity
- API/Controller xử lý return/notify của cổng thanh toán
- Views Razor cho UI, bộ controller cho admin

## Yêu cầu
- .NET 6 SDK
- SQL Server 
- Visual Studio 2022 hoặc dotnet CLI
- (Khi test thanh toán) ngrok hoặc public HTTPS endpoint

## Cài đặt & chạy nhanh
1. Clone repo
2. Cập nhật cấu hình:
   - Mở `appsettings.json` và set:
     - `ConnectionStrings:ConnectedDb`
     - `PaymentProviders` (Momo / VnPay): Endpoint, keys, ReturnUrl, NotifyUrl
     - `GoogleKeys` nếu dùng OAuth
   - Đối với development, thay secrets bằng placeholder và lưu secrets thật vào __User Secrets__ hoặc biến môi trường.

3. Restore & build:
   - dotnet restore
   - dotnet build

4. Migrations → tạo & áp dụng:
   - CLI:
     - dotnet ef migrations add AddPaymentFieldsToOrder
     - dotnet ef database update
   - Hoặc trong Visual Studio mở __Package Manager Console__ và chạy:
     - __Add-Migration__ AddPaymentFieldsToOrder
     - __Update-Database__


## Test thanh toán (local)
- Dùng ngrok: `ngrok http 5000` (hoặc port app đang chạy).
- Cập nhật `ReturnUrl` / `NotifyUrl` trong `appsettings.json` với URL ngrok.
- Dùng sandbox credentials của Momo/VNPay.
