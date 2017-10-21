/*global location, URL, $, json_data*/
"use strict";

$(function () {
    var rowSelector = ".api-row",
        nameColNum = 0,
        currColNum = 1;
    // lastColNum = 2;

    if (json_data) {
        //http://i.imgur.com/nqvSjOB.jpg
        $('#player_timestamp').text(new Date(json_data.Timestamp).toLocaleString());
        $('#factiontag').text(new URL(location).searchParams.get("tag"));
        $.prototype.append.apply($('#api-body'), $.map(json_data.Characters, function (item, name) {
            return $('<tr>')
                .addClass('api-row')
                .append(
                    $('<td>').text(name),
                    $('<td>').text(item.Current_Time),
                    $('<td>').text(item.Last_Time)
                );
        }));
    }

    $('.api-headers .tcat').each(function (index, element) {
        element.addEventListener("click", function () {
            var parser = parseDHMS;
            if (index == nameColNum) {
                parser = function (cell) { return cell.toLowerCase(); };
            }
            sortTable(index, parser, rowSelector);
        });
    });

    // Perform the initial sorting by Current Activity
    sortTable(currColNum, parseDHMS, rowSelector);
    sortTable(currColNum, parseDHMS, rowSelector);
});