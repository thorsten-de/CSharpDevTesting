using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ShoppingCartService.BusinessLogic;
using ShoppingCartService.BusinessLogic.Exceptions;
using ShoppingCartService.Controllers.Models;

namespace ShoppingCartService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CouponController : ControllerBase
    {
        private readonly CouponManager _couponManager;

        public CouponController(CouponManager couponManager)
        {
            _couponManager = couponManager;
        }

        /// <summary>
        ///     Create a new coupon
        /// </summary>
        [HttpPost]
        public ActionResult<CouponDto> CreateCoupon([FromBody] CreateCouponDto createCoupon)
        {
            var result = _couponManager.CreateCoupon(createCoupon);

            return CreatedAtRoute("GetCoupon", new {id = result.Id}, result);
        }

        /// <summary>
        ///     Get cart by id
        /// </summary>
        /// <param name="id">Shopping cart id</param>
        [HttpGet("{id:length(24)}", Name = "GetCoupon")]
        public ActionResult<CouponDto> FindById(string id)
        {
            var coupon = _couponManager.FindCouponById(id);

            if (coupon == null) return NotFound();

            return coupon;
        }

        /// <summary>
        ///     Delete coupon
        /// </summary>
        /// <param name="id">Coupon id</param>
        [HttpDelete("{id:length(24)}")]
        public IActionResult DeleteCoupon(string id)
        {
            _couponManager.DeleteCuponById(id);

            return NoContent();
        }
    }
}