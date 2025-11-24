using System;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;

namespace FarmingApi.Modules.Production
{
    [ApiController]
    // [Route("api/[controller]")]
    public class ProductionController : MyController
    {
        private readonly IMapper _mapper;
        private readonly IRepository<Production> _repository;

        public ProductionController(
            IRepository<Production> repository,
            IMapper mapper
        )
        {
            _mapper = mapper;
            _repository = repository;
        }

        // GET api/milkproduction
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Gets()
        {
            var queryable = _repository.GetAll();
            var results = _mapper.ProjectTo<ProductionResponse>(queryable).ToList();
            return Ok(results);
        }
    }
}
