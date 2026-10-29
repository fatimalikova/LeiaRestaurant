using Leia.Api.Extensions;
using Leia.Application.Dtos.AddressDtos;
using Leia.Application.Helpers;
using Leia.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leia.Api.Controllers
{
    [ApiController]
    [Route("api/addresses")]
    public class AddressController(IAddressService addressService) : ControllerBase
    {
        [HttpGet("my")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMy()
        {
            var result = await addressService.GetMyAsync(User.GetUserId());
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Create(CreateAddressDto dto)
        {
            var result = await addressService.CreateAsync(User.GetUserId(), dto);
            return StatusCode(201, ResponseModelHelper.CreatedResult(result));
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Update(Guid id, UpdateAddressDto dto)
        {
            var result = await addressService.UpdateAsync(User.GetUserId(), id, dto);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPut("{id:guid}/default")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> SetDefault(Guid id)
        {
            var result = await addressService.SetDefaultAsync(User.GetUserId(), id);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await addressService.DeleteAsync(User.GetUserId(), id);
            return Ok(ResponseModelHelper.SuccessResult("Deleted."));
        }
    }
}