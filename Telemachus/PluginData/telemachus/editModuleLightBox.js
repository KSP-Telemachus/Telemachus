var editModuleLightBox = new lightBox(openLightBox, closeLightBox);

function graphCloseLightBox(){
	createNewChart($('#graph-title').val(),
		[$('#x-value option:selected').val(), 
			$('#y-value-0 option:selected').text(),$('#y-value-1 option:selected').text(),
			$('#y-value-2 option:selected').text()], 
		$('#light').attr("value"), "", "");
	editModuleLightBox.closeLightBox();
}

function flightControlCloseLightBox(){
	createNewFlightControl($('#' + $('#light').attr("value")));
	editModuleLightBox.closeLightBox();
}

function closeLightBox(){

}

function openLightBox(){

	
}