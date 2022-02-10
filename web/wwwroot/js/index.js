"use strict";
var connection = new signalR.HubConnectionBuilder().withUrl("/homeHub").build();

// connection.on("ReceiveScripts", function (scripts) {
//     $("#script").empty();
//     $("#script").append('<option value="none" selected>- Select -</option>');

//     $.each(scripts, function() {                        
//         $("#script").append('<option value="'  + this.path + '">[' + this.source.toLowerCase() + '] ' + this.name + '</option>');
//     });
                
//     $("#script").show();
// });

connection.start().then(function () {
    //something    
}).catch(function (err) {
    return console.error(err.toString());
});


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

    var logSelector = "#log > div";
    connection.invoke("Run", $("#script").val(), target, vars).catch(function (err) {
        return console.error(err.toString());
    });

    // $.post("/home/Run", { script: $("#script").val(), target: target, vars: vars}, function(data){                                
    //     //TODO: this must be append in async mode   
    //     $.each(data, function(i) {      
    //         //Multiple log files                                                 
    //         $(logSelector).append(
    //             '<div class="collapsable">\
    //                 <div class="header' + (i==0 ? ' hidden' : '') + '">Display more...</div>\
    //                 <div class="content' + (i>0 ? ' hidden' : '') + '"></div>\
    //             </div>'
    //         );

    //         $(logSelector).on("click", "div.collapsable > div.header" , function() {
    //             $(node).hide();
    //             $(node).next().show();
    //         });

    //         $.each(this, function() {                
    //             var lastLogSelector = logSelector + " > .collapsable:last-child > .content";

    //             //TODO: this should come in two lines
    //             if(this.Text != null && this.Text.startsWith("ERROR:")){
    //                 $(lastLogSelector).append('<label class="'  + this.Style + '">ERROR:</label><br>');
    //                 this.Text = this.Text.replace("ERROR:", "").replace(/^\n|\n$/g, '');
    //             }
                
    //             $(lastLogSelector).append('<label class="'  + this.Style + '"><xmp>' + (this.Indent == null ? "" : this.Indent) +  (this.Text == null ? "" : this.Text) + '</xmp></label>');                        
    //             if(this.BreakLine || this.Style == null) $(lastLogSelector).append('<br>');
    //         });

    //         $(logSelector).append('<br>');
    //     });
    // }).fail(function(data) {
    //     $(logSelector).append('<label class="error">' + data.statusText + ' (' + data.status + '): ' + data.responseText + '</label>');                        
    // }).always(function() {
    //     $("#log > img").hide();

    //     $("#mode, #script, #run").prop( "disabled", false );  
    //     $("#target").find("input[type=text],select").each(function(){        
    //         $(this).prop( "disabled", false );  
    //     });
    // });
}

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
                if(key == "os") $("#target").append('<tr><td><label for"' + key + '">' + key + ': </label></td><td><select id="' + key + '" name="' + key + '"><option value="GNU">GNU/Linux</option><option value="MAC">Mac OS</option><option value="WIN">Windows</option></select></td><td></td></tr>'); 
                else if(key == "vars"){                    
                    Object.keys(data.vars).forEach(function(key) {
                        addTarget(key, "Some data", true);
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

                    addTarget(key, placeholder, false);
                };

                rows++;
            });            
                        
            $("#target > tr:not(:first)").each(function() {                        
                $(data).children("td").last().remove();
            });

            var runHolder = $("#target > tr").first().children("td").last();
            runHolder.attr("rowspan", rows);
            runHolder.append('<input id="run" onclick="run()" type="button" value="Run" disabled="true" />');
            
            $("#step-2").show();
        });
    });  

    $("#target").keyup(function() {
        var disabled = false;
        $(this).find("input[type=text],select").each(function(){
            if($(this).val() == ""){
                disabled = true;
                return false;
            }             
        });

        $("#run").prop("disabled", disabled); 
    }); 
    
    $.post("/home/CheckForUpdate", function(data){  
        if(data) $("#update-card").show();
    });
});

//aux methods
function addTarget(key, placeholder, isVar){
    $("#target").append('<tr><td><label for"' + key + '">' + key + ': </label></td><td><input type="text" id="' + key + '" name="' + key + '" ' + (isVar ? 'class="var"' : '') + ' placeholder="' + placeholder + '" /></td><td></td></tr>');
}

function reload(){
    $.post("/home/Index", function(data) {
        window.location.reload(true);
    }).fail(function(){ 
        setTimeout(function(){
            reload();
        }, 1000);
    });
} 