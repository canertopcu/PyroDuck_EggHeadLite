using System;
using com.pyroduck.eggheadslite.Editor.Promotion;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace com.pyroduck.eggheadslite.Tests.Editor
{
    public class CampaignTests
    {
        private static readonly DateTimeOffset Now = new DateTimeOffset(2026, 9, 6, 12, 0, 0, TimeSpan.Zero);
        private static Campaign Card(string id, int priority = 0)
        {
            return new Campaign { id = id, title = id, active = true, enabled = true,
                priority = priority, targetUrl = "https://assetstore.unity.com/publishers/137662" };
        }

        [Test]
        public void Catalog_FiltersDatesFlagsAndSortsByPriority()
        {
            var low = Card("low");
            var high = Card("high", 10);
            var expired = Card("expired"); expired.endUtc = "2026-09-06T12:00:00Z";
            var future = Card("future"); future.startUtc = "2026-09-07T00:00:00Z";
            var disabled = Card("disabled"); disabled.enabled = false;
            var inactive = Card("inactive"); inactive.active = false;
            var malformed = Card("malformed"); malformed.endUtc = "tomorrow";
            high.startUtc = "2026-09-06T12:00:00Z";
            var catalog = new CampaignCatalog { enabled = true, campaigns = new[] { low, expired, future, disabled, inactive, malformed, high } };
            var active = catalog.GetActive(Now);
            Assert.AreEqual(2, active.Count);
            Assert.AreEqual("high", active[0].id);
            Assert.AreEqual("low", active[1].id);
        }

        [Test]
        public void Catalog_KillSwitchAndEmptyFeedAreValid()
        {
            Assert.IsTrue(CampaignCatalog.TryParse("{\"schemaVersion\":1,\"enabled\":false,\"campaigns\":[]}", out var catalog));
            Assert.IsEmpty(catalog.GetActive(Now));
            Assert.IsFalse(CampaignCatalog.TryParse("{\"schemaVersion\":2,\"campaigns\":[]}", out _));
            Assert.IsFalse(CampaignCatalog.TryParse("<html>error</html>", out _));
        }

        [TestCase("javascript:alert(1)")]
        [TestCase("file:///C:/secret")]
        [TestCase("http://pyroduck.com")]
        [TestCase("https://assetstore.unity.com.evil.example/asset")]
        [TestCase("https://user:password@pyroduck.com")]
        public void Destination_RejectsUnsafeOrUnrelatedLinks(string url)
        {
            Assert.IsFalse(CampaignCatalog.IsDestination(url));
        }

        [Test]
        public void Catalog_DeduplicatesAndRejectsInvalidDateRanges()
        {
            var broken = Card("broken"); broken.startUtc = "2026-09-07T00:00:00Z"; broken.endUtc = "2026-09-06T00:00:00Z";
            var catalog = new CampaignCatalog { enabled = true, campaigns = new[] { Card("same"), Card("same"), broken } };
            Assert.AreEqual(1, catalog.GetActive(Now).Count);
        }

        [Test]
        public void Service_UnconfiguredFeedUsesBundledCatalogAndDisposesTwice()
        {
            var fallback = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/PyroDuck/EggHeadsLite/Editor/PyroDuck/AboutUs/campaigns.json");
            var service = new CampaignService("", fallback.text);
            try
            {
                service.Refresh(true);
                Assert.AreEqual("egg-heads-2d", service.Catalog.GetActive(Now)[0].id);
                service.Refresh(false);
                Assert.AreEqual(1, service.Catalog.GetActive(Now).Count);
            }
            finally { service.Dispose(); service.Dispose(); }
        }

        [Test]
        public void AboutWindow_LoadsUxmlStylesAndFallbackCard()
        {
            var window = ScriptableObject.CreateInstance<PyroDuckAboutWindow>();
            try
            {
                window.CreateGUI();
                var root = window.rootVisualElement;
                Assert.IsNotNull(root.Q<Button>("asset-store-button"));
                Assert.AreEqual("Egg Heads 2D", root.Q<Label>("campaign-title").text);
                Assert.AreEqual(DisplayStyle.Flex, root.Q("campaign-card").style.display.value);
                Assert.Greater(root.styleSheets.count, 0);
                // Recreating the UI must dispose the previous service and avoid duplicate trees.
                window.CreateGUI();
                Assert.AreEqual(1, root.Query<Button>("view-asset-button").ToList().Count);
            }
            finally { UnityEngine.Object.DestroyImmediate(window); }
        }
    }
}
