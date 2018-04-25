using System;
using System.Collections.Generic;
using System.Linq;
using SenseNet.ContentRepository.Storage;
using SenseNet.Search;
using SenseNet.ContentRepository;

namespace SenseNet.Portal.Portlets
{
    /// <summary>
    /// <c>TagManager</c> class is for managing tags on contents with Lucene.Net accelleration.
    /// Implements back end for tag-related user controls and portlets.
    /// </summary>
    public static class TagManager
    {
        /// <summary>
        /// Returns the occurrencies of every used tag.
        /// </summary>
        /// <returns>Dictionary key: Tag; Value: overall occurrency on content.</returns>
        public static Dictionary<string, int> GetTagOccurrencies()
        {
            return GetTagOccurrencies(null, null);
        }

        /// <summary>
        /// Gets the tag occurrencies.
        /// </summary>
        /// <param name="paths">The paths.</param>
        /// <param name="types">The types.</param>
        /// <returns>Dictionary key: Tag; Value: overall occurrency on content.</returns>
        public static Dictionary<string, int> GetTagOccurrencies(string[] paths, string[] types)
        {
            // direct Lucene access is not supported anymore from a general component
            return new Dictionary<string, int>();

            //using (var readerFrame = LuceneManager.GetIndexReaderFrame())
            //{
            //    var reader = readerFrame.IndexReader;

            //    var pathList = new List<string>();
            //    var typeList = new List<string>();

            //    if (types != null)
            //    {
            //        typeList.AddRange(types.Select(type => type.ToLower()));
            //    }
            //    if (paths != null)
            //    {
            //        pathList.AddRange(paths.Select(path => path.ToLower()));
            //    }



            //    var occ = new Dictionary<string, int>();
            //    var terms = reader.Terms(new Term("Tags", "*"));

            //    do
            //    {
            //        if (terms.Term().Field() == "Tags")
            //        {
            //            var docs = reader.TermDocs(terms.Term());
            //            var count = 0;


            //            while (docs.Next())
            //            {
            //                var pathValid = false;



            //                var doc = reader.Document(docs.Doc()); // lucene document to examine for search criterias

            //                if (pathList.Count() > 0)
            //                {
            //                    if (pathList.Any(path => doc.Get("Path").StartsWith(path)))
            //                    {
            //                        pathValid = true;
            //                    }
            //                }
            //                else
            //                {
            //                    pathValid = true;
            //                }

            //                var typeValid = typeList.Count() <= 0 || typeList.Contains(doc.Get("Type"));


            //                if (typeValid && pathValid)
            //                {
            //                    count++;

            //                }
            //            }


            //            if (!occ.ContainsKey(terms.Term().Text().ToLower()) && count > 0)
            //            {
            //                occ.Add(terms.Term().Text(), count);



            //            }
            //        }
            //    } while (terms.Next());

            //    terms.Close();
            //    return occ;
            //}
        }

        /// <summary>
        /// Returns the ID-s of Nodes, that contains the tag given as parameter.
        /// </summary>
        /// <param name="tag">Searched tag as String.</param>
        /// <returns>Node Id list.</returns>
        public static List<int> GetNodeIds(string tag)
        {
            return GetNodeIds(tag, new string[] { }, new string[] { });
        }

        /// <summary>
        /// Returns the ID-s of Nodes, that contains the tag given as parameter.
        /// </summary>
        /// <param name="tag">Searched tag as String.</param>
        /// <param name="searchPathArray">Paths to search in.</param>
        /// <param name="contentTypeArray">Types to search.</param>
        /// <param name="queryFilter"></param>
        /// <returns>Node Id list.</returns>
        public static List<int> GetNodeIds(string tag, string[] searchPathArray, string[] contentTypeArray, string queryFilter = "")
        {
            if (string.IsNullOrEmpty(tag))
                return new List<int>();

            tag = tag.ToLower();
            var pathList = string.Empty;
            var typeList = string.Empty;

            if (searchPathArray.Any() && contentTypeArray.Any())
            {
                pathList = "+Path:(";
                foreach (var path in searchPathArray)
                {
                    pathList = string.Concat(pathList, path, "* ");
                }
                pathList = string.Concat(pathList, ")");

                typeList = "+Type:(";
                foreach (var path in contentTypeArray)
                {
                    typeList = string.Concat(typeList, path, " ");
                }
                typeList = string.Concat(typeList, ")");

            }

            if (tag == string.Empty)
                return new List<int>();

            ContentQuery query;

            if (!string.IsNullOrEmpty(pathList) && !string.IsNullOrEmpty(typeList))
            {
                query = ContentQuery.CreateQuery($"+Tags:\"{tag.Trim()}\" {pathList} {typeList} {queryFilter}");
            }
            else
            {
                query = ContentQuery.CreateQuery($"+Tags:\"{tag.Trim()}\" {queryFilter}");
            }

            return query.Execute().Identifiers.ToList();
        }

        /// <summary>
        /// Returns all tags currently in use and containing the given string fragment.
        /// </summary>
        /// <param name="filter">Filter string.</param>
        /// <param name="pathList">The path list.</param>
        /// <returns>
        /// List of tags in use, and containing the filter string.
        /// </returns>
        public static List<string> GetAllTags(string filter, List<string> pathList)
        {
            var tags = GetTagOccurrencies(pathList != null ? pathList.ToArray() : null, null);
            return tags.Select(tag => tag.Key).ToList();
        }

        /// <summary>
        /// Replaces the given tag to the given new tag on every content in reppository.
        /// Also used for deleting tags, by calling with newtag = String.Empty parameter.
        /// </summary>
        /// <param name="tag">Tag to replace.</param>
        /// <param name="newtag">Tag to replace with.</param>
        /// <param name="pathList">The path list.</param>
        public static void ReplaceTag(string tag, string newtag, List<string> pathList)
        {
            tag = tag.ToLower();
            newtag = newtag.ToLower();
            if (tag == string.Empty)
                return;

            var query = $"+Tags:\"{tag}\"";

            if (pathList != null && pathList.Count > 0)
            {
                query = string.Concat(query,
                    $" +InTree:({pathList.Aggregate(string.Empty, (current, path) => string.Concat(current, "\"", path, "\"", " ")).TrimEnd(' ')})");
            }

            foreach (var tmp in ContentQuery.Query(query).Nodes)
            {
                if (tmp["Tags"] != null)
                {
                    tmp["Tags"] = (tmp["Tags"].ToString().ToLower().Replace(tag, newtag)).Trim();
                }
                tmp.Save();
            }
        }

        /// <summary>
        /// Determines if the given tag is blacklisted or not.
        /// </summary>
        /// <param name="tag">Tag to search on blacklist.</param>
        /// <param name="blacklistPaths">The blacklist paths.</param>
        /// <returns>True if blacklisted, false if not.</returns>
        public static bool IsBlacklisted(string tag, List<string> blacklistPaths)
        {
            // direct Lucene access is not supported anymore from a general component
            return false;

            //LucQuery query;

            //// escaping
            //if (!string.IsNullOrEmpty(tag))
            //    tag = tag.Replace("\"", "\\\"");

            //using (var readerFrame = LuceneManager.GetIndexReaderFrame())
            //{
            //    var reader = readerFrame.IndexReader;
            //    var terms = reader.Terms(new Term("BlackListPath", "*"));
            //    string queryString;

            //    if (blacklistPaths != null && blacklistPaths.Count > 0)
            //    {
            //        var usedPaths = new List<string>();

            //        do
            //        {
            //            if (terms.Term().Field() == "BlackListPath")
            //            {
            //                if (blacklistPaths.Any(path => path.ToLower().StartsWith(terms.Term().Text())))
            //                {
            //                    usedPaths.Add(terms.Term().Text());
            //                }
            //            }
            //        } while (terms.Next());

            //        var pathString = usedPaths.Aggregate(string.Empty, (current, usedPath) => string.Concat(current, "\"", usedPath, "\"", " "));

            //        pathString = pathString.TrimEnd(' ');

            //        queryString = $"+Type:tag +DisplayName:\"{tag.ToLower()}\"";
            //        queryString = string.Concat(queryString, string.IsNullOrEmpty(pathString) ? " +BlackListPath:/root" : $" +BlackListPath:({pathString})");
            //    }
            //    else
            //    {
            //        queryString = string.Concat("+Type:tag +DisplayName:\"", tag.ToLower(), "\" +BlackListPath:/root");
            //    }

            //    // tags are stored in the /Root/System folder
            //    queryString += " .AUTOFILTERS:OFF";

            //    try
            //    {
            //        query = LucQuery.Parse(queryString);
            //    }
            //    catch (ParserException)
            //    {
            //        // wrong query: return no
            //        SnLog.WriteWarning("TagAdmin query parse error: " + queryString);

            //        return false;
            //    }

            //    var result = query.Execute();
            //    return (result.Count() > 0);
            //}
        }

        /// <summary>
        /// Manages the blacklist.
        /// </summary>
        /// <param name="add">if set to <c>true</c> [add to blacklist] else remove from it.</param>
        /// <param name="tagId">The tag id.</param>
        /// <param name="pathList">The blacklist path list.</param>
        public static void ManageBlacklist(bool add, int tagId, List<string> pathList)
        {
            var tagNode = Node.LoadNode(tagId);
            var nodePaths = tagNode["BlackListPath"] as string;
            if (nodePaths == null) nodePaths = "";

            var blacklistPaths = nodePaths.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            foreach (var path in pathList)
            {
                if (add && !blacklistPaths.Contains(path))
                {
                    blacklistPaths.Add(path);
                }
                else if (blacklistPaths.Contains(path))
                {
                    blacklistPaths.Remove(path);
                }
            }

            tagNode["BlackListPath"] = blacklistPaths.Aggregate(string.Empty, (current, blacklistPath) => string.Concat(current, blacklistPath, ",")).TrimEnd(',');
            tagNode.Save();
        }

        /// <summary>
        /// Determines if the tag given as parameter is presented in content repository or not.
        /// </summary>
        /// <param name="tag">Tag to search.</param>
        /// <param name="tagPath">The tag path.</param>
        /// <returns>True is present, false if not.</returns>
        public static bool IsInRepository(string tag, string tagPath)
        {
            return Content.All.DisableAutofilters().Any(c => c.Type("Tag") && c.Name == GetTagName(tag) && c.InTree(tagPath));
        }

        /// <summary>
        /// Stores the given tag into the content repository as Tag node type.
        /// </summary>
        /// <param name="tag">Tag to store.</param>
        /// <param name="path">The folder path in repository, where you want to save the stored Tag node.</param>
        public static void AddToRepository(string tag, string path)
        {
            tag = tag.ToLower();
            var parentNode = Node.LoadNode(path);
            var subFolderName = GetTagName(tag);
            subFolderName = subFolderName.Length > 1 ? subFolderName.Substring(0, 2).Trim() : subFolderName;

            var fullPath = RepositoryPath.Combine(path, subFolderName);

            if (!Node.Exists(fullPath))
            {
                var cnt = Content.CreateNew("Folder", parentNode, subFolderName);
                cnt.Save();
                parentNode = cnt.ContentHandler;
            }
            else
            {
                parentNode = Node.LoadNode(fullPath);
            }
            var tagfileName = GetTagName(tag);
            var newTag = Content.CreateNew("Tag", parentNode, UrlNameValidator.ValidUrlName(tagfileName));
            newTag["DisplayName"] = tag.ToLower();
            newTag["TrashDisabled"] = true;

            newTag.Save();
        }

        private static string GetTagName(string tag)
        {
            return ContentNamingProvider.GetNameFromDisplayName("", tag).Trim().Replace('.', '_').Replace('"', '_').Replace('\'', '_');
        }

        /// <summary>
        /// Implements syncing between Lucene Index and Content Repository.
        /// - Cleans up not referenced tag Nodes (not used on any content)
        /// - Imports new tags which are not stored as Tag node in repository.
        /// - Optimizes Lucene Index for accurate search results.
        /// </summary>
        /// <param name="tagPath">Repository path where to import new tags.</param>
        /// <param name="searchPaths">The search paths.</param>
        public static void SynchronizeTags(string tagPath, List<string> searchPaths)
        {
            // 1. adding new tags to repositry
            var usedTags = GetAllTags(null, searchPaths);

            if (tagPath != string.Empty)
            {
                foreach (var tag in usedTags)
                {
                    if (!IsInRepository(tag, tagPath.ToLower()))
                    {
                        AddToRepository(tag.ToLower(), tagPath);
                    }
                }
            }
            // 2. deleting unused tag nodes
            foreach (var node in ContentQuery.Query("+Type:tag", QuerySettings.AdminSettings).Nodes)
            {
                if (!usedTags.Contains(node.DisplayName) && string.IsNullOrWhiteSpace(node.GetProperty<string>("BlackListPath")))
                {
                    node.Delete();
                }
            }
        }

        /// <summary>
        /// Gets the tag classes.
        /// </summary>
        /// <returns>List of classes</returns>
        public static List<KeyValuePair<string, int>> GetTagClasses()
        {
            return GetTagClasses(null, null);
        }

        /// <summary>
        /// Works like GetAllTags, but gives tag class (1-10) to tags, based on overall occurrencies.
        /// Used for TagCloud.
        /// </summary>
        /// <returns>List of tag classes.</returns>
        public static List<KeyValuePair<string, int>> GetTagClasses(string[] searchPaths, string[] contentTypes)
        {
            return GetTagClasses(searchPaths, contentTypes, 0);
        }

        /// <summary>
        /// Gets the tag classes.
        /// </summary>
        /// <param name="searchPaths">The search paths.</param>
        /// <param name="contentTypes">The content types.</param>
        /// <param name="top">Count of requested tags.</param>
        /// <returns>List of classes</returns>
        public static List<KeyValuePair<string, int>> GetTagClasses(string[] searchPaths, string[] contentTypes, int top)
        {
            Dictionary<string, int> occ;
            if (searchPaths != null && contentTypes != null)
            {
                occ = GetTagOccurrencies(searchPaths, contentTypes);
            }
            else
            {
                occ = GetTagOccurrencies();
            }

            var maxCount = 0;

            foreach (var o in occ)
            {
                if (o.Value > maxCount)
                {
                    maxCount = o.Value;
                }
            }

            var rnd = new Random();

            var classes = (from oc in occ
                           let tepmValue = (oc.Value / (float)(maxCount)) * 10
                           let value = (int)(tepmValue)
                           orderby rnd.Next()
                           select new KeyValuePair<string, int>(oc.Key, Math.Max(value, 1))).ToList();

            // If all tags are requested for tagcloud.
            if (top <= 0)
            {
                return classes;
            }

            // If a specified number of tags (top) are requsted for tagcloud.
            classes.Clear();

            var sortedOcc = (from entry in occ orderby entry.Value descending select entry).Take(top);

            classes.AddRange(from keyValuePair in sortedOcc
                             let tepmValue = (keyValuePair.Value / (float)(maxCount)) * 10
                             let value = (int)(tepmValue)
                             orderby rnd.Next()
                             select new KeyValuePair<string, int>(keyValuePair.Key, Math.Max(value, 1)));

            return classes;
        }

        /// <summary>
        /// Gets the tag classes.
        /// </summary>
        /// <param name="top">Count of requested tags.</param>
        /// <returns>List of classes</returns>
        public static List<KeyValuePair<string, int>> GetTagClasses(int top)
        {
            return GetTagClasses(null, null, top);
        }

    }
}

