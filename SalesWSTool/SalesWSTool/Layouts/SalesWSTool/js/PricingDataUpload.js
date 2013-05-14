var PricingUploader;
var PricingUploaderInsert;
var waitDialog;

$(document).ready(function () {
    //ButtonNext();
    CreateList();
    PricingDataUploadEXCEL();
});

function CreateList() {
    $("#ProductsSelectable").selectable({
        stop: function () {
            var result = $("#Pricing-Upload-Selected-Products").empty();
            $(".ui-selected", this).each(function () {
                var index = $("#Pricing-Upload-Selected-Products").index(this);
                result.append("<li data-id='" + $(this).attr('data-id') + "'>" + $(this).text() + "</li>");
            });
        }
    });
}

function ButtonNext() {
    $("#Pricing-Upload-Button-Upload")
    .button()
    .click(function (event) {
        alert("Next");
        //event.preventDefault();
    });
}

function PricingDataUploadEXCEL() {
    var button = $('#Pricing-Upload-Button-Upload'), interval;
    PricingUploader = $.ajax_upload(button, {
        action: '/_layouts/SalesWSTool/Services/PricingUpload.ashx',
        name: 'PricingDataFileInput',
        onSubmit: function (file, ext) {
            $("#Pricing-Upload-Result").empty();
            if (ext != "xlsx") {
                $("#Pricing-Upload-Result").append("<li>Only .xlsx files</li>");
                e.preventDefault();
            }
            var ProductsIDLi = $('#Pricing-Upload-Selected-Products').find('li');
            var ProductsID = "";
            ProductsIDLi.each(function () {
                if (ProductsID == "") {
                    ProductsID = $(this).attr("data-id");
                }
                else {
                    ProductsID += "," + $(this).attr("data-id");
                }
            });

            if (ProductsID == "") {
                $("#Pricing-Upload-Result").append("<li>You need to select at least one product</li>");
                e.preventDefault();
            }

            waitDialog = SP.UI.ModalDialog.showWaitScreenWithNoClose('Validating...', 'Please wait while we valitade the file...', 76, 330);

            this.set_data({ productsid: ProductsID, pricingaction: 'validate' });
            button.text('Validating...');
            this.disable();
            //$("#Pricing-Upload-Loader").css("display", "block");
            interval = window.setInterval(function () {
                var text = button.text();
                if (button.text().length < 13) {
                    button.text(button.text() + '.');
                } else {
                    button.text('Validating...');
                }
            }, 100);
        },
        onComplete: function (file, response) {
            waitDialog.close();
            var pricingresult = jQuery.parseJSON(response);

            //PricingUploaderInsert.submit();
            button.text("Upload");
            button.removeClass('hover');
            window.clearInterval(interval);
            this.enable();

            if (pricingresult.IsSuccess) {
                $('#Pricing-Upload-Validate').empty();
                $('#Pricing-Upload-Validate').append('<li>Upload ' + pricingresult.NumberOfRows + ' rows of data from tab "' + pricingresult.ExcelTabName + '" in excel "' + pricingresult.ExcelFileName + '"</li>');
                $('#Pricing-Upload-Validate').append('<li>Do you wish to continue?</li>');                
                varMyDiv = $('#Pricing-Upload-Dialog').clone();
                var options = {
                    html: varMyDiv[0],
                    width: 650,
                    height: 300,
                    showClose: false,
                    title: "Upload file?",
                    dialogReturnValueCallback: function (dialogResult, retValue) {
                        if (dialogResult == 1) {                            
                            PricingUploaderInsert.submit();
                        }
                    }
                };
                SP.UI.ModalDialog.showModalDialog(options);

                //PricingUploaderInsert.submit();
            }
            else {
                pricingresult.MessageErrors.forEach(function (lio) {
                    $("#Pricing-Upload-Result").append(lio);
                });
            }
            //$("#Pricing-Upload-Loade").css("display", "none");            
        }
    });
    //To Insert
    var buttoninsert = $('#Pricing-Upload-Button-Upload-Insert'), interval;
    PricingUploaderInsert = $.ajax_upload(buttoninsert, {
        action: '/_layouts/SalesWSTool/Services/PricingUpload.ashx',
        name: 'PricingDataFileInputInsert',
        onSubmit: function (file, ext) {
            $("#Pricing-Upload-Result").empty();
            waitDialog = SP.UI.ModalDialog.showWaitScreenWithNoClose('Updating...', 'Please wait while we update the information ...', 76, 330);
            var ProductsIDLi = $('#Pricing-Upload-Selected-Products').find('li');
            var ProductsID = "";
            ProductsIDLi.each(function () {
                if (ProductsID == "") {
                    ProductsID = $(this).attr("data-id");
                }
                else {
                    ProductsID += "," + $(this).attr("data-id");
                }
            });
            this.set_data({ productsid: ProductsID, pricingaction: 'insert' });
        },
        onComplete: function (file, response) {
            waitDialog.close();
            ResetSelect();
            var pricingresult = jQuery.parseJSON(response);
            if (pricingresult.IsSuccess) {
                $('#Pricing-Upload-Success-Message').empty();
                $('#Pricing-Upload-Success-Message').append('<li>' + pricingresult.NumberOfRows + ' rows were uploaded from the source.</li>');
                $('#Pricing-Upload-Success-Message').append('<li>' + pricingresult.NumberofRemoved + ' rows were removed from the pricing data.</li>');
                $('#Pricing-Upload-Success-Message').append('<li>' + pricingresult.NumberofRowsPricingData + ' rows were added to the pricing data.</li>');
                var htmlSucc = $('#Pricing-Upload-Success').clone();
                var options = {
                    html: htmlSucc[0],
                    width: 650,
                    height: 300,
                    showClose: false,
                    title: "Upload Successful!",
                    //dialogReturnValueCallback: function (dialogResult, retValue) {
                    //    if (dialogResult == 1) {
                    //        PricingUploaderInsert.submit();
                    //    }
                    //}
                };
                SP.UI.ModalDialog.showModalDialog(options);
            }
            else {
                pricingresult.MessageErrors.forEach(function (lio) {
                    $("#Pricing-Upload-Result").append(lio);
                });
            }
        }
    });
}

function ResetSelect() {
    $('#ProductsSelectable .ui-selected').removeClass('ui-selected');
    $('#Pricing-Upload-Selected-Products').empty();    
}