google.load('visualization', '1', {packages:['table']});

function initKSPWAPITable(APIArray, divName){
	
	jKSPWAPI.getAPISubset(APIArray,function(APIInformation){
		var data = new google.visualization.DataTable();
		data.addColumn('string', '');
		data.addColumn('number', '');

		buildRows(APIInformation.api);

		var table = new google.visualization.Table(document.getElementById('table_div'));

		jKSPWAPI.initPoll(buildAPIString(), function(rawData){}, function(rawData, d){drawTable(d);}, [[0]]);

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
				if(APIInformation.api[i].units.toLowerCase() == "string"){
					data.setCell(i, 1, 0, d["a" + i]); 
				}
				else{
				data.setCell(i, 1, d["a" + i], 
					jKSPWAPI.formatters.pad(
					jKSPWAPI.formatters[APIInformation.api[i].units.toLowerCase()](d["a" + i])));
				}
			}

			table.draw(data,  {
			  allowHtml: true
			  });
		} 
	});
}