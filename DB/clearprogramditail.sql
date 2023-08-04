USE [incposdb]
GO

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
	  ,[isDemo] = '1' 
 WHERE id=1
GO
delete from posSerials
go


