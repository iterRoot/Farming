using System;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;

namespace FarmingApi.Modules.RawMilk
{
    [ApiController]
    // [Route("api/[controller]")]
    public class RawMilkController : MyController
    {
        private readonly IMapper _mapper;
        private readonly IRepository<RawMilk> _repository;

        public RawMilkController(
            IRepository<RawMilk> repository,
            IMapper mapper
        )
        {
            _mapper = mapper;
            _repository = repository;
        }

        // GET api/milkproduction
        [AllowAnonymous]
        [HttpGet ("GetRawMail")]
        public IActionResult Gets()
        {
            var queryable = _repository.GetAll();
            var results = _mapper.ProjectTo<RawMilkResponse>(queryable).ToList();
            return Ok(results);
        }

        [HttpGet("{id:int}")]
        public IActionResult GetById([FromRoute] int id)
        {
            var entity = _repository.GetAll().FirstOrDefault(x => x.Id == id);
            if (entity == null) return NotFound();
            var dto = _mapper.Map<RawMilkResponse>(entity);
            return Ok(dto);
        }
    }
}
