//add code to recevie notification from SignalR hub
const connectionHubUrl = new URL('messageRelayHub', 'https://localhost:7270');

const connection = new signalR.HubConnectionBuilder().withUrl(connectionHubUrl.href).withAutomaticReconnect().withHubProtocol(new signalR.JsonHubProtocol()).build();

connection.on("ReceiveMessageUpdate", function (messageResponse) {
    if (messageResponse.state === "End") {

        var converter = new showdown.Converter({ tables: 'true' }),
            text = notifications.innerHTML,
            html = converter.makeHtml(text);

        notifications.innerHTML = html;

        notifications = null;

        //enable all btn css class
        $(".btn").removeClass("disabled");
    }
    else {

        if (messageResponse.state === "Start") {
            div = $("#spin" + messageResponse.whatAbout);
            document.getElementById('result' + messageResponse.whatAbout).innerHTML = "";
            OnlySpinOff(div);
        }
        else {
            if (div !== null) {

            }
            if (messageResponse.content !== null) {
                notifications.innerHTML = messageResponse.content;
            }
        }
    }

});

connection.start().then(function () {
    console.log("connected");
}).catch(function (err) {
    return console.error(err.toString());
});