google.load('visualization', '1', {packages:['table']});

function initKSPWAPITable(APIArray, divName){
	
	KSPWAPIGetAPISubset(APIArray,function(APIInformation){
		var data = new google.visualization.DataTable();
		data.addColumn('string', '');
		data.addColumn('number', '');

		var f = new KSPWAPIFormatters();

		buildRows(APIInformation.api);

		var table = new google.visualization.Table(document.getElementById('table_div'));

		initKSPWAPIPoll(buildAPIString(), function(rawData){}, function(rawData, d){drawTable(d);}, [[0]]);

		function buildAPIString(){
			APIString = "";
			for (var i=0;i<APIArray.length;i++)
			{ 
				APIString = APIString + "a" + i + "="  + APIArray[i];

				if(i<APIArray.length-1){
					APIString =  APIString + "&";
				}
			}

			return APIString;
		}

		function buildRows(api){
			for (var i=0;i<APIArray.length;i++)
			{ 
				data.addRow([api[i].name, {v: 0, f: ''}]);
			}
		}

		function drawTable(d) {
      
			for (var i=0;i<APIArray.length;i++)
			{ 
				data.setCell(i, 1, d["a" + i], f[APIInformation.api[i].units.toLowerCase()](d["a" + i]));
			}

			table.draw(data);
		} 
	});
}