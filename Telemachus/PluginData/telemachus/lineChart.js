var DATA_SIZE = 1000;
var SPLICE_SIZE = 10;
var SIG_FIG = 5;

var lineCharts = {};

function sigFigs(n, sig) {
    var m = false;

    if (n < 0) {
        m = true;
        n = Math.abs(n);
    }

    var mult = Math.pow(10,
        sig - Math.floor(Math.log(n) / Math.LN10) - 1);

    if (m) {
        n = n * -1;
    }

    return Math.round(n * mult) / mult;
}

function appendCurrentValueToLegend(rawData) {

    for (var i = 1; i < rawData[0].length; i++) {

        var last = rawData[0][i].lastIndexOf("(");
        if (last != -1) {
            rawData[0][i] = rawData[0][i].slice(0, last);
        }

        rawData[0][i] = rawData[0][i] + " (" + sigFigs(rawData[rawData.length - 1][i], SIG_FIG) + ")";
    }
}

function createNewChart(itsTitle, fields, location, xlabel, ylabel){

    options = {
        title: itsTitle,
        vAxis: { title: xlabel },
        hAxis: { title: ylabel },
        legend: {position: 'bottom'}
    };

	 try{
		for(var i = 0;i< fields.length;i++){
			kwa.addToRepeatRequestList(fields[i]);
			
		}
	 } catch (e) { alert("Failed to update repeat list - " + e.message);}
	 
	 var chart = new google.visualization.LineChart(document.getElementById(location));
	 lineCharts[location] = new lineChart(chart, options, kwa.getViewIndicies(fields));

	 kwa.addToUpdateCallBack(location, function(data) { lineCharts[location].update.call(lineCharts[location], data);});
}

function lineChart(chart, options, viewIndicies)
{

this.viewIndicies = viewIndicies;
this.chart = chart;
this.options = options;

this.update = update;

function update(data){
	console.log("1");
	var view = new google.visualization.DataView(data);
	console.log("2");
	view.setColumns(this.viewIndicies);
	console.log(this.viewIndicies);
	this.chart.draw(view, this.options);
}
}