using SenseNet.ContentRepository.Storage;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using SenseNet.ApplicationModel;
using SenseNet.Portal.Virtualization;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage.Security;

namespace SenseNet.Portal.Wall
{
    public class WallApi: GenericApi
    {
        // ===================================================================== Public methods
        /// <summary>
        /// Gets post from the given workspace using paging
        /// </summary>
        /// <param name="content">actual workspace</param>
        /// <param name="skip">how many posts to skip</param>
        /// <param name="pageSize">size of the page</param>
        /// <param name="rnd"></param>
        /// <returns></returns>
        [ODataFunction]
        public static string GetPosts(Content content, int skip, int pageSize, string rnd)
        {
            SetCurrentWorkspace(content.Path);
            var posts = DataLayer.GetPostsForWorkspace(content.Path).Skip(skip).Take(pageSize).ToList();
            var postsMarkup = WallHelper.GetWallPostsMarkup(content.Path, posts);
            return postsMarkup;
        }

        /// <summary>
        /// Creates a manually written Post in the Content Repository and returns Post markup.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        [ODataAction]
        public static string CreatePost(Content content, string text)
        {
            AssertPermission(PlaceholderPath);
            if (string.IsNullOrEmpty(text))
                return null;

            SetCurrentWorkspace(content.Path);
            var post = DataLayer.CreateManualPost(content.Path, text);
            var postInfo = new PostInfo(post);
            var postMarkup = WallHelper.GetPostMarkup(postInfo, content.Path);
            return postMarkup;
        }

        /// <summary>
        /// Creates a comment for a Post and returns Comment markup.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="postId"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        [ODataAction]
        public static string CreateComment(Content content, string postId, string text)
        {
            AssertPermission(PlaceholderPath);
            if (string.IsNullOrEmpty(text))
                return null;

            SetCurrentWorkspace(content.Path);
            var comment = DataLayer.CreateComment(postId, content.Path, text);

            var commentMarkup = WallHelper.GetCommentMarkup(comment.CreationDate, User.Current as User, text, comment.Id, new LikeInfo(), comment);
            return commentMarkup;
        }

        /// <summary>
        /// Creates a like for a Post/Comment and returns Like markup.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="itemId"></param>
        /// <param name="fullMarkup"></param>
        /// <returns></returns>
        [ODataAction]
        public static string Like(Content content, string itemId, bool fullMarkup)
        {
            AssertPermission(PlaceholderPath);
            SetCurrentWorkspace(content.Path);
            var parentId = 0;
            DataLayer.CreateLike(itemId, content.Path, out parentId);

            var likeInfo = new LikeInfo(parentId);
            return fullMarkup ? likeInfo.GetLongMarkup() : likeInfo.GetShortMarkup();
        }

        /// <summary>
        /// Deletes a like for a Post/Comment and returns Like markup.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="itemId"></param>
        /// <param name="fullMarkup"></param>
        /// <returns></returns>
        [ODataAction]
        public static string Unlike(Content content, string itemId, bool fullMarkup)
        {
            AssertPermission(PlaceholderPath);
            SetCurrentWorkspace(content.Path);
            var parentId = 0;
            DataLayer.DeleteLike(itemId, out parentId);

            var likeInfo = new LikeInfo(parentId);
            return fullMarkup ? likeInfo.GetLongMarkup() : likeInfo.GetShortMarkup();
        }

        /// <summary>
        /// Returns full markup of like list corresponding to itemId
        /// </summary>
        /// <param name="content"></param>
        /// <param name="itemId"></param>
        /// <param name="rnd"></param>
        /// <returns></returns>
        [ODataFunction]
        public static string GetLikeList(Content content, string itemId, string rnd)
        {
            if (!HasPermission())
                return JsonConvert.SerializeObject(SNSR.GetString(Compatibility.SR.Wall.PleaseLogIn));

            SetCurrentWorkspace(content.Path);
            var id = PostInfo.GetIdFromClientId(itemId);

            // create like markup
            var likeInfo = new LikeInfo(id);
            var likelist = new StringBuilder();
            foreach (var likeitem in likeInfo.LikeUsers)
            {
                var likeuser = likeitem as User;
                likelist.Append(WallHelper.GetLikeListItemMarkup(likeuser));
            }

            return likelist.ToString();
        }
        
        /// <summary>
        /// Shares a post.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="itemId"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        [ODataAction]
        public static string Share(Content content, int itemId, string text)
        {
            AssertPermission(PlaceholderPath);
            if (!WallHelper.HasWallPermission(content.Path))
            {
                return "403";
            }

            SetCurrentWorkspace(content.Path);
            try
            {
                DataLayer.CreateSharePost(content.Path, text, itemId);
            }
            catch (SenseNetSecurityException)
            {
                return "403";
            }

            return null;
        }


        // ===================================================================== Helper methods
        private const string PlaceholderPath = "/Root/System/PermissionPlaceholders/Wall-mvc";

        public static bool HasPermission()
        {
            var permissionContent = Node.LoadNode(PlaceholderPath);
            var nopermission = (permissionContent == null || !permissionContent.Security.HasPermission(PermissionType.RunApplication));
            return !nopermission;
        }
    }
}
