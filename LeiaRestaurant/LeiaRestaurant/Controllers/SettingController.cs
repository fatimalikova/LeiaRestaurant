using Leia.Application.Dtos.SystemSettingDtos;
using Leia.Application.Helpers;
using Leia.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leia.Api.Controllers
{
    [ApiController]
    [Route("api/settings")]
    public class SettingController(ISystemSettingService systemSettingService) : ControllerBase
    {
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Get()
        {
            var result = await systemSettingService.GetAsync();
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(UpdateSystemSettingDto dto)
        {
            var result = await systemSettingService.UpdateAsync(dto);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }
    }
}