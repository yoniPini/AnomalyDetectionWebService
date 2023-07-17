//tb_ prefix to avoid collision with modle_list
// use headerName for title
const tb_columnDefs = [
    //{ headerName:"", flex: 1, checkboxSelection: true },
    { field: "feature", sortable: true, filter: 'agTextColumnFilter', floatingFilter: true, checkboxSelection: true, resizable: true},
    { field: "highest", sortable: true },
    { field: "lowest", sortable: true },
    { field: "average", sortable: true },
    { field: "is anomaly", sortable: true },
    { field: "reason", resizable: true}
];

// specify the data [example]
var tb_rowData = [
];

// let the grid know which columns and what data to use
const tb_gridOptions = {
    columnDefs: tb_columnDefs,
    rowData: tb_rowData,
    rowSelection: 'multiple',
    rowMultiSelectWithClick: false,
    suppressRowDeselection: true,
    suppressRowClickSelection: true,
    onRowSelected: tb_onRowSelected,
    pagination: true,
    paginationAutoPageSize: true
};

// setup the grid after the page has finished loading
document.addEventListener('DOMContentLoaded', () => {
    const tb_gridDiv = document.querySelector('#myGrid_table');
    new agGrid.Grid(tb_gridDiv, tb_gridOptions);
});

// NOT called if all the data changed by gridOptions.api.setRowData(x)
function tb_onRowSelected(event) {
    if (event.node.isSelected()) {
        add_attr(event.node.data.feature);
    }
    else {
        remove_attr(event.node.data.feature);
    }
}
//event.node.data.feature  speed/roll ...
//event.node.isSelected()  true/false



// change all data by
// var tb_x = [{ "feature": "speed", "some information": "WOW"}, ...]
// tb_gridOptions.api.setRowData(tb_x);
function update_base_stats(values_dictionary) {
    var tb_x = [];
    for (let key in values_dictionary) {
        var sum = 0;
        var high = values_dictionary[key][0];
        var low = values_dictionary[key][0];
        var length = values_dictionary[key].length;
        for (let i = 0; i < length; i++) {
            if (high < values_dictionary[key][i])
                high = values_dictionary[key][i];
            if (low > values_dictionary[key][i])
                low = values_dictionary[key][i];
            sum += +values_dictionary[key][i];
        }
        var avg = sum / values_dictionary[key].length;
        tb_x.push({
            "feature": key, "highest": high, "lowest": low, "average": avg,
            "is anomaly": "--", "reason": "--"
        })
    }
    return tb_x;
}


function update_data(values_dictionary) {
    var tb_x = update_base_stats(values_dictionary);
    tb_gridOptions.api.setRowData(tb_x);
}


function update_anomalies(values_dictionary, anomalies_dictionary) {
    var rows = update_base_stats(values_dictionary);
    for (let row in rows) {
        if (rows[row].feature in anomalies_dictionary) {
            rows[row]["is anomaly"] = "YES";
            rows[row].reason = anomalies_dictionary[rows[row].feature];
        }
        else {
            rows[row]["is anomaly"] = "NO";
        }
    }
    tb_gridOptions.api.setRowData(rows);
}