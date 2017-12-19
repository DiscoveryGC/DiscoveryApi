/*global location, URL, $, json_data */
"use strict";

function parseHM(timeString) {
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

$(function () {
    var pingColNum = 3,
        timeColNum = 4,
        ipColNum = 5,
        is_admin = new URL(location).searchParams.get("action") === "players_online_admin";
    /*  nameColNum = 0,
        sysColNum = 1,
        regionColNum = 2,
        idColNum = 3,
        shipColNum = 4*/
    if (json_data) {
        //http://i.imgur.com/nqvSjOB.jpg
        $('#player_timestamp').text(new Date(json_data.Timestamp).toLocaleString());
        $('#player_count').text(json_data.Players.length);
        var seenIPs = {};
        $.prototype.append.apply($('#api-body'), $.map(json_data.Players, function (item) {
            var $row = $('<tr>')
                .addClass('api_player_row')
                .append(
                    $('<td>').text(item.Name),
                    $('<td>').text(item.System),
                    $('<td>').text(item.Region)
                );
            if (is_admin) {
                if (!(item.IP in seenIPs)) {
                    seenIPs[item.IP] = [];
                }
                seenIPs[item.IP].push($row);
                $row.append(
                    $('<td>').text(item.IDName).data('nickname', item.ID),
                    $('<td>').text(item.ShipName).data('nickname', item.Ship),
                    $('<td>').append(
                        $('<a>')
                            .attr('href', "modcp.php?action=ipsearch&search_users=1&search_posts=1&ipaddress=" + item.IP)
                            .text(item.IP)
                    )
                );
            } else {
                $row.append(
                    $('<td>').text(item.Ping),
                    $('<td>').text(item.Time)
                );
            }

            return $row;
        }));
        if (is_admin) {
            $.each(seenIPs, function (ip, rows) {
                if (rows.length > 1) {
                    $.each(rows, function () {
                        $(this).find(':nth-child(6)').css('background-color', 'red');
                    });
                }
            });
        }
    }

    $(".api-headers .tcat").each(function (index, element) {
        element.addEventListener("click", function () {
            var parser = function (cell) { return cell.toLowerCase(); };
            if (index === pingColNum && !is_admin) {
                parser = function (cell) { return parseInt(cell, 10); };
            } else if (index === timeColNum && !is_admin) {
                parser = parseHM;
            } else if (index === ipColNum && is_admin) {
                parser = function (cell) {
                    var parts = cell.split('.').map(function (e, i) { return parseInt(e, 10) * (2 ** 8) ** (3 - i); });
                    return parts[0] + parts[1] + parts[2] + parts[3];
                }
            }
            sortTable(index, parser, ".api_player_row");
        });
    });
});
