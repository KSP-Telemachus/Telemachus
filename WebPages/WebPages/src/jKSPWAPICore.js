var sNotify = {

    timeOpen: 3,	//change this number to the amount of seconds you want the message opened

    queue: new Array(),
    closeQueue: new Array(),

    addToQueue: function (msg) {
        sNotify.queue.push(msg);
    },

    createMessage: function (msg) {

        //create HTML + set CSS
        var messageBox = $("<div><span class=\"sNotify_close\">x</span>" + msg + "</div>").insertAfter($("body").first());
        $(messageBox).addClass("sNotify_message");

        sNotify.enableActions(messageBox);
        sNotify.closeQueue.push(0);

        return $(messageBox);
    },

    loopQueue: function () {
        //pop queue
        if (sNotify.queue.length > 0) {

            var messageBox = sNotify.createMessage(sNotify.queue[0]);
            sNotify.popMessage(messageBox);

            sNotify.queue.splice(0, 1);

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
                sNotify.closeQueue.splice(indexes[i], 1);
            }

        }

    },

    enableActions: function (messageBox) {
        //reset timer when hovering
        $(messageBox).hover(
			function () {
			    var index = ($(this).nextAll().length - 1);
			    sNotify.closeQueue[index] = -1000;
			},
			function () {
			    var index = ($(this).nextAll().length - 1);
			    sNotify.closeQueue[index] = 0;
			}
		);

        //enable click close button
        $(messageBox).find(".sNotify_close").click(function () {
            sNotify.closeMessage(this);
        });
    },

    popMessage: function (messageBox) {
        $(messageBox).css({
            marginRight: "-290px",
            opacity: 0.2,
            display: "block"
        });

        var height = parseInt($(messageBox).outerHeight()) + parseInt($(messageBox).css("margin-bottom"));

        $(".sNotify_message").next().each(function () {
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

    closeMessage: function (button) {
        var height = parseInt($(button).parent().outerHeight()) + parseInt($(button).parent().css("margin-bottom"));

        $(button).parent().nextAll(".sNotify_message").each(function () {
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

        $(button).parent().hide(200, function () {
            $(this).remove();
        });
    }

}

setInterval("sNotify.loopQueue()", 900);

var jKSPWAPI = {

    DATA_SIZE: 1000,
    UPDATE_INTERVAL: 200,
    IDLE_UPDATE_INTERVAL: 1000,
    SPLICE_SIZE: 10,
    SIG_FIG: 5,
    NOTIFICATIONS: true,
    rawData: [],

    initPoll: function (APIString, preUpdate, postUpdate, rawData) {

        APIString = "datalink?" + APIString + "&p=p.paused"
        update();
        var nolink = false;
        var previous = -1;
        this.rawData = rawData;

        function update() {

            if (rawData.length > 1) {
                preUpdate(rawData);
            }

            readStream()
        }

        function sanitise(str) {

            return str.replace(/nan/g, '0');
        }

        function readStream() {

            var callback = function (response, status) {
                if (status == "success") {
                    try{
                        if (typeof(response) == "string") {
                            d = $.parseJSON(sanitise(response));
                        } else {
                            d = response;
                        }
                    }
                    catch (e) {
                        console.log(e.message + " " + response)
                    }

                    if (!d.p) {
                        postUpdate(rawData, d);
                    }

                    if (d.p != previous) {
                        previous = d.p;

                        jKSPWAPI.generateNotificationWithCode(d.p);
                    }

                    if (rawData.length > jKSPWAPI.DATA_SIZE) {
                        rawData.splice(1, jKSPWAPI.SPLICE_SIZE);
                    }

                    nolink = false;
                    t = setTimeout(function () {
                        update();
                    }, jKSPWAPI.UPDATE_INTERVAL);
                }
                else {
                    document.writeln(response);
                }
            };

            $.get(APIString, callback).error(function () {
                rawData.length = 1;

                t = setTimeout(
					function () { update(); }, jKSPWAPI.IDLE_UPDATE_INTERVAL);

                if (!nolink) {
                    jKSPWAPI.generateNotification("No antenna found, entering broadcast mode.");
                    nolink = true;
                }
            });
        }
    },

    generateNotificationWithCode: function (code) {

        if (jKSPWAPI.NOTIFICATIONS) {
            if (code == 0) {
                sNotify.addToQueue("Signal found.");
            }
            else if (code == 1) {
                sNotify.addToQueue("Game paused.");
            }
            else if (code == 2) {
                sNotify.addToQueue("Potential power loss on antenna.");
            }
            else if (code == 3) {
                sNotify.addToQueue("Antenna is deactivated.");
            }
            else if (code == 4) {
                sNotify.addToQueue("Unable to reach antenna.");
            }
        }
    },

    generateNotification: function (message) {
        if (jKSPWAPI.NOTIFICATIONS) {
            sNotify.addToQueue(message);
        }
    },

    call: function (APIString, postUpdate) {
        d = new Object();
        var callback = function (response, status) {
            if (typeof(response) == "string") {
                d = $.parseJSON(response);
            } else {
                d = response;
            }
            postUpdate(d);
        };

        $.get("datalink?" + APIString, callback).error(function () {
            jKSPWAPI.log("Command failed: " + APIString);
            d.ret = 4;
            postUpdate(d);
        }
		);
    },

    getAPI: function (postUpdate) {
        jKSPWAPI.call("api=a.api", postUpdate);
    },

    getAPISubset: function (API, postUpdate) {
        jKSPWAPI.call("api=a.apiSubSet[" + API.toString().replace(/\[[^\]]*\]/g, '') + "]", postUpdate);
    },

    log: function (msg) {
        setTimeout(function () {
            throw new Error("[jKSPWAPI]" + msg);
        }, 0);
    },

    formatters: {

        velocity: function (v) {
            f = jKSPWAPI.formatters.formatScale(v, 1000, ["Too Large", "m/s", "Km/s", "Mm/s", "Gm/s", "Tm/s"]);
            return jKSPWAPI.formatters.fix(f.value) + " " + f.unit;
        },

        distance: function (v) {
            f = jKSPWAPI.formatters.formatScale(v, 1000, ["Too Large", "m", "Km", "Mm", "Gm", "Tm"]);
            return jKSPWAPI.formatters.fix(f.value) + " " + f.unit;
        },

        formatScale: function (v, s, u) {
            var i = 1;
            var isNeg = v < 0;

            v = Math.abs(v);

            while (v > s) {
                v = v / s;
                i = i + 1;
            }

            if (i >= u.length) {
                i = 0;
            }

            if (isNeg) {
                v = v * -1;
            }

            return { "value": v, "unit": u[i] };
        },

        unitless: function (v) {

            return jKSPWAPI.formatters.fix(v);
        },

        time: function (v) {

            f = [86400, 3600, 60, 60];
            u = ["d", "h", "m", "s"];
            vprime = [0, 0, 0, 0]

            v = Math.floor(v);

            for (var i = 0; i < f.length; i++) {
                vprime[i] = Math.floor(v / f[i]);
                v %= f[i];
            }
            vprime[f.length - 1] = v;


            for (var i = 1; i < f.length; i++) {
                if (vprime[i] < 10) {
                    vprime[i] = "0" + vprime[i];
                }
            }

            var formatted = "";
            for (var i = 0; i < f.length; i++) {
                formatted = formatted + vprime[i] + u[i] + " ";
            }

            if (formatted == "") {
                formatted = 0 + u[u.length - 1];
            }

            return formatted;
        },

        date: function (v) {
          year = ((v / (365 * 24 * 3600)) | 0) + 1
          v %= (365 * 24 * 3600)
          day = ((v / (24 * 3600)) | 0) + 1
          v %= (24 * 3600)
          return "Year " + year + ", day " + day + " at " + jKSPWAPI.formatters.hourMinSec(v)
        },

        hourMinSec: function (v) {
          hour = (v / 3600) | 0
          v %= 3600
          min = (v / 60) | 0
          if (min < 10) { min = "0" + min }
          sec = (v % 60).toFixed()
          if (sec < 10) { sec = "0" + sec }
          return "" + hour + ":" + min + ":" + sec;
        },

        deg: function (v) {
            return jKSPWAPI.formatters.fix(v) + '\xB0';
        },

        fix: function (v) {
            if (v === undefined) {
                return 0;
            } else {
                return v.toPrecision(6).replace(/((\.\d*?[1-9])|\.)0+($|e)/, '$2$3');
            }
        },

        pad: function (v) {
            return ("\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0" +
				v).slice(-30)
        },

        sigFigs: function (n, sig) {

            if (n != 0) {
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
            } else {
                return 0;
            }
        }
    }
}
