<?php

// Set some useful constants that the core may require or use
define("IN_MYBB", 1);
define('THIS_SCRIPT', 'api_interface.php');
error_reporting(E_ALL);

// Including global.php gives us access to a bunch of MyBB functions and variables
include('./global.php');
$api_key = "oflsareinsanepeople";
$api_location = "http://localhost:5000/";


// Add a breadcrumb
add_breadcrumb('API', THIS_SCRIPT);

if ($mybb->input['action'] == "players_online") {
    add_breadcrumb("Players Online", THIS_SCRIPT . "?action=players_online");
    if(!$mybb->user['uid'] || $mybb->user['uid'] < 1 )
    {
      error_no_permission();
    }
    
    $curl = curl_init();
    curl_setopt($curl, CURLOPT_URL, $api_location . "api/Online/GetPlayers/" . $api_key);
    curl_setopt($curl, CURLOPT_RETURNTRANSFER, true);
    $json_data = curl_exec($curl);
    curl_close($curl);
    
    //var_dump($json_data);
    if ($json_data != false)
     {
        eval("\$disco_body = \"".$templates->get("api_playersonline")."\";"); 
     }
     else
     {
        eval("\$disco_body = \"API Unavailable.\";");
     }
       
    eval("\$page = \"".$templates->get("disco")."\";"); 
    output_page($page);
}
else if ($mybb->input['action'] == "faction_summary") {
    add_breadcrumb("Faction Activity", THIS_SCRIPT . "?action=faction_summary");
    if(!$mybb->user['uid'] || $mybb->user['uid'] < 1 )
    {
      error_no_permission();
    }
    
    $curl = curl_init();
    curl_setopt($curl, CURLOPT_URL, $api_location . "api/Online/GetFactionSummary/" . $api_key);
    curl_setopt($curl, CURLOPT_RETURNTRANSFER, true);
    $json_data = curl_exec($curl);
    curl_close($curl);
    
    //var_dump($json_data);
    if ($json_data != false)
     {
        eval("\$disco_body = \"".$templates->get("api_factionsummary")."\";"); 
     }
     else
     {
        eval("\$disco_body = \"API Unavailable.\";");
     }
       
    eval("\$page = \"".$templates->get("disco")."\";"); 
    output_page($page);
}
else if ($mybb->input['action'] == "players") {
    add_breadcrumb("Individual Activity", THIS_SCRIPT . "?action=players");
    $curl = curl_init();
    $page = $mybb->input['page'];
    if (is_numeric($page)) {
        $page = intval($page);
    }
    if (!is_numeric($page) || $page < 0) {
            $page = 1;
    }
    curl_setopt($curl, CURLOPT_URL, $api_location . "api/Online/GetAllPlayers/" . $api_key . "/" . $page);
    curl_setopt($curl, CURLOPT_RETURNTRANSFER, true);
    $json_data = curl_exec($curl);
    curl_close($curl);

    if ($json_data != false)
    {
        $max_page = json_decode(json_decode($json_data))->MaxPage;
        $disco_body = 'Page ' . $page . ' of ' . $max_page . '.';
        if ($page > 1) {
            $disco_body .= ' <a href="https://discoverygc.com/forums/api_interface.php?action=players&page=' . ($page - 1) . '">Previous</a>';
        }
        if ($page < $max_page) {
            $disco_body .= ' <a href="https://discoverygc.com/forums/api_interface.php?action=players&page=' . ($page + 1) . '">Next</a>';
        }
        eval("\$disco_body .= \"".$templates->get("api_all_details")."\";");
    }
    else
    {
        eval("\$disco_body = \"API Unavailable.\";");
    }
    eval("\$page = \"".$templates->get("disco")."\";"); 
    output_page($page);
}
else if ($mybb->input['action'] == "faction_details") {
    add_breadcrumb("Faction Activity", THIS_SCRIPT . "?action=faction_summary");
    if(!$mybb->user['uid'] || $mybb->user['uid'] < 1 )
    {
        error_no_permission();
    }
    add_breadcrumb(htmlspecialchars($mybb->input['tag']), THIS_SCRIPT . "?action=faction_details&tag=" . urlencode($mybb->input['tag']));
    if (isset($mybb->input['tag'])) {
        $curl = curl_init();
        curl_setopt($curl, CURLOPT_URL, $api_location . "api/Online/GetFactionDetails/" . urlencode($mybb->input['tag']) . "/" . $api_key);
        curl_setopt($curl, CURLOPT_RETURNTRANSFER, true);
        $json_data = curl_exec($curl);
        curl_close($curl);

        if ($json_data != false)
        {
            eval("\$disco_body = \"".$templates->get("api_faction_details")."\";");
        }
        else
        {
            eval("\$disco_body = \"API Unavailable.\";");
        }
    } else {
        eval("\$disco_body = '<iframe width=\"560\" height=\"315\" src=\"https://www.youtube.com/embed/SAyHwbSuS3Y\" frameborder=\"0\" allowfullscreen></iframe>';");
    }


    eval("\$page = \"".$templates->get("disco")."\";"); 
    output_page($page);
}
else
{
    eval("\$disco_body = \"Uhhhhh\";");
    eval("\$page = \"".$templates->get("disco")."\";"); 
    output_page($page);
}


?>
