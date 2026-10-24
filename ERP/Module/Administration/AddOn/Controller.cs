//
//
// using AutoMapper;
// using FarmingApi.Core;
// using FarmingApi.Modules.Banking.IncomingPayment;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
//
// namespace FarmingApi.Modules.Administration.AddOn;
//
// public class AddOnController : MyController
// {
//     private readonly IMapper _maper;
//     private readonly IIAddOnRepository _repository;
//     private readonly MyDbContext _context;
//
//
//     public AddOnController(
//         IIAddOnRepository repository,
//         MyDbContext context, 
//         IMapper mapper)
//     {
//         _maper = mapper;
//         _context = context;
//         _repository = repository;
//         
//
//     }
//
//     [AllowAnonymous]
//     [HttpGet]
//     public IActionResult Get()
//     {
//         var iQueryable = _repository.GetAll().Include(p => p.A)   
//     }
// }
//
//
//
