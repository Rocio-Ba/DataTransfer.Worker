


Use[SourceDB]
Go

-- One Million Records For Test

WITH X1 AS(SELECT* FROM (VALUES (0),(0),(0),(0),(0),(0),(0),(0),(0),(0)) T(C))
INSERT INTO[Account].[User] (Name)
SELECT TOP(1000000) NEWID()
FROM X1 A,X1 B, X1 C,X1 D, X1 E,X1 F