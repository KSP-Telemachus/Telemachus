var DATA_SIZE = 1000;
var UPDATE_INTERVAL = 300;
var IDLE_UPDATE_INTERVAL = 4000;
var SPLICE_SIZE = 10;
var SIG_FIG = 5;

var sNotify = {
	
	timeOpen: 5,	//change this number to the amount of second you want the message opened
	
	queue: new Array(),
	closeQueue: new Array(),
	
	addToQueue: function(msg) {
		sNotify.queue.push(msg);
	},
	
	createMessage: function(msg) {
		
		//create HTML + set CSS
		var messageBox = $("<div><span class=\"sNotify_close\">x</span>" + msg + "</div>").appendTo("body");
		$(messageBox).addClass("sNotify_message");
		
		sNotify.enableActions(messageBox);
		sNotify.closeQueue.push(0);
		
		return $(messageBox);
		
	},
	
	loopQueue: function() {
		//pop queue
		if (sNotify.queue.length > 0) {
			
			var messageBox = sNotify.createMessage(sNotify.queue[0]);
			sNotify.popMessage(messageBox);
			
			sNotify.queue.splice(0,1);
			
		}
		
		//close queue
		if (sNotify.closeQueue.length > 0) {
			var indexes = new Array();
			
			for (var i = 0; i < sNotify.closeQueue.length; i++) {
				sNotify.closeQueue[i]++;
				
				if (sNotify.closeQueue[i] > sNotify.timeOpen) {
					indexes.push(i);
				}
			}
			
			//now close them
			for (var i = 0; i < indexes.length; i++) {
				var buttons = $(".sNotify_close");
				sNotify.closeMessage($(buttons[($(buttons).length - indexes[i]) - 1]));
				sNotify.closeQueue.splice(indexes[i],1);	
			}
			
		}
		
	},
	
	enableActions: function(messageBox) {
		//reset timer when hovering
		$(messageBox).hover(
			function() {
				var index = ($(this).nextAll().length - 1);
				sNotify.closeQueue[index] = -1000;
			},
			function() {
				var index = ($(this).nextAll().length - 1);
				sNotify.closeQueue[index] = 0;
			}
		);
		
		//enable click close button
		$(messageBox).find(".sNotify_close").click(function() {
			sNotify.closeMessage(this);
		});
	},
	
	popMessage: function(messageBox) {
		$(messageBox).css({
			marginRight: "-290px",
			opacity: 0.2,
			display: "block"
		});
		
		var height = parseInt($(messageBox).outerHeight()) + parseInt($(messageBox).css("margin-bottom"));
		
		$(".sNotify_message").next().each(function() {
			var topThis = $(this).css("top");
			
			if (topThis == "auto") {
				topThis = 0;
			}
			
			var newTop = parseInt(topThis) + parseInt(height);
			
			$(this).animate({
				top: newTop + "px"
			}, {
				queue: false,
				duration: 600
			});
		});
		
		$(messageBox).animate({
			marginRight: "20px",
			opacity: 1.0
		}, 800);
	},
	
	closeMessage: function(button) {
		var height = parseInt($(button).parent().outerHeight()) + parseInt($(button).parent().css("margin-bottom"));
		
		$(button).parent().nextAll().each(function() {
			var topThis = $(this).css("top");
			
			if (topThis == "auto") {
				topThis = 0;
			}
			
			var newTop = parseInt(topThis) - parseInt(height);
			
			$(this).animate({
				top: newTop + "px"
			}, {
				queue: false,
				duration: 300
			});
		});
		
		$(button).parent().hide(200, function() {
			$(this).remove();
		});
	}
		
}

setInterval("sNotify.loopQueue()", 900);

function initKSPWAPIPoll(APIString, preUpdate, postUpdate, rawData){

	APIString = "datalink?" + APIString + "&p=p.paused"
    update();
	var nolink = true;

	function update() {

		if (rawData.length > 1) {
			preUpdate(rawData);
		}

		readStream()
	}

	function sanitise(str){

		return str.replace("nan", "0");
	}

	function readStream() {

		var callback = function(response, status){
			if (status == "success") {
				
			    d = $.parseJSON(sanitise(response));

				if (!d.p) {
					postUpdate(rawData, d);
				}

				if (rawData.length > DATA_SIZE) {
					rawData.splice(1, SPLICE_SIZE);
				}

				nolink = false;
				t = setTimeout(function(){
					update();}, UPDATE_INTERVAL);	
			}
			else {
				document.writeln(response);
			}
		};

		$.get(APIString, callback).error(function() {
			rawData.length = 1;
			rawData.push(new Array(rawData[0].length+1).join('0').split('').map(parseFloat));
			if(!nolink){
				sNotify.addToQueue("Disconnected, waiting for reconnect.");
				nolink=true;
			}
			t = setTimeout(
				function(){update();}, IDLE_UPDATE_INTERVAL);
		});
	}
}

function KSPWAPICall(APIString, postUpdate){

	var callback = function(response, status){
			d = new Object();
			d = $.parseJSON(response);
			postUpdate(d);
		};

	$.get("datalink?" + APIString, callback).error(function() {
			KSPAPILog("Command failed: " + APIString);
		}
	);
}

function KSPWAPIGetAPI(postUpdate){

	KSPWAPICall("api=a.api", postUpdate);
}

function KSPWAPIGetAPISubset(API,postUpdate){

	KSPWAPICall("api=a.apiSubSet[" + API.toString() + "]", postUpdate);
}


function KSPAPILog(msg) {
    setTimeout(function() {
        throw new Error("[jKSPWAPI]" + msg);
    }, 0);
}

function KSPWAPIFormatters(){

	  this.velocity = function(v){

      return sigFigs(v, SIG_FIG) + ' m/s';
      }

      this.distance = function(v){

      return sigFigs(v, SIG_FIG) + ' m';
      }

      this.unitless = function(v){

      return sigFigs(v,SIG_FIG);
      }

      this.time = function(v){

      return sigFigs(v,SIG_FIG) + ' s';
      }

      this.deg = function(v){
      return sigFigs(v,SIG_FIG) + '\xB0';
      }
}

function sigFigs(n, sig) {

    var m = false;

    if (n < 0) {
        m = true;
        n = Math.abs(n);
    }

    var mult = Math.pow(10,
        sig - Math.floor(Math.log(n) / Math.LN10) - 1);

    if (m) {
        n = n * -1;
    }

    return Math.round(n * mult) / mult;
}