//var csv is the CSV file with headers
function csvJSON(csv) {

    csv = csv.replace("\r\n", "\n").replace("\r", "\n");
    var lines = csv.split("\n");
    var result = {};

    var tmp_headers = lines[0].split(",");
    //manage duplicate headers
    var counter = {};
    var current = {};
    for (var i = 0; i < tmp_headers.length; i++) {
        current[tmp_headers[i]] = 0;
        if (tmp_headers[i] in counter)
            counter[tmp_headers[i]]++;
        else
            counter[tmp_headers[i]] = 1;
    }
    var headers = [];
    for (var i = 0; i < tmp_headers.length; i++) {
        var suffix = "";
        if (counter[tmp_headers[i]] != 1) {
            suffix = "[" + current[tmp_headers[i]] + "]";
            current[tmp_headers[i]]++;
        }
        headers.push(tmp_headers[i] + suffix);
    }

    //initialize dictionary according to new headers
    for (var i = 0; i < headers.length; i++) {
        result[headers[i]] = [];
    }

    //add values to headers line by line
    for (var i = 1; i < lines.length; i++) {
        if (lines[i] == "")
            continue;
        var currentline = lines[i].split(",");
        for (var j = 0; j < headers.length; j++) {
            result[headers[j]].push(Number(currentline[j]));
        }
    }

    //return the dictionary
    return result;
}

//this part manage the reading from the input file
async function parseFile(file) {
    var data = await new Response(file).text();
    return await csvJSON(data);
}

//receive file from drop and pass it to the model as a dictionary
async function dropHandler(event) {
    allowDrop(event);
    var is_hybrid = document.getElementById("hybrid").checked == true;
    var is_anomaly = document.getElementById("anomaly").checked == true;
    var is_local_only = document.getElementById("show_local").checked == true;
    var drop = document.getElementById("The_File");
    if (is_local_only) {
        drop.innerHTML = event.dataTransfer.items[0].getAsFile().name + " loaded.<br/>" +
            "Data was not sent to server.";
    } else {
    drop.innerHTML = event.dataTransfer.items[0].getAsFile().name + " uploaded.<br/>" +
        (is_anomaly ? "anomaly data" : "create new model using<br/>") +
        (is_anomaly ? "" : (is_hybrid ? "hybrid " : "regression ") + "algorithem.");
    }
    var input_dictionary = await parseFile(event.dataTransfer.items[0].getAsFile());
    //dropzone should know the model in order to  activate model.new_drop
    var model_id = new_drop(input_dictionary, is_local_only, (is_anomaly ? true : false), (is_hybrid ? true : false));
    if (model_id != undefined) {
        drop.innerHTML += "<br/> new model is " + model_id;
    }
}

function disable_buttons(event) {
    document.getElementById("algo_detection").style.visibility = 'hidden';  
    //document.getElementById("hybrid").style.opacity = 0;
}


function enable_buttons(event) {
    document.getElementById("algo_detection").style.visibility = 'visible';
   // document.getElementById("hybrid").style.opacity = 1;
}


//Set those global vars off in order to calculate the current place of the dragged file.
var dragHtml = false;   //true= drag file is over the page. false- no dragged file.
var dragDropZone = false;   //same question specificly for the dropzone.
//Prevenr browser default for drag/drop (the default is to open the file in a new tab),
//in order to enable our actions to the drag/ drop events.
//In addition this function mark the edges of the dropzone when you drag a file
//according to the place that the cursor is on the screen.
function allowDrop(event) {
    event.preventDefault();
    var state;
    if (dragDropZone)
        state = 1;
    else
        state = dragHtml ? 2 : 0; 
    //0 - normal state (no file), 1 - file is on dropzone, 2 - file is on the rest of the page.

    var color = { 0: "black", 1: "green", 2: "red" };
    var style = { 0: "solid", 1: "solid", 2: "dashed" };
    var effect = { 0: "none", 1: "move", 2: "none" };

    event.dataTransfer.dropEffect = effect[state];
    document.getElementById("The_Drop").style.borderColor = color[state];
    document.getElementById("The_Drop").style.borderStyle = style[state];
}
