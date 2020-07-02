<%@ Control Language="C#" AutoEventWireup="true"  Codebehind="IconSelector.ascx.cs" Inherits="App.Boilerplate.IconSelector" %>
<%@ Register Src="~/CMSModules/Content/Controls/Attachments/DirectFileUploader/DirectFileUploader.ascx"
    TagName="DirectFileUploader" TagPrefix="cms" %>
<%@ Register Src="~/CMSAdminControls/UI/FontIconSelector.ascx" TagPrefix="cms" TagName="FontIconSelector" %>

<cms:CMSUpdatePanel runat="server" ID="pnlIcons" UpdateMode="Conditional">
    <ContentTemplate>
        <div class="doc-type-icon-uploader">
            <cms:FontIconSelector runat="server" ID="fontIconSelector" />
        </div>
    </ContentTemplate>
</cms:CMSUpdatePanel>
