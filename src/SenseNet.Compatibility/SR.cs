using System;
using SenseNet.ContentRepository.i18n;

namespace SenseNet.Compatibility
{
    // ReSharper disable once InconsistentNaming
    internal class SR
    {
        internal class Exceptions
        {
            internal class ContentView
            {
                public static string NotFound = "$Error_Portlets:ContentView_NotFound";
            }
            internal class Survey
            {
                public static string OnlySingleResponse = "$Error_ContentRepository:Survey_OnlySingleResponse_1";
            }
        }
        internal class Portlets
        {
            internal class ContextSearch
            {
                public static string SearchUnder = "$ContextSearch:SearchUnder_";
            }
        }
        internal class Wall
        {
            public static string OnePerson = "$Wall:OnePerson";
            public static string NPeople = "$Wall:NPeople";
            public static string YouLikeThis = "$Wall:YouLikeThis";
            public static string YouAndAnotherLikesThis = "$Wall:YouAndAnotherLikesThis";
            public static string YouAndOthersLikesThis = "$Wall:YouAndOthersLikesThis";
            public static string OnePersonLikesThis = "$Wall:OnePersonLikesThis";
            public static string MorePersonLikeThis = "$Wall:MorePersonLikeThis";
            public static string CommentedOnAContent = "$Wall:CommentedOnAContent";
            public static string LikesAContent = "$Wall:LikesAContent";
            public static string PleaseLogIn = "$Wall:PleaseLogIn";

            public static string What_Created = "$Wall:What_Created";
            public static string What_Modified = "$Wall:What_Modified";
            public static string What_Deleted = "$Wall:What_Deleted";
            public static string What_Moved = "$Wall:What_Moved";
            public static string What_Copied = "$Wall:What_Copied";
            public static string What_To = "$Wall:What_To";

            public static string Source = "$Wall:Source";
            public static string Target = "$Wall:Target";
        }

        public static string GetString(string fullResourceKey)
        {
            return SenseNetResourceManager.Current.GetString(fullResourceKey);
        }

        public static string GetString(string fullResourceKey, params object[] args)
        {
            return String.Format(GetString(fullResourceKey), args);
        }
    }
}
