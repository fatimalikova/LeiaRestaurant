using Leia.Application.Dtos.AllergenDtos;
using Leia.Application.Helpers;
using Leia.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leia.Api.Controllers
{
    [ApiController]
    [Route("api/allergens")]
    public class AllergenController(IAllergenService allergenService) : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetActive()
        {
            var result = await allergenService.GetActiveAsync();
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllForAdmin()
        {
            var result = await allergenService.GetAllAsync();
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateAllergenDto dto)
        {
            var result = await allergenService.CreateAsync(dto);
            return StatusCode(201, ResponseModelHelper.CreatedResult(result));
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, UpdateAllergenDto dto)
        {
            var result = await allergenService.UpdateAsync(id, dto);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await allergenService.DeleteAsync(id);
            return Ok(ResponseModelHelper.SuccessResult("Deleted."));
        }
    }
}