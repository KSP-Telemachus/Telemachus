var UPDATE_INTERVAL = 300;

function kspWebAPI()
{
	//public
	var api = {};
	var updateCallbacks = {};
	var repeatRequestList = {};
	var current = {};
	var apiStringDirty = false;
	var apiString = "";
	var dataStore = new google.visualization.DataTable();
	
	this.api = api;
	this.updateCallbacks = updateCallbacks;
	this.repeatRequestList = repeatRequestList;
	this.current  = current;
	this.apiStringDirty = apiStringDirty;
	this.apiString = apiString;
	this.dataStore = dataStore;


	this.blockingApiRequest = blockingApiRequest;
	this.addToRepeatRequestList = addToRepeatRequestList;
	this.addToUpdateCallBack = addToUpdateCallBack;
	this.beginStream = beginStream;
	this.getViewIndicies = getViewIndicies;

	try{
		var req = null;
		req = blockingApiRequest("api=a.apiList");
		if (req.status == 200) {
			toEval = req.responseText.substring(9, req.responseText.length-2)
			eval("this.api=" + toEval + ";");
			t = setTimeout(beginStream, UPDATE_INTERVAL);
			return true;
		}
		else {
			document.writeln(req.responseText);
			return false;
		}
	} catch (e) { alert("Failed to initialise the API" + e.message);}

	function blockingApiRequest(apiString){

		var req = null;
		try { req = new XMLHttpRequest(); } catch (e) { }
		if (!req) try { req = new ActiveXObject("Msxml2.XMLHTTP"); } catch (e) { }
		if (!req) try { req = new ActiveXObject("Microsoft.XMLHTTP"); } catch (e) { }
		req.open('GET', "datalink?" + apiString, false);
		req.send();

		return req;
	}

	function getViewIndicies(d){
		console.log("getViewIndicies");
		var indicies = [];

		for(var i = 0; i<d.length ;i++){
			var index = labelToIndex(d[i]);
			if(index != -1){
				indicies.push(index);
			}
		}

		return indicies;
	}

	function labelToIndex(d){
		console.log(d);
		for(var i = 0;i < kwa.dataStore.getNumberOfColumns();i++){
			console.log(kwa.dataStore.getColumnLabel(i));
			if(kwa.dataStore.getColumnLabel(i) == d){
				console.log(i);
				return i;
				
			}
		}

		return -1;
	}

	function addToRepeatRequestList(s){
		
		if(s != "None"){
			var oldSize = countFields(kwa.repeatRequestList);
			kwa.repeatRequestList[s] = "";
			var newSize = countFields(kwa.repeatRequestList);
			
			if(oldSize != newSize){
				kwa.apiStringDirty = true;
				kwa.dataStore.addColumn('number', s);
			}
		}
	}

	function addToUpdateCallBack(n, c){
		kwa.updateCallbacks[n] = c; 
	}

	function updateApiString(){
		
		kwa.apiString= "";
		for(var col in kwa.repeatRequestList){
			kwa.apiString = kwa.apiString + "kwa.current['" + col +"']=" + kwa.api[col] + "&" ;
		}

		if(kwa.apiString.length > 0){
			kwa.apiString = kwa.apiString + "p=p.paused";
		}

		kwa.apiStringDirty = false;
	}

	function beginStream(){
		var req = null;
		if(kwa.apiStringDirty){
			updateApiString();
		}

		if(kwa.apiString.length > 0){
		req = blockingApiRequest(kwa.apiString);
		}
		else {
			t = setTimeout('kwa.beginStream()', UPDATE_INTERVAL);
			return true;
		}
	
		if (req.status == 200) {
			eval(req.responseText);
			
			if(!p){
				var currentArray = [];
				for (var v in kwa.current) {
					currentArray.push(kwa.current[v]);
				}

				kwa.dataStore.addRow(currentArray);

				for(var col in kwa.updateCallbacks){
					kwa.updateCallbacks[col](kwa.dataStore);
				}
			}

			t = setTimeout('kwa.beginStream()', UPDATE_INTERVAL);
			return true;
		}
		else {
			document.writeln(req.responseText);
			return false;
		}
	}
}