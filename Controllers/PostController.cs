using Diplom.Auth;
using Diplom.Entities;
using Diplom.Models;
using Diplom.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Diplom.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly PostService _postService;
        private readonly ApplicationContext _dbContext;
        public PostController(PostService post, ApplicationContext dbContext)
        {
            _postService = post;
            _dbContext = dbContext;
        }

        //[HttpPost("CreatePost")]
        //public IActionResult CreatePost(CreatePostModel model)
        //{

        //    Post post = new Post { Name = model.Name, Description = model.Description};

        //    _postService.CreatePost(post);
        //    return Ok(model);
        //}

        [HttpGet("GetAllPosts")]
        public IActionResult GetAllPosts()
        {
            var user = (User)HttpContext.Items["User"];
            var result = _postService.GetAllPosts(user.Id);
            return Ok(result);
        }



        [HttpPut("UpdatePost")]
        public async Task<IActionResult> UpdatePost([FromForm] UpdatePostModel model)
        {
            //var result = _postService.UpdatePost(model);
            //return Ok(result);
            //if (result == null)
            //{
            //    return NotFound();
            //}

            //// Оновити властивості картинки
            //result.ImagePath = model.Image;
            //if (model.Image != null)
            //{
            //    var fileName = $"{Guid.NewGuid().ToString()}.{model.Image.FileName.Split('.').Last()}";
            //    var path = Path.Combine(_webHostEnvironment.ContentRootPath, "Images", fileName);
            //    using (var stream = new FileStream(path, FileMode.Create))
            //    {
            //        await model.Image.CopyToAsync(stream);
            //    }
            //    result.Path = fileName;
            //}

            //await _repository.Update(image);
            //return NoContent();

            var user = (User)HttpContext.Items["User"];
            var result = _postService.UpdatePost(model, user.Id);
            return Ok(result);
        }

        [HttpDelete("DeletePost")]
        public IActionResult DeletePost(int postId)
        {
            _postService.DeletePost(postId);
            return Ok();
        }

        [HttpGet("GetPostById")]
        public IActionResult GetPostById(int postId)
        {
            var result = _postService.GetPostById(postId);
            //return Ok(result);
            if (result == null)
            {
                return NotFound();
            }

            var imagePath = Path.Combine("Images", result.ImagePath);

            if (!System.IO.File.Exists(imagePath))
            {
                return NotFound();
            }

            var imageContent = System.IO.File.ReadAllBytes(imagePath);
            return new FileContentResult(imageContent, "image/png");
            //var imageContent = System.IO.File.ReadAllBytes(imagePath);
            //var imageBase64 = Convert.ToBase64String(imageContent);
            //var imageUrl = string.Format("data:image/png;base64,{0}", imageBase64);
            //var imageViewModel = new Post
            //{
            //    Id = result.Id,
            //    Name = result.Name,
            //    Description = result.Description,
            //    ImagePath = imageUrl
            //};

            //return Ok(imageViewModel);
        }


        [HttpPost("CreatePost")]
        public async Task<IActionResult> CreatePost([FromForm] CreatePostModel model)
        {
            //var guid = Guid.NewGuid().ToString(); // генерація GUID для зображення
            //var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", guid); // шлях до файлу зображення

            //using (var stream = new FileStream(path, FileMode.Create))
            //{
            //    await model.Image.CopyToAsync(stream); // копіювання зображення в файл
            //}

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Images");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Image.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.Image.CopyToAsync(stream);// копіювання зображення в файл
            }
            var user = (User)HttpContext.Items["User"];
            Post post = new Post { 
                Name = model.Name,
                Description = model.Description, 
                ImagePath = fileName,
                UserId = user.Id
            };
       
            await _postService.CreatePost(post);

            return Ok(post);
        }
    }
}
