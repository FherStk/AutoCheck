function reload(){
    $.post("/home/Index", function(data) {
        window.location.reload(true);
    }).fail(function(){ 
        setTimeout(function(){
            reload();
        }, 1000);
    });
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

        $.post("/home/Run", { script: $("#script").val(), mode: $("#mode").val(), target: $("#target").val()}, function(data){                        
            $("#log > img").hide();

            //TODO: this must be append in async mode   
            $.each(data, function() {      
                //Multiple log files     

                $.each(this, function() {                        
                    //TODO: this should come in two lines
                    if(this.Text != null && this.Text.startsWith("ERROR:")){
                        $("#log > div").append('<label class="'  + this.Style + '">ERROR:</label><br>');
                        this.Text = this.Text.replace("ERROR:", "").replace(/^\n|\n$/g, '');
                    }
                    
                    $("#log > div").append('<label class="'  + this.Style + '"><xmp>' + (this.Indent == null ? "" : this.Indent) +  (this.Text == null ? "" : this.Text) + '</xmp></label>');                        
                    if(this.BreakLine || this.Style == null) $("#log > div").append('<br>');
                });

                $("#log > div").append('<br>');
            });
        });

        $("#run").prop( "disabled", false ); 
    });
});