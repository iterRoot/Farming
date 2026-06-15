// using AutoMapper;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using FarmingApi.Core;
// using FarmingApi.Modules.Financials.ChartOfAccounts;
// using FarmingApi.Modules.Financials.JournalEntry;
// using FarmingApi.Modules.Financials.IncomingPayment;
// using FarmingApi.Modules.Financials.OutgoingPayment;

// namespace FarmingApi.Modules.Financials.BankReconciliation;

// public class BankReconciliationController : MyController
// {
//     private readonly IMapper                       _mapper;
//     private readonly IBankReconciliationRepository _repository;
//     private readonly IChartOfAccountsRepository    _coaRepository;
//     private readonly IJournalEntryRepository       _jeRepository;
//     private readonly IIncomingPaymentRepository    _ipRepository;
//     private readonly IOutgoingPaymentRepository    _opRepository;

//     public BankReconciliationController(
//         IBankReconciliationRepository repository,
//         IChartOfAccountsRepository    coaRepository,
//         IJournalEntryRepository       jeRepository,
//         IIncomingPaymentRepository    ipRepository,
//         IOutgoingPaymentRepository    opRepository,
//         IMapper mapper)
//     {
//         _mapper        = mapper;
//         _repository    = repository;
//         _coaRepository = coaRepository;
//         _jeRepository  = jeRepository;
//         _ipRepository  = ipRepository;
//         _opRepository  = opRepository;
//     }

//     // ═══════════════════════════════════════════════════════════════
//     // GET ALL
//     // ═══════════════════════════════════════════════════════════════
//     [AllowAnonymous]
//     [HttpGet]
//     public IActionResult Gets(
//         [FromQuery] string? accountCode = null,
//         [FromQuery] string? status      = null)
//     {
//         var query = _repository.GetAll().Include(b => b.Lines).AsQueryable();
//         if (!string.IsNullOrEmpty(accountCode)) query = query.Where(b => b.AccountCode == accountCode);
//         if (!string.IsNullOrEmpty(status))      query = query.Where(b => b.Status == status);

//         var list    = query.OrderByDescending(b => b.StatementDate).ToList();
//         var results = list.Select(b => MapWithStats(b)).ToList();
//         return Ok(results);
//     }

//     // ═══════════════════════════════════════════════════════════════
//     // GET BY ID
//     // ═══════════════════════════════════════════════════════════════
//     [AllowAnonymous]
//     [HttpGet("{id:int}")]
//     public IActionResult Get(int id)
//     {
//         var br = _repository.GetAll()
//             .Include(b => b.Lines)
//             .FirstOrDefault(b => b.Id == id);

//         if (br == null) return NotFound($"Bank Reconciliation {id} not found");
//         return Ok(MapWithStats(br));
//     }

//     // ═══════════════════════════════════════════════════════════════
//     // CREATE
//     // ═══════════════════════════════════════════════════════════════
//     [HttpPost]
//     public IActionResult Create([FromBody] BankReconciliationRequest request)
//     {
//         if (!ModelState.IsValid) return BadRequest(ModelState);
//         if (string.IsNullOrWhiteSpace(request.AccountCode))
//             return BadRequest("Account Code is required");
//         if (request.StartDate >= request.EndDate)
//             return BadRequest("End Date must be after Start Date");

//         // Load GL account info
//         var acct = _coaRepository.GetAll()
//             .FirstOrDefault(a => a.AcctCode == request.AccountCode);
//         if (acct == null)
//             return BadRequest($"Account '{request.AccountCode}' not found in Chart of Accounts");

//         var entity = _mapper.Map<BankReconciliation>(request);
//         entity.ReconcNo      = GenerateReconcNo();
//         entity.AccountName   = acct.AcctName;
//         entity.GlOpeningBal  = acct.DebitBalance - acct.CreditBalance;  // opening GL balance
//         entity.GlClosingBal  = acct.Balance;                             // current GL balance
//         entity.Status        = "O";
//         entity.VersionNum    = 1;
//         entity.CreatedAt     = DateTime.UtcNow;
//         entity.InActive      = false;

//         // Add lines
//         int lineNum = 1;
//         decimal runningBal = request.BankOpeningBal;
//         foreach (var lr in request.Lines.OrderBy(l => l.TransDate))
//         {
//             var line = _mapper.Map<BankReconciliationLine>(lr);
//             line.LineNum   = lineNum++;
//             runningBal    += line.Debit - line.Credit;
//             line.Balance   = runningBal;
//             line.CreatedAt = DateTime.UtcNow;
//             line.InActive  = false;
//             entity.Lines.Add(line);
//         }

//         RecalculateTotals(entity);
//         _repository.Add(entity);
//         _repository.Commit();

//         return Ok(new
//         {
//             message    = "Bank Reconciliation created",
//             id         = entity.Id,
//             reconcNo   = entity.ReconcNo,
//             difference = entity.Difference
//         });
//     }

//     // ═══════════════════════════════════════════════════════════════
//     // UPDATE
//     // ═══════════════════════════════════════════════════════════════
//     [HttpPut("{id:int}")]
//     public IActionResult Update(int id, [FromBody] BankReconciliationUpdateRequest request)
//     {
//         var br = _repository.GetAll()
//             .Include(b => b.Lines)
//             .FirstOrDefault(b => b.Id == id);

//         if (br == null) return NotFound();
//         if (br.Status == "C") return BadRequest("Cannot edit a Closed reconciliation");

//         _mapper.Map(request, br);
//         br.UpdatedAt   = DateTime.UtcNow;
//         br.VersionNum += 1;

//         // Replace lines
//         br.Lines.Clear();
//         int lineNum = 1;
//         decimal runningBal = request.BankOpeningBal;
//         foreach (var lr in request.Lines.OrderBy(l => l.TransDate))
//         {
//             var line = _mapper.Map<BankReconciliationLine>(lr);
//             line.LineNum   = lineNum++;
//             runningBal    += line.Debit - line.Credit;
//             line.Balance   = runningBal;
//             line.CreatedAt = DateTime.UtcNow;
//             line.InActive  = false;
//             br.Lines.Add(line);
//         }

//         RecalculateTotals(br);
//         _repository.Update(br);
//         _repository.Commit();
//         return NoContent();
//     }

//     // ═══════════════════════════════════════════════════════════════
//     // RECONCILE LINES — mark specific lines as matched
//     // ═══════════════════════════════════════════════════════════════
//     [HttpPost("{id:int}/Reconcile")]
//     public IActionResult ReconcileLines(int id, [FromBody] ReconcileLineRequest request)
//     {
//         var br = _repository.GetAll()
//             .Include(b => b.Lines)
//             .FirstOrDefault(b => b.Id == id);

//         if (br == null) return NotFound();
//         if (br.Status == "C") return BadRequest("Cannot modify a Closed reconciliation");

//         int count = 0;
//         foreach (var line in br.Lines.Where(l => request.LineIds.Contains(l.Id)))
//         {
//             line.IsReconciled   = true;
//             line.ReconciledDate = DateTime.UtcNow;
//             line.ReconciledRef  = request.ReconciledRef;
//             count++;
//         }

//         RecalculateTotals(br);
//         br.UpdatedAt = DateTime.UtcNow;
//         _repository.Update(br);
//         _repository.Commit();

//         return Ok(new
//         {
//             message      = $"{count} line(s) marked as reconciled",
//             difference   = br.Difference,
//             reconciledAmt= br.ReconciledAmt,
//         });
//     }

//     // ═══════════════════════════════════════════════════════════════
//     // UNRECONCILE LINES — undo reconciliation
//     // ═══════════════════════════════════════════════════════════════
//     [HttpPost("{id:int}/Unreconcile")]
//     public IActionResult UnreconcileLines(int id, [FromBody] ReconcileLineRequest request)
//     {
//         var br = _repository.GetAll()
//             .Include(b => b.Lines)
//             .FirstOrDefault(b => b.Id == id);

//         if (br == null) return NotFound();
//         if (br.Status == "C") return BadRequest("Cannot modify a Closed reconciliation");

//         foreach (var line in br.Lines.Where(l => request.LineIds.Contains(l.Id)))
//         {
//             line.IsReconciled   = false;
//             line.ReconciledDate = null;
//             line.ReconciledRef  = null;
//         }

//         RecalculateTotals(br);
//         br.UpdatedAt = DateTime.UtcNow;
//         _repository.Update(br);
//         _repository.Commit();

//         return Ok(new { message = "Lines unreconciled", difference = br.Difference });
//     }

//     // ═══════════════════════════════════════════════════════════════
//     // CLOSE — finalize reconciliation (difference must = 0)
//     // ═══════════════════════════════════════════════════════════════
//     [HttpPost("{id:int}/Close")]
//     public IActionResult Close(int id)
//     {
//         var br = _repository.GetAll()
//             .Include(b => b.Lines)
//             .FirstOrDefault(b => b.Id == id);

//         if (br == null) return NotFound();
//         if (br.Status == "C") return BadRequest("Already closed");

//         RecalculateTotals(br);

//         if (Math.Abs(br.Difference) > 0.01m)
//             return BadRequest($"Cannot close — Difference is {br.Difference:N2}. Must be 0.00 to close.");

//         br.Status    = "C";
//         br.UpdatedAt = DateTime.UtcNow;
//         _repository.Update(br);
//         _repository.Commit();

//         return Ok(new { message = "Bank Reconciliation closed successfully", reconcNo = br.ReconcNo });
//     }

//     // ═══════════════════════════════════════════════════════════════
//     // REOPEN
//     // ═══════════════════════════════════════════════════════════════
//     [HttpPost("{id:int}/Reopen")]
//     public IActionResult Reopen(int id)
//     {
//         var br = _repository.GetSingle(b => b.Id == id);
//         if (br == null) return NotFound();
//         if (br.Status != "C") return BadRequest("Only Closed reconciliations can be reopened");

//         br.Status    = "O";
//         br.UpdatedAt = DateTime.UtcNow;
//         _repository.Update(br);
//         _repository.Commit();

//         return Ok(new { message = "Bank Reconciliation reopened" });
//     }

//     // ═══════════════════════════════════════════════════════════════
//     // DELETE
//     // ═══════════════════════════════════════════════════════════════
//     [HttpDelete("{id:int}")]
//     public IActionResult Delete(int id)
//     {
//         var br = _repository.GetSingle(b => b.Id == id);
//         if (br == null) return NotFound();
//         if (br.Status == "C") return BadRequest("Cannot delete a Closed reconciliation");

//         br.DeletedAt = DateTime.UtcNow;
//         _repository.Remove(br);
//         _repository.Commit();
//         return NoContent();
//     }

//     // ═══════════════════════════════════════════════════════════════
//     // LOAD TRANSACTIONS — pull payments + JE from existing records
//     // Returns unreconciled transactions for the account and date range
//     // ═══════════════════════════════════════════════════════════════
//     [AllowAnonymous]
//     [HttpPost("LoadTransactions")]
//     public IActionResult LoadTransactions([FromBody] LoadTransactionsRequest request)
//     {
//         var lines = new List<BankReconciliationLineResponse>();
//         int lineNum = 1;

//         // ── Incoming Payments (money IN) ──────────────────────────
//         var incoming = _ipRepository.GetAll()
//             .Include(p => p.Lines)
//             .Where(p => p.DocDate >= request.FromDate
//                      && p.DocDate <= request.ToDate
//                      && p.Status  != "V"
//                      && p.Lines.Any(l => l.AccountCode == request.AccountCode ||
//                                          p.BankAccount  == request.AccountCode))
//             .ToList();

//         foreach (var ip in incoming)
//         {
//             lines.Add(new BankReconciliationLineResponse
//             {
//                 LineNum     = lineNum++,
//                 TransType   = "GL",
//                 TransDate   = ip.DocDate,
//                 RefNo       = ip.DocNo,
//                 Description = $"Incoming Payment – {ip.CardName ?? ip.CardCode ?? ""}",
//                 DocType     = "KPYI",
//                 DocEntry    = ip.Id,
//                 DocNo       = ip.DocNo,
//                 Debit       = ip.DocTotal,   // money IN → debit bank
//                 Credit      = 0,
//                 IsReconciled= false,
//             });
//         }

//         // ── Outgoing Payments (money OUT) ─────────────────────────
//         var outgoing = _opRepository.GetAll()
//             .Include(p => p.Lines)
//             .Where(p => p.DocDate >= request.FromDate
//                      && p.DocDate <= request.ToDate
//                      && p.Status  != "V"
//                      && (p.BankAccount == request.AccountCode ||
//                          p.Lines.Any(l => l.AccountCode == request.AccountCode)))
//             .ToList();

//         foreach (var op in outgoing)
//         {
//             lines.Add(new BankReconciliationLineResponse
//             {
//                 LineNum     = lineNum++,
//                 TransType   = "GL",
//                 TransDate   = op.DocDate,
//                 RefNo       = op.DocNo,
//                 Description = $"Outgoing Payment – {op.CardName ?? op.CardCode ?? ""}",
//                 DocType     = "KPYO",
//                 DocEntry    = op.Id,
//                 DocNo       = op.DocNo,
//                 Debit       = 0,
//                 Credit      = op.DocTotal,   // money OUT → credit bank
//                 IsReconciled= false,
//             });
//         }

//         // ── Journal Entries that touch this account ───────────────
//         var jes = _jeRepository.GetAll()
//             .Include(j => j.Lines)
//             .Where(j => j.TransDate >= request.FromDate
//                      && j.TransDate <= request.ToDate
//                      && j.Status    != "V"
//                      && j.Lines.Any(l => l.AccountCode == request.AccountCode))
//             .ToList();

//         foreach (var je in jes)
//         {
//             foreach (var jl in je.Lines.Where(l => l.AccountCode == request.AccountCode))
//             {
//                 lines.Add(new BankReconciliationLineResponse
//                 {
//                     LineNum     = lineNum++,
//                     TransType   = "GL",
//                     TransDate   = je.TransDate,
//                     RefNo       = je.JrnlNo,
//                     Description = je.Memo ?? $"Journal Entry – {je.JrnlNo}",
//                     DocType     = "KJRN",
//                     DocEntry    = je.Id,
//                     DocNo       = je.JrnlNo,
//                     Debit       = jl.Debit,
//                     Credit      = jl.Credit,
//                     IsReconciled= false,
//                 });
//             }
//         }

//         return Ok(lines.OrderBy(l => l.TransDate).ToList());
//     }

//     // ═══════════════════════════════════════════════════════════════
//     // HELPERS
//     // ═══════════════════════════════════════════════════════════════
//     private void RecalculateTotals(BankReconciliation br)
//     {
//         br.ReconciledAmt   = br.Lines.Where(l => l.IsReconciled).Sum(l => l.Debit - l.Credit);
//         br.UnreconciledAmt = br.Lines.Where(l => !l.IsReconciled).Sum(l => l.Debit - l.Credit);

//         // Difference = GL Closing Balance - Bank Statement Closing Balance
//         // Should be 0 when fully reconciled
//         br.Difference = br.GlClosingBal - br.BankClosingBal;
//     }

//     private BankReconciliationResponse MapWithStats(BankReconciliation br)
//     {
//         var response = _mapper.Map<BankReconciliationResponse>(br);
//         response.TotalLines      = br.Lines.Count;
//         response.ReconciledLines = br.Lines.Count(l => l.IsReconciled);
//         response.PendingLines    = br.Lines.Count(l => !l.IsReconciled);
//         return response;
//     }

//     private string GenerateReconcNo()
//     {
//         var year   = DateTime.Now.Year;
//         var prefix = $"BR-{year}-";
//         var last   = _repository.GetAll()
//             .Where(b => b.ReconcNo.StartsWith(prefix))
//             .OrderByDescending(b => b.Id)
//             .FirstOrDefault();

//         if (last == null) return $"{prefix}00001";
//         var parts = last.ReconcNo.Split('-');
//         return parts.Length >= 3 && int.TryParse(parts[2], out int n)
//             ? $"{prefix}{(n + 1):D5}"
//             : $"{prefix}00001";
//     }
// }