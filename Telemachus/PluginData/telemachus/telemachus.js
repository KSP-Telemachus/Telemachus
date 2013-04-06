var kwa;

google.load("visualization", "1", { packages: ["corechart"] });

$(document).ready(function() {
kwa = new kspWebAPI();
loadLayoutPage();
populateAxisOptions();

});

function populateAxisOptions(){

	$(".axis-select").append("<option>None</option>");
	
	try{
		for (var col in kwa.api) {
			$(".axis-select").append("<option value='" + col + "'>" + col + "</option>");
		}
	} catch (e) { alert("Failed to append API options.");}
}


function countFields(d){
	var count = 0;
	for (var col in d) {
		count++;
	}

	return count;
}

function loadLayoutPage(){
$("#content-div").html($("#page-layout-select").html());
$('#page-layout-content li').bind("mouseover", function(){
            var color  = $(this).css("background-color");

            $(this).css("background", "lightgray");

            $(this).bind("mouseout", function(){
                $(this).css("background", color);
            })    
        })
}

function loadGridPage(x, y){
	var dim = $("input[name='layout']:checked").val();

	var elements = x * y;
	width = 100 / x;
	height = 100 / y;

	$(".box").css("width", width + "%");
	$(".box").css("height", height + "%");

	$("#content-div").html("");

	for(var i = 0;i < elements;i++){

	$("#content-div").append($("#grid-layout").html());
	}
      
	$("#content-div").append("<div class=\"clear\"></div>");

	$(".box").on('contextmenu', function (e) {
		if(event.which == 3){
			try{editModuleLightBox.openLightBox($(this).attr('id'));} catch (e) { }
			return false;
		}
	});

	$('.box').bind("mouseover", function(){
				var color  = $(this).css("background-color");

				$(this).css("background", "lightgray");

				$(this).bind("mouseout", function(){
					$(this).css("background", color);
				})    
			}) 

	$(".box").attr("id", function (arr) {
	return "box-id" + arr;
	})
}