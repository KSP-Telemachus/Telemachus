google.load("visualization", "1", { packages: ["corechart"] });

function initKSPWAPIGraph(APIString, postUpdate, rawData, options, divName) {
    chart = new google.visualization.LineChart(document.getElementById(divName));
    var postUpdateComposition = function (rawData, d) { postUpdate(rawData, d); appendCurrentValueToLegend(rawData); };
    jKSPWAPI.initPoll(APIString, function (rawData) { drawChart(rawData); }, postUpdateComposition, rawData)

    function drawChart(rawData) {
        try {
            
            if (rawData.length > 1) {
                chart.draw(google.visualization.arrayToDataTable(rawData), $.extend(options, { backgroundColor: { fill: "#AAAAAA" } }));
            }
        }
        catch (e) {
            //Sensor graph reset when the number of sensors changes.
            rawData.length = 1;
        }
    }

    function appendCurrentValueToLegend(rawData) {
        try{
            if (rawData.length > 1) {
                for (var i = 1; i < rawData[0].length; i++) {
                    var last = rawData[0][i].lastIndexOf("(");

                    if (last != -1) {
                        rawData[0][i] = rawData[0][i].slice(0, last);
                    }

                    rawData[0][i] = rawData[0][i] +
                    " (" + jKSPWAPI.formatters.sigFigs(rawData[rawData.length - 1][i], jKSPWAPI.SIG_FIG) + ")";
                }
            }
        }
        catch(e){
        }
    }
}

