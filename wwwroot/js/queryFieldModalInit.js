// Query Field Modal Initialization Script
// This script initializes the query field selection and default settings modals

(function() {
    if (window.__queryFieldModalInit) return;
    window.__queryFieldModalInit = true;

    const fetcher = (input, init, msg) => {
        if (typeof window.http === "function") return window.http(input, init);
        if (typeof window.busyFetch === "function") return window.busyFetch(input, init, msg);
        return fetch(input, init);
    };

    document.addEventListener("DOMContentLoaded", function () {
        const qfModalEl = document.getElementById("queryFieldSelectModal");
        const qfDefaultModalEl = document.getElementById("queryDefaultModal");
        const qfAllEl = document.getElementById("qfAllFields");
        const qfSelectedEl = document.getElementById("qfSelectedFields");
        const qfDefaultBody = document.getElementById("qfDefaultBody");
        const qfDefaultTypeEl = document.getElementById("qfDefaultType");
        const qfDefaultValueEl = document.getElementById("qfDefaultValue");
        const qfDefaultCommandEl = document.getElementById("qfDefaultCommandText");
        const qfDefaultSuperIdEl = document.getElementById("qfDefaultSuperId");
        const qfDefaultControlEl = document.getElementById("qfDefaultControlType");
        const btnQfSelect = document.getElementById("btnQueryFieldSelect");
        const btnQfDefault = document.getElementById("btnQueryDefault");
        const btnQfSave = document.getElementById("qfSave");
        const btnQfDefaultSave = document.getElementById("qfDefaultSave");

        // Dynamically find the search modal
        const searchModalEl = document.getElementById("searchModal") 
            || document.getElementById("singleGridQueryModal")
            || document.getElementById("multiGridQueryModal")
            || document.querySelector('[id*="Query"][id*="Modal"]');

        let qfCache = null;
        let qfEquals = null;
        let reopenSearchOnClose = false;
        let pendingOpen = null;
        let currentDefaultRow = null;
        let defaultItems = [];

        const defaultTypeOptions = [
            { value: 0, text: "固定常數" },
            { value: 1, text: "SQL指令" },
            { value: 2, text: "登入帳號" },
            { value: 3, text: "公司代號" }
        ];

        if (!btnQfSelect || !btnQfDefault) return;

        console.log('Query Field Modal Init: modals found', {
            qfModalEl: !!qfModalEl,
            qfDefaultModalEl: !!qfDefaultModalEl,
            searchModalEl: !!searchModalEl
        });
