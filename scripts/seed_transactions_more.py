#!/usr/bin/env python3
"""Second seed pass: the transaction controllers seed_transactions.py does not reach
(reserve / down-payment invoices, return requests, production, landed cost, journal vouchers,
recurring postings, bank statements, reconciliations, depreciation, payment runs, period-end closing ...).

Run seed_transactions.py first (it creates the chart of accounts, items, partners and sample stock), then:
    SEED_PASSWORD=<Manager password> python3 seed_transactions_more.py [stage ...]
API address: SEED_BASE (default http://localhost:5182).
"""
import os, sys, datetime
import seed_transactions as st

st.BASE = os.environ.get("SEED_BASE", st.BASE)
NOW, DUE = st.NOW, st.DUE
doc, post, call, get = st.doc, st.post, st.call, st.get
sale, purchase = st.sale, st.purchase


def put(label, path, body=None):
    s, d = call("PUT", path, body)
    ok = s in (200, 201, 204)
    print(("OK   " if ok else "FAIL ") + label + ("" if ok else f"  [{s}] {st.msg(d)}"))
    st.results.append((label, ok)); return d if ok else None


def stage_sales_more():
    print("== SALES (more) ==")
    doc("AR Reserve Invoice", "/ARReserveInvoice", sale())
    doc("AR Down Payment Invoice", "/ARDownPaymentInvoice", sale(spec=[(1, 1, 100.0)]))
    doc("Return Request", "/ReturnRequest", sale(spec=[(1, 1, 30.0)]))
    pl = doc("Pick List", "/PickList", {"pickDate": NOW, "picker": "Manager", "status": "Released", "user": "Manager", "remarks": "Seed", "lines": [
        {"transType": "SO", "docNo": "1", "bpCode": "C00001", "deliveryDueDate": DUE, "itemNo": "ITM001", "description": "Organic Rice 25kg",
         "uomCode": "PCS", "uomName": "Pieces", "whsCode": "WH01", "orderQty": 10, "openQty": 10, "pickedQty": 10}]}, post_it=False)


def stage_purchasing_more():
    print("== PURCHASING (more) ==")
    doc("AP Reserve Invoice", "/APReserveInvoice", purchase())
    doc("AP Down Payment Invoice", "/APDownPaymentInvoice", purchase(spec=[(1, 1, 500.0)]))
    doc("Goods Return Request", "/GoodsReturnRequest", purchase(spec=[(1, 1, 20.0)]))
    post("Landed Cost", "/LandingCost", {"postingDate": NOW, "docDate": NOW, "vendorId": 2, "remarks": "Seed",
         "goodsReceiptPOIds": [1], "costLines": [{"costType": "Freight", "amount": 50.0, "remarks": "Freight"}]})


def stage_stock_more():
    print("== STOCK (more) ==")
    doc("Stock Transfer Request", "/StockTransferRequest", {"postingDate": NOW, "dueDate": DUE, "documentDate": NOW, "fromWarehouse": "WH01",
        "toWarehouse": "WH02", "remarks": "Seed", "lines": [{"lineNum": 1, "itemNo": "ITM001", "itemDescription": "Organic Rice 25kg",
        "fromWarehouse": "WH01", "toWarehouse": "WH02", "quantity": 2, "uoMCode": "PCS", "unitPrice": 20.0}]})
    doc("Stock Revaluation", "/StockRevaluation", {"postingDate": NOW, "documentDate": NOW, "revaluationType": "Price Change", "remarks": "Seed",
        "lines": [{"itemNo": "ITM001", "itemDescription": "Organic Rice 25kg", "whsCode": "WH01", "currentCost": 20.0, "newCost": 22.0, "uom": "PCS", "inStock": 85}]})


def stage_production():
    print("== PRODUCTION ==")
    if not get("/ItemsMaster"): return
    bom = post("Bill of Material", "/BillOfMaterial", {"parentItemCode": "ITM001", "quantity": 1, "warehouse": "WH01", "isActive": True,
               "remarks": "Seed", "lines": [{"itemCode": "ITM002", "quantity": 1}]})
    bid = (bom or {}).get("id") if isinstance(bom, dict) else None
    po = doc("Production Order", "/ProductionOrder", {"itemCode": "ITM001", "billOfMaterialId": bid or 1, "plannedQty": 5, "warehouse": "WH01",
             "dueDate": DUE, "remarks": "Seed"}, post_it=False)
    no = (po or {}).get("docNum") or (po or {}).get("productionOrderNo") or (po or {}).get("id")
    if po: print("     production order ->", {k: po[k] for k in list(po)[:8]})
    doc("Issue for Production", "/IssueForProduction", {"postingDate": NOW, "warehouse": "WH01", "productionOrderNo": str(no), "remarks": "Seed",
        "lines": [{"itemCode": "ITM002", "quantity": 5}]}, post_it=False)
    doc("Receipt from Production", "/ReceiptFromProduction", {"postingDate": NOW, "warehouse": "WH01", "productionOrderNo": str(no), "remarks": "Seed",
        "lines": [{"itemCode": "ITM001", "quantity": 5}]}, post_it=False)


def stage_banking():
    print("== BANKING / GL ==")
    jv = doc("Journal Voucher", "/JournalVoucher", post_it=False, body={"voucherDate": NOW, "dueDate": NOW, "refNo": "JV-1", "description": "Seed", "currency": "USD",
        "exchangeRate": 1, "lines": [{"lineNum": 1, "accountCode": "5200", "debit": 100.0, "credit": 0, "lineMemo": "Expense"},
                                      {"lineNum": 2, "accountCode": "1100", "debit": 0, "credit": 100.0, "lineMemo": "Cash"}]})
    if jv and jv.get("id"):
        jid = jv["id"]
        post("Journal Voucher submit", f"/JournalVoucher/{jid}/Submit")
        post("Journal Voucher approve", f"/JournalVoucher/{jid}/Approval", {"Action": "Approve", "Note": "Seed"})
        post("Journal Voucher post", f"/JournalVoucher/{jid}/Post")
    post("Recurring Posting", "/RecurringPosting", {"description": "Monthly rent", "frequency": "Monthly", "startDate": NOW,
         "validUntil": (datetime.datetime.now(datetime.timezone.utc) + datetime.timedelta(days=365)).strftime("%Y-%m-%dT00:00:00Z"),
         "status": "Active", "currency": "USD", "reference": "RENT", "lines": [
            {"lineNum": 1, "accountCode": "5200", "accountName": "Purchase Expense", "debit": 200.0, "credit": 0},
            {"lineNum": 2, "accountCode": "1100", "accountName": "Cash", "debit": 0, "credit": 200.0}]})
    post("Internal Transfer (cash to bank)", "/InternalTransfer", {"postingDate": NOW, "fromAccount": "1100", "toAccount": "1200", "amount": 300.0,
         "bankCharge": 0, "currency": "USD", "reference": "IT-1", "memo": "Seed"})
    post("Bank Statement", "/BankStatement", {"accountCode": "1200", "bankName": "Seed Bank", "bankAccountNo": "000111", "currency": "USD",
         "statementDate": NOW, "fromDate": NOW, "toDate": NOW, "openingBalance": 0, "closingBalance": 300.0, "remarks": "Seed",
         "lines": [{"valueDate": NOW, "reference": "IT-1", "description": "Transfer in", "amountIn": 300.0, "amountOut": 0}]})
    post("Bank Reconciliation", "/BankReconciliation", {"accountCode": "1200", "accountName": "Bank", "bankName": "Seed Bank", "bankAccountNo": "000111",
         "currency": "USD", "statementDate": NOW, "startDate": NOW, "endDate": DUE, "bankOpeningBal": 0, "bankClosingBal": 300.0, "remarks": "Seed",
         "lines": [{"lineNum": 1, "transType": "IT", "transDate": NOW, "refNo": "IT-1", "description": "Transfer in", "debit": 300.0, "credit": 0, "isReconciled": True}]})
    post("BP Reconciliation", "/BPReconciliation", {"partnerId": 1, "currency": "USD", "startDate": NOW, "endDate": DUE, "openingBalance": 0,
         "remarks": "Seed", "lines": [{"lineNum": 1, "transDate": NOW, "refNo": "INV-1", "description": "Seed", "docType": "ARInvoice", "docEntry": 1,
         "docNo": "1", "debit": 525.0, "credit": 0, "isReconciled": False}]})
    post("Budget", "/Budget", {"budgetName": "FY Seed", "description": "Seed", "fiscalYear": datetime.date.today().year, "startDate": NOW, "endDate": DUE,
         "currency": "USD", "lines": [{"lineNum": 1, "acctCode": "5200", "acctName": "Purchase Expense", "acctType": "X", "annual": 1200.0,
         "jan": 100.0, "feb": 100.0, "mar": 100.0, "apr": 100.0, "may": 100.0, "jun": 100.0, "jul": 100.0, "aug": 100.0, "sep": 100.0, "oct": 100.0,
         "nov": 100.0, "dec": 100.0}]})
    post("Conversion Differences", "/ConversionDifferences", {"postingDate": NOW, "documentDate": NOW, "localCurrency": "KHR", "systemCurrency": "USD",
         "conversionRate": 1, "accountFrom": "1100", "accountTo": "5400", "remarks": "Seed", "lines": [{"sourceType": "Account", "accountCode": "1100", "accountName": "Cash", "localBalance": 100.0, "historicalRate": 1}]})
    post("Exchange Rate Differences", "/ExchangeRateDifferences", {"postingDate": NOW, "documentDate": NOW, "currency": "USD", "baseCurrency": "USD",
         "closingRate": 1, "accountFrom": "1100", "accountTo": "5400", "remarks": "Seed", "lines": [{"sourceType": "Account", "accountCode": "1100", "accountName": "Cash", "foreignBalance": 100.0, "historicalRate": 1}]})


def invoice_to_pay(amount):
    """A posted AR invoice for `amount`, so a payment has something to settle."""
    d = doc("AR Invoice for payment", "/ARInvoice", sale(spec=[(1, 1, amount)]))
    return (d or {}).get("id"), amount


def stage_cards_payments():
    print("== CARDS, DEPOSITS, RUNS ==")
    post("Credit card setup", "/CreditCard", {"code": "VISA", "name": "Visa", "clearingAccount": "1200", "settlementAccount": "1100",
         "feePercent": 2.5, "feeAccount": "5200", "isActive": True})
    inv, amt = invoice_to_pay(100.0)
    ip = post("Incoming Payment by card", "/IncomingPayment", {"docDate": NOW, "dueDate": NOW, "cardCode": "C00001", "cardName": "Sample Customer Co.",
              "docTotal": amt, "currency": "USD", "exchangeRate": 1, "paymentMethod": "Card", "creditCardCode": "VISA", "bankAccount": "1200",
              "remarks": "Seed", "invoices": [{"invoiceId": inv, "appliedAmount": amt}]}) if inv else None
    pid = (ip or {}).get("id") if isinstance(ip, dict) else None
    if pid: post("Credit Card Settlement", "/CreditCardSettlement", {"cardCode": "VISA", "postingDate": NOW, "bankAccount": "1200", "feeAmount": 2.5,
                 "reference": "SET-1", "paymentIds": [pid]})
    inv2, amt2 = invoice_to_pay(50.0)
    ip2 = post("Incoming Payment (cash, for deposit)", "/IncomingPayment", {"docDate": NOW, "dueDate": NOW, "cardCode": "C00001", "cardName": "Sample Customer Co.",
               "docTotal": amt2, "currency": "USD", "exchangeRate": 1, "paymentMethod": "Cash", "bankAccount": "1100", "remarks": "Seed",
               "invoices": [{"invoiceId": inv2, "appliedAmount": amt2}]}) if inv2 else None
    pid2 = (ip2 or {}).get("id") if isinstance(ip2, dict) else None
    if pid2: post("Deposit", "/Deposit", {"depositDate": NOW, "bankAccount": "1200", "memo": "Seed", "incomingPaymentIds": [pid2]})
    post("Payment Run", "/PaymentRun/Execute", {"cardCodes": ["V00001"], "paymentMethod": "Cash", "bankAccount": "1100", "paymentDate": NOW, "bpType": "V",
         "dueDateFrom": NOW, "dueDateTo": DUE})
    post("Dunning term", "/DunningTerm", {"code": "DT1", "name": "Standard", "isActive": True, "remarks": "Seed",
         "levels": [{"level": 1, "daysOverdue": 7, "letterText": "Your invoice is overdue."}]})
    bp = next((b for b in (get("/BusinessPartnersMaster") or []) if b.get("code") == "C00001"), None)
    if bp and bp.get("id"):
        bp["dunningTerm"] = "DT1"
        put("Customer gets dunning term", f"/BusinessPartnersMaster/{bp['id']}", bp)
    past = (datetime.datetime.now(datetime.timezone.utc) - datetime.timedelta(days=45)).strftime("%Y-%m-%dT00:00:00Z")
    doc("AR Invoice, unpaid and overdue", "/ARInvoice", sale(extra={"postingDate": past, "dueDate": past}))
    later = NOW
    post("Dunning Run", "/Dunning/Run", {"runDate": later, "customerCodes": ["C00001"]})


def stage_assets():
    print("== FIXED ASSETS ==")
    for code, name, t_ in (("1500", "Fixed Assets", "A"), ("1510", "Accumulated Depreciation", "A")):
        post("Account " + code, "/ChartOfAccounts", {"acctCode": code, "acctName": name, "acctType": t_, "isGroup": False, "isActive": True,
             "level": 2, "currency": "USD", "fatherNum": "1000"})
    t = datetime.date.today()
    cap = (t.replace(day=1) - datetime.timedelta(days=40)).isoformat()   # capitalised well before this month
    post("Asset", "/AssetsMaster", {"itemNo": "AST001", "description": "Tractor", "itemType": "Fixed Asset", "manageItemBy": "None",
         "active": True, "capitalizationDate": cap, "usefulLifeMonths": 60, "remainingLifeMonths": 60, "depreciationMethod": "Straight Line",
         "acquisitionCost": 6000.0, "salvageValue": 0, "accumulatedDepreciation": 0, "assetStatus": "Active",
         "depreciationAccount": "5200", "accumulatedDepreciationAccount": "1510", "assetAccount": "1500"})
    post("Depreciation Run", "/DepreciationRun", {"periodYear": t.year, "periodMonth": t.month, "postingDate": NOW, "remarks": "Seed"})


def stage_close():
    print("== PERIOD-END CLOSING ==")
    t = datetime.date.today()
    first = t.replace(day=1).strftime("%Y-%m-%dT00:00:00Z")
    post("Period-End Closing", "/PeriodEndClosing", {"fromDate": first, "toDate": NOW, "postingDate": NOW, "retainedEarnings": "3100", "remarks": "Seed"})


STAGES = {"sales": stage_sales_more, "purchasing": stage_purchasing_more, "stock": stage_stock_more, "production": stage_production,
          "banking": stage_banking, "cards": stage_cards_payments, "assets": stage_assets, "close": stage_close}

if __name__ == "__main__":
    st.login()
    for name in (sys.argv[1:] or list(STAGES)):
        STAGES[name]()
    bad = [l for l, ok in st.results if not ok]
    print(f"\n{len(st.results) - len(bad)} ok / {len(st.results)} calls")
    if bad: print("FAILED:", ", ".join(bad))
