using Justin.EntityFramework.Model;
using Justin.EntityFramework.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Justin.EntityFramework.Controller {

    [Authorize]
    [ApiController]
    [Produces("application/json")]
    public abstract class BaseController<TEntity>(IBaseService<TEntity> baseService) : ControllerBase where TEntity : Base {

        private readonly IBaseService<TEntity> _baseService = baseService;
        
        [HttpGet]
        public virtual async Task<IActionResult> Get() {

            var pagedOptions = PagedOptions.GetPagedOptions(Request);
            if(pagedOptions == null) {
                var data = await _baseService.GetAll();
                if(data == null) return NotFound();
                return Ok(data);
            }
            else {
                var data = await _baseService.GetByFilter(pagedOptions);
                if(data.Data == null) return NotFound();
                return Ok(data);
            }

        }

        [HttpGet("GetById")]
        public virtual async Task<IActionResult> GetById(int id) {

            var data = await _baseService.GetById(id);
            if(data == null) return NotFound();
            return Ok(data);

        }

        [HttpPost]
        public virtual async Task<IActionResult> Post([FromBody] TEntity data) {

            var result = await _baseService.Save(data, true);
            return CreatedAtAction(nameof(Get), new { id = result.Entity.Id }, result);

        }

        [HttpPut]
        public virtual async Task<IActionResult> Put(int id, [FromBody] TEntity data) {

            if(id != data.Id) return BadRequest();

            var existingData = await _baseService.GetById(id);
            if(existingData == null) return NotFound();

            var result = await _baseService.Update(data, true);
            return Ok(result);

        }

        [HttpDelete]
        public virtual async Task<IActionResult> Delete(int id) {

            var existingData = await _baseService.GetById(id);
            if(existingData == null) return NotFound();

            var result = await _baseService.Delete(existingData, true);
            return Ok(result);

        }

    }
}
