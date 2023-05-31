using Diplom.Entities;
using Diplom.Interfacec;
using Diplom.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Diplom.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly ApplicationContext _dbContext;

        public PostRepository(ApplicationContext databaseOptions)
        {
            _dbContext = databaseOptions;
        }

        public async Task CreatePost(Post post)
        {
           await _dbContext.Posts.AddAsync(post);
           await _dbContext.SaveChangesAsync();
        }

        public List<Post> GetAllPosts(string userId)
        {
            return _dbContext.Posts.Where(w => w.UserId == userId || w.UserId == null).ToList();
        }

        public Post UpdatePost(UpdatePostModel model, string userId)
        {
            var oldModel = _dbContext.Posts.FirstOrDefault(x => x.PostId == model.PostId && x.UserId == userId);
            if (oldModel != null)
            {
                oldModel.Name = model.Name;
                oldModel.Description = model.Description;
               // oldModel.ImagePath = model.Image;
                _dbContext.SaveChanges();
            }
            return oldModel;
        }

        public void DeletePost(int postId)
        {
            var post = _dbContext.Posts.FirstOrDefault(x => x.PostId == postId);
            if (post != null)
            {
                _dbContext.Posts.Remove(post);
                _dbContext.SaveChanges();
            }
        }

        public Post GetPostById(int postId)
        {
            return _dbContext.Posts.FirstOrDefault(x => x.PostId == postId) ;
        }
    }
}
