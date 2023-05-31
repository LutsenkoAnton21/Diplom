using Diplom.Entities;
using Diplom.Interfacec;
using Diplom.Models;
using Diplom.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Diplom.Services
{
    public class PostService
    {
        private readonly IPostRepository _postRepository;
        public PostService(PostRepository postRepository)
        {
            _postRepository = postRepository;
        }

        public async Task CreatePost(Post post)
        {
            await _postRepository.CreatePost(post);
        }

        public List<Post> GetAllPosts(string userId)
        {
            return _postRepository.GetAllPosts(userId);
        }

        public Post UpdatePost(UpdatePostModel model, string userId)
        {
            return _postRepository.UpdatePost(model, userId);
        }

        public void DeletePost(int postId)
        {
            _postRepository.DeletePost(postId);
        }
        public Post GetPostById(int postId)
        {
            return _postRepository.GetPostById(postId);
        }
    }
}
