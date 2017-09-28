$(document).ready(function(){
    var table_tag = document.getElementById("api-body");
    var timestamp_tag = document.getElementById("player_timestamp");
    var count_tag = document.getElementById("player_count");
    
    if (json_data)
    {
      //http://i.imgur.com/nqvSjOB.jpg
      var timestamp_date = new Date(json_data.Timestamp);
      timestamp_tag.innerText = timestamp_date.toLocaleString();
      count_tag.innerText = json_data.Players.length;
      
      for(var i = 0; i < json_data.Players.length; i++)
      {
          var item = json_data.Players[i];
      
          var row_e = document.createElement("tr");
          row_e.classList.add("api_player_row");
          
          var name_e = document.createElement("td");
          name_e.innerText = item.Name;
          row_e.appendChild(name_e);
          
          var system_e = document.createElement("td");
          system_e.innerText = item.System;
          row_e.appendChild(system_e);
          
          var region_e = document.createElement("td");
          region_e.innerText = item.Region;
          row_e.appendChild(region_e);
          
          var ping_e = document.createElement("td");
          ping_e.innerText = item.Ping;
          row_e.appendChild(ping_e);
          
          var time_e = document.createElement("td");
          time_e.innerText = item.Time;
          row_e.appendChild(time_e);
          
          table_tag.appendChild(row_e);          
      }
    }    
    
    var tcats = document.querySelectorAll(".api-headers .tcat");
    var nameSortTrigger = tcats[nameColNum];
    var systemSortTrigger = tcats[sysColNum];
    var regionSortTrigger = tcats[regionColNum];
    var pingSortTrigger = tcats[pingColNum];
    var timeSortTrigger = tcats[timeColNum];
    
    nameSortTrigger.addEventListener("click", function(e) {
    	sortTable("name", currentDir);
    });
    
    systemSortTrigger.addEventListener("click", function(e) {
    	sortTable("system", currentDir);
    });
    
    regionSortTrigger.addEventListener("click", function(e) {
    	sortTable("region", currentDir);
    });
    
    pingSortTrigger.addEventListener("click", function(e) {
    	sortTable("ping", currentDir);
    });
    
    timeSortTrigger.addEventListener("click", function(e) {
    	sortTable("time", currentDir);
    });    
});

/* separate asc/desc sort buttons may be implemented by calling sortTable(sortType, direction) and uncommenting the handler code block */
var rowSelector = ".api-row";

var nameColNum = 0;
var sysColNum = 1;
var regionColNum = 2;
var pingColNum = 3;
var timeColNum = 4;



var currentDir = "descending";
var prevSort = "";

function sortTable(currentSort, dir) {
	var rowArray = Array.from(document.querySelectorAll(rowSelector));
	var sortedRowArray = rowArray.sort(function(a, b) {
		if (currentSort == "ping") {
			var aVal = parseInt(a.children[pingColNum].innerText);
			var bVal = parseInt(b.children[pingColNum].innerText);
			if (aVal > bVal) return 1;
			if (aVal < bVal) return -1;
			return 0;
		} else if (currentSort == "time") {
			var aVal = parseInt(getTimeInt(a.children[timeColNum].innerText));
			var bVal = parseInt(getTimeInt(b.children[timeColNum].innerText));
			if (aVal > bVal) return 1;
			if (aVal < bVal) return -1;
			return 0;
		} else if (currentSort == "name") {
			var aVal = a.children[nameColNum].innerText.toLowerCase();
			var bVal = b.children[nameColNum].innerText.toLowerCase();
			if (aVal > bVal) return 1;
			if (aVal < bVal) return -1;
			return 0;
		} else if (currentSort == "system") {
			var aVal = a.children[sysColNum].innerText.toLowerCase();
			var bVal = b.children[sysColNum].innerText.toLowerCase();
			if (aVal > bVal) return 1;
			if (aVal < bVal) return -1;
			return 0;
		} else if (currentSort == "region") {
			var aVal = a.children[regionColNum].innerText.toLowerCase();
			var bVal = b.children[regionColNum].innerText.toLowerCase();
			if (aVal > bVal) return 1;
			if (aVal < bVal) return -1;
			return 0;
		}
	});

	/* sort direction handling for one-button use; remove if using separate asc/desc sort buttons */
	if (prevSort == currentSort) {
		if (currentDir == "descending") {
			sortedRowArray.reverse();
			currentDir = "ascending";
		} else {
			currentDir = "descending";
		}
	} else {
		currentDir = "descending";
	}
	prevSort = currentSort;
	/* end sort direction handler */

	sortedRowArray.forEach(function(row) {
		document.querySelector(".api-body").insertBefore(row, null);
	});
}

function getTimeInt(timeString) {
	var minutes = 0;
	var hours = 0;
	try {
		minutes = parseInt(timeString.match(/([0-9]+)(m|\b)/)[1]);
	} catch (e) {
		minutes = 0;
	}
	if (timeString.indexOf("h") != -1) {
		try {
			hours = parseInt(timeString.match(/([0-9]+)h/)[1]);
		} catch (e) {
			hours = 0;
		}
	}
	return hours * 60 + minutes;
}
