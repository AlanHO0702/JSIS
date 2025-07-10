window.showDictModal = function(modalId = 'dictModal') {
    let modal = new bootstrap.Modal(document.getElementById(modalId));
    modal.show();
};

document.addEventListener('keydown', function(e){
    if(e.key === 'F3') {
        e.preventDefault();
        showDictModal(); // 預設dictModal
    }
});

function saveAllDictFields(tableSelector = '.dictTableBody', apiUrl = '/api/DictApi/UpdateDictFields') {
    // 1. 設定游標為 loading
    document.body.style.cursor = "wait";

    const rows = document.querySelectorAll(tableSelector + ' tr');
    const data = Array.from(rows).map(tr => {
        const serialNumValue = tr.querySelector('input[data-field="SerialNum"]').value;
        const visibleValue = tr.querySelector('input[data-field="Visible"]').checked ? 1 : 0;
        return {
            TableName: tr.getAttribute('data-tablename'),
            FieldName: tr.getAttribute('data-fieldname'),
            DisplayLabel: tr.querySelector('input[data-field="DisplayLabel"]').value,
            DataType: tr.querySelector('input[data-field="DataType"]').value,
            FieldNote: tr.querySelector('input[data-field="FieldNote"]').value,
            SerialNum: serialNumValue === "" ? null : parseInt(serialNumValue, 10),
            Visible: visibleValue
        };
    });

    fetch(apiUrl, {
        method: 'POST',
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(data)
    })
    .then(res => res.json())
    .then(result => {
        // 2. 還原游標
        document.body.style.cursor = "default";

        if (result.success) {
            alert("全部儲存成功！");
            location.reload();
        } else {
            alert(result.message || "儲存失敗！");
        }
    })
    .catch(err => {
        document.body.style.cursor = "default";
        alert("API失敗: " + err);
    });
}

