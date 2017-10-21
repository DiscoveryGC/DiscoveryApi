/*global $, json_data*/
"use strict";

$(function () {
    var rowSelector = ".api-row",
        nameColNum = 0,
        tagColNum = 1,
        currColNum = 2;
    // lastColNum = 3;

    if (json_data) {
        //http://i.imgur.com/nqvSjOB.jpg
        $('#player_timestamp').text(new Date(json_data.Timestamp).toLocaleString());
        $.prototype.append.apply($('#api-body'), $.map(json_data.Factions, function (item) {
            return $('<tr>')
                .addClass('api-row')
                .append(
                    $('<td>').append(
                        $('<a>')
                            .attr( 'href', "api_interface.php?action=faction_details&tag=" + encodeURIComponent(item.Tag))
                            .text(item.Name)
                    ),
                    $('<td>').text(item.Tag),
                    $('<td>').text(item.Current_Time),
                    $('<td>').text(item.Last_Time)
                );
        }));
    }

    $('.api-headers .tcat').each(function (index, element) {
        element.addEventListener("click", function () {
            var parser = parseDHMS;
            if (index === nameColNum || index === tagColNum) {
                parser = function (cell) { return cell.toLowerCase(); };
            }
            sortTable(index, parser, rowSelector);
        });
    });

    //Perform the initial sorting by Current Activity
    sortTable(currColNum, parseDHMS, rowSelector);
    sortTable(currColNum, parseDHMS, rowSelector);
});