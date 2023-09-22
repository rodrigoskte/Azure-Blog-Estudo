using Blog.Data;
using Blog.Extensions;
using Blog.Models;
using Blog.ViewModels;
using Blog.ViewModels.Categories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Data.Common;

namespace Blog.Controllers
{
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IMemoryCache _memoryCache;
        private readonly BlogDataContext _blogDataContext;

        public CategoryController(IMemoryCache memoryCache, BlogDataContext blogDataContext)
        {
            _memoryCache = memoryCache;
            _blogDataContext = blogDataContext;
        }

        [HttpGet("v1/categories")]
        public async Task<IActionResult> GetAsync([FromServices] BlogDataContext context)
        {
            try
            {
                var categories = _memoryCache.GetOrCreate("CategoriesCache", entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                    return GetCategories();
                });

                return Ok(new ResultViewModel<List<Category>>(categories));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<List<Category>>($"Falha interna do servidor: {ex.Message}"));
            }
        }

        private List<Category> GetCategories()
        {
            return _blogDataContext.Categories.ToList();
        }

        [HttpGet("v1/categories/{id:int}")]
        public async Task<IActionResult> GetByIdAsync([FromRoute] int id,
                                                      [FromServices] BlogDataContext context)
        {
            try
            {
                var category = await context.Categories.FirstOrDefaultAsync(e => e.Id == id);
                if (category == null)
                    return NotFound(new ResultViewModel<Category>("Conteúdo não encontrado!"));

                return Ok(new ResultViewModel<Category>(category));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<Category>("Falha interna do servidor"));
            }
        }

        [HttpPost("v1/categories")]
        public async Task<IActionResult> PostAsync([FromBody] EditorCategoryViewModel model,
                                                   [FromServices] BlogDataContext context)
        {
            if (!ModelState.IsValid)
                return StatusCode(400, new ResultViewModel<Category>(ModelState.GetErrors()));

            try
            {
                var category = new Category
                {
                    Id = 0,
                    Name = model.Name,
                    Slug = model.Slug.ToLower(),
                };
                await context.Categories.AddAsync(category);
                await context.SaveChangesAsync();
                return Created($"v1/{category.Id}", new ResultViewModel<Category>(category));
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new ResultViewModel<Category>("Não foi possível cadastrar a categoria"));
            }
            catch (Exception e)
            {
                return StatusCode(500, new ResultViewModel<Category>("Falha interna do servidor"));
            }
        }

        [HttpPut("v1/categories/{id:int}")]
        public async Task<IActionResult> PutAsync([FromRoute] int id,
                                                  [FromBody] EditorCategoryViewModel model,
                                                  [FromServices] BlogDataContext context)
        {
            try
            {
                var category = await context.Categories.FirstOrDefaultAsync(e => e.Id == id);
                if (category == null)
                    return NotFound(new ResultViewModel<Category>("Conteúdo não encontrado!"));

                category.Name = model.Name;
                category.Slug = model.Slug.ToLower();

                context.Categories.Update(category);
                await context.SaveChangesAsync();
                return Ok(new ResultViewModel<Category>(category));
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new ResultViewModel<Category>("Não foi possível atualizar a categoria"));
            }
            catch (Exception e)
            {
                return StatusCode(500, new ResultViewModel<Category>("Falha interna do servidor"));
            }
        }

        [HttpDelete("v1/categories/{id:int}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] int id,
                                                     [FromServices] BlogDataContext context)
        {
            try
            {
                var category = await context.Categories.FirstOrDefaultAsync(e => e.Id == id);
                if (category == null)
                    return NotFound(new ResultViewModel<Category>("Conteúdo não encontrado!"));

                context.Categories.Remove(category);
                await context.SaveChangesAsync();
                return Ok(new ResultViewModel<Category>(category));
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new ResultViewModel<Category>("Não foi possível deletar a categoria"));
            }
            catch (Exception e)
            {
                return StatusCode(500, new ResultViewModel<Category>("Falha interna do servidor"));
            }
        }
    }
}
