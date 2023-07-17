//ml_ prefix to avoid collision with table
const ml_columnDefs = [
    { headerName: "id", field: "model_id", sortable: true },
    { headerName: "status", field: "status", sortable: true }, 
    { headerName: "upload", field: "upload_time", sortable: true, resizable: true}
];

// specify the data [example]. ml_rowData set1 set2 are all for debuging.
const ml_rowData = [
    { "model_id": 123, "status": "ready", "upload_time" : "2021-04-22T19:15:32+02.00" },
    { "model_id": 124, "status": "ready", "upload_time": "2021-04-22T19:15:32+02.00"},
    { "model_id": 125, "status": "ready", "upload_time": "2021-04-22T19:15:32+02.00"},
    { "model_id": 1267, "status": "pending", "upload_time": "2021-04-22T19:15:32+02.00"},
    { "model_id": 333, "status": "pending", "upload_time": "2021-04-22T19:15:32+02.00"},
    { "model_id": 456, "status": "pending", "upload_time": "2021-04-22T19:15:32+02.00"}
];

const set1 = [
    { "model_id": 123, "status": "ready", "upload_time": "2021-04-22T19:15:32+02.00" },
    { "model_id": 124, "status": "ready", "upload_time": "2021-04-22T19:15:32+02.00" },
    
];

const set2= [
    { "model_id": 331, "status": "ready", "upload_time": "2021-04-22T19:15:32+02.00" },
    { "model_id": 124, "status": "ready", "upload_time": "2021-04-22T19:15:32+02.00" }
];

let blink_rate = 0.0;

// let the grid know which columns and what data to use and setting the other options.
const ml_gridOptions = {
    columnDefs: ml_columnDefs,
    rowData: [],
    rowSelection: 'single',
    rowMultiSelectWithClick: false,
    suppressRowDeselection: true,
    suppressRowClickSelection: false,
    onRowSelected: ml_onRowSelected,
    pagination: false,
    paginationAutoPageSize: true
};

// setup the grid after the page has finished loading
document.addEventListener('DOMContentLoaded', () => {
    const ml_gridDiv = document.querySelector('#myGrid_model_list');
    new agGrid.Grid(ml_gridDiv, ml_gridOptions);
    refresh_list_from_server();
});

// NOT called if all the data changed by    gridOptions.api.setRowData(x)
/**
 * change the content of the 'remove button' when a row is clicked and set it to enable.
 * @param {any} event - ignored
 */
function ml_onRowSelected(event) {
    var rmButton = document.getElementById("remove_button")
    if (getSelectedModelID() != undefined) {
        rmButton.value = "remove " + getSelectedModelID()
        rmButton.disabled = false
    }
    else 
        rmButton.disabled = true
}



/**
 * set the blink_rate to 1.5. further details in 'setInterval'.
 */
function blink() { blink_rate = 1.5; }

/**
 * when list is updated an 'updated' label is shown and fade away. meaning its opacity goes from
 * 1 till 0. when there's an update we set the blink_rate using blink() in refresh().
 * therefore the setInterval() will decrease its opacity every 0.2 seconds.
 */
setInterval(() => {
    if (blink_rate < 0) blink_rate = 0;
    document.getElementById("blink_txt").style.opacity = Math.min(1, blink_rate);
    blink_rate -= 0.06;
}, 200);


/**
 * get a whole new list and switch if with the current list.
 * @param {any} list - the new list
 */
function refresh(list) {
    var selected_model_id = getSelectedModelID()
    ml_gridOptions.api.setRowData(list)
    selectAfterRefresh(selected_model_id)
    blink();
}

/*
setInterval(() => {
    if (ml_gridOptions.api.getSelectedRows().length > 0)
        console.log(ml_gridOptions.api.getSelectedRows()[0].model_id)
}, 5000)
*/


/**
 * when updating the list we need to check if a row was selected before. if so
 * we keep it selected after the update.
 * @param {any} model_id - the model that was selected.
 */
function selectAfterRefresh(model_id) {
    document.getElementById("remove_button").disabled = true
    ml_gridOptions.api.forEachNode(function (node) {
        node.setSelected(node.data.model_id === model_id);
    });
}

/**
 * remove the selected row from the list.
 * @param {any} event - ignored
 */
function remove_selected(event) {
    var id = getSelectedModelID();
    if (id != undefined) {
        send_to_server("/api/model?model_id=" + id, "DELETE", "");
        refresh_list_from_server();
    }
}

/**
 * if some row is selected returns its model_if field. else returns undefined.
 */
function getSelectedModelID() {
    if (ml_gridOptions.api.getSelectedRows()[0] != undefined)
        return ml_gridOptions.api.getSelectedRows()[0].model_id
    return undefined
}
