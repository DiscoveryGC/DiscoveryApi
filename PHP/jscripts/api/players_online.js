/*global document, $, json_data*/
'use strict';

/* separate asc/desc sort buttons may be implemented by calling sortTable(sortType, direction) and uncommenting the handler code block */
var rowSelector = ".api-row",
    nameColNum = 0,
    sysColNum = 1,
    regionColNum = 2,
    pingColNum = 3,
    timeColNum = 4,
    currentDir = "descending",
    prevSort = "";

function getTimeInt(timeString) {
    var minutes = 0,
        hours = 0;
    try {
        minutes = parseInt(timeString.match(/([0-9]+)(m|\b)/)[1], 10);
    } catch (e) {
        minutes = 0;
    }
    if (timeString.indexOf("h") !== -1) {
        try {
            hours = parseInt(timeString.match(/([0-9]+)h/)[1], 10);
        } catch (e) {
            hours = 0;
        }
    }
    return hours * 60 + minutes;
}

function sortTable(currentSort, dir) {
    var rowArray = Array.from(document.querySelectorAll(rowSelector)),
        sortedRowArray = rowArray.sort(function (a, b) {
            var aVal, bVal;
            if (currentSort === "ping") {
                aVal = parseInt(a.children[pingColNum].innerText, 10);
                bVal = parseInt(b.children[pingColNum].innerText, 10);
            } else if (currentSort === "time") {
                aVal = parseInt(getTimeInt(a.children[timeColNum].innerText), 10);
                bVal = parseInt(getTimeInt(b.children[timeColNum].innerText), 10);
            } else if (currentSort === "name") {
                aVal = a.children[nameColNum].innerText.toLowerCase();
                bVal = b.children[nameColNum].innerText.toLowerCase();
            } else if (currentSort === "system") {
                aVal = a.children[sysColNum].innerText.toLowerCase();
                bVal = b.children[sysColNum].innerText.toLowerCase();
            } else if (currentSort === "region") {
                aVal = a.children[regionColNum].innerText.toLowerCase();
                bVal = b.children[regionColNum].innerText.toLowerCase();
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
        timestamp_tag = document.getElementById("player_timestamp"),
        count_tag = document.getElementById("player_count"),
        tcats = document.querySelectorAll(".api-headers .tcat"),
        nameSortTrigger = tcats[nameColNum],
        systemSortTrigger = tcats[sysColNum],
        regionSortTrigger = tcats[regionColNum],
        pingSortTrigger = tcats[pingColNum],
        timeSortTrigger = tcats[timeColNum],
        i,
        timestamp_date,
        item,
        row_e,
        name_e,
        system_e,
        region_e,
        ping_e,
        time_e;

    if (json_data) {
        //http://i.imgur.com/nqvSjOB.jpg
        timestamp_date = new Date(json_data.Timestamp);
        timestamp_tag.innerText = timestamp_date.toLocaleString();
        count_tag.innerText = json_data.Players.length;

        for (i = 0; i < json_data.Players.length; i += 1) {
            item = json_data.Players[i];

            row_e = document.createElement("tr");
            row_e.classList.add("api_player_row");

            name_e = document.createElement("td");
            name_e.innerText = item.Name;
            row_e.appendChild(name_e);

            system_e = document.createElement("td");
            system_e.innerText = item.System;
            row_e.appendChild(system_e);

            region_e = document.createElement("td");
            region_e.innerText = item.Region;
            row_e.appendChild(region_e);

            ping_e = document.createElement("td");
            ping_e.innerText = item.Ping;
            row_e.appendChild(ping_e);

            time_e = document.createElement("td");
            time_e.innerText = item.Time;
            row_e.appendChild(time_e);

            table_tag.appendChild(row_e);
        }
    }

    nameSortTrigger.addEventListener("click", function () {
        sortTable("name", currentDir);
    });

    systemSortTrigger.addEventListener("click", function () {
        sortTable("system", currentDir);
    });

    regionSortTrigger.addEventListener("click", function () {
        sortTable("region", currentDir);
    });

    pingSortTrigger.addEventListener("click", function () {
        sortTable("ping", currentDir);
    });

    timeSortTrigger.addEventListener("click", function () {
        sortTable("time", currentDir);
    });
});