#!/usr/bin/env python3
"""Seed sample master data and one transaction per document controller by
calling the running API (default http://localhost:5181) as the dev Manager
login. Master data is idempotent; the transaction stages are NOT - running
them twice creates a second set of documents.

  python3 scripts/seed_transactions.py                 # all stages
  python3 scripts/seed_transactions.py master sales    # chosen stages
Stages: master, purchasing, stock, sales, finance, misc
"""
import datetime, json, sys, urllib.request, urllib.error

BASE = "http://localhost:5181"
USER, PASSWORD = "Manager", "Admin@2026"   # DefaultUserSeeder's dev account
_tok = None
results = []

def call(method, path, body=None):
    h = {"Content-Type": "application/json"}
    if _tok: h["Authorization"] = "Bearer " + _tok
    data = json.dumps(body).encode() if body is not None else None
    req = urllib.request.Request(BASE + path, data=data, headers=h, method=method)
    try:
        with urllib.request.urlopen(req, timeout=60) as r:
            t = r.read().decode(); return r.status, (json.loads(t) if t else None)
    except urllib.error.HTTPError as e:
        t = e.read().decode()
        try: return e.code, json.loads(t)
        except Exception: return e.code, t

def login():
    global _tok
    s, d = call("POST", "/User/Login", {"userId": USER, "password": PASSWORD})
    if s != 200: sys.exit(f"login failed: {s} {d}")
    _tok = d["token"]

def msg(d): return d if isinstance(d, str) else json.dumps(d)[:240]

def post(label, path, body=None):
    s, d = call("POST", path, body)
    ok = s in (200, 201)
    print(("OK   " if ok else "FAIL ") + label + ("" if ok else f"  [{s}] {msg(d)}"))
    results.append((label, ok)); return d if ok else None

def doc(label, path, body, post_it=True):
    """Create a document (saved as draft), then POST /{id}/Post if it is one."""
    s, d = call("POST", path, body)
    if s not in (200, 201):
        print(f"FAIL {label}  [{s}] {msg(d)}"); results.append((label, False)); return None
    inner = d if isinstance(d, dict) and "id" in d else next((v for v in (d or {}).values() if isinstance(v, dict) and "id" in v), d)
    did = inner.get("id") if isinstance(inner, dict) else None
    d = inner
    if post_it and did is not None and d.get("status") in ("D", "Draft"):
        s2, d2 = call("POST", f"{path}/{did}/Post")
        if s2 not in (200, 201):
            print(f"FAIL {label} (created, post failed)  [{s2}] {msg(d2)}"); results.append((label, False)); return d
    print(f"OK   {label}  id={did}"); results.append((label, True)); return d

def get(p): return call("GET", p)[1]

_now = datetime.datetime.now(datetime.timezone.utc)
NOW = _now.strftime("%Y-%m-%dT00:00:00Z")
DUE = (_now + datetime.timedelta(days=30)).strftime("%Y-%m-%dT00:00:00Z")

def lines(spec):
    return [{"itemId": i, "quantity": q, "price": p, "total": q * p, "whsCode": "WH01"} for i, q, p in spec]
BUY = [(1, 100, 20.0), (2, 50, 30.0)]
SELL = [(1, 10, 30.0), (2, 5, 45.0)]
def total(spec): return sum(q * p for _, q, p in spec)
def purchase(extra=None, spec=BUY):
    return {"vendorId": 2, "postingDate": NOW, "docDate": NOW, "dueDate": DUE, "deliveryDate": DUE, "discount": 0, "tax": 0,
            "taxAmount": 0, "total": total(spec), "remarks": "Seed sample", "items": lines(spec), **(extra or {})}
def sale(extra=None, spec=SELL):
    return {"customerId": 1, "postingDate": NOW, "deliveryDate": DUE, "dueDate": DUE, "returnDate": NOW, "discount": 0, "tax": 0,
            "taxAmount": 0, "total": total(spec), "remarks": "Seed sample", "items": lines(spec), **(extra or {})}

def stage_master():
    print("== MASTER DATA ==")
    coa = [("1000","Assets","A",None),("1100","Cash","A","1000"),("1200","Bank","A","1000"),("1300","Accounts Receivable","A","1000"),
           ("1400","Inventory","A","1000"),("2000","Liabilities","L",None),("2100","Accounts Payable","L","2000"),
           ("2200","Sales Tax Payable","L","2000"),("2300","Goods Received Not Invoiced","L","2000"),("3000","Equity","E",None),
           ("3100","Retained Earnings","E","3000"),("4000","Revenue","I",None),("4100","Sales Revenue","I","4000"),
           ("4200","Sales Discount","I","4000"),("5000","Expenses","X",None),("5100","Cost of Goods Sold","X","5000"),
           ("5200","Purchase Expense","X","5000"),("5300","Inventory Adjustments","X","5000"),("5400","Exchange Gain/Loss","X","5000")]
    have = {a["acctCode"] for a in (get("/ChartOfAccounts") or [])}
    for code, name, t, parent in coa:
        if code in have: continue
        b = {"acctCode": code, "acctName": name, "acctType": t, "isGroup": parent is None, "isActive": True,
             "level": 1 if parent is None else 2, "currency": "USD"}
        if parent: b["fatherNum"] = parent
        post("Account " + code, "/ChartOfAccounts", b)
    if not (get("/GLAccountDetermination") or {}).get("accountsReceivable"):
        det = {"accountsReceivable":"1300","salesRevenue":"4100","salesDiscount":"4200","salesTax":"2200","customerDeposit":"2200",
               "accountsPayable":"2100","purchaseExpense":"5200","purchaseDiscount":"5200","purchaseTax":"2200","vendorDeposit":"1300",
               "inventoryAccount":"1400","costOfGoodsSold":"5100","inventoryOffset":"2300","inventoryRevaluationIncrease":"5300",
               "inventoryRevaluationDecrease":"5300","inventoryCountingGain":"5300","inventoryCountingLoss":"5300","priceDifference":"5300",
               "cashAccount":"1100","bankAccount":"1200","exchangeGainLoss":"5400","retainedEarnings":"3100","openingBalance":"3100"}
        s, d = call("POST", "/GLAccountDetermination", det)
        if s not in (200, 201): s, d = call("PUT", "/GLAccountDetermination", det)
        print(("OK   " if s in (200, 201) else "FAIL ") + "GL account determination"); results.append(("GL account determination", s in (200, 201)))
    post("Document number ranges", "/DocumentNumberRange/Seed")
    if not get("/Warehouse"): post("Warehouse WH01", "/Warehouse", {"code":"WH01","name":"Main Warehouse","isDefault":True,"nettable":True,"isActive":True})
    if not get("/UnitOfMeasure"): post("Unit PCS", "/UnitOfMeasure", {"code":"PCS","name":"Pieces","decimalPlaces":0,"isActive":True})
    if not get("/ItemGroup"): post("Item group GEN", "/ItemGroup", {"code":"GEN","name":"General Goods","isActive":True})
    if not get("/ItemsMaster"):
        for code, name, price, cost in [("ITM001","Organic Rice 25kg",30.0,20.0),("ITM002","Fertilizer 50kg",45.0,30.0)]:
            post("Item " + code, "/ItemsMaster", {"itemCode":code,"itemName":name,"status":0,"itemGroup":"GEN","itemType":"I","uomName":"Pieces",
                 "uomCode":"PCS","salesItem":True,"purchaseItem":True,"stockItem":True,"unitPrice":price,"salePrice":price,"purchasePrice":cost,
                 "itemCost":cost,"warehouse":"WH01","valuationMethod":"MovingAverage","priceCurrency":"USD"})
    if not get("/BusinessPartnersMaster"):
        post("Customer C00001", "/BusinessPartnersMaster", {"code":"C00001","cardName":"Sample Customer Co.","typeStatus":0,"activeStatus":0,"currency":"USD","creditLimit":100000})
        post("Vendor V00001", "/BusinessPartnersMaster", {"code":"V00001","cardName":"Sample Supplier Co.","typeStatus":1,"activeStatus":0,"currency":"USD"})

def stage_purchasing():
    print("== PURCHASING ==")
    doc("Purchase Blanket Agreement", "/PurchaseBlanketAgreement", purchase())
    doc("Purchase Request", "/PurchaseRequest", purchase())
    doc("Purchase Quotation", "/PurchaseQuotation", purchase())
    doc("Purchase Order", "/PurchaseOrder", purchase())
    doc("Goods Receipt PO (stock in)", "/GoodsReceiptPO", purchase())
    doc("AP Down Payment Request", "/APDownPaymentRequest", purchase(spec=[(1, 1, 500.0)]))
    doc("AP Invoice", "/APInvoice", purchase())

def stage_stock():
    print("== STOCK ==")
    doc("Goods Receipt", "/GoodsReceipt", {"vendorId":2,"customerCode":"V00001","customerName":"Sample Supplier Co.","postingDate":NOW,"docDate":NOW,
        "status":"O","totalAmount":200.0,"remarks":"Seed","lines":[{"itemId":1,"itemCode":"ITM001","itemName":"Organic Rice 25kg","quantity":10,"price":20.0,"total":200.0}]}, post_it=False)
    doc("Goods Issue", "/GoodsIssue", {"customerCode":"C00001","customerName":"Sample Customer Co.","postingDate":NOW,"docDate":NOW,
        "status":"O","totalAmount":40.0,"remarks":"Seed","lines":[{"itemId":1,"itemCode":"ITM001","itemName":"Organic Rice 25kg","quantity":2,"price":20.0,"total":40.0}]}, post_it=False)

def stage_sales():
    print("== SALES ==")
    doc("Sales Blanket Agreement", "/SaleBlanketAgreement", sale())
    doc("Sales Quotation", "/SaleQuotation", sale())
    doc("Sales Order", "/SaleOrder", sale())
    doc("Delivery", "/Delivery", sale())
    doc("AR Down Payment Request", "/ArDownPaymentRequest", sale(spec=[(1, 1, 100.0)]))
    doc("AR Invoice", "/ARInvoice", sale())

def stage_finance():
    print("== PAYMENTS, CREDIT NOTES, JOURNAL ==")
    doc("AR Credit Note", "/ARCreditNote", sale(spec=[(1, 1, 30.0)]))
    doc("AP Credit Note", "/APCreditNote", purchase(spec=[(1, 1, 20.0)]))
    doc("Sales Return", "/Return", sale(spec=[(1, 1, 30.0)]))
    doc("Goods Return", "/GoodsReturn", purchase(spec=[(1, 1, 20.0)]))
    post("Incoming Payment (settles AR Invoice 1)", "/IncomingPayment", {"docDate": NOW, "dueDate": NOW, "cardCode": "C00001",
         "cardName": "Sample Customer Co.", "docTotal": 525.0, "currency": "USD", "exchangeRate": 1, "paymentMethod": "Cash",
         "bankAccount": "1100", "remarks": "Seed", "invoices": [{"invoiceId": 1, "appliedAmount": 525.0}]})
    post("Outgoing Payment (part-pays AP Invoice 1)", "/OutgoingPayment", {"docDate": NOW, "dueDate": NOW, "cardCode": "V00001",
         "cardName": "Sample Supplier Co.", "docTotal": 1000.0, "currency": "USD", "exchangeRate": 1, "paymentMethod": "Cash",
         "bankAccount": "1100", "memo": "Seed", "invoices": [{"invoiceId": 1, "appliedAmount": 1000.0}]})
    je = post("Journal Entry (owner capital)", "/JournalEntry", {"transDate": NOW, "dueDate": NOW, "docDate": NOW, "refNo": "SEED-1",
         "transType": "JE", "currency": "USD", "exchangeRate": 1, "memo": "Seed owner capital",
         "lines": [{"lineNum": 1, "accountCode": "1100", "debit": 1000.0, "credit": 0, "lineMemo": "Cash in"},
                   {"lineNum": 2, "accountCode": "3100", "debit": 0, "credit": 1000.0, "lineMemo": "Capital"}]})
    if isinstance(je, dict) and je.get("id") and je.get("status") in ("D", "Draft"):
        post("Journal Entry post", f"/JournalEntry/{je['id']}/Post")

def stage_misc():
    print("== TRANSFERS ==")
    post("Warehouse WH02", "/Warehouse", {"code": "WH02", "name": "Secondary Warehouse", "isActive": True, "nettable": True})
    post("Stock Transfer WH01 -> WH02", "/StockTransfer", {"postingDate": NOW, "documentDate": NOW, "fromWarehouse": "WH01", "toWarehouse": "WH02",
         "remarks": "Seed", "lines": [{"lineNum": 1, "itemNo": "ITM001", "itemDescription": "Organic Rice 25kg", "fromWarehouse": "WH01",
         "toWarehouse": "WH02", "quantity": 5, "uoMCode": "PCS", "unitPrice": 20.0}]})

STAGES = {"master": stage_master, "purchasing": stage_purchasing, "stock": stage_stock, "sales": stage_sales, "finance": stage_finance, "misc": stage_misc}

if __name__ == "__main__":
    login()
    chosen = sys.argv[1:] or list(STAGES)
    for n in chosen:
        if n not in STAGES: sys.exit(f"unknown stage {n}; choose from {', '.join(STAGES)}")
        STAGES[n]()
    print(f"\n{sum(ok for _, ok in results)} ok / {len(results)} calls")
