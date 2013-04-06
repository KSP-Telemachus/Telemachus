function lightBox(open, close){
	
	var userClose = close;
	var userOpen = open
	this.openLightBox = openLightBox;
	this.closeLightBox = closeLightBox;

	function closeLightBox(){
		userClose();

		$("#light").css("display", "none");
		$("#fade").css("display", "none");
	}

	function openLightBox(id){
		$("#light").css("display", "block");
		$("#fade").css("display", "block");

		$("#light").attr("value", function (arr) {
		return id;
		})

		userOpen();
	}
}