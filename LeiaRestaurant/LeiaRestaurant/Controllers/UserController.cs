using Leia.Application.Dtos.AppUserDtos;
using Leia.Application.Dtos.Common;
using Leia.Application.Helpers;
using Leia.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leia.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController(IAppUserService appUserService) : ControllerBase
    {
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] PaginationDto dto)
        {
            var result = await appUserService.GetAllAsync(dto);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await appUserService.GetByIdAsync(id);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateUserDto dto)
        {
            var result = await appUserService.CreateAsync(dto);
            return StatusCode(201, ResponseModelHelper.CreatedResult(result));
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, UpdateUserDto dto)
        {
            var result = await appUserService.UpdateAsync(id, dto);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }
    }
}