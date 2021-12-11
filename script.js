var lastMenu, menuSender;
window.onload = function(){
    document.onclick = function(e){
        console.log(menuSender);
        if(e.target != menuSender && lastMenu != undefined){
            lastMenu.style.display = null;
        }
    };
};

function ShowDropdown(e, className){
    var visible = lastMenu != undefined && lastMenu.style.display == "block";
    if(lastMenu != undefined && visible && e.tagName != "LI"){
        lastMenu.style.display = null;
    }
    
    if(!visible){
        lastMenu = document.getElementsByClassName(className)[0];
        menuSender = e;
        lastMenu.style.display = "block";
    }
}