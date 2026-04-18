USE [PETSHOP]
GO

-- Xóa dữ liệu cũ nếu có
DELETE FROM ProductImages WHERE ProductId IN (SELECT Id FROM Products WHERE CategoryId = 2);
GO

-- 1. British Shorthair
INSERT INTO ProductImages (ProductId, ImageUrl, IsDefault) SELECT Id, '/images/cats/british-shorthair-01.jpg', 0 FROM Products WHERE Origin = N'British Shorthair';
INSERT INTO ProductImages (ProductId, ImageUrl, IsDefault) SELECT Id, '/images/cats/british-shorthair-02.jpg', 0 FROM Products WHERE Origin = N'British Shorthair';
INSERT INTO ProductImages (ProductId, ImageUrl, IsDefault) SELECT Id, '/images/cats/british-shorthair-03.jpg', 0 FROM Products WHERE Origin = N'British Shorthair';
INSERT INTO ProductImages (ProductId, ImageUrl, IsDefault) SELECT Id, '/images/cats/british-shorthair-04.jpg', 0 FROM Products WHERE Origin = N'British Shorthair';

-- 2. Scottish Fold
INSERT INTO ProductImages (ProductId, ImageUrl, IsDefault) SELECT Id, '/images/cats/scottish-fold-01.jpg', 0 FROM Products WHERE Origin = N'Scottish Fold';
INSERT INTO ProductImages (ProductId, ImageUrl, IsDefault) SELECT Id, '/images/cats/scottish-fold-02.jpg', 0 FROM Products WHERE Origin = N'Scottish Fold';
INSERT INTO ProductImages (ProductId, ImageUrl, IsDefault) SELECT Id, '/images/cats/scottish-fold-03.jpg', 0 FROM Products WHERE Origin = N'Scottish Fold';
INSERT INTO ProductImages (ProductId, ImageUrl, IsDefault) SELECT Id, '/images/cats/scottish-fold-04.jpg', 0 FROM Products WHERE Origin = N'Scottish Fold';

-- 3. Persian
INSERT INTO ProductImages (ProductId, ImageUrl, IsDefault) SELECT Id, '/images/cats/persian-01.jpg', 0 FROM Products WHERE Origin = N'Persian';
INSERT INTO ProductImages (ProductId, ImageUrl, IsDefault) SELECT Id, '/images/cats/persian-02.jpg', 0 FROM Products WHERE Origin = N'Persian';
INSERT INTO ProductImages (ProductId, ImageUrl, IsDefault) SELECT Id, '/images/cats/persian-03.jpg', 0 FROM Products WHERE Origin = N'Persian';
INSERT INTO ProductImages (ProductId, ImageUrl, IsDefault) SELECT Id, '/images/cats/persian-04.jpg', 0 FROM Products WHERE Origin = N'Persian';

-- 4. Siamese
INSERT INTO ProductImages (ProductId, ImageUrl, IsDefault) SELECT Id, '/images/cats/siamese-01.jpg', 0 FROM Products WHERE Origin = N'Siamese';
INSERT INTO ProductImages (ProductId, ImageUrl, IsDefault) SELECT Id, '/images/cats/siamese-02.jpg', 0 FROM Products WHERE Origin = N'Siamese';
INSERT INTO ProductImages (ProductId, ImageUrl, IsDefault) SELECT Id, '/images/cats/siamese-03.jpg', 0 FROM Products WHERE Origin = N'Siamese';
INSERT INTO ProductImages (ProductId, ImageUrl, IsDefault) SELECT Id, '/images/cats/siamese-04.jpg', 0 FROM Products WHERE Origin = N'Siamese';

-- 5. Ragdoll
INSERT INTO ProductImages (ProductId, ImageUrl, IsDefault) SELECT Id, '/images/cats/ragdoll-01.jpg', 0 FROM Products WHERE Origin = N'Ragdoll';
INSERT INTO ProductImages (ProductId, ImageUrl, IsDefault) SELECT Id, '/images/cats/ragdoll-02.jpg', 0 FROM Products WHERE Origin = N'Ragdoll';
INSERT INTO ProductImages (ProductId, ImageUrl, IsDefault) SELECT Id, '/images/cats/ragdoll-03.jpg', 0 FROM Products WHERE Origin = N'Ragdoll';
INSERT INTO ProductImages (ProductId, ImageUrl, IsDefault) SELECT Id, '/images/cats/ragdoll-04.jpg', 0 FROM Products WHERE Origin = N'Ragdoll';

-- 6. Maine Coon
INSERT INTO ProductImages (ProductId, ImageUrl, IsDefault) SELECT Id, '/images/cats/maine-coon-01.jpg', 0 FROM Products WHERE Origin = N'Maine Coon';
INSERT INTO ProductImages (ProductId, ImageUrl, IsDefault) SELECT Id, '/images/cats/maine-coon-02.jpg', 0 FROM Products WHERE Origin = N'Maine Coon';
INSERT INTO ProductImages (ProductId, ImageUrl, IsDefault) SELECT Id, '/images/cats/maine-coon-03.jpg', 0 FROM Products WHERE Origin = N'Maine Coon';
INSERT INTO ProductImages (ProductId, ImageUrl, IsDefault) SELECT Id, '/images/cats/maine-coon-04.jpg', 0 FROM Products WHERE Origin = N'Maine Coon';

-- 7. Russian Blue
INSERT INTO ProductImages (ProductId, ImageUrl, IsDefault) SELECT Id, '/images/cats/russian-blue-01.jpg', 0 FROM Products WHERE Origin = N'Russian Blue';
INSERT INTO ProductImages (ProductId, ImageUrl, IsDefault) SELECT Id, '/images/cats/russian-blue-02.jpg', 0 FROM Products WHERE Origin = N'Russian Blue';
INSERT INTO ProductImages (ProductId, ImageUrl, IsDefault) SELECT Id, '/images/cats/russian-blue-03.jpg', 0 FROM Products WHERE Origin = N'Russian Blue';
INSERT INTO ProductImages (ProductId, ImageUrl, IsDefault) SELECT Id, '/images/cats/russian-blue-04.jpg', 0 FROM Products WHERE Origin = N'Russian Blue';

-- 8. Sphynx
INSERT INTO ProductImages (ProductId, ImageUrl, IsDefault) SELECT Id, '/images/cats/sphynx-01.jpg', 0 FROM Products WHERE Origin = N'Sphynx';
INSERT INTO ProductImages (ProductId, ImageUrl, IsDefault) SELECT Id, '/images/cats/sphynx-02.jpg', 0 FROM Products WHERE Origin = N'Sphynx';
INSERT INTO ProductImages (ProductId, ImageUrl, IsDefault) SELECT Id, '/images/cats/sphynx-03.jpg', 0 FROM Products WHERE Origin = N'Sphynx';
INSERT INTO ProductImages (ProductId, ImageUrl, IsDefault) SELECT Id, '/images/cats/sphynx-04.jpg', 0 FROM Products WHERE Origin = N'Sphynx';

-- 9. Birman
INSERT INTO ProductImages (ProductId, ImageUrl, IsDefault) SELECT Id, '/images/cats/birman-01.jpg', 0 FROM Products WHERE Origin = N'Birman';
INSERT INTO ProductImages (ProductId, ImageUrl, IsDefault) SELECT Id, '/images/cats/birman-02.jpg', 0 FROM Products WHERE Origin = N'Birman';
INSERT INTO ProductImages (ProductId, ImageUrl, IsDefault) SELECT Id, '/images/cats/birman-03.jpg', 0 FROM Products WHERE Origin = N'Birman';
INSERT INTO ProductImages (ProductId, ImageUrl, IsDefault) SELECT Id, '/images/cats/birman-04.jpg', 0 FROM Products WHERE Origin = N'Birman';

GO
