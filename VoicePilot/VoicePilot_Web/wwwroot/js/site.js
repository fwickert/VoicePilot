//Deplace l'element "bot" au milieu de l'ecran avec unne transision



function moveBot(buttonId) {
    var zoneId = 'zone' + buttonId.slice(2); // Get the zone number from the button id
    var zone = document.getElementById(zoneId);

    // Get the position of the zone
    var rect = zone.getBoundingClientRect();
    var bot = document.getElementById('bot');

    // Set the position of the image to the position of the zone
    bot.style.top = rect.top + 'px';
    bot.style.left = rect.left + 'px';
}