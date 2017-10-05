$(document).ready(function(){
    var table_tag = document.getElementById("api-body");
    var tag_tag = document.getElementById("factiontag");
    var timestamp_tag = document.getElementById("player_timestamp");
    
    if (json_data)
    {
      //http://i.imgur.com/nqvSjOB.jpg
      var timestamp_date = new Date(json_data.Timestamp);
      timestamp_tag.innerText = timestamp_date.toLocaleString();
      tag_tag.innerText = new URL(location).searchParams.get("tag");
      
      for(var i = 0; i < Object.keys(json_data.Characters).length; i++)
      {
          var name = Object.keys(json_data.Characters)[i];
          var item = json_data.Characters[name];
      
          var row_e = document.createElement("tr");
          row_e.classList.add("api-row");
          
          var name_e = document.createElement("td");
          name_e.innerText = name;
          row_e.appendChild(name_e);
          
          var current_e = document.createElement("td");
          current_e.innerText = item.Current_Time;
          row_e.appendChild(current_e);
          
          var last_e = document.createElement("td");
          last_e.innerText = item.Last_Time;
          row_e.appendChild(last_e);
                    
          table_tag.appendChild(row_e);          
      }
    }    

   var tcats = document.querySelectorAll(".api-headers .tcat");
    var nameSortTrigger = tcats[nameColNum];
    var currSortTrigger = tcats[currColNum];
    var lastSortTrigger = tcats[lastColNum];
    
    nameSortTrigger.addEventListener("click", function(e) {
    	sortTable("name", currentDir);
    });
    
    currSortTrigger.addEventListener("click", function(e) {
    	sortTable("curr", currentDir);
    });
    
    lastSortTrigger.addEventListener("click", function(e) {
    	sortTable("last", currentDir);
    });
    
    //Perform the initial sorting by Current Activity
    sortTable("curr", currentDir);
    sortTable("curr", currentDir);
});

/* separate asc/desc sort buttons may be implemented by calling sortTable(sortType, direction) and uncommenting the handler code block */
var rowSelector = ".api-row";

var nameColNum = 0;
var currColNum = 1;
var lastColNum = 2;



var currentDir = "descending";
var prevSort = "";

function sortTable(currentSort, dir) {
	var rowArray = Array.from(document.querySelectorAll(rowSelector));
	var sortedRowArray = rowArray.sort(function(a, b) {
		if (currentSort == "last") {
      var aVal = parseInt(getTimeInt(a.children[lastColNum].innerText));
			var bVal = parseInt(getTimeInt(b.children[lastColNum].innerText));
			if (aVal > bVal) return 1;
			if (aVal < bVal) return -1;
			return 0;
		} else if (currentSort == "curr") {
			var aVal = parseInt(getTimeInt(a.children[currColNum].innerText));
			var bVal = parseInt(getTimeInt(b.children[currColNum].innerText));
			if (aVal > bVal) return 1;
			if (aVal < bVal) return -1;
			return 0;
		} else if (currentSort == "name") {
			var aVal = a.children[nameColNum].innerText.toLowerCase();
			var bVal = b.children[nameColNum].innerText.toLowerCase();
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
  var seconds = 0;
  var days = 0;
  
  var hms = timeString;
  
  if (timeString.indexOf("d") != -1) 
  {    
		try 
    {
			days = parseInt(timeString.match(/([0-9]+)d/)[1]);
      hms = timeString.substring(timeString.indexOf("d")+1);     
		} catch (e) 
    {
			days = 0;
		}
	}
    
  var a = hms.split(':');
  try
  {
    seconds = (+a[0]) * 60 * 60 + (+a[1]) * 60 + (+a[2]); 
  } catch (e)
  {
    seconds = 0;
  }
  
	return (((days * 24) * 60) * 60) + seconds;
}
