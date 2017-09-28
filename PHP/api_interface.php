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
    add_breadcrumb("Faction Activity Summary", THIS_SCRIPT . "?action=faction_summary");
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
else
{
    eval("\$disco_body = \"Uhhhhh\";");
    eval("\$page = \"".$templates->get("disco")."\";"); 
    output_page($page);
}


?>