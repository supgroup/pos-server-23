using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class StorageSTS
    {
        public string itemType { get; set; }
        //storagecost
        public Nullable<int> storageCostId { get; set; }
        public string storageCostName { get; set; }
        public decimal storageCostValue { get; set; }
        //
        public Nullable<int> min { get; set; }
        public Nullable<int> max { get; set; }

        public Nullable<int> minUnitId { get; set; }
        public Nullable<int> maxUnitId { get; set; }
        public string minUnitName { get; set; }
        public string maxUnitName { get; set; }
     
       // item unit
        public string itemName { get; set; }
        public string unitName { get; set; }
        public int itemUnitId { get; set; }

        public Nullable<int> itemId { get; set; }
        public Nullable<int> unitId { get; set; }

        public string barcode { get; set; }
        //item location
        public string CreateuserName { get; set; }
        public string CreateuserLName { get; set; }
        public string CreateuserAccName { get; set; }
        public string UuserName { get; set; }
        public string UuserLName { get; set; }
        public string UuserAccName { get; set; }

        //
        public string branchName { get; set; }

        public string branchType { get; set; }
        //itemslocations

        public int itemsLocId { get; set; }
        public Nullable<int> locationId { get; set; }
        public Nullable<decimal> quantity { get; set; }

        public Nullable<System.DateTime> startDate { get; set; }
        public Nullable<System.DateTime> endDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public string IULnote { get; set; }
        public Nullable<decimal> storeCost { get; set; }

        public string cuserName { get; set; }
        public string cuserLast { get; set; }
        public string cUserAccName { get; set; }
        public string uuserName { get; set; }
        public string uuserLast { get; set; }
        public string uUserAccName { get; set; }
        // Location
        public string x { get; set; }
        public string y { get; set; }
        public string z { get; set; }
 

        public Nullable<byte> LocisActive { get; set; }
        public int sectionId { get; set; }
        public string Locnote { get; set; }
        public Nullable<int> branchId { get; set; }
        public Nullable<byte> LocisFreeZone { get; set; }

        
        // section

        public string Secname { get; set; }
        public Nullable<byte> SecisActive { get; set; }
        public string Secnote { get; set; }
        public Nullable<byte> SecisFreeZone { get; set; }
        public bool isExpired { get; set; }
        public int alertDays { get; set; }
        public Nullable<decimal> avgPurchasePrice { get; set; }
        public Nullable<decimal> cost { get; set; }
        public Nullable<decimal> finalcost { get; set; }


    }
    public class ItemTransferInvoice
    {// new properties
        public Nullable<decimal> totalNetRep { get; set; }
        public string mainInvNumber { get; set; }
        public string processType0 { get; set; }
        public decimal totalNet0 { get; set; }
        public int archived { get; set; }
        public double? itemAvg { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public string causeFalls { get; set; }
        public string causeDestroy { get; set; }
        public string userdestroy { get; set; }
        public string userFalls { get; set; }
        public Nullable<int> userId { get; set; }
        public string inventoryNum { get; set; }
        public string inventoryType { get; set; }
        public Nullable<DateTime> inventoryDate { get; set; }
        public Nullable<long> itemCount { get; set; }
        public Nullable<decimal> subTotal { get; set; }
        public string agentCompany { get; set; }
        public string itemName { get; set; }
        public string unitName { get; set; }
        public int itemsTransId { get; set; }
        public Nullable<int> itemUnitId { get; set; }
        public Nullable<int> itemId { get; set; }
        public Nullable<int> unitId { get; set; }
        public Nullable<long> quantity { get; set; }
        public Nullable<decimal> price { get; set; }
        public string barcode { get; set; }

        // ItemTransfer
        public int ITitemsTransId { get; set; }
        public Nullable<int> ITitemUnitId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<int> ITitemId { get; set; }
        public Nullable<int> ITunitId { get; set; }
        public string ITitemName { get; set; }
        public string ITunitName { get; set; }
        private string ITitemUnitName;
        public Nullable<long> ITquantity { get; set; }
        public Nullable<decimal> ITprice { get; set; }


        public Nullable<System.DateTime> ITcreateDate { get; set; }
        public Nullable<System.DateTime> ITupdateDate { get; set; }
        public Nullable<int> ITcreateUserId { get; set; }
        public Nullable<int> ITupdateUserId { get; set; }
        public string ITnotes { get; set; }

        public string ITbarcode { get; set; }
        public string ITCreateuserName { get; set; }
        public string ITCreateuserLName { get; set; }
        public string ITCreateuserAccName { get; set; }

        public string ITUpdateuserName { get; set; }
        public string ITUpdateuserLName { get; set; }
        public string ITUpdateuserAccName { get; set; }
        //invoice
        public Nullable<int> sliceId { get; set; }
        public string sliceName { get; set; }
        public int invoiceId { get; set; }
        public string invNumber { get; set; }
        public Nullable<int> agentId { get; set; }
        public Nullable<int> createUserId { get; set; }
        public string invType { get; set; }
        public string discountType { get; set; }
        public Nullable<decimal> ITdiscountValue { get; set; }
        public Nullable<decimal> discountValue { get; set; }
        public Nullable<decimal> total { get; set; }
        public Nullable<decimal> totalNet { get; set; }
        public Nullable<decimal> paid { get; set; }
        public Nullable<decimal> deserved { get; set; }
        public Nullable<System.DateTime> deservedDate { get; set; }
        public Nullable<System.DateTime> invDate { get; set; }
        public Nullable<System.DateTime> IupdateDate { get; set; }
        public Nullable<int> IupdateUserId { get; set; }
        public Nullable<int> invoiceMainId { get; set; }
        public string invCase { get; set; }
        public Nullable<System.TimeSpan> invTime { get; set; }
        public string Inotes { get; set; }
        public string vendorInvNum { get; set; }
        public string name { get; set; }
        public string branchName { get; set; }
        public Nullable<System.DateTime> vendorInvDate { get; set; }
        public Nullable<int> branchId { get; set; }
        public Nullable<int> itemsCount { get; set; }
        public Nullable<decimal> tax { get; set; }
        public Nullable<int> taxtype { get; set; }
        public Nullable<int> posId { get; set; }
        public Nullable<byte> isApproved { get; set; }
        public Nullable<int> branchCreatorId { get; set; }
        public string branchCreatorName { get; set; }
        public string ITtype { get; set; }
        private string invTypeNumber;//number
        //public string InvTypeNumber { get => invTypeNumber = invType + "-" + invNumber; set => invTypeNumber = value; }
      

        // for report
        //public int countP { get; set; }
        //public int countS { get; set; }
        public int count { get; set; }

        public Nullable<decimal> totalS { get; set; }
        public Nullable<decimal> totalNetS { get; set; }
        public Nullable<decimal> totalP { get; set; }
        public Nullable<decimal> totalNetP { get; set; }
        public string branchType { get; set; }
        public string posName { get; set; }
        public string posCode { get; set; }
        public string agentName { get; set; }


        public string agentType { get; set; }
        public string agentCode { get; set; }
        public string cuserName { get; set; }
        public string cuserLast { get; set; }
        public string cUserAccName { get; set; }
        public string uuserName { get; set; }
        public string uuserLast { get; set; }
        public string uUserAccName { get; set; }
    
      
        public int countPb { get; set; }
        public int countD { get; set; }
        public Nullable<decimal> totalPb { get; set; }
        public Nullable<decimal> totalD { get; set; }
        public Nullable<decimal> totalNetPb { get; set; }
        public Nullable<decimal> totalNetD { get; set; }


        public Nullable<decimal> paidPb { get; set; }
        public Nullable<decimal> deservedPb { get; set; }
        public Nullable<decimal> discountValuePb { get; set; }
        public Nullable<decimal> paidD { get; set; }
        public Nullable<decimal> deservedD { get; set; }
        public Nullable<decimal> discountValueD { get; set; }
        // coupon


        public int CopcId { get; set; }
        public string Copname { get; set; }
        public string Copcode { get; set; }
        public Nullable<byte> CopisActive { get; set; }
        public Nullable<byte> CopdiscountType { get; set; }
        public Nullable<decimal> CopdiscountValue { get; set; }
        public Nullable<System.DateTime> CopstartDate { get; set; }
        public Nullable<System.DateTime> CopendDate { get; set; }
        public string Copnotes { get; set; }
        public Nullable<int> Copquantity { get; set; }
        public Nullable<int> CopremainQ { get; set; }
        public Nullable<decimal> CopinvMin { get; set; }
        public Nullable<decimal> CopinvMax { get; set; }
        public Nullable<System.DateTime> CopcreateDate { get; set; }
        public Nullable<System.DateTime> CopupdateDate { get; set; }
        public Nullable<int> CopcreateUserId { get; set; }
        public Nullable<int> CopupdateUserId { get; set; }
        public string Copbarcode { get; set; }
        public Nullable<decimal> couponTotalValue { get; set; }
        // offer

        public int OofferId { get; set; }
        public string Oname { get; set; }
        public string Ocode { get; set; }
        public Nullable<byte> OisActive { get; set; }
        public string OdiscountType { get; set; }
        public Nullable<decimal> OdiscountValue { get; set; }
        public Nullable<System.DateTime> OstartDate { get; set; }
        public Nullable<System.DateTime> OendDate { get; set; }
        public Nullable<System.DateTime> OcreateDate { get; set; }
        public Nullable<System.DateTime> OupdateDate { get; set; }
        public Nullable<int> OcreateUserId { get; set; }
        public Nullable<int> OupdateUserId { get; set; }
        public string Onotes { get; set; }
        public Nullable<int> Oquantity { get; set; }
        public int Oitemofferid { get; set; }
        public Nullable<decimal> offerTotalValue { get; set; }

        //external
        public Nullable<int> movbranchid { get; set; }
        public string movbranchname { get; set; }
        // internal
        public string exportBranch { get; set; }
        public string importBranch { get; set; }
        public int exportBranchId { get; set; }
        public int importBranchId { get; set; }

        public Nullable<int> invopr { get; set; }
        
        public string processType { get; set; }

        public List<itemsTransfer> invoiceItems { get; set; }
        public List<PayedInvclass> cachTrans { get; set; }
        public List<CashTransferModel> cachTransferList { get; set; }
        public List<InvoiceModel> returnInvList { get; set; }
        public  InvoiceModel  ChildInvoice { get; set; }
    

        /////////////////////


        public int isPrePaid { get; set; }

        public string notes { get; set; }

        public decimal cashReturn { get; set; }

        public Nullable<int> shippingCompanyId { get; set; }
        public string shipCompanyName { get; set; }
        public Nullable<int> shipUserId { get; set; }
        public string shipUserName { get; set; }
        public string status { get; set; }
        public int invStatusId { get; set; }
        public decimal manualDiscountValue { get; set; }
        public string manualDiscountType { get; set; }
        public string createrUserName { get; set; }
        public decimal shippingCost { get; set; }
        public decimal realShippingCost { get; set; }
        public bool isActive { get; set; }
        public string payStatus { get; set; }

        // for report

        public int printedcount { get; set; }
        public bool isOrginal { get; set; }
        public string agentAddress { get; set; }
        public string agentMobile { get; set; }
        //
        public Nullable<int> DBAgentId { get; set; }
        public Nullable<decimal> DBDiscountValue { get; set; }
        public string sales_invoice_note { get; set; }
        public string itemtax_note { get; set; }
        public bool isExpired { get; set; }
        public int alertDays { get; set; }
        public string CreateuserName { get; set; }
        public string CreateuserLName { get; set; }
        public string CreateuserAccName { get; set; }
        public string UpdateuserName { get; set; }
        public string UpdateuserLName { get; set; }
        public string UpdateuserAccName { get; set; }
        public bool performed { get; set; }
        public Nullable<decimal> taxValue { get; set; }
        public decimal VATValue { get; set; }
        public decimal VATRatio { get; set; }
        public bool isTaxExempt { get; set; }
        public List<InvoiceTaxesModel> invoiceTaxes { get; set; }
    }
    public class InventorySTS

    {
        public string userFalls { get; set; }
        private string itemUnits;
        public string ItemUnits { get; set; }
        public int shortfalls { get; set; }
        public Nullable<int> branchId { get; set; }
        public string branchName { get; set; }
        public int inventoryILId { get; set; }
        public Nullable<bool> isDestroyed { get; set; }
        public Nullable<int> amount { get; set; }
        public Nullable<int> amountDestroyed { get; set; }
        public Nullable<int> realAmount { get; set; }
        public Nullable<int> itemLocationId { get; set; }
        public Nullable<byte> isActive { get; set; }
        public string notes { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public int itemId { get; set; }
        public string itemName { get; set; }

        public int unitId { get; set; }
        public int itemUnitId { get; set; }
        public string unitName { get; set; }
        public Nullable<int> sectionId { get; set; }
        public string Secname { get; set; }

        public string x { get; set; }
        public string y { get; set; }
        public string z { get; set; }
        public string itemType { get; set; }
        public Nullable<System.DateTime> inventoryDate { get; set; }
        public string inventoryNum { get; set; }
        public string inventoryType { get; set; }
        public Nullable<int> inventoryId { get; set; }
        public decimal diffPercentage { get; set; }
        public int nCount { get; set; }
        public int dCount { get; set; }
        public int aCount { get; set; }
        public int itemCount { get; set; }
        public int DestroyedCount { get; set; }
        public Nullable<System.DateTime> startDate { get; set; }
        public Nullable<System.DateTime> endDate { get; set; }
        public Nullable<int> itemsLocId { get; set; }
        public Nullable<int> locationId { get; set; }
        public  decimal   shortfallspercent { get; set; }

        public bool isExpired { get; set; }
        public int alertDays { get; set; }

    }
    public class SerialStsModel
    {
        public int serialId { get; set; }
        public Nullable<int> itemsTransId { get; set; }
        public Nullable<int> itemUnitId { get; set; }
        public string serialNum { get; set; }
        public Nullable<byte> isActive { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public bool isSold { get; set; }
        public Nullable<int> branchId { get; set; }
        public bool isManual { get; set; }
        public string itemName { get; set; }
        public string unitName { get; set; }
        public string branchName { get; set; }

        public Nullable<int> itemId { get; set; }
        public Nullable<int> unitId { get; set; }
        public Nullable<int> invoiceId { get; set; }
        public string invNumber { get; set; }

        public string itemType { get; set; }
        public string branchType { get; set; }
        public Nullable<int> itemsLocId { get; set; }
        public Nullable<int> locationId { get; set; }
        public Nullable<long> quantity { get; set; }

        public Nullable<long> count { get; set; }
  
                    public bool isExpired { get; set; }
        public int alertDays { get; set; }
    }
    public class StorePropertyStsModel
    {
        public Nullable<long> quantity { get; set; }
        public int storeProbId { get; set; }
        public Nullable<int> itemUnitId { get; set; }
        public Nullable<int> itemsTransId { get; set; }
        public Nullable<int> serialId { get; set; }
        public Nullable<int> branchId { get; set; }
        public long count { get; set; }
        public bool isSold { get; set; }
        public string notes { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<byte> isActive { get; set; }

        public bool isManual { get; set; }

        #region storePropertyValues attributes
        public int storeProbValueId { get; set; }
        public Nullable<int> propertyId { get; set; }
        public Nullable<int> propertyItemId { get; set; }
        public string propName { get; set; }
        public string propValue { get; set; }
        public int propertyIndex { get; set; }

        #endregion
        public List<StorePropertyValueModel> StorePropertiesValueList { get; set; }
        #region serial attributes
        public string serialNum { get; set; }
        public string StorePropertiesValueString { get; set; }
        #endregion
  
        public Nullable<int> itemId { get; set; }
        public Nullable<int> unitId { get; set; }
       
        public string itemName { get; set; }
        public string unitName { get; set; }
        public string branchName { get; set; }
        public string branchType { get; set; }
        public string invNumber { get; set; }
    }
    public class  ItemUnitCostModel
    {
        public bool isExpired { get; set; }
        public int alertDays { get; set; }
        ///////////////
        public int itemUnitId { get; set; }
       
        public Nullable<int> itemId { get; set; }
        public string itemName { get; set; }
        public Nullable<int> unitId { get; set; }
        public string unitName { get; set; }
        public Nullable<decimal> avgPurchasePrice { get; set; }
      //  public Nullable<decimal> smallunitcost { get; set; }
        public Nullable<decimal> cost { get; set; }
        public Nullable<decimal> finalcost { get; set; }
        public Nullable<decimal> diffPercent{ get; set; }
        public string itemType { get; set; }
    }
    public class ItemUnitInvoiceProfitModel
    {

        ///////////////
        public Nullable<int> userId { get; set; }
        public bool isExpired { get; set; }
        public int alertDays { get; set; }
        public Nullable<decimal> avgPurchasePrice { get; set; }
        public string ITitemName { get; set; }
        public string ITunitName { get; set; }
        public int ITitemsTransId { get; set; }
        public Nullable<int> ITitemUnitId { get; set; }

        public Nullable<int> ITitemId { get; set; }
        public Nullable<int> ITunitId { get; set; }
        public Nullable<long> ITquantity { get; set; }

        public Nullable<System.DateTime> ITupdateDate { get; set; }
        //  public Nullable<int> IT.createUserId { get; set; } 
        public Nullable<int> ITupdateUserId { get; set; }

        public Nullable<decimal> ITprice { get; set; }
        public string ITbarcode { get; set; }

        public string ITUpdateuserNam { get; set; }
        public string ITUpdateuserLNam { get; set; }
        public string ITUpdateuserAccNam { get; set; }
        public int invoiceId { get; set; }
        public string invNumber { get; set; }
        public Nullable<int> agentId { get; set; }
        public Nullable<int> posId { get; set; }
        public string invType { get; set; }
        public Nullable<decimal> total { get; set; }
        public Nullable<decimal> totalNet { get; set; }

        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<System.DateTime> invDate { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<int> branchId { get; set; }
        public Nullable<decimal> discountValue { get; set; }
        public string discountType { get; set; }
        public Nullable<decimal> tax { get; set; }
        // public string name { get; set; }
        //  isApproved { get; set; }


        public Nullable<int> branchCreatorId { get; set; }
        public string branchCreatorName { get; set; }


        public string posName { get; set; }
        public string posCode { get; set; }
        public string agentName { get; set; }
        public string agentCode { get; set; }
        public string agentType { get; set; }

        public string uuserName { get; set; }
        public string uuserLast { get; set; }
        public string uUserAccName { get; set; }
        public string agentCompany { get; set; }
        public Nullable<decimal> subTotal { get; set; }
        public decimal purchasePrice { get; set; }
        public decimal totalwithTax { get; set; }
        public decimal subTotalNet { get; set; } // with invoice discount 
        public decimal itemunitProfit { get; set; }
        public decimal invoiceProfit { get; set; }
        public decimal shippingCost { get; set; }
        public decimal realShippingCost { get; set; }
        public decimal shippingProfit { get; set; }
        public decimal totalNoShip { get; set; }
        public decimal totalNetNoShip { get; set; }
        public string itemType { get; set; }
        public Nullable<decimal> ItemTaxes { get; set; }
        //net profit
        public int cashTransId { get; set; }
        public string transType { get; set; }
        public string transNum { get; set; }

        public string side { get; set; }
        public string processType { get; set; }
        public Nullable<int> cardId { get; set; }
        //
        public List<InvoiceModel> returnInvList { get; set; }
        public InvoiceModel ChildInvoice { get; set; }
        public Nullable<decimal> totalNetRep { get; set; }
        public string ChrguserName { get; set; }
        public string ChrguserLName { get; set; }
       

    }

    public class ItemTransferInvoiceTax
    {// new properties
        public bool isExpired { get; set; }
        public int alertDays { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
 
        public string agentCompany { get; set; }

        // ItemTransfer
        public int ITitemsTransId { get; set; }
        public Nullable<int> ITitemUnitId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<int> ITitemId { get; set; }
        public Nullable<int> ITunitId { get; set; }

        public Nullable<decimal> ITprice { get; set; }

        public string ITnotes { get; set; }

        public string ITbarcode { get; set; }
    
        //invoice
        public int invoiceId { get; set; }
     
        public Nullable<int> agentId { get; set; }

        public string invType { get; set; }
        public string discountType { get; set; }
  
        public Nullable<decimal> discountValue { get; set; }
        public Nullable<decimal> total { get; set; }
        public Nullable<decimal> totalNet { get; set; }
        public Nullable<decimal> paid { get; set; }
        public Nullable<decimal> deserved { get; set; }
        public Nullable<System.DateTime> deservedDate { get; set; }
        public Nullable<System.DateTime> invDate { get; set; }
    
        public Nullable<int> IupdateUserId { get; set; }
    
        public string invCase { get; set; }
   
        public string Inotes { get; set; }
        public string vendorInvNum { get; set; }
       
        public string branchName { get; set; }
        public string posName { get; set; }
        public Nullable<System.DateTime> vendorInvDate { get; set; }
        public Nullable<int> branchId { get; set; }

     
        public Nullable<int> taxtype { get; set; }
        public Nullable<int> posId { get; set; }

        public string ITtype { get; set; }

        public string branchType { get; set; }
       
        public string posCode { get; set; }
        public string agentName { get; set; }

        public string agentType { get; set; }
        public string agentCode { get; set; }
      
        public string uuserName { get; set; }
        public string uuserLast { get; set; }
        public string uUserAccName { get; set; }
        public Nullable<decimal> itemUnitPrice { get; set; }

    
        public Nullable<decimal> subTotalTax { get; set; }
       
     
        public Nullable<decimal> OneitemUnitTax { get; set; }
      
     
        public Nullable<decimal> OneItemOfferVal { get; set; }
        public Nullable<decimal> OneItemPriceNoTax { get; set; }
  
        public Nullable<decimal> OneItemPricewithTax { get; set; }
      
        public Nullable<decimal> itemsTaxvalue { get; set; }


        //invoice
     
        public Nullable<decimal> tax { get; set; }//نسبة الضريبة
        public Nullable<decimal> totalwithTax { get; set; }//قيمة الفاتورة النهائية Totalnet
        public Nullable<decimal> totalNoTax { get; set; }//قيمة الفاتورة قبل الضريبة total
        public Nullable<decimal> invTaxVal { get; set; }//قيمة ضريبة الفاتورة TAX
        public Nullable<int> itemsRowsCount { get; set; }//عدداسطر الفاتورة

        public List<InvoiceModel> returnInvList { get; set; }
        public InvoiceModel ChildInvoice { get; set; }

        public string mainInvNumber { get; set; }
        public Nullable<decimal> totalNetRep { get; set; }
        //item
        public string ITitemName { get; set; }//اسم العنصر
        public string ITunitName { get; set; }//وحدة العنصر

        public Nullable<long> ITquantity { get; set; }//الكمية
        public Nullable<decimal> subTotalNotax { get; set; }//سعر العناصر قبل الضريبة Price
        public Nullable<decimal> itemUnitTaxwithQTY { get; set; }//قيم الضريبة للعناصر
        public string invNumber { get; set; }//رقم الفاتورة//item
        public Nullable<System.DateTime> IupdateDate { get; set; }//تاريخ الفاتورة//item

        public Nullable<decimal> ItemTaxes { get; set; }//  ضريبة العنصر
        public decimal shippingCost { get; set; }
        public decimal realShippingCost { get; set; }
        //public string invNumber { get; set; }//رقم الفاتورة
        //public Nullable<System.DateTime> IupdateDate { get; set; }//تاريخ الفاتورة
        //public Nullable<decimal> tax { get; set; }//نسبة الضريبة
        //public Nullable<decimal> totalwithTax { get; set; }//قيمة الفاتورة النهائية Totalnet
        //public Nullable<decimal> totalNoTax { get; set; }//قيمة الفاتورة قبل الضريبة total
        //public Nullable<decimal> invTaxVal { get; set; }//قيمة ضريبة الفاتورة TAX
        //public Nullable<int> itemsRowsCount { get; set; }//عدداسطر الفاتورة
        // public Nullable<decimal> totalNet { get; set; }
        public Nullable<decimal> taxValue { get; set; }
        public decimal VATValue { get; set; }
        public decimal VATRatio { get; set; }
        public bool isTaxExempt { get; set; }
        public List<InvoiceTaxesModel> invoiceTaxes { get; set; }
    }

    public class OpenClosOperatinModel
    {
        public int cashTransId { get; set; }
        public string transType { get; set; }
        public Nullable<int> posId { get; set; }
        public Nullable<int> userId { get; set; }
        public Nullable<int> agentId { get; set; }
        public Nullable<int> invId { get; set; }
        public string transNum { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<decimal> cash { get; set; }
        public Nullable<decimal> posBalance { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<int> createUserId { get; set; }
        public string notes { get; set; }
        public Nullable<int> posIdCreator { get; set; }
        public Nullable<byte> isConfirm { get; set; }
        public Nullable<int> cashTransIdSource { get; set; }
        public string side { get; set; }
        public string opSideNum { get; set; }
        public string docName { get; set; }
        public string docNum { get; set; }
        public string docImage { get; set; }
        public Nullable<int> bankId { get; set; }
        public string bankName { get; set; }
        public string agentName { get; set; }
        public string usersName { get; set; }
        public string usersLName { get; set; }
        public string posName { get; set; }
        public string posCreatorName { get; set; }
        public Nullable<byte> isConfirm2 { get; set; }
        public int cashTrans2Id { get; set; }
        public Nullable<int> pos2Id { get; set; }

        public string pos2Name { get; set; }
        public string processType { get; set; }
        public Nullable<int> cardId { get; set; }
        public Nullable<int> bondId { get; set; }
        public string createUserName { get; set; }
        public string updateUserName { get; set; }
        public string updateUserJob { get; set; }
        public string updateUserAcc { get; set; }
        public string createUserJob { get; set; }
        public string createUserLName { get; set; }
        public string updateUserLName { get; set; }
        public string cardName { get; set; }
        public Nullable<System.DateTime> bondDeserveDate { get; set; }
        public Nullable<byte> bondIsRecieved { get; set; }
        public string agentCompany { get; set; }
        public Nullable<int> shippingCompanyId { get; set; }
        public string shippingCompanyName { get; set; }
        public string userAcc { get; set; }

        public Nullable<int> branchCreatorId { get; set; }
        public string branchCreatorname { get; set; }
        public Nullable<int> branchId { get; set; }
        public string branchName { get; set; }
        public Nullable<int> branch2Id { get; set; }
        public string branch2Name { get; set; }

        public string invNumber { get; set; }
        public string MaininvNumber { get; set; }
        public string invType { get; set; }
        public Nullable<decimal> commissionValue { get; set; }
        public Nullable<decimal> commissionRatio { get; set; }

    }
    public class POSOpenCloseModel
    {

        public int cashTransId { get; set; }
        public string transType { get; set; }
        public Nullable<int> posId { get; set; }
        public Nullable<decimal> posBalance { get; set; }
        public string transNum { get; set; }
      
        public Nullable<decimal> cash { get; set; }
       
        public string notes { get; set; }
       
        public Nullable<byte> isConfirm { get; set; }
        public Nullable<int> cashTransIdSource { get; set; }
        public string side { get; set; }
        
        public string posName { get; set; }
       

        
        public string processType { get; set; }

        
        public Nullable<int> branchId { get; set; }
        public string branchName { get; set; }

        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<System.DateTime> openDate { get; set; }
        public Nullable<decimal> openCash { get; set; }
        public Nullable<int> openCashTransId { get; set; }
         


    }

    public class InvoiceDaylyModel
    {
        public int invoiceId { get; set; }
        public string invNumber { get; set; }
        public Nullable<int> agentId { get; set; }
        public Nullable<int> userId { get; set; }
        public Nullable<int> createUserId { get; set; }
        public string invType { get; set; }
        public string discountType { get; set; }
        public Nullable<decimal> discountValue { get; set; }
        public Nullable<decimal> total { get; set; }
        public Nullable<decimal> totalNet { get; set; }
        public Nullable<decimal> paid { get; set; }
        public Nullable<decimal> deserved { get; set; }
        public Nullable<System.DateTime> deservedDate { get; set; }
        public Nullable<System.DateTime> invDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<int> invoiceMainId { get; set; }
        public string invCase { get; set; }
        public Nullable<System.TimeSpan> invTime { get; set; }
        public string notes { get; set; }
        public string vendorInvNum { get; set; }
        public string name { get; set; }
        public string branchName { get; set; }
        public string branchCreatorName { get; set; }
        public Nullable<System.DateTime> vendorInvDate { get; set; }
        public Nullable<int> branchId { get; set; }
        public Nullable<int> posId { get; set; }
        public Nullable<int> itemsCount { get; set; }
        public Nullable<decimal> tax { get; set; }
        public Nullable<int> taxtype { get; set; }
        public Nullable<byte> isApproved { get; set; }
        public Nullable<int> branchCreatorId { get; set; }
        public Nullable<int> shippingCompanyId { get; set; }
        public Nullable<int> shipUserId { get; set; }
        public string agentName { get; set; }
        public string shipUserName { get; set; }
        public string shipCompanyName { get; set; }
        public string status { get; set; }
        public int invStatusId { get; set; }
        public decimal manualDiscountValue { get; set; }
        public string manualDiscountType { get; set; }
        public string createrUserName { get; set; }
        public bool isActive { get; set; }
        public decimal cashReturn { get; set; }
        public int printedcount { get; set; }
        public bool isOrginal { get; set; }

        public decimal shippingCost { get; set; }
        public decimal realShippingCost { get; set; }
        public string payStatus { get; set; }
        public int sequence { get; set; }

        public string agentAddress { get; set; }
        public string agentMobile { get; set; }

        public List<PayedInvclass> cachTrans { get; set; }
        public List<ItemTransferModel> invoiceItems { get; set; }
        public int count { get; set; }
        public string posName { get; set; }
        public string posCode { get; set; }

        public string agentCode { get; set; }
        public string agentType { get; set; }
        public string cuserName { get; set; }
        public string cuserLast { get; set; }
        public string cUserAccName { get; set; }
        public string uuserName { get; set; }
        public string uuserLast { get; set; }
        public string uUserAccName { get; set; }
        public string agentCompany { get; set; }
       public string processType { get; set; }
        public List<CashTransferModel> cachTransferList { get; set; }
        public int isPrePaid { get; set; }
        public Nullable<decimal> DBDiscountValue { get; set; }
        public string sales_invoice_note { get; set; }
        public string itemtax_note { get; set; }
        public string mainInvNumber { get; set; }
       
                                          
        public Nullable<int> DBAgentId { get; set; }
        public List<InvoiceModel> returnInvList { get; set; }
        public InvoiceModel ChildInvoice { get; set; }
        public Nullable<decimal> totalNetRep { get; set; }
    }
}