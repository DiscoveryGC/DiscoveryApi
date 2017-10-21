/*global document*/
"use strict";

var currentDir = "descending",
    prevSort = "";

function parseDHMS(timeString) {
    var seconds = 0,
        days = 0,
        hms = timeString,
        a;

    if (timeString === 'null') {
        return 0;
    }
    if (timeString.indexOf("d") !== -1) {
        try {
            days = parseInt(timeString.match(/([0-9]+)d/)[1], 10);
            hms = timeString.substring(timeString.indexOf("d") + 1);
        } catch (e) {
            days = 0;
        }
    }

    a = hms.split(":");
    try {
        seconds = (+a[0]) * 60 * 60 + (+a[1]) * 60 + (+a[2]);
    } catch (e) {
        seconds = 0;
    }

    return (((days * 24) * 60) * 60) + seconds;
}

/* separate asc/desc sort buttons may be implemented by calling sortTable(sortType, direction) and uncommenting the handler code block */
function sortTable(currentSort, parseCell, rowSelector) {
    var rowArray = Array.from(document.querySelectorAll(rowSelector)),
        sortedRowArray = rowArray.sort(function (a, b) {
            var aVal, bVal;
            aVal = parseCell(a.children[currentSort].innerText);
            bVal = parseCell(b.children[currentSort].innerText);
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
        if (currentDir === "descending") {
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

    sortedRowArray.forEach(function (row) {
        document.querySelector(".api-body").insertBefore(row, null);
    });
}
