

var data = {
    labels: [],
    datasets: []
};

var gr_config = {
    type: 'line',
    data: data,
    options: {
        responsive: true,
        plugins: {
            legend: {
                position: 'top',
            },
            title: {
                display: true,
                text: 'מאפיינים'
            }
        }
    }
};

var ctx = document.getElementById('myChart');

var myChart = new Chart(ctx, gr_config);
//var dicForAmonaly = {}

/**
 * in order to determine the length of x axis we measure the length of the array
 * of the first (i.e. the the data.labels is clear) character that is added to the graph.
 * the representation of each x value is done by strTime function.
 * @param {any} num - the length of the array.
 */
function addLables(num) {
    for (var i = 0; i < num; i++)
        myChart.data.labels.push(strTime(i));
}

/**
 * a function that get a number and returns a string of  its last 2 digits.
 * @param {any} x - number
 */
function twoDigitsRepr(x) {
    if (x == 0) return "00";
    if (x >= 10) return "" + x;
    return "0" + x;
}

/**
 * when given a time step we would like to represent it in time. for example, the 1000 time step
 * is actually 1 minute and 40 seconds. hence 01:40.
 * @param {any} ts - a time step
 */
function strTime(ts) {
    let originalTs = ts;
    //a:b:c.d
    let d = ts % 10;
    ts -= d; ts = ts / 10;
    let c = ts % 60;
    ts -= c; ts = ts / 60;
    let b = ts % 60;
    ts -= b; ts = ts / 60;
    let a = ts;

    c = twoDigitsRepr(c);
    b = twoDigitsRepr(b);
    a = twoDigitsRepr(a);
    return a + ":" + b + ":" + c + "." + d + "  [" + originalTs + "]";
}

/**
 * adding a character which has no anomalies and update the graph to re-render it.
 * @param {any} name - the character's name for the label.
 * @param {any} array - character's data.
 */
function add_attribute(name, array) {
    if (myChart.data.labels.length == 0) 
        addLables(array.length);
    
    adding_without_update(name, array)
    myChart.update()
}

/**
 * when given a chacter's name find the index of it in the data.
 * @param {any} name - the character name
 */
function findIndexAttribute(name) {
    for (var i in myChart.data.datasets)
        if (myChart.data.datasets[i].label === name)
            return i;
    return undefined
}

/**
 * when given a chacter's name remove it from the data.
 * @param {any} name - the character name
 */
function remove_attribute(name) {
    let x = findIndexAttribute(name)
    if ( x != undefined) {
        myChart.data.datasets.splice(x, 1)
        myChart.update()
    }
}

/**
 * adding a character which has no anomalies without updating the graph.
 * @param {any} name - the character's name for the label.
 * @param {any} array - character's data.
 */
function adding_without_update(name, array) {
    let ranR = Math.floor(Math.random() * 200) + 1   // ranR can be any number from 1-201
    let ranB = Math.floor(Math.random() * 200) + 51 // ranB can be any number from 51-251
    let ranG = Math.floor(Math.random() * 200) + 51 // ranG can be any number from 51-251
    myChart.data.datasets.push({
        label: name,
        data: array,
        fill: false,
        pointRadius: 0 ,
        borderColor: 'rgb(' + ranR + ',' + ranG + ',' + ranB + ')'
        //tension: 0.1
    })
}

/**
 * check if a given point 1's x value and point 2's x value  is in span.
 * if so return true. else false.
 * @param {any} p - x value of point 1
 * @param {any} p1 - x value of point 2
 * @param {any} span - the span
 */
function inSpan(p, p1, span) {
    let b = false
    if (p >= span[0] && p < span[1] && p1 >= span[0] && p1 < span[1]) {

        b = true
    }
    return b;
}

/**
 * checck if a given point's x value is in edge of span.
 * @param {any} p - x value of point
 * @param {any} span - the span
 */
function inSpanPoint(p, span) {
    let b = false
    if (p == span[0] || p == span[1] - 1) {
        b = true
    }
    return b;
}

/**
 * checks if the current point'x value we eant to paint is in some anomaly span.
 * if so we paint it RED. else with the color of the rest of the line.
 * @param {any} ctx - the canvas element to paint on.
 * @param {any} spanList - the list of spans
 * @param {any} v - the color with which to paint the line if it's in of span.
 */
const lineAnomaly = (ctx, spanList, v) => {
    for (var i in spanList)
        if (inSpan(ctx.p0.parsed.x, ctx.p1.parsed.x, spanList[i]))
            return v;
    return undefined;
};

/**
 * checks if the current point'x value  we eant to paint is in the edge of some anomaly span.
 * if so we mark it with cross. else with none.
 * @param {any} ctx - the canvas element to paint on.
 * @param {any} spanList - the list of spans
 * @param {any} v - the mark with which to paint the dot if it's in edge of span.
 * @param {any} v2 - the default mark if the condition is false.
 */
const pointAnomaly = (ctx, spanList, v, v2) => {
    for (var i in spanList)
        if (inSpanPoint(ctx.dataIndex, spanList[i]))
            return v;
    return v2;
};

/**
 * add a new character to the graph without upadting it.
 * @param {any} name - the name of the character.
 * @param {any} array - the character's data.
 * @param {any} span - the character's anomalies.
 */
function adding_anomaly_without_update(name, array, span) {
    let ranR = Math.floor(Math.random() * 200) + 1   // ranR can be any number from 1-201
    let ranB = Math.floor(Math.random() * 200) + 51 // ranB can be any number from 51-251
    let ranG = Math.floor(Math.random() * 200) + 51 // ranG can be any number from 51-251
    let str = 'rgb(' + ranR + ',' + ranG + ',' + ranB + ')'
    myChart.data.datasets.push({
        label: name,
        data: array,
        fill: false,
        borderColor: str,
        pointRadius: ctx => pointAnomaly(ctx, span, 5, 0),
        //pointStyle: 'circle',
        pointStyle: ctx => pointAnomaly(ctx, span, 'crossRot', undefined),
        //pointBackgroundColor: '',
        pointBorderWidth: 3,
        pointBorderColor: ctx => pointAnomaly(ctx, span, 'red', str),
        segment: {
            borderColor: ctx => lineAnomaly(ctx, span, 'rgb(255,0,0)'),
        }
    })
}

/**
 * add a new character to the graph and upadte it. paints the anomaly segment in RED.
 * the anomaly segment is also marked with cross in its egdes.
 * @param {any} name - the name of the character.
 * @param {any} array - the character's data.
 * @param {any} span - the character's anomalies.
 */
function add_anomaly_attribute(name, array, span) {
    if (myChart.data.labels.length == 0)
        addLables(array.length);

    adding_anomaly_without_update(name, array, span)
    myChart.update();
}

/**
 * clean the graph from all datasets (lines) and labels
 */
function cleanGraph() {
    myChart.data.datasets = [];
    myChart.data.labels = [];
    myChart.update();
}