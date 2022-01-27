function reload(){
    $.post("/home/Index", function(data) {
        window.location.reload(true);
    }).fail(function(){ 
        setTimeout(function(){
            reload();
        }, 1000);
    });
} 

function showMore(node){
    $(node).hide();
    $(node).next().show();
}

$(function(){  
    $.post("/home/CheckForUpdate", function(data){  
        if(data) $("#update-card").show();
    });

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
        $.post("/home/GetScripts", { mode: this.value}, function(data){                     
            $("#script").empty();
            $("#script").append('<option value="none" selected>- Select -</option>');

            $.each(data, function() {                        
                $("#script").append('<option value="'  + this.path + '">[' + this.source.toLowerCase() + '] ' + this.name + '</option>');
            });
            
            $("#step-2").hide();
            if(data.length == 0) $("#script").hide();
            else $("#script").show();
        });
    });

    $("#script").change(function() {
        var mode = $("#mode").val();
        $("#step-2").show();

        if(mode == "single"){
            $("#info-batch").hide();
            $("#info-single").show();
        }
        else if(mode == "batch"){
            $("#info-single").hide();
            $("#info-batch").show();
        }

        debugger;

        $.post("/home/GetTargetData", {script: $("#script").val()}, function(data){                  
            $("#target").empty();
            
            Object.keys(data).forEach(function(key) {
                if(key == "os") $("#target").append('<label for"' + key + '">' + key + ': </label><select id="' + key + '" name="' + key + '"><option value="GNU">GNU/Linux</option><option value="MAC">Mac OS</option><option value="WIN">Windows</option></select>');
                else if(key == "vars"){
                    $("#target").append("<div id='vars'></div>");
                    
                    Object.keys(data.vars).forEach(function(key) {
                        $("#vars").append('<label for"' + key + '">' + key + ': </label><input type="text" id="' + key + '" name="' + key + '" placeholder="Some data" />');
                        $("#vars").append('<br>');
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

                    $("#target").append('<label for"' + key + '">' + key + ': </label><input type="text" id="' + key + '" name="' + key + '" placeholder="' + placeholder + '" />');
                }

                $("#target").append('<br>');
            });            
            
            // $("#step-2").hide();
            // if(data.length == 0) $("#script").hide();
            // else $("#script").show();
        });
    });  

    $("#target").keyup(function() {
        if(this.value.length > 0) $("#run").prop( "disabled", false ); 
        else $("#run").prop( "disabled", true ); 
    });  

    $("#run").click(function() {
        $("#mode, #script, #target, #run").prop( "disabled", true );  
        $("#step-3").show();  
        $("#log > div").empty();
        $("#log > img").show();

        $.post("/home/Run", { script: $("#script").val(), target: $("#target").val()}, function(data){                        
            $("#log > img").hide();

            //TODO: this must be append in async mode   
            $.each(data, function(i) {      
                //Multiple log files  
                var logSelector = "#log > div";                                   
                $(logSelector).append(
                    '<div class="collapsable">\
                        <div onclick="showMore(this);" class="header' + (i==0 ? ' hidden' : '') + '">Display more...</div>\
                        <div class="content' + (i>0 ? ' hidden' : '') + '"></div>\
                    </div>'
                );

                $.each(this, function() {                
                    var lastLogSelector = logSelector + " > .collapsable:last-child > .content";

                    //TODO: this should come in two lines
                    if(this.Text != null && this.Text.startsWith("ERROR:")){
                        $(lastLogSelector).append('<label class="'  + this.Style + '">ERROR:</label><br>');
                        this.Text = this.Text.replace("ERROR:", "").replace(/^\n|\n$/g, '');
                    }
                    
                    $(lastLogSelector).append('<label class="'  + this.Style + '"><xmp>' + (this.Indent == null ? "" : this.Indent) +  (this.Text == null ? "" : this.Text) + '</xmp></label>');                        
                    if(this.BreakLine || this.Style == null) $(lastLogSelector).append('<br>');
                });

                $(logSelector).append('<br>');
            });
        });

        $("#run").prop( "disabled", false ); 
    });
});