namespace Sitecore.Support.Links
{
    using Sitecore;
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.Links;
    using Sitecore.Sites;
    using Sitecore.Text;
    using Sitecore.Web;
    using System;
    using System.Collections.Generic;
    public class LinkProvider : Sitecore.Links.LinkProvider
    {
        protected new LinkBuilder CreateLinkBuilder(Sitecore.Links.UrlOptions options) =>
            new LinkBuilder(options);

        public override string GetItemUrl(Item item, UrlOptions options)
        {
            Assert.ArgumentNotNull(item, "item");
            Assert.ArgumentNotNull(options, "options");
            string itemUrl = this.CreateLinkBuilder(options).GetItemUrl(item);
            if (options.LowercaseUrls)
            {
                itemUrl = itemUrl.ToLowerInvariant();
            }
            return itemUrl;
        }

        public new class LinkBuilder : Sitecore.Links.LinkProvider.LinkBuilder
        {
            public LinkBuilder(Sitecore.Links.UrlOptions options) : base(options)
            {
            }

            protected override string GetServerUrlElement(SiteInfo siteInfo)
            {
                SiteContext site = Context.Site;
                string value = (site != null) ? site.Name : string.Empty;
                string hostName = this.GetHostName();
                string result = this.AlwaysIncludeServerUrl ? WebUtil.GetServerUrl() : string.Empty;
                if (siteInfo == null)
                {
                    return result;
                }
                string text = ((!string.IsNullOrEmpty(siteInfo.HostName) && !string.IsNullOrEmpty(hostName)) && DoesHostNameMatchSiteInfo(hostName, siteInfo))/*siteInfo.Matches(hostName))Sitecore.Support.177155*/ ? hostName : StringUtil.GetString(new string[] { this.GetTargetHostName(siteInfo), hostName });
                string @string = StringUtil.GetString(new string[]
                {
            siteInfo.Scheme,
            this.GetScheme()
                });
                int num = MainUtil.GetInt(siteInfo.Port, WebUtil.GetPort());
                int port = WebUtil.GetPort();
                int @int = MainUtil.GetInt(siteInfo.ExternalPort, num);
                if (@int != num)
                {
                    if (this.AlwaysIncludeServerUrl)
                    {
                        result = ((@int == 80) ? string.Format("{0}://{1}", @string, this.GetHostName()) : string.Format("{0}://{1}:{2}", @string, this.GetHostName(), @int));
                    }
                    num = @int;
                }
                if (!this.AlwaysIncludeServerUrl && siteInfo.Name.Equals(value, StringComparison.OrdinalIgnoreCase) && hostName.Equals(text, StringComparison.OrdinalIgnoreCase))
                {
                    return result;
                }
                if (string.IsNullOrEmpty(text) || text.IndexOf('*') >= 0)
                {
                    return result;
                }
                string scheme = this.GetScheme();
                StringComparison comparisonType = StringComparison.OrdinalIgnoreCase;
                if (text.Equals(hostName, comparisonType) && num == port && @string.Equals(scheme, comparisonType))
                {
                    return result;
                }
                string text2 = @string + "://" + text;
                if (num > 0 && num != 80)
                {
                    text2 = text2 + ":" + num;
                }
                return text2;
            }
            internal virtual string GetScheme()
            {
                return WebUtil.GetScheme();
            }

            internal virtual string GetHostName()
            {
                return WebUtil.GetHostName();
            }
            
            //substitutes siteInfo.Matches(hostName)) sitecore.support.177155
            private bool DoesHostNameMatchSiteInfo(string host, SiteInfo siteInfo)
            {
                Assert.ArgumentNotNull(host, "host");
                if ((host.Length == 0) || (siteInfo.HostName.Length == 0))
                {
                    return true;
                }
                host = host.ToLowerInvariant();
                foreach (string[] strArray in GetHostNamePatterns(siteInfo.HostName))
                {
                    if (WildCarserParserMatches(host, strArray))
                    {
                        return true;
                    }
                }
                return false;
            }

            //substitutes Sitecore.Text.WildCardParser.Matches(host, strArray) sitecore.support.177155 
            public static bool WildCarserParserMatches(string value, string[] matchParts)
            {
                return WildCarserParserMatches(value, matchParts, false);
            }

            //substitutes Sitecore.Text.WildCardParser.Matches sitecore.support.177155
            public static bool WildCarserParserMatches(string value, string[] matchParts, bool disableTrailingWildcard)
            {
                Assert.ArgumentNotNull(value, "value");
                Assert.ArgumentNotNull(matchParts, "matchParts");
                if (value.Length > 0 && matchParts.Length != 0)
                {
                    bool flag = false;
                    for (int i = 0; i < matchParts.Length; i++)
                    {
                        string text = matchParts[i];
                        if (text.Length > 0)
                        {
                            if (text[0] == '*')
                            {
                                flag = true;
                            }
                            else
                            {
                                int num = value.IndexOf(text, StringComparison.InvariantCulture);
                                //sitecore.support.177155
                                int reverseIndex = text.IndexOf(value, StringComparison.InvariantCulture);
                                if ((num < 0) || (reverseIndex < 0) || ((num > 0) && !flag))
                                //if (num < 0 || (num > 0 && !flag))
                                //end of sitecore.support.177155
                                {
                                    return false;
                                }
                                value = value.Substring(num + text.Length);
                            }
                        }
                    }
                }
                return string.IsNullOrEmpty(value) || !disableTrailingWildcard;
            }


            //substitutes this.hostNamePatterns from SiteInfo class sitecore.support.177155
            private List<string[]> GetHostNamePatterns(string hostName)
            {
                var hostNamePatterns = new List<string[]>();
                foreach (string str in hostName.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    hostNamePatterns.Add(WildCardParser.GetParts(str));
                }
                return hostNamePatterns;
            }
        }
    }
}