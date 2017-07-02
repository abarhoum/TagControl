# Tags Field
Tag field is using <a target='blank' href="https://github.com/aehlke/tag-it">tag-it</a> jquery plugin , where it shows suggested tags from repository in sitecore as auto-complete, or you add new tags not available in the repository and it will be added to the repository when you save the item.
<h2> Sitecore versions :</h2>
XP 8.1, XP 8.2 I did not test that on erlier versions but it should works.

<h2> Installation :</h2>

Download the Sitecore Package : <b>TagsField20170702-1.0.zip</b>
Create new field from type "Tags", and in datasource provide the Folder Id for tags repository


<h2> Configurations : </h2>

All Configurations will be in include/TagField/TagField.config

<h3> TagField.TemplateId </h3>

Define the template Id for the tag Item

<setting name="TagField.TemplateId" value="{5AC7DEB1-15A5-46E1-B2E7-FC9C8DADEBFD}" />

<h3> Title Field </h3>

Define the title field for tag item
<setting name="TagField.TitleField" value="Title" />
      
<h3> HtmlTemplatePath </h3>
by default the field html and assets will be installed in this location, no need to change this unless you need to move it to different folder
<setting name="TagField.HTMLTemplatePath" value="sitecore\\shell\\Controls\\tag field\\template.html" />

<h2> Uninstall : </h2>

1) Remove TagControl.dll from bin folder.
2) Remove \App_Config\Include\TagField folder.
