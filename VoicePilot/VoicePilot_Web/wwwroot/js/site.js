//Deplace l'element "bot" au milieu de l'ecran avec unne transision



function moveBot(action, zoneId) {
    //var zoneId = 'zone' + buttonId.slice(2); // Get the zone number from the button id
    
    var zone = document.getElementById(zoneId.toLowerCase());

    // Get the position of the zone
    var rect = zone.getBoundingClientRect();
    var bot = document.getElementById('bot');

    //add switch statement
    switch (action) {
        case 'deplace':
        case 'move':    
            bot.style.left = rect.left + 'px';
            bot.style.top = rect.top + 'px';
            break;
        case 'reset':     
            bot.style.left = '50%';
            bot.style.top = '50%';
            break;
    }
}