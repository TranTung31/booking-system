<!-- @format -->

# Plan: Tích hợp CQRS (MediatR) vào IdentityService

## 1. Tổng quan

**IdentityService** hiện đang được xây dựng theo kiến trúc phân lớp (layered architecture):

- `IdentityService.Api` — tầng trình diễn, chỉ chứa `Controllers`.
- `IdentityService.Application` — tầng ứng dụng (hiện chỉ có `Class1.cs`, chưa có logic nghiệp vụ).
- `IdentityService.Contracts` — chứa các DTO / message chia sẻ.
- `IdentityService.Domain` — entity, interface repository, `BaseEntity`.
- `IdentityService.Infrastructure` — EF Core (`AppIdentityDbContext`), Identity, `EfRepository`, `UserRepository`, migration Oracle.

Mục tiêu của plan này là **đưa logic nghiệp vụ từ Controller về tầng Application** thông qua mô hình **CQRS với MediatR**, theo đúng kiến trúc chuẩn mà các project microservices .NET hiện đại đang dùng (thường gọi là "Vertical Slice" / "Clean Architecture").

Vì dự án dùng **quản lý phiên bản package tập trung** (Central Package Management — `Directory.Packages.props`), ta chỉ cần thêm `PackageReference` với `Version` rỗng ở tầng Application; phiên bản đã khai báo sẵn `MediatR 14.0.0`.

### 1.1. Kiến trúc trước và sau khi tích hợp CQRS

**Trước (hiện tại):** Controller gọi trực tiếp `UserManager` / repository → logic nằm rải rác, khó test, khó mở rộng.

```
Controller ──> UserRepository / UserManager ──> AppIdentityDbContext ──> Oracle
```

**Sau (mục tiêu):** Controller gửi Command/Query qua `ISender` (MediatR), Handler xử lý và gọi repository / domain service.

```
Controller ──> ISender (MediatR)
                  │
                  ├── Command ──> CommandHandler ──> Repository / UserManager ──> DbContext
                  └── Query   ──> QueryHandler  ──> Repository / UserManager ──> DbContext
```

### 1.2. CQRS là gì và vì sao dùng MediatR?

- **CQRS (Command Query Responsibility Segregation)**: tách riêng hai hướng đọc (`Query`) và ghi (`Command`).
  - `Command`: thay đổi trạng thái hệ thống (Create/Update/Delete), **không trả dữ liệu** hoặc chỉ trả ID/status.
  - `Query`: chỉ đọc dữ liệu, **không thay đổi trạng thái**.
- **MediatR**: thư viện triển khai _mediator pattern_, giúp:
  - Controller gọn nhẹ (chỉ 1-2 dòng gọi `ISender`).
  - Tách biệt request (command/query) khỏi xử lý (handler).
  - Hỗ trợ sẵn **pipeline behaviors** (validation, logging, transaction, caching...) — rất hữu ích để áp dụng **FluentValidation** (đã có trong `Directory.Packages.props`) và các cross-cutting concern sau này.
  - Dễ test: từng handler là một unit độc lập.

## 2. Kiến trúc CQRS trong IdentityService

Cấu trúc thư mục đề xuất bên trong `IdentityService.Application`:

```
IdentityService.Application/
├── IdentityService.Application.csproj
├── Commands/
│   ├── User/
│   │   ├── CreateUser/
│   │   │   ├── CreateUserCommand.cs          (IRequest + record)
│   │   │   ├── CreateUserCommandHandler.cs
│   │   │   └── CreateUserCommandValidator.cs (FluentValidation, tùy chọn)
│   │   ├── UpdateUser/
│   │   │   └── ...
│   │   └── DeleteUser/
│   │       └── ...
├── Queries/
│   ├── User/
│   │   ├── GetUserById/
│   │   │   ├── GetUserByIdQuery.cs
│   │   │   └── GetUserByIdQueryHandler.cs
│   │   ├── GetUsersPaged/
│   │   │   └── ...
│   │   └── GetUserByName/
│   │       └── ...
├── Behaviours/
│   ├── LoggingBehaviour.cs           (tùy chọn)
│   └── ValidationBehaviour.cs        (tùy chọn, cần FluentValidation)
├── Common/
│   ├── PagedResult.cs                (DTO trả về phân trang)
│   └── UserDto.cs / UserCreateDto.cs ...
├── DependencyInjection.cs            (đăng ký MediatR, FluentValidation)
└── Exceptions/                       (tùy chọn — exception riêng cho Application)
```

```
Application/
  Users/
    Commands/
      CreateUser/
        CreateUserCommand.cs
        CreateUserCommandHandler.cs
        CreateUserCommandValidator.cs
    Queries/
      GetUserById/
        GetUserByIdQuery.cs
        GetUserByIdQueryHandler.cs
    Mappings/
      UserMappingProfile.cs
    Dtos/
      UserDto.cs
  Products/ ...
  Common/
    Behaviours/ ...
  DependencyInjection.cs
```

**Quy ước quan trọng (convention):**

- Tên file & class theo dạng `<TênNghiệpVụ>Command/Query` + `Handler` + `Validator`.
- `IRequest<TReturn>` cho command/query có trả về dữ liệu; `IRequest<Unit>` (hoặc `IRequest`) cho command không cần kết quả. (MediatR 14 vẫn hỗ trợ `IRequest` → handler trả `Task<Unit>`.)
- DTO dùng `record` để gọn và bất biến (immutable).
- Mỗi feature là một "vertical slice" nhỏ: command/query + handler + validator đặt trong cùng thư mục.

## 3. Cài đặt Package (Central Package Management)

### Bước 1 — Đảm bảo `Directory.Packages.props` có MediatR

Đã có sẵn (xem [Directory.Packages.props](../../Directory.Packages.props)):

```xml
<PackageVersion Include="MediatR" Version="14.0.0" />
<PackageVersion Include="FluentValidation.DependencyInjectionExtensions" Version="12.0.0" />
```

### Bước 2 — Thêm package vào `IdentityService.Application.csproj`

Thêm vào [IdentityService.Application.csproj](../src/Services/IdentityService/IdentityService.Application/IdentityService.Application.csproj):

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <ItemGroup>
    <ProjectReference Include="..\IdentityService.Domain\IdentityService.Domain.csproj" />
    <ProjectReference Include="..\IdentityService.Contracts\IdentityService.Contracts.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="MediatR" />
    <PackageReference Include="FluentValidation.DependencyInjectionExtensions" />
  </ItemGroup>

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

</Project>
```

> **Lưu ý:** Không ghi `Version` vì đã quản lý tập trung ở `Directory.Packages.props`. Nếu chưa dùng CPM thì ghi `Version="14.0.0"`.

### Bước 3 — `IdentityService.Infrastructure` cần tham chiếu MediatR không?

**Không bắt buộc.** Handlers nằm ở `Application`; `Infrastructure` chỉ cung cấp repository. Tuy nhiên nếu sau này muốn xử lý **domain events** trong `SaveChangesAsync` của `AppIdentityDbContext` thì cần inject `IPublisher` (MediatR) vào Infrastructure — sẽ trình bày ở mục **Nâng cao**.

## 4. Đăng ký Dependency Injection

### 4.1. Tạo `DependencyInjection.cs` trong `IdentityService.Application`

```csharp
using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityService.Application;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Đăng ký MediatR, tự quét tất cả handler trong assembly này
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // (Tùy chọn) Đăng ký FluentValidation + pipeline behavior
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}
```

> **MediatR 14**: dùng `cfg.RegisterServicesFromAssembly(...)` (khác với MediatR 12 cũ là `services.AddMediatR(typeof(Program).Assembly)`). Với CPM, `AddMediatR` nằm trong package `MediatR`; các extension DI nằm trong `MediatR` luôn (từ v12 trở đi `MediatR` tự bao gồm DI).

### 4.2. Đăng ký vào `Program.cs` của `IdentityService.Api`

Sửa [Program.cs](../src/Services/IdentityService/IdentityService.Api/Program.cs):

```csharp
using IdentityService.Application;
using IdentityService.Infrastructure;
using IdentityService.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();        // <-- Thêm
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

## 5. Các class cần dùng chung (Common)

Tạo trong `IdentityService.Application/Common/`:

### 5.1. `PagedResult<T>` — DTO kết quả phân trang

Dùng để map từ `IPagedList<TEntity>` (tầng Domain/Infrastructure) sang DTO, tránh leak entity ra ngoài API.

```csharp
namespace IdentityService.Application.Common;

public sealed record PagedResult<T>(
    int PageIndex,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasPreviousPage,
    bool HasNextPage,
    IReadOnlyList<T> Items);
```

### 5.2. `UserDto` — DTO trả về cho user

```csharp
namespace IdentityService.Application.Common;

public sealed record UserDto(
    Guid Id,
    string Username,
    string Email,
    string? FullName,
    DateTime CreatedOnUtc,
    DateTime? UpdatedOnUtc);
```

> Có thể đặt ở `IdentityService.Contracts` nếu DTO cần chia sẻ giữa nhiều service (giao tiếp giữa các microservice). Với DTO dùng nội bộ trong IdentityService, để ở `Application/Common` là hợp lý.

### 5.3. `UserCreateDto` / `UserUpdateDto` — input từ client

```csharp
namespace IdentityService.Application.Common;

public sealed record UserCreateDto(
    string Username,
    string Email,
    string Password,
    string? FullName);

public sealed record UserUpdateDto(
    string? FullName,
    string? Email);
```

## 6. Command layer (Ghi dữ liệu)

### 6.1. Tạo user — `CreateUserCommand`

**`IdentityService.Application/Commands/User/CreateUser/CreateUserCommand.cs`**

```csharp
using IdentityService.Application.Common;
using MediatR;

namespace IdentityService.Application.Commands.User.CreateUser;

public sealed record CreateUserCommand(string Username, string Email, string Password, string? FullName)
    : IRequest<UserDto>;
```

**`CreateUserCommandHandler.cs`**

```csharp
using IdentityService.Application.Common;
using IdentityService.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Application.Commands.User.CreateUser;

public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public CreateUserCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Kiểm tra trùng username/email (có thể đưa vào Validator để rõ ràng hơn)
        if (await _userManager.FindByNameAsync(request.Username) != null)
            throw new InvalidOperationException($"Username '{request.Username}' already exists.");

        var appUser = new ApplicationUser
        {
            UserName = request.Username,
            Email = request.Email,
            FullName = request.FullName,
            EmailConfirmed = true,
        };

        var result = await _userManager.CreateAsync(appUser, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create user: {errors}");
        }

        return new UserDto(
            appUser.Id,
            appUser.UserName ?? string.Empty,
            appUser.Email ?? string.Empty,
            appUser.FullName,
            appUser.CreatedOnUtc,
            appUser.UpdatedOnUtc);
    }
}
```

### 6.2. Cập nhật user — `UpdateUserCommand`

**`IdentityService.Application/Commands/User/UpdateUser/UpdateUserCommand.cs`**

```csharp
using MediatR;

namespace IdentityService.Application.Commands.User.UpdateUser;

public sealed record UpdateUserCommand(Guid Id, string? FullName, string? Email) : IRequest<Unit>;
```

**`UpdateUserCommandHandler.cs`**

```csharp
using IdentityService.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Application.Commands.User.UpdateUser;

public sealed class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Unit>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UpdateUserCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Unit> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id.ToString())
                   ?? throw new InvalidOperationException($"User '{request.Id}' not found.");

        user.FullName = request.FullName;
        user.Email = request.Email;
        user.UserName = request.Email; // (tùy chọn) đồng bộ username = email

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to update user: {errors}");
        }

        return Unit.Value;
    }
}
```

### 6.3. Xóa user — `DeleteUserCommand`

**`IdentityService.Application/Commands/User/DeleteUser/DeleteUserCommand.cs`**

```csharp
using MediatR;

namespace IdentityService.Application.Commands.User.DeleteUser;

public sealed record DeleteUserCommand(Guid Id) : IRequest<Unit>;
```

**`DeleteUserCommandHandler.cs`**

```csharp
using IdentityService.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Application.Commands.User.DeleteUser;

public sealed class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Unit>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public DeleteUserCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id.ToString())
                   ?? throw new InvalidOperationException($"User '{request.Id}' not found.");

        var result = await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to delete user: {errors}");
        }

        return Unit.Value;
    }
}
```

## 7. Query layer (Đọc dữ liệu)

> **Ghi chú quan trọng về `User` domain entity:** hiện `UserRepository` map từ `ApplicationUser` (Identity) sang `User` (domain). `ApplicationUser` là entity thật của EF Core (`AppIdentityDbContext`). Nên các Query handler nên thao tác qua `UserManager<ApplicationUser>` hoặc `AppIdentityDbContext` trực tiếp — nếu đi qua `UserRepository`/`User` domain entity, việc query và map sẽ phức tạp hơn. Dưới đây mình dùng `UserManager` cho nhất quán với các Command.

### 7.1. Lấy user theo Id — `GetUserByIdQuery`

**`IdentityService.Application/Queries/User/GetUserById/GetUserByIdQuery.cs`**

```csharp
using IdentityService.Application.Common;
using MediatR;

namespace IdentityService.Application.Queries.User.GetUserById;

public sealed record GetUserByIdQuery(Guid Id) : IRequest<UserDto?>;
```

**`GetUserByIdQueryHandler.cs`**

```csharp
using IdentityService.Application.Common;
using IdentityService.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Application.Queries.User.GetUserById;

public sealed class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public GetUserByIdQueryHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id.ToString());

        if (user == null)
            return null;

        return new UserDto(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            user.FullName,
            user.CreatedOnUtc,
            user.UpdatedOnUtc);
    }
}
```

### 7.2. Lấy danh sách user phân trang — `GetUsersPagedQuery`

**`IdentityService.Application/Queries/User/GetUsersPaged/GetUsersPagedQuery.cs`**

```csharp
using IdentityService.Application.Common;
using MediatR;

namespace IdentityService.Application.Queries.User.GetUsersPaged;

public sealed record GetUsersPagedQuery(int PageIndex = 1, int PageSize = 10, string? Search = null)
    : IRequest<PagedResult<UserDto>>;
```

**`GetUsersPagedQueryHandler.cs`**

Handler này dùng `AppIdentityDbContext` để query trực tiếp (LINQ to Entities, không tải toàn bộ lên memory):

```csharp
using IdentityService.Application.Common;
using IdentityService.Infrastructure.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Application.Queries.User.GetUsersPaged;

public sealed class GetUsersPagedQueryHandler : IRequestHandler<GetUsersPagedQuery, PagedResult<UserDto>>
{
    private readonly AppIdentityDbContext _context;

    public GetUsersPagedQueryHandler(AppIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<UserDto>> Handle(GetUsersPagedQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(u =>
                u.UserName!.ToLower().Contains(search) ||
                u.Email!.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(u => u.UserName)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new UserDto(
                u.Id,
                u.UserName ?? string.Empty,
                u.Email ?? string.Empty,
                u.FullName,
                u.CreatedOnUtc,
                u.UpdatedOnUtc))
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling((double)totalCount / Math.Max(request.PageSize, 1));

        return new PagedResult<UserDto>(
            request.PageIndex,
            request.PageSize,
            totalCount,
            totalPages,
            request.PageIndex > 1,
            request.PageIndex < totalPages,
            items);
    }
}
```

> Nếu muốn dùng lại `IRepository<T, TKey>` / `IPagedList<T>` đã có sẵn, có thể viết query qua `IUserRepository`/`EfRepository` rồi map sang `PagedResult<UserDto>` — tuy nhiên vì repository hiện làm việc với `User` (domain entity) còn Identity dùng `ApplicationUser`, nên truy vấn trực tiếp `AppIdentityDbContext` là đơn giản và rõ ràng nhất.

### 7.3. Lấy user theo username — `GetUserByNameQuery`

**`IdentityService.Application/Queries/User/GetUserByName/GetUserByNameQuery.cs`**

```csharp
using IdentityService.Application.Common;
using MediatR;

namespace IdentityService.Application.Queries.User.GetUserByName;

public sealed record GetUserByNameQuery(string UserName) : IRequest<UserDto?>;
```

**`GetUserByNameQueryHandler.cs`**

```csharp
using IdentityService.Application.Common;
using IdentityService.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Application.Queries.User.GetUserByName;

public sealed class GetUserByNameQueryHandler : IRequestHandler<GetUserByNameQuery, UserDto?>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public GetUserByNameQueryHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserDto?> Handle(GetUserByNameQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByNameAsync(request.UserName);

        if (user == null)
            return null;

        return new UserDto(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            user.FullName,
            user.CreatedOnUtc,
            user.UpdatedOnUtc);
    }
}
```

## 8. Controller (tầng trình diễn)

Controller trở nên rất gọn — chỉ inject `ISender` và gọi command/query.

### 8.1. `UsersController`

```csharp
using IdentityService.Application.Commands.User.CreateUser;
using IdentityService.Application.Commands.User.DeleteUser;
using IdentityService.Application.Commands.User.UpdateUser;
using IdentityService.Application.Common;
using IdentityService.Application.Queries.User.GetUserById;
using IdentityService.Application.Queries.User.GetUserByName;
using IdentityService.Application.Queries.User.GetUsersPaged;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ISender _sender;

    public UsersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetUserByIdQuery(id), ct);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("{userName}")]
    public async Task<ActionResult<UserDto>> GetByName(string userName, CancellationToken ct)
    {
        var result = await _sender.Send(new GetUserByNameQuery(userName), ct);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<UserDto>>> GetPaged(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(new GetUsersPagedQuery(pageIndex, pageSize, search), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(UserCreateDto dto, CancellationToken ct)
    {
        var command = new CreateUserCommand(dto.Username, dto.Email, dto.Password, dto.FullName);
        var result = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UserUpdateDto dto, CancellationToken ct)
    {
        await _sender.Send(new UpdateUserCommand(id, dto.FullName, dto.Email), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteUserCommand(id), ct);
        return NoContent();
    }
}
```

> **Lưu ý route:** `[HttpGet("{id:guid}")]` và `[HttpGet("{userName}")]` cùng tồn tại sẽ khiến MVC không phân biệt được hai route tham số (cùng `{xxx}`). Nên đặt tên khác hoặc tách controller, ví dụ `[HttpGet("by-name/{userName}")]`. Đây là điểm cần chỉnh khi code thực tế.

## 9. Validation với FluentValidation (tùy chọn)

Đã có sẵn `FluentValidation.DependencyInjectionExtensions 12.0.0` trong `Directory.Packages.props`. Ta có thể:

### 9.1. Tạo `CreateUserCommandValidator`

```csharp
using FluentValidation;
using IdentityService.Application.Commands.User.CreateUser;

namespace IdentityService.Application.Commands.User.CreateUser;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(3).MaximumLength(50);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty().MinimumLength(6)
            .WithMessage("Password must be at least 6 characters.");
    }
}
```

### 9.2. Tạo pipeline behavior `ValidationBehaviour`

```csharp
using FluentValidation;
using MediatR;

namespace IdentityService.Application.Behaviours;

public sealed class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            var results = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = results
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count > 0)
                throw new ValidationException(failures);
        }

        return await next();
    }
}
```

### 9.3. Đăng ký behavior trong `AddApplication`

```csharp
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

    // Đăng ký pipeline behavior theo đúng thứ tự
    cfg.AddOpenBehavior(typeof(LoggingBehaviour<,>));       // chạy đầu tiên
    cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));    // chạy sau logging
});
```

> `LoggingBehaviour` nằm trong `IdentityService.Application/Behaviours/LoggingBehaviour.cs` (xem mục 11).

## 10. Xử lý lỗi tập trung (tùy chọn)

Thay vì throw `InvalidOperationException` tràn lan, ta nên định nghĩa **exception riêng** cho Application và **Middleware** bắt lỗi để trả HTTP status phù hợp.

### 10.1. `IdentityService.Application/Exceptions/NotFoundException.cs`

```csharp
namespace IdentityService.Application.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
    public NotFoundException(string name, object key)
        : base($"Entity \"{name}\" ({key}) was not found.") { }
}
```

### 10.2. `IdentityService.Application/Exceptions/BusinessException.cs`

```csharp
namespace IdentityService.Application.Exceptions;

public class BusinessException : Exception
{
    public BusinessException(string message) : base(message) { }
}
```

### 10.3. `IdentityService.Api/Middlewares/ExceptionHandlingMiddleware.cs`

```csharp
using IdentityService.Application.Exceptions;
using System.Net;
using System.Text.Json;

namespace IdentityService.Api.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            await WriteErrorAsync(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (BusinessException ex)
        {
            await WriteErrorAsync(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (ValidationException ex)
        {
            await WriteErrorAsync(context, HttpStatusCode.BadRequest,
                string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await WriteErrorAsync(context, HttpStatusCode.InternalServerError, "An error occurred.");
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(new { message }));
    }
}
```

### 10.4. Đăng ký middleware trong `Program.cs`

```csharp
using IdentityService.Api.Middlewares;

// ...

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlingMiddleware>();   // <-- thêm

app.UseAuthorization();
```

> Middleware phải được đặt **trước** `UseAuthorization()`/`MapControllers()` để bắt lỗi phát sinh trong pipeline.

## 11. Logging với Serilog (tùy chọn)

`Serilog.AspNetCore 9.0.0` đã khai báo trong `Directory.Packages.props`. Có thể bổ sung `LoggingBehaviour` để log mỗi command/query:

```csharp
using MediatR;
using Microsoft.Extensions.Logging;

namespace IdentityService.Application.Behaviours;

public sealed class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;

    public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogInformation("Handling {RequestName}: {@Request}", requestName, request);

        var response = await next();

        _logger.LogInformation("Handled {RequestName}", requestName);

        return response;
    }
}
```

## 12. Đơn vị công việc (Unit of Work) & giao dịch (tùy chọn)

Hiện `EfRepository` không lưu transaction rõ ràng — `SaveChanges` do `UserManager` tự gọi. Khi có nhiều command ghi nhiều aggregate trong một request, cần một **Unit of Work** bao quanh:

```csharp
// IdentityService.Domain.Interfaces/IUnitOfWork.cs
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
```

Và một **TransactionBehaviour** dùng MediatR pipeline:

```csharp
public sealed class TransactionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly AppIdentityDbContext _dbContext;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Chỉ áp dụng cho Command (request ghi)
        if (typeof(TRequest).FullName!.Contains("Command"))
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var response = await next();

            await transaction.CommitAsync(cancellationToken);

            return response;
        }

        return await next();
    }
}
```

> Đây là pattern phổ biến: mọi Command tự động nằm trong một transaction, Query thì không. Nếu đã dùng `UserManager` (tự gọi `SaveChanges`), transaction này vẫn bao được vì `AppIdentityDbContext` là context chung của Identity.

## 13. Domain Events (nâng cao, tùy chọn)

Nếu muốn phát hành **domain events** khi entity thay đổi, cần:

1. Thêm vào `AppIdentityDbContext.SaveChangesAsync` để lấy các domain events từ `ChangeTracker` và publish qua `IPublisher` (MediatR).
2. Thêm `MediatR` vào `IdentityService.Infrastructure.csproj`.
3. Đăng ký domain event handlers trong `IdentityService.Application`.

Ví dụ trong `AppIdentityDbContext`:

```csharp
private readonly IPublisher? _publisher;

public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options, IPublisher? publisher = null)
    : base(options)
{
    _publisher = publisher;
}

public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    // (xử lý IAuditable như hiện tại)

    var result = await base.SaveChangesAsync(cancellationToken);

    // Sau khi lưu, publish các domain events đã đăng ký trong entity
    // (cần các entity hỗ trợ thu thập event — ví dụ triển khai IDomainEventSource)
    // await PublishDomainEventsAsync();

    return result;
}
```

> Vì các entity Identity hiện đang được dùng trực tiếp, việc thêm domain events cần các entity hỗ trợ lưu trữ event — phần này nên cân nhắc kỹ trước khi làm.

## 14. Danh sách các bước triển khai (Checklist)

1. **`Directory.Packages.props`** — đã có `MediatR 14.0.0`, `FluentValidation.DependencyInjectionExtensions 12.0.0`. ✅
2. **`IdentityService.Application.csproj`** — thêm `PackageReference` cho `MediatR` và `FluentValidation.DependencyInjectionExtensions`.
3. **`IdentityService.Application/DependencyInjection.cs`** — tạo `AddApplication()` đăng ký MediatR (và FluentValidation nếu dùng).
4. **`Program.cs`** — gọi `builder.Services.AddApplication()`.
5. **Tạo `Common/`** — `PagedResult<T>`, `UserDto`, `UserCreateDto`, `UserUpdateDto`.
6. **Tạo `Commands/User/`** — `CreateUserCommand(+Handler)`, `UpdateUserCommand(+Handler)`, `DeleteUserCommand(+Handler)`.
7. **Tạo `Queries/User/`** — `GetUserByIdQuery(+Handler)`, `GetUserByNameQuery(+Handler)`, `GetUsersPagedQuery(+Handler)`.
8. **Tạo `UsersController`** trong `IdentityService.Api` — inject `ISender`, gọi command/query.
9. **(Tùy chọn)** Validators + `ValidationBehaviour` + `LoggingBehaviour`.
10. **(Tùy chọn)** Exceptions + `ExceptionHandlingMiddleware`.
11. **(Tùy chọn)** `TransactionBehaviour` / Unit of Work cho các command nhiều bước.
12. **Test** — build solution, chạy service, gọi API để verify.

## 15. Câu lệnh build & chạy

```powershell
# Build toàn bộ solution
dotnet build .\BookingSystem.sln

# Chạy riêng IdentityService
dotnet run --project .\src\Services\IdentityService\IdentityService.Api\IdentityService.Api.csproj

# (Nếu cần) tạo migration mới — thực hiện trong Infrastructure
dotnet ef migrations add <TenMigration> --project .\src\Services\IdentityService\IdentityService.Infrastructure --startup-project .\src\Services\IdentityService\IdentityService.Api
```

## 16. Tài liệu tham khảo

- [MediatR GitHub](https://github.com/jbogard/MediatR)
- [FluentValidation](https://docs.fluentvalidation.net/)
- Clean Architecture (Jason Taylor) — mô hình `Application` + `AddApplication` tương tự đã dùng ở trên.
