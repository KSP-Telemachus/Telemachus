google.load("visualization", "1", { packages: ["corechart"] });

function initKSPWAPIGraph(APIString, postUpdate, rawData, options, divName){
	chart = new google.visualization.LineChart(document.getElementById(divName));
	var postUpdateComposition = function(rawData, d){postUpdate(rawData, d); appendCurrentValueToLegend(rawData);};
	initKSPWAPIPoll(APIString, function(rawData){drawChart(rawData);}, postUpdateComposition, rawData)

	function drawChart(rawData) {
		try{
			chart.draw(google.visualization.arrayToDataTable(rawData), options);
		}
		catch(e){rawData.length = 0;}
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

