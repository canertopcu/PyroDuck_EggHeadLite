using UnityEngine;

namespace com.pyroduck.eggheadslite.Editor.Promotion
{
    public sealed class PyroDuckCampaignSettings : ScriptableObject
    {
        [Tooltip("Public HTTPS URL of campaigns.json. Empty keeps the bundled card offline.")]
        public string feedUrl = "";
        [Tooltip("Bundled content used until a valid remote catalog is available.")]
        public TextAsset fallbackCatalog;
    }
}
