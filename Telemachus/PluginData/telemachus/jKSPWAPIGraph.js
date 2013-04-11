google.load("visualization", "1", { packages: ["corechart"] });

function initKSPWAPIGraph(APIString, postUpdate, rawData, options){
		
	chart = new google.visualization.LineChart(document.getElementById('chart_div'));
	var postUpdateComposition = function(rawData, d){postUpdate(rawData, d); appendCurrentValueToLegend(rawData);};
	initKSPWAPIPoll(APIString, function(rawData){drawChart(rawData);}, postUpdateComposition, rawData)

	function drawChart(rawData) {
		chart.draw(google.visualization.arrayToDataTable(rawData), options);
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
}

