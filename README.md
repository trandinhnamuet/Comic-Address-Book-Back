![image](https://github.com/trandinhnamuet/Comic-Address-Book-Back/assets/57098527/e843ad80-a323-4291-8dea-c741c648194e)﻿# Comic-Address-Book-Back
Back-end for Comic-Address-Book Project

# Cách chạy code
- Cài đặt Visual Studio (Tải từ https://visualstudio.microsoft.com/fr/downloads/)
- Trong Visual Installer, chọn ASP.NET and web development và .NET desktop development -> Cài đặt
- Mở project bằng Visual Studio, Ctrl F5 hoặc click vào nút ![image](https://github.com/trandinhnamuet/Comic-Address-Book-Back/assets/57098527/2d38f799-75ea-4b36-b9bc-19df9192f0e6)
(Hoặc tải và cài dotnet từ https://dotnet.microsoft.com/en-us/download rồi mở folder chứa project, chạy dotnet run. VD: C:\Users\Admin\source\repos\ComicAddressBook\ComicAddressBook>dotnet run)

- Cơ sở dữ liệu
  + Tải và cài SQL Server từ https://www.microsoft.com/en-us/sql-server/sql-server-downloads
  + Tải và cài SSMS để thao tác với csdl từ https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms?view=sql-server-ver16
  + Import file CSDL ComicBookAddressDatabase.bacpac lưu tại https://github.com/trandinhnamuet/Comic-Address-Book-Back/tree/main/ComicAddressBook/Database
# Mô tả chức năng
- Liên kết với SQL Server
- Trả lời truy vấn login của frontend
- API ComicLink
