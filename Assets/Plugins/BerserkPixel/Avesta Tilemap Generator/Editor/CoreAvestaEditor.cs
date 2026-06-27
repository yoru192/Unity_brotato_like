using UnityEditor;
using UnityEngine;

namespace BerserkPixel.Tilemap_Generator
{
    public static class CoreAvestaEditor
    {
        [MenuItem("Window/Avesta/Leave a Review")]
        public static void OpenReviewsPage()
        {
            Application.OpenURL(BerserkURL.URL_REVIEWS);
        }
        
        [MenuItem("Window/Avesta/Learning Resources/Online Manual")]
        public static void ShowOnlineManual()
        {
            Application.OpenURL(BerserkURL.URL_AVESTA_DOCS);
        }
        
        [MenuItem("Window/Avesta/Learning Resources/Youtube")]
        public static void ShowYoutube()
        {
            Application.OpenURL(BerserkURL.URL_AVESTA_YOUTUBE);
        }
        
        [MenuItem("Window/Avesta/Explore/Berserk Pixel\'s Assets")]
        public static void OpenBerserkStorePage()
        {
            Application.OpenURL(BerserkURL.URL_STORE_PAGE);
        }

        [MenuItem("Window/Avesta/Contact/Support")]
        public static void ShowSupportEmailEditor()
        {
            BerserkURL.OpenEmailEditor(
                BerserkURL.URL_SUPPORT_EMAIL,
                "[Avesta] SHORT_QUESTION_HERE",
                "YOUR_QUESTION_IN_DETAIL");
        }
        
        [MenuItem("Window/Avesta/Contact/Business")]
        public static void ShowBusinessEmailEditor()
        {
            BerserkURL.OpenEmailEditor(
                BerserkURL.URL_BUSINESS_EMAIL,
                "[Avesta] SHORT_QUESTION_HERE",
                "YOUR_QUESTION_IN_DETAIL");
        }
    }
}