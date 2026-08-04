<!-- @format -->

# Hướng dẫn cài đặt Oracle Database trên local bằng Docker

Hướng dẫn chi tiết giúp bạn cài đặt nhanh Oracle Database (23c Free) trên máy cá nhân thông qua Docker, tích hợp dễ dàng vào các dự án phát triển. Chúng ta sử dụng image cộng đồng `gvenzl/oracle-free` – gọn nhẹ, không yêu cầu tài khoản Oracle, sẵn sàng cho môi trường dev.

## Yêu cầu hệ thống

- **Docker Engine** >= 20.10 (có thể cài [Docker Desktop](https://www.docker.com/products/docker-desktop/) cho Windows/Mac)
- Tối thiểu **4 GB RAM** khả dụng cho container
- Ít nhất **10 GB** dung lượng ổ đĩa trống
- Hệ điều hành hỗ trợ Docker (Linux, macOS, Windows)

---

## 1. Cài đặt Docker (nếu chưa có)

- **Windows/macOS:** Tải và cài đặt Docker Desktop từ [trang chủ](https://www.docker.com/products/docker-desktop/).
- **Linux (Ubuntu):**
  ```bash
  sudo apt update
  sudo apt install -y docker.io
  sudo systemctl enable docker --now
  sudo usermod -aG docker $USER   # đăng xuất và đăng nhập lại để có quyền
  ```

docker pull gvenzl/oracle-free:23-slim

New-Item -ItemType Directory -Path "$env:USERPROFILE\docker-volumes\oracle-data" -Force

docker run -d \
 --name oracle-db \
 -p 1521:1521 \
 -e ORACLE_PASSWORD=MySecurePass123 \
 -e APP_USER=devuser \
 -e APP_USER_PASSWORD=DevUserPass456 \
 -v ~/docker-volumes/oracle-data:/opt/oracle/oradata \
 gvenzl/oracle-free:23-slim

# Xem container đang chạy

docker ps -a | grep oracle-db

# Theo dõi log quá trình khởi tạo

docker logs -f oracle-db

services:
oracle-db:
image: gvenzl/oracle-free:23-slim
container_name: oracle-db
ports: - "1521:1521"
environment:
ORACLE_PASSWORD: MySecurePass123
ORACLE_DATABASE: FREE
APP_USER: devuser
APP_USER_PASSWORD: Ab@123456
volumes: - oracle-data:/opt/oracle/oradata
restart: unless-stopped

volumes:
oracle-data:
driver: local
