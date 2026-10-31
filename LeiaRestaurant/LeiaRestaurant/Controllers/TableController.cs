using Leia.Api.Extensions;
using Leia.Application.Dtos.TableDtos;
using Leia.Application.Helpers;
using Leia.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leia.Api.Controllers
{
    [ApiController]
    [Route("api/tables")]
    public class TableController(ITableService tableService) : ControllerBase
    {
        [HttpGet("my")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> GetMy()
        {
            var result = await tableService.GetMyAsync(User.GetUserId());
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPost]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> Create(CreateTableDto dto)
        {
            var result = await tableService.CreateAsync(User.GetUserId(), dto);
            return StatusCode(201, ResponseModelHelper.CreatedResult(result));
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> Update(Guid id, UpdateTableDto dto)
        {
            var result = await tableService.UpdateAsync(User.GetUserId(), id, dto);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await tableService.DeleteAsync(User.GetUserId(), id);
            return Ok(ResponseModelHelper.SuccessResult("Deleted."));
        }
    }
}