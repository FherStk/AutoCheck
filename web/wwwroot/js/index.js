"use strict";

var disconnect = false;
var newBatch = true;
var firstBatch = true;
var logContent = $("#log > #content");
var connection = new signalR.HubConnectionBuilder().withUrl("/homeHub").withAutomaticReconnect().build();

//setting up server-to-client messages
connection.on("ReceiveLog", function (log, endOfScript, endOfExecution) {        
    if(log != null){        
        log = JSON.parse(log);

        if(newBatch){
            newBatch = false;
            
            //creating new batch container
            logContent.append(
                '<div class="collapsable">\
                    <div class="header' + (firstBatch ? ' hidden' : '') + '">Display more...</div>\
                    <div class="content' + (firstBatch ? '' : ' hidden') + '"></div>\
                </div>'
            );            
            firstBatch = false;
            
            //adding events
            logContent.on("click", "div.collapsable > div.header" , function() {
                $(this).hide();
                $(this).next().show();
            });
        }

        //adding content to the last batch log content
        var lastLog = logContent.find("div.collapsable:last-child > div.content");
        $.each(log, function(i) {                   
            if(this.Text != null && this.Text.startsWith("ERROR:")){
                lastLog.append('<label class="'  + this.Style + '">ERROR:</label><br>');
                this.Text = this.Text.replace("ERROR:", "").replace(/^\n|\n$/g, '');
            }
            
            lastLog.append('<label class="'  + this.Style + '"><xmp>' + (this.Indent == null ? "" : this.Indent) +  (this.Text == null ? "" : this.Text) + '</xmp></label>');                        
            if(this.BreakLine || this.Style == null) lastLog.append('<br>');
        });
    }

    if(endOfScript){
        //current batch can be closed
        newBatch = true;
        logContent.append('<br>');
    }

    if(endOfExecution){
        end();
    }
});

//setting up client-to-server requests (but run, which needs SignalR in order to receive async log update)
$(function(){  
    $("#update-no").click(function(){
        $("#update-card").hide();
    });

    $("#update-yes").click(function(){
        $("#update-card, #step-1, #step-2, #step-3").hide();
        $("#updating").show();

        $.post("/home/PerformUpdate", function(data){
            reload();
        });
    });

    $("#mode").change(function() {
        $("#step-2").hide();
        
        if($("#mode").val() == 'none') $("#script").hide();
        else{
            $.post("/home/GetScripts", { mode: this.value}, function(data){                     
                $("#script").empty();
                $("#script").append('<option value="none" selected>- Select -</option>');

                $.each(data, function() {                        
                    $("#script").append('<option value="'  + this.path + '">[' + this.source.toLowerCase() + '] ' + this.name + '</option>');
                });
                            
                $("#script").show();
            });
        }
    });

    $("#script").change(function() {
        var mode = $("#mode").val();        
        if(mode == "single"){
            $("#info-batch").hide();
            $("#info-single").show();
        }
        else if(mode == "batch"){
            $("#info-single").hide();
            $("#info-batch").show();
        }

        //Displaying target data
        $.post("/home/GetTargetData", {script: $("#script").val()}, function(data){                  
            $("#target").empty();
            
            var rows = 0;
            Object.keys(data).forEach(function(key) {
                if(key == "os") $("#target").append('<tr><td><label for"' + key + '">' + key + ': </label></td><td><select id="' + key + '" name="' + key + '"><option value="GNU"' + (data[key] == "GNU" ? " selected" : "") + '>GNU/Linux</option><option value="MAC"' + (data[key] == "MAC" ? " selected" : "") + '>Mac OS</option><option value="WIN"' + (data[key] == "WIN" ? " selected" : "") + '>Windows</option></select></td><td></td></tr>'); 
                else if(key == "vars"){                    
                    Object.keys(data.vars).forEach(function(key) {
                        addTarget(key, data.vars[key], "Some data", true);
                        rows++;
                    });
                }
                else{
                    var placeholder;
                    switch(key){
                        case "folder":
                        case "path":
                            placeholder="/home/user/Documents/folder";
                            break;
    
                        case "host":
                            placeholder="hostname or IP address";
                            break;
    
                        case "user":
                            placeholder="username";
                            break;
    
                        case "password":
                            placeholder="password";
                            break;                               
                    }
                    
                    addTarget(key, data[key], placeholder, false);
                    rows++;
                };
            });            
                        
            $("#target > tr:not(:first)").each(function() {                        
                $(data).children("td").last().remove();
            });

            var runHolder = $("#target > tr").first().children("td").last();
            runHolder.attr("rowspan", rows);
            runHolder.append('<input id="run" onclick="run()" type="button" value="Run" disabled="true" />');
            
            enable();
            $("#step-2").show();
        });
    });  

    $("#target").keyup(function() {
        enable();
    }); 
    
    $.post("/home/CheckForUpdate", function(data){  
        if(data) $("#update-card").show();
    });
});

//script exection
function run(){    
    $("#step-3").show();  
    $("#log > div").empty();
    $("#log > img").show();

    //Get the data target data to send
    var target = {};
    $("#mode, #script, #run").prop( "disabled", true );  
    $("#target").find("input[type=text],select").not('.var').each(function(){        
        $(this).prop( "disabled", true );  
        target[$(this).attr('name')] = $(this).val();        
    });

    var vars = {};
    $("#target").find("input[type=text][class='var']").each(function(){
        $(this).prop( "disabled", true );  
        vars[$(this).attr('name')] = $(this).val();
    });
        
    disconnect = false;
    newBatch = true;
    firstBatch = true;   
    
    //starting connection
    connection.start().then(function () {
        connection.invoke("Run", $("#script").val(), target, vars).catch(function (err) {
            if(!disconnect) error(err);
        });
    }).catch(function (err) {
        error(err);
    });
}

//aux methods
function enable(){
    var disabled = false;
    $(document).find("input[type=text],select").each(function(){
        if($(this).val() == ""){
            disabled = true;
            return false;
        }             
    });

    $("#run").prop("disabled", disabled); 
}

function addTarget(key, value, placeholder, isVar){
    $("#target").append('<tr><td><label for"' + key + '">' + key + ': </label></td><td><input type="text" id="' + key + '" name="' + key + '" ' + (isVar ? 'class="var"' : '') + ' value="' + value + '" placeholder="' + placeholder + '" /></td><td></td></tr>');
}

function reload(){
    
    $.post("/home/CheckForUpdate", function(data) {
        if(!data){
            //update completed
            window.location.reload(true);
        } 
        else{
            //updated has not started
            setTimeout(function(){
                reload();
            }, 1000);    
        }
    }).fail(function(){ 
        //updated in progress
        setTimeout(function(){
            reload();
        }, 1000);
    });
} 

function error(message){
    logContent.append('<label class="error">' + message + '</label><br>');  
    end();      
}

function end(){
    //end of all the executions, log completed
    $("#log > img").hide();

    $("#mode, #script, #run").prop( "disabled", false );  
    $("#target").find("input[type=text],select").each(function(){        
        $(this).prop( "disabled", false );  
    });

    disconnect = true;
    connection.stop();
}