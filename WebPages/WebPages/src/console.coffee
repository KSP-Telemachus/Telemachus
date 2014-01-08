standardCharts = 
  "Altitude":
    series: ["v.altitude", "v.heightFromTerrain"]
    yaxis: { label: "Altitude", unit: "m", min: 0, max: null}
  "Apoapsis and Periapsis":
    series: ["o.ApA", "o.PeA"]
    yaxis: { label: "Altitude", unit: "m", min: 0, max: null}
  "Atmospheric Density":
    series: ["v.atmosphericDensity"]
    yaxis: { label: "Altitude", unit: "Pa", min: 0, max: null}
  "Dynamic Pressure":
    series: ["v.dynamicPressure"]
    yaxis: { label: "Dynamic Pressure", unit: "Pa", min: 0, max: null}
  "G-Force":
    series: ["s.sensor.acc"]
    yaxis: { label: "Acceleration", unit: "Gs", min: null, max: null}
  "Gravity":
    series: ["s.sensor.grav"]
    yaxis: { label: "Gravity", unit: "m/s\u00B2", min: 0, max: null}
  "Pressure":
    series: ["s.sensor.pres"]
    yaxis: { label: "Pressure", unit: "Pa", min: 0, max: null}
  "Temperature":
    series: ["s.sensor.temp"]
    yaxis: { label: "Temperature", unit: "\u00B0C", min: null, max: null}
  "Orbital Velocity":
    series: ["v.orbitalVelocity"]
    yaxis: { label: "Velocity", unit: "m/s", min: 0, max: null}
  "Surface Velocity":
    series: ["v.surfaceSpeed", "v.verticalSpeed"]
    yaxis: { label: "Velocity", unit: "m/s", min: null, max: null}
  "Angular Velocity":
    series: ["v.angularVelocity"]
    yaxis: { label: "Angular Velocity", unit: "\u00B0/s", min: 0, max: null}
  "Liquid Fuel and Oxidizer":
    series: ["r.resource[LiquidFuel]", "r.resource[Oxidizer]"]
    yaxis: { label: "Volume", unit: "L", min: 0, max: null}
  "Electric Charge":
    series: ["r.resource[ElectricCharge]"]
    yaxis: { label: "Electric Charge", unit: "Wh", min: 0, max: null}
  "Monopropellant":
    series: ["r.resource[MonoPropellant]"]
    yaxis: { label: "Volume", unit: "L", min: 0, max: null}
  "Heading":
    series: ["n.heading"]
    yaxis: { label: "Angle", unit: "\u00B0", min: 0, max: 360}
  "Pitch":
    series: ["n.pitch"]
    yaxis: { label: "Angle", unit: "\u00B0", min: -90, max: 90}
  "Roll":
    series: ["n.roll"]
    yaxis: { label: "Angle", unit: "\u00B0", min: -180, max: 180}
  "Target Distance":
    series: ["tar.distance"]
    yaxis: { label: "Distance", unit: "m", min: 0, max: null}
  "Relative Velocity":
    series: ["tar.o.relativeVelocity"]
    yaxis: { label: "Velocity", unit: "m/s", min: 0, max: null}
  "True Anomaly":
    series: ["o.trueAnomaly"]
    yaxis: { label: "Angle", unit: "\u00B0", min: null, max: null}
  "Map":
    series: ["v.long", "v.lat", "v.name", "v.body"]
    type: "map"

testCharts =
  "Sine and Cosine":
    series: ["test.sin", "test.cos"]
    yaxis: { label: "Angle", unit: "\u00B0", min: null, max: null}
  "Quadratic":
    series: ["test.square"]
    yaxis: { label: "Altitude", unit: "m", min: 0, max: null}
  "Random":
    series: ["test.rand"]
    yaxis: { label: "Velocity", unit: "m/s", min: 0, max: null}
  "Square Root":
    series: ["test.sqrt"]
    yaxis: { label: "Velocity", unit: "m/s", min: 0, max: null}

customCharts = {}

charts = {}

standardLayouts = 
  "Flight Dynamics":
    charts: ["Altitude", "Orbital Velocity", "True Anomaly"],
    telemetry: ["o.sma", "o.eccentricity", "o.inclination", "o.lan", "o.argumentOfPeriapsis", "o.timeOfPeriapsisPassage", "o.trueAnomaly", "v.altitude", "v.orbitalVelocity"]
  "Retrofire":
    charts: ["Map", "Altitude", "Surface Velocity"],
    telemetry: ["v.altitude", "v.heightFromTerrain", "v.surfaceSpeed", "v.verticalSpeed", "v.lat", "v.long"]
  "Booster Systems":
    charts: ["Liquid Fuel and Oxidizer", "Dynamic Pressure", "Atmospheric Density"]
    telemetry: ["r.resource[LiquidFuel]", "r.resourceMax[LiquidFuel]", "r.resource[Oxidizer]", "r.resourceMax[Oxidizer]", "v.dynamicPressure", "v.atmosphericDensity"]
  "Instrumentation":
    charts: ["G-Force", "Pressure", "Temperature"]
    telemetry: ["s.sensor.acc", "s.sensor.pres", "s.sensor.temp", "s.sensor.grav"]
  "Electrical, Environmental and Comm.":
    charts: ["Electric Charge", "Pressure", "Temperature"]
    telemetry: ["r.resource[ElectricCharge]", "r.resourceMax[ElectricCharge]", "s.sensor.pres", "s.sensor.temp"]
  "Guidance, Navigation and Control":
    charts: ["Heading", "Pitch", "Roll"]
    telemetry: ["r.resource[MonoPropellant]", "r.resourceMax[MonoPropellant]", "n.heading", "n.pitch", "n.roll"]
  "Rendezvous and Docking":
    charts: ["Target Distance", "Relative Velocity"],
    telemetry: ["tar.name", "tar.o.sma", "tar.o.eccentricity", "tar.o.inclination", "tar.o.lan", "tar.o.argumentOfPeriapsis", "tar.o.timeOfPeriapsisPassage", "tar.o.trueAnomaly", "tar.distance", "tar.o.relativeVelocity"]

testLayouts =
  "Test":
    charts: ["Sine and Cosine", "Quadratic", "Random"]
    telemetry: ['test.rand', 'test.sin', 'test.cos', 'test.square', 'test.exp', 'test.sqrt', 'test.log']

customLayouts = {}

layouts = {}

defaultLayout = "Flight Dynamics"

Telemachus =
  CELESTIAL_BODIES: [ "Kerbol", "Kerbin", "Mun", "Minmus", "Moho", "Eve", "Duna", "Ike", "Jool", "Laythe", "Vall", "Bop", "Tylo", "Gilly", "Pol", "Dres", "Eeloo" ]
  RESOURCES: ["ElectricCharge", "SolidFuel", "LiquidFuel", "Oxidizer", "MonoPropellant", "IntakeAir", "XenonGas"]
  
  api: {}
  telemetry: { "p.paused": 0, "v.missionTime": 0, "t.universalTime": 0 }
  lastUpdate: null
  
  apiSubscriptionCounts: { "t.universalTime": 1, "v.missionTime": 1 }
  $telemetrySubscribers: $()
  $alertSubscribers: $()
  
  format: (value, api) ->
    if !value? then "No Data"
    else if $.isArray(value) then @format(value[1][0], api)
    else
      units = @api[api].units.toLowerCase()
      if units of @formatters then @formatters[units](value) else value.toString()

  subscribe: ($collection, apis...) ->
    @$telemetrySubscribers = @$telemetrySubscribers.add($collection)
    $collection.data("telemachus-api-subscriptions", apis)
    @apiSubscriptionCounts[api] = (@apiSubscriptionCounts[api] ? 0) + $collection.length for api in apis
    $collection
    
  unsubscribe: ($collection, apis...) ->
    @$telemetrySubscribers = @$telemetrySubscribers.not($collection)
    for elem in $collection
      apis = $(elem).data("telemachus-api-subscriptions")
      console.log(elem, apis)
      if apis?
        for api in apis when api of @apiSubscriptionCounts
          @apiSubscriptionCounts[api] = @apiSubscriptionCounts[api] - 1
          delete @apiSubscriptionCounts[api] if @apiSubscriptionCounts[api] <= 0
    console.log(@apiSubscriptionCounts)
    $collection
  
  subscribeAlerts: ($collection) ->
    @$alertSubscribers = @$alertSubscribers.add($collection)
  
  unsubscribeAlerts: ($collection) ->
    @$alertSubscribers = @$alertSubscribers.not($collection)
  
  loadAPI: (testMode) ->
    if testMode
      @api =
        "p.paused": { apistring: 'p.paused', name: "Paused", units: 'UNITLESS'}
        "v.missionTime": { apistring: 'v.missionTime', name: "Mission Time", units: 'TIME'}
        "t.universalTime": { apistring: 't.universalTime', name: "Universal Time", units: 'DATE'}
        "test.rand": { apistring: 'test.rand', name: "Random", units: 'UNITLESS'}
        "test.sin": { apistring: 'test.sin', name: "Sine", units: 'UNITLESS'}
        "test.cos": { apistring: 'test.cos', name: "Cosine", units: 'UNITLESS'}
        "test.square": { apistring: 'test.square', name: "Quadratic", units: 'UNITLESS'}
        "test.exp": { apistring: 'test.exp', name: "Exponential", units: 'UNITLESS'}
        "test.sqrt": { apistring: 'test.sqrt', name: "Square Root", units: 'UNITLESS'}
        "test.log": { apistring: 'test.log', name: "Logarithmic", units: 'UNITLESS'}
      
      @testStart = Date.now()
      @testDownlink()
      $.Deferred().resolve(@api).promise()
    else
      $.get("datalink", { api: "a.api" }, "json").then (data, textStatus, jqXHR) =>
        for api in JSON.parse(data).api
          if api.apistring.match(/^b\./)
            @api[api.apistring + "[#{i}]"] = api for i in [0...@CELESTIAL_BODIES.length]
          else if api.apistring.match(/^r\./)
            if api.apistring != "r.resourceCurrent"
              for r in @RESOURCES
                resourceApi = $.extend({}, api)
                resourceApi.name = r.replace(/([a-z])([A-Z])/g, "$1 $2")
                resourceApi.name += " Max" if api.apistring.match(/Max$/) 
                @api[api.apistring + "[#{r}]"] = resourceApi
          else if api.plotable and api.apistring != "s.sensor"
            @api[api.apistring] = api
        
        @downlink()
        @api
      , =>
        @$alertSubscribers.trigger("telemetryAlert", ["No Signal Found"])
        timeout = $.Deferred()
        setTimeout((-> timeout.resolve()), 5000)
        timeout.then(=> @loadAPI())
  
  downlink: ->
    query = {}
    i = 0
    query["p"] = "p.paused"
    query["a#{i++}"] = api for api of @apiSubscriptionCounts when api != "p.paused"
    url = "datalink?#{("#{key}=#{api}" for key, api of query).join("&")}"
    $.get(url).done (data, textStatus, jqXHR) =>
      try
        data = JSON.parse(data)
      catch error
        @$alertSubscribers.trigger("telemetryAlert", ["Bad Data"])
        setTimeout((=> @downlink()), 5000)
        return
      
      @telemetry["p.paused"] = data.p
      switch data.p
        when 4 then @$alertSubscribers.trigger("telemetryAlert", ["Signal Lost"])
        when 3 then @$alertSubscribers.trigger("telemetryAlert", ["Signal Terminated"])
        when 2 then @$alertSubscribers.trigger("telemetryAlert", ["Signal Power Loss"])
        when 1 then @$alertSubscribers.trigger("telemetryAlert", ["Game Paused"])
        when 0 then @$alertSubscribers.trigger("telemetryAlert", [null])
    
      if data.p != 1
        @lastUpdate = Date.now()
        @telemetry = {}
        for key, value of data
          if data.p == 0 or ["p.paused", "v.missionTime", "t.universalTime"].indexOf(query[key]) != -1
            @telemetry[query[key]] = value
          else
            @telemetry[query[key]] = null

        @$telemetrySubscribers.trigger("telemetry", [@telemetry])
    
      setTimeout((=> @downlink()), 500)
    .fail =>
      @$alertSubscribers.trigger("telemetryAlert", ["No Signal Found"])
      setTimeout((=> @downlink()), 5000)
  
  testDownlink: ->
    rand = Math.random() * 1000
    paused = if rand >= 10 then 0 else Math.floor(rand / 2)
    
    switch paused
      when 4 then @$alertSubscribers.trigger("telemetryAlert", ["Signal Lost"])
      when 3 then @$alertSubscribers.trigger("telemetryAlert", ["Signal Terminated"])
      when 2 then @$alertSubscribers.trigger("telemetryAlert", ["Signal Power Loss"])
      when 1 then @$alertSubscribers.trigger("telemetryAlert", ["Game Paused"])
      when 0 then @$alertSubscribers.trigger("telemetryAlert", [null])
    
    if paused != 1
      @lastUpdate = Date.now()
      t = (@lastUpdate - @testStart) / 1000
      lastRand = @telemetry["test.rand"] ? rand
      @telemetry = { "p.paused": paused, "v.missionTime": t, "t.universalTime": @lastUpdate / 1000}
      x = t / 120
      for api of @apiSubscriptionCounts
        unless api of @telemetry
          @telemetry[api] = if paused != 0 then null else
            switch api
              when 'test.rand' then lastRand + (rand - 500) / 10
              when 'test.sin' then 1000 * Math.sin(x * 2 * Math.PI)
              when 'test.cos' then 1000 * Math.cos(x * 2 * Math.PI)
              when 'test.square' then x * x
              when 'test.exp' then Math.exp(x)
              when 'test.sqrt' then Math.sqrt(x)
              when 'test.log' then Math.log(x)
      @$telemetrySubscribers.trigger("telemetry", [@telemetry])
      
    setTimeout((=> @testDownlink()), if paused == 0 then 500 else 5000)
    
  formatters:
    unitless: (v) -> if typeof v == "number" then v.toPrecision(6) else v
    velocity: (v) -> siUnit(v, "m/s") 
    deg: (v) -> v.toPrecision(6) + "\u00B0"
    distance: (v) -> siUnit(v, "m")
    time: (v) -> durationString(v)
    string: (v) -> v
    temp: (v) -> v.toPrecision(6) + "\u00B0C"
    pres: (v) -> siUnit(v / 1000, "Pa")
    grav: (v) -> siUnit(v, "m/s\u00B2")
    acc: (v) -> v.toPrecision(6) + " G"
    date: (v) -> dateString(v)

$(document).ready ->
  # TODO:
  #  Custom charts (auto-save)
  #  Custom layout import/export
  
  if window.localStorage?
    customCharts = JSON.parse(window.localStorage.getItem("telemachus.console.charts")) ? {}
    $.extend(charts, standardCharts, customCharts)
    customLayouts = JSON.parse(window.localStorage.getItem("telemachus.console.layouts")) ? {}
    $.extend(layouts, standardLayouts, customLayouts)
    savedDefault = window.localStorage.getItem("defaultLayout")
    defaultLayout = savedDefault if savedDefault? and savedDefault of layouts
    
  testMode = (window.location.protocol == "file:" or window.location.hash == "#test")
  if testMode
    $.extend(layouts, testLayouts)
    $.extend(charts, testCharts)
    defaultLayout = "Test"
  
  # Populate layout and chart menus
  $layoutMenu = $("body > header nav ul")
  populateLayoutMenu = ->
    $layoutMenu.empty()
    $layoutMenu.append($("<li>").append($("<a>").attr(href: "#").text(layout))) for layout of layouts
  populateLayoutMenu()
  $layoutMenu.on "click", "li a", (event) ->
    event.preventDefault()
    layoutName = $(this).text().trim()
    setLayout(layoutName)
    $("#deleteLayout").prop("disabled", layoutName not of customLayouts)
    $(this).closest("ul").hide()
  
  $chartMenus = $(".chart nav ul")
  $chartMenus.append($("<li>").append($("<a>").attr(href: "#").text(chart))) for chart of charts
  $chartMenus.on "click", "li a", (event) ->
    event.preventDefault()
    setChart($(this).closest(".chart"), $(this).text())
    $(this).closest("ul").hide()
    
  $(document).on "click.dropdown", ".dropdown > a", ->
    $this = $(this)
    $menu = $this.next()
    $menu.toggle().css
      left: Math.max($this.position().left + $this.width() - $menu.outerWidth(), 0)
      top: $this.position().top + Math.min($(window).height() - $menu.outerHeight() - $this.offset().top, $this.height())
  
  $(document).on "click.dropdown", ->
    $(".dropdown").not($(event.target).parents()).children("ul").hide()
  
  setLayout(defaultLayout)
  $("#deleteLayout").prop("disabled", defaultLayout not of customLayouts)
  
  $("#apiCategory").change (event) ->
    category = $("#apiCategory").val()
    $("#apiSelect").empty()
    for apistring, api of Telemachus.api when apistring.match(category)
      $("#apiSelect").append($("<option>").attr("value", apistring).text(api.name))
  
  $("#telemetry").on "click", "dt a", (event) ->
    event.preventDefault()
    removeTelemetry($(this).parent())
  
  $(".alert").on "telemetryAlert", (event, message) ->
    $(".alert").text(message ? "")
    if message?
      $this = $(this)
      $display = $this.siblings(".display")
      $this.css("marginTop", -($display.outerHeight() + $this.height()) / 2)
  
  if window.localStorage?
    $("#saveLayout").click (event) ->
      event.preventDefault()
      name = window.prompt("What name would you like to save this layout under?", $("h1").text().trim()).trim()
      return if !name? or name == "" or (name of layouts and !window.confirm("That name is already in use. Are you sure you want to overwrite the existing layout?"))
      layouts[name] = customLayouts[name] =
        charts: ($(elem).text().trim() for elem in $(".chart h2"))
        telemetry: ($(elem).data("api") for elem in $("#telemetry dt"))
      window.localStorage.setItem("telemachus.console.layouts", JSON.stringify(customLayouts))
      populateLayoutMenu()
      $("h1").text(name)
      $("#deleteLayout").prop("disabled", false)
      
    $("#deleteLayout").click (event) ->
      event.preventDefault()
      if window.confirm("Are you sure you want to delete the current custom layout?")
        layoutName = $("h1").text().trim()
        return unless layoutName of customLayouts
        delete customLayouts[layoutName]
        window.localStorage.setItem("telemachus.console.layouts", JSON.stringify(customLayouts))
        console.log("hello")
        
        if layoutName of standardLayouts
          layouts[layoutName] = standardLayouts[layoutName]
          reloadLayout()
          $("#deleteLayout").prop("disabled", true)
        else
          delete layouts[layoutName]
          populateLayoutMenu()
          $("body > header nav ul li:first-child a").click()
  else
    $("#saveLayout").prop("disabled", true)
    $("#deleteLayout").prop("disabled", true)
  
  Telemachus.subscribeAlerts($(".alert"))
  
  Telemachus.loadAPI(testMode).done ->
    $("#apiCategory").change()
    reloadLayout()
  
  # Clock updater
  setInterval ->
    if Telemachus.telemetry["p.paused"] != 1
      missionTime = Telemachus.telemetry["v.missionTime"]
      missionTime += (Date.now() - Telemachus.lastUpdate) / 1000 if missionTime > 0
      universalTime = Telemachus.telemetry["t.universalTime"] + (Date.now() - Telemachus.lastUpdate) / 1000
      $("#met").text(missionTimeString(missionTime))
      $("#ut").text(dateString(universalTime))
  , 1000
  
  $("#telemetry form").submit (event) ->
    event.preventDefault()
    addTelemetry($("#apiSelect").val())

  $(window).resize ->
    winHeight = Math.min($(window).height(), window.innerHeight ? 1e6)
    $("#container").height(winHeight - ($("#container").position().top + $("body > footer").outerHeight(true)))
  
    for display in $(".display", "#charts")
      $display= $(display)
      $chart = $display.closest(".chart")
      $display.height($chart.height() - $display.position().top - 20)
      $alert = $display.siblings(".alert")
      $alert.css("fontSize", $display.height() / 5).css("marginTop", -($display.outerHeight() + $alert.height()) / 2)
    
    $(canvas).prop(width: $(canvas).width(), height: $(canvas).height()) for canvas in $("canvas")
  
    $telemetry = $("#telemetry")
    $telemetryList = $("dl", $telemetry)
    $telemetryForm = $("form", $telemetry)
    margins = $telemetryList.outerHeight(true) - $telemetryList.height()
    $telemetryList.height($telemetryForm.position().top - $telemetryList.position().top - margins)
  
    $("form", $telemetry).width($telemetry.width())
    $telemetrySelect = $("#apiSelect")
    $telemetryAdd = $("form input", $telemetry)
    buttonWidth = $telemetryAdd.outerWidth(true) + 5 # 5px for whitespace between controls
    $telemetrySelect.width($telemetry.width() - buttonWidth)
      
  .resize()

addTelemetry = (api) ->
  if api? and api of Telemachus.api and $("#telemetry dd[data-api='#{api}']").length == 0
    $("<dt>").data("api", api).text(Telemachus.api[api].name + " ")
      .append($("<a>").attr(href: "#", title: "Remove")).appendTo("#telemetry dl")
    $dd = $("<dd>").data("api", api).text("No Data").appendTo("#telemetry dl").on "telemetry", (event, data) ->
      value = data[api]
      $dd.text(Telemachus.format(value, api))
    Telemachus.subscribe($dd, api)

removeTelemetry = (elem) ->
  $elem = $(elem).next().addBack()
  Telemachus.unsubscribe($elem)
  $elem.remove()
  
resetChart = (elem) ->
  $display = $(".display", elem).empty()
  Telemachus.unsubscribe($display)

setChart = (elem, chartName) ->
  resetChart(elem)
  
  chart = charts[chartName]
  return unless chart?
  
  $("h2", elem).text(chartName)
  $display = $(".display", elem)
  Telemachus.subscribe($display, chart.series...)
  
  if chart.type == "map"
    $map = $("<div>").appendTo($display)
    
    map = new L.KSP.Map $map[0],
      layers: [L.KSP.CelestialBody.KERBIN],
      zoom: L.KSP.CelestialBody.KERBIN.defaultLayer.options.maxZoom,
      center: [0, 0],
      bodyControl: false,
      layerControl: true,
      scaleControl: true
      
    map.fitWorld()
    
    body = L.KSP.CelestialBody.KERBIN
    marker = null
    
    $display.on "telemetry", (event, data) ->
      bodyName = data["v.body"]?.toUpperCase()
      if data["p.paused"] != 0
        if marker?
          map.removeLayer(marker)
          marker = null
      else if bodyName?
        if !(bodyName of L.KSP.CelestialBody)
          if bodyName? and body?
            map.removeLayer(body)
            body = null
        else
          if body != L.KSP.CelestialBody[data["v.body"].toUpperCase()]
            map.removeLayer(body)
            body = L.KSP.CelestialBody[data["v.body"].toUpperCase()]
            map.addLayer(body)
        
          long = if data["v.long"] > 180 then data["v.long"] - 360 else data["v.long"]
          if !marker?
            marker = L.marker([data["v.lat"], data["v.long"]])
            map.addLayer(marker)
          else
            marker.setLatLng([data["v.lat"], long])
            marker.bindPopup(data["v.name"] + " </br>Latitude: " + data["v.lat"] + "</br>Longitude: " + data["v.long"])
            marker.update()
  else
    $canvas = $("<canvas>").attr(width: $display.width(), height: $display.height()).appendTo($display)
    seriesData = []
    
    $display.on "telemetry", (event, data) ->
      t = data["t.universalTime"]
      sample = (data[e] for e in chart.series)
      sample[i] = e[1][0] for e, i in sample when $.isArray(e)
      sample.unshift(t)
      
      seriesData = (e for e in seriesData when e[0] < t and e[0] > t - 300)
      seriesData.push(sample)
      
      $canvas = $(".display canvas", elem)
      return if $canvas.length == 0
      width = $canvas.width()
      height = $canvas.height()
      return if width == 0 or height == 0
      
      chartWidth = width - 80
      chartHeight = height - 40
      
      ctx = $canvas[0].getContext('2d')
      ctx.save()
      
      ctx.clearRect(0, 0, width, height)
      
      # X-axis parameters
      xmin = t - 300
      xmax = t
      xscale = chartWidth / 300
      missionTime = data["v.missionTime"]
      xticks = (x for x in [(t - missionTime % 60)...xmin] by -60 when missionTime > 0 and (t - x - 0.01) <= missionTime).reverse()
      
      # Figure out the Y-axis parameters
      if chart.yaxis.min? and chart.yaxis.max?
        ymin = chart.yaxis.min
        ymax = chart.yaxis.max
      else
        dataValues = (e for e in [].concat((e.slice(1) for e in seriesData)...) when e?)
        if dataValues.length > 0
          ymin = chart.yaxis.min ? Math.min(dataValues...)
          ymax = chart.yaxis.max ? Math.max(dataValues...)
        else
          ymin = chart.yaxis.min ? chart.yaxis.max ? 0
          ymax = chart.yaxis.max ? chart.yaxis.min ? 0
      
      yticks = calculateTicks(ymin, ymax, chart.yaxis.min?, chart.yaxis.max?, ((height - 40) / 30) | 0)
      
      ymin = yticks[0]
      ymax = yticks[yticks.length - 1]
      yscale = chartHeight / (ymax - ymin)
      
      ctx.translate(70, height - 30)
      ctx.scale(1, -1)
      
      drawChartGrid(ctx, chartWidth, chartHeight, xmin, xscale, ymin, yscale, xticks, yticks)
      drawChartSeries(ctx, xmin, xscale, ymin, yscale, seriesData, i) for i in [chart.series.length..1]
      
      ctx.clearRect(-70, -30, 70, height)
      ctx.clearRect(-70, -30, width, 30)
      
      drawChartAxes(ctx, chartWidth, chartHeight, ymin, yscale, yticks, chart.yaxis)
      drawChartLegend(ctx, chartWidth, chart.series) if chart.series.length > 1
      
      ctx.restore()

calculateTicks = (min, max, bottomFixed, topFixed, maxIntervals) ->
  reduce = (interval) ->
    switch interval[0]
      when 5 then [2, interval[1]]
      when 2 then [1, interval[1]]
      when 1 then [5, interval[1] - 1]
  
  intervalValue = (interval) -> interval[0] * Math.pow(10, interval[1])
  
  intervalAbove = (num, interval) ->
    v = intervalValue(interval)
    m = if num < 0 then v + num % v else num % v
    num - m + v
    
  intervalBelow = (num, interval) ->
    v = intervalValue(interval)
    m = if num < 0 then v + num % v else num % v
    if m == 0 then num - v else num - m
  
  if max < min
    if topFixed and bottomFixed
      [min, max] = [max, min]
    else if topFixed
      min = max
    else
      max = min
  
  if maxIntervals < 1
    maxIntervals = 1

  bottom = min
  top = max
  
  # Special case for angle axes
  if bottomFixed and topFixed and top - bottom >= 90 and ((top - bottom) % 90 == 0)
    intervals = [15, 30, 45, 90]
    intervals.shift() while ((top - bottom) / intervals[0]) > maxIntervals
    return (tick for tick in [bottom..top] by intervals[0])
    
  if min == max
    if max == 0
      return [0, 1]
    else
      magnitude = orderOfMagnitude(max)
      interval = [1, magnitude]
      bottom = intervalBelow(min, interval) unless bottomFixed
      top = intervalAbove(max, interval) unless topFixed and not bottomFixed
      topFixed = bottomFixed = true
  else
    magnitude = Math.max(orderOfMagnitude(min), orderOfMagnitude(max))
    interval = [1, magnitude]
    bottom = intervalBelow(min, interval) unless bottomFixed
    top = intervalAbove(min, interval) unless topFixed
  
  loop
    nextInterval = reduce(interval)
    nextBottom = if bottomFixed then bottom else intervalBelow(min, nextInterval)
    nextTop = if topFixed then top else intervalAbove(max, nextInterval)
    
    break if (nextTop - nextBottom) / intervalValue(nextInterval) > maxIntervals
    
    [bottom, top, interval] = [nextBottom, nextTop, nextInterval]
  
  ticks = [bottom, top]
  ticks[1..0] = (i for i in [intervalAbove(bottom, interval)...top] by intervalValue(interval))
  ticks

drawChartGrid = (ctx, width, height, xoffset, xscale, yoffset, yscale, xticks, yticks) ->
  ctx.save()
  
  ctx.strokeStyle = 'rgb(96, 96, 96)'
  
  ctx.beginPath()
  for tick in xticks
    x = Math.round(xscale * (tick - xoffset) - 0.5) + 0.5
    ctx.moveTo(x, 0)
    ctx.lineTo(x, height)
  for tick in yticks
    y = Math.round(yscale * (tick - yoffset) - 0.5) + 0.5
    ctx.moveTo(0, y)
    ctx.lineTo(width, y)
  ctx.stroke()
  
  ctx.strokeStyle = 'rgb(192, 192, 192)'
  ctx.beginPath()
  y = Math.round(-yscale * yoffset - 0.5) + 0.5
  ctx.moveTo(0, y)
  ctx.lineTo(width, y)
  ctx.stroke()
  
  ctx.restore()

drawChartSeries = (ctx, xmin, xscale, ymin, yscale, seriesData, i) ->
  SERIES_COLORS = ['rgb(192, 128, 0)', 'rgb(0, 128, 0)', 'rgb(0, 128, 192)', 'rgb(192, 192, 192)']
  ctx.save()
  
  ctx.lineWidth = 2
  ctx.lineJoin = 'round'
  ctx.lineCap = 'round'
  ctx.strokeStyle = SERIES_COLORS[i-1]
  
  ctx.beginPath()
  for d, j in seriesData when d[i]?
    if seriesData[j-1]?[i]?
      ctx.lineTo(xscale * (d[0] - xmin), yscale * (d[i] - ymin))
    else
      ctx.moveTo(xscale * (d[0] - xmin), yscale * (d[i] - ymin))
  ctx.stroke()
  
  ctx.restore()

drawChartAxes = (ctx, width, height, yoffset, yscale, yticks, yaxis) ->
  PREFIXES = ['f', 'p', 'n', '\u03bc', 'm', '', 'k', 'M', 'G', 'T', 'P', 'E', 'Z', 'Y']
  
  ctx.save()
  
  ctx.strokeStyle = 'rgb(192, 192, 192)'
  ctx.beginPath()
  ctx.moveTo(-5, 0.5)
  ctx.lineTo(width + 0.5, 0.5)
  ctx.moveTo(0.5, 0)
  ctx.lineTo(0.5, height + 0.5)
  
  for tick in yticks when tick != yoffset
    y = Math.round(yscale * (tick - yoffset) - 0.5) + 0.5
    ctx.moveTo(-5, y)
    ctx.lineTo(0.5, y)
    
  ctx.stroke()
  
  tickMagnitude = Math.max(orderOfMagnitude(yticks[0]), orderOfMagnitude(yticks[yticks.length - 1]))
  tickMagnitude -= 1 if tickMagnitude > 0
  tickScale = Math.ceil(tickMagnitude / 3)
  tickScale -= 1 if tickScale > 0
  prefix = PREFIXES[tickScale + 5]
  
  ctx.font = 'bold 10pt "Helvetic Neue",Helvetica,Arial,sans serif'
  ctx.textBaseline = 'middle'
  ctx.fillStyle = 'rgb(192, 192, 192)'
  ctx.scale(1, -1)
  
  ctx.save()
  ctx.textAlign = 'center'
  ctx.translate(-60, -height / 2)
  ctx.rotate(-Math.PI / 2)
  ctx.fillText("#{yaxis.label} (#{prefix}#{yaxis.unit})", 0, 0, height)
  ctx.restore()
  
  ctx.textAlign = 'right'
  tickScale = Math.pow(1000, -tickScale)
  for tick in yticks
    y = Math.round(yscale * (tick - yoffset) - 0.5) + 0.5
    ctx.fillText(stripInsignificantZeros((tick * tickScale).toFixed(3)), -10, -y)
  
  ctx.restore()

drawChartLegend = (ctx, width, series) ->
  SERIES_COLORS = ['rgb(192, 128, 0)', 'rgb(0, 128, 0)', 'rgb(0, 128, 192)', 'rgb(192, 192, 192)']
  
  ctx.save()
  
  ctx.font = 'bold 10pt "Helvetic Neue",Helvetica,Arial,sans serif'
  ctx.textAlign = 'left'
  ctx.textBaseline = 'middle'

  apiWidths = (ctx.measureText(Telemachus.api[api].name).width for api in series)
  legendWidth = (apiWidths.reduce (sum, width) -> sum + width) + (series.length - 1) * 40
  
  ctx.scale(1, -1)
  x = width / 2 - legendWidth / 2
  for api, i in series
    ctx.fillStyle = SERIES_COLORS[i]
    ctx.fillRect(x, 17.5, 5, 5)
    ctx.fillStyle = 'rgb(192, 192, 192)'
    ctx.fillText(Telemachus.api[api].name, x + 10, 20)
    x += apiWidths[i] + 40
  
  ctx.restore()
  
  
reloadLayout = -> setLayout($("h1").text().trim())

setLayout = (name) ->
  if name of layouts
    window.localStorage.setItem("defaultLayout", name)
    $("h1").text(name)
    layout = layouts[name]
    removeTelemetry(elem) for elem in $("#telemetry dl dt")
    addTelemetry(telemetry) for telemetry in layout.telemetry
    setChart(elem, layout.charts[i]) for elem, i in $(".chart")

orderOfMagnitude = (v) ->
  return 0 if v == 0
  Math.floor(Math.log(Math.abs(v)) / Math.LN10 + 1.0000000000000001)

siUnit = (v, unit = "") ->
  return "0 #{unit}" if v == 0
  
  prefixes = ['\u03bc', 'm', '', 'k', 'M', 'G', 'T']
  scale = Math.ceil(orderOfMagnitude(v) / 3)
  
  if scale <= 0 and ++scale < 0
    scale = 0
  else if scale == 1
    scale = 2
  else if scale >= prefixes.length
    scale = prefixes.length - 1
  
  (v / Math.pow(1000, scale - 2)).toPrecision(6) + " " + prefixes[scale] + unit

stripInsignificantZeros = (v) -> v.toString().replace(/((\.\d*?[1-9])|\.)0+($|e)/, '$2$3')

hourMinSec = (t) ->
  hour = (t / 3600) | 0
  hour = "0#{hour}" if hour < 10
  t %= 3600
  min = (t / 60) | 0
  min = "0#{min}" if min < 10
  sec = (t % 60).toFixed()
  sec = "0#{sec}" if sec < 10
  "#{hour}:#{min}:#{sec}"
  
dateString = (t) ->
  year = ((t / (365 * 24 * 3600)) | 0) + 1
  t %= (365 * 24 * 3600)
  day = ((t / (24 * 3600)) | 0) + 1
  t %= (24 * 3600)
  "Year #{year}, Day #{day}, #{hourMinSec(t)} UT"

missionTimeString = (t) ->
  result = "T+"
  if t >= 365 * 24 * 3600
    result += (t / (365 * 24 * 3600) | 0) + ":"
    t %= 365 * 24 * 3600
    result += "0:" if t < 24 * 3600
  result += (t / (24 * 3600) | 0) + ":" if t >= 24 * 3600
  t %= 24 * 3600
  result + hourMinSec(t) + " MET"

durationString = (t) ->
  result = ""
  if t >= 365 * 24 * 3600
    result += (t / (365 * 24 * 3600) | 0) + " years "
    t %= 365 * 24 * 3600
    result += "0 days " if t < 24 * 3600
  result += (t / (24 * 3600) | 0) + " days " if t >= 24 * 3600
  t %= 24 * 3600
  result + hourMinSec(t)
