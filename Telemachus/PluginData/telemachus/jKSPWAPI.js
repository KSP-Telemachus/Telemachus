var DATA_SIZE = 1000;
var UPDATE_INTERVAL = 300;
var SPLICE_SIZE = 10;
var SIG_FIG = 5;

google.load("visualization", "1", { packages: ["corechart"] });


function initKSPWAPIGraph(APIString, f, rawData, options){

	chart = new google.visualization.LineChart(document.getElementById('chart_div'));
    update(APIString, f, rawData, google.visualization.arrayToDataTable(rawData), options);
}

function drawChart(APIString, f, rawData, data, options) {
	chart.draw(data, options);
}

function update(APIString, f, rawData, data, options) {
	if (rawData.length > 1) {
		drawChart(APIString, f, rawData, data, options);
	}
	readStream(APIString, f, rawData, data, options)
}

function readStream(APIString, f, rawData, data, options) {
	var callback = function(APIString, f, rawData, data, options) { 
		return function(response,status){
		if (status == "success") {
			
				d = new Object();
				eval(response);

				if (!p) {
					f(rawData, d);
					appendCurrentValueToLegend(rawData);
					data = google.visualization.arrayToDataTable(rawData);
				}

				if (rawData.length > DATA_SIZE) {
				rawData.splice(1, SPLICE_SIZE);
				}

				t = setTimeout(function(){update(APIString, f, rawData, data, options);}, UPDATE_INTERVAL);
					
		}
		else {
			document.writeln(response);
		}
		};
	};

	$.get(APIString, callback(APIString, f, rawData, data, options)).error(function() {
		rawData.length = 1;
		t = setTimeout(function(){update(APIString, f, rawData, data, options);}, UPDATE_INTERVAL);
	});
}

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