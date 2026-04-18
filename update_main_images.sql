USE [PETSHOP]
GO

UPDATE Products SET MainImage = '/images/cats/british-shorthair-01.jpg' WHERE Origin = N'British Shorthair';
UPDATE Products SET MainImage = '/images/cats/scottish-fold-01.jpg' WHERE Origin = N'Scottish Fold';
UPDATE Products SET MainImage = '/images/cats/persian-01.jpg' WHERE Origin = N'Persian';
UPDATE Products SET MainImage = '/images/cats/siamese-01.jpg' WHERE Origin = N'Siamese';
UPDATE Products SET MainImage = '/images/cats/ragdoll-01.jpg' WHERE Origin = N'Ragdoll';
UPDATE Products SET MainImage = '/images/cats/maine-coon-01.jpg' WHERE Origin = N'Maine Coon';
UPDATE Products SET MainImage = '/images/cats/russian-blue-01.jpg' WHERE Origin = N'Russian Blue';
UPDATE Products SET MainImage = '/images/cats/sphynx-01.jpg' WHERE Origin = N'Sphynx';
UPDATE Products SET MainImage = '/images/cats/birman-01.jpg' WHERE Origin = N'Birman';

GO
