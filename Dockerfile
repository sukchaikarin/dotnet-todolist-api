# Build stage: ใช้ .NET 9.0 SDK
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy ไฟล์ทั้งหมดไปใน Container
COPY . .

# Restore Dependencies
RUN dotnet restore

# Build ในโหมด Release และ Output ไปที่ /app
RUN dotnet publish -c Release -o /app

# Runtime stage: ใช้ .NET 9.0 Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copy ไฟล์ที่ Build เสร็จแล้วจาก Build Stage
COPY --from=build /app .

# เปิดพอร์ตสำหรับ Container
EXPOSE 80

# รันแอป
ENTRYPOINT ["dotnet", "dotnet-todolist-api.dll"]

