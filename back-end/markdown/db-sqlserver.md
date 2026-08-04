# Plan: Tích hợp SQL Server vào Hệ thống Hotel Booking Microservices (ASP.NET Core)

## 1. Tổng quan
Dự án Hotel Booking được xây dựng theo kiến trúc microservices trên nền tảng ASP.NET Core. Để đảm bảo tính độc lập, cô lập dữ liệu và khả năng mở rộng, mỗi microservice sẽ sở hữu cơ sở dữ liệu riêng (Database per Service pattern). SQL Server được lựa chọn làm hệ quản trị cơ sở dữ liệu chính nhờ khả năng tích hợp sâu với .NET, chi phí hợp lý, hiệu năng cao và sự hỗ trợ mạnh mẽ từ Entity Framework Core.

Mục tiêu của kế hoạch này là:
- Định nghĩa kiến trúc dữ liệu cho từng service.
- Thiết lập quy trình phát triển, quản lý schema và migration.
- Đảm bảo bảo mật, hiệu năng và khả năng vận hành trên môi trường production (đặc biệt là Azure).

## 2. Kiến trúc tổng thể
Kiến trúc microservices của Hotel Booking sẽ bao gồm các service chính:
- **Identity Service**: Quản lý người dùng, xác thực, phân quyền.
- **Hotel & Room Service (Inventory)**: Quản lý thông tin khách sạn, phòng, giá, tình trạng phòng.
- **Booking Service**: Xử lý đặt phòng, kiểm tra khả dụng, lưu lịch sử đặt phòng.
- **Payment Service**: Xử lý thanh toán, hóa đơn.
- **Notification Service**: Gửi email/SMS xác nhận, nhắc nhở.

Mỗi service sẽ có một database SQL Server riêng biệt, **không chia sẻ trực tiếp** bảng dữ liệu với service khác. Việc đồng bộ dữ liệu (nếu cần) được thực hiện thông qua message broker (RabbitMQ, Azure Service Bus) theo mô hình eventual consistency.

## 3. Thiết kế cơ sở dữ liệu cho từng service (SQL Server)

### 3.1. Identity Service
- **Database name**: `HotelIdentityDb`
- **Bảng chính**:
  - `AspNetUsers` (mở rộng từ IdentityUser)
  - `AspNetRoles`, `AspNetUserRoles`, `AspNetRoleClaims`, `AspNetUserClaims`, `AspNetUserLogins`, `AspNetUserTokens`
  - `UserProfiles` (thông tin bổ sung: họ tên, số điện thoại, địa chỉ, quốc tịch)
  - `RefreshTokens` (hỗ trợ JWT refresh)

### 3.2. Hotel & Room Service
- **Database name**: `HotelInventoryDb`
- **Bảng chính**:
  - `Hotels` (Id, Name, Description, Address, StarRating, City, Country, Amenities JSON, ImageUrls JSON)
  - `Rooms` (Id, HotelId, RoomType, BedType, MaxGuests, PricePerNight, Currency)
  - `RoomAvailability` (RoomId, Date, Status [Available/Reserved/Blocked], PriceOverride, ReservedByBookingId – có thể nullable)
  - `RoomAmenities` (RoomId, AmenityId)
  - `Amenities` (Id, Name, Icon)
  - `RoomImages` (RoomId, ImageUrl, IsPrimary)

### 3.3. Booking Service
- **Database name**: `HotelBookingDb`
- **Bảng chính**:
  - `Bookings` (Id, UserId, HotelId, RoomId, CheckInDate, CheckOutDate, NumberOfGuests, Status [Pending/Confirmed/Cancelled/Completed], TotalAmount, Currency, CreatedAt)
  - `BookingGuests` (BookingId, FullName, Age, IsMainGuest)
  - `BookingStatusHistory` (BookingId, Status, ChangedAt, ChangedBy)
  - `OutboxMessages` (dùng cho Transactional Outbox Pattern, lưu các event như BookingCreated, BookingCancelled để gửi qua message broker)

### 3.4. Payment Service
- **Database name**: `HotelPaymentDb`
- **Bảng chính**:
  - `Payments` (Id, BookingId, UserId, Amount, Currency, PaymentMethod, TransactionId, Status, CreatedAt)
  - `Invoices` (Id, BookingId, InvoiceNumber, IssuedDate, TotalAmount, TaxAmount)

### 3.5. Notification Service
- **Database name**: `HotelNotificationDb`
- **Bảng chính**:
  - `Notifications` (Id, UserId, BookingId, Type [Email/SMS], Subject, Body, Status, CreatedAt, SentAt)
  - `NotificationTemplates` (Id, TemplateKey, SubjectTemplate, BodyTemplate)

## 4. Tích hợp SQL Server vào ASP.NET Core

### 4.1. Cấu hình dự án
Mỗi microservice sẽ là một ASP.NET Core Web API riêng, sử dụng .NET 8 (hoặc mới hơn). Các gói NuGet cần thiết:
- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.EntityFrameworkCore.Tools`
- `Microsoft.EntityFrameworkCore.Design`

### 4.2. Connection String & Secrets Management
- **Môi trường phát triển**: dùng User Secrets hoặc file `appsettings.Development.json`.
- **Production**: Sử dụng Azure Key Vault hoặc environment variables để lưu connection string.

Ví dụ cấu hình trong `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "BookingDb": "Server=.;Database=HotelBookingDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "SqlServer": {
    "RetryCount": 3,
    "MaxRetryDelay": "00:00:10"
  }
}