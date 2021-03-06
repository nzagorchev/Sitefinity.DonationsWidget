﻿
Type.registerNamespace("SitefinityEcommerceDonations");

SitefinityEcommerceDonations.DonationsWidget = function (element) {
    SitefinityEcommerceDonations.DonationsWidget.initializeBase(this, [element]);

    this._onLoadDelegate = null;
    this._donationAmountDropDown = null;
    this._donationAmountDropDownChangedDelegate = null;
    this._otherAmount = null;
}

SitefinityEcommerceDonations.DonationsWidget.prototype =
{
    initialize: function () {
        SitefinityEcommerceDonations.DonationsWidget.callBaseMethod(this, "initialize");

        if (this._onLoadDelegate === null) {
            this._onLoadDelegate = Function.createDelegate(this, this._onLoad);
        }


        this._donationAmountDropDownChangedDelegate = Function.createDelegate(this, this._donationAmountDropDownChangedHandler);
        this.get_donationAmountDropDown().add_valueChanged(this._donationAmountDropDownChangedDelegate);

        Sys.Application.add_load(this._onLoadDelegate);
    },

    dispose: function () {
        SitefinityEcommerceDonations.DonationsWidget.callBaseMethod(this, "dispose");

        Sys.Application.remove_load(this._onLoadDelegate);
        if (this._onLoadDelegate) {
            delete this._onLoadDelegate;
        }
    },

    /* -------------------- public methods ------------ */

    /* -------------------- events -------------------- */

    /* -------------------- event handlers ------------ */

    _onLoad: function (sender, args) {
        $('#otherAmountDiv').hide();
    },

    _donationAmountDropDownChangedHandler: function (sender, args) {
        var amount = sender.get_value();
        if (isNaN(amount)) {
            $('#otherAmountDiv').show();
        }
        else {
            $('#otherAmountDiv').hide();
        }

    },


    /* -------------------- private methods ----------- */

    /* -------------------- properties ---------------- */

    get_donationAmountDropDown: function () {
        return this._donationAmountDropDown;
    },

    set_donationAmountDropDown: function (value) {
        this._donationAmountDropDown = value;
    },

    get_otherAmount: function () {
        return this._otherAmount;
    },

    set_otherAmount: function (value) {
        this._otherAmount = value;
    }
};

SitefinityEcommerceDonations.DonationsWidget.registerClass("SitefinityEcommerceDonations.DonationsWidget", Sys.UI.Control);
