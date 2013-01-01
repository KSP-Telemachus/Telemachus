var DATA_SIZE = 1000;
var UPDATE_INTERVAL = 300;
var SPLICE_SIZE = 10;
var SIG_FIG = 5;

function sigFigs(n, sig) {
    var mult = Math.pow(10,
        sig - Math.floor(Math.log(n) / Math.LN10) - 1);
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