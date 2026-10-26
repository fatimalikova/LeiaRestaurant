using Leia.Api.Extensions;
using Leia.Application.Dtos.ProfileDtos;
using Leia.Application.Helpers;
using Leia.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leia.Api.Controllers
{
    [ApiController]
    [Route("api/profile")]
    public class ProfileController(IProfileService profileService) : ControllerBase
    {
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get()
        {
            var result = await profileService.GetAsync(User.GetUserId());
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Update(UpdateProfileDto dto)
        {
            var result = await profileService.UpdateAsync(User.GetUserId(), dto);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPut("password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            await profileService.ChangePasswordAsync(User.GetUserId(), dto);
            return Ok(ResponseModelHelper.SuccessResult("Password changed."));
        }
    }
}