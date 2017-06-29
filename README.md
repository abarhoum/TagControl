# Tags Field
Tag field is using  tag-it jquery plugin https://github.com/aehlke/tag-it, where it shows suggested tags from repository in sitecore as auto-complete, if you add new tags not available  or you can add new tag and when you save the item, it will save the new tag in tag repostiry.

# Installation :

Download the Sitecore Package : TagField20170630-1.0.zip
Create new field from type "Tags", and in datasource provide the Folder Id for tags repository


# Configurations :

All Configurations will be in include/TagField/TagField.config

# TagField.TemplateId

Define the template Id for the tag Item :

<setting name="TagField.TemplateId" value="{5AC7DEB1-15A5-46E1-B2E7-FC9C8DADEBFD}" />

# Title Field

Define the title field for tag item
<setting name="TagField.TitleField" value="Title" />
      
# HtmlTemplatePath
by default the field html and assets will be installed in this location, no need to change this unless you need to move it to different folder
<setting name="TagField.HTMLTemplatePath" value="sitecore\\shell\\Controls\\tag field\\template.html" />

