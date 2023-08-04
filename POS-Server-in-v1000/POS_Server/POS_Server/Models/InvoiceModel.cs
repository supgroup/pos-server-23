using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class InvoiceModel
    {
        public int invoiceId { get; set; }
        public string invNumber { get; set; }
        public Nullable<int> agentId { get; set; }
        public Nullable<int> userId { get; set; }
        public Nullable<int> createUserId { get; set; }
        public string invType { get; set; }
        public string discountType { get; set; }
        public Nullable<decimal> DBDiscountValue { get; set; }
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
        public int isPrePaid { get; set; }
        public int isShipPaid { get; set; }
        public int isFreeShip { get; set; }
        public decimal shippingCost { get; set; }
        public decimal realShippingCost { get; set; }
        public string payStatus { get; set; }
        public Nullable<int> sliceId { get; set; }
        public string sliceName { get; set; }
        public int sequence { get; set; }

        public string agentAddress { get; set; }
        public string agentMobile { get; set; }

        public List<PayedInvclass> cachTrans { get; set; }
        public List<ItemTransferModel> invoiceItems { get; set; }
        public List<InvoiceTaxesModel> invoiceTaxes { get; set; }
        public string sales_invoice_note { get; set; }
        public string itemtax_note { get; set; }
        public string mainInvNumber { get; set; }
        public bool isArchived { get; set; }
        public bool canReturn { get; set; }
        public InvoiceModel ChildInvoice { get; set; }
        //4 report
        public List<InvoiceModel> returnInvList { get; set; }
        public Nullable<decimal> totalNetRep { get; set; }
        public bool performed { get; set; }
        public decimal taxValue { get; set; }

        public ShippingCompaniesModel ShippingCompany { get; set; }
        public UserModel DeliveryMan { get; set; }
        public AgentModel Agent { get; set; }
        public BranchModel FromBranch { get; set; }
        public BranchModel ToBranch { get; set; }
        public decimal VATValue { get; set; }
    }

    public class PayedInvclass
    {
        public string processType { get; set; }
        public Nullable<decimal> cash { get; set; }
        public string cardName { get; set; }
        public int sequenc { get; set; }
        public Nullable<int> cardId { get; set; }
        public Nullable<decimal> commissionValue { get; set; }
        public Nullable<decimal> commissionRatio { get; set; }
        public string docNum { get; set; }

    }
    public class CouponInvoiceModel
    {
        public int id { get; set; }
        public Nullable<int> couponId { get; set; }
        public Nullable<int> InvoiceId { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<decimal> discountValue { get; set; }
        public Nullable<byte> discountType { get; set; }

        public string couponCode { get; set; }
    }
}