USE [PETSHOP]
GO

DELETE FROM Products WHERE CategoryId = 2;
GO

INSERT INTO Products (CategoryId, ProductName, Price, StockQuantity, MainImage, IsPet, AgeMonths, Gender, FurColor, HealthStatus, FullDescription, Origin, CreatedAt)
VALUES
(2, N'Bella (British Shorthair)', 7500000, 17, 'https://images.unsplash.com/photo-1543852786-1cf6624b9987?auto=format&fit=crop&w=500&q=80', 1, 3, 0, N'Xám xanh (blue)', N'Đã tiêm mũi cơ bản, ăn khỏe', N'Mặt tròn, lông plush dày, tính cách điềm đạm.', N'British Shorthair', GETDATE()),
(2, N'Luna (Scottish Fold)', 9800000, 22, 'https://images.unsplash.com/photo-1573865526739-10659fec78a5?auto=format&fit=crop&w=500&q=80', 1, 4, 1, N'Vàng kem trắng', N'Đã tẩy giun, sức khỏe ổn định', N'Tai cụp đặc trưng, mặt tròn, hiền và quấn người.', N'Scottish Fold', GETDATE()),
(2, N'Mimi (Persian)', 8600000, 26, 'https://images.unsplash.com/photo-1533738363-b7f9aef128ce?auto=format&fit=crop&w=500&q=80', 1, 5, 0, N'Trắng tuyết', N'Đã tiêm phòng, lông đẹp', N'Mặt tịt, lông dài sang, phù hợp nuôi trong nhà.', N'Persian', GETDATE()),
(2, N'Coco (Siamese)', 6900000, 19, 'https://images.unsplash.com/photo-1511044568932-338cba0ad803?auto=format&fit=crop&w=500&q=80', 1, 3, 1, N'Seal point', N'Nhanh nhẹn, ăn tốt, đã tiêm', N'Mặt xanh, thân thon, rất thông minh và hoạt bát.', N'Siamese', GETDATE()),
(2, N'Mochi (Ragdoll)', 12500000, 24, 'https://images.unsplash.com/photo-1513245543132-31f507417b26?auto=format&fit=crop&w=500&q=80', 1, 4, 0, N'Blue bicolor', N'Đã tiêm phòng, thân thiện', N'Lông bán dài mềm mượt, mắt xanh, bế lên rất ngoan.', N'Ragdoll', GETDATE()),
(2, N'Simba (Maine Coon)', 14800000, 29, 'https://images.unsplash.com/photo-1568043210943-0e8aac4b9734?auto=format&fit=crop&w=500&q=80', 1, 6, 1, N'Nâu tabby', N'Khung xương tốt, ăn khỏe', N'Kích thước lớn, đuôi xù đẹp, vẻ ngoài mạnh mẽ.', N'Maine Coon', GETDATE()),
(2, N'Nala (Russian Blue)', 8200000, 16, 'https://images.unsplash.com/photo-1555685812-4b943f1cb0eb?auto=format&fit=crop&w=500&q=80', 1, 4, 0, N'Xanh xám bạc', N'Mắt sáng, lông mượt, thể trạng tốt', N'Lông ngắn ánh bạc, mắt xanh lục, tính hiền.', N'Russian Blue', GETDATE()),
(2, N'Suri (Sphynx)', 18900000, 27, 'https://images.unsplash.com/photo-1571566882372-1598d88abd90?auto=format&fit=crop&w=500&q=80', 1, 5, 1, N'Hồng kem (ít lông)', N'Da sạch, nhạy bén', N'Gần như không lông, tai lớn, ngoại hình rất nổi bật.', N'Sphynx', GETDATE()),
(2, N'Yuki (Birman)', 9200000, 21, 'https://images.unsplash.com/photo-1514888286974-6c03e2ca1dba?auto=format&fit=crop&w=500&q=80', 1, 4, 0, N'Seal point găng trắng', N'Đã tiêm mũi cơ bản, tinh thần tốt', N'Mắt xanh, chân có găng trắng, dịu dàng và dễ chăm.', N'Birman', GETDATE());
GO
