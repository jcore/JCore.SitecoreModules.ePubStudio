using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Buckets.FieldTypes;
using Sitecore.Buckets.Util;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.ContentSearch.Utilities;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;

namespace Sitecore.SharedModules.ePub.Publishing.Renderers
{
    public class RepeaterRenderer : EpubItemRendererBase
    {
        // <summary>
        /// Gets or sets the data sources.
        /// </summary>
        /// <value>
        /// The data sources.
        /// </value>
        public Item[] DataSources { get; set; }
        /// <summary>
        /// Gets or sets the repeat count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public string Count { get; set; }

        /// <summary>
        /// Preliminary render action invoked before RenderContent <see cref="RenderContent"/>.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        protected override void BeginRender(PrintContext printContext)
        {
            if (!string.IsNullOrEmpty(this.RenderingItem["Item Reference"]))
            {
                this.DataSource = this.RenderingItem["Item Reference"];
            }

            // Get the data item assigned to the repeater
            var dataItem = this.GetDataItem(printContext);
            if (dataItem != null)
            {
                // apply the selector to the data item
                if (!string.IsNullOrEmpty(this.RenderingItem["Item Selector"]))
                {
                    var xpath = this.RenderingItem["Item Selector"];
                    if (!string.IsNullOrEmpty(xpath))
                    {
                        var items = dataItem.Axes.SelectItems(xpath);
                        if (items != null)
                        {
                            this.DataSources = items;
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(this.RenderingItem["Item Field"]))
                {
                    // Get the number of times we need to repeat the child elements
                    this.Count = this.DataSources != null ? this.DataSources.Count().ToString() : "0";
                }
            }
            else if (!string.IsNullOrEmpty(this.RenderingItem["Search Query"]))
            {
                var query = this.RenderingItem["Search Query"];
                this.DataSources = RunSearchQuery(query);
            }
            else if (!string.IsNullOrEmpty(this.RenderingItem["Items"]))
            {
                this.DataSources = ((MultilistField)this.RenderingItem.Fields["Items"]).GetItems();
            }
        }

        /// <summary>
        /// Renders the content.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        /// <param name="output">The output.</param>
        protected override void RenderContent(PrintContext printContext, object output, Epub.NavPoint parentEndPoint)
        {
            //if (!string.IsNullOrEmpty(this.ChildDataKeyName))
            //{
            //    printContext.Settings.Parameters[this.ChildDataKeyName] = this.DataSource;
            //}

            if (this.DataSources != null)
            {
                foreach (var dataSource in this.DataSources)
                {
                    this.DataSource = dataSource.ID.ToString();
                    RenderChildren(printContext, output, parentEndPoint);
                }
                return;
            }

            var dataItem = this.GetDataItem(printContext);
            if (dataItem == null)
            {
                return;
            }

            int count;
            if (!string.IsNullOrEmpty(this.Count) && int.TryParse(this.Count, out count) && count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    // Render child elements
                    this.RenderChildren(printContext, output, parentEndPoint);
                }

                return;
            }

            this.RenderChildren(printContext, output, parentEndPoint);
        }

        /// <summary>
        /// Runs the search query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        private static Item[] RunSearchQuery(string query)
        {
            var maxCount = 10;
            
            IEnumerable<SearchStringModel> stringModel = UIFilterHelpers.ParseQueryString(query);
            using (IProviderSearchContext context = Index().CreateSearchContext())
            {
                var searchResults = LinqHelper.CreateQuery(context, stringModel).Take(maxCount).ToList();
                if (searchResults.Any())
                {
                    return ConvertToItems(searchResults).ToArray();
                }
            }
            return null;
        }

        /// <summary>
        /// Indexes this instance.
        /// </summary>
        /// <returns></returns>
        private static ISearchIndex Index()
        {
            if (Sitecore.Context.Item != null)
            {
                var indexable = new SitecoreIndexableItem(Sitecore.Context.Item);
                return ContentSearchManager.GetIndex(indexable);
            }
            else
            {
                if (!Sitecore.Context.PageMode.IsNormal)
                {
                    return ContentSearchManager.GetIndex("sitecore_master_index");
                }
                else
                {
                    return ContentSearchManager.GetIndex("sitecore_web_index");
                }
            }
        }

        /// <summary>
        /// Converts the automatic items.
        /// </summary>
        /// <param name="searchResults">The search results.</param>
        /// <returns></returns>
        private static IEnumerable<Item> ConvertToItems(List<SitecoreUISearchResultItem> searchResults)
        {
            foreach (var res in searchResults)
            {
                yield return res.GetItem();
            }
        }
    }
}
