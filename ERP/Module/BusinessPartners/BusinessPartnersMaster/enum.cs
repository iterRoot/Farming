namespace FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster;

// ⚠️  Values are locked to match existing KOCR rows — do NOT reorder.

public enum BPType
{
    Customer = 0,
    Vendor   = 1,
    Lead     = 2,
}

public enum BPActiveStatus
{
    Active   = 0,
    Inactive = 1,
    Advanced = 2,   // uses ActiveFrom / ActiveTo date range
}

public enum BPTypeOfBusiness
{
    Company    = 0,
    Private    = 1,
    Government = 2,
    Employee   = 3,
}

public enum BPConsolidationType
{
    Payment  = 0,
    Delivery = 1,
}