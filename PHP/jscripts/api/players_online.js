/*global document, location, URL, $, json_data */
"use strict";

/* separate asc/desc sort buttons may be implemented by calling sortTable(sortType, direction) and uncommenting the handler code block */
var /*nameColNum = 0,
    sysColNum = 1,
    regionColNum = 2,*/
    pingColNum = 3,
    timeColNum = 4,
    /*idColNum = 3,
    shipColNum = 4,
    ipColNum = 5,*/
    currentDir = "descending",
    prevSort = "",
    is_admin = new URL(location).searchParams.get("action") === "players_online_admin";

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
    var rowArray = Array.from(document.querySelectorAll(".api_player_row")),
        sortedRowArray = rowArray.sort(function (a, b) {
            var aVal, bVal;
            if (currentSort === pingColNum && !is_admin) {
                aVal = parseInt(a.children[currentSort].innerText, 10);
                bVal = parseInt(b.children[currentSort].innerText, 10);
            } else if (currentSort === timeColNum && !is_admin) {
                aVal = parseInt(getTimeInt(a.children[currentSort].innerText), 10);
                bVal = parseInt(getTimeInt(b.children[currentSort].innerText), 10);
            } else {
                aVal = a.children[currentSort].innerText.toLowerCase();
                bVal = b.children[currentSort].innerText.toLowerCase();
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
        i,
        timestamp_date,
        item,
        row_e,
        name_e,
        system_e,
        region_e,
        ping_e,
        time_e,
        id_e,
        ship_e,
        ip_e,
        ip_a_e;

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

            if (is_admin) {
                id_e = document.createElement("td");
                id_e.innerText = item.ID;
                row_e.appendChild(id_e);

                ship_e = document.createElement("td");
                ship_e.innerText = item.Ship;
                row_e.appendChild(ship_e);

                ip_e = document.createElement("td");
                ip_a_e = document.createElement("a");
                ip_a_e.href = "https://discoverygc.com/forums/modcp.php?action=ipsearch&search_users=1&search_posts=1&ipaddress=" + item.IP;
                ip_a_e.innerText = item.IP;
                ip_e.appendChild(ip_a_e);
                row_e.appendChild(ip_e);
            } else {
                ping_e = document.createElement("td");
                ping_e.innerText = item.Ping;
                row_e.appendChild(ping_e);

                time_e = document.createElement("td");
                time_e.innerText = item.Time;
                row_e.appendChild(time_e);
            }

            table_tag.appendChild(row_e);
        }
    }

    $(".api-headers .tcat").each(function (index, element) {
        element.addEventListener("click", function () {
            sortTable(index, currentDir);
        });
    });
});