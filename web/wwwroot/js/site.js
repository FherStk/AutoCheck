$(function(){  
    $("#shutdown").click(function(){
        $.post("/home/ShutDown", function(data){  
            window.open('','_self').close();
        });    
    });
});