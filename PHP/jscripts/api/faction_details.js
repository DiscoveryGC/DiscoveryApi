/*global document, location, URL, $, json_data*/
'use strict';

var rowSelector = ".api-row",
    currentDir = "descending",
    prevSort = "",
    nameColNum = 0,
    currColNum = 1,
    lastColNum = 2;

function getTimeInt(timeString) {
    var seconds = 0,
        days = 0,
        hms = timeString,
        a;

    if (timeString.indexOf("d") !== -1) {
        try {
            days = parseInt(timeString.match(/([0-9]+)d/)[1], 10);
            hms = timeString.substring(timeString.indexOf("d") + 1);
        } catch (e) {
            days = 0;
        }
    }

    a = hms.split(':');
    try {
        seconds = (+a[0]) * 60 * 60 + (+a[1]) * 60 + (+a[2]);
    } catch (e) {
        seconds = 0;
    }

    return (((days * 24) * 60) * 60) + seconds;
}

/* separate asc/desc sort buttons may be implemented by calling sortTable(sortType, direction) and uncommenting the handler code block */
function sortTable(currentSort, dir) {
    var rowArray = Array.from(document.querySelectorAll(rowSelector)),
        sortedRowArray = rowArray.sort(function (a, b) {
            var aVal, bVal;
            if (currentSort === lastColNum || currentSort === currColNum) {
                aVal = parseInt(getTimeInt(a.children[currentSort].innerText), 10);
                bVal = parseInt(getTimeInt(b.children[currentSort].innerText), 10);
            } else if (currentSort === nameColNum) {
                aVal = a.children[nameColNum].innerText.toLowerCase();
                bVal = b.children[nameColNum].innerText.toLowerCase();
            }

            if (aVal > bVal) {
                return 1;
            }
            if (aVal < bVal) {
                return -1;
            }
            return 0;
        });

    /* sort direction handling for one-button use; remove if using separate asc/desc sort buttons */
    if (prevSort === currentSort) {
        if (dir === "descending") {
            sortedRowArray.reverse();
            dir = "ascending";
        } else {
            dir = "descending";
        }
    } else {
        dir = "descending";
    }
    prevSort = currentSort;
    /* end sort direction handler */

    sortedRowArray.forEach(function (row) {
        document.querySelector(".api-body").insertBefore(row, null);
    });
}

$(document).ready(function () {
    var table_tag = document.getElementById("api-body"),
        tag_tag = document.getElementById("factiontag"),
        timestamp_tag = document.getElementById("player_timestamp"),
        tcats = document.querySelectorAll(".api-headers .tcat"),
        nameSortTrigger = tcats[nameColNum],
        currSortTrigger = tcats[currColNum],
        lastSortTrigger = tcats[lastColNum],
        i,
        timestamp_date,
        name,
        item,
        row_e,
        name_e,
        current_e,
        last_e;

    if (json_data) {
        //http://i.imgur.com/nqvSjOB.jpg
        timestamp_date = new Date(json_data.Timestamp);
        timestamp_tag.innerText = timestamp_date.toLocaleString();
        tag_tag.innerText = new URL(location).searchParams.get("tag");

        for (i = 0; i < Object.keys(json_data.Characters).length; i += 1) {
            name = Object.keys(json_data.Characters)[i];
            item = json_data.Characters[name];

            row_e = document.createElement("tr");
            row_e.classList.add("api-row");

            name_e = document.createElement("td");
            name_e.innerText = name;
            row_e.appendChild(name_e);

            current_e = document.createElement("td");
            current_e.innerText = item.Current_Time;
            row_e.appendChild(current_e);

            last_e = document.createElement("td");
            last_e.innerText = item.Last_Time;
            row_e.appendChild(last_e);

            table_tag.appendChild(row_e);
        }
    }

    nameSortTrigger.addEventListener("click", function () {
        sortTable(nameColNum, currentDir);
    });

    currSortTrigger.addEventListener("click", function () {
        sortTable(currColNum, currentDir);
    });

    lastSortTrigger.addEventListener("click", function () {
        sortTable(lastColNum, currentDir);
    });

    //Perform the initial sorting by Current Activity
    sortTable(currColNum, currentDir);
    sortTable(currColNum, currentDir);
});