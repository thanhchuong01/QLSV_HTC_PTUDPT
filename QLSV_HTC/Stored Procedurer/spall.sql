USE [QLSV_HTC]
GO
/****** Object:  StoredProcedure [dbo].[gv_ltc]    Script Date: 11/12/2023 6:05:52 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[gv_ltc] @maltc int
as
begin 
	select gvday.MAGV
	from (select * FROM DAY where DAY.MALTC = @maltc) as gvday 

end



GO
/****** Object:  StoredProcedure [dbo].[sp_DangKyLopTinChi]    Script Date: 11/12/2023 6:05:52 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_DangKyLopTinChi]
	@MALTC int, @MASV nchar(10), @MAMH nchar(10), @HOCKY int, @NIENKHOA nchar(9)
AS
BEGIN
	SELECT DK.MALTC 
	FROM DANGKY DK
	LEFT JOIN LOPTINCHI LTC
	ON  DK.MALTC = LTC.MALTC
	WHERE	LTC.MAMH = @MAMH AND 
			LTC.NIENKHOA = @NIENKHOA AND 
			LTC.HOCKY = @HOCKY  AND 
			DK.HUYDK = 0 AND 
			DK.MASV = @MASV
	
	IF @@ROWCOUNT > 0
	BEGIN
		RAISERROR ('Môn học đã đăng ký rồi!', 16,1)
		RETURN 1;
	END


	UPDATE DANGKY 
	SET    HUYDK = 0 
	WHERE  MALTC = @MALTC AND MASV = @MASV 

	IF @@ROWCOUNT = 0 
	  INSERT INTO DANGKY (MALTC, MASV, DIEMCC, DIEMGK, DIEMCK, HUYDK) 
	  VALUES (@MALTC, @MASV , 0, 0, 0, 0)
END


GO
/****** Object:  StoredProcedure [dbo].[SP_DANGNHAP]    Script Date: 11/12/2023 6:05:52 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[SP_DANGNHAP]
	@TENLOGIN NVARCHAR (50)
AS
BEGIN
	DECLARE @TENUSER NVARCHAR(50)

	SELECT @TENUSER=NAME FROM sys.sysusers WHERE sid = SUSER_SID(@TENLOGIN)
 
	SELECT USERNAME = @TENUSER, 
		HOTEN = (SELECT HO + ' '+ TEN FROM GIANGVIEN  WHERE MAGV = @TENUSER),
		TENNHOM = NAME
	FROM sys.sysusers 
	WHERE UID = (SELECT GROUPUID 
				FROM SYS.SYSMEMBERS
				WHERE MEMBERUID = (SELECT UID FROM sys.sysusers WHERE NAME=@TENUSER))
END

GO
/****** Object:  StoredProcedure [dbo].[sp_DSLTC]    Script Date: 11/12/2023 6:05:52 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_DSLTC]
AS
BEGIN

	SELECT  ROW_NUMBER() over(ORDER BY mh.TENMH, ltc.NHOM) STT, mh.TENMH, ltc.NHOM, gv.TEN, ltc.MAKHOA, ltc.SOSVTT, ltc.SOSVTT, ltc.HUYLOP
	FROM LOPTINCHI as ltc

	LEFT JOIN (SELECT DISTINCT day.MALTC, 
			STUFF((SELECT ' - '+ gv.HO + ' ' + gv.TEN 
				FROM GIANGVIEN as gv, LOPTINCHI as ltc
				JOIN DAY as day ON day.MALTC = ltc.MALTC
				WHERE gv.MAGV = day.MAGV
				FOR XML PATH ('') ),1,2,'') as TEN
			FROM GIANGVIEN as gv, LOPTINCHI as ltc
				JOIN DAY as day ON day.MALTC = ltc.MALTC
				WHERE gv.MAGV = day.MAGV
	) AS GV ON GV.MALTC = ltc.MALTC 
	LEFT JOIN MONHOC as mh ON mh.MAMH = ltc.MAMH
	LEFT JOIN DANGKY as dk ON dk.MALTC = ltc.MALTC

	GROUP BY ltc.NHOM, mh.TENMH, gv.TEN, ltc.MAKHOA, ltc.SOSVTT, ltc.HUYLOP
	ORDER BY mh.TENMH, ltc.NHOM
	  
END


GO
/****** Object:  StoredProcedure [dbo].[sp_Get_Info_Login]    Script Date: 11/12/2023 6:05:52 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_Get_Info_Login]
	@LOGINNAME varchar(50)
AS
BEGIN
	DECLARE @TENUSER NVARCHAR(50)
	SELECT @TENUSER=NAME FROM sys.sysusers WHERE sid = SUSER_SID(@LOGINNAME)
 
	DECLARE @HOTEN NVARCHAR(50)
	if exists (SELECT HO+ ' '+ TEN FROM GIANGVIEN  WHERE MAGV =  @TENUSER)
	SELECT @HOTEN=HO+ ' '+ TEN FROM GIANGVIEN  WHERE MAGV =  @TENUSER

	SELECT USERNAME = @TENUSER, HOTEN = @HOTEN, TENNHOM = NAME
	   FROM sys.sysusers 
	   WHERE UID = (SELECT GROUPUID 
					FROM SYS.SYSMEMBERS 
					WHERE MEMBERUID = (SELECT UID FROM sys.sysusers WHERE NAME = @TENUSER)
					)
END

GO
/****** Object:  StoredProcedure [dbo].[sp_InBangDiemMonHoc]    Script Date: 11/12/2023 6:05:52 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_InBangDiemMonHoc] @NIENKHOA nchar(9), @HOCKY int, @MAMH nchar(10), @NHOM int 
AS
BEGIN

	SELECT ROW_NUMBER() over(ORDER BY sv.TEN, sv.HO) STT, sv.MASV, sv.HO, sv.TEN, dk.DIEMCC, dk.DIEMGK, dk.DIEMCK, (dk.DIEMCC *0.1 +dk.DIEMGK*0.3 + dk.DIEMCK * 0.6) as 'DIEM_HM'
	FROM (SELECT ltc.MALTC from LOPTINCHI as ltc where ltc.MAMH = @MAMH AND ltc.NIENKHOA = @NIENKHOA AND ltc.HOCKY = @HOCKY AND ltc.NHOM = @NHOM) as ltc

	INNER JOIN DANGKY as dk ON dk.MALTC= ltc.MALTC
	INNER JOIN SINHVIEN as sv ON dk.MASV = sv.MASV

	WHERE sv.DANGHIHOC = 'False'

	ORDER BY sv.TEN, sv.HO
	  
END

GO
/****** Object:  StoredProcedure [dbo].[sp_InBangDiemTongKet]    Script Date: 11/12/2023 6:05:52 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_InBangDiemTongKet]
	-- KHAI BÁO BI?N T?M
	@MALOP nchar(10)
AS
BEGIN
	SELECT HOTEN = SV.MASV + ' - ' +SV.HO + ' ' + SV.TEN, DIEM.TENMH, DIEM.DIEM
FROM (SELECT * FROM SINHVIEN WHERE DANGHIHOC = 'true')  AS SV
INNER JOIN (SELECT MASV, DIEM = MAX(DIEM), TENMH
			FROM 
			(SELECT DK.MALTC, DK.MASV, DIEM = DK.DIEMCC *0.1 + DK.DIEMCK * 0.6 + DK.DIEMGK * 0.3 
			FROM DANGKY DK
			GROUP BY DK.MALTC, DK.MASV, DK.DIEMCC, DK.DIEMGK, DK.DIEMCK) AS DIEM

			LEFT JOIN  (
				SELECT LTC.MALTC, MH.TENMH
				FROM MONHOC MH
				INNER JOIN LOPTINCHI LTC
				ON LTC.MAMH = MH.MAMH
			) AS MH

			ON DIEM.MALTC = MH.MALTC
			GROUP BY MH.TENMH, DIEM.MASV) AS DIEM

ON SV.MASV = DIEM.MASV  AND SV.MALOP = @MALOP
GROUP BY sv.HO, sv.MASV, sv.TEN, DIEM.TENMH, DIEM.DIEM
ORDER BY sv.TEN, sv.HO ASC

END


GO
/****** Object:  StoredProcedure [dbo].[sp_InDSLTC]    Script Date: 11/12/2023 6:05:52 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_InDSLTC] @NIENKHOA nchar(9), @HOCKY int
AS
BEGIN

	SELECT  ROW_NUMBER() over(ORDER BY mh.TENMH, ltc.NHOM) STT, mh.TENMH, ltc.NHOM, ltc.SOSVTT, COUNT(dk.MASV) as soSVDK
	FROM  (SELECT LOPTINCHI.MALTC, LOPTINCHI.NIENKHOA, LOPTINCHI.HOCKY, LOPTINCHI.nhom, LOPTINCHI.SOSVTT, LOPTINCHI.MAMH , LOPTINCHI.HUYLOP 
		FROM LOPTINCHI WHERE LOPTINCHI.NIENKHOA = @NIENKHOA AND LOPTINCHI.HOCKY = @HOCKY AND LOPTINCHI.HUYLOP = 0) as ltc

	
	LEFT JOIN MONHOC as mh ON mh.MAMH = ltc.MAMH
	LEFT JOIN DANGKY as dk ON dk.MALTC = ltc.MALTC

	GROUP BY ltc.NHOM, mh.TENMH, ltc.SOSVTT
	ORDER BY mh.TENMH, ltc.NHOM
	  
END


GO
/****** Object:  StoredProcedure [dbo].[sp_InDSSV]    Script Date: 11/12/2023 6:05:52 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_InDSSV]
	-- Add the parameters for the stored procedure here
	@NIENKHOA nchar(9), @HOCKY int, @MAMH nchar(10), @NHOM int
AS
BEGIN

	SELECT  ROW_NUMBER() over(ORDER BY SV.TEN, SV.HO) STT, SV.MASV, SV.HO, SV.TEN, Case 
            When SV.PHAI='false' Then 'Nam' 
            Else 'Nu' END AS Phai  
			, SV.MALOP
	FROM  (SELECT LTC.MALTC FROM LOPTINCHI AS LTC WHERE LTC.MAMH = @MAMH AND LTC.NIENKHOA = @NIENKHOA AND LTC.HOCKY = @HOCKY AND LTC.NHOM = @NHOM AND LTC.HUYLOP='false') AS LTC
	Inner join DANGKY as DK  on DK.MALTC = LTC.MALTC
	Inner join SINHVIEN as SV on DK.MASV = SV.MASV

	ORDER BY SV.TEN, SV.HO

END


GO
/****** Object:  StoredProcedure [dbo].[sp_InDSSVLop]    Script Date: 11/12/2023 6:05:52 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_InDSSVLop]
	-- Add the parameters for the stored procedure here
	@MAKHOA nchar(10), @MALOP nchar(10)
AS
BEGIN

	SELECT  ROW_NUMBER() over(ORDER BY SV.TEN, SV.HO) STT, SV.MASV, SV.HO, SV.TEN, Case 
            When SV.PHAI='false' Then 'Nam' 
            Else 'Nu' END AS Phai  
	FROM (SELECT sv.MASV, sv.HO, sv.TEN, sv.PHAI  FROM SINHVIEN AS sv, LOP WHERE LOP.MAKHOA = @MAKHOA AND LOP.MALOP = @MALOP) AS SV		
	ORDER BY SV.TEN, SV.HO

END

GO
/****** Object:  StoredProcedure [dbo].[sp_KiemTraLTC]    Script Date: 11/12/2023 6:05:52 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[sp_KiemTraLTC]
@NIENKHOA nchar(9), @HOCKY int, @MAMH nchar(10), @NHOM int 
AS
BEGIN
	IF EXISTS ( SELECT * FROM LOPTINCHI WHERE LOPTINCHI.NIENKHOA = @NIENKHOA AND LOPTINCHI.HOCKY = @HOCKY AND LOPTINCHI.MAMH = @MAMH AND LOPTINCHI.NHOM = @NHOM)
	BEGIN
		RAISERROR ('Lớp tín chỉ đã tồn tại', 16,1)
		RETURN 1;
	END


	IF EXISTS ( SELECT * FROM LINK0.QLSV_HTC.DBO.LOPTINCHI WHERE LOPTINCHI.NIENKHOA = @NIENKHOA AND LOPTINCHI.HOCKY = @HOCKY AND LOPTINCHI.MAMH = @MAMH AND LOPTINCHI.NHOM = @NHOM)
	BEGIN
		RAISERROR ('Lớp tín chỉ đã tồn tại trên chi nhánh khác', 16,1)
		RETURN 1;
	END

RETURN 0;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_KiemTraMaGV]    Script Date: 11/12/2023 6:05:52 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[sp_KiemTraMaGV]
@MAGV char(15), @MAKHOA char(10)
AS
BEGIN
	IF NOT EXISTS  ( SELECT * FROM GIANGVIEN WHERE GIANGVIEN.MAGV = @MAGV AND GIANGVIEN.MAKHOA = @MAKHOA)
	BEGIN
		RAISERROR ('Mã giảng viên không tồn tại trên khoa đang chọn', 16,1)
		RETURN 1;
	END

RETURN 0;
END

GO
/****** Object:  StoredProcedure [dbo].[sp_KiemTraMaLop]    Script Date: 11/12/2023 6:05:52 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[sp_KiemTraMaLop]
@MALOP char(10)
AS
BEGIN
	IF EXISTS ( SELECT * FROM LOP WHERE LOP.MALOP = @MALOP)
	BEGIN
		RAISERROR ('Mã lớp học đã tồn tại', 16,1)
		RETURN 1;
	END

	IF EXISTS ( SELECT * FROM LINK1.QLSV_HTC.DBO.LOP WHERE LOP.MALOP = @MALOP)
	BEGIN
		RAISERROR ('Mã lớp học đã tồn tại trên chi nhánh khác', 16,1)
		RETURN 1;
	END

	IF EXISTS ( SELECT * FROM LINK0.QLSV_HTC.DBO.LOP WHERE LOP.MALOP = @MALOP)
	BEGIN
		RAISERROR ('Mã lớp học đã tồn tại trên chi nhánh khác', 16,1)
		RETURN 1;
	END

RETURN 0;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_KiemTraMaMH]    Script Date: 11/12/2023 6:05:52 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[sp_KiemTraMaMH]
@MAMH char(15)
AS
BEGIN
	IF EXISTS ( SELECT * FROM MONHOC WHERE MONHOC.MAMH = @MAMH)
	BEGIN
		RAISERROR ('Mã môn học đã tồn tại', 16,1)
		RETURN 1;
	END

	IF EXISTS ( SELECT * FROM LINK1.QLSV_HTC.DBO.MONHOC WHERE MONHOC.MAMH = @MAMH)
	BEGIN
		RAISERROR ('Mã môn học đã tồn tại trên chi nhánh khác', 16,2)
		RETURN 1;
	END

	IF EXISTS ( SELECT * FROM LINK0.QLSV_HTC.DBO.MONHOC WHERE MONHOC.MAMH = @MAMH)
	BEGIN
		RAISERROR ('Mã môn học đã tồn tại trên chi nhánh khác', 16,3)
		RETURN 1;
	END

RETURN 0;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_KiemTraMaSV]    Script Date: 11/12/2023 6:05:52 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_KiemTraMaSV]
	-- Add the parameters for the stored procedure here
	@MASV nchar(10)
AS
BEGIN
	IF EXISTS ( SELECT * FROM SINHVIEN as sv WHERE sv.MASV = @MASV )
		BEGIN
			RAISERROR ('Mã sinh viên đã tồn tại', 16,1)
			RETURN 1;
		END

		IF EXISTS ( SELECT * FROM LINK1.QLSV_HTC.DBO.SINHVIEN as sv WHERE sv.MASV = @MASV)
		BEGIN
			RAISERROR ('Mã sinh viên đã tồn tại ở Khoa khác', 16,1)
			RETURN 1;
		END

		IF EXISTS ( SELECT * FROM LINK0.QLSV_HTC.DBO.SINHVIEN as sv WHERE sv.MASV = @MASV)
		BEGIN
			RAISERROR ('Mã sinh viên đã tồn tại ở Khoa khác', 16,1)
			RETURN 1;
		END

	RETURN 0;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_LayDSLopTinChiDaDangKy]    Script Date: 11/12/2023 6:05:52 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_LayDSLopTinChiDaDangKy]
	-- khai bao cac bien tam 
	@MASV nchar(10), @NIENKHOA nchar(9), @HOCKY int
AS
BEGIN

	SELECT ROW_NUMBER() over(ORDER BY MH.TENMH, LTC.NHOM) STT,LTC.MALTC, MH.MAMH, MH.TENMH, LTC.NHOM
	FROM  LOPTINCHI as LTC

	INNER JOIN MONHOC as MH ON MH.MAMH = LTC.MAMH
	INNER JOIN (SELECT MASV, MALTC FROM DANGKY WHERE MASV = @MASV AND HUYDK = 0) as DK ON DK.MALTC = LTC.MALTC

	WHERE LTC.NIENKHOA = @NIENKHOA AND LTC.HOCKY = @HOCKY AND LTC.HUYLOP = 0 
	GROUP BY LTC.NHOM, MH.MAMH, MH.TENMH, LTC.MALTC
	ORDER BY MH.TENMH, LTC.NHOM
	  
END

GO
/****** Object:  StoredProcedure [dbo].[sp_LayDSLopTinChiDeDangKy]    Script Date: 11/12/2023 6:05:52 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_LayDSLopTinChiDeDangKy]
	-- khai bao cac bien tam 
	@NIENKHOA nchar(9), @HOCKY int
AS
BEGIN
	SELECT ltc.MALTC, mh.MAMH, mh.TENMH, ltc.NHOM, GV.TEN, ltc.SOSVTT, COUNT(dk.MASV) as SOSVDK
	FROM  LOPTINCHI as ltc

	LEFT JOIN DAY as day ON day.MALTC = ltc.MALTC 
	LEFT JOIN (SELECT DISTINCT day.MALTC, 
			STUFF((SELECT ' - '+ gv.HO + ' ' + gv.TEN 
				FROM GIANGVIEN as gv, LOPTINCHI as ltc
				JOIN DAY as day ON day.MALTC = ltc.MALTC
				WHERE gv.MAGV = day.MAGV
				FOR XML PATH ('') ),1,2,'') as TEN
			FROM GIANGVIEN as gv, LOPTINCHI as ltc
				JOIN DAY as day ON day.MALTC = ltc.MALTC
				WHERE gv.MAGV = day.MAGV
	) AS GV ON GV.MALTC = ltc.MALTC 
	
	LEFT JOIN MONHOC as mh ON mh.MAMH = ltc.MAMH
	LEFT JOIN (SELECT MASV, MALTC FROM DANGKY WHERE HUYDK = 0) as dk ON dk.MALTC = ltc.MALTC
	
	WHERE ltc.NIENKHOA = @NIENKHOA AND ltc.HOCKY = @HOCKY AND ltc.HUYLOP = 0
	GROUP BY ltc.MALTC, ltc.NHOM, mh.MAMH, mh.TENMH, GV.TEN, ltc.SOSVTT
	ORDER BY mh.TENMH, ltc.NHOM
	  
END


GO
/****** Object:  StoredProcedure [dbo].[sp_LayDSLopTinChiDeDangKy_tmp]    Script Date: 11/12/2023 6:05:52 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_LayDSLopTinChiDeDangKy_tmp]
	-- khai bao cac bien tam 
	@NIENKHOA nchar(9), @HOCKY int
AS
BEGIN
	SELECT ltc.MALTC, mh.MAMH, mh.TENMH, ltc.NHOM, GV.TEN, ltc.SOSVTT, COUNT(dk.MASV) as SOSVDK
	FROM  LOPTINCHI as ltc

	LEFT JOIN DAY as day ON day.MALTC = ltc.MALTC 
	LEFT JOIN (SELECT DISTINCT day.MALTC, 
			STUFF((SELECT ' - '+ gv.HO + ' ' + gv.TEN 
				FROM GIANGVIEN as gv, LOPTINCHI as ltc
				JOIN DAY as day ON day.MALTC = ltc.MALTC
				WHERE gv.MAGV = day.MAGV
				FOR XML PATH ('') ),1,2,'') as TEN
			FROM GIANGVIEN as gv, LOPTINCHI as ltc
				JOIN DAY as day ON day.MALTC = ltc.MALTC
				WHERE gv.MAGV = day.MAGV
	) AS GV ON GV.MALTC = ltc.MALTC 
	
	LEFT JOIN MONHOC as mh ON mh.MAMH = ltc.MAMH
	LEFT JOIN (SELECT MASV, MALTC FROM DANGKY WHERE HUYDK = 0) as dk ON dk.MALTC = ltc.MALTC
	
	WHERE ltc.NIENKHOA = @NIENKHOA AND ltc.HOCKY = @HOCKY AND ltc.HUYLOP = 0
	GROUP BY ltc.MALTC, ltc.NHOM, mh.MAMH, mh.TENMH, GV.TEN, ltc.SOSVTT
	ORDER BY mh.TENMH, ltc.NHOM
	  
END


GO
/****** Object:  StoredProcedure [dbo].[sp_PhieuDiem]    Script Date: 11/12/2023 6:05:52 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_PhieuDiem]
	-- khai bao cac bien 
	@MASV char(12)
AS
BEGIN
   SELECT  ROW_NUMBER() over(ORDER BY mh.TENMH) STT, mh.TENMH , MAX(dk.DIEMCC *0.1 + dk.DIEMCK * 0.6 + dk.DIEMGK * 0.3) AS 'DIEM'
   
   FROM (SELECT * from DANGKY as dk where dk.MASV = @MASV ) as dk
	
   INNER JOIN LOPTINCHI as ltc ON dk.MALTC= ltc.MALTC
   INNER JOIN MONHOC as mh ON mh.MAMH = ltc.MAMH
   

   GROUP BY mh.TENMH
   ORDER BY mh.TENMH
END

GO
/****** Object:  StoredProcedure [dbo].[sp_TaoTaiKhoan]    Script Date: 11/12/2023 6:05:52 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_TaoTaiKhoan]
	@USERNAME VARCHAR(50),  @PASSWORD VARCHAR(50),
    @USERID VARCHAR(50),  @ROLE VARCHAR(50)    
AS

	DECLARE @RET INT

	EXEC @RET= SP_ADDLOGIN @USERNAME, @PASSWORD, 'QLSV_HTC'
	IF (@RET = 1)  -- USERNAME BI TRUNG
		BEGIN
			RETURN 1 -- Username đã tồn tại
		END 

  EXEC @RET= SP_GRANTDBACCESS @USERNAME, @USERID
  IF (@RET = 1)  -- USERID BI TRUNG
	  BEGIN
		   EXEC SP_DROPLOGIN @USERNAME
		   RETURN 2 -- UserId đã tồn tại tài khoản đăng nhập
	  END


  EXEC sp_addrolemember @ROLE, @USERID
  IF @ROLE <> 'SV'
	  BEGIN
		  EXEC sp_addsrvrolemember @USERNAME, 'SecurityAdmin'
		  EXEC sp_addsrvrolemember @USERNAME, 'SysAdmin'
		  EXEC sp_addsrvrolemember @USERNAME, 'ProcessAdmin'
	  END
  ELSE
	  BEGIN
		  EXEC sp_addsrvrolemember @USERNAME, 'ProcessAdmin'
	  END

RETURN 0 -- tạo thành công


GO
