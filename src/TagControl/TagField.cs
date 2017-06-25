﻿using Sitecore;
using Sitecore.Configuration;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq.Utilities;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.SecurityModel;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using TagControl.sitecore;

namespace TagControl
{
    public class TagField : Sitecore.Web.UI.HtmlControls.Control
    {
       
        #region Private Properties

        private string HTMLTemplatePath
        {
            get
            {
                return Settings.GetSetting("TagField.HTMLTemplatePath");
            }
        }

        private string Source
        {

            get { return StringUtil.GetString(ViewState["Source"]); }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                ViewState["Source"] = value;
            }
        }

        private string InputId
        {
            get { return GetID("input"); }
        }

        private string TitleField
        {
            get
            {
                return Settings.GetSetting("TagField.TitleField");
            }
        }

        public string ItemID { get; set; }

        private static ID TagTemplateId

        {
            get
            {
                Sitecore.Data.ID id = null;
                var templateId = Settings.GetSetting("TagField.TemplateId");
                if (!string.IsNullOrEmpty(templateId))
                {
                    Sitecore.Data.ID.TryParse(templateId, out id);
                    if (!Sitecore.Data.ID.IsNullOrEmpty(id))
                    {
                        return id;
                    }
                }
                return id;
            }
        }

        private ID TagFolderId
        {
            get
            {
                Sitecore.Data.ID id = null;
                var TagFolderId = Source;
                if (!string.IsNullOrEmpty(TagFolderId))
                {
                    Sitecore.Data.ID.TryParse(TagFolderId, out id);
                    if (!Sitecore.Data.ID.IsNullOrEmpty(id))
                    {
                        return id;
                    }
                }
                return id;
            }
        }

        #endregion

        #region Events

        protected override void OnLoad(EventArgs e)
        {
            var literalTags = new System.Web.UI.WebControls.Literal();
            if (!Sitecore.Context.ClientPage.IsEvent)
            {

                BuildTagsControl(literalTags);
                Controls.Add(literalTags);
            }
            else
            {
                /*This line to make sure to add new tag item only when the triggered event is save, this event it will be rasied by one of the following actions:
                 1) Click save from ribbon.
                 2) Shortcut cltr + save.
                 3) When you change the field and you go to other item without click save aciton, the dialog of "save itme changes" will appear 
                    and ask you if you want to save the changes, then you click "Yes"*/
                var eventType = Sitecore.Context.ClientPage.ClientRequest.Parameters;
                if (eventType.Equals("contenteditor:save") || eventType.Equals("item:save()"))
                {
                    var tagEntities = new List<TagEntity>();
                    var jsonSerialiser = new JavaScriptSerializer();
                    var tagList = jsonSerialiser.Deserialize<List<TagEntity>>(Context.Request.Form["hdnJsonObject"]);
                    var createItemTasks = new List<Task>();
                    foreach (var tag in tagList)
                    {
                        if (tag.id == "0")
                        {
                            /* I created another task to create sitecore item, the reason behind that is when you create an item here in this place sitecore will take
                            you to the newly created item in the tree which is this is the default behavior for sitecore, but for this control if new tag added not already
                            exists in the tag repositroy, then i want to create that tage in the reop and get the newly tag item id, and save it in raw value of the field.*/
                            var task = Task.Run(() => CreateItem(tag.label));
                            task.Wait(1);
                            var item = task.Result;
                            tagList.First(p => p.label.Equals(tag.label)).id = item.ID.ToString();
                        }
                    }
                    var value = jsonSerialiser.Serialize(tagList) ?? "";
                    Sitecore.Context.ClientPage.Modified = (Value != value);
                    if (value != null && value != Value)
                        Value = value;
                }
            }
            base.OnLoad(e);
        }

        #endregion

        #region Private Methods

        void BuildTagsControl(System.Web.UI.WebControls.Literal literalTags)
        {
            string tagEntitiesJson = string.Empty;
            var tagEntities = new List<TagEntity>();
            StringBuilder list = new StringBuilder();
            var appDomain = AppDomain.CurrentDomain;
            var basePath = appDomain.BaseDirectory;
            var tags = TagsSearch();
            var jsonSerialiser = new JavaScriptSerializer();
            var tagsJson = jsonSerialiser.Serialize(tags);
            var path = Path.Combine(basePath, HTMLTemplatePath);
            var html = System.IO.File.ReadAllText(path);

            html = html.Replace("($tags$)", tagsJson);

            if (!string.IsNullOrEmpty(Value))
            {

                var tagList = jsonSerialiser.Deserialize<List<TagEntity>>(Value);
                foreach (var tag in tagList)
                {
                    var id = tag.id;
                    if (id != "0")
                    {
                        ID parsedId;
                        Sitecore.Data.ID.TryParse(id, out parsedId);
                        if (!Sitecore.Data.ID.IsNullOrEmpty(parsedId))
                        {
                            var item = Client.ContentDatabase.GetItem(parsedId);
                            if (item != null)
                            {
                                list.Append(string.Format("<li data-id='{0}'>{1}</li>", item.ID.ToString(), item[TitleField]));
                                tagEntities.Add(new TagEntity { id = item.ID.ToString(), label = item[TitleField] });
                            }
                        }
                    }
                }

                tagEntitiesJson = jsonSerialiser.Serialize(tagEntities);
            }
            html = html.Replace("($avalilableTags$)", list.ToString());
            html = html.Replace("($jsonObject$)", string.IsNullOrEmpty(tagEntitiesJson) ? "[]" : HttpUtility.HtmlEncode(tagEntitiesJson));
            literalTags.Text = html;
        }

        private Item[] GetTags()
        {
            Item[] tags = Client.ContentDatabase.SelectItems(Source);
            return tags;
        }

        private List<TagEntity> TagsSearch()
        {
            var items = new List<TagEntity>();
            if (!(Sitecore.Data.ID.IsNullOrEmpty(TagFolderId) || Sitecore.Data.ID.IsNullOrEmpty(TagTemplateId)))
            {
                using (var context = ContentSearchManager.GetIndex("sitecore_master_index").CreateSearchContext())
                {
                    var predicate = PredicateBuilder.True<SearchResultItem>();
                    predicate = predicate.And(p => p.Paths.Contains(TagFolderId));
                    predicate = predicate.And(p => p.TemplateId == TagTemplateId);
                    predicate = predicate.And(p => p.Language == Sitecore.Context.Language.Name);
                    var result = context.GetQueryable<SearchResultItem>().Where(predicate);
                    if (result != null && result.Count() > 0)
                    {
                        var searchresultItems = result.ToList();
                        foreach (var searchResultItem in searchresultItems)
                        {
                            var currentItem = searchResultItem.GetItem();
                            items.Add(new TagEntity
                            {
                                id = currentItem.ID.ToString(),
                                label = currentItem[TitleField]
                            });
                        }

                    }
                }
            }
            return items;
        }

        public Item CreateItem(string itemName)
        {
            //First get the parent item from the master database
            Database contentDatabase = Factory.GetDatabase("master");
            Item parentItem = contentDatabase.GetItem(Source);
            //Now we need to get the template from which the item is created
            TemplateItem template = contentDatabase.GetTemplate(TagTemplateId);
            //Now we can add the new item as a child to the parent
            return parentItem.Add(itemName, template);
        }

        #endregion
    }

}