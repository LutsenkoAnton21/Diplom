using Diplom.Entities;
using Diplom.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Diplom.Interfacec
{
    interface IPostRepository
    {
        Task CreatePost(Post post);
        public List<Post> GetAllPosts(string userId);
        public Post UpdatePost(UpdatePostModel model, string userId);
        public void DeletePost(int postId);
        public Post GetPostById(int postId);
    }
}
