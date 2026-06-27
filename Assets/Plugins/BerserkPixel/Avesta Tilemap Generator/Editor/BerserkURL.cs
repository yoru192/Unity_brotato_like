using UnityEngine;

namespace BerserkPixel.Tilemap_Generator
{
    public static class BerserkURL
    {
        public static readonly string URL_REVIEWS = "https://u3d.as/2Kyh#reviews";
        
        public static readonly string URL_STORE_PAGE = "https://assetstore.unity.com/publishers/51713";
        
        public static readonly string URL_AVESTA_DOCS = "https://berserkpixel.studio/docs/avesta.html";
            
        public static readonly string URL_AVESTA_YOUTUBE = "https://www.youtube.com/watch?v=aqP1FLpSMUA&list=PL0HP3BNvjjHcRWiDWFDlSKGYg9YZ2ogZk";
        
        public static readonly string URL_SUPPORT_EMAIL = "support@berserkpixel.studio";
        
        public static readonly string URL_BUSINESS_EMAIL = "hello@berserkpixel.studio";
        
        public static void OpenEmailEditor(string receiver, string subject, string body)
        {
            string url = $"mailto:{receiver}" + $"?subject={subject.Replace(" ", "%20")}" +
                         $"&body={body.Replace(" ", "%20")}";

            Application.OpenURL(url);
        }
    }
}