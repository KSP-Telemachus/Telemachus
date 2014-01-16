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
    yaxis: { label: "Angle", unit: "\u00B0", min: -360, max: 360}
  "Quadratic":
    series: ["test.square"]
    yaxis: { label: "Altitude", unit: "m", min: 0, max: null}
  "Random":
    series: ["test.rand"]
    yaxis: { label: "Velocity", unit: "m/s", min: null, max: null}
  "Square Root":
    series: ["test.sqrt"]
    yaxis: { label: "Velocity", unit: "m/s", min: 0, max: null}
  "Exponential":
    series: ["test.exp"]
    yaxis: { label: "Velocity", unit: "m/s", min: 1, max: null}
  "Logarithmic":
    series: ["test.log"]
    yaxis: { label: "Velocity", unit: "m/s", min: null, max: null}

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
    charts: ["Sine and Cosine", "Exponential", "Random"]
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
      if apis?
        for api in apis when api of @apiSubscriptionCounts
          @apiSubscriptionCounts[api] = @apiSubscriptionCounts[api] - 1
          delete @apiSubscriptionCounts[api] if @apiSubscriptionCounts[api] <= 0
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
              when 'test.sin' then 360 * Math.sin(x * 2 * Math.PI)
              when 'test.cos' then 360 * Math.cos(x * 2 * Math.PI)
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

class Chart
  uniqueId = (-> counter = 0; -> "chart-id-#{counter++}")()
  
  activeCharts = []
  resizeHandler = (event) ->
    activeCharts = (c for c in activeCharts when $.contains(document, c.svg.node()))
    if activeCharts.length == 0
      $(window).off('resize', resizeHandler)
    else
      c.resize() for c in activeCharts
  
  constructor: (parent, series, yaxis) ->
    $parent = $(parent)
    @data = []
    @series = series.slice()
    
    # Styling parameters
    # We use 1/2 pixels for the padding so that single pixel lines
    # will be aligned with display pixels
    @padding = { left: 70.5, top: 13.5, right: 13.5, bottom: 30.5 }
    @padding.bottom = 13.5 if @series.length <= 1
    @legendSpacing = 30
    
    # Determine the (initial) chart size
    @width = $parent.width()
    @height = $parent.height()
    dataWidth = Math.max(@width - (@padding.left + @padding.right), 0)
    dataHeight = Math.max(@height - (@padding.top + @padding.bottom), 0)
    
    # Create the X and Y scales
    @x = d3.scale.linear()
      .range([0, dataWidth])
      .domain([0, 300])
      
    magnitude = Math.max(orderOfMagnitude(yaxis.min), orderOfMagnitude(yaxis.max))
    prefix = d3.formatPrefix(Math.pow(10, magnitude - 2))
    @y = d3.scale.linear()
      .range([dataHeight, 0])
      .domain([prefix.scale(yaxis.min), prefix.scale(yaxis.max)])
    @y.prefix = prefix
    @y.fixedDomain = [yaxis.min, yaxis.max]
    
    # Create the X and Y axes
    @xaxis = d3.svg.axis()
      .scale(@x)
      .orient("bottom")
      .tickSize(dataHeight, 0)
      .tickFormat (d) =>
        if @missionTimeOffset
          t = (d - @missionTimeOffset) / 60
          if t < 0
            result = "T-"
            t = -t
          else
            result = "T+"
          h = (t / 60 | 0)
          m = t % 60 | 0
          m = "0#{m}" if m < 10
          result + h + ":" + m
      .tickValues([])
      
    @yaxis = d3.svg.axis()
      .scale(@y)
      .orient("left")
      .ticks((dataHeight / 39) | 0)
    @yaxis.label = yaxis.label
    @yaxis.unit = yaxis.unit
    if @y.fixedDomain[0]? and @y.fixedDomain[1]? and (@y.fixedDomain[1] - @y.fixedDomain[0]) % 90 == 0
      @yaxis.tickValues(angleTicks(@y.fixedDomain, @yaxis.ticks()...))
    
    # Generate the SVG
    
    @svg = d3.select($parent[0]).append("svg:svg")
      .attr("width", @width)
      .attr("height", @height)
    
    rootGroup = @svg.append("svg:g")
      .attr("transform", "translate(#{@padding.left}, #{@padding.top})")
    
    # Clipping rectangle for the chart data to keep it in bounds
    # (though we allow overshooting the top of the chart)
    clipPathId = uniqueId()
    rootGroup.append("svg:defs").append("svg:clipPath")
        .attr("id", clipPathId)
      .append("svg:rect")
        .attr("x", 0)
        .attr("y", -@padding.top)
        .attr("width", dataWidth)
        .attr("height", dataHeight + @padding.top)
    
    # Group for the Y-axis grid lines
    rootGroup.append("svg:g")
      .attr("class", "y grid")
      .attr("clip-path", "url(##{clipPathId})")
    
    # The X-axis
    g = rootGroup.append("svg:g")
      .attr("class", "x axis")
    g.append("svg:g")
      .attr("class", "ticks")
      .attr("clip-path", "url(##{clipPathId})")
    g.append("svg:path")
      .attr("class", "domain")
      .attr("d", "M0,#{dataHeight}H#{dataWidth}")
    refreshXAxis.call(@)
    
    # The Y-axis
    g = rootGroup.append("svg:g").attr("class", "y axis")
    g.append("svg:text")
      .attr("class", "label")
      .attr("text-anchor", "middle")
      .attr("x", -dataHeight / 2)
      .attr("y", -(@padding.left - 18))
      .attr("transform", "rotate(-90)")
      .text(@yaxis.label + (if @yaxis.unit? or @y.prefix.symbol != '' then " (#{@y.prefix.symbol}#{@yaxis.unit})" else ''))
    refreshYAxis.call(@)
    
    # The data lines
    rootGroup.append("svg:g")
        .attr("class", "data")
        .attr("clip-path", "url(##{clipPathId})")
      .selectAll("path").data(@series).enter().append("svg:path")
    
    # Add a legend if we have multiple series of data
    if @series.length > 1
      tspan = rootGroup.append("svg:text")
          .attr("class", "legend")
          .attr("transform", "translate(#{dataWidth / 2}, #{dataHeight + 20})")
          .attr("text-anchor", "middle")
        .selectAll("tspan").data(@series).enter().append("svg:tspan")
          .attr("dx", (d, i) => @legendSpacing if i > 0)
          .on "mouseover", (d, i) =>
            if @svg.select(".active").empty()
              @svg.selectAll(".data path").classed "inactive", (d, j) -> j != i
              @svg.selectAll(".legend > tspan").classed "inactive", (d, j) -> j != i
          .on "mouseout", =>
            if @svg.select(".active").empty()
              @svg.selectAll(".data path, .legend > tspan").classed "inactive", false
          .on "click", (d, i) ->
            if d3.select(@).classed "active"
              rootGroup.selectAll(".data path, .legend > tspan")
                .classed("inactive", false)
                .classed("active", false)
            else
              rootGroup.selectAll(".data path").classed("inactive", (d, j) -> j != i)
              rootGroup.selectAll(".legend > tspan")
                .classed("inactive", (d, j) -> j != i)
                .classed("active", (d, j) -> j == i)
              
      
      tspan.append("svg:tspan").attr("class", "bullet").text("\u25fc ") # Unicode medium black square
      tspan.append("svg:tspan").attr("class", "title").text((d) -> d)
    
    activeCharts.push(@)
    $(window).on('resize', resizeHandler) if activeCharts.length == 1
    
    # TODO: Smooth y-axis scaling
  
  destroy: ->
    i = activeCharts.indexOf(@)
    activeCharts.splice(i, 1)
    $(window).off('resize', resizeHandler) if activeCharts.length == 0
    
  addSample: (x, sample) ->
    @data.push([x].concat(sample))
    @data.sort((a,b) -> a[0] - b[0])
    
    if @lastUpdate?
      dt = Date.now() - (@lastUpdate ? 0)
      @lastUpdate += dt
    else
      dt = 0
      @lastUpdate = Date.now()
    
    # Discard data outside of the x-axis domain
    for i in [(@data.length - 1)..0]
      if @data[i][0] <= @x.domain()[1]
        windowEnd = i + 1
        break
    
    if dt > 100 and dt < 1000 and @data.length >= 2
      x1 = @data[@data.length - 2][0]
      x2 = @data[@data.length - 1][0]
    else
      x1 = x2 = 0
    
    windowStart = 0
    for i in [1...@data.length]
      if @data[i][0] >= @x.domain()[0] - (x2 - x1)
        windowStart = i - 1
        break
    
    @data = @data.slice(windowStart, windowEnd)
    
    # Check the y-axis domain
    unless @y.fixedDomain[0]? and @y.fixedDomain[1]?
      extent = d3.extent(d3.merge(e.slice(1) for e in @data))
      extent = [@y.fixedDomain[0] ? extent[0], @y.fixedDomain[1] ? extent[1]]
      if extent[1] < extent[0]
        if @y.fixedDomain[0]? then extent[1] = extent[0] else extent[0] = extent[1]
        
      if @y.prefix.scale(extent[0]) != @y.domain()[0] or @y.prefix.scale(extent[1]) != @y.domain()[1]
        # Check for a SI-prefix change
        magnitude = Math.max(orderOfMagnitude(extent[0]), orderOfMagnitude(extent[1]))
        prefix = d3.formatPrefix(Math.pow(10, magnitude - 2))
        if prefix.symbol != @y.prefix.symbol
          @y.prefix = prefix
          @svg.select('.y.axis text.label')
            .text(@yaxis.label + (if @yaxis.unit? or @y.prefix.symbol != '' then " (#{@y.prefix.symbol}#{@yaxis.unit})" else ''))
        
        # Update the domain to nice values while preserving the fixed ends
        @y.domain([@y.prefix.scale(extent[0]), @y.prefix.scale(extent[1])]).nice(@yaxis.ticks()...)
          .domain [
            if @y.fixedDomain[0]? then @y.prefix.scale(@y.fixedDomain[0]) else @y.domain()[0]
            if @y.fixedDomain[1]? then @y.prefix.scale(@y.fixedDomain[1]) else @y.domain()[1]]
        
        refreshYAxis.call(@, dt)
    
    # Update the drawing
    $parent = $(@svg.node().parentElement)
    if $parent.length == 0
      return
    else if @width != $parent.width() or @height != $parent.height()
      @resize()
    else
      refreshXAxis.call(@)
      updateDataPaths.call(@)
      
      if dt > 100 and dt < 1000 # Use smooth scrolling when we're getting frequent updates
        @svg.selectAll(".data path,.x.axis .tick")
            .attr("transform", "translate(#{@x(x2) - @x(x1)},0)")
          .interrupt()
          .transition()
            .duration(dt)
            .ease("linear")
            .attr("transform", "translate(0,0)")
  
  resize: ->
    $parent = $(@svg.node().parentElement)
    return if $parent.length == 0
    
    @width = $parent.width()
    @height = $parent.height()
    
    # Re-calculate the width and height of the data area
    dataWidth = Math.max(@width - (@padding.left + @padding.right), 0)
    dataHeight = Math.max(@height - (@padding.top + @padding.bottom), 0)
    
    # Update the ranges of our scaling functions
    @x.range([0, dataWidth])
    @y.range([dataHeight, 0])
    
    @xaxis.tickSize(dataHeight, 0) # Update the height of the x-axis grid lines
    @yaxis.ticks((dataHeight / 39) | 0) # Update the maximum ticks that will fit on the y-axis
    if @y.fixedDomain[0]? and @y.fixedDomain[1]? and (@y.fixedDomain[1] - @y.fixedDomain[0]) % 90 == 0
      @yaxis.tickValues(angleTicks(@y.fixedDomain, @yaxis.ticks()...))
    
    # Update width and height of the root SVG element
    @svg.attr("width", @width)
      .attr("height", @height)
      
    # Update the data area clipping rectangle
    @svg.select("defs rect")
      .attr("width", dataWidth)
      .attr("height", dataHeight + @padding.top)
    
    # Update the width of the grid lines
    @svg.select("g.y.grid").selectAll("line")
      .attr("x2", dataWidth)
    
    # Update the x-axis domain path
    @svg.select("g.x.axis path.domain")
      .attr("d", "M0,#{dataHeight}H#{dataWidth}")
      
    # Re-center the y-axis label
    @svg.select("g.y.axis text").attr("x", -dataHeight / 2)
    
    # Re-center the legend (if it exists)
    @svg.select("text.legend")
      .attr("transform", "translate(#{dataWidth / 2},#{dataHeight + 20})")
    
    refreshXAxis.call(@)
    refreshYAxis.call(@)
    updateDataPaths.call(@)
  
  # Private methods
  refreshXAxis = ->
    ticks = @svg.select("g.x.axis .ticks").selectAll("g.tick").data(@xaxis.tickValues())
    ticks.select("line")
      .attr("x1", @x)
      .attr("y1", 0)
      .attr("x2", @x)
      .attr("y2", @xaxis.tickSize())
    ticks.select("text") 
      .attr("x", @x)
      .attr("y", @xaxis.tickSize())
      .text(@xaxis.tickFormat())
      
    tick = ticks.enter().append("svg:g").attr("class", "tick")
    tick.append("svg:line")
      .attr("x1", @x)
      .attr("y1", 0)
      .attr("x2", @x)
      .attr("y2", @xaxis.tickSize())
    tick.append("svg:text")
      .attr("x", @x)
      .attr("y", @xaxis.tickSize())
      .attr("dx", "0.5em")
      .attr("dy", "-1ex")
      .attr("text-anchor", "start")
      .text(@xaxis.tickFormat())
    
    ticks.exit().remove()

  refreshYAxis = (duration) ->
    # Update the grid lines
    grid = @svg.select("g.y.grid").selectAll("line")
      .data(@yaxis.tickValues() ? @y.ticks(@yaxis.ticks()...))
    grid.classed("zero", (d) -> d == 0)
    (if duration? then grid.transition().duration(duration) else grid)
      .attr("y1", (d) => @y(d))
      .attr("y2", (d) => @y(d))
      
    grid.enter().append("svg:line")
      .attr("x1", 0)
      .attr("y1", (d) => @y(d))
      .attr("x2", @width)
      .attr("y2", (d) => @y(d))
      .classed("zero", (d) -> d == 0)
    grid.exit().remove()
    
    # Update the Y-axis
    if duration?
      @svg.select("g.y.axis").transition().duration(duration).call(@yaxis)
    else
      @svg.select("g.y.axis").call(@yaxis)
  
  updateDataPaths = ->
    @svg.selectAll("g.data path").data(@series)
      .attr "d", (d, i) =>
        path = ""
        for d, j in @data when d[i+1]
          path += if path.length > 0 and @data[j-1]?[i+1]? then "L" else "M"
          path += "#{@x(d[0])},#{@y(@y.prefix.scale(d[i+1]))}"
        path
  
  angleTicks = (domain, maxTicks) ->
    span = Math.abs(domain[1] - domain[0])
    for i in [15, 30, 45, 90, 180, 360]
      return (tick for tick in [domain[0]..domain[1]] by i) if span / i <= maxTicks
    return domain
    
$(document).ready ->
  # TODO:
  #  Custom charts (auto-save)
  #  Custom layout import/export
  
  # Load custom charts and layouts
  if window.localStorage?
    customCharts = JSON.parse(window.localStorage.getItem("telemachus.console.charts")) ? {}
    $.extend(charts, standardCharts, customCharts)
    customLayouts = JSON.parse(window.localStorage.getItem("telemachus.console.layouts")) ? {}
    $.extend(layouts, standardLayouts, customLayouts)
    savedDefault = window.localStorage.getItem("defaultLayout")
    defaultLayout = savedDefault if savedDefault? and savedDefault of layouts
  
  # Detect test mode and load test charts and layouts
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
  
  # Event handlers
  $(document).on "click.dropdown", ".dropdown button", ->
    $this = $(this)
    console.log $this.width(), $this.height(), $this.outerWidth(true), $this.outerHeight(true)
    $menu = $this.next()
    $menu.toggle().css
      left: Math.max($this.position().left + $this.outerWidth(true) - $menu.outerWidth() + 5, 0)
      top: $this.position().top + Math.min($(window).height() - $menu.outerHeight() - $this.offset().top, $this.outerHeight(true))
  
  $(document).on "click.dropdown", (event) ->
    $(".dropdown").not($(event.target).parents()).children("ul").hide()
  
  $("#apiCategory").change (event) ->
    category = $("#apiCategory").val()
    $("#apiSelect").empty()
    for apistring, api of Telemachus.api when apistring.match(category)
      $("#apiSelect").append($("<option>").attr("value", apistring).text(api.name))
  
  $("#telemetry form").submit (event) ->
    event.preventDefault()
    addTelemetry($("#apiSelect").val())
  
  $("#telemetry ul").on "click", "button.remove", (event) ->
    event.preventDefault()
    removeTelemetry($(this).parent())
  
  $("#telemetry ul").sortable({ handle: ".handle", containment: "#telemetry" })
  
  $(".alert").on "telemetryAlert", (event, message) ->
    $(".alert").text(message ? "")
    if message?
      $this = $(this)
      $display = $this.siblings(".display")
      $this.css("marginTop", -($display.outerHeight() + $this.height()) / 2)
  
  # Custom layout event handlers
  if window.localStorage?
    $("#saveLayout").click (event) ->
      event.preventDefault()
      name = window.prompt("What name would you like to save this layout under?", $("h1").text().trim()).trim()
      return if !name? or name == "" or (name of layouts and !window.confirm("That name is already in use. Are you sure you want to overwrite the existing layout?"))
      layouts[name] = customLayouts[name] =
        charts: ($(elem).text().trim() for elem in $(".chart h2"))
        telemetry: ($(elem).data("api") for elem in $("#telemetry li"))
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

  $(window).resize ->
    winHeight = Math.min($(window).height(), window.innerHeight ? 1e6)
    $("#container").height(winHeight - ($("#container").position().top + $("body > footer").outerHeight(true)))
  
    for display in $(".display", "#charts")
      $display= $(display)
      $chart = $display.closest(".chart")
      $display.height($chart.height() - $display.position().top - ($display.outerHeight() - $display.height()))
      $alert = $display.siblings(".alert")
      $alert.css("fontSize", $display.height() / 5).css("marginTop", -($display.outerHeight() + $alert.height()) / 2)
  
    $telemetry = $("#telemetry")
    $telemetryList = $("ul", $telemetry)
    $telemetryForm = $("form", $telemetry)
    margins = $telemetryList.outerHeight(true) - $telemetryList.height()
    $telemetryList.height($telemetryForm.position().top - $telemetryList.position().top - margins)
  
    $("form", $telemetry).width($telemetry.width())
    $telemetrySelect = $("#apiSelect")
    $telemetryAdd = $("form input", $telemetry)
    buttonWidth = $telemetryAdd.outerWidth(true) + 5 # 5px for whitespace between controls
    $telemetrySelect.width($telemetry.width() - buttonWidth - ($telemetrySelect.outerWidth() - $telemetrySelect.width()))
  .resize()
  
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
  
  setLayout(defaultLayout)
  $("#deleteLayout").prop("disabled", defaultLayout not of customLayouts)

addTelemetry = (api) ->
  if api? and api of Telemachus.api and $("#telemetry li[data-api='#{api}']").length == 0
    $li = $("<li>").data("api", api)
      .append($("<h3>").text(Telemachus.api[api].name))
      .append($("<button>").attr(class: "remove"))
      .append($("<img>").attr(class: "handle", src: "draghandle.png", alt: "Drag to reorder"))
      .appendTo("#telemetry ul")
    $data = $("<div>").attr(class: "telemetry-data").text("No Data").appendTo($li)
    $li.on "telemetry", (event, data) ->
        value = data[api]
        $data.text(Telemachus.format(value, api))
    Telemachus.subscribe($li, api)
    $("#telemetry ul").sortable("refresh").disableSelection()

removeTelemetry = (elem) ->
  $elem = $(elem)
  Telemachus.unsubscribe($elem)
  $elem.remove()
  $("#telemetry ul").sortable("refresh")
  
resetChart = (elem) ->
  $display = $(".display", elem).empty()
  Telemachus.unsubscribe($display)
  $display.data('chart')?.destroy()
  $display.data('chart', null)

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
    apis = chart.series
    
    # Convert chart definition to a Chart
    chart = new Chart($display[0], (Telemachus.api[e].name for e in apis when e of Telemachus.api), chart.yaxis)
    
    $display.data('chart', chart).on "telemetry", (event, data) ->
      t = data["t.universalTime"]
      missionTime = data["v.missionTime"]
      
      chart.missionTimeOffset = (t - missionTime if missionTime > 0)
      chart.x.domain([t - 300, t])
      lastT = Math.min(chart.data[chart.data.length - 1]?[0] ? t, t)
      chart.xaxis.tickValues(x for x in [(t - missionTime % 60)...(t - 360 - (t - lastT))] by -60 when missionTime > 0 and x >= (t - missionTime))
      
      sample = (data[e] for e in apis)
      sample[i] = e[1][0] for e, i in sample when $.isArray(e)
      chart.addSample(t, sample)

reloadLayout = -> setLayout($("h1").text().trim())

setLayout = (name) ->
  if name of layouts
    window.localStorage.setItem("defaultLayout", name)
    $("h1").text(name)
    layout = layouts[name]
    removeTelemetry(elem) for elem in $("#telemetry ul li")
    addTelemetry(telemetry) for telemetry in layout.telemetry
    setChart(elem, layout.charts[i]) for elem, i in $(".chart")

# Utility functions
orderOfMagnitude = (v) ->
  return 0 if +v == 0
  1 + Math.floor(1e-12 + Math.log(Math.abs(+v)) / Math.LN10)

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

hourMinSec = (t = 0) ->
  hour = (t / 3600) | 0
  hour = "0#{hour}" if hour < 10
  t %= 3600
  min = (t / 60) | 0
  min = "0#{min}" if min < 10
  sec = (t % 60 | 0).toFixed()
  sec = "0#{sec}" if sec < 10
  "#{hour}:#{min}:#{sec}"
  
dateString = (t = 0) ->
  year = ((t / (365 * 24 * 3600)) | 0) + 1
  t %= (365 * 24 * 3600)
  day = ((t / (24 * 3600)) | 0) + 1
  t %= (24 * 3600)
  "Year #{year}, Day #{day}, #{hourMinSec(t)} UT"

missionTimeString = (t = 0) ->
  result = "T+"
  if t >= 365 * 24 * 3600
    result += (t / (365 * 24 * 3600) | 0) + ":"
    t %= 365 * 24 * 3600
    result += "0:" if t < 24 * 3600
  result += (t / (24 * 3600) | 0) + ":" if t >= 24 * 3600
  t %= 24 * 3600
  result + hourMinSec(t) + " MET"

durationString = (t = 0) ->
  result = if t < 0 then "-" else ""
  t = Math.abs(t)
  if t >= 365 * 24 * 3600
    result += (t / (365 * 24 * 3600) | 0) + " years "
    t %= 365 * 24 * 3600
    result += "0 days " if t < 24 * 3600
  result += (t / (24 * 3600) | 0) + " days " if t >= 24 * 3600
  t %= 24 * 3600
  result + hourMinSec(t)
