/****** Script for SelectTopNRows command from SSMS  ******/


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

delete
  FROM 
[dbo].[invoices]
delete
  FROM 
[dbo].[itemsLocations]
delete
  FROM 
[dbo].invoiceOrder
delete
  FROM 
dbo.itemsTransfer
delete
  FROM 
  dbo.itemTransferOffer
delete
  FROM 
dbo.couponsInvoices
delete
  FROM 
[dbo].[itemsOffers]
  delete
  FROM 
[dbo].[itemsTransfer]
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

