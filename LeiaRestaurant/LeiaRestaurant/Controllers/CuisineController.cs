using Leia.Application.Dtos.CuisineDtos;
using Leia.Application.Helpers;
using Leia.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leia.Api.Controllers
{
    [ApiController]
    [Route("api/cuisines")]
    public class CuisineController(ICuisineService cuisineService) : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetActive()
        {
            var result = await cuisineService.GetActiveAsync();
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllForAdmin()
        {
            var result = await cuisineService.GetAllAsync();
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateCuisineDto dto)
        {
            var result = await cuisineService.CreateAsync(dto);
            return StatusCode(201, ResponseModelHelper.CreatedResult(result));
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, UpdateCuisineDto dto)
        {
            var result = await cuisineService.UpdateAsync(id, dto);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await cuisineService.DeleteAsync(id);
            return Ok(ResponseModelHelper.SuccessResult("Deleted."));
        }
    }
}