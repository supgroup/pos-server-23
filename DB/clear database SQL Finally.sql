/****** Script for SelectTopNRows command from SSMS  ******/
use [posempty]
UPDATE [dbo].[agents]
   SET 
     [balance] = 0
      ,[balanceType] = 0
	  UPDATE [dbo].users
   SET 
     [balance] = 0
      ,[balanceType] = 0

   	  UPDATE [dbo].[shippingCompanies]
   SET 
     [balance] = 0
      ,[balanceType] = 0

	    UPDATE [dbo].pos
   SET 
     [balance] = 0

        UPDATE [dbo].pos 
		SET 
     [boxState] = 'c',
	 [createUserId]=1,
	  [updateUserId]=1,
	    [branchId]=2
	  WHERE pos.posId=1

DELETE FROM [dbo].[invoiceTaxes]
  
GO

delete
  FROM 
[dbo].[invoices]
DELETE FROM [dbo].[categoryuser]
    
GO

delete
  FROM 
[dbo].[notification]
delete
  FROM 
[dbo].[notificationUser]


UPDATE [dbo].[groups]
   SET 
   
      [createUserId] = 1
      ,[updateUserId] = 1
      
 
GO
UPDATE [dbo].[propertiesItems]
   SET  
       
       [createUserId] = 1
      ,[updateUserId] = 1

GO
DELETE FROM [dbo].[invoiceTypesPrinters]
   
GO
delete
  FROM 
[dbo].[itemsLocations]
delete
  FROM 
[dbo].invoiceOrder
delete
  FROM 
dbo.itemsTransfer

DELETE FROM [dbo].[taxes]
   
GO
DELETE FROM [dbo].[sliceUser]
  
GO
DELETE FROM [dbo].[slices]
      
GO
DELETE FROM [dbo].[cards]
   
GO
DELETE FROM [dbo].[warranty]
     
GO
DELETE FROM [dbo].[bondes]
       
GO
DELETE FROM [dbo].[posPrinters]
   
GO
delete
  FROM 
  dbo.itemTransferOffer
  GO
delete
  FROM 
dbo.couponsInvoices
GO
delete
  FROM 
[dbo].[itemsOffers]
GO
  delete
  FROM 
[dbo].[itemsTransfer]
GO
  delete
  FROM 
[dbo].[error]
  delete
  FROM 
[dbo].[cashTransfer]
delete
  FROM 
[dbo].[Inventory]
  delete
  FROM 
[dbo].[inventoryItemLocation]
  delete
  FROM 
[dbo].[invoiceStatus]

 delete
  FROM 
[dbo].[usersLogs]

delete
  FROM 
[dbo].[agents]

  delete
  FROM 
[dbo].[banks]






  delete
  FROM [categories]


  delete
  FROM [dbo].[coupons]


  delete
  FROM [dbo].[couponsInvoices]





  delete
  FROM [dbo].[docImages]




  delete
  FROM [dbo].[error]




  delete 
  FROM [dbo].[Inventory]




  delete 
  FROM [dbo].[items]
  



  delete 
  FROM
[dbo].[itemsOffers]



  delete 
  FROM
[dbo].[itemsProp]


  delete 
  FROM
[dbo].[itemsUnits]



  delete 
  FROM
[dbo].[itemTransferOffer]



  delete 
  FROM
[dbo].[itemUnitUser]



  delete 
	  FROM [posempty].[dbo].[locations]
	  where locationId NOT IN
	(
	SELECT locationId
	  FROM [posempty].[dbo].[locations]
	  where (branchId = 2 and isFreeZone = 1) 
	  )

	  

delete 
	  FROM [posempty].[dbo].[sections]
	  where sectionId NOT IN
(SELECT sectionId
  FROM [posempty].[dbo].[sections]
  where branchId = 2  and [sections].[name] = 'FreeZone'
  )



  delete 
  FROM
[dbo].[memberships]


 


  delete 
  FROM [dbo].[offers]



  delete 
  FROM [dbo].[packages]



  delete 
  FROM [dbo].[pos]
  where pos.posId != 1



  delete 
  FROM [dbo].[posSerials]



  delete 
  FROM [dbo].[posSetting]




  delete 
  FROM [dbo].[posUsers]



  delete 
  FROM [dbo].[printers]








   delete 
  FROM [dbo].[serials]

  delete 
  FROM [dbo].[shippingCompanies]



  delete 
  FROM [dbo].[storageCost]



  delete 
  FROM [dbo].[subscriptionFees]




  delete 
  FROM [dbo].[sysEmails]



  delete 
  FROM [dbo].[userSetValues]




  delete 
  FROM [dbo].[userSetValues]

   

   

delete 
  FROM 
[dbo].[users]
where users.userId != 1
and users.userId != 2


  delete
  FROM 
[dbo].[branches]
where branches.branchId != 1 and  branches.branchId != 2

  delete
  FROM [dbo].[branchesUsers]


  delete
  FROM [dbo].[branchStore]

  UPDATE [dbo].[ProgramDetails]
   SET  
      [programName] =  '' 
      ,[branchCount] = 0
      ,[posCount] = 0
      ,[userCount] = 0
      ,[vendorCount] =0
      ,[customerCount] =0
      ,[itemCount] = 0
      ,[saleinvCount] =0
      ,[programIncId] = 0
      ,[versionIncId] = 0
      ,[versionName] = '' 
      ,[storeCount] = 0
      ,[packageSaleCode] = '' 
      ,[customerServerCode] ='' 
      ,[expireDate] = NULL
      ,[isOnlineServer] = 1
      ,[packageNumber] =  '' 
      ,[updateDate] =NULL
      ,[isLimitDate] = 1
      ,[isLimitCount] = 1
      ,[isActive] = 1
      ,[packageName] = '' 
      ,[bookDate] = NULL
      ,[pId] = 0
      ,[pcdId] =0
      ,[customerName] = ''  
      ,[customerLastName] = '' 
      ,[agentName] =  '' 
      ,[agentLastName] = '' 
      ,[agentAccountName] = ''  
      ,[isServerActivated] = 0
      ,[activatedate] =NULL
      ,[pocrDate] = NULL
      ,[poId] = 0
      ,[upnum] = ''  
      ,[notes] = '' 
 WHERE id=1
GO
delete from posSerials
go


DELETE FROM [dbo].[agents]
 
GO
DELETE FROM [dbo].[TokensTable]
      
GO

DELETE FROM [dbo].[UsersRequest]
   
GO
UPDATE [dbo].[setValues]
   SET [value] = ''
     
 WHERE [settingId]=1 or [settingId]=2 or [settingId]=3 or [settingId]=4 or [settingId]=5 or [settingId]=6
  or [settingId]=15 or [settingId]=16 or [settingId]=17 or [settingId]=18 or [settingId]=30 
  or [settingId]=40 or [settingId]=41 or [settingId]=48 or [settingId]=49  or [settingId]=56 or [settingId]=57  or [settingId]=58
GO

UPDATE [dbo].[setValues]
   SET [value] = '1'
     
 WHERE [settingId]=19 or [settingId]=20 or [settingId]=27 or [settingId]=39  or [settingId]=43  or [settingId]=45
GO

UPDATE [dbo].[setValues]
   SET [value] = '0'
     
 WHERE [settingId]=11 or [settingId]=12 or [settingId]=21 or [settingId]=22  or [settingId]=23  or [settingId]=24
 or [settingId]=42
GO
UPDATE [dbo].[setValues]
   SET [value] = 'en'
     
 WHERE [settingId]=46  
GO
UPDATE [dbo].[users]
   SET 
      [createUserId] =1
      ,[updateUserId] = 1
      
      ,[isActive] = 1
      
      ,[isOnline] = 0
      
      ,[image] =''
     
      ,[balance] =0
      ,[balanceType] = 0
      ,[isAdmin] = 1
      ,[hasCommission] =0
      ,[commissionValue] = 0
      ,[commissionRatio] = 0
 WHERE userId=2      
GO
