<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %> 
<%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PricingDataUploadUserControl.ascx.cs" Inherits="SalesWSTool.Webparts.PricingDataUpload.PricingDataUploadUserControl" %>

<div id="Pricing-Data" class="pricing-upload-main">
    <div class="pricing-upload-main-content">
        <div class="pricing-upload-main-content-products">
            <div class="pricing-upload-main-title">Products</div>
            <ol id="ProductsSelectable">
                <asp:Literal ID="OLProductData" runat="server"></asp:Literal>
            </ol>
        </div>
        <div class="pricing-upload-main-content-upload" style="display:none;">            
            <div class="pricing-upload-main-content-upload-controls">
                
                <div class="pricing-upload-main-content-upload-controls-button">
                    <div id="Pricing-Upload-Button-pas">Select File</div>
                </div>
                
                <div class="pricing-upload-main-content-upload-controls-result" id="Pricing-Upload-Result-DIV">
                    <ul id="Pricing-Upload-Result-UL"></ul>
                </div>
            </div>
        </div>
    </div>
    <div class="pricing-upload-main-controls">
        <div class="pricing-upload-main-controls-container">
            <div class="pricing-upload-main-controls-input">
                <input type="file" runat="server" accept="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" id='PricingFileInput' style="display:none;"/>
                <input type="file" runat="server" accept="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" id='PricingFileInputInsert' style="display:none;"/>
            </div>
            <div class="pricing-upload-main-controls-container-button">
                <div class="pricing-upload-main-title">Select a excel file</div>
                <a id="Pricing-Upload-Button-Upload" href="#">Upload File</a>
                <a id="Pricing-Upload-Button-Upload-Insert" href="#" style="display:none;">Upload Insert</a>
                <div class="pricing-upload-main-controls-loader" id="Pricing-Upload-Loader" style="display:none;"></div>
            </div>
            <div class="pricing-upload-main-controls-container-selected-products">
                <div class="pricing-upload-main-title">Selected Products</div>
                <ul id="Pricing-Upload-Selected-Products">
                </ul>
            </div>
            <div class="pricing-upload-main-controls-container-result-upload">
                <div class="pricing-upload-main-title">Errors</div>
                <ul id="Pricing-Upload-Result">
                </ul>
            </div>
            <div class="pricing-upload-main-controls-container-result-validate" style="display:none;">     
                <div id="Pricing-Upload-Dialog">
                    <ul id="Pricing-Upload-Validate">
                    </ul>
                    <input type="button" value="OK"onclick="SP.UI.ModalDialog.commonModalDialogClose(SP.UI.DialogResult.OK, 'Ok clicked'); return false;" class="ms-ButtonHeightWidth" />
                    <input type="button" value="Cancel"onclick="SP.UI.ModalDialog.commonModalDialogClose(SP.UI.DialogResult.cancel, 'Cancel clicked'); return false;" class="ms-ButtonHeightWidth" />
                </div>
            </div>
             <div class="pricing-upload-main-controls-container-result-sucess" style="display:none;">     
                <div id="Pricing-Upload-Success">
                    <ul id="Pricing-Upload-Success-Message">
                    </ul>
                    <input type="button" value="OK" onclick="SP.UI.ModalDialog.commonModalDialogClose(SP.UI.DialogResult.OK, 'Ok clicked'); return false;" class="ms-ButtonHeightWidth" />                    
                </div>
            </div>
        </div>
    </div>
</div>