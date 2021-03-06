USE [gear_db]
GO
/****** Object:  UserDefinedFunction [dbo].[ModelMaxYear]    Script Date: 01/12/2015 13:25:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
ALTER FUNCTION [dbo].[ModelMaxYear]
(
	-- Add the parameters for the function here
	@NewOnly bit,
	@ModelID int
)
RETURNS int
AS
BEGIN
	-- Declare the return variable here
	DECLARE @year int = null
	declare @today int = YEAR(GETDATE())
	
	declare @AllYears table(CarID int, Value decimal(11,3))
	
	--insert into @AllYears (CarID, Value)
	--select cpv.CarID, cpv.Value
	--from
	--	Cars c
	--	left join CarPropertiesValues cpv on c.ID = cpv.CarID
	--	left join CarProperties cp on cpv.CarPropertyID = cp.ID		
	--where cp.EngName=N'car_prop_last_local_year' 
	--		and cp.ID=cpv.CarPropertyID 
	--		and c.ID=cpv.CarID 
	--		and c.ModelID=@ModelID
	
	insert into @AllYears (CarID, Value)
	SELECT DISTINCT 
			Cpv.CarID
			,CASE	
				WHEN (SELECT COUNT(*) FROM [CarPropertiesValues] C WHERE C.CarID = Cpv.CarID AND C.CarPropertyID = 3 ) > 0 
						THEN (SELECT TOP 1 Value FROM [CarPropertiesValues] C WHERE C.CarID = Cpv.CarID AND C.CarPropertyID = 3 )
				ELSE YEAR(GETDATE()) 
				END AS Value
	FROM Cars C LEFT JOIN CarPropertiesValues Cpv ON C.ID = Cpv.CarID
	WHERE	C.ModelID = @ModelID 
			AND Cpv.CarID IS NOT NULL
		
	-- Add the T-SQL statements to compute the return value here
	if(@NewOnly=1)
		select @year=max(case cpv.Value
							when null then @today
							when 0 then @today
							else CAST(cpv.Value as int)
						end)
		from NewCars c left outer join @AllYears cpv
									on c.ID=cpv.CarID
		where c.ModelID=@ModelID
		and c.IsActive=1
		and c.IsDeleted=0
	else
		select @year=max(case cpv.Value
							when null then @today
							when 0 then @today
							else CAST(cpv.Value as int)
						end)
		from AllCars c left outer join @AllYears cpv
									on c.ID=cpv.CarID
		where c.ModelID=@ModelID
		and c.IsActive=1
		and c.IsDeleted=0

	-- Return the result of the function
	set @year=ISNULL(@year, @today)
	
	RETURN case @year
				when 0 then @today
				else @year
			end

END
