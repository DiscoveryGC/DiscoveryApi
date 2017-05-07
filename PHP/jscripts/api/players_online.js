$(document).ready(function(){
    var table_tag = document.getElementById("player_table");
    var timestamp_tag = document.getElementById("player_timestamp");
    var count_tag = document.getElementById("player_count");
    
    if (json_data)
    {
      //http://i.imgur.com/nqvSjOB.jpg
      var timestamp_date = new Date(json_data.Timestamp);
      timestamp_tag.innerText = timestamp_date.toLocaleString();
      count_tag.innerText = json_data.Players.length;
      
      for(var i = 0; i < json_data.Players.length; i++)
      {
          var item = json_data.Players[i];
      
          var row_e = document.createElement("tr");
          row_e.classList.add("api_player_row");
          
          var name_e = document.createElement("td");
          name_e.innerHTML = item.Name;
          row_e.appendChild(name_e);
          
          var system_e = document.createElement("td");
          system_e.innerHTML = item.System;
          row_e.appendChild(system_e);
          
          var region_e = document.createElement("td");
          region_e.innerHTML = item.Region;
          row_e.appendChild(region_e);
          
          var ping_e = document.createElement("td");
          ping_e.innerHTML = item.Ping;
          row_e.appendChild(ping_e);
          
          var time_e = document.createElement("td");
          time_e.innerHTML = item.Time;
          row_e.appendChild(time_e);
          
          table_tag.appendChild(row_e);          
      }
    }    
});