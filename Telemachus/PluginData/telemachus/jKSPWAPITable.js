google.load('visualization', '1', {packages:['table']});

function initKSPWAPITable(APIArray, divName){
	
	var data = new google.visualization.DataTable();
    data.addColumn('string', 'Value Name');
    data.addColumn('number', 'Value');

	var f = new KSPWAPIFormatters();

	buildRows();

    var table = new google.visualization.Table(document.getElementById('table_div'));

    rawData = [[0]];
	initKSPWAPIPoll(buildAPIString(), function(rawData){}, function(rawData, d){drawTable(d);}, rawData);

	function buildAPIString(){
		APIString = "";
		for (var i=0;i<APIArray.length;i++)
		{ 
			APIString = APIString + "a" + i + "="  + APIArray[i][0];

			if(i<APIArray.length-1){
				APIString =  APIString + "&";
			}
		}

		return APIString;
	}

	function buildRows(){
		for (var i=0;i<APIArray.length;i++)
		{ 
			data.addRow([APIArray[i][1], {v: 0, f: ''}]);
		}
	}

	function drawTable(d) {
      
		for (var i=0;i<APIArray.length;i++)
		{ 
			data.setCell(i, 1, d["a" + i], f[APIArray[i][2]](d["a" + i]));
		}

		table.draw(data);
    } 
}